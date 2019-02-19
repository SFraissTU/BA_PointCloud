using UnityEngine;

namespace BAPointCloudRenderer.Eval {
    
    /// <summary>
    /// Used in animations. OnAnimationEnd is called in the end of the animations to exit the application.
    /// </summary>
    public class AnimationHelper : MonoBehaviour {

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }

        void OnAnimationEnd() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}