using Pathfinding.RVO;
using Pathfinding.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RVOObstacle : MonoBehaviour
{
    static readonly float HeightMinimum = 0.5f;
    static readonly int DegreeSubdivisionMinimum = 5;
    static readonly int DegreeSubdivisionMaximum = 360;

    enum ObstacleShape
    {
        Box,
        Circle
    }

    [SerializeField]
    ObstacleShape m_Shape;

    [SerializeField]
    float padding = 0f;

    [SerializeField]
    float height = 1;

    [SerializeField]
    Vector2 size;

    [SerializeField]
    float radius;

    [SerializeField]
    int degreeSubdivisions = 360;


    RVOSimulator m_Simulator;


    void Start()
    {
        AddToSimulator();
    }

    void AddToSimulator()
    {
        Vector3[] verts = null;
        switch (m_Shape)
        {
            case ObstacleShape.Box:
                verts = CalculateBoxVerts();
                break;
            case ObstacleShape.Circle:
                verts = CalculateCircleVerts();
                break;
        }

        AddToSimulator(verts);
    }
    void AddToSimulator(Vector3[] verts)
    {
        AddToSimulator(verts, Height);
    }
    void AddToSimulator(Vector3[] verts, float _height)
    {
        if (RVOSimulator.active == null)
            return;

        if (verts == null || verts.Length < 2)
            return;

        RVOSimulator.active.GetSimulator().AddObstacle(verts, _height);
    }



    Vector3[] CalculateBoxVerts()
    {
        Vector3[] verts = new Vector3[4];
        Vector3 boxSize = CalculateBoxSize();

        Vector3 rightCenter = transform.right * (boxSize.x + Padding);
        Vector3 forwardCenter = transform.forward * (boxSize.z + Padding);

        verts[0] = rightCenter + forwardCenter + transform.position;
        verts[1] = rightCenter - forwardCenter + transform.position;
        verts[2] = -rightCenter + forwardCenter + transform.position;
        verts[3] = -rightCenter - forwardCenter + transform.position;

        return verts;
    }
    Vector3[] CalculateCircleVerts()
    {
        Vector3[] verts = new Vector3[DegreeSubdivisions];


        float degreeDelta = 360f / DegreeSubdivisions;
        float currentDegree = 0f;


        for (int i = 0; i < DegreeSubdivisions; i++)
        {
            float radians = currentDegree * Mathf.Deg2Rad;

            verts[i] = new Vector3((float)Mathf.Cos(radians), 0f, (float)Mathf.Sin(radians)) * (Radius + Padding);
            verts[i] += transform.position;

            currentDegree += degreeDelta;
        }

        return verts;
    }


    Vector3 CalculateBoxSize()
    {
        Vector3 tempSize = new Vector3(size.x, Height, size.y);
        Vector3 scale = transform.lossyScale;

        tempSize.x *= scale.x;
        tempSize.y *= scale.y;
        tempSize.z *= scale.z;

        tempSize.x += Padding;
        tempSize.z += Padding;

        return tempSize;
    }
    float CalculateCircleSize()
    {
        float tempRadius = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        tempRadius *= Radius;
        tempRadius += Padding;

        return tempRadius;
    }



    #region Accessors
    public float Height
    {
        get { return height; }
        private set { height = Mathf.Clamp(value, HeightMinimum, value); }
    }
    public float Padding
    {
        get { return padding; }
        private set { padding = Mathf.Clamp(value, 0f, value); }
    }
    public Vector3 Size
    {
        get { return size; }
    }
    public float Radius
    {
        get { return radius; }
        private set { radius = Mathf.Clamp(value, 0f, value); }
    }
    public int DegreeSubdivisions
    {
        get { return degreeSubdivisions; }
        private set { degreeSubdivisions = Mathf.Clamp(value, DegreeSubdivisionMinimum, DegreeSubdivisionMaximum); }
    }
    #endregion


    void OnValidate()
    {
        ValidateSize();

        Height = Height;
        Padding = Padding;
        Radius = Radius;
        DegreeSubdivisions = DegreeSubdivisions;
    }
    void ValidateSize()
    {
        if (size.x < 0)
        {
            size.x = 0f;
        }

        if(size.y < 0)
        {
            size.y = 0f;
        }
    }
    //void CheckCollider()
    //{
    //    switch (m_Shape)
    //    {
    //        case ObstacleShape.Box:
    //            boxCollider = GetComponent<BoxCollider>();
    //            if (boxCollider == null)
    //            {
    //                boxCollider = this.gameObject.AddComponent<BoxCollider>();
    //            }
    //            break;
    //        case ObstacleShape.Circle:
    //            sphereCollder = GetComponent<SphereCollider>();
    //            if (sphereCollder == null)
    //            {
    //                sphereCollder = this.gameObject.AddComponent<SphereCollider>();
    //            }
    //            break;
    //    }
    //}


    private static readonly Color GizmoColor = new Color(240 / 255f, 213 / 255f, 30 / 255f);

    void OnDrawGizmos()
    {
        Gizmos.color = GizmoColor;

        switch (m_Shape)
        {
            case ObstacleShape.Box:
                DrawBox();
                break;
            case ObstacleShape.Circle:
                DrawCircle();
                break;
        }
    }

    void DrawBox()
    {
        Vector3 tempSize = CalculateBoxSize();

        Utilities.DrawWireCube(transform.position, transform.rotation, Vector3.one, tempSize);
    }
    void DrawCircle()
    {
        float tempRadius = CalculateCircleSize();

        Gizmos.DrawWireSphere(transform.position, tempRadius);
    }
    
}
