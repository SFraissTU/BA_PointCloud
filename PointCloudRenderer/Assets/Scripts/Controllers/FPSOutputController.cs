using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers {
    public class FPSOutputController : MonoBehaviour {

        //Functionality for calculating the average fps over a time period
        private static List<float> deltats = new List<float>();

        public static void NoteFPS(bool flush) {
            if (flush) {
                string str = "";
                float sumtime = 0;
                float sumfps = 0;
                foreach (float f in deltats) {
                    str += (1/f) + ";";
                    sumfps += (1/f);
                    sumtime += f;
                }
                if (sumtime != 0) {
                    float avg = sumfps / deltats.Count;
                    Debug.Log("overalltime: " + sumtime + ", avg: " + avg/* + " - " + str*/);
                }
                deltats.Clear();
            } else {
                deltats.Add(Time.deltaTime);
            }
        }


        //For printing FPS constantly

        public float outputInterval = 1;

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
            }
        }
    }
}