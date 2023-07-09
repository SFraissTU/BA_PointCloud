using BAPointCloudRenderer.CloudData;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System;
using UnityEngine;
using System.Linq;

namespace BAPointCloudRenderer.Loading {
    /// <summary>
    /// Provides methods for loading point clouds from the file system
    /// </summary>
    class CloudLoader {
        /* Loads the metadata from the json-file in the given cloudpath
         */
         /// <summary>
         /// Loads the meta data from the json-file in the given cloudpath. Attributes "cloudPath", and "cloudName" are set as well.
         /// </summary>
         /// <param name="cloudPath">Folderpath of the cloud or URL to download the cloud from. In the latter case, it will be downloaded to a /temp folder</param>
         /// <param name="moveToOrigin">True, if the center of the cloud should be moved to the origin</param>
        public static PointCloudMetaData LoadMetaData(string cloudPath, bool moveToOrigin = false) {
            string jsonfile = "";
            //Debug.Log(cloudPath);
            bool isCloudOnline = Uri.IsWellFormedUriString(cloudPath, UriKind.Absolute);
            if (isCloudOnline){
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(cloudPath + "cloud.js");
                StreamReader reader = new StreamReader(stream);
                jsonfile = reader.ReadToEnd();
                reader.Close();
            }else{
                string filePath;
                if (File.Exists(cloudPath + "cloud.js"))
                {
                    filePath = cloudPath + "cloud.js";
                }
                else if (File.Exists(cloudPath + "metadata.json"))
                {
                    filePath = cloudPath + "metadata.json";
                }
                else
                {
                    Debug.LogError("Unable to find neither cloud.js nor metadata.json from " + cloudPath);
                    throw new Exception("Unable to find neither cloud.js nor metadata.json from " + cloudPath);
                }
                if (filePath.Length > 0)
                {
                    using (StreamReader reader = new StreamReader(filePath, Encoding.Default))
                    {
                        jsonfile = reader.ReadToEnd();
                        reader.Close();
                    }
                }
            }

            PointCloudMetaData metaData = PointCloudMetaDataReader.ReadFromJson(jsonfile, moveToOrigin);

            metaData.cloudName =  cloudPath.Substring(0, cloudPath.Length-1).Substring(cloudPath.Substring(0, cloudPath.Length - 1).LastIndexOf("/") + 1);
            //Debug.Log(metaData.cloudName);

            if (isCloudOnline){
                metaData.cloudUrl = cloudPath;
                metaData.cloudPath = "temp/"+metaData.cloudName+"/";
            }else{
                metaData.cloudPath = cloudPath;
                metaData.cloudUrl = null;
            }


            return metaData;
        }

        /// <summary>
        /// Loads the complete Hierarchy and ALL points from the pointcloud, if so commanded.
        /// </summary>
        /// <param name="metaData">MetaData-Object, as received by LoadMetaData</param>
        /// <param name="loadAllPoints">whether to load ALL points or not.</param>
        /// <returns>The Root Node of the point cloud</returns>
        public static Node LoadPointCloud(PointCloudMetaData metaData, bool loadAllPoints = true) {
            string dataRPath = metaData.octreeDir + "/r/";
            Node rootNode = metaData.createRootNode();
            if (metaData.version == "2.0")
            {
                LoadHierarchy(metaData, ref rootNode);
            }
            else
            {
                LoadHierarchy(dataRPath, metaData, rootNode);
            }
            if (loadAllPoints)
            {
                LoadAllPoints(dataRPath, metaData, rootNode);
            }
            return rootNode;
        }

        /// <summary>
        /// Loads the hierarchy, but no points are loaded
        /// </summary>
        /// <param name="metaData">MetaData-Object, as received by LoadMetaData</param>
        /// <returns>The Root Node of the point cloud</returns>
        public static Node LoadHierarchyOnly(PointCloudMetaData metaData) {
            return LoadPointCloud(metaData, false);
        }

        /// <summary>
        /// Loads the points for the given node
        /// </summary>
        public static void LoadPointsForNode(Node node) {
            string dataRPath = node.MetaData.octreeDir + "/r/";
            LoadPoints(dataRPath, node.MetaData, node);
        }
        /// <summary>
        /// for Potree v2
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="node"></param>
        private static void LoadHierarchy(PointCloudMetaData metaData, ref Node node)
        {
            // sanitycheck.
            if (node.hierarchyByteSize > 0)
            {
                byte[] data = ReadFromFile(metaData.cloudPath + "hierarchy.bin", (long)node.hierarchyByteOffset, node.hierarchyByteSize);
                if (data.Length == (int)node.hierarchyByteSize)
                {
                    ParseHierarchy(ref node, data);
                }
                else
                {
                    Debug.Log("Got incorrect amount of data from hierarchy.bin: " + data.Length + " != " + node.hierarchyByteSize);
                }
            }
        }
        /// <summary>
        /// for Potree v2
        /// </summary>
        /// <param name="node"></param>
        /// <param name="buffer"></param>
        private static void ParseHierarchy(ref Node node, byte[] buffer)
        {
            int bytesPerNode = 22;
            int numNodes = buffer.Length / bytesPerNode;

            Node[] nodes = new Node[numNodes];
            nodes[0] = node;
            // 1, because root node will be at 0
            int nodePos = 1;

            for (int i = 0; i < numNodes; i++)
            {
                Node current = nodes[i];

                // weirdness squared.
                if (current is null)
                {
                    break;
                }

                byte type = buffer[i * bytesPerNode + 0];
                byte childMask = buffer[i * bytesPerNode + 1];
                UInt32 numPoints = System.BitConverter.ToUInt32(buffer, i * bytesPerNode + 2);
                UInt64 byteOffset = System.BitConverter.ToUInt64(buffer, i * bytesPerNode + 6);
                UInt64 byteSize = System.BitConverter.ToUInt64(buffer, i * bytesPerNode + 14);

                if (current.type == 2)
                {
                    // replace proxy with real node
                    current.byteOffset = byteOffset;
                    current.byteSize = byteSize;
                    current.numPoints = numPoints;
                }
                else if (type == 2)
                {
                    // load proxy
                    current.hierarchyByteOffset = byteOffset;
                    current.hierarchyByteSize = byteSize;
                    current.numPoints = numPoints;
                }
                else
                {
                    // load real node 
                    current.byteOffset = byteOffset;
                    current.byteSize = byteSize;
                    current.numPoints = numPoints;
                }

                if (current.byteSize == 0)
                {
                    // workaround for issue #1125
                    // some inner nodes erroneously report >0 points even though have 0 points
                    // however, they still report a byteSize of 0, so based on that we now set node.numPoints to 0
                    current.numPoints = 0;
                }

                current.type = type;

                if (current.type == 2)
                {
                    continue;
                }

                for (int childIndex = 0; childIndex < 8; childIndex++)
                {
                    bool childExists = ((1 << childIndex) & childMask) != 0;

                    if (!childExists)
                    {
                        continue;
                    }

                    string childName = current.Name + childIndex;

                    BoundingBox childAABB = CalculateBoundingBox(current.BoundingBox, childIndex);
                    Node child = new Node(childName, node.MetaData, childAABB, current)
                    {
                        spacing = current.spacing / 2,
                        level = current.level + 1,
                        numPoints = numPoints
                    };  

                    current.SetChild(childIndex, child);

                    if (nodePos >= numNodes)
                    {
                        break;
                    }
                    else
                    {
                        nodes[nodePos] = child;
                        nodePos++;
                    }
                } 
            }
        }

        /// <summary>
        /// Loads the complete hierarchy of the given node. Creates all the children and their data. 
        /// Points are not yet stored in there. dataRPath is the path of the R-folder
        /// </summary>
        /// <param name="dataRPath"></param>
        /// <param name="metaData"></param>
        /// <param name="root"></param>
        private static void LoadHierarchy(string dataRPath, PointCloudMetaData metaData, Node root) {
            byte[] data = FindAndLoadFile(dataRPath, metaData, root.Name, ".hrc");
            int nodeByteSize = 5;
            int numNodes = data.Length / nodeByteSize;
            int offset = 0;
            Queue<Node> nextNodes = new Queue<Node>();
            nextNodes.Enqueue(root);

            for (int i = 0; i < numNodes; i++) {
                Node n = nextNodes.Dequeue();
                byte configuration = data[offset];
                //uint pointcount = System.BitConverter.ToUInt32(data, offset + 1);
                //n.PointCount = pointcount; //TODO: Pointcount is wrong
                for (int j = 0; j < 8; j++) {
                    //check bits
                    if ((configuration & (1 << j)) != 0) {
                        //This is done twice for some nodes
                        Node child = new Node(n.Name + j, metaData, CalculateBoundingBox(n.BoundingBox, j), n);
                        n.SetChild(j, child);
                        nextNodes.Enqueue(child);
                    }
                }
                offset += 5;
            }
            HashSet<Node> parentsOfNextNodes = new HashSet<Node>();
            while (nextNodes.Count != 0) {
                Node n = nextNodes.Dequeue().Parent;
                if (!parentsOfNextNodes.Contains(n)) {
                    parentsOfNextNodes.Add(n);
                    LoadHierarchy(dataRPath, metaData, n);
                }
                //Node n = nextNodes.Dequeue();
                //LoadHierarchy(dataRPath, metaData, n);
            }
        }

        private static BoundingBox CalculateBoundingBox(BoundingBox parent, int index) {
            Vector3d min = parent.Min();
            Vector3d max = parent.Max();
            Vector3d size = parent.Size();
            //z and y are different here than in the sample-code because these coordinates are switched in unity
            if ((index & 2) != 0) {
                min.z += size.z / 2;
            } else {
                max.z -= size.z / 2;
            }
            if ((index & 1) != 0) {
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

        /// <summary>
        /// Loads the points for just that one node
        /// </summary>
        /// <param name="dataRPath"></param>
        /// <param name="metaData"></param>
        /// <param name="node"></param>
        private static void LoadPoints(string dataRPath, PointCloudMetaData metaData, Node node) {
            // in potree v2 type 2 nodes are proxies and their hierarchy 
            // yearns to be loaded just-in-time.
            if (metaData.version == "2.0" && node.type == 2)
            {
                LoadHierarchy(metaData, ref node);
            }
            byte[] data = metaData.version switch
            {
                "2.0" => ReadFromFile(metaData.cloudPath + "octree.bin", (long)node.byteOffset, node.byteSize),
                _ => FindAndLoadFile(dataRPath, metaData, node.Name, ".bin"),
            };
            int pointByteSize = metaData.pointByteSize;
            int numPoints = data.Length / pointByteSize;
            int offset = 0, toSetOff = 0;

            Vector3[] vertices = new Vector3[numPoints];
            Color[] colors = new Color[numPoints];
            //Read in data
            foreach (PointAttribute pointAttribute in metaData.pointAttributesList) {
                toSetOff = 0;
                if (pointAttribute.name.ToUpper().Equals(PointAttributes.POSITION_CARTESIAN) || pointAttribute.name.ToUpper().Equals(PointAttributes.POSITION)) {
                    for (int i = 0; i < numPoints; i++) {
                        //Reduction to single precision!
                        //Note: y and z are switched
                        float x = (float)(System.BitConverter.ToUInt32(data, offset + i * pointByteSize + 0) * metaData.scale3d.x);
                        float y = (float)(System.BitConverter.ToUInt32(data, offset + i * pointByteSize + 8) * metaData.scale3d.z);
                        float z = (float)(System.BitConverter.ToUInt32(data, offset + i * pointByteSize + 4) * metaData.scale3d.y);
                        vertices[i] = new Vector3(x, y, z);
                    }
                    toSetOff += 12;
                } else if (pointAttribute.name.ToUpper().Equals(PointAttributes.COLOR_PACKED)) {
                    for (int i = 0; i < numPoints; i++) {
                        byte r = data[offset + i * pointByteSize + 0];
                        byte g = data[offset + i * pointByteSize + 1];
                        byte b = data[offset + i * pointByteSize + 2];
                        colors[i] = new Color32(r, g, b, 255);
                    }
                    toSetOff += 3;
                }else if (pointAttribute.name.ToUpper().Equals(PointAttributes.RGBA) || pointAttribute.name.ToUpper().Equals(PointAttributes.RGB)) {
                    if (metaData.version == "2.0")
                    {
                        CalculateRGBA(ref colors, ref offset, data, pointByteSize, numPoints, pointAttribute.name.EndsWith("a"));
                    } 
                    else
                    {
                        for (int i = 0; i < numPoints; i++)
                        {
                            byte r = data[offset + i * pointByteSize + 0];
                            byte g = data[offset + i * pointByteSize + 1];
                            byte b = data[offset + i * pointByteSize + 2];
                            byte a = data[offset + i * pointByteSize + 3];
                            colors[i] = new Color32(r, g, b, a);
                        }
                        toSetOff += 4;
                    }
                }
                /*
                 * for future reference.
                else if (metaData.version == 2.0)
                {
                    byte[] buff = new byte[numPoints * 4];
                    float[] f32 = new float[buff.Length / 4];

                    int taipsais = (pointAttribute as PointAttributeV2_0).typeSize;

                    double localOffset = 0;
                    double scale = 1;

                    // compute offset and scale to pack larger types into 32 bit floats
                    if ((pointAttribute as PointAttributeV2_0).typeSize > 4)
                    {
                        long[] aminmax = (pointAttribute as PointAttributeV2_0).range;
                        localOffset = aminmax[0];
                        scale = 1 / (aminmax[1] - aminmax[0]);
                        // this linq gymnastics is necessary for "future" types that have multiple values in minmax arrays. like "position".
                        scale = 1 / ((pointAttribute as PointAttributeV2_0).max.OrderByDescending(f => f).First() - (pointAttribute as PointAttributeV2_0).min.OrderBy(f => f).First());
                    }
                }
                */
                offset += metaData.version == "2.0" ? (pointAttribute as PointAttributeV2_0).byteSize : toSetOff;
            }
            node.SetPoints(vertices, colors);
        }
        private static void CalculateRGBA(ref Color[] colors, ref int offset, byte[] data, int pointByteSize, int numPoints, bool alpha)
        {
            int size = alpha ? 4 : 3;

            for (int j = 0; j < numPoints; j++)
            {
                int pointOffset = j * pointByteSize;

                UInt16 r = BitConverter.ToUInt16(data, pointOffset + offset + 0);
                UInt16 g = BitConverter.ToUInt16(data, pointOffset + offset + 2);
                UInt16 b = BitConverter.ToUInt16(data, pointOffset + offset + 4);

                // ~~~ !!! hardcoded alphaville !!! ~~~
                // although its called RGBA theres no alpha. so..
                colors[j] = new Color32((byte)(r >> 8), (byte)(g >> 8), (byte)(b >> 8), (byte)255);     //<< 8: Move from [0, 65535] to [0, 255]
            }
        }
        /* Finds a file for a node in the hierarchy.
         * Assuming hierarchyStepSize is 3 and we are looking for the file 0123456765.bin, it is in:
         * 012/012345/012345676/r0123456765.bin
         * 012/345/676/r012345676.bin
         */
        private static byte[] FindAndLoadFile(string dataRPath, PointCloudMetaData metaData, string id, string fileending) {
            int levels = id.Length / metaData.hierarchyStepSize;
            string path = "";
            for (int i = 0; i < levels; i++) {
                path += id.Substring(i * metaData.hierarchyStepSize, metaData.hierarchyStepSize) + "/";
            }
            path += "r" + id + fileending;
            if (File.Exists(metaData.cloudPath + dataRPath + path)){
                return File.ReadAllBytes(metaData.cloudPath + dataRPath + path);
            }else if(metaData.cloudUrl != null){
                Directory.CreateDirectory(Path.GetDirectoryName(metaData.cloudPath + dataRPath + path));
                WebClient webClient = new WebClient();
                webClient.DownloadFile(metaData.cloudUrl + dataRPath + path, metaData.cloudPath + dataRPath + path);
                return File.ReadAllBytes(metaData.cloudPath + dataRPath + path);
            }
            return null;
        }
        /// <summary>
        /// used only for Potree v2. for now.
        /// </summary>
        /// <param name="fileNameWithPath"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private static byte[] ReadFromFile(string fileNameWithPath, long offset, UInt64 size)
        {
            if (size == 0)
            {
                return new byte[] { };
            }
            byte[] returnable = new byte[size];

            if (File.Exists(fileNameWithPath))
            {
                using FileStream stream = File.OpenRead(fileNameWithPath);
                stream.Seek(offset, SeekOrigin.Begin);
                stream.Read(returnable, 0, (int)size);
                stream.Close();
            }

            return returnable;
        }
        /* Loads the points for that node and all its children
         */
        private static uint LoadAllPoints(string dataRPath, PointCloudMetaData metaData, Node node) {
            LoadPoints(dataRPath, metaData, node);
            uint numpoints = (uint)node.PointCount;
            for (int i = 0; i < 8; i++) {
                if (node.HasChild(i)) {
                    numpoints += LoadAllPoints(dataRPath, metaData, node.GetChild(i));
                }
            }
            return numpoints;
        }

        public static uint LoadAllPointsForNode(Node node) {
            string dataRPath = node.MetaData.octreeDir + "/r/";
            return LoadAllPoints(dataRPath, node.MetaData, node);
        }
    }
}
