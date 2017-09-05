using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class ListWrapper<T>
{

    public List<T> wrappedList;

    public ListWrapper()
    {

    }
    public ListWrapper(List<T> _list)
    {

        wrappedList = _list;
    }



    public int Count
    {
       get { return wrappedList == null ? 0 : wrappedList.Count; }
    }
    
}


[System.Serializable]
public class ListWrapperOfColorPoint : ListWrapper<ColorPoint>
{
    public ListWrapperOfColorPoint() : base()
    {

    }
    public ListWrapperOfColorPoint(List<ColorPoint> _list) : base(_list)
    {

    }
}