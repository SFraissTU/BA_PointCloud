using CloudData;
using Loading;
using ObjectCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Controllers {

    /* This enabled loading several PointClouds at the same time. The configured options work globally.
     * This class uses a ConcurrentOneTimeRenderer, so only when the user presses "X", the cloud is reloaded
     */
    public class PointCloudSetMultiTimeController : AbstractPointSetController {

        public uint pointBudget;
        public int minNodeSize;
        //Defines the type of PointCloud (Points, Quads, Circles)
        public MeshConfiguration meshConfiguration;

        private Camera userCamera;

        // Use this for initialization
        protected override void Start() {
            userCamera = Camera.main;
            //pRenderer = new SingleThreadedMultiTimeRenderer(minNodeSize, pointBudget, userCamera);
            pRenderer = new ConcurrentMultiTimeRenderer(minNodeSize, pointBudget, userCamera);
            pRenderer.StartUpdatingPoints();
            base.Start();
        }
        

        int lastX = 0;

        // Update is called once per frame
        void Update() {
            if (!CheckReady()) return;
            if (Input.GetKey(KeyCode.X)) {
                if (lastX == 0) {
                    Debug.Log("Updating!");
                    pRenderer.UpdateRenderingQueue(meshConfiguration);
                    lastX = 1;
                }
            } else {
                pRenderer.UpdateGameObjects(meshConfiguration);
                if (lastX != 0) {
                    lastX = (lastX + 1) % 10;   //Nur alle 10 Frames X drücken ermöglichen
                }
            }
        }
    }
}