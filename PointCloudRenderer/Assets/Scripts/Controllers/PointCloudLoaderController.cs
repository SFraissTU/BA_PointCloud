using System;
using System.Threading;
using UnityEngine;
using MeshConfigurations;
using CloudData;
using Loading;

namespace Controllers
{
    /* MonoBehaviour for loading PointClouds from a file. All points are loaded at once at the beginning and are then displayed.
     * No dynamic changing is possible afterwards
     */
    public class PointCloudLoaderController : MonoBehaviour
    {

        //Path to the folder in which the cloud.js is
        public string cloudPath;
        //Defines the type of PointCloud (Points, Quads, Circles)
        public MeshConfiguration meshConfiguration;
        //If the cloud should be moved to the origin
        public bool moveToOrigin;

        private PointCloudMetaData metaData;
        private Node rootNode;
        private bool fileLoading = false;

        // Use this for initialization
        void Start()
        {
            Thread thread = new Thread(new ThreadStart(LoadFile));
            thread.Start();
        }

        //Loads the complete point cloud
        private void LoadFile()
        {
            try
            {
                Debug.Log("Loading file");
                fileLoading = true;
                if (!cloudPath.EndsWith("\\"))
                {
                    cloudPath = cloudPath + "\\";
                }

                metaData = CloudLoader.LoadMetaData(cloudPath, moveToOrigin);

                rootNode = CloudLoader.LoadPointCloud(cloudPath, metaData);

                Debug.Log("Finished Loading");

                fileLoading = false;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!fileLoading && rootNode != null)
            {
                rootNode.CreateAllGameObjects(meshConfiguration);
                rootNode = null; //TODO: temporary line, so this doesnt happen every frame
                Debug.Log("Created GameObject");
            }
        }

        /*
         * Stops the loading of the file if the application is closed
         * TODO: This doesn't have any consequence right now
         */
        private void OnApplicationQuit()
        {
            fileLoading = false;
        }
    }

}
