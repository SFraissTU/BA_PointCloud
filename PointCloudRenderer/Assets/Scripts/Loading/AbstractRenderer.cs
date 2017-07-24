using CloudData;
using ObjectCreation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Loading {
    /// <summary>
     /// The job of an AbstractRenderer is to create GameObjects from PointCloud-Nodes and update these each frame.
     /// There used to be several implementations for experimenting purposes, the only implementation right now is the V2Renderer.
     /// </summary>
    public interface AbstractRenderer {
        
         /// <summary>
         /// Registers the root node of a point cloud in the renderer.
         /// </summary>
         /// <param name="rootNode">not null</param>
        void AddRootNode(Node rootNode);
        
        /// <summary>
        /// Returns how many root nodes have been added
        /// </summary>
        int GetRootNodeCount();
        
         /// <summary>
         /// Stops the rendering process and all concurrent threads get scheduled to stop.
         /// </summary>
        void ShutDown();
        
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
