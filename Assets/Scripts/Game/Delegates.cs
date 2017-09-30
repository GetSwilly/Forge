using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delegates  {

    public delegate void Alert();
    public delegate void PathUpdated(Pathfinding.Path p);

    public delegate void StatChanged(StatType type, int level);
}
