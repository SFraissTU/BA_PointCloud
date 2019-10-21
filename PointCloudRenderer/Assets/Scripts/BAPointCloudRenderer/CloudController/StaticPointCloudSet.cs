using BAPointCloudRenderer.Loading;
using BAPointCloudRenderer.ObjectCreation;

namespace BAPointCloudRenderer.CloudController {
    /// <summary>
    /// This Point Cloud Set loads the complete point cloud one and displays everything at once.
    /// Should only be used for small clouds. Might take some time to load.
    /// </summary>
    class StaticPointCloudSet : AbstractPointCloudSet {

        // Use this for initialization
        protected override void Initialize() {
            PointRenderer = new StaticRenderer(this, meshConfiguration);
        }


        // Update is called once per frame
        void Update() {
            if (!CheckReady()) return;
            PointRenderer.Update();
            DrawDebugInfo();
        }
    }
}
