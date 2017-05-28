using CloudData;
using ObjectCreation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Loading {
    /* The job of Renderers is to create GameObjects from PointCloud-Nodes. Therefore the renderer has to check, which nodes should be visible (Hierarchy Traversal), load these and create GameObjects with help of the MeshConfiguration-Class.
     * There are OneTime- and MultiTime-Renderers. OneTime-Renderers check which nodes should be visible, then loads them and creates the GameObjects. Only when that is done, visibility can be checked again.
     * MultiTime-Renderers however can check the visibility anytime and adjust the nodes to load also during loading.
     * There are also SingleThreaded- and Concurrent-Renderers. ConcurrentRenderers use several threads.
     */
    public interface AbstractRenderer {

        /* Registers the root node of a pointcloud in the renderer, so it will be considered in future visibility checks and GameObject creations.
         * The given rootNode may not be null! */
        void AddRootNode(Node rootNode);

        /* Returns how man root nodes have been added */
        int GetRootNodeCount();

        /* Returns weither a call of UpdateVisibleNodes is allowed right now. This is mainly important for the OneTimeRenderers. */
        bool IsReadyForUpdate();
        
        /* This method checks which nodes of the PointCloud are visible and adjusts the rendering queue(s) accordingly.
         * Should be called in the main thread, because Unity-operations are used, which are only allowed there.
         * config is the MeshConfiguration used for GameObject-Creation (null is not allowed). This is needed because GameObjects might be deleted. */
        void UpdateVisibleNodes(MeshConfiguration config);

        /* Should be called every frame in the main thread, because GameObject-Creation happens here.
         * This method takes nodes which have been scheduled for GameObject-Creation in UpdateVisibleNodes and creates GameObjects for them.
         * meshConfiguration is the MeshConfiguration used for GameObject-Creation (null is not allowed). */
        void UpdateGameObjects(MeshConfiguration meshConfiguration);

        /* This methods stops and disables the renderer. Method calls should not leed to any GameObject-modifications anymore. Concurrent threads are scheduled to stop.
         * Consistency is not guaranteed after the call of this method. This should only be called at the end of the program to stop concurrent threads. */
        void ShutDown();

        /* This method returns the current pointcount, so how many points should be visible (including points that are not yet visible, but are loaded and scheduled for GO-creation, not including points that are visible, but are scheduled for GO-destruction).
         */
        uint GetPointCount();
    }
}
