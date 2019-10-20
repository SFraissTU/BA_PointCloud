using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BAPointCloudRenderer.CloudController;

[CustomEditor(typeof(DirectoryLoader))]
public class DirectoryLoaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        DirectoryLoader loaderscript = (DirectoryLoader)target;
        if (!EditorApplication.isPlaying)
        {
            if (GUILayout.Button("Load Directory"))
            {
                loaderscript.LoadAll();
            }
        }
    }
}
