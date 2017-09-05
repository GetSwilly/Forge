using UnityEngine;
using System.Collections;

[System.Serializable]
public class ColorRange {
    
    [System.Serializable]
    public struct colorData
    {
        public Color[] colorRow;
    }


    // public Color[][] colorFields;
    public colorData[] colorFields;

    public ColorRange(int size)
    {

        colorFields = new colorData[size];

        for(int i = 0; i < colorFields.Length; i++)
        {
            colorFields[i].colorRow = new Color[size];
        }
    }

    public Color? GetColor(int x, int y)
    {
        
        if (x < 0 || x >= colorFields.Length)
            return null;

        if (y < 0 || y >= colorFields[x].colorRow.Length)
            return null;
            
        
        Color _color = colorFields[x].colorRow[y];

        return _color;
    }

    public void UpdateColor(int x, int y, Color _color)
    {
        if (x < 0 || x >= colorFields.Length)
            return;

        if (y < 0 || y >= colorFields[x].colorRow.Length)
            return;

        colorFields[x].colorRow[y] = _color;
    }
}
