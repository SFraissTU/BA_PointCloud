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
    /// Contains all attributes from the cloud.js-File.
    /// The cloud may be local or online. If it's local, cloudPath is the path to the cloud,
    /// and cloudUrl is null. If it's local, cloudUrl is the url to the cloud, and cloudPath
    /// is the destination where to download it to.
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
        [NonSerialized]
        public List<PointAttribute> pointAttributesList;
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
    }

    [Serializable]
    public class PointCloudMetaDataV1_8 : PointCloudMetaData
    {
        public List<PointAttribute> pointAttributes;
    }

    [Serializable]
    public class PointCloudMetaDataV1_7 : PointCloudMetaData
    {
        public List<string> pointAttributes;
    }

    public class PointCloudMetaDataReader
    {
        /// <summary>
        /// Reads the metadata from a json-string.
        /// </summary>
        /// <param name="json">Json-String</param>
        /// <param name="moveToOrigin">True, iff the center of the bounding boxes should be moved to the origin</param>
        public static PointCloudMetaData ReadFromJson(string json, bool moveToOrigin)
        {
            PointCloudMetaData data = JsonUtility.FromJson<PointCloudMetaData>(json);
            //Debug.Log("ReadFromJson - Version: "+data.version);
            if(data.version == "1.8"){
                PointCloudMetaDataV1_8 dt = JsonUtility.FromJson<PointCloudMetaDataV1_8>(json);
                data.pointAttributesList = dt.pointAttributes;
            }else{
                //workarround for version < 1.7
                PointCloudMetaDataV1_7 dt = JsonUtility.FromJson<PointCloudMetaDataV1_7>(json);
                data.pointAttributesList = new List<PointAttribute>();
                foreach(string attr in dt.pointAttributes){
                    PointAttribute pta = new PointAttribute();
                    pta.name = attr;
                    if (attr == Loading.PointAttributes.POSITION_CARTESIAN) {
                        pta.size = 12;
                    }else if (attr == Loading.PointAttributes.COLOR_PACKED) {
                        pta.size = 4;
                    }else if (attr == Loading.PointAttributes.INTENSITY){
                        pta.size = 2;
                    }else if (attr == Loading.PointAttributes.CLASSIFICATION){
                        pta.size = 2;
                    }
                    data.pointAttributesList.Add(pta);
                }
            }
            data.pointByteSize = 0;
            foreach (PointAttribute pointAttribute in data.pointAttributesList) {
                data.pointByteSize += pointAttribute.size;
            }

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
