using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(AutoFade))]
public class AutoFadeEditor : Editor {


    SerializedProperty method;
    SerializedProperty cycle;
    SerializedProperty fadeTime;
    SerializedProperty fadeDelay;
    SerializedProperty shouldFadeOnAwake;
    SerializedProperty shouldDisableOnCompletion;
    SerializedProperty loopCount;

    //Color Variables
    SerializedProperty startColor;
    SerializedProperty endColor;
    SerializedProperty resetColor;
    SerializedProperty graphic;


    //Transparent Variables
    SerializedProperty alphaValue;
    SerializedProperty resetAlpha;

    //Canvas Group Variables
    SerializedProperty canvasGroup;



    AutoFade script;


    void OnEnable()
    {
        script = target as AutoFade;

        method = serializedObject.FindProperty("m_Method");
        cycle = serializedObject.FindProperty("m_Cycle");
        fadeTime = serializedObject.FindProperty("fadeTime");
        fadeDelay = serializedObject.FindProperty("fadeDelay");
        shouldFadeOnAwake = serializedObject.FindProperty("fadeOnAwake");
        shouldDisableOnCompletion = serializedObject.FindProperty("disableOnCompletion");
        loopCount = serializedObject.FindProperty("limitedLoopCount");


        startColor = serializedObject.FindProperty("m_StartColor");
        endColor = serializedObject.FindProperty("m_EndColor");
        resetColor = serializedObject.FindProperty("resetColor");
        graphic = serializedObject.FindProperty("m_Graphic");

        alphaValue = serializedObject.FindProperty("m_Alpha");
        resetAlpha = serializedObject.FindProperty("resetAlpha");

        canvasGroup = serializedObject.FindProperty("m_CanvasGroup");
    }





    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawBasic();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        switch (script.Method)
        {
            case AutoFade.FadeMethod.Color:
                DrawColor();
                break;
            case AutoFade.FadeMethod.Transparent:
                DrawTransparent();
                break;
            case AutoFade.FadeMethod.CanvasGroup:
                DrawCanvasGroup();
                break;
        }


        serializedObject.ApplyModifiedProperties();
    }
    void DrawBasic()
    {
        EditorGUILayout.PropertyField(method);
        EditorGUILayout.PropertyField(cycle);

        if (script.Cycle == AutoFade.FadeCycle.LimitedLoop)
        {
            EditorGUILayout.PropertyField(loopCount);
        }



        EditorGUILayout.PropertyField(fadeTime);
        EditorGUILayout.PropertyField(fadeDelay);
        EditorGUILayout.PropertyField(shouldFadeOnAwake);
        EditorGUILayout.PropertyField(shouldDisableOnCompletion);
    }
    void DrawColor()
    {
        EditorGUILayout.PropertyField(startColor);
        EditorGUILayout.PropertyField(endColor);
        EditorGUILayout.PropertyField(resetColor);

        EditorGUILayout.PropertyField(graphic);
    }
    void DrawTransparent()
    {
        EditorGUILayout.Slider(resetAlpha, 0f, 1f);
        EditorGUILayout.Slider(alphaValue, 0f, 1f);
    }
    void DrawCanvasGroup()
    {
        EditorGUILayout.PropertyField(canvasGroup);
        EditorGUILayout.Slider(resetAlpha, 0f, 1f);
        EditorGUILayout.Slider(alphaValue, 0f, 1f);
    }
}
