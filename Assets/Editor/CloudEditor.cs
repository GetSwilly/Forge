using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Cloud))]
[CanEditMultipleObjects]
public class CloudEditor : Editor {

    public override void OnInspectorGUI()
    {
        Cloud myScript = (Cloud)target;

        if (GUILayout.Button("Initialize"))
        {
            myScript.Initialize();
        }


        DrawDefaultInspector();
    }
}
