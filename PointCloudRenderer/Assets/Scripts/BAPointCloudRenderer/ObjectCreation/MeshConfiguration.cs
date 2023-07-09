using BAPointCloudRenderer.CloudData;
using UnityEngine;

namespace BAPointCloudRenderer.ObjectCreation
{
    /// <summary>
    /// Defines how the points of a point cloud are rendered (points / quads / spheres etc.).
    /// Even though this is a MonoBehaviour-Script, it does not use the Start- or Update-Function. Having it as an object in your scene graph makes it easier to test different configuartions though.
    /// </summary>
    public abstract class MeshConfiguration : MonoBehaviour
    {
        /// <summary>
        /// Returns the maximum number of points a mesh can contain. 4-Vertex Quad Rendering for example only allows 16250, while most others allow 65000.
        /// </summary>
        public abstract int GetMaximumPointsPerMesh();

        /// <summary>
        /// Creates a single GameObject for the given data.
        /// </summary>
        /// <param name="name">Name of the GameObject</param>
        /// <param name="vertexData">Vertices (same size as colorData and the length is not higher than GetMaximumPointsPerMesh)</param>
        /// <param name="colorData">Colors (same size as vertexData and the length is not higher than GetMaximumPointsPerMesh)</param>
        /// <param name="boundingBox">Bounding Box</param>
        /// <returns></returns>
        public abstract GameObject CreateGameObject(string name, Vector3[] vertexData, Color[] colorData, BoundingBox boundingBox, Transform parent, string version, Vector3d translationV2);

        /// <summary>
        /// Removes the GameObject
        /// </summary>
        /// <param name="gameObject">Should be a GameObject created by this MeshConfiguration</param>
        public abstract void RemoveGameObject(GameObject gameObject);

        protected class BoundingBoxComponent : MonoBehaviour
        {
            public BoundingBox boundingBox;
            public Transform parent;
        }
    }

}