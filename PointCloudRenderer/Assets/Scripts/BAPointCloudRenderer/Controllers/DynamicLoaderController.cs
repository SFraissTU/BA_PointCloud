using System;
using System.Threading;
using UnityEngine;
using BAPointCloudRenderer.CloudData;
using BAPointCloudRenderer.Loading;

namespace BAPointCloudRenderer.Controllers {

    /* While PointCloudLoaderController will load the complete file as one and render the comlete one, 
     * the DynamicLoaderController will first only load the hierarchy. It can be given registered at a PointCloudSetController to render it.
     */
    /// <summary>
    /// Use this script to load a single PointCloud from a directory.
    /// </summary>
    [Obsolete("This class is outdated. Please use PointCloudLoader instead!")]
    public class DynamicLoaderController : MonoBehaviour {
        
        /// <summary>
        /// Path to the folder which contains the cloud.js file
        /// </summary>
        public string cloudPath;

        /// <summary>
        /// The PointSetController to use
        /// </summary>
        public AbstractPointSetController setController;

        void Start() {
            setController.RegisterController(this);
            Thread thread = new Thread(LoadHierarchy);
            thread.Start();
        }
        
        private void LoadHierarchy() {
            try {
                if (!cloudPath.EndsWith("\\")) {
                    cloudPath = cloudPath + "\\";
                }

                PointCloudMetaData metaData = CloudLoader.LoadMetaData(cloudPath, false);

                setController.UpdateBoundingBox(this, metaData.boundingBox);

                Node rootNode = CloudLoader.LoadHierarchyOnly(metaData);

                setController.AddRootNode(rootNode);
            } catch (Exception ex) {
                Debug.LogError(ex);
            }
        }
        
    }

}