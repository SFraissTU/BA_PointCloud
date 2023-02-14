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

    [Serializable]
    public class PointAttributeV2_0 : PointAttribute
    {
        public int numElements, byteSize, typeSize;
        public List<float> min, max, scale, offset, histogram;
        public long[] range = new long[2] { long.MaxValue, long.MinValue };
        public enum Types
        {
            Double = 8,
            Float = 4,
            Int8 = 1,
            Uint8 = 1,
            Int16 = 2,
            Uint16 = 2,
            Int32 = 4,
            Uint32 = 4,
            Int64 = 8,
            Uint64 = 8
        }
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
        public double version;
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

    [Serializable]
    public class PointCloudMetaDataV2_0 : PointCloudMetaData
    {
        public string name;
        public string description;
        public Dictionary<string, UInt64> hierarchy;
        public string pointAttributes;
        public List<float> offset;
        public new List<double> scale;
        /// <summary>
        /// this is a tricky one, as the dict is a new invention for V2 
        /// while the name of the attribue is the same as in earlier versions.
        /// hence the base.boundingBox will need to be used instead for setting 
        /// and reading The Real Box of Bounding.
        /// </summary>
        public new Dictionary<string, List<float>> boundingBox;
        public BoundingBox boundingBoxInternal;
        public string encoding = "DEFAULT";
        public List<PointAttributeV2_0> attributes;
        //public new List<PointAttributeV2_0> pointAttributesList;
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
            //if (double.TryParse(data.version, out double ver) && ver == 2.0)
            if(data.version == 2.0)
            {
                Debug.Log("Potree v2");
                // as in v2 coordinates are from different origin, the groups need to relocate to origin.
                moveToOrigin = true;
                // JsonUtility is incapable of serializing nested dicts.
                PointCloudMetaDataV2_0 data_tmp = Newtonsoft.Json.JsonConvert.DeserializeObject<PointCloudMetaDataV2_0>(json); //JsonUtility.FromJson<PointCloudMetaDataV2_0>(json);
                data_tmp.pointAttributesList = new List<PointAttribute>();
                data_tmp.pointByteSize = 0;
                foreach (PointAttributeV2_0 pointAttribute in data_tmp.attributes)
                {
                    data_tmp.pointAttributesList.Add(pointAttribute);
                    data_tmp.pointByteSize += pointAttribute.size;

                    // as the potree (both, v1 & v2) documentation is missingin in action,
                    // i included here only the vars i found in some converted v2 sets.
                    // if you encounter others.. have fun :)
                    pointAttribute.typeSize = pointAttribute.type switch
                    {
                        "double"    => (int)PointAttributeV2_0.Types.Double,
                        "int32"     => (int)PointAttributeV2_0.Types.Int32,
                        "uint16"    => (int)PointAttributeV2_0.Types.Uint16,
                        "uint8"     => (int)PointAttributeV2_0.Types.Uint8,
                        "Int32"     => (int)PointAttributeV2_0.Types.Int32,

                        // this will probably break things unexpectedly, but.. YOLO :D
                        _           => (int)PointAttributeV2_0.Types.Int32, 
                    };
                    pointAttribute.byteSize = pointAttribute.numElements * pointAttribute.typeSize;
                }
                /*
                 * thats how its SUPPOSED to work.
                (data_tmp as PointCloudMetaData).boundingBox = new BoundingBox(
                    new Vector3d(data_tmp.boundingBox["min"][0], data_tmp.boundingBox["min"][1], data_tmp.boundingBox["min"][2]),
                    new Vector3d(data_tmp.boundingBox["max"][0], data_tmp.boundingBox["max"][1], data_tmp.boundingBox["max"][2])
                    );
                data_tmp.tightBoundingBox = new BoundingBox(
                    new Vector3d(data_tmp.boundingBox["min"][0], data_tmp.boundingBox["min"][1], data_tmp.boundingBox["min"][2]),
                    new Vector3d(data_tmp.boundingBox["max"][0], data_tmp.boundingBox["max"][1], data_tmp.boundingBox["max"][2])
                    );
                */

                // a hack to overcome p2 converter's weird interpretaion of calculated pointclouds bounds.
                (data_tmp as PointCloudMetaData).boundingBox = new BoundingBox(
                    new Vector3d(0,0,0),
                    new Vector3d(data_tmp.boundingBox["max"][0] - data_tmp.boundingBox["min"][0], data_tmp.boundingBox["max"][1] - data_tmp.boundingBox["min"][1], data_tmp.boundingBox["max"][2] - data_tmp.boundingBox["min"][2])
                    );
                data_tmp.tightBoundingBox = new BoundingBox(
                    new Vector3d(0, 0, 0),
                    new Vector3d(data_tmp.boundingBox["max"][0] - data_tmp.boundingBox["min"][0], data_tmp.boundingBox["max"][1] - data_tmp.boundingBox["min"][1], data_tmp.boundingBox["max"][2] - data_tmp.boundingBox["min"][2])
                    );

                (data_tmp as PointCloudMetaData).boundingBox.Init();
                data_tmp.tightBoundingBox.Init();
                if (moveToOrigin)
                {
                    (data_tmp as PointCloudMetaData).boundingBox.MoveToOrigin();
                    data_tmp.tightBoundingBox.MoveToOrigin();
                }
                return data_tmp;
            }
            else if (data.version == 1.8){
                PointCloudMetaDataV1_8 dt = JsonUtility.FromJson<PointCloudMetaDataV1_8>(json);
                data.pointAttributesList = dt.pointAttributes;
            }
            else if (data.version < 1.8)
            {
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
            else
            {
                throw new Exception("Unsupportder Potree version: " + data.version.ToString());
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
