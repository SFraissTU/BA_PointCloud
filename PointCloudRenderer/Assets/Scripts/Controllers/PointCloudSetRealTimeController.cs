using CloudData;
using Loading;
using ObjectCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Controllers {

    /* This PointSetController updates the loading queue every frame, so at every frame the nodes which should be loaded are adapted to the current camera position.
     */
    public class PointCloudSetRealTimeController : AbstractPointSetController {

        public uint pointBudget;
        public int minNodeSize;
        //Defines the type of PointCloud (Points, Quads, Circles)
        public MeshConfiguration meshConfiguration;
        public bool multithreaded = true;
        public uint cacheSizeInPoints = 0;

        private Camera userCamera;

        // Use this for initialization
        protected override void Initialize() {
            userCamera = Camera.main;
            if (multithreaded) {
                PointRenderer = new ConcurrentMultiTimeRenderer(minNodeSize, pointBudget, userCamera, LRUCache.CacheFromPointCount(cacheSizeInPoints));
            } else {
                PointRenderer = new SingleThreadedMultiTimeRenderer(minNodeSize, pointBudget, userCamera);
            }
        }

        // Update is called once per frame
        void Update() {
            if (!CheckReady()) return;
            PointRenderer.UpdateVisibleNodes(meshConfiguration);
            PointRenderer.UpdateGameObjects(meshConfiguration);
        }
    }
}