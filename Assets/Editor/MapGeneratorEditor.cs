using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor {

	public override void OnInspectorGUI(){
		
		DrawDefaultInspector();
		MapGenerator myScript = (MapGenerator) target;
		
		if(GUILayout.Button("Generate Map")){
			myScript.EditorGenerateMap();
		}
	}
}
