using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomLayout))]
public class CustomLayoutEditor : Editor {

    SerializedProperty layoutStyle;
    SerializedProperty subStyle;
    SerializedProperty properties;

    SerializedProperty movementSmoothing;
    SerializedProperty rotationSmoothing;

    SerializedProperty anchor;
    SerializedProperty ignoreList;

    //Radial Variables

    SerializedProperty placementCircle;
    SerializedProperty startAngle;
    SerializedProperty maxAngle;

    //Stack Variables
    SerializedProperty selectedOffset;
    SerializedProperty unselectedOffset;
    SerializedProperty stackDirection;
    SerializedProperty stackRotation;
    SerializedProperty stackPadding;
    SerializedProperty shouldIsolateSelected;


    //Scrolling Variables
    SerializedProperty scrollValue;
    SerializedProperty scrollPadding;
    


    CustomLayout script;
    



    void OnEnable()
    {
        script = target as CustomLayout;

        layoutStyle = serializedObject.FindProperty("m_LayoutStyle");
        subStyle = serializedObject.FindProperty("m_SubStyle");
        properties = serializedObject.FindProperty("m_Properties");

        movementSmoothing = serializedObject.FindProperty("movementSmoothing");
        rotationSmoothing = serializedObject.FindProperty("rotationSmoothing");

        anchor = serializedObject.FindProperty("anchorPosition");
        ignoreList = serializedObject.FindProperty("ignoreList");


        placementCircle = serializedObject.FindProperty("placementCircle");
        startAngle = serializedObject.FindProperty("startAngle");
        maxAngle = serializedObject.FindProperty("maxAngle");

        selectedOffset = serializedObject.FindProperty("selectedLocalOffset");
        unselectedOffset = serializedObject.FindProperty("stackLocalOffset");
        stackDirection = serializedObject.FindProperty("stackDirection");
        stackRotation = serializedObject.FindProperty("stackRotation");
        stackPadding = serializedObject.FindProperty("stackPadding");
        shouldIsolateSelected = serializedObject.FindProperty("shouldIsolateSelected");

        scrollPadding = serializedObject.FindProperty("scrollPadding");
        scrollValue = serializedObject.FindProperty("scrollValue");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        DrawBasic();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();

        switch (script.MyLayoutStyle)
        {
            case CustomLayout.LayoutStyle.Radial:
                DrawRadial();
                break;
            case CustomLayout.LayoutStyle.Stack:
                DrawStack();
                break;
            case CustomLayout.LayoutStyle.Scrolling:
                DrawScrolling();
                break;
        }


        serializedObject.ApplyModifiedProperties();
    }


    void DrawBasic()
    {
        EditorGUILayout.PropertyField(layoutStyle);
        EditorGUILayout.PropertyField(subStyle);
        EditorGUILayout.PropertyField(properties);

        EditorGUILayout.Slider(movementSmoothing, 0f, 1f);
        EditorGUILayout.Slider(rotationSmoothing, 0f, 1f);

        EditorGUILayout.PropertyField(anchor);
        EditorGUILayout.PropertyField(ignoreList,true);
        /*
        EditorGUI.BeginChangeCheck();

        int newValue = EditorGUI.MaskField(

        if (EditorGUI.EndChangeCheck())
        {
            _property.intValue = newValue;
        }*/
    }
    void DrawRadial()
    {
        EditorGUILayout.PropertyField(placementCircle);

        EditorGUILayout.Slider(startAngle, 0f, 360f);
        EditorGUILayout.Slider(maxAngle, 0f, 360f);
    }
    void DrawStack()
    {
        EditorGUILayout.PropertyField(selectedOffset);
        EditorGUILayout.PropertyField(unselectedOffset);
        EditorGUILayout.PropertyField(stackDirection);
        EditorGUILayout.PropertyField(stackRotation);
        EditorGUILayout.PropertyField(stackPadding);
        EditorGUILayout.PropertyField(shouldIsolateSelected);
    }
    void DrawScrolling()
    {
        EditorGUILayout.PropertyField(scrollPadding);
        EditorGUILayout.Slider(scrollValue, 0f, 1f);
    }
}
