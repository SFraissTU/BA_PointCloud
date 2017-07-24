using CloudData;
using Loading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Controllers {
     /// <summary>
     /// A PointSetController enables loading and rendering several point clouds at once using an AbstractRenderer.
     /// Everytime you want to use an AbstractRenderer it is recommended to use an AbstractPointSetController, even if you have only one cloud.
     /// The configured options of the point set controller (for example point budget) work for all point clouds attached to this set.
     /// Every pointcloud has its own controller (for example a DynamicLoaderController), which has to register itself at the PointSetController via the methods RegisterController, UpdateBoundingBox and AddRootNode.
     /// The only current implementation of this class is PointCloudSetRealTimeController.
     /// </summary>
    public abstract class AbstractPointSetController : MonoBehaviour {

        /// <summary>
        /// Whether the center of the cloud should be moved to the position of this component
        /// </summary>
        public bool moveCenterToTransformPosition = true;

        //For origin-moving:
        private bool hasMoved = false;
        private BoundingBox overallBoundingBox = new BoundingBox(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,
                                                                    double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
        private Dictionary<MonoBehaviour, BoundingBox> boundingBoxes = new Dictionary<MonoBehaviour, BoundingBox>();
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
        /// Registers a PointCloud-Controller (See DynamicLoaderController). This should be done in the start-method of the pc-controller and is neccessary for the bounding-box-recalculation.
        /// The whole cloud will be moved and rendered as soon as for every registererd controller the bounding box was given via UpdateBoundingBox.
        /// Should be called only once for every controller
        /// </summary>
        /// <param name="controller">not null</param>
        /// <seealso cref="DynamicLoaderController"/>
        public void RegisterController(MonoBehaviour controller) {
            lock (boundingBoxes) {
                boundingBoxes[controller] = null;
            }
        }
        
        /// <summary>
        /// Sets the bounding box of a given Cloud-Controller, which has been registered via RegisterController first. 
        /// If the bounding box should be moved (moveToOrigin), this method does not terminate until the movement has happened (via update),
        /// so this method should not be called in the main thread.
        /// </summary>
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
         /// Returns true, iff all the nodes are registered, have been moved to the center (if required) and the renderer is loaded.
         /// </summary>
        protected bool CheckReady() {
            lock (boundingBoxes) {
                if (!hasMoved) {
                    if (!boundingBoxes.ContainsValue(null)) {
                        Vector3d moving = new Vector3d(transform.position) - overallBoundingBox.Center();
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
        public void OnApplicationQuit() {
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