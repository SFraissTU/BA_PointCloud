using UnityEngine;
using BAPointCloudRenderer.CloudData;
using BAPointCloudRenderer.ObjectCreation;

namespace BAPointCloudRenderer.Eval {
    /// <summary>
    /// Creates a simple Debug-Point-Cloud to test the MeshConfigurations. Creates a circle of 12 points near the origin.
    /// </summary>
    public class DebugCloud : MonoBehaviour {

        public MeshConfiguration configuration;

        // Use this for initialization
        void Start() {
            PointCloudMetaData meta = new PointCloudMetaDataV1_8();
            meta.cloudName = "";
            Node n = new Node("", meta, new BoundingBox(0,0, 0,10, 10,10), null);
            Vector3[] vecs = new Vector3[12];
            Color[] cols = new Color[12];
            for (int i = 0; i < 12; i++) {
                vecs[i] = new Vector3(Mathf.Sin(i * Mathf.PI / 6), 0, Mathf.Cos(i * Mathf.PI / 6));
                cols[i] = new Color(i / 12.0f, 1 - i / 12.0f, 1);
            }
            n.SetPoints(vecs, cols);
            n.CreateGameObjects(configuration, this.transform);
        }

        // Update is called once per frame
        void Update() {

        }
    }

}