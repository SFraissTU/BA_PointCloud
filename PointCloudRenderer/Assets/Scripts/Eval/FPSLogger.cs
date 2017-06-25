using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Controllers;

namespace Eval {
    public class FPSLogger : MonoBehaviour {

        public AbstractPointSetController controller;
        public string testIdentifier;

        private List<float> deltaTs = new List<float>(115 * 60);
        private List<uint> pointcounts = new List<uint>(115 * 60);

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            deltaTs.Add(Time.deltaTime);
            pointcounts.Add(controller.GetPointCount());
        }

        private void OnApplicationQuit() {
            System.IO.StreamWriter output = new System.IO.StreamWriter("evaltest_" + testIdentifier + ".txt");
            output.WriteLine("TestRun: " + DateTime.Now.ToString() + " - " + testIdentifier + ", count: " + deltaTs.Count + "/" + pointcounts.Count);
            foreach (float dT in deltaTs) {
                output.Write(dT);
                output.Write(";");
            }
            output.WriteLine();
            foreach (uint pc  in pointcounts) {
                output.Write(pc);
                output.Write(";");
            }
            output.WriteLine();
            output.Flush();
            output.Close();
        }
    }
}