using System.Collections.Generic;
using UnityEngine;

public class Node {
    //static int ID_COUNTER = 0;

    private NodeType type;
    private int x;
    private int y;

    private float staticWeight;
    float additionalWeight;
    private bool walkable;
    private Vector3 worldPosition;

    private float lowestHeight;

    private int nodeID;

    private Node parentNode;
    private float gCost;
    private float hCost;


    private List<UnitController> occupyingUnits = new List<UnitController>();

    public Node(NodeType _type, Vector3 worldPosition, float _height, int indexX, int indexY, int _id, float weight, bool walkable)
    {

        type = _type;

        this.walkable = walkable;

        this.worldPosition = worldPosition;
        lowestHeight = _height;

        x = indexX;
        y = indexY;
        //		this.ID = ID;
        // nodeID = ID_COUNTER;
        //ID_COUNTER++;
        nodeID = _id;

        staticWeight = weight;

        additionalWeight = 0;
    }


    public void AddOccupier(UnitController _controller)
    {
        occupyingUnits.Add(_controller);
    }
    public void RemoveOccupier(UnitController _controller)
    {
        occupyingUnits.Remove(_controller);
    }


    public void WeightChange(float amt)
    {
        additionalWeight += amt;
    }

    public bool IsWalkable(NodeType _walkableTypes)
    {

        return walkable && Utilities.HasFlag(_walkableTypes, Type);

    }


    public bool Equals(Node n)
    {
        return X == n.X && Y == n.Y;
    }
    public int CompareTo(Node n)
    {
        return (int)(fCost - n.fCost);
    }

    public Node Copy()
    {
        Node newNode = new Node(type, worldPosition, HeightClearance, x, y, ID, staticWeight, walkable);
        newNode.additionalWeight = additionalWeight;
        newNode.gCost = gCost;
        newNode.hCost = hCost;

        return newNode;
    }


    #region Getters

    public int X
    {
        get { return x; }
    }
    public int Y
    {
        get { return y; }
    }
    public Vector3 WorldPosition
    {
        get { return worldPosition; }
    }

    public float LowestHeight
    {
        get { return lowestHeight; }
        set { lowestHeight = value; }
    }
    public float HeightClearance
    {
        get { return lowestHeight - worldPosition.y; }
    }


    public float StaticWeight
    {
        get { return staticWeight; }
        set { staticWeight = value; }
    }
    public bool Walkable
    {
        get { return walkable; }
        set { walkable = value; }
    }

    public float GCost
    {
        get { return gCost; }
        set { gCost = value; }
    }
    public float HCost
    {
        get { return hCost; }
        set { hCost = value; }
    }
    public float fCost
    {
        get
        {
            return ((gCost + hCost) * staticWeight) + additionalWeight;
        }
    }

    public NodeType Type
    {
        get { return type; }
        set { type = value; }
    }
    public Node ParentNode
    {
        get { return parentNode; }
        set { parentNode = value; }
    }
    public int ID
    {
        get { return nodeID; }
    }
    #endregion

    public override string ToString()
    {
        return "Node -- X: " + x + ". Y: " + y + ". Position: " + worldPosition;
    }
}
