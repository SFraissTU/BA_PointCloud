using BAPointCloudRenderer.CloudController;
using System;
using UnityEngine;

namespace BAPointCloudRenderer.Eval {
    class DynamicAddingTest : MonoBehaviour {

        public AbstractPointCloudSet set = null;
        public PointCloudLoader loader1 = null;
        public String cloudPath2 = null;
        private int status = 0;
        private PointCloudLoader loader2;

        private void Update() {
            if (Input.GetKeyDown("space")) {
                if (status == 0) {
                    GameObject go = new GameObject("TestCloud1");
                    loader1.LoadPointCloud();
                } else if (status == 1) {
                    GameObject go = new GameObject("TestCloud2");
                    loader2 = go.AddComponent<PointCloudLoader>();
                    loader2.cloudPath = cloudPath2;
                    loader2.setController = set;
                } else if (status == 2) {
                    loader1.RemovePointCloud();
                } else if (status == 3) {
                    loader2.RemovePointCloud();
                } else if (status == 4) {
                    GameObject go = new GameObject("TestCloud2");
                    loader2 = go.AddComponent<PointCloudLoader>();
                    loader2.cloudPath = cloudPath2;
                    loader2.setController = set;
                    loader2.loadOnStart = false;
                } else if (status == 5) {
                    loader2.LoadPointCloud();
                } else if (status == 6) {
                    loader2.RemovePointCloud();
                } else if (status == 7) {
                    set.StopRendering();
                }
                status++;
            }
        }
    }
}
