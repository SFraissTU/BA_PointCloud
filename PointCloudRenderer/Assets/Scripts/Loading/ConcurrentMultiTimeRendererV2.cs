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
        private List<Node> rootNodes;   //List of root nodes of the point clouds

        private MeshConfiguration config;

        //Camera Info
        private Camera camera;

        private double minNodeSize; //Min projected node size
        private uint pointBudget;   //Point Budget

        private uint renderingpointcount;

        public ConcurrentMultiTimeRendererV2(int minNodeSize, uint pointBudget, Camera camera, MeshConfiguration config) {
            rootNodes = new List<Node>();
            this.minNodeSize = minNodeSize;
            this.pointBudget = pointBudget;
            this.camera = camera;
            this.config = config;
            traversalThread = new V2TraversalThread(rootNodes, minNodeSize, pointBudget);
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
            
        }

        public void ShutDown() {
            shuttingDown = true;
        }

        public uint GetPointCount() {
            return renderingpointcount; //TODO: Lock
        }
    }
}
