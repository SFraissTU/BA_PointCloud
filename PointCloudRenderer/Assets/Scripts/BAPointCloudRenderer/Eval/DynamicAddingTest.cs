using BAPointCloudRenderer.CloudController;
using System;
using UnityEngine;

namespace BAPointCloudRenderer.Eval {
    class DynamicAddingTest : MonoBehaviour {

        public AbstractPointCloudSet set = null;
        public String cloudPath1 = null;
        public String cloudPath2 = null;
        private int status = 0;
        private PointCloudLoader loader1;

        private void Update() {
            if (Input.GetKeyDown("space")) {
                if (status == 0) {
                    GameObject go = new GameObject("TestCloud1");
                    loader1 = go.AddComponent<PointCloudLoader>();
                    loader1.cloudPath = cloudPath1;
                    loader1.setController = set;
                } else if (status == 1) {
                    GameObject go = new GameObject("TestCloud2");
                    PointCloudLoader loader = go.AddComponent<PointCloudLoader>();
                    loader.cloudPath = cloudPath2;
                    loader.setController = set;
                } else if (status == 2) {
                    loader1.RemovePointCloud();
                } else if (status == 3) {
                    set.StopRendering();
                }
                status++;
            }
        }
    }
}
