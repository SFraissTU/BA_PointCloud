using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

public class PointCloudLoaderController : MonoBehaviour {

    public string cloudPath;
    public MeshConfiguration meshConfiguration;

    private PointCloudMetaData metaData;
    private string dataRPath;
    private Node rootNode;
    private bool fileLoading = false; //TO IMPLEMENT

    // Use this for initialization
    void Start () {
        Thread thread = new Thread(new ThreadStart(LoadFile));
        thread.Start();
    }

    private void LoadFile()
    {
        fileLoading = true;
        if (!cloudPath.EndsWith("\\"))
        {
            cloudPath = cloudPath + "\\";
        }
        string jsonfile;
        using (StreamReader reader = new StreamReader(cloudPath + "cloud.js", Encoding.Default))
        {
            jsonfile = reader.ReadToEnd();
            reader.Close();
        }
        metaData = PointCloudMetaData.ReadFromJson(jsonfile);

        dataRPath = cloudPath + metaData.octreeDir + "\\r\\";
        LoadNode("");
    }
    
    //Loads JUST that node
    private void LoadNode(string id)
    {
        Debug.Log("Loading Node " + id);
        //TODO: This seems to stop without any feedback
        byte[] data;
        try
        {
            data = File.ReadAllBytes(dataRPath + "r" + id + ".bin");
        } catch (IOException ex)
        {
            Debug.LogError(ex.Message);
            return;
        }
        Debug.Log("Read File: " + data.Length + " bytes");
        int pointByteSize = 24;//TODO: Is this always the case?
        int numPoints = data.Length / pointByteSize;
        int offset = 0;

        Node node = new Node(id);

        //Read in data
        foreach (string pointAttribute in metaData.pointAttributes)
        {
            if (pointAttribute.Equals(PointAttributes.POSITION_CARTESIAN))
            {
                Vector3[] vertices = new Vector3[numPoints];
                for (int i = 0; i < numPoints; i++)
                {
                    //TODO: min
                    //Note: y and z are switched
                    float x = System.BitConverter.ToUInt32(data, offset + i * pointByteSize + 0) * metaData.scale;
                    float y = System.BitConverter.ToUInt32(data, offset + i * pointByteSize + 8) * metaData.scale;
                    float z = System.BitConverter.ToUInt32(data, offset + i * pointByteSize + 4) * metaData.scale;
                    vertices[i] = new Vector3(x, y, z);
                }
                offset += 12;
                node.VerticesToStore = vertices;
            }
            else if (pointAttribute.Equals(PointAttributes.COLOR_PACKED))
            {
                Color[] colors = new Color[numPoints];
                for (int i = 0; i < numPoints; i++)
                {
                    byte r = data[offset + i * pointByteSize + 0];
                    byte g = data[offset + i * pointByteSize + 1];
                    byte b = data[offset + i * pointByteSize + 2];
                    colors[i] = new Color32(r, g, b, 255);
                }
                offset += 3;
                node.ColorsToStore = colors;
            }
        }
        rootNode = node;
        Debug.Log("Created Node");
    }
	
	// Update is called once per frame
	void Update () {
		if (rootNode != null)
        {
            rootNode.CreateGameObjects(meshConfiguration);
            rootNode = null; //TODO: Just now so this doesnt happen every frame
            Debug.Log("Created GameObject");
        }
	}

    /*
     * Stops the loading of the file if the application is closed
     */
    private void OnApplicationQuit()
    {
        fileLoading = false;
    }
}
