using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/* Resembles the points in the node of a point cloud
 */
public class Node
{
    private string name;
    //Values that are yet to be created in a mesh. Set in the constructor. Set to null after createGameObjects
    private Vector3[] verticesToStore;
    private Color[] colorsToStore;

    //List containing the gameobjects resembling this node
    private List<GameObject> gameObjects = new List<GameObject>();

    public Node(string name)
    {
        this.name = name;
    }
    
    public Node(string name, Vector3[] verticesToStore, Color[] colorsToStore)
    {
        this.name = name;
        this.verticesToStore = verticesToStore;
        this.colorsToStore = colorsToStore;
    }

    //Does not happen in the constructor, as gameobjects should be created on the main thread (valled via update)
    public void CreateGameObjects(MeshConfiguration configuration)
    {
        if (verticesToStore == null || colorsToStore == null) return;
        int max = configuration.GetMaximumPointsPerMesh();
        int amount = Math.Min(max, verticesToStore.Length);
        int index = 0; //name index
        while (amount > 0)
        {
            Vector3[] vertices = verticesToStore.Take(amount).ToArray();
            Color[] colors = colorsToStore.Take(amount).ToArray(); ;
            verticesToStore = verticesToStore.Skip(amount).ToArray();
            colorsToStore = colorsToStore.Skip(amount).ToArray();
            gameObjects.Add(configuration.CreateGameObject(name + index, vertices, colors));
            amount = Math.Min(max, verticesToStore.Length);
            index++;
        }
        verticesToStore = null;
        colorsToStore = null;
    }

    public Vector3[] VerticesToStore
    {
        get
        {
            return verticesToStore;
        }

        set
        {
            verticesToStore = value;
        }
    }

    public Color[] ColorsToStore
    {
        get
        {
            return colorsToStore;
        }

        set
        {
            colorsToStore = value;
        }
    }
}

