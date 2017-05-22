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
    /* This class is responsible for the HierarchyTraversal (determining which nodes are to be seen and which not), loading the new nodes concurrent to the main thread and creating the gameobjects.
     * How to use:
     * If the rendered nodes should be adapted to the current view, call UpdateRenderingQueue (this cannot be done again until the point loading is finished). This updates the rendering queue
     * To start loading the points in the rendering queue in a new thread call StartUpdatingPoints
     * To create or delete GameObjects call UpdateGameObjects per Frame
     */
    public class ConcurrentMultiTimeRenderer : AbstractRenderer {
        private bool loadingPoints = false; //true, iff there are still nodes scheduled to be loaded
        private bool shuttingDown = false;  //true, iff everything should be stopped (the point loading will stop and every method will not do anything anymore)

        //Rendering Collections
        private PriorityQueue<double, Node> toLoad;                 //Priority Queue of nodes in the view frustum that exceed the minimum size. No GameObjects are created yet. PointBudget-Correctness has yet to be checked
        private ThreadSafeQueue<Node> toRender;                     //Queue of nodes that are loaded and ready for GO-Creation and do not have GOs yet (No Priority Queue - Order might not be 100% correct...)
        private PriorityQueue<double, Node> alreadyLoaded;          //Priority Queue of nodes which are at least in state TORENDER. Nodes with higher priority are more likely to be removed in case its pointcount blocks the rendering of a more important node.
        private ThreadSafeQueue<Node> toDelete;                     //Queue of Points that are supposed to be deleted (used because some neccessary deletions are noticed outside the main thread, which is the only one who can remove GameObjects)

        private List<Node> rootNodes;   //List of root nodes of the point clouds

        //Camera Info
        private Camera camera;

        private double minNodeSize; //Min projected node size
        private uint pointBudget;   //Point Budget

        private uint renderingPointCount = 0;   //Number of points being in GameObjects right now

        private object toLoadLock = new object();

        //Frame-Limits, see UpdateGameObjects
        private const int MAX_NODES_CREATE_PER_FRAME = 15;
        private const int MAX_NODES_DELETE_PER_FRAME = 10;


        public ConcurrentMultiTimeRenderer(int minNodeSize, uint pointBudget, Camera camera) {
            toLoad = new HeapPriorityQueue<double, Node>();
            toRender = new ThreadSafeQueue<Node>();
            alreadyLoaded = new ListPriorityQueue<double, Node>();
            toDelete = new ThreadSafeQueue<Node>();
            rootNodes = new List<Node>();
            this.minNodeSize = minNodeSize;
            this.pointBudget = pointBudget;
            this.camera = camera;
        }

        public void AddRootNode(Node rootNode) {
            rootNodes.Add(rootNode);
        }

        public int GetRootNodeCount() {
            return rootNodes.Count;
        }

        //true, iff there are still nodes scheduled to be loaded
        public bool IsLoadingPoints() {
            return loadingPoints;
        }

        /* Updates the rendering collections. Traverses the hierarchies and checks for each node, weither it is in the view frustum and weither the min node size is alright.
         * GameObjects of Nodes that fail this test are deleted right away, so this method should be called from the main thread!
         * This method can only be called if the renderer is not currently loading points.
         * The RenderingPointCount is set to the number of points visible after calling this method (points of GameObjects which have been visible before and still are).
         * If shuttingDown is set to true while this method is running, the traversal simply stops. The state of the renderer might be inconsistent afterward and will not be usable anymore.
         */
        public void UpdateRenderingQueue(MeshConfiguration config) {
            if (shuttingDown) {
                return;
            }
            if (rootNodes.Count == 0) return;

            //Camera Data
            Vector3d cameraPosition = new Vector3d(camera.transform.position);
            float screenHeight = camera.pixelRect.height;
            float fieldOfView = camera.fieldOfView;
            Plane[] frustum = GeometryUtility.CalculateFrustumPlanes(camera);
            //Clearing Queues
            PriorityQueue<double, Node> newToLoad = new HeapPriorityQueue<double, Node>();
            PriorityQueue<double, Node> newAlreadyLoaded = new HeapPriorityQueue<double, Node>();
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

                //TODO: PointCount currently not available. Fix after fixing of converter
                //Is Node inside frustum?
                if (GeometryUtility.TestPlanesAABB(frustum, currentNode.BoundingBox.GetBoundsObject())) {
                    //Calculate projected size
                    Vector3d center = currentNode.BoundingBox.Center();
                    double distance = center.distance(cameraPosition);
                    double slope = Math.Tan(fieldOfView / 2 * (Math.PI / 180));
                    double projectedSize = (screenHeight / 2.0) * radius / (slope * distance);
                    if (projectedSize >= minNodeSize) {
                        //Calculate centrality. TODO: Approach works, but maybe theres a better way of combining the two factors
                        //TODO: Centrality ignored, because it created unwanted results. Put back in later after discussion with supervisor
                        //Vector3 pos = currentNode.BoundingBox.Center().ToFloatVector();
                        //Vector3 projected = camera.WorldToViewportPoint(pos);
                        //projected = (projected * 2) - new Vector3(1, 1, 0);
                        double priority = projectedSize;// Math.Sqrt(Math.Pow(projected.x, 2) + Math.Pow(projected.y, 2));

                        //Node should be loaded. So, let's check the status:
                        lock (currentNode) {
                            switch (currentNode.NodeStatus) {
                                case NodeStatus.INVISIBLE:
                                case NodeStatus.TOLOAD:
                                case NodeStatus.TODELETE:
                                    currentNode.NodeStatus = NodeStatus.TOLOAD;
                                    newToLoad.Enqueue(currentNode, priority);
                                    break;
                                default:
                                    //LOADING, TORENDER, RENDERED: Add to alreadyLoaded!
                                    //Note: LOADING-Nodes are added too, just in case loading should be finished during hierarchy traversal.
                                    //So the status has to be checked later again! Also if loading finishes after traversal, the node might be two times in aL
                                    newAlreadyLoaded.Enqueue(currentNode, -priority);
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

            //Debug.Log("URQ Obtaining Lock");
            lock (toLoadLock) { //Synchronisation with UpdateLoadingPoints
                //Debug.Log("URQ Obtained Lock");
                alreadyLoaded.Clear();
                alreadyLoaded = newAlreadyLoaded;
                toLoad.Clear();
                toLoad = newToLoad;
                //Debug.Log("URQ Returning Lock");
            }
        }

        /* Deletes the GOs of the given node as well as all its children.
         */
        private void DeleteNode(Node currentNode, MeshConfiguration config) {
            //Assumption: Parents have always higher priority than children, so if the parent is not already rendered, the child cannot be either!!!
            Queue<Node> childrenToCheck = new Queue<Node>();
            childrenToCheck.Enqueue(currentNode);
            while (childrenToCheck.Count != 0) {
                Node child = childrenToCheck.Dequeue();
                
                lock (child) {
                    switch (child.NodeStatus) {
                        case NodeStatus.TORENDER:
                            child.ForgetPoints();
                            break;
                        case NodeStatus.RENDERED:
                        case NodeStatus.TODELETE:
                            child.RemoveGameObjects(config);
                            break;
                    }
                    if (child.NodeStatus >= NodeStatus.TORENDER) {
                        renderingPointCount -= child.PointCount;
                    }
                    if (child.NodeStatus >= NodeStatus.TOLOAD) {    //Loading of the children has to be aborted as well
                        foreach (Node childchild in child) {
                            childrenToCheck.Enqueue(childchild);
                        }
                    }
                    child.NodeStatus = NodeStatus.INVISIBLE;
                }
            }
        }

        /* Loads points which have to be loaded in a new thread
         */
        public void StartUpdatingPoints() {
            new Thread(UpdateLoadedPoints).Start();
        }

        /* Loads point which have to be loaded. Ideally this should run parallel to the main thread (started by StartUpdatingPoints).
         * The toRenderNew-Queue is iterated and points are loaded if neccessary. PointCount and PointBudget are checked. Nodes that are not needed anymore will be marked for deletion (toDelete-Queue)
         */
        public void UpdateLoadedPoints() {
            try {
                loadingPoints = true;
                while (!shuttingDown) {
                    //Debug.Log("ULP Obtaining Lock");
                    lock (toLoadLock) { //Locking over toLoad because toLoad might be cleared and we do not want to clear the new stuff (replacement in traversal)
                        //Debug.Log("ULP Obtained Lock");
                        if (toLoad.IsEmpty()) {
                            //Debug.Log("ULP Returning Lock");
                            continue;
                        }
                        double nPriority;
                        Node n = toLoad.Dequeue(out nPriority);
                        lock (n) {
                            if (n.NodeStatus != NodeStatus.TOLOAD) {
                                //Debug.Log("ULP Returning Lock");
                                continue;
                            } else {
                                n.NodeStatus = NodeStatus.LOADING;
                            }
                        }
                        uint amount = n.PointCount;
                        //PointCount might already be there from loading the points before
                        if (amount == 0) {
                            CloudLoader.LoadPointsForNode(n);
                            amount = n.PointCount;
                        }
                        //If the pointbudget would be exheeded by loading the points, old GameObjects that already exist but have a lower priority might be removed
                        while (renderingPointCount + amount > pointBudget && !alreadyLoaded.IsEmpty()) {
                            //AL could contain nodes that have been set to invisible by now (in hierarchy traversal). -> Locking neccessary (but already locked with toLoad above)
                            Node u;
                            double arPriority;
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
                                            renderingPointCount -= u.PointCount;
                                            if (u.NodeStatus == NodeStatus.TORENDER) {
                                                u.NodeStatus = NodeStatus.INVISIBLE; //Will not be rendered
                                            } else /* RENDERED */ {
                                                toDelete.Enqueue(u);
                                                u.NodeStatus = NodeStatus.TODELETE;
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
                        }
                        if (renderingPointCount + amount <= pointBudget) {
                            renderingPointCount += amount;
                            if (!n.HasPointsToRender()) {
                                CloudLoader.LoadPointsForNode(n); //TODO: Achtung: wenn Punkte in toRender geschmissen werden, aber ehe der GO-Erzeugung entfernt werden (durch URQ), bleiben sie geladen!
                            }
                            lock (n) {
                                switch (n.NodeStatus) {
                                    case NodeStatus.LOADING:
                                        toRender.Enqueue(n);
                                        n.NodeStatus = NodeStatus.TORENDER;
                                        break;
                                    case NodeStatus.INVISIBLE:
                                    case NodeStatus.RENDERED:
                                        n.ForgetPoints();
                                        renderingPointCount -= amount;
                                        break;
                                    default:
                                        //Undefined. Should not happen
                                        Debug.LogError("Invalid Node Status");
                                        break;
                                }
                                if (n.NodeStatus == NodeStatus.LOADING) {
                                    toRender.Enqueue(n);
                                    n.NodeStatus = NodeStatus.TORENDER;
                                } 
                            }
                        } else {
                            //If one note cannot be rendered, the following notes shouldn't be rendered either
                            //Stop Loading
                            //AlreadyRendered is empty, so no nodes are visible
                            toLoad.Clear(); //Locking over toLoad removes synchronization problems with the traversal
                        }
                        //Debug.Log("Loaded Node: " + n + ", " + DateTime.Now);
                        //Debug.Log("ULP Returning Lock");
                    }
                }
                loadingPoints = false;
            } catch (Exception ex) {
                Debug.LogError(ex);
                loadingPoints = false;
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
                        //Create GameObjects
                        n.CreateGameObjects(meshConfiguration);
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
                        n.RemoveGameObjects(meshConfiguration);
                        n.NodeStatus = NodeStatus.INVISIBLE;
                    }
                }
            }
        }

        public void ShutDown() {
            shuttingDown = true;
        }

        public bool HasNodesToRender() {
            return !toRender.IsEmpty();
        }

        public bool HasNodesToDelete() {
            return !toDelete.IsEmpty();
        }

        public uint GetPointCount() {
            return renderingPointCount;
        }
    }
}