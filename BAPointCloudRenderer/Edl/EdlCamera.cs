// Author: Craig James
// Modified by: Kazys Stepanas
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace BAPointCloudRenderer.Edl
{
    /// <summary>
    /// Applies an eye dome lighting (EDL) post processing effect.
    /// </summary>
    /// <remarks>
    /// This relies on the scene being rendered into a <see cref="ViewCamera"/>. See that class for
    /// tips on the relationship between that camera and the main camera.
    /// </remarks>
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("CustomEffect/EDL")]
    public class EdlCamera : MonoBehaviour
    {
        /// <summary>
        /// The camera object from which to source a render target for EDL.
        /// </summary>
        [SerializeField]
        private ViewCamera _edlSourceCamera = null;

        /// <summary>
        /// Get the camera object from which to source a render target for EDL.
        /// </summary>
        public ViewCamera EdlSourceCamera { get { return _edlSourceCamera; } }

        /// <summary>
        /// The EDL shader used to blit the other camera and apply EDL effects.
        /// </summary>
        [SerializeField]
        private Shader _edlShader = null;

        /// <summary>
        /// The shader to use when EDL is off. This only blits the other camera; no other effects.
        /// </summary>
        [SerializeField]
        private Shader _edlOffShader = null;
        /// <summary>
        /// Defines the range of the EDL shadowing. Increase for wider shadows.
        /// </summary>
        [SerializeField, Range(1, 10)]
        private float _edlRadius = 2;
        /// <summary>
        /// Defines the range of the EDL shadowing. Increase for wider shadows.
        /// </summary>
        public float EdlRadius { get { return _edlRadius; } set { _edlRadius = value; } }
        /// <summary>
        /// Controls the exponential scaling used in EDL. Increase to darken EDL shadowing.
        /// </summary>
        [SerializeField, Range(0.1f, 30)]
        private float _edlExpScale = 3;
        /// <summary>
        /// Controls the exponential scaling used in EDL. Increase to darken EDL shadowing.
        /// </summary>
        public float EdlExpScale { get { return _edlExpScale; } set { _edlExpScale = value; } }
        /// <summary>
        /// Controls the linear scaling used in EDL. Increase to darken EDL shadowing.
        /// </summary>
        [SerializeField, Range(1, 10)]
        private float _edlScale = 1;
        /// <summary>
        /// Controls the linear scaling used in EDL. Increase to darken EDL shadowing.
        /// </summary>
        public float EdlScale { get { return _edlScale; } set { _edlScale = value; } }

        /// <summary>
        /// EDL status.
        /// </summary>
        [SerializeField]
        private bool _edlOn = true;

        /// <summary>
        /// Migrate camera settings to the <see cref="ViewCamera"/> on update?
        /// </summary>
        /// <remarks>
        /// When enabled, the following settings are copied from this camera to the <see cref="ViewCamera"/>:
        /// <list type="bullet">
        /// <item>Near clip plane</item>
        /// <item>Far clip plane</item>
        /// <item>Field of view</item>
        /// </list>
        /// </remarks>
        public bool MigrateSettings { get { return _migrateSettings; } set { _migrateSettings = value; } }

        /// <summary>
        /// EDL status.
        /// </summary>
        [SerializeField]
        private bool _migrateSettings = true;

        /// <summary>
        /// Controls EDL shader activation. Uses simple screen blit when off.
        /// </summary>
        public bool EdlOn { get { return _edlOn; } set { _edlOn = value; } }

        /// <summary>
        /// Maximum neighbours considered in calculating depth. Must match NEIGHBOUR_COUNT in the EDL
        /// shader.
        /// </summary>
        private const int MAX_NEIGHBOURS = 8;

        /// <summary>
        /// The EDL material.
        /// </summary>
        private Material _edlMaterial;

        /// <summary>
        /// The material used when EDL is off.
        /// </summary>
        private Material _edlOffMaterial;

        /// <summary>
        /// Fetches the EDL render material. Affected by the <see cref="EdlOn"/> flag.
        /// </summary>
        protected Material EdlMaterial {
            get {
                if (_edlOffMaterial == null && _edlOffShader != null)
                {
                    _edlOffMaterial = new Material(_edlOffShader);
                    _edlOffMaterial.hideFlags = HideFlags.HideAndDontSave;
                }

                if (_edlMaterial == null && _edlShader != null)
                {
                    _edlMaterial = new Material(_edlShader);
                    _edlMaterial.hideFlags = HideFlags.HideAndDontSave;
                    Vector4[] neighbourAddress = new Vector4[MAX_NEIGHBOURS];
                    for (int i = 0; i < MAX_NEIGHBOURS; i++)
                    {
                        neighbourAddress[i] = new Vector2((float)Math.Cos(2 * i * Math.PI / MAX_NEIGHBOURS),
                                                          (float)Math.Sin(2 * i * Math.PI / MAX_NEIGHBOURS));
                    }
                    _edlMaterial.SetVectorArray("_NeighbourAddress", neighbourAddress);
                }

                return (EdlOn) ? _edlMaterial : _edlOffMaterial;
            }

            private set { _edlMaterial = value; }
        }

        /// <summary>
        /// Start up shader validation.
        /// </summary>
        void Start()
        {
            // Disable if we don't support image effects
#pragma warning disable CS0618 // Type or member is obsolete
            if (!SystemInfo.supportsImageEffects)
#pragma warning restore CS0618 // Type or member is obsolete
            {
                Debug.Log("Doesn't support Image effects");
                enabled = false;
                return;
            }

            // Disable the image effect if the shader can't
            // run on the users graphics card
            if (!_edlShader || !_edlShader.isSupported)
            {
                enabled = false;
                Debug.Log("No shader or not supported");
                return;
            }
        }

        void Update()
        {
            if (MigrateSettings && _edlSourceCamera != null)
            {
                Camera src = GetComponent<Camera>();
                Camera dst = _edlSourceCamera.GetComponent<Camera>();
                dst.nearClipPlane = src.nearClipPlane;
                dst.farClipPlane = src.farClipPlane;
                dst.fieldOfView = src.fieldOfView;
            }
        }

        /// <summary>
        /// Pre render which applies the EDL effect.
        /// </summary>
        /// <remarks>
        /// Note: pre Unity 5.6 this method was set to <c>OnPreRender()</c> with the
        /// blit call set as follows: <c>Graphics.Blit(_edlSourceCamera.RenderTarget, null, mat, -1)</c>
        /// and worked well. However, since 5.6, something has changed under OpenGL and the fix has been to
        /// change the method to <c>OnRenderImage()</c> and blit to <c>dest</c> while ignoring
        /// <c>source</c>. This has been noted as it may have other side effects.
        /// </remarks>
        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            // Fetch material to use. Either the EDL material, or a screen blit.
            Material mat = EdlMaterial;
            Camera cam = GetComponent<Camera>();
            if (cam != null)
            {
                if (mat != null)
                {
                    if (EdlOn)
                    {
                        // Configure EDL shader variables.
                        mat.SetFloat("_Radius", EdlRadius);
                        mat.SetFloat("_ExpScale", EdlExpScale * cam.farClipPlane / 1000.0f);
                        mat.SetFloat("_EdlScale", EdlScale * cam.farClipPlane / 1000.0f);
                        mat.SetTexture("_DepthTexture", _edlSourceCamera.DepthTarget);
                    }

                    // Render the other camera to this one.
                    Graphics.Blit(_edlSourceCamera.RenderTarget, dest, mat, -1);
                    // Graphics.Blit(src, dest);
                    // cam.Render();
                    // Graphics.Blit(cam.active, dest);
                }
            }
        }
    }
}