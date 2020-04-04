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
        if (EditorApplication.isPlaying)
        {
            if (GUILayout.Button("Load"))
            {
                loaderscript.LoadPointCloud();
            }
            if (GUILayout.Button("Remove"))
            {
                loaderscript.RemovePointCloud();
            }
        }
    }
}
