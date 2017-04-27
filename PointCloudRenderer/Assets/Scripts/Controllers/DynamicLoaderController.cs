using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using DataStructures;
using ObjectCreation;
using CloudData;
using Loading;

namespace Controllers {

    /* While PointCloudLoaderController will load the complete file as one, the DynamicLoaderController will first just load the hierarchy and load only the important nodes when pressing a key
     */
    public class DynamicLoaderController : MonoBehaviour {

        //-----Public Options-----
        //Path to the folder in which the cloud.js is
        public string cloudPath;

        public PointCloudSetController setController;


        // Use this for initialization
        void Start() {
            setController.RegisterController(this);
            Thread thread = new Thread(LoadHierarchy);
            thread.Start();
        }

        void LoadHierarchy() {
            try {
                Debug.Log("Loading Hierarchy");
                if (!cloudPath.EndsWith("\\")) {
                    cloudPath = cloudPath + "\\";
                }

                PointCloudMetaData metaData = CloudLoader.LoadMetaData(cloudPath, false);

                setController.UpdateBoundingBox(this, metaData.boundingBox);

                Node rootNode = CloudLoader.LoadHierarchyOnly(metaData);

                setController.AddRootNode(rootNode);

                Debug.Log("Finished Loading Hierachy");
            } catch (Exception ex) {
                Debug.LogError(ex);
            }
        }
        
    }

}