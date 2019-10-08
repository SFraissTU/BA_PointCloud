using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BAPointCloudRenderer.CloudController;

[CustomEditor(typeof(PointCloudLoader))]
public class PointCloudLoaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PointCloudLoader loaderscript = (PointCloudLoader)target;
        if (!EditorApplication.isPlaying)
        {
            /*if (!loaderscript.pointPreview)
            {
                if (!loaderscript.HasPointCloudLoaded() && GUILayout.Button("Load Preview"))
                {
                    loaderscript.LoadPointCloud();
                }
                if (loaderscript.HasPointCloudLoaded() && GUILayout.Button("Hide Preview"))
                {
                    loaderscript.RemovePointCloud();
                }
            }*/
        }
    }
}
