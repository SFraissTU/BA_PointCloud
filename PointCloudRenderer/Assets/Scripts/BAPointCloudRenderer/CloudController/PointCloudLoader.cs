using BAPointCloudRenderer.CloudData;
using BAPointCloudRenderer.Loading;
using System;
using System.Threading;
using UnityEngine;

namespace BAPointCloudRenderer.CloudController {
    /* While PointCloudLoaderController will load the complete file as one and render the comlete one, 
     * the DynamicLoaderController will first only load the hierarchy. It can be given registered at a PointCloudSetController to render it.
     */
    /// <summary>
    /// Use this script to load a single PointCloud from a directory.
    /// </summary>
    public class PointCloudLoader : MonoBehaviour {

        /// <summary>
        /// Path to the folder which contains the cloud.js file
        /// </summary>
        public string cloudPath;

        /// <summary>
        /// The PointSetController to use
        /// </summary>
        public AbstractPointCloudSet setController;

        /// <summary>
        /// True if the point cloud should be loaded when the behaviour is started. Otherwise the point cloud is loaded when LoadPointCloud is loaded.
        /// </summary>
        public bool loadOnStart = true;

        private Node rootNode;

        void Start() {
            if (loadOnStart) {
                LoadPointCloud();
            }
        }

        private void LoadHierarchy() {
            try {
                if (!cloudPath.EndsWith("\\")) {
                    cloudPath = cloudPath + "\\";
                }

                PointCloudMetaData metaData = CloudLoader.LoadMetaData(cloudPath, false);

                setController.UpdateBoundingBox(this, metaData.boundingBox);

                rootNode = CloudLoader.LoadHierarchyOnly(metaData);

                setController.AddRootNode(rootNode);
            } catch (Exception ex) {
                Debug.LogError(ex);
            }
        }

        /// <summary>
        /// Starts loading the point cloud. When the hierarchy is loaded it is registered at the corresponding point cloud set
        /// </summary>
        public void LoadPointCloud() {
            setController.RegisterController(this);
            Thread thread = new Thread(LoadHierarchy);
            thread.Start();
        }

        /// <summary>
        /// Removes the point cloud from the scene. Should only be called from the main thread!
        /// </summary>
        /// <returns>True if the cloud was removed. False, when the cloud hasn't even been loaded yet.</returns>
        public bool RemovePointCloud() {
            if (rootNode == null) {
                return false;
            }
            setController.RemoveRootNode(this, rootNode);
            return true;
        }

    }
}
