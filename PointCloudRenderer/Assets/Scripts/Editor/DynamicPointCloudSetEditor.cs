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
        if (!EditorApplication.isPlaying)
        {
            //Möglicherweise nicht mal mehr nötig, da wir jetzt kontinuierliche Updates haben
            if (GUILayout.Button("Refresh Preview"))
            {
                setscript.UpdatePreview();
            }
            if (GUILayout.Button("Hide Preview"))
            {
                setscript.HidePreview();
            }
        }
    }
}
