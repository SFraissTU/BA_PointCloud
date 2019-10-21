using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BAPointCloudRenderer.CloudController;

[CustomEditor(typeof(Preview))]
public class PreviewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Preview previewscript = (Preview)target;
        if (!EditorApplication.isPlaying)
        {
            if (GUILayout.Button("Update Preview"))
            {
                previewscript.UpdatePreview();
            }
            if (GUILayout.Button("Hide Preview"))
            {
                previewscript.HidePreview();
            }
        }
    }
}
