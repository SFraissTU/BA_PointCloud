using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using UnityEngine;

/*
 * This script is attached to an empty object to load in a pointcloud from a textfile (Space seperated values).
 * Several new gameobjects are created which contain the meshes of the pointcloud
 */
public class FilePointCloudController : MonoBehaviour {

    //Name of the pointcloud-file. Set in editor
    public string FileName;
    //Type of PointCloud. See PointCloudType for explanation.
    public PointCloudType.Types cloudType;
    //CloudConfiguration, derived by type
    private PointCloudType cloudConfiguration;
    //List of Points to still be put into the cloud
    private List<PointCloudPoint> points;
    //True, iff the file is currently loaded. False before loading and after loading
    private bool fileLoading = false;
    
    /*
     * Reads in a file and adds the points to the list. Called by start in a new thread
     */
	void LoadFile () {
        fileLoading = true;
        points = new List<PointCloudPoint>();
        using (StreamReader reader = new StreamReader(FileName, System.Text.Encoding.Default))
        {
            string line;
            while (fileLoading && (line = reader.ReadLine()) != null)
            {
                string[] words = line.Split(' ');
                lock (points)
                {
                    points.Add(new PointCloudPoint(
                        float.Parse(words[0], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(words[1], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(words[2], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(words[3], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(words[4], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(words[5], CultureInfo.InvariantCulture.NumberFormat)));
                }
            }
            reader.Close();
        }
        Debug.Log("Points loaded");
        fileLoading = false;
    }

    /*
     * Starts loading the file and initializes the material.
     */
    private void Start()
    {
        cloudConfiguration = PointCloudType.getTypeObject(cloudType);
        Thread thread = new Thread(new ThreadStart(LoadFile));
        thread.Start();
    }
	
	// Calls CreateMeshesFromPointList
	void Update () {
        CreateMeshesFromPointList();
	}

    /*
     * Stops the loading of the file if the application is closed
     */
    private void OnApplicationQuit()
    {
        fileLoading = false;
    }

    /*
     * Takes in the loaded points from the list and creates new gameobjects from them as long as enough points are still in the list to form a mesh.
     * If loading is ended all remaining points are put into gameobjects.
     */
    void CreateMeshesFromPointList()
    {
        if (points.Count >= cloudConfiguration.MaxPointsPerMesh || (!fileLoading && points.Count > 0))
        {
            List<PointCloudPoint> currentPoints;
            lock (points)
            {
                currentPoints = points.GetRange(0, Math.Min(cloudConfiguration.MaxPointsPerMesh, points.Count));
                points.RemoveRange(0, Math.Min(cloudConfiguration.MaxPointsPerMesh, points.Count));
            }
            //Create new GameObject
            GameObject subObject = new GameObject();
            subObject.AddComponent<MeshFilter>();
            subObject.AddComponent<MeshRenderer>();
            PointCloud cloud = subObject.AddComponent(cloudConfiguration.CloudClass) as PointCloud;
            cloud.PointList = currentPoints;
            CreateMeshesFromPointList();
        } 
    }
}
