using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct IndexHolder {

    private int x;
    private int y;

    public IndexHolder(int _x, int _y)
    {
        x = _x;
        y = _y;
    }


    public int X
    {
        get { return x; }
        set { x = value; }
    }
    public int Y
    {
        get { return y; }
        set { y = value; }
    }
}
