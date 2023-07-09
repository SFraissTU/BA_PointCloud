using BAPointCloudRenderer.CloudData;
using System.Collections.Generic;
using UnityEngine;

namespace BAPointCloudRenderer.ObjectCreation {

    /// <summary>
    /// What kind of interpolation to use
    /// </summary>
    enum FragInterpolationMode {
        /// <summary>
        /// No interpolation
        /// </summary>
        OFF,
        /// <summary>
        /// Paraboloids
        /// </summary>
        PARABOLOIDS,
        /// <summary>
        /// Cones
        /// </summary>
        CONES
    }

    /// <summary>
    /// This is the default Mesh Configuration, that is able to render points as pixels, quads, circles and also provides fragment and cone interpolations using the fragment shader (see Thesis chapter 3.3.4 "Interpolation").
    /// This works using Geometry Shader Quad Rendering, as described in the Bachelor Thesis in chapter 3.3.3.
    /// This configuration also supports changes of the parameters while the application is running. Just change the parameters and check the checkbox "reload".
    /// This class replaces GeoQuadMeshConfiguration in Version 1.2.
    /// </summary>
    class DefaultMeshConfiguration : MeshConfiguration {
        /// <summary>
        /// Radius of the point (in pixel or world units, depending on variable screenSize)
        /// </summary>
        public float pointRadius = 5;
        /// <summary>
        /// Whether the quads should be rendered as circles (true) or as squares (false)
        /// </summary>
        public bool renderCircles = false;
        /// <summary>
        /// True, if pointRadius should be interpreted as pixels, false if it should be interpreted as world units
        /// </summary>
        public bool screenSize = true;
        /// <summary>
        /// Wether and how to use interpolation
        /// </summary>
        public FragInterpolationMode interpolation = FragInterpolationMode.OFF;
        /// <summary>
        /// If changing the parameters should be possible during execution, this variable has to be set to true in the beginning! Later changes to this variable will not change anything
        /// </summary>
        public const bool reloadingPossible = true;
        /// <summary>
        /// Set this to true to reload the shaders according to the changed parameters. After applying the changes, the variable will set itself back to false.
        /// </summary>
        public bool reload = false;
        /// <summary>
        /// The camera that's used for rendering. If not set, Camera.main is used. 
        /// This should usually be the same camera that's used as "User Camera" in the point cloud set.
        /// </summary>
        public Camera renderCamera = null;
        /// <summary>
        /// If set to true, the Bounding Boxes of the individual octree nodes will be displayed.
        /// </summary>
        public bool displayLOD = false;

        private Material material;
        private HashSet<GameObject> gameObjectCollection = null;

        private void LoadShaders() {
            if (interpolation == FragInterpolationMode.OFF) {
                if (screenSize) {
                    material = new Material(Shader.Find("Custom/QuadGeoScreenSizeShader"));
                } else {
                    material = new Material(Shader.Find("Custom/QuadGeoWorldSizeShader"));
                }
            } else if (interpolation == FragInterpolationMode.PARABOLOIDS || interpolation == FragInterpolationMode.CONES) {
                if (screenSize) {
                    material = new Material(Shader.Find("Custom/ParaboloidFragScreenSizeShader"));
                } else {
                    material = new Material(Shader.Find("Custom/ParaboloidFragWorldSizeShader"));
                }
                material.SetInt("_Cones", (interpolation == FragInterpolationMode.CONES) ? 1 : 0);
            }
            material.enableInstancing = true;
            material.SetFloat("_PointSize", pointRadius);
            material.SetInt("_Circles", renderCircles ? 1 : 0);

            if (renderCamera == null)
            {
                renderCamera = Camera.main;
            }
        }

        public void Start() {
            if (reloadingPossible) {
                gameObjectCollection = new HashSet<GameObject>();
            }
            LoadShaders();
        }

        public void Update() {
            if (reload && gameObjectCollection != null) {
                LoadShaders();
                foreach (GameObject go in gameObjectCollection) {
                    go.GetComponent<MeshRenderer>().material = material;
                }
                reload = false;
            }
            if (displayLOD)
            {
                foreach (GameObject go in gameObjectCollection)
                {
                    BoundingBoxComponent bbc = go.GetComponent<BoundingBoxComponent>();
                    Utility.BBDraw.DrawBoundingBox(bbc.boundingBox, bbc.parent, Color.red, false);
                }
            }
            if (screenSize) {
                if (interpolation != FragInterpolationMode.OFF) {
                    Matrix4x4 invP = (GL.GetGPUProjectionMatrix(renderCamera.projectionMatrix, true)).inverse;
                    material.SetMatrix("_InverseProjMatrix", invP);
                    material.SetFloat("_FOV", Mathf.Deg2Rad * renderCamera.fieldOfView);
                }
                Rect screen = renderCamera.pixelRect;
                material.SetInt("_ScreenWidth", (int)screen.width);
                material.SetInt("_ScreenHeight", (int)screen.height);
            }
        }

        public override GameObject CreateGameObject(string name, Vector3[] vertexData, Color[] colorData, BoundingBox boundingBox, Transform parent, string version, Vector3d translationV2) {
            GameObject gameObject = new GameObject(name);

            Mesh mesh = new Mesh();

            MeshFilter filter = gameObject.AddComponent<MeshFilter>();
            filter.mesh = mesh;
            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.material = material;

            int[] indecies = new int[vertexData.Length];
            for (int i = 0; i < vertexData.Length; ++i) {
                indecies[i] = i;
            }
            mesh.vertices = vertexData;
            mesh.colors = colorData;
            mesh.SetIndices(indecies, MeshTopology.Points, 0);

            //Set Translation
            if (version == "2.0")
            {
                // 20230125: potree v2 vertices have absolute coordinates,
                // hence all gameobjects need to reside at Vector.Zero.
                // And: the position must be set after parenthood has been granted.
                //gameObject.transform.Translate(boundingBox.Min().ToFloatVector());
                gameObject.transform.SetParent(parent, false);
                gameObject.transform.localPosition = translationV2.ToFloatVector();
            }
            else
            {
                gameObject.transform.Translate(boundingBox.Min().ToFloatVector());
                gameObject.transform.SetParent(parent, false);
            }

            BoundingBoxComponent bbc = gameObject.AddComponent<BoundingBoxComponent>();
            bbc.boundingBox = boundingBox; ;
            bbc.parent = parent;

            if (gameObjectCollection != null) {
                gameObjectCollection.Add(gameObject);
            }

            return gameObject;
        }

        public override int GetMaximumPointsPerMesh() {
            return 65000;
        }

        public override void RemoveGameObject(GameObject gameObject) {
            if (gameObjectCollection != null) {
                gameObjectCollection.Remove(gameObject);
            }
            if (gameObject != null) {
                Destroy(gameObject.GetComponent<MeshFilter>().sharedMesh);
                Destroy(gameObject);
            }
        }
    }
}
