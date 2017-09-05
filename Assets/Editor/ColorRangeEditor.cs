using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(ColorRange))]
public class ColorRangeEditor : PropertyDrawer
{
    static Color bgColor = new Color(0, 0, 0, .25f);


    const int propertySize = 250;


    //const int gridSize = 5;

    const float rectWidth = 30;
    const float rectHeight = 30;

    const float widthBuffer = 5;
    const float heightBuffer = 5;

    const float guiWidthBuffer = 100f;


  //  bool foldout = true;


    public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label)
    {

        //foldout = EditorGUI.Foldout(pos, foldout, label);


      //  if (!foldout)
           // return;


        //property.serializedObject.Update();


        EditorGUI.BeginProperty(pos, label, property);

        EditorGUI.PrefixLabel(pos, GUIUtility.GetControlID(FocusType.Passive), label);


        //EditorGUILayout.Foldout(true, "Color Range");
        // return;
        SerializedProperty colors = property.FindPropertyRelative("colorFields");
        int gridSize = colors.arraySize;

        EditorGUI.DrawRect(pos, bgColor);


        Rect drawRect = pos;
        drawRect.width = RectangleSize(gridSize); 
        drawRect.height = RectangleSize(gridSize);


        float totalWidth = ((gridSize - 0.5f) * RectangleSize(gridSize)) + ((gridSize - 1f) * widthBuffer);
        float additionalX = pos.width - totalWidth;

        Vector3 zeroPos = drawRect.position;
        zeroPos.x = additionalX;// * .9f;
        


        for (int i = 0; i < gridSize ; i++)
        {
            SerializedProperty colorRow = colors.GetArrayElementAtIndex(i).FindPropertyRelative("colorRow");

            Vector3 _position = zeroPos;
            _position.x = zeroPos.x + (i * RectangleSize(gridSize)) + (i * widthBuffer);



            for (int k =0; k < gridSize; k++)
            {
                _position.y = zeroPos.y + ((gridSize-k-1) * RectangleSize(gridSize)) + (((gridSize-k-1) + 0.5f) * heightBuffer);


                drawRect.position = _position;
                colorRow.GetArrayElementAtIndex(k).colorValue = EditorGUI.ColorField(drawRect, GUIContent.none , colorRow.GetArrayElementAtIndex(k).colorValue, false, false ,false,null);
            }
        }
        EditorGUI.EndProperty();
        

       
    }


    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        //int numFields = property.FindPropertyRelative("colorFields").arraySize;
       // float rectSize = RectangleSize(numFields);
        return propertySize * 1.01f; // (gridSize * (rectHeight + heightBuffer)) * 1.05f;
    }






    float RectangleSize(int size)
    {
        float val = propertySize;
        val -= size * heightBuffer;
        val /= size;

        return val;
    }
}