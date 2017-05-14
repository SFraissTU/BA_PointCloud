using CloudData;
using Loading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Controllers {
    public class AbstractPointSetController : MonoBehaviour {

        public bool moveToOrigin = true;
        //Defines the type of PointCloud (Points, Quads, Circles)

        //For origin-moving:
        private bool hasMovedToOrigin = false;
        private BoundingBox overallBoundingBox = new BoundingBox(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,
                                                                    double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
        private Dictionary<MonoBehaviour, BoundingBox> boundingBoxes = new Dictionary<MonoBehaviour, BoundingBox>();
        private ManualResetEvent waiterForBoundingBoxUpdate = new ManualResetEvent(false);

        protected AbstractRenderer pRenderer;

        // Use this for initialization
        protected virtual void Start() {
            if (!moveToOrigin) hasMovedToOrigin = true;
        }

        //Register a Controller. This should be done in the start-method of the controller and is neccessary for the bounding-box-recalculation.
        //The whole cloud will be moved and rendered as soon as for every registered controller the bounding box is given via UpdateBoundingBox
        public void RegisterController(MonoBehaviour controller) {
            lock (boundingBoxes) {
                boundingBoxes[controller] = null;
            }
        }

        //Sets the bounding box of a given Cloud-Controller. If the bounding box should be moved (moveToOrigin), this method does not terminate until the movement has happened (via update),
        //so this method should be called in an extra thread
        public void UpdateBoundingBox(MonoBehaviour controller, BoundingBox boundingBox) {
            lock (boundingBoxes) {
                boundingBoxes[controller] = boundingBox;
                overallBoundingBox.Lx = Math.Min(overallBoundingBox.Lx, boundingBox.Lx);
                overallBoundingBox.Ly = Math.Min(overallBoundingBox.Ly, boundingBox.Ly);
                overallBoundingBox.Lz = Math.Min(overallBoundingBox.Lz, boundingBox.Lz);
                overallBoundingBox.Ux = Math.Max(overallBoundingBox.Ux, boundingBox.Ux);
                overallBoundingBox.Uy = Math.Max(overallBoundingBox.Uy, boundingBox.Uy);
                overallBoundingBox.Uz = Math.Max(overallBoundingBox.Uz, boundingBox.Uz);
            }
            if (moveToOrigin) {
                waiterForBoundingBoxUpdate.WaitOne();
            }
        }

        protected bool CheckReady() {
            lock (boundingBoxes) {
                if (!hasMovedToOrigin) {
                    if (!boundingBoxes.ContainsValue(null)) {
                        Vector3d moving = overallBoundingBox.DistanceToOrigin();
                        foreach (BoundingBox bb in boundingBoxes.Values) {
                            bb.MoveAlong(moving);
                        }
                        overallBoundingBox.MoveAlong(moving);
                        hasMovedToOrigin = true;
                        waiterForBoundingBoxUpdate.Set();
                    } else {
                        return false;
                    }
                }
            }
            lock (pRenderer) {
                //Checking, weither all RootNodes are there
                if (pRenderer.GetRootNodeCount() != boundingBoxes.Count) {
                    return false;
                }
            }
            return true;
        }



        //Call this before setting the bounding boxes
        public void AddRootNode(Node node) {
            lock (pRenderer) {
                pRenderer.AddRootNode(node);
            }
        }

        public void OnApplicationQuit() {
            pRenderer.ShutDown();
        }
    }
}