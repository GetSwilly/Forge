using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delegates  {

    public delegate void Alert();
    public delegate void ValueAlertEvent(float newValue);
    public delegate void ValueChangeEvent(float newValue, float valueDelta);
    public delegate void AbilityChangeEvent(string abilityName, float newValue);
    public delegate void PathUpdated(Pathfinding.Path p);

    public delegate void StatChanged(StatType type, int level);
}
