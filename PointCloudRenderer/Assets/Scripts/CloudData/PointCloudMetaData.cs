using System;
using System.Collections.Generic;
using UnityEngine;

namespace CloudData
{
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
        public List<string> pointAttributes;
        public double spacing;
        public double scale;
        public int hierarchyStepSize;
        [NonSerialized]
        public string cloudPath;
        [NonSerialized]
        public string cloudName;

        /// <summary>
        /// Reads the metadata from a json-string.
        /// </summary>
        /// <param name="json">Json-String</param>
        /// <param name="moveToOrigin">True, iff the center of the bounding boxes should be moved to the origin</param>
        public static PointCloudMetaData ReadFromJson(string json, bool moveToOrigin)
        {
            PointCloudMetaData data = JsonUtility.FromJson<PointCloudMetaData>(json);
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
