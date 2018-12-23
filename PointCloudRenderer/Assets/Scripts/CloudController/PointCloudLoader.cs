using CloudData;
using Loading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace CloudController {
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
        /// True if the point cloud should be loaded when the behaviour is started
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

        public void LoadPointCloud() {
            setController.RegisterController(this);
            Thread thread = new Thread(LoadHierarchy);
            thread.Start();
        }

        //Has to be called from the main thread!
        public bool RemovePointCloud() {
            if (rootNode == null) {
                return false;
            }
            setController.RemoveRootNode(this, rootNode);
            return true;
        }

    }
}
