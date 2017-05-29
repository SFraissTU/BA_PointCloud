﻿using CloudData;
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
     * It is concurrent, meaning the loading happens in an extra thread.
     * 
     * How to use:
     * If the rendered nodes should be adapted to the current view, call UpdateVisibleNodes (this cannot be done again until the point loading is finished). This updates the rendering queue.
     * The loading of the points in the rendering queue is started in a new thread at the end of UpdateVisibleNodes.
     * To create or delete GameObjects call UpdateGameObjects once per Frame
     * 
     * Note: This renderer does not make use of the NodeStatus-Property
     */
    public class ConcurrentOneTimeRenderer : AbstractRenderer {
        private bool loadingPoints = false; //true, iff there are still nodes scheduled to be loaded
        private bool shuttingDown = false;  //true, iff everything should be stopped (the point loading will stop and every method will not do anything anymore)

        //Rendering Collections
        private PriorityQueue<double, Node> toLoad;                 //Priority Queue of nodes in the view frustum that exceed the minimum size. It doesn't matter if GameObjects are created yet. PointBudget-Correctness has yet to be checked
        private PriorityQueue<double, Node> toRender;               //Priority Queue of nodes that are loaded and ready for GO-Creation and do not have GOs yet
        private ListPriorityQueue<double, Node> alreadyRendered;    //Priority Queue of nodes in the view frustum that exceed the minimum size, for which GameObjects already exists. Nodes with higher priority are more likely to be removed in case its pointcount blocks the rendering of a more important node. A List Priority Queue is chosen, because we have to remove elements from both sides
        private ThreadSafeQueue<Node> toDelete;                     //Queue of Points that are supposed to be deleted (used because some neccessary deletions are noticed outside the main thread, which is the only one who can remove GameObjects)

        private List<Node> rootNodes;   //List of root nodes of the point clouds

        //Camera Info
        private Camera camera;

        private double minNodeSize; //Min projected node size
        private uint pointBudget;   //Point Budget
        
        private uint renderingPointCount = 0;   //Number of points being in GameObjects right now

        //Frame-Limits, see UpdateGameObjects
        private const int MAX_NODES_CREATE_PER_FRAME = 15;
        private const int MAX_NODES_DELETE_PER_FRAME = 10;

        /* Creates a new ConcurrentOneTimeRenderer.
         */
        public ConcurrentOneTimeRenderer(int minNodeSize, uint pointBudget, Camera camera) {
            toLoad = new HeapPriorityQueue<double, Node>();
            toRender = new HeapPriorityQueue<double, Node>();
            alreadyRendered = new ListPriorityQueue<double, Node>();
            toDelete = new ThreadSafeQueue<Node>();
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
            return !shuttingDown && !loadingPoints && toRender.IsEmpty();
        }


        /* This method checks which nodes of the PointCloud are visible and adjusts the rendering collections accordingly.
         * Traverses the hierarchies and checks for each node, weither it is in the view frustum and weither the min node size is alright.
         * GameObjects of Nodes that fail this test are deleted right away, so this method should be called from the main thread!
         * This method can only be called if the renderer is currently not loading points (see method IsReadyForUpdate)
         * config is the MeshConfiguration used for GameObject-Creation (null is not allowed). This is needed because GameObjects might be deleted. 
         * At the end of the method a new thread is started, which loads the nodes.
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
            toRender.Clear();
            toDelete.Clear();
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
                            renderingPointCount += currentNode.PointCount;
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

            new Thread(UpdateLoadedPoints).Start();
        }

        /* Deletes the GOs of the given node as well as all its children.
         */
        private void DeleteNode(Node currentNode, MeshConfiguration config) {
            //Assumption: Parents have always higher priority than children, so if the parent is not already rendered, the child cannot be either!!!
            Queue<Node> childrenToCheck = new Queue<Node>();
            if (currentNode.HasGameObjects()) {
                currentNode.RemoveGameObjects(config);
                foreach (Node child in currentNode) {
                    childrenToCheck.Enqueue(child);
                }
            }
            while (childrenToCheck.Count != 0) {
                Node child = childrenToCheck.Dequeue();
                if (child.HasGameObjects()) {
                    child.RemoveGameObjects(config);
                    foreach (Node childchild in child) {
                        childrenToCheck.Enqueue(childchild);
                    }
                }
            }
        }


        /* Loads point which have to be loaded. This should run parallel to the main thread (started in UpdateVisibleNodes).
         * The toLoad-Queue is iterated and points are loaded if neccessary. PointCount and PointBudget are checked. Nodes that are not needed anymore will be marked for deletion (toDelete-Queue)
         */
        private void UpdateLoadedPoints() {
            try {
                loadingPoints = true;
                while (!toLoad.IsEmpty()) {
                    if (shuttingDown) return;
                    double priority;
                    Node n = toLoad.Dequeue(out priority);
                    if (n.HasGameObjects()) {
                        //Remove already rendered element with highest priority (this element) from alreadyrendered-queue, so it might not be deleted!
                        //It could be that this element was already removed. However as the removal starts from the other side of the list, this can only be the case if the queue is empty
                        if (!alreadyRendered.IsEmpty()) {
                            alreadyRendered.Dequeue();
                        }
                        //Already in PointCount!
                    } else {
                        uint amount = n.PointCount;
                        //PointCount might already be there from loading the points before
                        if (amount == 0) {
                            CloudLoader.LoadPointsForNode(n);
                            amount = n.PointCount;
                        }
                        //If the pointbudget would be exheeded by loading the points, old GameObjects that already exist but have a lower priority might be removed
                        while (renderingPointCount + amount > pointBudget && !alreadyRendered.IsEmpty()) {
                            Node u = alreadyRendered.Pop(); //Get element with lowest priority
                            toDelete.Enqueue(u);//Direct Deleting not possible here, so scheduling for later
                            renderingPointCount -= u.PointCount;
                        }
                        if (renderingPointCount + amount <= pointBudget) {
                            renderingPointCount += amount;
                            if (!n.HasPointsToRender()) {
                                CloudLoader.LoadPointsForNode(n);
                            }
                            toRender.Enqueue(n, priority);
                        } else {
                            //Stop Loading
                            //AlreadyRendered is empty, so no nodes are visible
                            toLoad.Clear();
                        }
                    }
                }
                loadingPoints = false;
                Debug.Log("Loaded " + renderingPointCount + " points");
            } catch (Exception ex) {
                Debug.LogError(ex);
                loadingPoints = false;
            }
        }
        
        /* Creates new GameObjects for nodes that are scheduled to be rendered. This has to be called from the main thread.
         * Up to MAX_NDOES_CREATE_PER_FRAME are created in one frame. Up to MAX_NODES_DELETE_PER_FRAME are deleted in a frame except during Hierachy Traversal (updateRenderingQueue), where no limit is given.
         * Should be called every frame in the main thread, because GameObject-Creation happens here.
         * meshConfiguration is the MeshConfiguration used for GameObject-Creation (null is not allowed). */
        public void UpdateGameObjects(MeshConfiguration meshConfiguration) {
            if (shuttingDown) return;
            int i;
            for (i = 0; i < MAX_NODES_CREATE_PER_FRAME && !toRender.IsEmpty(); i++) {
                Node n = toRender.Dequeue();
                //Create GameObjects
                n.CreateGameObjects(meshConfiguration);
            }
            //toDelete only contains nodes that where there last frame, are in the view frustum, but would exheed the point budget
            for (int j = 0; i < MAX_NODES_DELETE_PER_FRAME && !toDelete.IsEmpty(); j++) {
                toDelete.Dequeue().RemoveGameObjects(meshConfiguration);
            }
        }

        /* This methods stops and disables the renderer. Method calls should not leed to any GameObject-modifications anymore. Concurrent threads are scheduled to stop.
         * Consistency is not guaranteed after the call of this method. This should only be called at the end of the program to stop concurrent threads. */
        public void ShutDown() {
            shuttingDown = true;
        }

        /* This method returns the current pointcount, so how many points should be visible (including points that are not yet visible, but are loaded and scheduled for GO-creation, not including points that are visible, but are scheduled for GO-destruction).
        */
        public uint GetPointCount() {
            return renderingPointCount;
        }
    }
}