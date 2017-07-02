using CloudData;
using DataStructures;
using ObjectCreation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Loading {
    class ConcurrentMultiTimeRendererV2 : AbstractRenderer {

        private bool shuttingDown = false;  //true, iff everything should be stopped (the point loading will stop and every method will not do anything anymore)

        private V2TraversalThread traversalThread;
        private V2LoadingThread loadingThread;
        private List<Node> rootNodes;   //List of root nodes of the point clouds

        private MeshConfiguration config;
        private uint renderingpointcount;

        //Camera Info
        private Camera camera;

        private object locker = new object();
        private Queue<Node> toRender;
        private Queue<Node> toDelete;

        public ConcurrentMultiTimeRendererV2(int minNodeSize, uint pointBudget, Camera camera, MeshConfiguration config) {
            rootNodes = new List<Node>();
            this.camera = camera;
            this.config = config;
            loadingThread = new V2LoadingThread();
            loadingThread.Start();
            traversalThread = new V2TraversalThread(this, loadingThread, rootNodes, minNodeSize, pointBudget, 0);
            traversalThread.Start();
        }

        public void AddRootNode(Node rootNode) {
            rootNodes.Add(rootNode);
        }

        public int GetRootNodeCount() {
            return rootNodes.Count;
        }

        public bool IsReadyForUpdate() {
            return !shuttingDown; //TODO: Except its updating right now
        }

        public void UpdateVisibleNodes() {
            traversalThread.SetNextCameraData(camera.transform.position, camera.transform.forward, GeometryUtility.CalculateFrustumPlanes(camera), camera.pixelRect.height, camera.fieldOfView);
        }
             

        public void UpdateGameObjects() {
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
                    }
                }
            }
            while (toRender.Count != 0) {
                Node n = toRender.Dequeue();
                lock (n) {
                    if (n.HasPointsToRender() && (n.Parent == null || n.Parent.HasGameObjects())) {
                        n.CreateGameObjects(config);
                        n.ForgetPoints();
                    }
                }
            }
        }

        public void ShutDown() {
            shuttingDown = true;
            traversalThread.Stop();
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
