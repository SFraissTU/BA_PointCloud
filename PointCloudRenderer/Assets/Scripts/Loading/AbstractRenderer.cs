using CloudData;
using ObjectCreation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Loading {
    public interface AbstractRenderer {
        void AddRootNode(Node rootNode);
        int GetRootNodeCount();
        bool IsLoadingPoints();
        void UpdateRenderingQueue(MeshConfiguration config);
        void StartUpdatingPoints();
        void UpdateGameObjects(MeshConfiguration meshConfiguration);
        void ShutDown();
        bool HasNodesToRender();
        bool HasNodesToDelete();
        uint GetPointCount();
    }
}
