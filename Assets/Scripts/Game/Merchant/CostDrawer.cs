using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//[CustomPropertyDrawer(typeof(Cost))]
public class CostDrawer : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Cost cScript = attribute as Cost;

        SerializedProperty currencyType = property.FindPropertyRelative("m_Currency");
        SerializedProperty value = property.FindPropertyRelative("m_Value");
        SerializedProperty statType = property.FindPropertyRelative("m_StatType");


        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var amountRect = new Rect(position.x, position.y, 30, position.height);
        var unitRect = new Rect(position.x + 35, position.y, 50, position.height);
        var nameRect = new Rect(position.x + 90, position.y, position.width - 90, position.height);

        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("m_Currency"), GUIContent.none);
        EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("m_Value"), GUIContent.none);

        if (cScript.Currency == CurrencyType.StatLevel)
        {
            EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("m_StatType"), GUIContent.none);

        }


        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();


    }
}
