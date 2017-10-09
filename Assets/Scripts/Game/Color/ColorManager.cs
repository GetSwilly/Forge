using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorManager
{

    static Color ATTRIBUTE_FIRE_COLOR = new Color(0.8f, 0f, 0f, .6f);
    static Color ATTRIBUTE_ICE_COLOR = new Color(.05f, .85f, .88f, .6f);
    static Color ATTRIBUTE_ELECTRICITY_COLOR = new Color(0.9f, .88f, .05f, .6f);
    static Color ATTRIBUTE_POISON_COLOR = new Color(0.87f, 0f, .89f, .6f);
    static Color ATTRIBUTE_WATER_COLOR = new Color(0f, .28f, .82f, .6f);

    static Color CURRENCY_EXPERIENCE_COLOR = new Color(0.3f, .88f, 0.33f, .6f);
    static Color CURRENCY_HEALTH_COLOR = new Color(1f, .175f, .5f, .6f);
    static Color CURRENCY_LEVELPOINTS_COLOR = new Color(1f, .66f, .05f, .6f);
    static Color CURRENCY_STATLEVEL_COLOR = new Color(0.78f, 0.4f, 1f, .6f);




    public static Color GetColor(Attribute _attribute)
    {
        switch (_attribute)
        {
            case Attribute.Fire:
                return ATTRIBUTE_FIRE_COLOR;
            case Attribute.Ice:
                return ATTRIBUTE_ICE_COLOR;
            case Attribute.Shock:
                return ATTRIBUTE_ELECTRICITY_COLOR;
            case Attribute.Poison:
                return ATTRIBUTE_POISON_COLOR;
            case Attribute.Water:
                return ATTRIBUTE_WATER_COLOR;
        }


        return Color.black;
    }

}
