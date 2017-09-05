using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WeatherSystem))]
public class WeatherSystemEditor : Editor {


    public override void OnInspectorGUI()
    {
        WeatherSystem myScript = (WeatherSystem)target;

        if (GUILayout.Button("Start Weather"))
        {
            myScript.Initialize();
        }


        DrawDefaultInspector();
    }
}
