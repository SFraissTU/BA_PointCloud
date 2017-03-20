using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

/* Provides methods for loading pointclouds
 */
class CloudLoader
{
    /* Loads the metadata from the json-file in the given cloudpath
     */
    public static PointCloudMetaData LoadMetaData(string cloudPath)
    {
        string jsonfile;
        using (StreamReader reader = new StreamReader(cloudPath + "cloud.js", Encoding.Default))
        {
            jsonfile = reader.ReadToEnd();
            reader.Close();
        }
        return PointCloudMetaData.ReadFromJson(jsonfile);
    }

    /* Loads the complete Hierarchy and ALL points from the pointcloud described in the PointCloudMetaData
     */
    public static Node LoadPointCloud(string cloudPath, PointCloudMetaData metaData)
    {
        string dataRPath = cloudPath + metaData.octreeDir + "\\r\\";
        Node rootNode = new Node("", metaData.boundingBox);
        LoadHierarchy(dataRPath, metaData, rootNode);
        LoadAllPoints(dataRPath, metaData, rootNode);
        return rootNode;
    }

    /* Loads the complete hierarchy of the given node. Creates all the children and their data. Points are not yet stored in there.
     * dataRPath is the path of the R-folder
     */
    private static void LoadHierarchy(string dataRPath, PointCloudMetaData metaData, Node root)
    {
        byte[] data;
        if (root.Name.Length < metaData.hierarchyStepSize)
        {
            data = File.ReadAllBytes(dataRPath + "r" + root.Name + ".hrc");
        } else
        {
            data = File.ReadAllBytes(dataRPath + "\\" + root.Name + "\\r" + root.Name + ".hrc");
        }
        int nodeByteSize = 5;
        int numNodes = data.Length / nodeByteSize;
        int offset = 0;
        Queue<Node> nextNodes = new Queue<Node>();
        nextNodes.Enqueue(root);

        for (int i = 0; i < numNodes; i++)
        {
            Node n = nextNodes.Dequeue();
            byte configuration = data[offset];
            //uint pointcount = System.BitConverter.ToUInt32(data, offset + 1);
            //n.Pointcount = pointcount; //Pointcount is wrong
            for (int j = 0; j < 8; j++)
            {
                //check bits
                if ((configuration & (1 << j)) != 0)
                {
                    Node child = new Node(n.Name + j, calculateBoundingBox(n.BoundingBox, j));
                    n.SetChild(j, child);
                    nextNodes.Enqueue(child);
                }
            }
            offset += 5;
        }
        while(nextNodes.Count != 0) {
            Node n = nextNodes.Dequeue();
            LoadHierarchy(dataRPath, metaData, n);
        }
    }

    private static BoundingBox calculateBoundingBox(BoundingBox parent, int index)
    {
        Vector3 min = parent.Min();
        Vector3 max = parent.Max();
        Vector3 size = parent.Size();
        if ((index & 1) != 0) {
            min.z += size.z / 2;
        } else {
            max.z -= size.z / 2;
        }
        if ((index & 2) != 0) {
            min.y += size.y / 2;
        } else {
            max.y -= size.y / 2;
        }
        if ((index & 4) != 0) {
            min.x += size.x / 2;
        } else {
            max.x -= size.x / 2;
        }
        return new BoundingBox(min, max);
    }

    /* Loads the points for just that one node
     */
    private static void LoadPoints(string dataRPath, PointCloudMetaData metaData, Node node)
    {
        byte[] data;
        if (node.Name.Length < metaData.hierarchyStepSize)
        {
            data = File.ReadAllBytes(dataRPath + "r" + node.Name + ".bin");
        }
        else
        {
            data = File.ReadAllBytes(dataRPath + "\\" + node.Name + "\\r" + node.Name + ".bin");
        }
        int pointByteSize = 24;//TODO: Is this always the case?
        int numPoints = data.Length / pointByteSize;
        int offset = 0;


        //Read in data
        foreach (string pointAttribute in metaData.pointAttributes)
        {
            if (pointAttribute.Equals(PointAttributes.POSITION_CARTESIAN))
            {
                Vector3[] vertices = new Vector3[numPoints];
                for (int i = 0; i < numPoints; i++)
                {
                    //TODO: min
                    //TODO: double precision...
                    //Note: y and z are switched
                    float x = System.BitConverter.ToUInt32(data, offset + i * pointByteSize + 0) * metaData.scale + node.BoundingBox.lx;
                    float y = System.BitConverter.ToUInt32(data, offset + i * pointByteSize + 8) * metaData.scale + node.BoundingBox.lz;
                    float z = System.BitConverter.ToUInt32(data, offset + i * pointByteSize + 4) * metaData.scale + node.BoundingBox.ly;
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
    }

    /* Loads the points for that node and all its children
     */
    private static void LoadAllPoints(string dataRPath, PointCloudMetaData metaData, Node node)
    {
        LoadPoints(dataRPath, metaData, node);
        for (int i = 0; i < 8; i++)
        {
            if (node.HasChild(i))
            {
                LoadAllPoints(dataRPath, metaData, node.GetChild(i));
            }
        }
    }
}