using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using UnityEngine;

[Serializable]
public class PointCloudMetaData
{
    public string version;
    public string octreeDir;
    public string projection;
    public int points;
    public BoundingBox boundingBox;
    public BoundingBox tightBoundingBox;
    public List<string> pointAttributes;
    public float spacing;
    public float scale;
    public int hierarchyStepSize;

    public static PointCloudMetaData ReadFromJson(string json)
    {
        return JsonUtility.FromJson<PointCloudMetaData>(json);
        /*PointCloudMetaData data = new PointCloudMetaData();
        JObject jObject = JObject.Parse(json);
        data.version = (string)jObject["version"];
        data.octreeDir = (string)jObject["octreeDir"];
        data.projection = (string)jObject["projection"];
        data.points = (int)jObject["points"];
        JToken bb1 = jObject["boundingBox"];
        data.boundingBox = new BoundingBox((int)bb1["lx"], (int)bb1["ly"], (int)bb1["lz"], (int)bb1["ux"], (int)bb1["uy"], (int)bb1["uz"]);
        JToken bb2 = jObject["tightBoundingBox"];
        data.tightBoundingBox = new BoundingBox((int)bb2["lx"], (int)bb2["ly"], (int)bb2["lz"], (int)bb2["ux"], (int)bb2["uy"], (int)bb2["uz"]);
        JToken attrs = jObject["pointAttributes"];
        foreach (string attr in attrs)
        {
            data.pointAttributes.Add(attr);
        }
        data.spacing = (double)jObject["spacing"];
        data.scale = (double)jObject["scale"];
        data.hierarchyStepSize = (int)jObject["hierarchyStepSize"];
        return data;*/
    }
    
    
}