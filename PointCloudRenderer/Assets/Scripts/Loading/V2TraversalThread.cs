using CloudData;
using DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Loading {
    class V2TraversalThread {

        private object locker = new object();
        private List<Node> rootNodes;
        private double minNodeSize; //Min projected node size
        private uint pointBudget;   //Point Budget
        private bool shuttingDown = false;

        //Camera Data
        Vector3 cameraPosition;
        float screenHeight;
        float fieldOfView;
        Plane[] frustum;
        Vector3 camForward;

        public V2TraversalThread(List<Node> rootNodes, double minNodeSize, uint pointBudget) {
            this.rootNodes = rootNodes;
            this.minNodeSize = minNodeSize;
            this.pointBudget = pointBudget;
        }

        public void Start() {
            new Thread(Run).Start();
        }

        private void Run() {
            while(!shuttingDown) {
                Traverse();
            }
        }

        public void SetNextCameraData(Vector3 cameraPosition, Vector3 camForward, Plane[] frustum, float screenHeight, float fieldOfView) {
            lock (locker) {
                this.cameraPosition = cameraPosition;
                this.camForward = camForward;
                this.frustum = frustum;
                this.screenHeight = screenHeight;
                this.fieldOfView = fieldOfView;
            }
        }

        private void Traverse() {
            //Camera Data
            Vector3 cameraPosition;
            Vector3 camForward;
            Plane[] frustum;
            float screenHeight;
            float fieldOfView;
            lock (locker) {
                if (this.frustum == null) {
                    return;
                }
                cameraPosition = this.cameraPosition;
                camForward = this.camForward;
                frustum = this.frustum;
                screenHeight = this.screenHeight;
                fieldOfView = this.fieldOfView;
            }
            //Clearing Queues
            PriorityQueue<LoadingPriority, Node> toProcess = new HeapPriorityQueue<LoadingPriority, Node>();
            Queue<Node> toRender = new Queue<Node>();

            //Initializing Checking-Queue
            Queue<Node> toCheck = new Queue<Node>();
            foreach (Node rootNode in rootNodes) {
                toCheck.Enqueue(rootNode);
            }
            //Radii & Level
            Dictionary<PointCloudMetaData, double> radii = new Dictionary<PointCloudMetaData, double>(rootNodes.Count);
            for (int i = 0; i < rootNodes.Count; i++) {
                radii.Add(rootNodes[i].MetaData, rootNodes[i].BoundingBox.Radius());
            }
            int lastLevel = 0;
            //Check all nodes - Breadth first
            while (toCheck.Count != 0 && !shuttingDown) {
                Node currentNode = toCheck.Dequeue();
                //Check Level and radius
                if (currentNode.GetLevel() > lastLevel) {
                    for (int i = 0; i < rootNodes.Count; i++) {
                        radii[rootNodes[i].MetaData] /= 2;
                    }
                    ++lastLevel;
                }

                //Is Node inside frustum?
                if (Util.InsideFrustum(currentNode.BoundingBox, frustum)) { 
                    //Calculate projected size
                    Vector3 center = currentNode.BoundingBox.GetBoundsObject().center;
                    double distance = (center - cameraPosition).magnitude;
                    double slope = Math.Tan(fieldOfView / 2 * Mathf.Deg2Rad);
                    double projectedSize = (screenHeight / 2.0) * radii[currentNode.MetaData] / (slope * distance);
                    if (projectedSize >= minNodeSize) {
                        Vector3 camToNodeCenterDir = (center - cameraPosition).normalized;
                        double angle = Math.Acos(camForward.x*camToNodeCenterDir.x + camForward.y*camToNodeCenterDir.y + camForward.z*camToNodeCenterDir.z);
                        double angleWeight = Math.Abs(angle) + 1.0; //+1, to prevent divsion by zero
                        //angleWeight = Math.Pow(angle, 2);
                        double priority = projectedSize / angleWeight;

                        toProcess.Enqueue(currentNode, new LoadingPriority(currentNode.MetaData, currentNode.Name, priority, false));

                        foreach (Node child in currentNode) {
                            toCheck.Enqueue(child);
                        }

                    } else {
                        //This node or its children might be visible
                        DeleteNode(currentNode);
                    }
                } else {
                    //This node or its children might be visible
                    DeleteNode(currentNode);
                }
            }
        }

        private void DeleteNode(Node currentNode) {
            //TODO
            Queue<Node> nodesToDelete = new Queue<Node>();
            nodesToDelete.Enqueue(currentNode);

            while (nodesToDelete.Count != 0) {
                Node child = nodesToDelete.Dequeue();
                if (child.HasGameObjects()) {

                    //child.RemoveGameObjects(config);

                    foreach (Node childchild in child) {
                        nodesToDelete.Enqueue(childchild);
                    }
                }
            }
        }

        public void Stop() {
            shuttingDown = true;
        }

    }
}
