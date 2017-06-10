using CloudData;
using Loading;
using ObjectCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Controllers {

    /* When the user presses "X", the cloud is reloaded according to the current camera position. "X" can be pressed anytime, even if the current loading has not been finished.
     */
    public class PointCloudSetMultiTimeController : AbstractPointSetController {

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
                PointRenderer = new ConcurrentMultiTimeRenderer(minNodeSize, pointBudget, userCamera, meshConfiguration, cacheSizeInPoints);
            } else {
                PointRenderer = new SingleThreadedMultiTimeRenderer(minNodeSize, pointBudget, userCamera, meshConfiguration);
            }
        }
        

        int lastX = 0;

        // Update is called once per frame
        void Update() {
            if (!CheckReady()) return;
            if (Input.GetKey(KeyCode.X)) {
                if (lastX == 0) {
                    Debug.Log("Updating!");
                    PointRenderer.UpdateVisibleNodes();
                    lastX = 1;
                }
            } else {
                PointRenderer.UpdateGameObjects();
                if (lastX != 0) {
                    lastX = (lastX + 1) % 10;   //Nur alle 10 Frames X drücken ermöglichen
                }
            }
        }
    }
}