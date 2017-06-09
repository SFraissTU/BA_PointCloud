using CloudData;
using Controllers;
using DataStructures;
using ObjectCreation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Loading {
    /* This renderer is a MultiTimeRenderer, meaning the visibility check can be done any time.
     * It is Concurrent, meaning the loading happens in a concurrent thread!
     * 
     * How to use:
     * If the rendered nodes should be adapted to the current view, call UpdateVisibleNodes (this cannot be done again until the point loading is finished). This updates the loading queue.
     * To load and create GameObjects call UpdateGameObjects once per Frame.
     * 
     * Note: This renderer uses the NodeStatus-Property, in the meaning defined in the NodeStatus-class
     */
    public class ConcurrentMultiTimeRenderer : AbstractRenderer {
        private bool shuttingDown = false;  //true, iff everything should be stopped (the point loading will stop and every method will not do anything anymore)

        //Rendering Collections
        //Important note about the rendering collections: The nodes in each queue fulfilled the conditions of that queue at enqueuing time. However, they might not do anymore at dequeueing time. So the NodeStatus has to always be checked!!!
        private PriorityQueue<LoadingPriority, Node> toLoad;                 //Priority Queue of nodes in the view frustum that exceed the minimum size. No GameObjects are created yet. PointBudget-Correctness has yet to be checked
        private ThreadSafeQueue<Node> toRender;                     //Queue of nodes that are loaded and ready for GO-Creation and do not have GOs yet (No Priority Queue - Order might not be 100% correct...)
        private PriorityQueue<LoadingPriority, Node> alreadyLoaded;          //Priority Queue of nodes which are in state TORENDER or RENDERED. Nodes with higher priority are more likely to be removed in case its pointcount blocks the rendering of a more important node.
        private ThreadSafeQueue<Node> toDelete;                     //Queue of Points that are supposed to be deleted (used because some neccessary deletions are noticed outside the main thread, which is the only one who can remove GameObjects)

        private List<Node> rootNodes;   //List of root nodes of the point clouds

        private GameObjectLRUCache cache;

        //Camera Info
        private Camera camera;

        private double minNodeSize; //Min projected node size
        private uint pointBudget;   //Point Budget

        private uint renderingPointCount = 0;   //Number of points being in nodes in state TORENDER or RENDERED

        private object toLoadLock = new object();           //MutEx-Object. All access to toLoad should be done while locking over this object
        private object pointCountLock = new object();       //MutEx-Object. All access to renderingPointCount should be done while locking over this object

        //Frame-Limits, see UpdateGameObjects
        private const int MAX_NODES_CREATE_PER_FRAME = 25;
        private const int MAX_NODES_DELETE_PER_FRAME = 10;

        /* Creates a new ConcurrentMultiTimeRenderer. Already starts the Loading-Thread!!!
         */
        public ConcurrentMultiTimeRenderer(int minNodeSize, uint pointBudget, Camera camera, GameObjectLRUCache cache) {
            toLoad = new HeapPriorityQueue<LoadingPriority, Node>();
            toRender = new ThreadSafeQueue<Node>();
            alreadyLoaded = new ListPriorityQueue<LoadingPriority, Node>();
            toDelete = new ThreadSafeQueue<Node>();
            rootNodes = new List<Node>();
            this.minNodeSize = minNodeSize;
            this.pointBudget = pointBudget;
            this.camera = camera;
            this.cache = cache;
            new Thread(UpdateLoadedPoints).Start();
        }

        public void AddRootNode(Node rootNode) {
            rootNodes.Add(rootNode);
        }

        public int GetRootNodeCount() {
            return rootNodes.Count;
        }

        /* Returns weither a call of UpdateVisibleNodes is allowed right now, which is always the case except after shuttingDown. */
        public bool IsReadyForUpdate() {
            return !shuttingDown;
        }

        /* This method checks which nodes of the PointCloud are visible and adjusts the rendering collections accordingly. Note that the NodeStatuses are changed right away. The collections however get replaced in the end of the method.
         * Traverses the hierarchies and checks for each node, weither it is in the view frustum and weither the min node size is alright.
         * GameObjects of Nodes that fail this test are deleted right away, so this method should be called from the main thread!
         * config is the MeshConfiguration used for GameObject-Creation (null is not allowed). This is needed because GameObjects might be deleted. 
         * Points are only scheduled for loading in this method. This method does not load the points though. Loading happens concurrently in an other thread.
         * If shuttingDown is set to true while this method is running, the traversal simply stops. The state of the renderer might be inconsistent afterward and will not be usable anymore.
         */
        public void UpdateVisibleNodes(MeshConfiguration config) {
            if (shuttingDown) {
                return;
            }
            if (rootNodes.Count == 0) return;

            //Camera Data
            Vector3d cameraPosition = new Vector3d(camera.transform.position);
            float screenHeight = camera.pixelRect.height;
            float fieldOfView = camera.fieldOfView;
            Plane[] frustum = GeometryUtility.CalculateFrustumPlanes(camera);
            Vector3d camToScreenCenterDir = new Vector3d(camera.transform.forward);
            //Clearing Queues
            PriorityQueue<LoadingPriority, Node> newToLoad = new HeapPriorityQueue<LoadingPriority, Node>();
            PriorityQueue<LoadingPriority, Node> newAlreadyLoaded = new HeapPriorityQueue<LoadingPriority, Node>();
            //Initializing Checking-Queue
            Queue<Node> toCheck = new Queue<Node>();
            foreach (Node rootNode in rootNodes) {
                toCheck.Enqueue(rootNode);
            }
            //Radius & Level
            double radius = rootNodes[0].BoundingBox.Radius();
            int lastLevel = rootNodes[0].GetLevel();//= 0
            //Check all nodes - Breadth first
            while (toCheck.Count != 0 && !shuttingDown) {
                Node currentNode = toCheck.Dequeue();
                //Check Level and radius
                if (currentNode.GetLevel() > lastLevel) {
                    radius /= 2;
                    ++lastLevel;
                } else if (currentNode.GetLevel() < lastLevel) {
                    //Should not happen, but just in case...
                    lastLevel = currentNode.GetLevel();
                    radius = currentNode.BoundingBox.Radius();
                }
                
                //Is Node inside frustum?
                if (GeometryUtility.TestPlanesAABB(frustum, currentNode.BoundingBox.GetBoundsObject())) {
                    //Calculate projected size
                    Vector3d center = currentNode.BoundingBox.Center();
                    double distance = center.distance(cameraPosition);
                    double slope = Math.Tan(fieldOfView / 2 * (Math.PI / 180));
                    double projectedSize = (screenHeight / 2.0) * radius / (slope * distance);
                    if (projectedSize >= minNodeSize) {
                        Vector3d camToNodeCenterDir = (center - cameraPosition).Normalize();
                        double angle = Math.Acos(camToScreenCenterDir * camToNodeCenterDir);
                        double angleWeight = Math.Abs(angle) + 1.0;
                        angleWeight = Math.Pow(angle, 2);
                        double priority = projectedSize / angleWeight;

                        //Node should be loaded. So, let's check the status:
                        lock (currentNode) {
                            switch (currentNode.NodeStatus) {
                                case NodeStatus.UNDEFINED:
                                case NodeStatus.INVISIBLE:
                                case NodeStatus.TOLOAD:
                                    currentNode.NodeStatus = NodeStatus.TOLOAD;
                                    newToLoad.Enqueue(currentNode, new LoadingPriority(currentNode.GetLevel(), priority));
                                    break;
                                case NodeStatus.TODELETE:
                                    currentNode.NodeStatus = NodeStatus.TOLOAD;
                                    newToLoad.Enqueue(currentNode, new LoadingPriority(currentNode.GetLevel(), priority));
                                    //Note: This has to be done, as we do not want to increase the pointcount in here because of synchronisation problems with the other thread
                                    //These lines mean, that nodes can be in TOLOAD, that are already rendered!!! Keep that in mind!
                                    break;
                                default:
                                    //LOADING, TORENDER, RENDERED: Add to alreadyLoaded!
                                    //Note: LOADING-Nodes are added too, just in case loading should be finished during hierarchy traversal.
                                    //So the status has to be checked later again! Also if loading finishes after traversal, the node might be two times in aL
                                    newAlreadyLoaded.Enqueue(currentNode, new LoadingPriority(-currentNode.GetLevel(), -priority));
                                    break;
                            }
                        }

                        foreach (Node child in currentNode) {
                            toCheck.Enqueue(child);
                        }
                    } else {
                        //This node or its children might be visible
                        DeleteNode(currentNode, config);
                    }
                } else {
                    //This node or its children might be visible
                    DeleteNode(currentNode, config);
                }
            }
            
            lock (toLoadLock) { //Synchronisation with UpdateLoadingPoints
                alreadyLoaded.Clear();
                alreadyLoaded = newAlreadyLoaded;
                toLoad.Clear();
                toLoad = newToLoad;
            }
        }

        /* Deletes the GOs of the given node as well as all its children.
         */
        private void DeleteNode(Node currentNode, MeshConfiguration config) {
            //Assumption: Parents have always higher priority than children, so if the parent is not already rendered, the child cannot be either!!!
            Queue<Node> childrenToCheck = new Queue<Node>();
            childrenToCheck.Enqueue(currentNode);
            Stack<Node> nodesToDelete = new Stack<Node>();//Delete in different order

            while (childrenToCheck.Count != 0) {
                Node child = childrenToCheck.Dequeue();
                nodesToDelete.Push(child);
                if (child.NodeStatus >= NodeStatus.TOLOAD) {
                    foreach (Node childchild in child) {
                        childrenToCheck.Enqueue(childchild);
                    }
                }
            }

            while (nodesToDelete.Count != 0) {
                Node child = nodesToDelete.Pop();
                lock (child) {
                    int oldStatus = child.NodeStatus;
                    if (child.HasGameObjects() && child.AreGameObjectsActive()) {   //Note that even TOLOAD and LOADING might have GOs because of TODELETE -> TOLOAD in UVN
                        cache.Insert(child);
                    }
                    lock (pointCountLock) {
                        child.NodeStatus = NodeStatus.INVISIBLE;
                        if (oldStatus == NodeStatus.TORENDER || oldStatus == NodeStatus.RENDERED) {
                            renderingPointCount -= (uint)child.PointCount;
                        }
                        if (oldStatus >= NodeStatus.TOLOAD) {    //Loading of the children has to be aborted as well
                            foreach (Node childchild in child) {
                                childrenToCheck.Enqueue(childchild);
                            }
                        }
                    }
                }
            }
        }

        /* Loads point which have to be loaded. This should run parallel to the main thread (started in the Constructor).
         * The toLoad-Queue is iterated and points are loaded if neccessary.  PointCount and PointBudget are checked. Nodes that are not needed anymore will be marked for deletion (toDelete-Queue).
         * Nodes that have been successfully loaded and are still supposed to be visible will be put into the toRender-Queue, so GameObjects will be created in UpdateGameObjects
         */
        private void UpdateLoadedPoints() {
            try {
                while (!shuttingDown) {
                    Monitor.Enter(toLoadLock);  //Locking over toLoad because toLoad might be cleared and we do not want to clear the new stuff (replacement in traversal)
                    if (toLoad.IsEmpty()) {
                        Monitor.Exit(toLoadLock);
                        continue;
                    }
                    var oldToLoad = toLoad;
                    LoadingPriority nPriority;
                    Node n = toLoad.Dequeue(out nPriority);
                    lock (n) {
                        if (n.NodeStatus != NodeStatus.TOLOAD) {
                            Monitor.Exit(toLoadLock);
                            continue;
                        } else {
                            n.NodeStatus = NodeStatus.LOADING;
                        }
                    }
                    int amount = n.PointCount;
                    //PointCount might already be there from loading the points before
                    if (amount == -1) {
                        //Not happening for nodes that were once are already loaded. So also no cache-checking neccessary
                        Monitor.Exit(toLoadLock);
                        CloudLoader.LoadPointsForNode(n);
                        Monitor.Enter(toLoadLock);
                        amount = n.PointCount;
                    }
                    //If the pointbudget would be exheeded by loading the points, old GameObjects that already exist but have a lower priority might be removed
                    Monitor.Enter(pointCountLock);
                    while (renderingPointCount + amount > pointBudget && !alreadyLoaded.IsEmpty()) {
                        Monitor.Exit(pointCountLock);
                        //AL could contain nodes that have been set to invisible by now (in hierarchy traversal). -> Locking neccessary (but already locked with toLoad above)
                        Node u;
                        LoadingPriority arPriority;
                        if (!alreadyLoaded.IsEmpty()) {
                            u = alreadyLoaded.Peek();
                            arPriority = -alreadyLoaded.MaxPriority();
                        } else {
                            continue;
                        }
                            
                        lock (u) {
                            if (u.NodeStatus == NodeStatus.TORENDER || u.NodeStatus == NodeStatus.RENDERED) {
                                if (arPriority < nPriority) {
                                    alreadyLoaded.Dequeue(); //Get element with lowest priority
                                    if (u.NodeStatus == NodeStatus.TORENDER || u.NodeStatus == NodeStatus.RENDERED) {
                                        lock (pointCountLock) {
                                            renderingPointCount -= (uint)u.PointCount;
                                            if (u.NodeStatus == NodeStatus.TORENDER) {
                                                u.NodeStatus = NodeStatus.INVISIBLE; //Will not be rendered
                                            } else /* RENDERED */ {
                                                toDelete.Enqueue(u);
                                                u.NodeStatus = NodeStatus.TODELETE;
                                            }
                                        }
                                    }
                                } else {
                                    break;
                                }
                            } else {
                                //If the node is not visible anymore anyway
                                alreadyLoaded.Dequeue();
                            }
                        }
                        Monitor.Enter(pointCountLock);
                    }
                    if (renderingPointCount + amount <= pointBudget) {
                        Monitor.Exit(pointCountLock);
                        Monitor.Exit(toLoadLock);
                        //LOADING OR RECEIVING FROM CACHE vvvvv
                        //TODO: SYNCHRONISATION
                        if (n.HasGameObjects() && !n.AreGameObjectsActive()) {
                            cache.Withdraw(n);
                        }
                        else if (!n.HasGameObjects()) {
                            if (!n.HasPointsToRender()) {
                                CloudLoader.LoadPointsForNode(n);
                            }
                        }
                        lock (n) {
                            switch (n.NodeStatus) {
                                case NodeStatus.LOADING:
                                    lock (pointCountLock) {
                                        if (!n.HasGameObjects() || !n.AreGameObjectsActive()) {
                                            toRender.Enqueue(n);
                                            n.NodeStatus = NodeStatus.TORENDER;
                                        } else {//hat GOs und GOs sind aktiv
                                            n.NodeStatus = NodeStatus.RENDERED;
                                        }
                                        renderingPointCount += (uint)amount;
                                    }
                                    break;
                                case NodeStatus.UNDEFINED:
                                case NodeStatus.INVISIBLE:
                                case NodeStatus.TOLOAD:
                                    n.ForgetPoints();
                                    break;
                                case NodeStatus.RENDERED:
                                case NodeStatus.TODELETE:
                                    n.ForgetPoints();
                                    break;
                            }
                        }
                    } else {
                        Monitor.Exit(pointCountLock);
                        lock (n) {
                            if (n.HasGameObjects() && n.AreGameObjectsActive()) {
                                toDelete.Enqueue(n);
                                n.NodeStatus = NodeStatus.TODELETE;
                            } else {
                                n.ForgetPoints();
                                n.NodeStatus = NodeStatus.INVISIBLE;
                            }
                        }
                        //If one note cannot be rendered, the following notes shouldn't be rendered either
                        //Stop Loading
                        //AlreadyRendered is empty, so no nodes are visible
                        if (toLoad == oldToLoad) { //If it has been replaced during loading, we will not clear it
                            toLoad.Clear(); //Locking over toLoad removes synchronization problems with the traversal
                        }
                        Monitor.Exit(toLoadLock);
                    }
                }
            } catch (Exception ex) {
                Debug.LogError(ex);
            }
        }

        /* Creates new GameObjects for nodes that are scheduled to be rendered. This has to be called from the main thread.
         * Up to MAX_NDOES_CREATE_PER_FRAME are created in one frame. Up to MAX_NODES_DELETE_PER_FRAME are deleted in a frame except during Hierachy Traversal (updateRenderingQueue), where no limit is given
         */
        public void UpdateGameObjects(MeshConfiguration meshConfiguration) {
            if (shuttingDown) return;
            int i;
            for (i = 0; i < MAX_NODES_CREATE_PER_FRAME && !toRender.IsEmpty(); i++) {
                Node n = toRender.Dequeue();
                lock (n) {
                    if (n.NodeStatus == NodeStatus.TORENDER) {
                        if (n.HasGameObjects()) {
                            cache.Withdraw(n);
                            n.ReactivateGameObjects();
                            i--;
                        } else {
                            //Create GameObjects
                            n.CreateGameObjects(meshConfiguration);
                            n.ForgetPoints();
                        }
                        n.NodeStatus = NodeStatus.RENDERED;
                    }
                }
            }
            //FPSOutputController.NoteFPS(i == 0);
            //toDelete only contains nodes that where there last frame, are in the view frustum, but would exheed the point budget
            for (int j = 0; i < MAX_NODES_DELETE_PER_FRAME && !toDelete.IsEmpty(); j++) {
                Node n = toDelete.Dequeue();
                lock (n) {
                    if (n.NodeStatus == NodeStatus.TODELETE) {
                        cache.Insert(n);
                        n.NodeStatus = NodeStatus.INVISIBLE;
                    }
                }
            }
        }

        //This method is for test purposes only. It checks weither the pointcount is correct
        private void CheckPointCount(string identifier) {
            lock (toLoadLock) {
                lock (pointCountLock) {
                    uint correctPointCount = 0;
                    Queue<Node> toCheck = new Queue<Node>();
                    foreach (Node root in rootNodes) {
                        toCheck.Enqueue(root);
                    }
                    while (toCheck.Count != 0) {
                        Node n = toCheck.Dequeue();
                        if (n.NodeStatus == NodeStatus.TORENDER || n.NodeStatus == NodeStatus.RENDERED) {
                            correctPointCount += (uint)n.PointCount;
                        }
                        foreach (Node child in n) {
                            toCheck.Enqueue(child);
                        }
                    }
                    if (correctPointCount != renderingPointCount) {
                        Debug.LogError("ALARM! ALARM! @" + identifier + ": Real: " + correctPointCount + " vs. Wrong: " + renderingPointCount);
                        ShutDown();
                        throw new Exception("Correct PC: " + correctPointCount);
                    }
                }
            }
        }

        public void ShutDown() {
            shuttingDown = true;
        }

        public uint GetPointCount() {
            return renderingPointCount;
        }
    }
}