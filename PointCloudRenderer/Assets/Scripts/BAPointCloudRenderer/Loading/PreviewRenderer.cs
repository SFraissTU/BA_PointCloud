using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using BAPointCloudRenderer.CloudData;
using BAPointCloudRenderer.ObjectCreation;
using BAPointCloudRenderer.DataStructures;
using BAPointCloudRenderer.CloudController;

namespace BAPointCloudRenderer.Loading
{
    class PreviewRenderer : AbstractRenderer
    {
        private AbstractPointCloudSet set;
        private List<Tuple<Node,PointCloudLoader>> nodes = new List<Tuple<Node, PointCloudLoader>>();
        private RandomAccessQueue<Tuple<Node, PointCloudLoader>> toLoad = new RandomAccessQueue<Tuple<Node, PointCloudLoader>>();
        private RandomAccessQueue<Tuple<Node, PointCloudLoader>> toDisplay = new RandomAccessQueue<Tuple<Node, PointCloudLoader>>();
        private Thread loadingThread = null;
        private bool loadingthreadactive = false;
        private Material material;

        private int pointBudget = 65000;

        public PreviewRenderer(AbstractPointCloudSet set)
        {
            this.set = set;
            material = new Material(Shader.Find("Custom/PointShader"));
        }

        public void AddRootNode(Node n, PointCloudLoader loader)
        {
            Tuple<Node, PointCloudLoader> pair = new Tuple<Node, PointCloudLoader>(n, loader);
            lock (toLoad)
            {
                toLoad.Enqueue(pair);
            }
            if (loadingThread == null)
            {
                loadingThread = new Thread(Load);
                loadingthreadactive = true;
                loadingThread.Start();
            }
        }

        public void Display()
        {
            throw new NotImplementedException();
        }

        public uint GetPointCount()
        {
            throw new NotImplementedException();
        }

        public int GetRootNodeCount()
        {
            return nodes.Count;
        }

        public void Hide()
        {
            RemoveMeshes();
        }

        public void RemoveRootNode(Node n, PointCloudLoader loader)
        {
            Tuple<Node, PointCloudLoader> tuple = new Tuple<Node, PointCloudLoader>(n, loader);
            try {
                lock (toLoad)
                {
                    toLoad.Remove(tuple);
                }
            } catch (InvalidOperationException ex) { }
            lock (nodes)
            {
                nodes.Remove(tuple);
            }
            MeshFilter mf = loader.GetComponent<MeshFilter>();
            if (mf != null)
            {
                UnityEngine.Object.DestroyImmediate(mf.sharedMesh);
                mf.mesh = null;
                UnityEngine.Object.DestroyImmediate(mf);
                UnityEngine.Object.DestroyImmediate(loader.GetComponent<MeshRenderer>());
            }
        }

        public void ShutDown()
        {
            loadingthreadactive = false;
            lock (toLoad) {
                toLoad.Clear();
            }
            lock (nodes)
            {
                RemoveMeshes();
                nodes.Clear();
            }
            if (loadingThread != null)
            {
                loadingThread.Join();
            }
        }
        
        public void Update()
        {
            //Check if pointPreview has been disabled for a node
            lock (nodes)
            {
                for (int i = 0; i < nodes.Count; ++i)
                {
                    Tuple<Node, PointCloudLoader> t = nodes[i];
                    PointCloudLoader loader = t.Item2;
                    if (!loader.pointPreview)
                    {
                        Debug.Log("Removing Node from Preview");
                        nodes.Remove(t);
                        --i;
                        t.Item1.ForgetPoints();
                        MeshFilter mf = loader.GetComponent<MeshFilter>();
                        if (mf != null)
                        {
                            UnityEngine.Object.DestroyImmediate(mf.sharedMesh);
                            mf.mesh = null;
                            UnityEngine.Object.DestroyImmediate(mf);
                            UnityEngine.Object.DestroyImmediate(loader.GetComponent<MeshRenderer>());
                        }
                        lock (toLoad)
                        {
                            toLoad.Enqueue(t);
                        }
                    }
                }
            }
        }

        public void UpdatePreview()
        {
            lock (toDisplay)
            {
                if (!toDisplay.IsEmpty())
                {
                    toDisplay.Clear();
                }
            }
            CreateMesh();
        }

        private void Load()
        {
            while (loadingthreadactive)
            {
                Monitor.Enter(toLoad);
                if (toLoad.IsEmpty())
                {
                    Monitor.Exit(toLoad);
                }
                else
                {
                    Tuple<Node, PointCloudLoader> t = toLoad.Dequeue();
                    Monitor.Exit(toLoad);
                    if (t.Item2.pointPreview)
                    {
                        CloudLoader.LoadPointsForNode(t.Item1);
                        lock (nodes)
                        {
                            nodes.Add(t);
                        }
                        lock (toDisplay)
                        {
                            toDisplay.Enqueue(t);
                        }
                    } else
                    {
                        toLoad.Enqueue(t);
                    }
                }
            }
        }

        private void CreateMesh()
        {
            List<Tuple<PointCloudLoader, Vector3[], Color[]>> data = ChoosePoints();

            foreach (Tuple<PointCloudLoader, Vector3[], Color[]> cloud in data)
            {
                Vector3[] vertexData = cloud.Item2;
                Color[] colorData = cloud.Item3;

                GameObject go = cloud.Item1.gameObject;
                MeshFilter filter = go.GetComponent<MeshFilter>();
                Mesh mesh;
                if (filter == null)
                {
                    filter = go.AddComponent<MeshFilter>();
                    mesh = new Mesh();
                    filter.mesh = mesh;
                }
                else
                {
                    mesh = filter.sharedMesh;
                    if (mesh == null)
                    {
                        mesh = new Mesh();
                        filter.mesh = mesh;
                    }
                }
                MeshRenderer renderer = go.GetComponent<MeshRenderer>();
                if (renderer == null)
                {
                    renderer = go.AddComponent<MeshRenderer>();
                }
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.material = material;

                if (vertexData.Length == 0)
                {
                    filter.mesh = null;
                }
                else
                {
                    int[] indecies = new int[vertexData.Length];
                    for (int i = 0; i < vertexData.Length; ++i)
                    {
                        indecies[i] = i;
                    }
                    mesh.Clear();
                    mesh.vertices = vertexData;
                    mesh.colors = colorData;
                    mesh.SetIndices(indecies, MeshTopology.Points, 0);
                }
            }
        }

        private List<Tuple<PointCloudLoader, Vector3[], Color[]>> ChoosePoints()
        {
            lock (nodes)
            {
                List<Tuple<PointCloudLoader, Vector3[], Color[]>> result = new List<Tuple<PointCloudLoader, Vector3[], Color[]>>();
                //Here we need to subsample up to 65.000 points from the nodes
                //Such that all nodes are more or less equally represented
                int sumpoints = 0;  //Sum of points in all nodes
                int[] assignedPointCounts = new int[nodes.Count];   //Assigned Count for each node (Assigned = will be displayed)
                int[] remainingPointCounts = new int[nodes.Count];  //Not-yet-Assigned Count for each node
                int minPC = pointBudget;  //Smallest point count of a node
                int j = 0;
                //Initialize sumpoints, remainingPointCounts and minPC
                foreach (Tuple<Node,PointCloudLoader> t in nodes)
                {
                    Node n = t.Item1;
                    sumpoints += n.PointCount;
                    remainingPointCounts[j] = Math.Min(n.PointCount,65000);
                    minPC = Math.Min(minPC, remainingPointCounts[j]);
                    ++j;
                }
                int remainingNodeCount = nodes.Count;   //The count of nodes that still have unassigned points
                int currentPointCount = 0; //The number of points that are assigned
                int finalsumpoints = Math.Min(sumpoints, pointBudget); //Number of points we'll display eventually
                //As long as we still need to assign more points
                while (currentPointCount < finalsumpoints)
                {
                    //Find a value that we can reduce from each remainingPointCount without exceeding the limit
                    //The smallest value of: Smallest remaining point count, remaining point count to fill up divided by the number of remaining nodes
                    int reduce = Math.Min(minPC, (finalsumpoints - currentPointCount) / remainingNodeCount);
                    if (reduce == 0) reduce = 1;
                    //Reduce each remainingPointCount by this value
                    for (j = 0; j < remainingPointCounts.Length && currentPointCount < finalsumpoints; ++j)
                    {
                        //if it's still remaining
                        if (remainingPointCounts[j] != 0)
                        {
                            remainingPointCounts[j] -= reduce;
                            assignedPointCounts[j] += reduce;
                            currentPointCount += reduce;
                            if (remainingPointCounts[j] == 0)
                            {
                                --remainingNodeCount;
                            }
                            else
                            {
                                minPC = Math.Min(minPC, remainingPointCounts[j]);
                            }
                        }
                    }
                }
                //Build Vertices-Array
                j = 0;
                foreach (Tuple<Node, PointCloudLoader> t in nodes)
                {
                    Node n = t.Item1;
                    Vector3[] nodeVertices = n.VerticesToStore;
                    Color[] nodeColors = n.ColorsToStore;
                    Vector3[] filteredVertices = new Vector3[assignedPointCounts[j]];
                    Color[] filteredColors = new Color[assignedPointCounts[j]];
                    int stride = n.PointCount / assignedPointCounts[j];
                    Vector3 translation = n.BoundingBox.Min().ToFloatVector();
                    for (int newIndex = 0, oldIndex = 0; newIndex < assignedPointCounts[j]; oldIndex += stride, ++newIndex)
                    {
                        filteredVertices[newIndex] = nodeVertices[oldIndex] + translation;
                        filteredColors[newIndex] = nodeColors[oldIndex];
                    }
                    result.Add(new Tuple<PointCloudLoader, Vector3[], Color[]>(t.Item2, filteredVertices, filteredColors));
                    ++j;
                }
                return result;
            }
        }

        private void RemoveMeshes()
        {
            if (set.GetComponent<MeshFilter>() != null)
            {
                UnityEngine.Object.DestroyImmediate(set.GetComponent<MeshFilter>().sharedMesh);
                set.GetComponent<MeshFilter>().sharedMesh = null;
            }
        }
    }
}
