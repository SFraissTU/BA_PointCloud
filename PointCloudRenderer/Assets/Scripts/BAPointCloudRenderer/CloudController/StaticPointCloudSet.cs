using BAPointCloudRenderer.Loading;
using BAPointCloudRenderer.ObjectCreation;

namespace BAPointCloudRenderer.CloudController {
    /// <summary>
    /// This Point Cloud Set loads the complete point cloud one and displays everything at once.
    /// Should only be used for small clouds. Might take some time to load.
    /// </summary>
    class StaticPointCloudSet : AbstractPointCloudSet {
        
        /// <summary>
        /// MeshConfiguration that specifies how the cloud is to be displayed
        /// </summary>
        public MeshConfiguration meshConfiguration = null;

        // Use this for initialization
        protected override void Initialize() {
            PointRenderer = new StaticRenderer(meshConfiguration);
        }


        // Update is called once per frame
        void Update() {
            if (!CheckReady()) return;
            PointRenderer.Update();
        }
    }
}
