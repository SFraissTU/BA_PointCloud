using System;
using System.Threading;
using UnityEngine;
using ObjectCreation;
using CloudData;
using Loading;

namespace Controllers {
    
     /// <summary>
     /// Loads and renders a PointCloud from a folder completely at once. For each node a GameObject is created.
     /// </summary>
    public class PointCloudLoaderController : MonoBehaviour {
        
        /// <summary>
        /// Path to the folder which contains the cloud.js file
        /// </summary>
        public string cloudPath;
        /// <summary>
        /// How to render the PointCloud
        /// </summary>
        public MeshConfiguration meshConfiguration;
        /// <summary>
        /// If the center of the cloud should be moved to the origin
        /// </summary>
        public bool moveToOrigin;

        private PointCloudMetaData metaData;
        private Node rootNode;
        private bool fileLoading = false;

        // Use this for initialization
        void Start() {
            Thread thread = new Thread(new ThreadStart(LoadFile));
            thread.Start();
        }

        //Loads the complete point cloud
        private void LoadFile() {
            try {
                Debug.Log("Loading file");
                fileLoading = true;
                if (!cloudPath.EndsWith("\\")) {
                    cloudPath = cloudPath + "\\";
                }

                metaData = CloudLoader.LoadMetaData(cloudPath, moveToOrigin);

                rootNode = CloudLoader.LoadPointCloud(metaData);

                Debug.Log("Finished Loading");

                fileLoading = false;
            } catch (Exception ex) {
                Debug.LogError(ex);
            }
        }

        // Update is called once per frame
        void Update() {
            if (!fileLoading && rootNode != null) {
                rootNode.CreateAllGameObjects(meshConfiguration);
                rootNode = null;
                Debug.Log("Created GameObject");
            }
        }

        /*
         * Stops the loading of the file if the application is closed
         */
        private void OnApplicationQuit() {
            fileLoading = false;
        }
    }

}
