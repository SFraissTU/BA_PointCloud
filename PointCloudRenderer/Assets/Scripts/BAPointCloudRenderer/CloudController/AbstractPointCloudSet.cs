using BAPointCloudRenderer.CloudData;
using BAPointCloudRenderer.Loading;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BAPointCloudRenderer.CloudController {
    /// <summary>
    /// A PointCloudSet enables loading and rendering several point clouds at once. But even if you just have one point cloud to render, you have to attach it to a PointCloudSet.
    /// The configured options of the PointCloudSet controller (for example point budget) work for all point clouds attached to this set.
    /// Every pointcloud has its own PointCloudLoader, which has to register itself at the PointSetController via the methods RegisterController, UpdateBoundingBox and AddRootNode.
    /// The current implementations of this class are StaticPointCloudSet and DynamicPointCloudSet.
    /// </summary>
    public abstract class AbstractPointCloudSet: MonoBehaviour {

        /// <summary>
        /// Whether the center of the cloud should be moved to the position of this component. To calculate the center, only the point clouds are considered that exist in the beginning of the scene.
        /// </summary>
        public bool moveCenterToTransformPosition = true;

        //For origin-moving:
        private bool hasMoved = false;
        private Vector3d moving = new Vector3d(0,0,0);
        private BoundingBox overallBoundingBox = new BoundingBox(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,
                                                                    double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
        private Dictionary<PointCloudLoader, BoundingBox> boundingBoxes = new Dictionary<PointCloudLoader, BoundingBox>();
        private ManualResetEvent waiterForBoundingBoxUpdate = new ManualResetEvent(false);

        private AbstractRenderer pRenderer;

        void Start() {
            if (!moveCenterToTransformPosition) hasMoved = true;
            Initialize();
            if (pRenderer == null) {
                throw new InvalidOperationException("PointRenderer has not been set!");
            }
        }

        /// <summary>
        /// Override this instead of Start!! Make sure to set the PointRenderer in here!!!
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// Registers a PointCloud-Controller (See PointCloudLoader). This should be done in the start-method of the pc-controller and is neccessary for the bounding-box-recalculation.
        /// The whole cloud will be moved and rendered as soon as for every registererd controller the bounding box was given via UpdateBoundingBox.
        /// Should be called only once for every controller
        /// </summary>
        /// <param name="controller">not null</param>
        /// <seealso cref="DynamicLoaderController"/>
        public void RegisterController(PointCloudLoader controller) {
            lock (boundingBoxes) {
                boundingBoxes[controller] = null;
            }
        }

        /// <summary>
        /// Sets the bounding box of a given Cloud-Controller, which has been registered via RegisterController first. 
        /// If the bounding box should be moved (moveToOrigin), this method does not terminate until the movement has happened (via update),
        /// so this method should not be called in the main thread.
        /// </summary>
        public void UpdateBoundingBox(PointCloudLoader controller, BoundingBox boundingBox) {
            boundingBox.MoveAlong(moving);
            lock (boundingBoxes) {
                boundingBoxes[controller] = boundingBox;
                overallBoundingBox.Lx = Math.Min(overallBoundingBox.Lx, boundingBox.Lx);
                overallBoundingBox.Ly = Math.Min(overallBoundingBox.Ly, boundingBox.Ly);
                overallBoundingBox.Lz = Math.Min(overallBoundingBox.Lz, boundingBox.Lz);
                overallBoundingBox.Ux = Math.Max(overallBoundingBox.Ux, boundingBox.Ux);
                overallBoundingBox.Uy = Math.Max(overallBoundingBox.Uy, boundingBox.Uy);
                overallBoundingBox.Uz = Math.Max(overallBoundingBox.Uz, boundingBox.Uz);
            }
            if (moveCenterToTransformPosition) {
                waiterForBoundingBoxUpdate.WaitOne();
            }
        }

        /// <summary>
        /// Adds a root node to the renderer. Should be called by the PC-Controller, which also has to call RegisterController and UpdateBoundingBox.
        /// </summary>
        public void AddRootNode(Node node) {
            lock (pRenderer) {
                pRenderer.AddRootNode(node);
            }
        }

        /// <summary>
        /// Removes a point cloud
        /// </summary>
        public void RemoveRootNode(PointCloudLoader controller, Node node) {
            lock (pRenderer) {
                pRenderer.RemoveRootNode(node);
                boundingBoxes.Remove(controller);
            }
        }

        /// <summary>
        /// Returns true, iff all the nodes are registered, have been moved to the center (if required) and the renderer is loaded.
        /// </summary>
        protected bool CheckReady() {
            lock (boundingBoxes) {
                if (!hasMoved) {
                    if (!boundingBoxes.ContainsValue(null)) {
                        moving = new Vector3d(transform.position) - overallBoundingBox.Center();
                        foreach (BoundingBox bb in boundingBoxes.Values) {
                            bb.MoveAlong(moving);
                        }
                        overallBoundingBox.MoveAlong(moving);
                        hasMoved = true;
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

        /// <summary>
        /// Shuts down the renderer
        /// </summary>
        public void OnDestroy() {
            if (pRenderer != null) {
                pRenderer.ShutDown();
            }
        }

        /// <summary>
        /// Returns the point count
        /// </summary>
        /// <returns></returns>
        public uint GetPointCount() {
            return pRenderer.GetPointCount();
        }

        public void StopRendering() {
            if (pRenderer != null) {
                pRenderer.ShutDown();
            }
        }

        /// <summary>
        /// The Renderer (value may not be null at setting)
        /// </summary>
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
