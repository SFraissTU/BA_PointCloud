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
    /* This renderer is a OneTimeRenderer, meaning it only can check the visibility of the nodes again after all currently visible nodes have been loaded.
     * It is SingleThreaded, meaning the loading happens on the main thread!
     * 
     * How to use:
     * If the rendered nodes should be adapted to the current view, call UpdateVisibleNodes (this cannot be done again until the point loading is finished). This updates the rendering queue.
     * To load and create GameObjects call UpdateGameObjects once per Frame.
     * 
     * Note: This renderer does not make use of the NodeStatus-Property
     */
    public class SingleThreadedOneTimeRenderer : AbstractRenderer {
        private bool loadingPoints = false; //true, iff there are still nodes scheduled to be loaded
        private bool shuttingDown = false;  //true, iff everything should be stopped (the point loading will stop and every method will not do anything anymore)

        //Rendering Collections
        private PriorityQueue<double, Node> toLoad;                 //Priority Queue of nodes in the view frustum that exceed the minimum size. It doesn't matter if GameObjects are created yet. PointBudget-Correctness has yet to be checked
        private ListPriorityQueue<double, Node> alreadyRendered;    //Priority Queue of nodes in the view frustum that exceed the minimum size, for which GameObjects already exists. Nodes with higher priority are more likely to be removed in case its pointcount blocks the rendering of a more important node. A List Priority Queue is chosen, because we have to remove elements from both sides

        private List<Node> rootNodes;   //List of root nodes of the point clouds

        //Camera Info
        private Camera camera;

        private double minNodeSize; //Min projected node size
        private uint pointBudget;   //Point Budget

        private uint renderingPointCount = 0;   //Number of points being in GameObjects right now

        //Frame-Limits, see UpdateGameObjects
        private const int MAX_NODES_CREATE_PER_FRAME = 15;

        /* Creates a new SingleThreadedOneTimeRenderer.
         */
        public SingleThreadedOneTimeRenderer(int minNodeSize, uint pointBudget, Camera camera) {
            toLoad = new HeapPriorityQueue<double, Node>();
            alreadyRendered = new ListPriorityQueue<double, Node>();
            rootNodes = new List<Node>();
            this.minNodeSize = minNodeSize;
            this.pointBudget = pointBudget;
            this.camera = camera;
        }

        /* Registers the root node of a pointcloud in the renderer, so it will be considered in future visibility checks and GameObject creations.
         * The given rootNode may not be null! */
        public void AddRootNode(Node rootNode) {
            rootNodes.Add(rootNode);
        }

        /* Returns how man root nodes have been added */
        public int GetRootNodeCount() {
            return rootNodes.Count;
        }

        /* Returns weither a call of UpdateVisibleNodes is allowed right now. This is mainly important for the OneTimeRenderers. */
        public bool IsReadyForUpdate() {
            return !shuttingDown && !loadingPoints && toLoad.IsEmpty();
        }

        /* This method checks which nodes of the PointCloud are visible and adjusts the rendering collections accordingly.
         * Traverses the hierarchies and checks for each node, weither it is in the view frustum and weither the min node size is alright.
         * GameObjects of Nodes that fail this test are deleted right away, so this method should be called from the main thread!
         * This method can only be called if the renderer is currently not loading points (see method IsReadyForUpdate)
         * config is the MeshConfiguration used for GameObject-Creation (null is not allowed). This is needed because GameObjects might be deleted. 
         * Points are only scheduled for loading in this method and are not loaded yet.
         * The PointCount is set to the number of points visible after calling this method (points of GameObjects which have been visible before and still are).
         * If shuttingDown is set to true while this method is running, the traversal simply stops. The state of the renderer might be inconsistent afterward and will not be usable anymore.
         */
        public void UpdateVisibleNodes(MeshConfiguration config) {
            if (loadingPoints) {
                throw new InvalidOperationException("Renderer is not ready for filling. Still loading points.");
            }
            if (shuttingDown) {
                return;
            }
            if (rootNodes.Count == 0) return;
            renderingPointCount = 0;
            //Camera Data
            Vector3d cameraPosition = new Vector3d(camera.transform.position);
            float screenHeight = camera.pixelRect.height;
            float fieldOfView = camera.fieldOfView;
            Plane[] frustum = GeometryUtility.CalculateFrustumPlanes(camera);
            //Clearing Queues
            toLoad.Clear();
            alreadyRendered.Clear();
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
                        //Calculate centrality. TODO: Approach works, but maybe theres a better way of combining the two factors
                        //TODO: Centrality ignored, because it created unwanted results. Put back in later after discussion with supervisor
                        Vector3 pos = currentNode.BoundingBox.Center().ToFloatVector();
                        Vector3 projected = camera.WorldToViewportPoint(pos);
                        projected = (projected * 2) - new Vector3(1, 1, 0);
                        double priority = projectedSize;// Math.Sqrt(Math.Pow(projected.x, 2) + Math.Pow(projected.y, 2));
                        //Object has no GameObjects -> Enqueue for Loading
                        //Object has GameObjects -> Also Enqueue for Loading. Will be checked later. Enqueue for possible GO-Removal
                        toLoad.Enqueue(currentNode, priority);
                        if (currentNode.HasGameObjects()) {
                            alreadyRendered.Enqueue(currentNode, priority);
                            renderingPointCount += (uint)currentNode.PointCount;
                        }
                        foreach (Node child in currentNode) {
                            toCheck.Enqueue(child);
                        }
                    } else {
                        //This node or its children might be visible
                        DeleteNote(currentNode, config);
                    }
                } else {
                    //This node or its children might be visible
                    DeleteNote(currentNode, config);
                }
            }
        }

        /* Deletes the GOs of the given node as well as all its children.
         */
        private void DeleteNote(Node currentNode, MeshConfiguration config) {
            //Assumption: Parents have always higher priority than children, so if the parent is not already rendered, the child cannot be either!!!
            Queue<Node> childrenToCheck = new Queue<Node>();
            if (currentNode.HasGameObjects()) {
                currentNode.RemoveGameObjects();
                foreach (Node child in currentNode) {
                    childrenToCheck.Enqueue(child);
                }
            }
            while (childrenToCheck.Count != 0) {
                Node child = childrenToCheck.Dequeue();
                if (child.HasGameObjects()) {
                    child.RemoveGameObjects();
                    foreach (Node childchild in child) {
                        childrenToCheck.Enqueue(childchild);
                    }
                }
            }
        }

        /* Loads the points and creates new GameObjects for nodes that are scheduled to be rendered. This has to be called from the main thread.
         * Up to MAX_NDOES_CREATE_PER_FRAME are loaded and created in one frame.
         * Should be called every frame in the main thread, because GameObject-Creation happens here.
         * meshConfiguration is the MeshConfiguration used for GameObject-Creation (null is not allowed). */
        public void UpdateGameObjects(MeshConfiguration meshConfiguration) {
            if (shuttingDown) return;
            int i;
            for (i = 0; i < MAX_NODES_CREATE_PER_FRAME && !toLoad.IsEmpty(); i++) {
                Node n = toLoad.Dequeue();
                if (n.HasGameObjects()) {
                    //Remove already rendered element with highest priority (this element) from alreadyrendered-queue, so it might not be deleted!
                    //It could be that this element was already removed. However as the removal starts from the other side of the list, this can only be the case if the queue is empty
                    if (!alreadyRendered.IsEmpty()) {
                        alreadyRendered.Dequeue();
                    }
                    //Already in PointCount!
                } else {
                    int amount = n.PointCount;
                    //PointCount might already be there from loading the points before
                    if (amount == -1) {
                        CloudLoader.LoadPointsForNode(n);
                        amount = n.PointCount;
                    }
                    //If the pointbudget would be exheeded by loading the points, old GameObjects that already exist but have a lower priority might be removed
                    while (renderingPointCount + amount > pointBudget && !alreadyRendered.IsEmpty()) {
                        Node u = alreadyRendered.Pop(); //Get element with lowest priority
                        u.RemoveGameObjects();
                        renderingPointCount -= (uint)u.PointCount;
                    }
                    if (renderingPointCount + amount <= pointBudget) {
                        renderingPointCount += (uint)amount;
                        if (!n.HasPointsToRender()) {
                            CloudLoader.LoadPointsForNode(n);
                        }
                        //Create GameObjects
                        n.CreateGameObjects(meshConfiguration);
                        n.ForgetPoints();
                    } else {
                        //Stop Loading
                        //AlreadyRendered is empty, so no nodes are visible
                        toLoad.Clear();
                    }
                }
            }
            //FPSOutputController.NoteFPS(i == 0);
        }

        public void ShutDown() {
            shuttingDown = true;
        }

        public uint GetPointCount() {
            return renderingPointCount;
        }
    }
}