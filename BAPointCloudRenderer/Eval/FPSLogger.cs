#pragma warning disable CS1692
#pragma warning disable CS0618

using System;
using System.Collections.Generic;
using UnityEngine;
using BAPointCloudRenderer.Controllers;
using BAPointCloudRenderer.CloudController;

namespace BAPointCloudRenderer.Eval {
    /// <summary>
    /// Used for logging the Frames per Second and the Updates per Second
    /// </summary>
    public class FPSLogger : MonoBehaviour {
        
        public string testIdentifier;
        public bool log = false;

        public DynamicPointCloudSet controller;

        private List<float> deltaTs = new List<float>(115 * 60);
        private static List<float> updateDTs = new List<float>(115 * 60);

        // Use this for initialization
        void Start() {
        }

        // Update is called once per frame
        void Update() {
            deltaTs.Add(Time.deltaTime);
        }

        public static void NextUpdateFrame(float updated) {
            updateDTs.Add(updated);
        }

        private void OnApplicationQuit() {
            float sum = 0;
            float mindT = float.PositiveInfinity;
            float maxdT = float.NegativeInfinity;
            foreach (float dT in deltaTs) {
                sum += dT;
                if (dT < mindT) {
                    mindT = dT;
                }
                if (dT > maxdT) {
                    maxdT = dT;
                }
            }
            float avgfps = deltaTs.Count / sum;
            float devsum = 0;
            foreach (float dT in deltaTs) {
                devsum += Mathf.Pow(((1/dT) - avgfps), 2)* dT;
            }
            float devfps = Mathf.Sqrt(devsum / sum);
            float minfps = 1 / maxdT;
            float maxfps = 1 / mindT;
            Debug.Log("AvgFPS: " + avgfps);
            Debug.Log("DevFPS: " + devfps);
            Debug.Log("MinFPS: " + minfps);
            Debug.Log("MaxFPS: " + maxfps);

            //sum = 0;
            //float minUdT = float.PositiveInfinity;
            //float maxUdT = float.NegativeInfinity;
            //foreach (float dT in updateDTs) {
            //    sum += dT;
            //    if (dT < minUdT) {
            //        minUdT = dT;
            //    }
            //    if (dT > maxUdT) {
            //        maxUdT = dT;
            //    }
            //}
            //float avgups = updateDTs.Count / sum;
            //devsum = 0;
            //foreach (float dT in updateDTs) {
            //    devsum += Mathf.Pow(((1 / dT) - avgups), 2) * dT;
            //}
            //float devups = Mathf.Sqrt(devsum / sum);
            //float minups = 1 / maxUdT;
            //float maxups = 1 / minUdT;
            //Debug.Log("AvgUPS: " + avgups);
            //Debug.Log("DevUPS: " + devups);
            //Debug.Log("MinUPS: " + minups);
            //Debug.Log("MaxUPS: " + maxups);

            
            if (log) {
                System.IO.StreamWriter output = new System.IO.StreamWriter("evalogfiles/" + testIdentifier + ".txt");
                output.WriteLine("TestRun: " + DateTime.Now.ToString() + " - " + testIdentifier);
                output.WriteLine("Point Budget: " + controller.pointBudget);
                output.WriteLine("Min Node Size: " + controller.minNodeSize);
                output.WriteLine("Nodes Loaded Per Frame: " + controller.nodesLoadedPerFrame);
                output.WriteLine("Nodes GOs per Frame: " + controller.nodesGOsPerFrame);
                output.WriteLine("Mesh Configuration: " + controller.meshConfiguration.GetType().Name);
                if (controller.meshConfiguration.GetType() == typeof(ObjectCreation.GeoQuadMeshConfiguration)) {
                    ObjectCreation.GeoQuadMeshConfiguration config = (ObjectCreation.GeoQuadMeshConfiguration)controller.meshConfiguration;
                    output.WriteLine("  Point Radius: " + config.pointRadius);
                    output.WriteLine("  Circles: " + config.renderCircles);
                    output.WriteLine("  Screen Size: " + config.screenSize);
                    output.WriteLine("  Paraboloid: " + config.interpolation);
                }
                output.WriteLine();
                output.WriteLine(deltaTs.Count);
                output.WriteLine();
                output.WriteLine("AvgFPS: " + avgfps);
                output.WriteLine("DevFPS: " + devfps);
                output.WriteLine("MinFPS: " + minfps);
                output.WriteLine("MaxFPS: " + maxfps);
                //output.WriteLine("AvgUPS: " + avgups);
                //output.WriteLine("DevUPS: " + devups);
                //output.WriteLine("MinUPS: " + minups);
                //output.WriteLine("MaxUPS: " + maxups);
                output.Flush();
                output.Close();
            }
        }
    }
}