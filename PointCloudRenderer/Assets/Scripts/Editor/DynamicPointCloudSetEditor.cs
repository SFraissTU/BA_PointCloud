using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BAPointCloudRenderer.CloudController;

[CustomEditor(typeof(DynamicPointCloudSet))]
public class DynamicPointCloudSetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        DynamicPointCloudSet setscript = (DynamicPointCloudSet)target;
        if (EditorApplication.isPlaying)
        {
            if (GUILayout.Button("Hide"))
            {
                setscript.PointRenderer.Hide();
            }
            if (GUILayout.Button("Display"))
            {
                setscript.PointRenderer.Display();
            }
        }
    }
}
