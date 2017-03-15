using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/*
 * Defines how the PointCloud is rendered (points / quads / spheres etc.)
 */
public abstract class MeshConfiguration : MonoBehaviour
{
    public abstract int GetMaximumPointsPerMesh();
    //Condition: Length of vertexData and colorData are the same and not more than MaximumPointsPerMesh
    public abstract GameObject CreateGameObject(string name, Vector3[] vertexData, Color[] colorData);
}
