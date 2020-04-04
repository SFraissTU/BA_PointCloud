using System.IO;
using UnityEngine;

namespace BAPointCloudRenderer.CloudController {
    /// <summary>
    /// Use this loader, if you have several pointcloud-folders in the same directory and want to import all of them to your program.
    /// To  import them, create a DirectoryLoader and press the "Load Directory" Button in the Editor (or call the function LoadAll).
    /// 
    /// Streaming Assets support provided by Pablo Vidaurre
    /// </summary>
    [ExecuteInEditMode]
    public class DirectoryLoader : MonoBehaviour {

        /// <summary>
        /// Path of the directory containing the point clouds
        /// </summary>
        public string path;
        /// <summary>
        /// When true, the cloudPath is relative to the streaming assets directory
        /// </summary>
        public bool streamingAssetsAsRoot = false;

        /// <summary>
        /// The PointSetController
        /// </summary>
        public AbstractPointCloudSet pointset;

        //may include streaming assets path
        private string fullPath;

        /// <summary>
        /// Creates PointCloudLoader objects for all the point clouds in the given path.
        /// </summary>
        public void LoadAll() {
            if (streamingAssetsAsRoot) fullPath = Application.streamingAssetsPath + "/" + path;
            else { fullPath = path; }

            DirectoryInfo dir = new DirectoryInfo(fullPath);
            foreach (DirectoryInfo sub in dir.GetDirectories()) {
                GameObject go = new GameObject(sub.Name);
                PointCloudLoader loader = go.AddComponent<PointCloudLoader>();
                if (streamingAssetsAsRoot)
                {
                    loader.streamingAssetsAsRoot = true;
                    loader.cloudPath = path + sub.Name;
                }
                else
                {
                    loader.cloudPath = sub.FullName;
                }

                loader.setController = pointset;
            }
        }
    }
}
