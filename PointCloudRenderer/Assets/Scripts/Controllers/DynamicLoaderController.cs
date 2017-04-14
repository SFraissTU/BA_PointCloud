using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using DataStructures;
using MeshConfigurations;
using CloudData;
using Loading;

namespace Controllers
{

    /* While PointCloudLoaderController will load the complete file as one, the DynamicLoaderController will first just load the hierarchy and load only the important nodes when pressing a key
     */
    public class DynamicLoaderController : MonoBehaviour
    {

        //-----Public Options-----
        //Path to the folder in which the cloud.js is
        public string cloudPath;
        //Defines the type of PointCloud (Points, Quads, Circles)
        public MeshConfiguration meshConfiguration;
        //If the cloud should be moved to the origin
        public bool moveToOrigin;
        //Min-Node-Size on screen in pixels
        public double minNodeSize;
        //Point-Budget
        public uint pointBudget;

        //-----Point-Cloud-Info-----
        private PointCloudMetaData metaData;
        private Node rootNode;

        //-----DataStructures for Rendering-----
        //Points that are supposed to be rendered. PriorityQueue
        private PriorityQueue<double, Node> toRender = new ListPriorityQueue<double, Node>();
        //Points that are supposed to be deleted. Normal Queue (but threadsafe)
        private ThreadSafeQueue<Node> toDelete = new ThreadSafeQueue<Node>();

        //-----Status-Info of the Rendering-Prozess-----
        //current Status
        private Status status = Status.INITIALIZED;
        //Wether the programm is exiting
        private bool shuttingDown;

        //-----Screen- and Camera-Info-----
        private Camera userCamera;
        private float screenHeight;
        private float fieldOfView;
        private Vector3 cameraPositionF;
        private Plane[] frustum;


        // Use this for initialization
        void Start()
        {
            Thread thread = new Thread(LoadHierarchy);
            thread.Start();
            userCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        }

        void LoadHierarchy()
        {
            try
            {
                SetStatus(Status.LOADING_HIERARCHY);
                Debug.Log("Loading Hierarchy");
                if (!cloudPath.EndsWith("\\"))
                {
                    cloudPath = cloudPath + "\\";
                }

                metaData = CloudLoader.LoadMetaData(cloudPath, moveToOrigin);

                rootNode = CloudLoader.LoadHierarchyOnly(cloudPath, metaData);

                Debug.Log("Finished Loading Hierachy");

                SetStatus(Status.READY_FOR_FILL);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        void FillRenderingQueue()
        {
            SetStatus(Status.FILLING_QUEUE);
            Vector3d cameraPosition = new Vector3d(cameraPositionF);
            toRender.Clear();
            Queue<Node> toCheck = new Queue<Node>();
            toCheck.Enqueue(rootNode);
            double radius = rootNode.BoundingBox.Radius();
            int lastLevel = rootNode.GetLevel();//= 0
                                                //Breitensuche
            while (toCheck.Count != 0 && !shuttingDown)
            {
                Node currentNode = toCheck.Dequeue();
                if (currentNode.GetLevel() != lastLevel)
                {
                    radius /= 2;
                    ++lastLevel;
                }

                //if (renderingPoints + currentNode.PointCount < pointBudget)   //TODO: PointCount currently not available. Fix after fixing of converter
                if (GeometryUtility.TestPlanesAABB(frustum, currentNode.BoundingBox.GetBoundsObject()))
                {
                    double distance = currentNode.BoundingBox.Center().distance(cameraPosition); //TODO: Maybe other point?
                    double slope = Math.Tan(fieldOfView / 2 * (Math.PI / 180));
                    double projectedSize = (screenHeight / 2.0) * radius / (slope * distance);
                    //Debug.Log("Radius = " + radius + ", Distance = " + distance + ", Slope = " + slope + ", fov: " + fieldOfView +  ", projectedSize = " + projectedSize);
                    //TODO: Include centrality into priority
                    if (projectedSize >= minNodeSize)
                    {
                        if (!currentNode.HasGameObjects())
                        {
                            toRender.Enqueue(currentNode, projectedSize);
                        }
                        //renderingPoints += currentNode.PointCount;
                        foreach (Node child in currentNode)
                        {
                            toCheck.Enqueue(child);
                        }
                    }
                    else
                    {
                        if (currentNode.HasGameObjects())
                        {
                            //Remove lower LOD-Objects first!
                            Queue<Node> childrenToCheck = new Queue<Node>();
                            Stack<Node> newNodesToDelete = new Stack<Node>();
                            newNodesToDelete.Push(currentNode);
                            foreach (Node child in currentNode)
                            {
                                childrenToCheck.Enqueue(child);
                            }
                            while (childrenToCheck.Count != 0)
                            {
                                Node child = childrenToCheck.Dequeue();
                                if (child.HasGameObjects())
                                {
                                    newNodesToDelete.Push(child);
                                    foreach (Node childchild in child)
                                    {
                                        childrenToCheck.Enqueue(childchild);
                                    }
                                }
                            }
                            while (newNodesToDelete.Count != 0)
                            {
                                toDelete.Enqueue(newNodesToDelete.Pop());
                            }
                        }
                    }
                }
                else
                {
                    //TODO: DUPLICATE CODE - UGLY
                    if (currentNode.HasGameObjects())
                    {
                        //Remove lower LOD-Objects first!
                        Queue<Node> childrenToCheck = new Queue<Node>();
                        Stack<Node> newNodesToDelete = new Stack<Node>();
                        newNodesToDelete.Push(currentNode);
                        foreach (Node child in currentNode)
                        {
                            childrenToCheck.Enqueue(child);
                        }
                        while (childrenToCheck.Count != 0)
                        {
                            Node child = childrenToCheck.Dequeue();
                            if (child.HasGameObjects())
                            {
                                newNodesToDelete.Push(child);
                                foreach (Node childchild in child)
                                {
                                    childrenToCheck.Enqueue(childchild);
                                }
                            }
                        }
                        while (newNodesToDelete.Count != 0)
                        {
                            toDelete.Enqueue(newNodesToDelete.Pop());
                        }
                    }
                }
            }
            SetStatus(Status.LOADING_AND_RENDERING);
        }

        void LoadRenderingPoints()
        {
            try
            {
                Debug.Log("Nodes in queue: " + toRender.Count);
                uint renderingPoints = 0;
                foreach (Node n in toRender)
                {
                    if (shuttingDown) return;
                    uint amount = n.PointCount;
                    //PointCount might already be sad from loading the points before
                    if (amount == 0)
                    {
                        CloudLoader.LoadPointsForNode(cloudPath, metaData, n);
                        amount = n.PointCount;
                    }
                    if (renderingPoints + amount < pointBudget)
                    {
                        renderingPoints += amount;
                        if (!n.HasPointsToRender())
                        {
                            CloudLoader.LoadPointsForNode(cloudPath, metaData, n);
                        }
                        if (!n.HasGameObjects())
                        {
                            n.SetReadyForGameObjectCreation();
                        }
                    }
                    else
                    {
                        toRender.Remove(n); //TODO: Very ugly, fix with converter fix
                        if (n.HasGameObjects())
                        {
                            toDelete.Enqueue(n);
                        }
                    }
                }
                SetStatus(Status.ONLY_RENDERING);
            } catch (Exception ex)
            {
                Debug.LogError(ex);
                SetStatus(Status.ERROR);
            }
        }


        void UpdateGameObjects()
        {
            int MAX_NODES_CREATE_PER_FRAME = 5;
            int MAX_NODES_DELETE_PER_FRAME = 3;
            for (int i = 0; i < MAX_NODES_CREATE_PER_FRAME && !toRender.IsEmpty() && !shuttingDown; i++)
            {
                Node n = toRender.Peek();
                if (n.IsWaitingForReadySet())
                {
                    break;
                }
                else if (n.IsReadyForGameObjectCreation())
                {
                    toRender.Dequeue();
                    n.CreateGameObjects(meshConfiguration);
                }
            }
            for (int i = 0; i < MAX_NODES_DELETE_PER_FRAME && !toDelete.IsEmpty() && !shuttingDown; i++)
            {
                toDelete.Dequeue().RemoveGameObjects();
            }
            if (status == Status.ONLY_RENDERING && toRender.IsEmpty() && toDelete.IsEmpty())
            {
                SetStatus(Status.READY_FOR_FILL);
            }
        }

        // Update is called once per frame
        void Update()
        {
            screenHeight = userCamera.pixelRect.height;
            cameraPositionF = userCamera.transform.position;
            fieldOfView = userCamera.fieldOfView;
            frustum = GeometryUtility.CalculateFrustumPlanes(userCamera);
            if (status == Status.READY_FOR_FILL && Input.GetKey(KeyCode.X))
            {
                FillRenderingQueue();
                new Thread(LoadRenderingPoints).Start();
            }
            else if (status == Status.LOADING_AND_RENDERING || status == Status.ONLY_RENDERING)
            {
                UpdateGameObjects();
            }
        }

        public void OnApplicationQuit()
        {
            shuttingDown = true;
        }

        private void SetStatus(Status status)
        {
            this.status = status;
            Debug.Log("Status set to: " + status.ToString());
        }

        enum Status
        {
            INITIALIZED,
            LOADING_HIERARCHY,
            READY_FOR_FILL,
            FILLING_QUEUE,
            LOADING_AND_RENDERING,
            ONLY_RENDERING,
            ERROR
        }
    }

}