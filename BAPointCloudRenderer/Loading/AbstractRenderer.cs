using BAPointCloudRenderer.CloudController;
using BAPointCloudRenderer.CloudData;

namespace BAPointCloudRenderer.Loading {
    /// <summary>
     /// The job of an AbstractRenderer is to create GameObjects from PointCloud-Nodes and update these each frame.
     /// There used to be several implementations for experimenting purposes, the only implementation right now is the V2Renderer.
     /// </summary>
    public interface AbstractRenderer {
        
         /// <summary>
         /// Registers the root node of a point cloud in the renderer.
         /// </summary>
         /// <param name="rootNode">not null</param>
        void AddRootNode(Node rootNode, PointCloudLoader loader);

        /// <summary>
        /// Removes the root node of a point cloud from the renderer. The node will not be rendered any more.
        /// </summary>
        /// <param name="rootNode">not null</param>
        void RemoveRootNode(Node rootNode, PointCloudLoader loader);
        
        /// <summary>
        /// Returns how many root nodes have been added
        /// </summary>
        int GetRootNodeCount();
        
         /// <summary>
         /// Stops the rendering process and all concurrent threads get scheduled to stop. 
         /// Also removes all cloud objects created by this renderer from the scene.
         /// </summary>
        void ShutDown();
        
        /// <summary>
        /// Pauses the rendering and hides all visible point clouds.
        /// </summary>
        void Hide();

        /// <summary>
        /// Continues the rendering and displays all visible point clouds after them being hidden via hide.
        /// </summary>
        void Display();
        
         /// <summary>
         /// Returns the current PointCount, so how many points are loaded / visible
         /// </summary>
        uint GetPointCount();

        /// <summary>
        /// Has to be called each frame
        /// </summary>
        void Update();
    }
}
