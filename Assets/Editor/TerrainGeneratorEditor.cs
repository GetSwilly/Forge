using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor {


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TerrainGenerator script = (TerrainGenerator)target;

        if (GUILayout.Button("Generate"))
        {
            script.GenerateTerrain();
        }
    }
}
