using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/* Resembles a node of the nested octree
 */
public class Node : IEnumerable<Node>
{
    private string name;//filename without the r. for example 073
    private BoundingBox boundingBox;
    //Values that are yet to be created in a mesh. Set in the constructor. Set to null after createGameObjects
    private Vector3[] verticesToStore;
    private Color[] colorsToStore;
    private Node[] children = new Node[8];
    private Node parent;
    private uint pointCount;
    private bool isReadyForGameObjectCreation = false;

    //List containing the gameobjects resembling this node
    private List<GameObject> gameObjects = new List<GameObject>();

    public Node(string name, BoundingBox boundingBox, Node parent)
    {
        this.name = name;
        this.boundingBox = boundingBox;
        this.parent = parent;
    }

    public int GetLevel()
    {
        return name.Length;
    }

    //Creates the Game Object(s) containing the points of this node
    //Does not happen in the constructor, as gameobjects should be created on the main thread (valled via update)
    //Set verticesToStore and colorsToStore before calling this!
    public void CreateGameObjects(MeshConfiguration configuration)
    {
        if (!isReadyForGameObjectCreation)
        {
            throw new InvalidOperationException("Not ready for GameObject creation");// ArgumentException("GameObjects already created!");
        }
        if (gameObjects.Count != 0)
        {
            throw new ArgumentException("GameObjects already created!");
        }
        if (verticesToStore == null || colorsToStore == null)
        {
            throw new ArgumentException("No point data stored!");
        }
        /*if (verticesToStore.Length < pointcount || colorsToStore.Length < pointcount)
        {
            throw new ArgumentException("To few points stored! Should be: " + pointcount + ", are: " + verticesToStore.Length);
        }*/
        int max = configuration.GetMaximumPointsPerMesh();
        int amount = Math.Min(max, verticesToStore.Length);        //Typecast: As max is an int, the value cannot be out of range
        int index = 0; //name index
        while (amount > 0)
        {
            Vector3[] vertices = verticesToStore.Take(amount).ToArray();
            Color[] colors = colorsToStore.Take(amount).ToArray(); ;
            verticesToStore = verticesToStore.Skip(amount).ToArray();
            colorsToStore = colorsToStore.Skip(amount).ToArray();
            gameObjects.Add(configuration.CreateGameObject("r" + name + "_" + index, vertices, colors, boundingBox));
            amount = Math.Min(max, verticesToStore.Length);
            index++;
        }
        IsReadyForGameObjectCreation = false;
    }

    public void CreateBoundingBoxGameObject()
    {
        GameObject box = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/BoundingBoxPrefab"));
        box.transform.Translate((boundingBox.Min() + (boundingBox.Size() / 2)).ToFloatVector());
        box.transform.localScale = boundingBox.Size().ToFloatVector();
    }

    /* As CreateGameObjects, but it also creates the gameobjects of the children recursively
     */
    public void CreateAllGameObjects(MeshConfiguration configuration)
    {
        CreateGameObjects(configuration);
        for (int i = 0; i < 8; i++)
        {
            if (children[i] != null)
            {
                children[i].CreateAllGameObjects(configuration);
            }
        }
    }

    /* Removes the gameobjects again
     */
    public void RemoveGameObjects()
    {
        foreach (GameObject go in gameObjects) {
            UnityEngine.Object.Destroy(go);
        }
        gameObjects.Clear();
    }

    public bool HasGameObjects()
    {
        return gameObjects.Count != 0;
    }

    public void SetChild(int index, Node node)
    {
        children[index] = node;
    }

    public Node GetChild(int index)
    {
        return children[index];
    }

    public bool HasChild(int index)
    {
        return children[index] != null;
    }

    public IEnumerator<Node> GetEnumerator()
    {
        return new ChildEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator(); //...?
    }

    private class ChildEnumerator : IEnumerator<Node>
    {
        Node outer;

        public ChildEnumerator(Node n)
        {
            outer = n;
        }

        int nextIndex = -1;

        public Node Current
        {
            get
            {
                if (nextIndex < 0 || nextIndex >= 8)
                {
                    throw new InvalidOperationException();
                }
                return outer.children[nextIndex];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            do
            {
                ++nextIndex;
            }
            while (nextIndex < 8 && outer.children[nextIndex] == null) ;
            if (nextIndex == 8) return false;
            return true;
        }

        public void Reset()
        {
            nextIndex = -1;
        }
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

    public string Name
    {
        get { return name;  }
    }

    public BoundingBox BoundingBox
    {
        get
        {
            return boundingBox;
        }
    }

    public Node Parent
    {
        get
        {
            return parent;
        }

        set
        {
            parent = value;
        }
    }

    public uint PointCount
    {
        get
        {
            return pointCount;
        }
        set
        {
            pointCount = value;
        }
    }

    public bool IsReadyForGameObjectCreation
    {
        get
        {
            return isReadyForGameObjectCreation;
        }

        set
        {
            isReadyForGameObjectCreation = value;
        }
    }

    public override string ToString()
    {
        return "Node: r" + Name;
    }
}

