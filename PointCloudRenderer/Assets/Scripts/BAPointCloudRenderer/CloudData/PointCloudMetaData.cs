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
        public string version;
        public string octreeDir;
        public string projection;
        public int points;
        [NonSerialized]
        public BoundingBox boundingBox_transformed;  //This has to be set explicitely!
        [NonSerialized]
        public BoundingBox tightBoundingBox_transformed;
        [NonSerialized]
        public List<PointAttribute> pointAttributesList;
        public double spacing;
        [NonSerialized]
        public Vector3d scale3d;
        public int hierarchyStepSize;
        [NonSerialized]
        public string cloudPath;
        [NonSerialized]
        public string cloudName;
        [NonSerialized]
        public string cloudUrl;
        [NonSerialized]
        public int pointByteSize;

        public virtual Node createRootNode()
        {
            return null;
        }

        public virtual Vector3d getAdditionalTranslation()
        {
            return new Vector3d(0, 0, 0);
        }
    }

    [Serializable]
    public class PointCloudMetaDataV1_8 : PointCloudMetaData
    {
        public List<PointAttribute> pointAttributes;
        public BoundingBox boundingBox;
        public BoundingBox tightBoundingBox;
        public double scale;

        public override Node createRootNode()
        {
            return new Node("", this, this.boundingBox_transformed, null);
        }
    }

    [Serializable]
    public class PointCloudMetaDataV1_7 : PointCloudMetaData
    {
        public List<string> pointAttributes;
        public BoundingBox boundingBox;
        public BoundingBox tightBoundingBox;
        public double scale;

        public override Node createRootNode()
        {
            return new Node("", this, this.boundingBox_transformed, null);
        }
    }

    [Serializable]
    public class PointCloudMetaDataV2_0 : PointCloudMetaData
    {
        [Serializable]
        public class Hierarchy
        {
            public UInt64 firstChunkSize;
            public UInt64 stepSize;
            public UInt64 depth;
        }

        public string name;
        public string description;
        public Hierarchy hierarchy;
        public string pointAttributes;
        public List<float> offset;
        public new List<double> scale;
        public BoundingBoxV2 boundingBox;
        public string encoding = "DEFAULT";
        public List<PointAttributeV2_0> attributes;

        [NonSerialized]
        public Vector3d additionalTranslation = null;

        public override Node createRootNode()
        {
            return new Node("", this, base.boundingBox_transformed, null)
            {
                type = 2,
                level = 0,
                hierarchyByteOffset = 0,
                hierarchyByteSize = this.hierarchy.firstChunkSize,
                spacing = this.spacing,
                byteSize = 0,
                byteOffset = 0
            };
        }
        
        public override Vector3d getAdditionalTranslation()
        {
            if (additionalTranslation == null)
            {
                //additionalTranslation = -data_tmp.boundingBox_transformed.Center();
                BoundingBox originalBB = new BoundingBox(
                    new Vector3d(
                        boundingBox.min[0] - offset[0],
                        boundingBox.min[1] - offset[1],
                        boundingBox.min[2] - offset[2]
                        ),
                    new Vector3d(
                        boundingBox.max[0] - offset[0],
                        boundingBox.max[1] - offset[1],
                        boundingBox.max[2] - offset[2]
                        )
                    );
                additionalTranslation = boundingBox_transformed.Center() - originalBB.Center();
            }
            return additionalTranslation;
        }
    }

    public class PointCloudMetaDataReader
    {
        /// <summary>
        /// Reads the metadata from a json-string.
        /// </summary>
        /// <param name="json">Json-String</param>
        /// <param name="moveToOrigin">True, if the center of the bounding boxes should be moved to the origin</param>
        public static PointCloudMetaData ReadFromJson(string json, bool moveToOrigin)
        {
            PointCloudMetaData data = JsonUtility.FromJson<PointCloudMetaData>(json);

            if(data.version == "2.0")
            {
                // JsonUtility is incapable of serializing nested dicts. Newton to the help!
                PointCloudMetaDataV2_0 data_tmp = JsonUtility.FromJson<PointCloudMetaDataV2_0>(json);

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
                
                data_tmp.boundingBox_transformed = new BoundingBox(
                    new Vector3d(
                        data_tmp.boundingBox.min[0] - data_tmp.offset[0], 
                        data_tmp.boundingBox.min[1] - data_tmp.offset[1], 
                        data_tmp.boundingBox.min[2] - data_tmp.offset[2]
                        ),
                    new Vector3d(
                        data_tmp.boundingBox.max[0] - data_tmp.offset[0], 
                        data_tmp.boundingBox.max[1] - data_tmp.offset[1], 
                        data_tmp.boundingBox.max[2] - data_tmp.offset[2]
                        )
                    );

                data_tmp.tightBoundingBox_transformed = data_tmp.boundingBox_transformed.Clone();

                data_tmp.boundingBox_transformed.Init();
                data_tmp.tightBoundingBox_transformed.Init();

                data_tmp.scale3d = new Vector3d(data_tmp.scale[0], data_tmp.scale[1], data_tmp.scale[2]);

                if (moveToOrigin)
                {
                    data_tmp.boundingBox_transformed.MoveToOrigin();
                    data_tmp.tightBoundingBox_transformed.MoveToOrigin();
                }
                return data_tmp;
            }
            else if (data.version.StartsWith("1."))
            {
                if (data.version == "1.8")
                {
                    PointCloudMetaDataV1_8 dt = JsonUtility.FromJson<PointCloudMetaDataV1_8>(json);
                    dt.boundingBox_transformed = dt.boundingBox;
                    dt.tightBoundingBox_transformed = dt.tightBoundingBox;
                    dt.scale3d = new Vector3d(dt.scale, dt.scale, dt.scale);
                    data = dt;
                    data.pointAttributesList = dt.pointAttributes;
                }
                else
                {
                    //workarround for version < 1.7
                    PointCloudMetaDataV1_7 dt = JsonUtility.FromJson<PointCloudMetaDataV1_7>(json);
                    dt.boundingBox_transformed = dt.boundingBox;
                    dt.tightBoundingBox_transformed = dt.tightBoundingBox;
                    dt.scale3d = new Vector3d(dt.scale, dt.scale, dt.scale);
                    data = dt;
                    data.pointAttributesList = new List<PointAttribute>();
                    foreach (string attr in dt.pointAttributes)
                    {
                        PointAttribute pta = new PointAttribute();
                        pta.name = attr;
                        if (attr == Loading.PointAttributes.POSITION_CARTESIAN)
                        {
                            pta.size = 12;
                        }
                        else if (attr == Loading.PointAttributes.COLOR_PACKED)
                        {
                            pta.size = 4;
                        }
                        else if (attr == Loading.PointAttributes.INTENSITY)
                        {
                            pta.size = 2;
                        }
                        else if (attr == Loading.PointAttributes.CLASSIFICATION)
                        {
                            pta.size = 2;
                        }
                        data.pointAttributesList.Add(pta);
                    }
                }
                //Common code for V1
                data.pointByteSize = 0;
                foreach (PointAttribute pointAttribute in data.pointAttributesList)
                {
                    data.pointByteSize += pointAttribute.size;
                }

                data.boundingBox_transformed.Init();
                data.boundingBox_transformed.SwitchYZ();
                data.tightBoundingBox_transformed.Init();
                data.tightBoundingBox_transformed.SwitchYZ();
                if (moveToOrigin)
                {
                    data.boundingBox_transformed.MoveToOrigin();
                    data.tightBoundingBox_transformed.MoveToOrigin();
                }
                return data;
            }
            else
            {
                throw new Exception("Unsupported Potree version: " + data.version.ToString());
            }

        }
    }



}
