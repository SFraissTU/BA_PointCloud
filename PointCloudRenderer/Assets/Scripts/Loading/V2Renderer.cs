using CloudData;
using ObjectCreation;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Loading {
    /// <summary>
    /// The multithreaded Real-Time-Renderer as described in the Bachelor Thesis in chapter 3.2.2 - 3.2.7
    /// </summary>
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

        /// <summary>
        /// Creates a new V2Renderer and starts all the threads
        /// </summary>
        /// <param name="minNodeSize">Minimum Node Size</param>
        /// <param name="pointBudget">Point Budget</param>
        /// <param name="nodesLoadedPerFrame">Maximum number of nodes loaded per frame</param>
        /// <param name="nodesGOsperFrame">Maximum number of nodes for which GameObjects should be created per frame</param>
        /// <param name="camera">User Camera</param>
        /// <param name="config">MeshConfiguration, defining how the points should be rendered</param>
        /// <param name="cacheSize">Size of cache in points</param>
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

        /// <summary>
        /// Registers the root node of a point cloud in the renderer.
        /// </summary>
        /// <param name="rootNode">not null</param>
        public void AddRootNode(Node rootNode) {
            rootNodes.Add(rootNode);
        }

        /// <summary>
        /// Returns how many root nodes have been added
        /// </summary>
        public int GetRootNodeCount() {
            return rootNodes.Count;
        }

        /// <summary>
        /// True, if ShutDown() has not been called yet
        /// </summary>
        public bool IsRunning() {
            return !shuttingDown;
        }

        /// <summary>
        /// Gives the current camera data to the traversal thread and updates the GameObjects. Called from the MainThread. As described in the Bachelor Thesis in chapter 3.1.3 "Main Thread"
        /// </summary>
        public void Update() {
            //Set new Camera Data
            traversalThread.SetNextCameraData(camera.transform.position, camera.transform.forward, GeometryUtility.CalculateFrustumPlanes(camera), camera.pixelRect.height, camera.fieldOfView);
            
            //Update GameObjects
            Queue<Node> toRender;
            Queue<Node> toDelete;
            lock (locker) {
                toRender = this.toRender;
                toDelete = this.toDelete;
                this.toRender = null;
                this.toDelete = null;
            }
            if (toRender == null) {
                return;
            }
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

        /// <summary>
        /// Stops the rendering process and all threads
        /// </summary>
        public void ShutDown() {
            shuttingDown = true;
            traversalThread.Stop();
            lock (traversalThread) {
                Monitor.PulseAll(traversalThread);
            }
            loadingThread.Stop();
            
        }

        /// <summary>
        /// Returns the current PointCount, so how many points are loaded / visible
        /// </summary>
        public uint GetPointCount() {
            return renderingpointcount;
        }

        /// <summary>
        /// Sets the new GO-update-queues. Called from the TraversalThread.
        /// </summary>
        public void SetQueues(Queue<Node> toRender, Queue<Node> toDelete, uint pointcount) {
            lock (locker) {
                this.toRender = toRender;
                this.toDelete = toDelete;
                this.renderingpointcount = pointcount;
            }
        }
    }
}
