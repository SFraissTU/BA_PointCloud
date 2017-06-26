using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Controllers;

namespace Eval {
    public class FPSLogger : MonoBehaviour {
        
        public string testIdentifier;
        public bool log = false;

        private List<float> deltaTs = new List<float>(115 * 60);

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            deltaTs.Add(Time.deltaTime);
        }

        private void OnApplicationQuit() {
            float sum = 0;
            foreach (float dT in deltaTs) {
                sum += dT;
            }
            float avg = deltaTs.Count / sum;
            Debug.Log(avg);
            float devsum = 0;
            foreach (float dT in deltaTs) {
                devsum += Mathf.Pow(((1/dT) - avg), 2)* dT;
            }
            float dev = Mathf.Sqrt(devsum / sum);
            Debug.Log(dev);
            if (log) {
                System.IO.StreamWriter output = new System.IO.StreamWriter("evaltest_" + testIdentifier + ".txt");
                output.WriteLine("TestRun: " + DateTime.Now.ToString() + " - " + testIdentifier);
                output.WriteLine(deltaTs.Count);
                output.WriteLine();
                output.WriteLine("Avg/Dev");
                output.WriteLine(avg);
                output.WriteLine(dev);
                output.Flush();
                output.Close();
            }
        }
    }
}