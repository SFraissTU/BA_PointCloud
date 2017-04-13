using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using DataStructures;

/* While PointCloudLoaderController will load the complete file as one, the DynamicLoaderController will first just load the hierarchy and load only the important nodes when pressing a key
 */
public class DynamicLoaderController : MonoBehaviour {

    //Path to the folder in which the cloud.js is
    public string cloudPath;
    //Defines the type of PointCloud (Points, Quads, Circles)
    public MeshConfiguration meshConfiguration;
    //Move bounding box to origin
    public bool moveToOrigin;
    //Min-Node-Size on screen
    public double minNodeSize;
    //Point-Budget
    public uint pointBudget;

    private PointCloudMetaData metaData;
    private Node rootNode;

    private PriorityQueue<double, Node> toRender = new ListPriorityQueue<double, Node>();//new DictionaryQueue<double,Node>();
    private ThreadSafeQueue<Node> toDelete = new ThreadSafeQueue<Node>();

    private Status status = Status.INITIALIZED;
    private bool shuttingDown;
    
    private float screenHeight;
    private float fieldOfView;
    private Vector3 cameraPositionF;
    private Plane[] frustum;
    private Camera userCamera;


    // Use this for initialization
    void Start () {
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
            if (GeometryUtility.TestPlanesAABB(frustum, currentNode.BoundingBox.ToBounds()))
            {
                double distance = currentNode.BoundingBox.Center().distance(cameraPosition); //TODO: Maybe other point?
                double slope = Math.Tan(fieldOfView / 2 * (Math.PI / 180));
                double projectedSize = (screenHeight / 2.0) * radius / (slope * distance);
                //Debug.Log("Radius = " + radius + ", Distance = " + distance + ", Slope = " + slope + ", fov: " + fieldOfView +  ", projectedSize = " + projectedSize);
                //TODO: Include centrality into priority
                if (projectedSize >= minNodeSize)
                {
                    toRender.Enqueue(currentNode, projectedSize);
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
                        toDelete.Enqueue(currentNode);
                    }
                }
            }
            else
            {
                if (currentNode.HasGameObjects())
                {
                    toDelete.Enqueue(currentNode);
                }
            }
        }
        SetStatus(Status.LOADING_AND_RENDERING);
    }

    void LoadRenderingPoints()
    {
        Debug.Log("Nodes in queue: " + toRender.Count);
        uint renderingPoints = 0;
        foreach (Node n in toDelete)
        {
            CloudLoader.UnloadPointsForNode(n);
        }
        foreach (Node n in toRender)
        {
            if (shuttingDown) return;
            CloudLoader.LoadPointsForNode(cloudPath, metaData, n);
            int amount = n.VerticesToStore.Length;  //TODO: PointCount currently not available. Fix after fixing of converter
            if (renderingPoints + amount < pointBudget)
            {
                renderingPoints += (uint)amount;
                if (!n.HasGameObjects())
                {
                    n.IsReadyForGameObjectCreation = true;
                }
                //Debug.Log("Loaded points for node " + n.Name);
            } else
            {
                CloudLoader.UnloadPointsForNode(n);
                toRender.Remove(n); //TODO: Very ugly, fix with converter fix
                if (n.HasGameObjects())
                {
                    toDelete.Enqueue(n);
                }
                //Debug.Log("Unloading points for node " + n.Name);
            }
        }
        SetStatus(Status.ONLY_RENDERING);
    }

    //TODO: End Thread when game ends (?)

    void CreateNextNodeGameObjects()
    {
        int MAX_NODES_CREATE_PER_FRAME = 5;
        int MAX_NODES_DELETE_PER_FRAME = 3;
        for (int i = 0; i < MAX_NODES_CREATE_PER_FRAME && !toRender.IsEmpty() && !shuttingDown; i++)
        {
            Node n = toRender.Peek();
            if (!n.IsReadyForGameObjectCreation && !n.HasGameObjects())
            {
                break;
            }
            toRender.Dequeue();
            //Debug.Log("Creating GO for node " + n.Name + ", next one: " + toRender.Peek()); 
            if (!n.HasGameObjects())
            {
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
	void Update ()
    {
        screenHeight= userCamera.pixelRect.height;
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
            CreateNextNodeGameObjects();
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
        ONLY_RENDERING
    }
}
