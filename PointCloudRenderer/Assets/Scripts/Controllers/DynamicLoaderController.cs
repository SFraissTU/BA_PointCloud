using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using DataStructures;
using ObjectCreation;
using CloudData;
using Loading;

namespace Controllers {

    /* While PointCloudLoaderController will load the complete file as one, the DynamicLoaderController will first just load the hierarchy and load only the important nodes when pressing a key
     */
    public class DynamicLoaderController : MonoBehaviour {

        //-----Public Options-----
        //Path to the folder in which the cloud.js is
        public string cloudPath;
        //Defines the type of PointCloud (Points, Quads, Circles)
        public MeshConfiguration meshConfiguration;

        public PointCloudSetController setController;

        private ConcurrentRenderer pRenderer;
        private Camera userCamera;
        private bool hierarchyLoaded = false;


        // Use this for initialization
        void Start() {
            setController.RegisterController(this);
            Thread thread = new Thread(LoadHierarchy);
            thread.Start();
            userCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        }

        void LoadHierarchy() {
            try {
                Debug.Log("Loading Hierarchy");
                if (!cloudPath.EndsWith("\\")) {
                    cloudPath = cloudPath + "\\";
                }

                PointCloudMetaData metaData = CloudLoader.LoadMetaData(cloudPath, false);

                setController.UpdateBoundingBox(this, metaData.boundingBox);

                Node rootNode = CloudLoader.LoadHierarchyOnly(cloudPath, metaData);

                pRenderer = new ConcurrentRenderer(rootNode, metaData, cloudPath, setController.minNodeSize, setController.pointBudget);

                Debug.Log("Finished Loading Hierachy");
            } catch (Exception ex) {
                Debug.LogError(ex);
            }
        }


        // Update is called once per frame
        void Update() {
            if (pRenderer != null) {
                if (!pRenderer.IsLoadingPoints() && Input.GetKey(KeyCode.X) && !pRenderer.HasNodesToRender() && !pRenderer.HasNodesToDelete()) {
                    float screenHeight = userCamera.pixelRect.height;
                    Vector3 cameraPositionF = userCamera.transform.position;
                    float fieldOfView = userCamera.fieldOfView;
                    Plane[] frustum = GeometryUtility.CalculateFrustumPlanes(userCamera);
                    pRenderer.SetCameraInfo(screenHeight, fieldOfView, cameraPositionF, frustum);
                    pRenderer.UpdateRenderingQueue();
                    pRenderer.StartUpdatingPoints();
                } else {
                    pRenderer.UpdateGameObjects(meshConfiguration);
                }
            }
        }

        public void OnApplicationQuit() {
            pRenderer.ShutDown();
        }
    }

}