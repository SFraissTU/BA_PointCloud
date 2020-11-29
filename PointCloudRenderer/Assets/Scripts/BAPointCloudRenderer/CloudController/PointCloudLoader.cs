using BAPointCloudRenderer.CloudData;
using BAPointCloudRenderer.Loading;
using System;
using System.Threading;
using UnityEngine;
using UnityEditor;

namespace BAPointCloudRenderer.CloudController {
    /// <summary>
    /// Use this script to load a single PointCloud from a directory.
    ///
    /// Streaming Assets support provided by Pablo Vidaurre
    /// </summary>
    public class PointCloudLoader : MonoBehaviour {

        /// <summary>
        /// Path to the folder which contains the cloud.js file or URL to download the cloud from. In the latter case, it will be downloaded to a /temp folder
        /// </summary>
        public string cloudPath;

        /// <summary>
        /// When true, the cloudPath is relative to the streaming assets directory
        /// </summary>
        public bool streamingAssetsAsRoot = false;

        /// <summary>
        /// The PointSetController to use
        /// </summary>
        public AbstractPointCloudSet setController;

        /// <summary>
        /// True if the point cloud should be loaded when the behaviour is started. Otherwise the point cloud is loaded when LoadPointCloud is loaded.
        /// </summary>
        public bool loadOnStart = true;

        private Node rootNode;

        private void Awake()
        {
            if (streamingAssetsAsRoot) cloudPath = Application.streamingAssetsPath + "/" + cloudPath;
        }

        void Start() {
            if (loadOnStart) {
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

                rootNode = CloudLoader.LoadHierarchyOnly(metaData);

                setController.AddRootNode(this, rootNode, metaData);
                
            } catch (System.IO.FileNotFoundException ex)
            {
                Debug.LogError("Could not find file: " + ex.FileName);
            } catch (System.IO.DirectoryNotFoundException ex)
            {
                Debug.LogError("Could not find directory: " + ex.Message);
            } catch (System.Net.WebException ex)
            {
                Debug.LogError("Could not access web address. " + ex.Message);
            }
            catch (Exception ex) {
                Debug.LogError(ex + Thread.CurrentThread.Name);
            }
        }

        /// <summary>
        /// Starts loading the point cloud. When the hierarchy is loaded it is registered at the corresponding point cloud set
        /// </summary>
        public void LoadPointCloud() {
            if (rootNode == null && setController != null && cloudPath != null)
            {
                setController.RegisterController(this);
                Thread thread = new Thread(LoadHierarchy);
                thread.Name = "Loader for " + cloudPath;
                thread.Start();
            }
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
            rootNode = null;
            return true;
        }

    }
}
