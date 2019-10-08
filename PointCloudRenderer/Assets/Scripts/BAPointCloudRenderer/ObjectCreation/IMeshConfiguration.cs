using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BAPointCloudRenderer.CloudData;

namespace BAPointCloudRenderer.ObjectCreation
{
    /// <summary>
    /// Defines how the points of a point cloud are rendered (points / quads / spheres etc.).
    /// Even though this is a MonoBehaviour-Script, it does not use the Start- or Update-Function. Having it as an object in your scene graph makes it easier to test different configuartions though.
    /// </summary>
    public interface IMeshConfiguration
    {
        /// <summary>
        /// Returns the maximum number of points a mesh can contain. 4-Vertex Quad Rendering for example only allows 16250, while most others allow 65000.
        /// </summary>
        int GetMaximumPointsPerMesh();

        /// <summary>
        /// Creates a single GameObject for the given data.
        /// </summary>
        /// <param name="name">Name of the GameObject</param>
        /// <param name="vertexData">Vertices (same size as colorData and the length is not higher than GetMaximumPointsPerMesh)</param>
        /// <param name="colorData">Colors (same size as vertexData and the length is not higher than GetMaximumPointsPerMesh)</param>
        /// <param name="boundingBox">Bounding Box</param>
        /// <returns></returns>
        GameObject CreateGameObject(string name, Vector3[] vertexData, Color[] colorData, BoundingBox boundingBox);

        /// <summary>
        /// Removes the GameObject
        /// </summary>
        /// <param name="gameObject">Should be a GameObject created by this MeshConfiguration</param>
        void RemoveGameObject(GameObject gameObject);
    }
}
