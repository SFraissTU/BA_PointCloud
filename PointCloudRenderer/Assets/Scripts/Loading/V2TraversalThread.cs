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
        private uint nodesLoadedPerFrame;
        private uint nodesGOsPerFrame;
        private bool running = true;

        //Camera Data
        Vector3 cameraPosition;
        float screenHeight;
        float fieldOfView;
        Plane[] frustum;
        Vector3 camForward;

        private Queue<Node> toDelete;
        private Queue<Node> toRender;
        private HashSet<Node> visibleNodes;

        private V2Renderer mainThread;
        private V2LoadingThread loadingThread;
        private V2Cache cache;

        public V2TraversalThread(V2Renderer mainThread, V2LoadingThread loadingThread, List<Node> rootNodes, double minNodeSize, uint pointBudget, uint nodesLoadedPerFrame, uint nodesGOsPerFrame, V2Cache cache) {
            this.mainThread = mainThread;
            this.loadingThread = loadingThread;
            this.rootNodes = rootNodes;
            this.minNodeSize = minNodeSize;
            this.pointBudget = pointBudget;
            visibleNodes = new HashSet<Node>();
            this.cache = cache;
            this.nodesLoadedPerFrame = nodesLoadedPerFrame;
            this.nodesGOsPerFrame = nodesGOsPerFrame;
        }

        public void Start() {
            new Thread(Run).Start();
        }

        private void Run() {
            try {
                while (running) {
                    toDelete = new Queue<Node>();
                    toRender = new Queue<Node>();
                    var toProcess = Traverse();
                    uint pointcount = BuildRenderingQueue(toProcess);
                    mainThread.SetQueues(toRender, toDelete, pointcount);
                    lock (this) {
                        Monitor.Wait(this);
                    }
                }
            } catch (Exception ex) {
                Debug.LogError(ex);
            }
            Debug.Log("Traversal Thread stopped");
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

        private PriorityQueue<LoadingPriority, Node> Traverse() {
            //Camera Data
            Vector3 cameraPosition;
            Vector3 camForward;
            Plane[] frustum;
            float screenHeight;
            float fieldOfView;

            PriorityQueue<LoadingPriority, Node> toProcess = new HeapPriorityQueue<LoadingPriority, Node>();

            lock (locker) {
                if (this.frustum == null) {
                    return toProcess;
                }
                cameraPosition = this.cameraPosition;
                camForward = this.camForward;
                frustum = this.frustum;
                screenHeight = this.screenHeight;
                fieldOfView = this.fieldOfView;
            }
            //Clearing Queues

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
            while (toCheck.Count != 0 && running) {
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
                    //Vector3d cP3d = new Vector3d(cameraPosition);
                    //Vector3d center = currentNode.BoundingBox.Center();
                    //double distance = center.Distance(cP3d);
                    //double slope = Math.Tan(fieldOfView / 2 * Mathf.Deg2Rad);
                    //double projectedSize = (screenHeight / 2.0) * radii[currentNode.MetaData] / (slope * distance);
                    if (projectedSize >= minNodeSize) {
                        Vector3 camToNodeCenterDir = (center - cameraPosition).normalized;
                        double angle = Math.Acos(camForward.x * camToNodeCenterDir.x + camForward.y * camToNodeCenterDir.y + camForward.z * camToNodeCenterDir.z);
                        double angleWeight = Math.Abs(angle) + 1.0; //+1, to prevent divsion by zero
                        //Vector3d camToNodeCenterDir = (center - cP3d).Normalize();
                        //Vector3d camToScreenCenterDir = new Vector3d(camForward);
                        //double angle = Math.Acos(camToScreenCenterDir * camToNodeCenterDir);
                        //double angleWeight = Math.Abs(angle) + 1.0; //+1, to prevent divsion by zero
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
            return toProcess;
        }

        private void DeleteNode(Node currentNode) {
            lock (currentNode) {
                if (!currentNode.HasGameObjects()) {
                    return;
                }
            }
            Queue<Node> nodesToDelete = new Queue<Node>();
            nodesToDelete.Enqueue(currentNode);
            Stack<Node> tempToDelete = new Stack<Node>();   //To assure better order in cache

            while (nodesToDelete.Count != 0) {
                Node child = nodesToDelete.Dequeue();
                Monitor.Enter(child);
                if (child.HasGameObjects()) {
                    Monitor.Exit(child);
                    tempToDelete.Push(child);

                    foreach (Node childchild in child) {
                        nodesToDelete.Enqueue(childchild);
                    }
                } else {
                    Monitor.Exit(child);
                }
            }
            while (tempToDelete.Count != 0) {
                Node n = tempToDelete.Pop();
                toDelete.Enqueue(n);
            }
        }

        private uint BuildRenderingQueue(PriorityQueue<LoadingPriority, Node> toProcess) {
            uint renderingpointcount = 0;
            uint maxnodestoprocess = nodesLoadedPerFrame;
            uint maxnodestorender = nodesGOsPerFrame;
            HashSet<Node> newVisibleNodes = new HashSet<Node>();
            while (maxnodestoprocess > 0 && maxnodestorender > 0 && !toProcess.IsEmpty()) {
                LoadingPriority p;
                Node n = toProcess.Dequeue(out p);
                lock (n) {
                    if (n.PointCount == -1) {
                        loadingThread.ScheduleForLoading(n);
                        --maxnodestoprocess;
                    } else if (renderingpointcount + n.PointCount <= pointBudget) {
                        if (n.HasGameObjects()) {
                            renderingpointcount += (uint)n.PointCount;
                            visibleNodes.Remove(n);
                            newVisibleNodes.Add(n);
                        } else if (n.HasPointsToRender()) {
                            //Might be in Cache -> Withdraw
                            cache.Withdraw(n);
                            renderingpointcount += (uint)n.PointCount;
                            toRender.Enqueue(n);
                            --maxnodestorender;
                            newVisibleNodes.Add(n);
                        } else {
                            loadingThread.ScheduleForLoading(n);
                            --maxnodestoprocess;
                        }
                    } else {
                        maxnodestoprocess = 0;
                        maxnodestorender = 0;
                        if (n.HasGameObjects()) {
                            visibleNodes.Remove(n);
                            DeleteNode(n);
                        }
                    }
                }
            }
            foreach (Node n  in visibleNodes) {
                DeleteNode(n);
            }
            visibleNodes = newVisibleNodes;
            return renderingpointcount;
        }

        public void Stop() {
            running = false;
        }

    }
}
