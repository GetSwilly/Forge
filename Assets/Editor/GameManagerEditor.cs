using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor {

	public override void OnInspectorGUI(){
		/*
		GameManager myScript = (GameManager) target;
		
		if(GUILayout.Button("Generate Level")){
			myScript.GenerateLevel();
		}

*/


		DrawDefaultInspector();
	}
}
