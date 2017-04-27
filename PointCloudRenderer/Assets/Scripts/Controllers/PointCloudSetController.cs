using CloudData;
using Loading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Controllers {

    /* Contains options for a set of PointClouds
     */
    public class PointCloudSetController : MonoBehaviour {

        public uint pointBudget;
        public int minNodeSize;
        public bool moveToOrigin;

        private bool hasMovedToOrigin = false;

        private BoundingBox overallBoundingBox = new BoundingBox(Double.PositiveInfinity,Double.PositiveInfinity,Double.PositiveInfinity,
                                                                    Double.NegativeInfinity,Double.NegativeInfinity,Double.NegativeInfinity);
        private Dictionary<MonoBehaviour, BoundingBox> boundingBoxes = new Dictionary<MonoBehaviour, BoundingBox>();
        //private List<BoundingBox> otherBoundingBoxes = new List<BoundingBox>();
        private ManualResetEvent waiterForBoundingBoxUpdate = new ManualResetEvent(false);

        // Use this for initialization
        void Start() {
        }

        public void RegisterController(MonoBehaviour controller) {
            boundingBoxes[controller] = null;
            Debug.Log("Registering Controller");
        }

        public void UpdateBoundingBox(MonoBehaviour controller, BoundingBox boundingBox) {
            boundingBoxes[controller] = boundingBox;
            overallBoundingBox.Lx = Math.Min(overallBoundingBox.Lx, boundingBox.Lx);
            overallBoundingBox.Ly = Math.Min(overallBoundingBox.Ly, boundingBox.Ly);
            overallBoundingBox.Lz = Math.Min(overallBoundingBox.Lz, boundingBox.Lz);
            overallBoundingBox.Ux = Math.Max(overallBoundingBox.Ux, boundingBox.Ux);
            overallBoundingBox.Uy = Math.Max(overallBoundingBox.Uy, boundingBox.Uy);
            overallBoundingBox.Uz = Math.Max(overallBoundingBox.Uz, boundingBox.Uz);
            if (moveToOrigin) {
                waiterForBoundingBoxUpdate.WaitOne();
            }
        }

        // Update is called once per frame
        void Update() {
            if (moveToOrigin && !hasMovedToOrigin && !boundingBoxes.ContainsValue(null)) {
                Vector3d moving = overallBoundingBox.DistanceToOrigin();
                foreach (BoundingBox bb in boundingBoxes.Values) {
                    bb.MoveAlong(moving);
                }
                overallBoundingBox.MoveAlong(moving);
                hasMovedToOrigin = true;
                waiterForBoundingBoxUpdate.Set();
            }
        }
    }
}