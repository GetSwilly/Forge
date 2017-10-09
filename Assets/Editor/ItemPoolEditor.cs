using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemPool))]
public class ItemPoolEditor : Editor {

    SerializedProperty poolType;
    SerializedProperty poolName;
    
    SerializedProperty poolList;
    SerializedProperty itemList;

    SerializedProperty allowRepeats;

    SerializedProperty autoFill;
    SerializedProperty poolRank;
    SerializedProperty resourcePaths;


    ItemPool script;




    void OnEnable()
    {
        script = target as ItemPool;

        poolType = serializedObject.FindProperty("m_PoolType");
        poolName = serializedObject.FindProperty("m_PoolName");
        allowRepeats = serializedObject.FindProperty("allowRepeatPulls");
        
        poolList = serializedObject.FindProperty("m_Pools");
        itemList = serializedObject.FindProperty("m_Items");

        autoFill = serializedObject.FindProperty("autoFill");
        poolRank = serializedObject.FindProperty("m_Rank");
        resourcePaths = serializedObject.FindProperty("resourcePaths");
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawBasic();
        
        EditorGUILayout.Space();

        switch (script.Type)
        {
            case ItemPool.PoolType.Items:
                DrawItemList();
                break;
            case ItemPool.PoolType.Pools:
                DrawPoolList();
                break;
        }


        serializedObject.ApplyModifiedProperties();
    }


    void DrawBasic()
    {
        EditorGUILayout.PropertyField(poolType);
        EditorGUILayout.PropertyField(poolName);
        EditorGUILayout.PropertyField(allowRepeats);
    }
    void DrawPoolList()
    {
        EditorGUILayout.PropertyField(poolList,true);
    }
    void DrawItemList()
    {
        EditorGUILayout.PropertyField(itemList,true);

        EditorGUILayout.PropertyField(autoFill);

        if (script.AutoFill)
        {
            EditorGUILayout.PropertyField(poolRank);
            EditorGUILayout.PropertyField(resourcePaths,true);
        }
    }
}
