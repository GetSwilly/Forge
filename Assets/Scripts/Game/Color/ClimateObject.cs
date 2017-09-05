using UnityEngine;
using System.Collections;

[System.Serializable]
public class ClimateObject<T> {

    [SerializeField]
    T genericObject;

    [SerializeField]
    Vector2 temperatureRange = Vector2.zero;

    [SerializeField]
    Vector2 moistureRange = Vector2.zero;


    public bool IsInRange(float temp, float moisture)
    {
        return temp >= temperatureRange.x && temp <= temperatureRange.y && moisture >= moistureRange.x && moisture <= moistureRange.y;
    }


    public T GenericObject
    {
        get { return genericObject; }
    }
}

[System.Serializable]
public class ClimateObjectOfPhysicMaterial : ClimateObject<PhysicMaterial>
{

}


