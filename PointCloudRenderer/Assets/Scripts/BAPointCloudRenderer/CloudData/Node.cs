using BAPointCloudRenderer.ObjectCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BAPointCloudRenderer.CloudData {
    /// <summary>
    /// Resembles a node of the nested octree.
    /// </summary>
    public class Node : IEnumerable<Node> {

        //filename without the r. for example 073. identifying the node in the tree
        private string name;
        //MetaData of the cloud
        private PointCloudMetaData metaData;
        //BoundingBox of this node
        private BoundingBox boundingBox;
        //The vertices this node is containing. Null before being set.
        private Vector3[] verticesToStore;
        //The colors for the nodes. colors and vertices should always have the same length! Null before being set.
        private Color[] colorsToStore;
        //Array of children. May contain null-elements if no child on this index exists
        private Node[] children = new Node[8];
        //Parent-element. May be null at the root.
        private Node parent;
        //PointCount, if known. -1 before points have been read in
        private int pointCount = -1;

        //List containing the gameobjects resembling this node
        private List<GameObject> gameObjects = new List<GameObject>();
        internal UInt64
            byteOffset = 0,
            byteSize,
            hierarchyByteOffset,
            hierarchyByteSize,
            rootHierarchyByteSize = 0;
        internal int
            type = 0,
            level = 0,
            bytesPerNode = 22,
            rootLevel = 0,
            rootNodeType = 2,
            rootHierarchyByteOffset = 0;
        internal double spacing;
        internal uint numPoints;

        /// <summary>
        /// Creates a new node object.
        /// </summary>
        /// <param name="name">The path of the node in the tree. For example "023" for the forth child of the third child of the first child of the root node.</param>
        /// <param name="metaData">The meta data of the point cloud, to identify which cloud this node belongs to.</param>
        /// <param name="boundingBox">The bounding box of this node.</param>
        /// <param name="parent">The parent node. May be null if this is the root node.</param>
        public Node(string name, PointCloudMetaData metaData, BoundingBox boundingBox, Node parent) {
            this.name = name;
            this.metaData = metaData;
            this.boundingBox = boundingBox;
            this.parent = parent;
        }

        /// <summary>
        /// Returns how deep in the tree this node is. 0 means it is the root node, 1 means it is a child of the root node and so on...
        /// </summary>
        /// <returns>the level in the tree (>=0)</returns>
        public int GetLevel() {
            return name.Length;
        }
        
        /// <summary>
        /// Creates the Game Object(s) containing the points of this node.
        /// Vertices and Colors have to be set before calling this function (via SetPoints)! This function has to be called from the main thread!
        /// </summary>
        /// <param name="configuration">The MeshConfiguration which should be used for creating the Game Objects</param>
        public void CreateGameObjects(MeshConfiguration configuration, Transform parent) {
            int max = configuration.GetMaximumPointsPerMesh();
            if (verticesToStore.Length <= max) {
                gameObjects.Add(configuration.CreateGameObject(metaData.cloudName + "/" + "r" + name + " (" + verticesToStore.Length + ")", verticesToStore, colorsToStore, boundingBox, parent, metaData.version, metaData.getAdditionalTranslation()));
            } else {
                int amount = Math.Min(max, verticesToStore.Length);
                int index = 0; //name index
                Vector3[] restVertices = verticesToStore;
                Color[] restColors = colorsToStore;
                while (amount > 0) {
                    Vector3[] vertices = restVertices.Take(amount).ToArray();
                    Color[] colors = restColors.Take(amount).ToArray(); ;
                    restVertices = restVertices.Skip(amount).ToArray();
                    restColors = restColors.Skip(amount).ToArray();
                    gameObjects.Add(configuration.CreateGameObject(metaData.cloudName + "/" + "r" + name + "_" + index + " (" + vertices.Length + ")", vertices, colors, boundingBox, parent, metaData.version, metaData.getAdditionalTranslation()));
                    amount = Math.Min(max, vertices.Length);
                    index++;
                }
            }
        }
        
         /// <summary>
         /// Creates a transparent box game object with the shape of the bounding box of this node. The color is determined by the hashcode of this node.
         /// Used for debugging purposes only.
         /// </summary>
        public GameObject CreateBoundingBoxGameObject() {
            GameObject box = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/BoundingBoxPrefab"));
            int h = GetHashCode();
            box.GetComponent<MeshRenderer>().material.color = new Color(((float)h / int.MaxValue), (h % 10000) / 10000f, (h % 100) / 100f, 0.472f);
            box.transform.Translate((boundingBox.Min() + (boundingBox.Size() / 2)).ToFloatVector());
            box.transform.localScale = boundingBox.Size().ToFloatVector();
            return box;
        }

        /// <summary>
        /// Creates GameObjects for this node and all its (grand-)children. Useable for displaying a whole point cloud at once.
        /// Vertices and Colors have to be set before calling this function (via SetPoints) for this object and all its children! This function has to be called from the main thread!
        /// </summary>
        /// <param name="configuration">The MeshConfiguration which should be used for creating the Game Objects</param>
        public void CreateAllGameObjects(MeshConfiguration configuration, Transform parent) {
            CreateGameObjects(configuration, parent);
            for (int i = 0; i < 8; i++) {
                if (children[i] != null) {
                    children[i].CreateAllGameObjects(configuration, parent);
                }
            }
        }

        /// <summary>
        /// Removes the GameObjects of this node. Has to be called from the main thread.
        /// </summary>
        /// <param name="config">The MeshConfiguration which should be used for removing the Game Objects</param>
        public void RemoveGameObjects(MeshConfiguration config) {
            foreach (GameObject go in gameObjects) {
                config.RemoveGameObject(go);
            }
            gameObjects.Clear();
        }

        /// <summary>
        /// Removes the GameObjects of this node and all it's children. Has to be called from the main thread.
        /// </summary>
        /// <param name="config">The MeshConfiguration which should be used for removing the Game Objects</param>
        public void RemoveAllGameObjects(MeshConfiguration configuration) {
            RemoveGameObjects(configuration);
            for (int i = 0; i < 8; i++) {
                if (children[i] != null) {
                    children[i].RemoveAllGameObjects(configuration);
                }
            }
        }



        /// <summary>
        /// Deactivates the GameObjects of this node. Has to be called from the main thread.
        /// </summary>
        public void DeactivateGameObjects() {
            foreach (GameObject go in gameObjects) {
                go.SetActive(false);
            }
        }

        /// <summary>
        /// Deactivates the GameObjects of this node and of all its children. Has to be called from the main thread.
        /// </summary>
        public void DeactivateAllGameObjects() {
            DeactivateGameObjects();
            for (int i = 0; i < 8; i++) {
                if (children[i] != null) {
                    children[i].DeactivateAllGameObjects();
                }
            }
        }

        /// <summary>
        /// Reactivates the GameObjects of this node. Has to be called from the main thread.
        /// </summary>
        public void ReactivateGameObjects() {
            foreach (GameObject go in gameObjects) {
                go.SetActive(true);
            }
        }

        /// <summary>
        /// Deactivates the GameObjects of this node and of all its children. Has to be called from the main thread.
        /// </summary>
        public void ReactivateAllGameObjects() {
            ReactivateGameObjects();
            for (int i = 0; i < 8; i++) {
                if (children[i] != null) {
                    children[i].ReactivateAllGameObjects();
                }
            }
        }



        /// <summary>
        /// Sets the point data. Throws an exception if gameobjects already exist or vertices or colors are null or their length do not match.
        /// Also sets the point count.
        /// </summary>
        /// <param name="vertices">Position-Data</param>
        /// <param name="colors">Color-Data (has to have the same length as vertices)</param>
        public void SetPoints(Vector3[] vertices, Color[] colors) {
            if (gameObjects.Count != 0) {
                throw new ArgumentException("GameObjects already created!");
            }
            if (vertices == null || colors == null || vertices.Length != colors.Length) {
                throw new ArgumentException("Invalid data given!");
            }
            verticesToStore = vertices;
            colorsToStore = colors;
            pointCount = vertices.Length;
        }
        
         /// <summary>
         /// Deletes the loaded vertex- and color-information (to release the used memory). The point count stays saved however.
         /// </summary>
        public void ForgetPoints() {
            verticesToStore = null;
            colorsToStore = null;
        }
        
         /// <summary>
         /// Returns true, iff vertices and colors are set
         /// </summary>
        public bool HasPointsToRender() {
            return verticesToStore != null && colorsToStore != null && pointCount != -1;
        }

        /// <summary>
        /// Returns true, iff this object has GameObjects.
        /// </summary>
        public bool HasGameObjects() {
            return gameObjects.Count != 0;
        }

        public Vector3[] VerticesToStore {
            get { return verticesToStore; }
        }

        public Color[] ColorsToStore {
            get { return colorsToStore; }
        }

        /// <summary>
        /// Sets the child with the given index
        /// </summary>
        /// <param name="index">0 <= index < 8</param>
        /// <param name="node">Child node.</param>
        public void SetChild(int index, Node node) {
            children[index] = node;
        }

        /// <summary>
        /// Returns the child at the given index
        /// </summary>
        /// <param name="index">0 <= index < 8</param>
        /// <returns>The child (may be null, if no child exists at that index)</returns>
        public Node GetChild(int index) {
            return children[index];
        }

        /// <summary>
        /// Returns true, iff the node has a child at the given index.
        /// </summary>
        /// <param name="index">0 <= index < 8</param>
        public bool HasChild(int index) {
            return children[index] != null;
        }
        
         /// <summary>
         /// Returns an enumerator with which it is possible to enumerate through the children of the node (not including null-values)
         /// </summary>
         /// <returns></returns>
        public IEnumerator<Node> GetEnumerator() {
            return new ChildEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator(); //...?
        }

        //Enumerator for the children
        private class ChildEnumerator : IEnumerator<Node> {
            Node outer;

            public ChildEnumerator(Node n) {
                outer = n;
            }

            int nextIndex = -1;

            public Node Current {
                get {
                    if (nextIndex < 0 || nextIndex >= 8) {
                        throw new InvalidOperationException();
                    }
                    return outer.children[nextIndex];
                }
            }

            object IEnumerator.Current {
                get {
                    return Current;
                }
            }

            public void Dispose() {
            }

            public bool MoveNext() {
                do {
                    ++nextIndex;
                }
                while (nextIndex < 8 && outer.children[nextIndex] == null);
                if (nextIndex == 8) return false;
                return true;
            }

            public void Reset() {
                nextIndex = -1;
            }
        }

        /// <summary>
        /// The name of the node, which is the path of the node in the tree. For example "023" for the forth child of the third child of the first child of the root node.
        /// (readonly)
        /// </summary>
        public string Name {
            get { return name; }
        }

        /// <summary>
        /// The BoundingBox of the node (readonly)
        /// </summary>
        public BoundingBox BoundingBox {
            get {
                return boundingBox;
            }
        }

        /// <summary>
        /// The parent of this node (may be null, if this is a root node)
        /// </summary>
        public Node Parent {
            get {
                return parent;
            }

            set {
                parent = value;
            }
        }

        /// <summary>
        /// Number of points given the last time SetPoints was called. Or -1 if it hasn't been called yet
        /// </summary>
        public int PointCount {
            get {
                return pointCount;
            }
        }

        /// <summary>
        /// The metadata identifying which pointcloud this node belongs to
        /// </summary>
        public PointCloudMetaData MetaData {
            get { return metaData; }
        }

        /// <summary>
        /// Returns a string representation of the Node (for example "Node: r123")
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return "Node: r" + Name;
        }
    }


}

