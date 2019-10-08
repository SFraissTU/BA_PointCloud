using BAPointCloudRenderer.CloudController;
using BAPointCloudRenderer.Controllers;
using System;
using UnityEngine;

namespace BAPointCloudRenderer.Eval {
    /// <summary>
    /// Used for printing the current FPS contstantly
    /// </summary>
    [Obsolete]
    public class FPSOutputController : MonoBehaviour {
        
        /// <summary>
        /// Time between two outputs (in seconds)
        /// </summary>
        public float outputInterval = 1;
        /// <summary>
        /// Used pointcloudsetcontroller to output the current pointcount. May be null, if no pcsc is used.
        /// </summary>
        public AbstractPointCloudSet pointset = null;

        private float accTime = 0;

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            float dT = Time.deltaTime;
            if ((accTime += dT) > outputInterval) {
                Debug.Log("FPS: " + (1 / dT));
                while (accTime > outputInterval) {
                    accTime -= outputInterval;
                }
                if (pointset != null) {
                    Debug.Log("PointCount: " + pointset.GetPointCount());
                }
            }
        }
    }
}