using MeshConfigurations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CloudData
{
    /* Resembles a node of the nested octree
    */
    public class Node : IEnumerable<Node>
    {
        //filename without the r. for example 073. identifieing the node in the tree
        private string name;
        //BoundingBox of this node
        private BoundingBox boundingBox;
        //The vertices this node is containing. Null before being set. Will be set to null after creating gameobjects
        private Vector3[] verticesToStore;
        //The colors for the nodes. colors and vertices should always have the same length! Null before being set. Will be set to null after creating gameobjects
        private Color[] colorsToStore;
        //Array of children. May contain null-elements if no child on this index exists
        private Node[] children = new Node[8];
        //Parent-element. May be null at the root.
        private Node parent;
        //PointCount, read from hierarchy-file
        private uint pointCount = 0;
        //This flag is true iff vertices and colors to store exist, no game objects exist and SetReadyForGameObjectCreation() has been called
        //If this is true, CreateGameObjects can be called
        private bool readyFlag = false;

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
        //A gameobject can be created, if no gameobject is created yet, vertices and colors are set, and SetReadyForGameObjectCreation() has been called
        public void CreateGameObjects(MeshConfiguration configuration)
        {
            if (!readyFlag)
            {
                throw new InvalidOperationException("Not ready for GameObject creation!");
            }
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
            verticesToStore = null;
            colorsToStore = null;
            readyFlag = false;
        }

        /* Creates a box game object with the shape of the bounding box of this node
         */
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

        /* Removes the gameobjects again. To create them again, you once again have to set the vertices and colors and call SetReadyForGameObjectCreation()
         */
        public void RemoveGameObjects()
        {
            foreach (GameObject go in gameObjects)
            {
                UnityEngine.Object.Destroy(go);
            }
            gameObjects.Clear();
        }

        /* Wether CreateGameObjects can be called without an exception.
         * GameObjects can be created if no gameObjects for this node already exist, vertices and colors are set, and SetReadyForGameObjectCreation() has been called
         */
        public bool IsReadyForGameObjectCreation()
        {
            return readyFlag && gameObjects.Count == 0 && verticesToStore != null && colorsToStore != null;
        }

        /* Enables the creation of GameObjects, iff no GameObjects already exist and vertices and colors are set
         */
        public void SetReadyForGameObjectCreation()
        {
            if (gameObjects.Count != 0)
            {
                throw new ArgumentException("GameObjects already created!");
            }
            if (verticesToStore == null || colorsToStore == null) //TODO: PointCount
            {
                throw new ArgumentException("No or invalid point data stored!");
            }
            readyFlag = true;
        }

        /* True, iff no GameObjects are existing yet and at least a call to SetReadyForGameObjectCreation() is needed to enable creating them
         */
        public bool IsWaitingForReadySet()
        {
            return gameObjects.Count == 0 && !readyFlag;
        }

        /* Sets the point data to be stored.
         * Throws an exception if gameobjects already exist or vertices or colors are null or their length do not match
         */
        public void SetPoints(Vector3[] vertices, Color[] colors)
        {
            if (gameObjects.Count != 0)
            {
                throw new ArgumentException("GameObjects already created!");
            }
            if (vertices == null || colors == null || vertices.Length != colors.Length)
            {
                throw new ArgumentException("Invalid data given!");
            }
            verticesToStore = vertices;
            colorsToStore = colors;
            pointCount = (uint)vertices.Length;
        }

        /* Wether there are points which can be used for the creation of a game object
         */
        public bool HasPointsToRender()
        {
            return verticesToStore != null && colorsToStore != null;
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

        /* This enumerator enables enumerating through the children of this Node
         */
        public IEnumerator<Node> GetEnumerator()
        {
            return new ChildEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator(); //...?
        }

        //Enumerator for the children
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
                while (nextIndex < 8 && outer.children[nextIndex] == null);
                if (nextIndex == 8) return false;
                return true;
            }

            public void Reset()
            {
                nextIndex = -1;
            }
        }
        
        public string Name
        {
            get { return name; }
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

        //Number of points given the last time SetPoints was called. Or 0 if it hasn't been called
        public uint PointCount
        {
            get
            {
                return pointCount;
            }
        }
        
        public override string ToString()
        {
            return "Node: r" + Name;
        }
    }


}

