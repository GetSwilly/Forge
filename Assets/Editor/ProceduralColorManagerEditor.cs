using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ProceduralColorManager))]
public class ProceduralColorManagerEditor : Editor {

	public override void OnInspectorGUI(){

		DrawDefaultInspector();
		ProceduralColorManager myScript = (ProceduralColorManager) target;

		if(GUILayout.Button("Generate Colors")){
			myScript.GenerateRandomColors();
		}
	}
}
