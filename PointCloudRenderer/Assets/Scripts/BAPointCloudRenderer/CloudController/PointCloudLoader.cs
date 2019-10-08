using BAPointCloudRenderer.CloudData;
using BAPointCloudRenderer.Loading;
using System;
using System.Threading;
using UnityEngine;
using UnityEditor;

namespace BAPointCloudRenderer.CloudController {
    /* While PointCloudLoaderController will load the complete file as one and render the comlete one, 
     * the DynamicLoaderController will first only load the hierarchy. It can be given registered at a PointCloudSetController to render it.
     */
    /// <summary>
    /// Use this script to load a single PointCloud from a directory.
    /// </summary>
    [ExecuteInEditMode]
    public class PointCloudLoader : MonoBehaviour {

        /// <summary>
        /// Path to the folder which contains the cloud.js file
        /// </summary>
        public string cloudPath;

        private string lastCloudPath;

        /// <summary>
        /// The PointSetController to use
        /// </summary>
        public AbstractPointCloudSet setController;

        private AbstractPointCloudSet lastSetController;

        /// <summary>
        /// True if the point cloud should be loaded when the behaviour is started. Otherwise the point cloud is loaded when LoadPointCloud is loaded.
        /// </summary>
        public bool loadOnStart = true;

        public bool pointPreview = false;

        private bool active = false;

        private bool editmode = false;

        private Node rootNode;

        void Start() {
            editmode = !EditorApplication.isPlaying;
            if (!editmode)
            {
                MeshFilter mf = GetComponent<MeshFilter>();
                if (mf != null) Destroy(mf);
                MeshRenderer mr = GetComponent<MeshRenderer>();
                if (mr != null) Destroy(mr);
            }
            if (!active && (loadOnStart || editmode)) {
                LoadPointCloud();
            }
        }

        private void OnDisable()
        {
            RemovePointCloud();
        }

        private void Update()
        {
            if (active)
            {
                if (cloudPath != lastCloudPath || setController != lastSetController)
                {
                    RemovePointCloud();
                }
            }
            lastCloudPath = cloudPath;
            lastSetController = setController;
            if (!active && editmode)
            {
                LoadPointCloud();
            }
        }

        private void LoadHierarchy() {
            try {
                if (!cloudPath.EndsWith("/")) {
                    cloudPath = cloudPath + "/";
                }

                PointCloudMetaData metaData = CloudLoader.LoadMetaData(cloudPath, false);
                
                setController.UpdateBoundingBox(this, metaData.boundingBox, metaData.tightBoundingBox);

                if (editmode)
                {
                    rootNode = new Node("", metaData, metaData.boundingBox, null);
                }
                else
                {
                    rootNode = CloudLoader.LoadHierarchyOnly(metaData);
                }

                setController.AddRootNode(this, rootNode, metaData);
                
            } catch (System.IO.FileNotFoundException ex)
            {
                Debug.LogError("Could not find file: " + ex.FileName);
                active = false;
            } catch (System.IO.DirectoryNotFoundException ex)
            {
                Debug.LogError("Could not find directory: " + ex.Message);
                active = false;
            } catch (Exception ex) {
                Debug.LogError(ex);
                active = false;
            }
        }

        /// <summary>
        /// Starts loading the point cloud. When the hierarchy is loaded it is registered at the corresponding point cloud set
        /// </summary>
        public void LoadPointCloud() {
            if (setController != null && cloudPath != null)
            {
                setController.RegisterController(this);
                active = true;
                Thread thread = new Thread(LoadHierarchy);
                thread.Start();
            }
        }

        /// <summary>
        /// Removes the point cloud from the scene. Should only be called from the main thread!
        /// </summary>
        /// <returns>True if the cloud was removed. False, when the cloud hasn't even been loaded yet.</returns>
        public bool RemovePointCloud() {
            /*if (rootNode == null) {
                return false;
            }*/
            setController.RemoveRootNode(this, rootNode);
            active = false;
            return true;
        }

        public bool HasPointCloudLoaded()
        {
            return active;
        }

    }
}
