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
    public double spacing;
    public double scale;
    public int hierarchyStepSize;

    public static PointCloudMetaData ReadFromJson(string json)
    {
        PointCloudMetaData data = JsonUtility.FromJson<PointCloudMetaData>(json);
        data.boundingBox.SwitchYZ();
        data.tightBoundingBox.SwitchYZ();
        return data;
    }
    
    
}