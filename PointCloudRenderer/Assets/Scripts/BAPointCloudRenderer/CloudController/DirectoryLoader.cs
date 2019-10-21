using System.IO;
using UnityEngine;

namespace BAPointCloudRenderer.CloudController {
    /// <summary>
    /// Use this loader, if you have several pointcloud-folders in the same directory and want to import all of them to your program.
    /// To  import them, create a DirectoryLoader and press the "Load Directory" Button in the Editor (or call the function LoadAll).
    /// </summary>
    [ExecuteInEditMode]
    public class DirectoryLoader : MonoBehaviour {

        /// <summary>
        /// Path of the directory containing the point clouds
        /// </summary>
        public string path;
        /// <summary>
        /// The PointSetController
        /// </summary>
        public AbstractPointCloudSet pointset;

        /// <summary>
        /// Creates PointCloudLoader objects for all the point clouds in the given path.
        /// </summary>
        public void LoadAll() {
            DirectoryInfo dir = new DirectoryInfo(path);
            foreach (DirectoryInfo sub in dir.GetDirectories()) {
                GameObject go = new GameObject(sub.Name);
                PointCloudLoader loader = go.AddComponent<PointCloudLoader>();
                loader.cloudPath = sub.FullName;
                loader.setController = pointset;
            }
        }
    }
}
