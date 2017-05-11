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

        private PriorityQueue<double, Node> toRenderNew;
        private PriorityQueue<double, Node> alreadyRendered;
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
            toRenderNew = new HeapPriorityQueue<double, Node>();
            alreadyRendered = new HeapPriorityQueue<double, Node>();
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
         //Sets RenderingPointCount to number of points visible after calling this method
        public void UpdateRenderingQueue(MeshConfiguration config) {
            if (loadingPoints) {
                throw new InvalidOperationException("Renderer is not ready for filling. Still loading points.");
            }
            if (rootNodes.Count == 0) return;
            renderingPointCount = 0;
            Vector3d cameraPosition = new Vector3d(cameraPositionF);
            toRenderNew.Clear();
            toDelete.Clear();
            notToRender.Clear();
            alreadyRendered.Clear();
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
                    double distance = center.distance(cameraPosition);
                    double slope = Math.Tan(fieldOfView / 2 * (Math.PI / 180));
                    double projectedSize = (screenHeight / 2.0) * radius / (slope * distance);
                    if (projectedSize >= minNodeSize) {
                        Vector3 projected = vpMatrix * currentNode.BoundingBox.Center().ToFloatVector();
                        projected = projected / projected.z;
                        double priority = projectedSize / projected.magnitude;
                        if (!currentNode.HasGameObjects()) {
                            toRenderNew.Enqueue(currentNode, priority);
                        } else {
                            alreadyRendered.Enqueue(currentNode, -priority);
                            renderingPointCount += currentNode.PointCount;
                        }
                        foreach (Node child in currentNode) {
                            toCheck.Enqueue(child);
                        }
                    } else {
                        //if (currentNode.HasGameObjects()) {
                            SetNodeToBeDeleted(currentNode, config);
                        //}
                    }
                } else {
                    //if (currentNode.HasGameObjects()) {
                        SetNodeToBeDeleted(currentNode, config);
                    //}
                }
            }
        }

        /* Deletes the GOs of the given node as well as all its children.
         */
        private void SetNodeToBeDeleted(Node currentNode, MeshConfiguration config) {
            //Remove lower LOD-Objects first!
            Queue<Node> childrenToCheck = new Queue<Node>();
            //Stack<Node> newNodesToDelete = new Stack<Node>(); //<- saved in a stack because the order in the queue will be reverse
            //newNodesToDelete.Push(currentNode);
            if (currentNode.HasGameObjects()) {
                currentNode.RemoveGameObjects(config);
            }
            foreach (Node child in currentNode) {
                childrenToCheck.Enqueue(child);
            }
            while (childrenToCheck.Count != 0) {
                Node child = childrenToCheck.Dequeue();
                if (child.HasGameObjects()) {
                    //newNodesToDelete.Push(child);
                    child.RemoveGameObjects(config);
                }
                foreach (Node childchild in child) {
                    childrenToCheck.Enqueue(childchild);
                }
            }
            /*while (newNodesToDelete.Count != 0) {
                Node n = newNodesToDelete.Pop();
                renderingPointCount -= n.PointCount;
                //toDelete.Enqueue(n);
                n.RemoveGameObjects(config);
            }*/
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
                foreach (Node n in toRenderNew) {
                    if (shuttingDown) return;
                    uint amount = n.PointCount;
                    //PointCount might already be sad from loading the points before
                    if (amount == 0) {
                        CloudLoader.LoadPointsForNode(n);
                        amount = n.PointCount;
                    }
                    while (renderingPointCount + amount > pointBudget && !alreadyRendered.IsEmpty()) {
                        Node u = alreadyRendered.Dequeue();
                        toDelete.Enqueue(u);//Direktes Löschen hier nicht möglich, daher über toDelete
                        renderingPointCount -= u.PointCount;
                    }
                    if (renderingPointCount + amount <= pointBudget) {
                        renderingPointCount += amount;
                        if (!n.HasPointsToRender()) {
                            CloudLoader.LoadPointsForNode(n);
                        }
                        //Vorbedingung ist, dass toRenderNew nur Nodes enthält, die noch keine GOs besitzen!!
                        //if (!n.HasGameObjects()) {
                            n.SetReadyForGameObjectCreation();
                        //}
                    } else {
                        //toRender.Remove(n); //TODO: Very ugly, fix with converter fix
                        notToRender.Add(n); //This way we do not need to remove something from the queue (would be expensive)
                        /*if (n.HasGameObjects()) { //Kann nicht der Fall sein
                            toDelete.Enqueue(n);
                        }*/
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
            for (int i = 0; i < MAX_NODES_CREATE_PER_FRAME && !toRenderNew.IsEmpty(); i++) {
                Node n = toRenderNew.Peek();
                if (notToRender.Contains(n)) {
                    notToRender.Remove(n);
                    toRenderNew.Dequeue();
                } else if (n.IsWaitingForReadySet()) //Still waiting for point rendering
                {
                    break;
                } else if (n.IsReadyForGameObjectCreation()) {
                    toRenderNew.Dequeue();
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
            return !toRenderNew.IsEmpty();
        }

        public bool HasNodesToDelete() {
            return !toDelete.IsEmpty();
        }
    }
}