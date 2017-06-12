using CloudData;
using Loading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Controllers {
    /* Super class for all PointSet-Controllers. A PointSetController enables the loading of several point clouds at once.
     * This enabled loading several PointClouds at the same time. The configured options work globally for all point clouds.
     * The part that this abstract class is responsible for is the waiting for every Cloud to register itself at the PointSet and to move it to the origin, if so wanted by the user.
     */
    public abstract class AbstractPointSetController : MonoBehaviour {

        public bool moveToOrigin = true;

        //For origin-moving:
        private bool hasMovedToOrigin = false;
        private BoundingBox overallBoundingBox = new BoundingBox(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,
                                                                    double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
        private Dictionary<MonoBehaviour, BoundingBox> boundingBoxes = new Dictionary<MonoBehaviour, BoundingBox>();
        private ManualResetEvent waiterForBoundingBoxUpdate = new ManualResetEvent(false);

        private AbstractRenderer pRenderer;

        void Start() {
            if (!moveToOrigin) hasMovedToOrigin = true;
            Initialize();
            if (pRenderer == null) {
                throw new InvalidOperationException("PointRenderer has not been set!");
            }
        }

        //Override this instead of Start!! Make sure to set the PointRenderer in here!!!
        protected abstract void Initialize();

        //Register a PointCloud-Controller. This should be done in the start-method of the controller and is neccessary for the bounding-box-recalculation.
        //The whole cloud will be moved and rendered as soon as for every registered controller the bounding box is given via UpdateBoundingBox
        //Can be called several times, registering will only be done once, but boundingboxes will be deleted
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

        //Adds a rootNode to the renderer.
        public void AddRootNode(Node node) {
            lock (pRenderer) {
                pRenderer.AddRootNode(node);
            }
        }

        /* Returns true, if all the nodes are registered, have been moved to the center and the renderer is loaded
         */
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

        public void OnApplicationQuit() {
            if (pRenderer != null) {
                pRenderer.ShutDown();
            }
        }

        public uint GetPointCount() {
            return pRenderer.GetPointCount();
        }

        public AbstractRenderer PointRenderer {
            get {
                return pRenderer;
            }

            set {
                if (value != null) {
                    pRenderer = value;
                }
            }
        }
    }
}