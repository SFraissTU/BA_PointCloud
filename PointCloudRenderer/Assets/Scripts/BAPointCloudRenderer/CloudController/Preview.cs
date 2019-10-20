using System;
using System.Threading;
using UnityEngine;
using UnityEditor;

using BAPointCloudRenderer.CloudData;
using BAPointCloudRenderer.Loading;
using System.Collections.Generic;

namespace BAPointCloudRenderer.CloudController
{
    [ExecuteAlways]
    public class Preview : MonoBehaviour
    {
        private List<PointCloudLoader> _loaders = null;
        private List<Node> _nodes = null;
        private BoundingBox _currentBB = null;
        private Vector3 _setPosition;
        private AbstractPointCloudSet _setToPreview;
        private bool _showPoints;
        private int _pointBudget;
        private Material _material;
        private bool _createMesh = false;

        public AbstractPointCloudSet SetToPreview;
        public bool ShowPoints = false;
        public int PointBudget = 65000;

        public void Start()
        {
            //EditorApplication.update = DrawBoundingBox;
            _material = new Material(Shader.Find("Custom/PointShader"));
        }

        public void UpdatePreview()
        {
            if (SetToPreview == null)
            {
                Debug.Log("No PointCloudSet given. Preview aborted.");
                return;
            }
            if (_loaders != null)
            {
                Debug.Log("Please wait until last preview is ready! Preview aborted.");
                return;
            }
            _setToPreview = SetToPreview;
            _showPoints = ShowPoints;
            _setPosition = _setToPreview.transform.position;
            _pointBudget = PointBudget;
            PointCloudLoader[] allLoaders = FindObjectsOfType<PointCloudLoader>();
            _loaders = new List<PointCloudLoader>();
            _nodes = new List<Node>();
            for (int i = 0; i < allLoaders.Length; ++i)
            {
                if (allLoaders[i].enabled && allLoaders[i].setController == _setToPreview)
                {
                    _loaders.Add(allLoaders[i]);
                }
                MeshFilter mf = allLoaders[i].GetComponent<MeshFilter>();
                MeshRenderer mr = allLoaders[i].GetComponent<MeshRenderer>();
                if (mf != null)
                {
                    DestroyImmediate(mf);
                }
                if (mr != null)
                {
                    DestroyImmediate(mr);
                }
            }
            Thread thread = new Thread(LoadBoundingBoxes);
            thread.Start();
        }

        private void LoadBoundingBoxes()
        {
            BoundingBox overallBoundingBox = new BoundingBox(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,
                                                                    double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);
            foreach (PointCloudLoader loader in _loaders)
            {
                string path = loader.cloudPath;
                if (!path.EndsWith("/"))
                {
                    path += "/";
                }
                PointCloudMetaData metaData = CloudLoader.LoadMetaData(path, false);
                BoundingBox currentBoundingBox = metaData.tightBoundingBox;
                overallBoundingBox.Lx = Math.Min(overallBoundingBox.Lx, currentBoundingBox.Lx);
                overallBoundingBox.Ly = Math.Min(overallBoundingBox.Ly, currentBoundingBox.Ly);
                overallBoundingBox.Lz = Math.Min(overallBoundingBox.Lz, currentBoundingBox.Lz);
                overallBoundingBox.Ux = Math.Max(overallBoundingBox.Ux, currentBoundingBox.Ux);
                overallBoundingBox.Uy = Math.Max(overallBoundingBox.Uy, currentBoundingBox.Uy);
                overallBoundingBox.Uz = Math.Max(overallBoundingBox.Uz, currentBoundingBox.Uz);

                if (_showPoints)
                {
                    Node rootNode = new Node("", metaData, metaData.boundingBox, null);
                    CloudLoader.LoadPointsForNode(rootNode);
                    _nodes.Add(rootNode);
                }
            }
            if (_setToPreview.moveCenterToTransformPosition)
            {
                Vector3d moving = new Vector3d(_setPosition) - overallBoundingBox.Center();
                overallBoundingBox.MoveAlong(moving);
                foreach (Node n  in _nodes)
                {
                    n.BoundingBox.MoveAlong(moving);
                }
            }
            _currentBB = overallBoundingBox;
            if (_showPoints)
            {
                _createMesh = true;
            } else
            {
                _loaders = null;
                _nodes = null;
            }
        }

        public void OnDrawGizmos()
        {
            if (_createMesh)
            {
                CreateMesh();
                _createMesh = false;
                _loaders = null;
                _nodes = null;
            }
            DrawBoundingBox();
        }

        public void DrawBoundingBox()
        {
            if (_currentBB != null)
            {
                //Utility.BBDraw.DrawBoundingBox(currentBB, null, Color.cyan, true, 0.1f);
                Utility.BBDraw.DrawBoundingBoxInEditor(_currentBB, null);
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
                renderer.material = _material;

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
            List<Tuple<PointCloudLoader, Vector3[], Color[]>> result = new List<Tuple<PointCloudLoader, Vector3[], Color[]>>();
            //Here we need to subsample up to 65.000 points from the nodes
            //Such that all nodes are more or less equally represented
            int sumpoints = 0;  //Sum of points in all nodes
            int[] assignedPointCounts = new int[_nodes.Count];   //Assigned Count for each node (Assigned = will be displayed)
            int[] remainingPointCounts = new int[_nodes.Count];  //Not-yet-Assigned Count for each node
            int minPC = _pointBudget;  //Smallest point count of a node
            int j = 0;
            //Initialize sumpoints, remainingPointCounts and minPC
            foreach (Node n in _nodes)
            {
                sumpoints += n.PointCount;
                remainingPointCounts[j] = Math.Min(n.PointCount, 65000);
                minPC = Math.Min(minPC, remainingPointCounts[j]);
                ++j;
            }
            int remainingNodeCount = _nodes.Count;   //The count of nodes that still have unassigned points
            int currentPointCount = 0; //The number of points that are assigned
            int finalsumpoints = Math.Min(sumpoints, _pointBudget); //Number of points we'll display eventually
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
            foreach (Node n in _nodes)
            {
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
                result.Add(new Tuple<PointCloudLoader, Vector3[], Color[]>(_loaders[j], filteredVertices, filteredColors));
                ++j;
            }
            return result;
        }
    }
}
