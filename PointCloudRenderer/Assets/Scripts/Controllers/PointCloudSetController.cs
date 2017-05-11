using CloudData;
using Loading;
using ObjectCreation;
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
        //Defines the type of PointCloud (Points, Quads, Circles)
        public MeshConfiguration meshConfiguration;

        //For origin-moving:
        private bool hasMovedToOrigin = false;
        private BoundingBox overallBoundingBox = new BoundingBox(double.PositiveInfinity, double.PositiveInfinity,double.PositiveInfinity,
                                                                    double.NegativeInfinity,double.NegativeInfinity,double.NegativeInfinity);
        private Dictionary<MonoBehaviour, BoundingBox> boundingBoxes = new Dictionary<MonoBehaviour, BoundingBox>();
        private ManualResetEvent waiterForBoundingBoxUpdate = new ManualResetEvent(false);

        private ConcurrentRenderer pRenderer;

        private Camera userCamera;

        // Use this for initialization
        void Start() {
            pRenderer = new ConcurrentRenderer(minNodeSize, pointBudget);
            userCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
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
            if (!pRenderer.IsLoadingPoints() && Input.GetKey(KeyCode.X) && !pRenderer.HasNodesToRender() && !pRenderer.HasNodesToDelete()) {
                Debug.Log("Updating!");
                float screenHeight = userCamera.pixelRect.height;
                Vector3 cameraPositionF = userCamera.transform.position;
                float fieldOfView = userCamera.fieldOfView;
                Plane[] frustum = GeometryUtility.CalculateFrustumPlanes(userCamera);
                pRenderer.SetCameraInfo(screenHeight, fieldOfView, cameraPositionF, frustum, userCamera.worldToCameraMatrix * userCamera.projectionMatrix);
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