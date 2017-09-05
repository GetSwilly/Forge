using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(LevelController))]
public class LevelControllerEditor : Editor {


    public override void OnInspectorGUI()
    {
        LevelController myScript = (LevelController)target;

        if (GUILayout.Button("Generate Level"))
        {
            myScript.EditorGenerateLevel(false);
        }

        if (GUILayout.Button("Generate Random Level"))
        {
            myScript.EditorGenerateLevel(true);
        }

        DrawDefaultInspector();
    }
}
