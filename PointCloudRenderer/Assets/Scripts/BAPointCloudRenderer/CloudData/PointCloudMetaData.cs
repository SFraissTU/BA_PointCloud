using System;
using System.Collections.Generic;
using UnityEngine;

namespace BAPointCloudRenderer.CloudData
{

  [Serializable]
  public class PointAttribute
  {
      public string name;
      public int size;
      public int elements;
      public int elementSize;
      public string type;
      public string description;

  }
    /// <summary>
    /// Description of a Bounding Box. Created from the cloud.js-File.
    /// Contains all attributes from that file plus two more: cloudPath (folder path of the cloud) and cloudName (name of the cloud)
    /// </summary>
    [Serializable]
    public class PointCloudMetaData
    {
        public string version;
        public string octreeDir;
        public string projection;
        public int points;
        public BoundingBox boundingBox;
        public BoundingBox tightBoundingBox;
        public List<PointAttribute> pointAttributes;
        public double spacing;
        public double scale;
        public int hierarchyStepSize;
        [NonSerialized]
        public string cloudPath;
        [NonSerialized]
        public string cloudName;
        [NonSerialized]
        public string cloudUrl;
        [NonSerialized]
        public int pointByteSize;
        /// <summary>
        /// Reads the metadata from a json-string.
        /// </summary>
        /// <param name="json">Json-String</param>
        /// <param name="moveToOrigin">True, iff the center of the bounding boxes should be moved to the origin</param>
        public static PointCloudMetaData ReadFromJson(string json, bool moveToOrigin)
        {
            Debug.Log("ReadFromJson");
            PointCloudMetaData data = JsonUtility.FromJson<PointCloudMetaData>(json);
            if(data.version == "1.8"){
              foreach (PointAttribute pointAttribute in data.pointAttributes) {
                Debug.Log(pointAttribute.name);
                Debug.Log(pointAttribute.size);
                data.pointByteSize += pointAttribute.size;
                }
            }else{
                //workarround for version < 1.7
                data.pointByteSize = 16;
                data.pointAttributes[0].name="POSITION_CARTESIAN";
                data.pointAttributes[1].name="COLOR_PACKED";
            }
            
            Debug.Log(data.pointByteSize);

            data.boundingBox.Init();
            data.boundingBox.SwitchYZ();
            data.tightBoundingBox.SwitchYZ();
            if (moveToOrigin)
            {
                data.boundingBox.MoveToOrigin();
                data.tightBoundingBox.MoveToOrigin();
            }
            return data;
        }


    }
}
