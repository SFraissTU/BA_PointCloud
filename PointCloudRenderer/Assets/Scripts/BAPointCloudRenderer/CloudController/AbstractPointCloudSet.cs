using BAPointCloudRenderer.CloudData;
using BAPointCloudRenderer.Loading;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEditor;
using BAPointCloudRenderer.ObjectCreation;

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

        /// <summary>
        /// Whether a bounding box for the whole point cloud set should be displayed. This is rendered as a gizmo, so it might only be visible in Scene View.
        /// </summary>
        public bool showBoundingBox = false;

        /// <summary>
        /// MeshConfiguration. Defines how to render the points.
        /// </summary>
        public MeshConfiguration meshConfiguration = null;

        //For origin-moving:
        private bool hasMoved = false;
        private Vector3d moving = new Vector3d(0,0,0);
        private BoundingBox overallBoundingBox = new BoundingBox(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,
                                                                    double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
        private BoundingBox overallTightBoundingBox = new BoundingBox(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,
                                                                    double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
        private Dictionary<PointCloudLoader, BoundingBox> boundingBoxes = new Dictionary<PointCloudLoader, BoundingBox>();
        private Dictionary<PointCloudLoader, BoundingBox> tightBoundingBoxes = new Dictionary<PointCloudLoader, BoundingBox>();
        private ManualResetEvent waiterForBoundingBoxUpdate = new ManualResetEvent(false);

        private AbstractRenderer pRenderer;

        private ManualResetEvent initializedEvent = new ManualResetEvent(false);

        public void Start() {
            if (!moveCenterToTransformPosition)
            {
                hasMoved = true;
            }
            Initialize();
            if (pRenderer == null) {
                throw new InvalidOperationException("PointRenderer has not been set!");
            }
            initializedEvent.Set();
        }

        /// <summary>
        /// Returns true, iff Start has already been executed
        /// </summary>
        public bool IsInitialized()
        {
            return initializedEvent.WaitOne(0);
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
        /// <seealso cref="PointCloudLoader"/>
        public void RegisterController(PointCloudLoader controller) {
            lock (boundingBoxes) {
                boundingBoxes[controller] = null;
                tightBoundingBoxes[controller] = null;
            }
        }

        /// <summary>
        /// Sets the bounding box of a given Cloud-Controller, which has been registered via RegisterController first. 
        /// If the bounding box should be moved (moveToOrigin), this method does not terminate until the movement has happened (via update),
        /// so this method should not be called in the main thread.
        /// </summary>
        public void UpdateBoundingBox(PointCloudLoader controller, BoundingBox boundingBox, BoundingBox tightBoundingBox) {
            initializedEvent.WaitOne();
            boundingBox.MoveAlong(moving);
            tightBoundingBox.MoveAlong(moving);
            lock (boundingBoxes) {
                boundingBoxes[controller] = boundingBox;
                tightBoundingBoxes[controller] = tightBoundingBox;
                overallBoundingBox.Lx = Math.Min(overallBoundingBox.Lx, boundingBox.Lx);
                overallBoundingBox.Ly = Math.Min(overallBoundingBox.Ly, boundingBox.Ly);
                overallBoundingBox.Lz = Math.Min(overallBoundingBox.Lz, boundingBox.Lz);
                overallBoundingBox.Ux = Math.Max(overallBoundingBox.Ux, boundingBox.Ux);
                overallBoundingBox.Uy = Math.Max(overallBoundingBox.Uy, boundingBox.Uy);
                overallBoundingBox.Uz = Math.Max(overallBoundingBox.Uz, boundingBox.Uz);
                overallTightBoundingBox.Lx = Math.Min(overallTightBoundingBox.Lx, tightBoundingBox.Lx);
                overallTightBoundingBox.Ly = Math.Min(overallTightBoundingBox.Ly, tightBoundingBox.Ly);
                overallTightBoundingBox.Lz = Math.Min(overallTightBoundingBox.Lz, tightBoundingBox.Lz);
                overallTightBoundingBox.Ux = Math.Max(overallTightBoundingBox.Ux, tightBoundingBox.Ux);
                overallTightBoundingBox.Uy = Math.Max(overallTightBoundingBox.Uy, tightBoundingBox.Uy);
                overallTightBoundingBox.Uz = Math.Max(overallTightBoundingBox.Uz, tightBoundingBox.Uz);
            }
            if (moveCenterToTransformPosition) {
                waiterForBoundingBoxUpdate.WaitOne();
            }
        }

        private void RecalculateBoundingBox()
        {
            lock (boundingBoxes)
            {
                BoundingBox noBB = new BoundingBox(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,
                                                                    double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
                BoundingBox noTBB = new BoundingBox(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,
                                                                    double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
                foreach (PointCloudLoader key in boundingBoxes.Keys)
                {
                    BoundingBox boundingBox = boundingBoxes[key];
                    BoundingBox tightBoundingBox = tightBoundingBoxes[key];
                    if (boundingBox != null)
                    {
                        noBB.Lx = Math.Min(noBB.Lx, boundingBox.Lx);
                        noBB.Ly = Math.Min(noBB.Ly, boundingBox.Ly);
                        noBB.Lz = Math.Min(noBB.Lz, boundingBox.Lz);
                        noBB.Ux = Math.Max(noBB.Ux, boundingBox.Ux);
                        noBB.Uy = Math.Max(noBB.Uy, boundingBox.Uy);
                        noBB.Uz = Math.Max(noBB.Uz, boundingBox.Uz);
                        noTBB.Lx = Math.Min(noTBB.Lx, tightBoundingBox.Lx);
                        noTBB.Ly = Math.Min(noTBB.Ly, tightBoundingBox.Ly);
                        noTBB.Lz = Math.Min(noTBB.Lz, tightBoundingBox.Lz);
                        noTBB.Ux = Math.Max(noTBB.Ux, tightBoundingBox.Ux);
                        noTBB.Uy = Math.Max(noTBB.Uy, tightBoundingBox.Uy);
                        noTBB.Uz = Math.Max(noTBB.Uz, tightBoundingBox.Uz);
                    }
                }
                overallBoundingBox = noBB;
                overallTightBoundingBox = noTBB;
            }
        }

        /// <summary>
        /// Adds a root node to the renderer. Should be called by the PC-Controller, which also has to call RegisterController and UpdateBoundingBox.
        /// </summary>
        public void AddRootNode(PointCloudLoader controller, Node node, PointCloudMetaData metaData) {
            initializedEvent.WaitOne();
            lock (pRenderer)
            {
                pRenderer.AddRootNode(node, controller);
            }
        }

        /// <summary>
        /// Removes a point cloud
        /// </summary>
        public void RemoveRootNode(PointCloudLoader controller, Node node) {
            lock (pRenderer)
            {
                pRenderer.RemoveRootNode(node, controller);
            }
            lock (boundingBoxes)
            {
                boundingBoxes.Remove(controller);
                tightBoundingBoxes.Remove(controller);
            }
        }

        /// <summary>
        /// Returns true, iff all the nodes are registered, have been moved to the center (if required) and the renderer is loaded.
        /// </summary>
        protected bool CheckReady() {
            if (!IsInitialized())
            {
                return false;
            }
            lock (boundingBoxes)
            {
                if (!hasMoved)
                {
                    if (boundingBoxes.Count == 0)
                    {
                        //nothing to move along...
                        hasMoved = true;
                        waiterForBoundingBoxUpdate.Set();
                    }
                    else if (!boundingBoxes.ContainsValue(null))
                    {
                        moving = -overallTightBoundingBox.Center();
                        foreach (BoundingBox bb in boundingBoxes.Values)
                        {
                            bb.MoveAlong(moving);
                        }
                        foreach (BoundingBox tbb in tightBoundingBoxes.Values)
                        {
                            tbb.MoveAlong(moving);
                        }
                        overallBoundingBox.MoveAlong(moving);
                        overallTightBoundingBox.MoveAlong(moving);

                        hasMoved = true;
                        waiterForBoundingBoxUpdate.Set();
                    }
                    else
                    {
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
        /// If Bounding-Box-Drawing is enabled this will render the bounding box (Gizmos have to be enabled to see them)
        /// </summary>
        public void DrawDebugInfo()
        {
            if (showBoundingBox)
            {
                Utility.BBDraw.DrawBoundingBox(overallTightBoundingBox, transform, Color.cyan, false);
            }
        }

        /// <summary>
        /// Shuts down the renderer
        /// </summary>
        public void OnDisable()
        {
            StopRendering();
            boundingBoxes.Clear();
            tightBoundingBoxes.Clear();
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
                lock (pRenderer)
                {
                    pRenderer.ShutDown();
                }
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
