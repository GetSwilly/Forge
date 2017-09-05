using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(A_Star_Pathfinding))]
public class A_Star_PathfindingEditor : Editor {

	public override void OnInspectorGUI(){

		A_Star_Pathfinding myScript = (A_Star_Pathfinding) target;
		
		if(GUILayout.Button("Detect Level")){
			myScript.GridFromDetection();
		}


		
		
		DrawDefaultInspector();
	}
}
