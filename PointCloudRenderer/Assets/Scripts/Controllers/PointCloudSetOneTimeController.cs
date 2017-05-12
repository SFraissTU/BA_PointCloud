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
    public class PointCloudSetOneTimeController : MonoBehaviour {

        public uint pointBudget;
        public int minNodeSize;
        public bool moveToOrigin;
        //Defines the type of PointCloud (Points, Quads, Circles)
        public MeshConfiguration meshConfiguration;

        //For origin-moving:
        private bool hasMovedToOrigin = false;
        private BoundingBox overallBoundingBox = new BoundingBox(double.PositiveInfinity, double.PositiveInfinity,double.PositiveInfinity,
                                                                    double.NegativeInfinity,double.NegativeInfinity,double.NegativeInfinity);
        private Dictionary<MonoBehaviour, BoundingBox> boundingBoxes = new Dictionary<MonoBehaviour, BoundingBox>();
        private ManualResetEvent waiterForBoundingBoxUpdate = new ManualResetEvent(false);

        private ConcurrentOneTimeRenderer pRenderer;

        private Camera userCamera;

        // Use this for initialization
        void Start() {
            userCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
            pRenderer = new ConcurrentOneTimeRenderer(minNodeSize, pointBudget, userCamera);
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

        // Update is called once per frame
        void Update() {
            lock (boundingBoxes) {
                if (moveToOrigin && !hasMovedToOrigin && !boundingBoxes.ContainsValue(null)) {
                    Debug.Log(overallBoundingBox);
                    Vector3d moving = overallBoundingBox.DistanceToOrigin();
                    foreach (BoundingBox bb in boundingBoxes.Values) {
                        bb.MoveAlong(moving);
                    }
                    overallBoundingBox.MoveAlong(moving);
                    hasMovedToOrigin = true;
                    waiterForBoundingBoxUpdate.Set();
                }
            }
            if (!pRenderer.IsLoadingPoints() && Input.GetKey(KeyCode.X) && !pRenderer.HasNodesToRender() && !pRenderer.HasNodesToDelete()) {
                Debug.Log("Updating!");
                pRenderer.UpdateRenderingQueue(meshConfiguration);
                pRenderer.StartUpdatingPoints();
            } else {
                pRenderer.UpdateGameObjects(meshConfiguration);
            }
        }

        public void AddRootNode(Node node) {
            pRenderer.AddRootNode(node);
        }
        
        public void OnApplicationQuit() {
            pRenderer.ShutDown();
        }
    }
}