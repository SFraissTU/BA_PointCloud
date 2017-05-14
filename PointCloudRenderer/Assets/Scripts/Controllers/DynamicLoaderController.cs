using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using DataStructures;
using ObjectCreation;
using CloudData;
using Loading;

namespace Controllers {

    /* While PointCloudLoaderController will load the complete file as one and render the comlete one, 
     * the DynamicLoaderController will first only load the hierarchy. It can be given registered at a PointCloudSetController to render it.
     */
    public class DynamicLoaderController : MonoBehaviour {

        //-----Public Options-----
        //Path to the folder in which the cloud.js is
        public string cloudPath;

        public AbstractPointSetController setController;


        // Use this for initialization
        void Start() {
            setController.RegisterController(this);
            Thread thread = new Thread(LoadHierarchy);
            thread.Start();
        }

        void LoadHierarchy() {
            try {
                if (!cloudPath.EndsWith("\\")) {
                    cloudPath = cloudPath + "\\";
                }

                PointCloudMetaData metaData = CloudLoader.LoadMetaData(cloudPath, false);

                setController.UpdateBoundingBox(this, metaData.boundingBox);

                Node rootNode = CloudLoader.LoadHierarchyOnly(metaData);

                setController.AddRootNode(rootNode);
            } catch (Exception ex) {
                Debug.LogError(ex);
            }
        }
        
    }

}