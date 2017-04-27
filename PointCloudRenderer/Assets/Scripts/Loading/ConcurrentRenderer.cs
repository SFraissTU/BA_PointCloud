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
    /* This class is responsible for the HierarchyTraversal (determining which nodes are to be seen and which not) and loading the new nodes concurrent to the main thread
     */
    public class ConcurrentRenderer {
        private bool loadingPoints = false;
        private bool shuttingDown = false;

        private PriorityQueue<double, Node> toRender;
        //Points in toRender, that should not be rendered because they would exceed the point budget
        private HashSet<Node> notToRender;
        //Points that are supposed to be deleted. Normal Queue (but threadsafe)
        private ThreadSafeQueue<Node> toDelete;

        private List<Node> rootNodes;

        private float screenHeight;
        private float fieldOfView;
        private Vector3 cameraPositionF;
        private Plane[] frustum;
        private Matrix4x4 vpMatrix;

        private double minNodeSize;
        private uint pointBudget;

        private uint renderingPointCount = 0;
        

        public ConcurrentRenderer(int minNodeSize, uint pointBudget) {
            toRender = new HeapPriorityQueue<double, Node>();
            toDelete = new ThreadSafeQueue<Node>();
            notToRender = new HashSet<Node>();
            rootNodes = new List<Node>();
            this.minNodeSize = minNodeSize;
            this.pointBudget = pointBudget;
        }

        public void AddRootNode(Node rootNode) {
            rootNodes.Add(rootNode);
        }

        public bool IsLoadingPoints() {
            return loadingPoints;
        }

        public void SetCameraInfo(float screenHeight, float fieldOfView, Vector3 cameraPosition, Plane[] frustum, Matrix4x4 vpMatrix) {
            this.screenHeight = screenHeight;
            this.fieldOfView = fieldOfView;
            this.cameraPositionF = cameraPosition;
            this.frustum = frustum;
            this.vpMatrix = vpMatrix;
        }

        /* Updates the queues of nodes to be rendered / deleted. Important: Update camera data before!*/
         
        public void UpdateRenderingQueue() {
            if (loadingPoints) {
                throw new InvalidOperationException("Renderer is not ready for filling. Still loading points.");
            }
            if (rootNodes.Count == 0) return;
            Vector3d cameraPosition = new Vector3d(cameraPositionF);
            toRender.Clear();
            toDelete.Clear();
            notToRender.Clear();
            Queue<Node> toCheck = new Queue<Node>();
            foreach (Node rootNode in rootNodes) {
                toCheck.Enqueue(rootNode);
            }
            double radius = rootNodes[0].BoundingBox.Radius();
            int lastLevel = rootNodes[0].GetLevel();//= 0
            //Check all nodes - Breadth first
            while (toCheck.Count != 0 && !shuttingDown) {
                Node currentNode = toCheck.Dequeue();
                if (currentNode.GetLevel() > lastLevel) {
                    radius /= 2;
                    ++lastLevel;
                } else if (currentNode.GetLevel() < lastLevel) {
                    //Should not happen, but just in case...
                    lastLevel = currentNode.GetLevel();
                    radius = currentNode.BoundingBox.Radius();
                }

                //if (renderingPoints + currentNode.PointCount < pointBudget)   //TODO: PointCount currently not available. Fix after fixing of converter
                //Is Node inside frustum?
                if (GeometryUtility.TestPlanesAABB(frustum, currentNode.BoundingBox.GetBoundsObject())) {
                    Vector3d center = currentNode.BoundingBox.Center();
                    double distance = center.distance(cameraPosition); //TODO: Maybe other point?
                    double slope = Math.Tan(fieldOfView / 2 * (Math.PI / 180));
                    double projectedSize = (screenHeight / 2.0) * radius / (slope * distance);
                    if (projectedSize >= minNodeSize) {
                        Vector3 projected = vpMatrix * currentNode.BoundingBox.Center().ToFloatVector();
                        projected = projected / projected.z;
                        double priority = projectedSize / projected.magnitude;
                        if (!currentNode.HasGameObjects()) {
                            toRender.Enqueue(currentNode, priority);
                        }
                        foreach (Node child in currentNode) {
                            toCheck.Enqueue(child);
                        }
                    } else {
                        if (currentNode.HasGameObjects()) {
                            SetNodeToBeDeleted(currentNode);
                        }
                    }
                } else {
                    if (currentNode.HasGameObjects()) {
                        SetNodeToBeDeleted(currentNode);
                    }
                }
            }
        }

        /* Puts the given node into the toDelete-Queue as well as all its children. However, the children are put first into the queue, because they should be removed first.
         */
        private void SetNodeToBeDeleted(Node currentNode) {
            //Remove lower LOD-Objects first!
            Queue<Node> childrenToCheck = new Queue<Node>();
            Stack<Node> newNodesToDelete = new Stack<Node>(); //<- saved in a stack because the order in the queue will be reverse
            newNodesToDelete.Push(currentNode);
            foreach (Node child in currentNode) {
                childrenToCheck.Enqueue(child);
            }
            while (childrenToCheck.Count != 0) {
                Node child = childrenToCheck.Dequeue();
                if (child.HasGameObjects()) {
                    newNodesToDelete.Push(child);
                    foreach (Node childchild in child) {
                        childrenToCheck.Enqueue(childchild);
                    }
                }
            }
            while (newNodesToDelete.Count != 0) {
                Node n = newNodesToDelete.Pop();
                renderingPointCount -= n.PointCount;
                toDelete.Enqueue(n);
            }
        }



        /* Loads points which have to be loaded in a new thread
         */
        public void StartUpdatingPoints() {
            new Thread(UpdateLoadedPoints).Start();
        }

        /* Loads point which have to be loaded
         */
        public void UpdateLoadedPoints() {
            try {
                loadingPoints = true;
                foreach (Node n in toRender) {
                    if (shuttingDown) return;
                    uint amount = n.PointCount;
                    //PointCount might already be sad from loading the points before
                    if (amount == 0) {
                        CloudLoader.LoadPointsForNode(n);
                        amount = n.PointCount;
                    }
                    if (renderingPointCount + amount < pointBudget) {
                        renderingPointCount += amount;
                        if (!n.HasPointsToRender()) {
                            CloudLoader.LoadPointsForNode(n);
                        }
                        if (!n.HasGameObjects()) {
                            n.SetReadyForGameObjectCreation();
                        }
                    } else {
                        //toRender.Remove(n); //TODO: Very ugly, fix with converter fix
                        notToRender.Add(n); //This way we do not need to remove something from the queue (would be expensive)
                        if (n.HasGameObjects()) {
                            toDelete.Enqueue(n);
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


        public void UpdateGameObjects(MeshConfiguration meshConfiguration) {
            int MAX_NODES_CREATE_PER_FRAME = 15;
            int MAX_NODES_DELETE_PER_FRAME = 10;
            for (int i = 0; i < MAX_NODES_CREATE_PER_FRAME && !toRender.IsEmpty(); i++) {
                Node n = toRender.Peek();
                if (notToRender.Contains(n)) {
                    notToRender.Remove(n);
                    toRender.Dequeue();
                } else if (n.IsWaitingForReadySet()) //Still waiting for point rendering
                {
                    break;
                } else if (n.IsReadyForGameObjectCreation()) {
                    toRender.Dequeue();
                    n.CreateGameObjects(meshConfiguration);
                }
            }
            for (int i = 0; i < MAX_NODES_DELETE_PER_FRAME && !toDelete.IsEmpty(); i++) {
                toDelete.Dequeue().RemoveGameObjects(meshConfiguration);
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
    }
}