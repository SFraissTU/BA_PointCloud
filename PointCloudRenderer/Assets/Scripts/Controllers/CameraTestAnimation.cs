using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Controllers {
    class CameraTestAnimation : MonoBehaviour {

        Vector3 startpoint = new Vector3(-1400, -265, -1465);
        Vector3 firstcurve = new Vector3(-1400, -265, -600);
        float time = 0;
        float speed = 80;
        float sqr2 = Mathf.Sqrt(2) / 2;

        private void Start() {
            transform.position = startpoint;
        }

        void Update() {
            time += Time.deltaTime;
            if (time < 865/speed) {
                float alpha = time / 10;
                //transform.position = (1 - alpha) * startpoint + alpha * firstcurve;
                transform.Translate(0, 0, speed * Time.deltaTime);
            } else if (time < 1465/speed) {
                transform.Translate(speed * sqr2 * Time.deltaTime, 0, speed * sqr2 * Time.deltaTime);
                if (time < 1165/speed) {
                    transform.Rotate(new Vector3(0, 1, 0), (time-865) * 45.0f / 300.0f);
                }
            }
        }
    }
}
