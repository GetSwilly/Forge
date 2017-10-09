using UnityEngine;
using System.Collections;

[System.Serializable]
public class CustomTuple2<T,U> {

    [SerializeField]
    T item1;

    [SerializeField]
    U item2;

    public CustomTuple2(T a, U b)
    {
        item1 = a;
        item2 = b;
    }




    public T Item1
    {
        get { return item1; }
        set { item1 = value; }
    }
    public U Item2
    {
        get { return item2; }
        set { item2 = value; }
    }
}

[System.Serializable]
public class CustomTuple3<T, U, V>
{
    [SerializeField]
    T item1;

    [SerializeField]
    U item2;

    [SerializeField]
    V item3;

    public CustomTuple3(T a, U b, V c)
    {
        item1 = a;
        item2 = b;
        item3 = c;
    }




    public T Item1
    {
        get { return item1; }
        set { item1 = value; }
    }
    public U Item2
    {
        get { return item2; }
        set { item2 = value; }
    }
    public V Item3
    {
        get { return item3; }
        set { item3 = value; }
    }
}



[System.Serializable]
public class Tuple_StringTransform : CustomTuple2<string, Transform>
{

    public Tuple_StringTransform(string a, Transform b) : base(a, b)
    {

    }
}


[System.Serializable]
public class Tuple_StringGameobject : CustomTuple2<string, GameObject>
{

    public Tuple_StringGameobject(string a, GameObject b) : base(a, b)
    {

    }
}


[System.Serializable]
public class Tuple_NodeTypeFloat
{
    [SerializeField]
    [EnumFlags]
    NodeType node;

    [SerializeField]
    [Range(0f, 1f)]
    float placementChance;



    public Tuple_NodeTypeFloat(NodeType a, float b)
    {
        node = a;
        placementChance = b;
    }

    public NodeType Node
    {
        get { return node; }
    }
    public float PlacementChance
    {
        get { return placementChance; }
    }
}


[System.Serializable]
public class Tuple_AttributeFloat : CustomTuple2<Attribute, float>
{

    public Tuple_AttributeFloat(Attribute a, float b) : base(a, b) { }
}




[System.Serializable]
public class Tuple_PhysicalMaterialVector2Array:CustomTuple2<PhysicMaterial,Vector2[]>
{

    //[SerializeField]
    //PhysicMaterial myMaterial;

    //[SerializeField]
    //Vector2[] myVectorArray;



    public Tuple_PhysicalMaterialVector2Array(PhysicMaterial a, Vector2[] b) : base(a, b) { }


    //public PhysicMaterial Material
    //{
    //    get { return myMaterial; }
    //    set { myMaterial = value; }
    //}

    //public Vector2[] VectorArray
    //{
    //    get { return myVectorArray; }
    //    set { myVectorArray = value; }
    //}
}





[System.Serializable]
public class Tuple_ItemPool_PoolWeightIdentifier : CustomTuple2<ItemPool, PoolWeightIdentifier>
{

    public Tuple_ItemPool_PoolWeightIdentifier(ItemPool a, PoolWeightIdentifier b) : base(a, b)
    {

    }
}



[System.Serializable]
public class Tuple_StatTypeInt : CustomTuple2<StatType, int>
{

    public Tuple_StatTypeInt(StatType a, int b) : base(a, b)
    {

    }
}
