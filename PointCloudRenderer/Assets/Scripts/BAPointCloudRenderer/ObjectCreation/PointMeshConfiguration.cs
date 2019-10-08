using BAPointCloudRenderer.CloudData;
using UnityEngine;

namespace BAPointCloudRenderer.ObjectCreation
{
    /// <summary>
    /// Renders every point as a single pixel using the points primitive. As described in the Bachelor Thesis in chapter 3.3.1 "Single-Pixel Point Rendering".
    /// </summary>
    class PointMeshConfiguration : MeshConfiguration
    {
        private InternalPointMeshConfiguration internalConfig;

        public void Start()
        {
            internalConfig = new InternalPointMeshConfiguration();
        }

        public override GameObject CreateGameObject(string name, Vector3[] vertexData, Color[] colorData, BoundingBox boundingBox)
        {
            return internalConfig.CreateGameObject(name, vertexData, colorData, boundingBox);
        }

        public override int GetMaximumPointsPerMesh()
        {
            return internalConfig.GetMaximumPointsPerMesh();
        }

        public override void RemoveGameObject(GameObject gameObject) {
            internalConfig.RemoveGameObject(gameObject);
        }
    }
}