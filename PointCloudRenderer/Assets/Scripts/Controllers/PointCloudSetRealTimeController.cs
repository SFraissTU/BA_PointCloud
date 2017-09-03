using CloudData;
using Loading;
using ObjectCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Controllers {
    
     /// <summary>
     /// The PointSetController for RealTimeRendering described in the thesis. Uses a V2Renderer, so each frame the displayed GameObjects get refreshed
     /// </summary>
    public class PointCloudSetRealTimeController : AbstractPointSetController {
        
        /// <summary>
        /// Point Budget - Maximum Number of Points in Memory / to Render
        /// </summary>
        public uint pointBudget = 1000000;
        /// <summary>
        /// Minimum Node Size
        /// </summary>
        public int minNodeSize = 10;
        /// <summary>
        /// Maximum number of nodes loaded per frame
        /// </summary>
        public uint nodesLoadedPerFrame = 15;
        /// <summary>
        /// Maximum number of nodes having their gameobjects created per frame
        /// </summary>
        public uint nodesGOsPerFrame = 30;
        /// <summary>
        /// MeshConfiguration. Defines how to render the points.
        /// </summary>
        public MeshConfiguration meshConfiguration;
        /// <summary>
        /// Cache Size in POints
        /// </summary>
        public uint cacheSizeInPoints = 1000000;

        private Camera userCamera;

        // Use this for initialization
        protected override void Initialize() {
            userCamera = Camera.main;
            PointRenderer = new V2Renderer(minNodeSize, pointBudget, nodesLoadedPerFrame, nodesGOsPerFrame, userCamera, meshConfiguration, cacheSizeInPoints);
        }
        

        // Update is called once per frame
        void Update() {
            if (!CheckReady()) return;
            PointRenderer.Update();
        }
    }
}