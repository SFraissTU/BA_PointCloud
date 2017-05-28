using CloudData;
using Loading;
using ObjectCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Controllers {

    /* 
     * This class uses a OneTimeRenderer, so only when the user presses "X", the cloud is reloaded according to the current camera position. The next loading can only be started when the current loading is completely finished.
     */
    public class PointCloudSetOneTimeController : AbstractPointSetController {

        public uint pointBudget;
        public int minNodeSize;
        //Defines the type of PointCloud (Points, Quads, Circles)
        public MeshConfiguration meshConfiguration;
        public bool multithreaded = true;

        private Camera userCamera;

        // Use this for initialization
        protected override void Initialize() {
            userCamera = Camera.main;
            if (multithreaded) {
                PointRenderer = new ConcurrentOneTimeRenderer(minNodeSize, pointBudget, userCamera);
            } else {
                PointRenderer = new SingleThreadedOneTimeRenderer(minNodeSize, pointBudget, userCamera);
            }
        }
        
        // Update is called once per frame
        void Update() {
            if (!CheckReady()) return;
            if (PointRenderer.IsReadyForUpdate() && Input.GetKey(KeyCode.X)) {
                Debug.Log("Updating!");
                PointRenderer.UpdateVisibleNodes(meshConfiguration);
            } else {
                PointRenderer.UpdateGameObjects(meshConfiguration);
            }
        }
    }
}