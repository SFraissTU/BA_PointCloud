using BAPointCloudRenderer.CloudData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace BAPointCloudRenderer.ObjectCreation
{
    //This class is used internally, where a MeshConfiguration is needed that is not a MonoBehaviour!
    //Particularly for Editor-Preview
    class InternalPointMeshConfiguration : IMeshConfiguration
    {
        private Material material;

        public InternalPointMeshConfiguration()
        {
            material = new Material(Shader.Find("Custom/PointShader"));
        }

        public GameObject CreateGameObject(string name, Vector3[] vertexData, Color[] colorData, BoundingBox boundingBox)
        {
            GameObject gameObject = new GameObject(name);

            Mesh mesh = new Mesh();

            MeshFilter filter = gameObject.AddComponent<MeshFilter>();
            filter.mesh = mesh;
            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.material = material;

            int[] indecies = new int[vertexData.Length];
            for (int i = 0; i < vertexData.Length; ++i)
            {
                indecies[i] = i;
            }
            mesh.vertices = vertexData;
            mesh.colors = colorData;
            mesh.SetIndices(indecies, MeshTopology.Points, 0);

            //Set Translation
            gameObject.transform.Translate(boundingBox.Min().ToFloatVector());

            return gameObject;
        }

        public int GetMaximumPointsPerMesh()
        {
            return 65000;
        }

        public void RemoveGameObject(GameObject gameObject)
        {
            if (EditorApplication.isPlaying)
            {
                UnityEngine.Object.Destroy(gameObject.GetComponent<MeshFilter>().sharedMesh);
                UnityEngine.Object.Destroy(gameObject);
            } else
            {
                EditorApplication.delayCall += () =>
                {
                    Debug.Log("Destroying " + gameObject.name);
                    UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<MeshFilter>().sharedMesh);
                    UnityEngine.Object.DestroyImmediate(gameObject);
                };
            }
        }
    }
}
