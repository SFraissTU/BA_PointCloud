using System.Collections.Generic;
using BAPointCloudRenderer.CloudController;
using BAPointCloudRenderer.CloudData;
using UnityEngine;

namespace BAPointCloudRenderer.ObjectCreation {
    public class ComputeBufferPointMeshConfiguration : MeshConfiguration {
        public Shader shader;
        private HashSet<GameObject> _gameObjectCollection = null;

        public void Start () {
            _gameObjectCollection = new HashSet<GameObject> ();
        }

        public override GameObject CreateGameObject (string name, Vector3[] vertexData, Color[] colorData, BoundingBox boundingBox, Transform parent, string version, Vector3d translationV2) {
            GameObject gameObject = new GameObject (name);

            //Set Translation
            if (version == "2.0") {
                // 20230125: potree v2 vertices have absolute coordinates,
                // hence all gameobjects need to reside at Vector.Zero.
                // And: the position must be set after parenthood has been granted.
                //gameObject.transform.Translate(boundingBox.Min().ToFloatVector());
                gameObject.transform.SetParent (parent, false);
                gameObject.transform.localPosition = translationV2.ToFloatVector ();
            } else {
                gameObject.transform.Translate (boundingBox.Min ().ToFloatVector ());
                gameObject.transform.SetParent (parent, false);
            }
            
            ComputeBufferPointMesh computeBufferPointMesh = gameObject.AddComponent<ComputeBufferPointMesh> ();
            computeBufferPointMesh.Init (shader, vertexData, colorData);

            gameObject.AddComponent<BoundingBoxComponent> ().boundingBox = boundingBox;

            if (_gameObjectCollection != null) {
                _gameObjectCollection.Add (gameObject);
            }

            return gameObject;
        }

        public override void RemoveGameObject (GameObject gameObject) {
            if (_gameObjectCollection != null) {
                _gameObjectCollection.Remove (gameObject);
            }
            if (gameObject != null) {
                Destroy (gameObject);
            }
        }

        public override int GetMaximumPointsPerMesh () {
            return 65000;
        }
    }
}