using Loading;
using ObjectCreation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CloudController {
    class StaticPointCloudSet : AbstractPointCloudSet {
        
        public MeshConfiguration meshConfiguration = null;

        // Use this for initialization
        protected override void Initialize() {
            PointRenderer = new StaticRenderer(meshConfiguration);
        }


        // Update is called once per frame
        void Update() {
            if (!CheckReady()) return;
            PointRenderer.Update();
        }
    }
}
