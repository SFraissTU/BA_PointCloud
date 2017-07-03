using CloudData;
using ObjectCreation;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Loading {
    class V2Renderer : AbstractRenderer {

        private bool shuttingDown = false;  //true, iff everything should be stopped (the point loading will stop and every method will not do anything anymore)

        private V2TraversalThread traversalThread;
        private V2LoadingThread loadingThread;
        private V2Cache cache;
        private List<Node> rootNodes;   //List of root nodes of the point clouds

        private MeshConfiguration config;
        private uint renderingpointcount;

        //Camera Info
        private Camera camera;

        private object locker = new object();
        private Queue<Node> toRender;
        private Queue<Node> toDelete;

        public V2Renderer(int minNodeSize, uint pointBudget, uint nodesLoadedPerFrame, uint nodesGOsperFrame, Camera camera, MeshConfiguration config, uint cacheSize) {
            rootNodes = new List<Node>();
            this.camera = camera;
            this.config = config;
            cache = new V2Cache(cacheSize);
            loadingThread = new V2LoadingThread(cache);
            loadingThread.Start();
            traversalThread = new V2TraversalThread(this, loadingThread, rootNodes, minNodeSize, pointBudget, nodesLoadedPerFrame, nodesGOsperFrame, cache);
            traversalThread.Start();
        }

        public void AddRootNode(Node rootNode) {
            rootNodes.Add(rootNode);
        }

        public int GetRootNodeCount() {
            return rootNodes.Count;
        }

        public bool IsRunning() {
            return !shuttingDown;
        }

        public void Update() {
            //Set new Camera Data
            traversalThread.SetNextCameraData(camera.transform.position, camera.transform.forward, GeometryUtility.CalculateFrustumPlanes(camera), camera.pixelRect.height, camera.fieldOfView);
            
            //Update GameObjects
            Queue<Node> toRender;
            Queue<Node> toDelete;
            lock (locker) {
                toRender = this.toRender;
                toDelete = this.toDelete;
            }
            if (toRender == null) return;
            while (toDelete.Count != 0) {
                Node n = toDelete.Dequeue();
                lock (n) {
                    if (n.HasGameObjects()) {
                        n.RemoveGameObjects(config);
                        cache.Insert(n);
                    }
                }
            }
            while (toRender.Count != 0) {
                Node n = toRender.Dequeue();
                lock (n) {
                    if (n.HasPointsToRender() && (n.Parent == null || n.Parent.HasGameObjects())) {
                        n.CreateGameObjects(config);
                    }
                }
            }

            //Notify Traversal Thread
            lock (traversalThread) {
                Monitor.PulseAll(traversalThread);
            }
        }

        public void ShutDown() {
            shuttingDown = true;
            traversalThread.Stop();
            lock (traversalThread) {
                Monitor.PulseAll(traversalThread);
            }
            loadingThread.Stop();
            
        }

        public uint GetPointCount() {
            return renderingpointcount;
        }

        public void SetQueues(Queue<Node> toRender, Queue<Node> toDelete, uint pointcount) {
            lock (locker) {
                this.toRender = toRender;
                this.toDelete = toDelete;
                this.renderingpointcount = pointcount;
            }
        }
    }
}
