// Author: Kazys Stepanas
using UnityEngine;
using System;

namespace BAPointCloudRenderer.Edl
{
    /// <summary>
    /// Script to attach to a render to target camera, which can then be rendered to another camera.
    /// </summary>
    /// <remarks>
    /// The rendered results of this camera are available in <see cref="RenderTarget"/> and
    /// <see cref="DepthTarget"/>, capturing the colour and depth buffers respectively.
    ///
    /// The recommended setup is to a view camera which renders the scene and a main camera which
    /// blits the scene with appropriate effects and renders the UI.
    ///
    /// The scene/view camera should have the following properties:
    /// <list type="bullet">
    /// <item>Tag not equal to "MainCamera".</item>
    /// <item>Culling must cleared of "UI".</item>
    /// <item>Depth set to 0. Less than the main camera to render first.</item>
    /// </list>
    ///
    /// The Main Camera should be set up with these properties:
    /// <list type="bullet">
    /// <item>Tag set to "MainCamera"</item>
    /// <item>Culing Mask set to "UI" only.</item>
    /// <item>Clear flags can be "Depth only"</item>
    /// <item>Must have a script acessing a <see cref="ViewCamera"/></item>
    /// <item>Depth set to 1. Greater than the <see cref="ViewCamera"/> to render later.</item>
    /// </list>
    ///
    /// The main camera should also have a script which has the <c>OnPostRender()</c> method shown
    /// below.
    /// <code lang="C#">
    /// class OtherCamera : MonoBehaviour
    /// {
    ///   // Camera to render from.
    ///   public ViewCamera _sourceCamera = null;
    ///
    ///   void OnPreRender()
    ///   {
    ///     if (_sourceCamera != null)
    ///     {
    ///       // Get the material to render with. Remember, the Blit operation will render a Quad,
    ///       // so this material's shader should only have an interesting fragment shader.
    ///       Material mat = SomeMaterial;
    ///       // Upload the captured depth texture if required.
    ///       mat.SetTexture("_DepthTexture", _sourceCamera.DepthTarget);
    ///       // Blit the colour buffer from the other camera.
    ///       Graphics.Blit(_sourceCamera.RenderTarget, null, mat, -1);
    ///     }
    ///   }
    /// }
    /// </code>
    /// </remarks>
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class ViewCamera : MonoBehaviour
    {
        /// <summary>
        /// Cached Screen.width value.
        /// </summary>
        /// <remarks>
        /// The <c>RenderTexture</c> objects are maintained at this size.
        /// </remarks>
        public int ScreenWidth { get; private set; }

        /// <summary>
        /// Cached Screen.height value.
        /// </summary>
        /// <remarks>
        /// The <c>RenderTexture</c> objects are maintained at this size.
        /// </remarks>
        public int ScreenHeight { get; private set; }

        /// <summary>
        /// Exposes the colour buffer render target.
        /// </summary>
        public RenderTexture RenderTarget { get; private set; }

        /// <summary>
        /// Exposes the depth buffer render target.
        /// </summary>
        public RenderTexture DepthTarget { get; private set; }

        /// <summary>
        /// Called on start.
        /// </summary>
        /// <remarks>
        /// Clears the screen dimensions to ensure the render targets are created on first
        /// <see cref="Update()"/>.
        /// </remarks>
        void Start()
        {
            ScreenWidth = ScreenHeight = -1;
        }

        /// <summary>
        /// Maintains the render targets.
        /// </summary>
        /// <remarks>
        /// The render targets are recreated whenever the screen size does not match the cached
        /// dimensions.
        /// </remarks>
        void Update()
        {
            if (ScreenWidth != Screen.width || ScreenHeight != Screen.height)
            {
                Camera camera = gameObject.GetComponent<Camera>();
                // Avoid an OpenGL/Vulkan warning:
                camera.targetTexture = null;
                // Update the render buffers whenever the screen size changes.
                ScreenWidth = Screen.width;
                ScreenHeight = Screen.height;
                // Create the depth buffer target.
                DepthTarget = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
                // Create the colour buffer target.
                RenderTarget = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
                // Set the target buffers.
                camera.SetTargetBuffers(RenderTarget.colorBuffer, DepthTarget.depthBuffer);
            }
        }
    }
}