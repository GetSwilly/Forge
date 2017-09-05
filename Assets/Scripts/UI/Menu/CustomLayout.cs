using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CustomLayout : LayoutGroup {


    public enum LayoutStyle { Radial, Stack, Scrolling }
    public enum SubStyle { A, B }

    [System.Flags]
    public enum LayoutProperties
    {
        StartAtOrigin = 1,
        UpdateContinuously = 2,
        Alphabetize = 4
    }

   



    [SerializeField]
    public LayoutStyle m_LayoutStyle = LayoutStyle.Radial;


    [SerializeField]
    public SubStyle m_SubStyle = SubStyle.A;


    [SerializeField]
    [EnumFlags]
    public LayoutProperties m_Properties;

    [SerializeField]
    [Range(0f, 1f)]
    float movementSmoothing = 0.5f;

    [SerializeField]
    [Range(0f, 1f)]
    float rotationSmoothing = 0.5f;

    [SerializeField]
    Vector3 anchorPosition = Vector3.zero;

    [SerializeField]
    List<Transform> ignoreList = new List<Transform>();




    //Radial Variables
    [SerializeField]
    Vector2 placementCircle = new Vector2(100, 100);

    [SerializeField]
    [Range(0f, 360f)]
    float startAngle = 0f;


    [SerializeField]
    [Range(0f, 360f)]
    float maxAngle = 360f;


    //Stack Variables
    [SerializeField]
    Vector3 selectedLocalOffset = Vector3.zero;

    [SerializeField]
    Vector3 stackLocalOffset = Vector3.zero;

    [SerializeField]
    Vector3 stackDirection = new Vector3(-1, 0, 1);

    [SerializeField]
    Vector3 stackRotation = Vector3.zero;

    [SerializeField]
    float stackPadding = 5;

    [SerializeField]
    bool shouldIsolateSelected = true;


    //Scrolling Variables
    [SerializeField]
    [Range(0f, 1f)]
    float scrollValue = 0.5f;

    [SerializeField]
    float scrollPadding = 1f;



    
    Dictionary<Transform, Orientation> desiredOrientations = new Dictionary<Transform, Orientation>();

    List<Transform> activeTransforms = new List<Transform>();



    protected override void OnEnable()
    {

        CalculateActiveTransforms();


        if (HasFlag(LayoutProperties.StartAtOrigin))
        {
            for (int i = 0; i < activeTransforms.Count; i++)
            {
                activeTransforms[i].localPosition = Vector3.zero;
            }
        }

        base.OnEnable();


        CalculateLayout();
    }


    void Update()
    {

        if (Application.isEditor || EditorApplication.isPlaying || HasFlag(LayoutProperties.UpdateContinuously))
        {
            for (int i = 0; i < activeTransforms.Count; i++)
            {
                if (activeTransforms[i] != null && desiredOrientations.ContainsKey(activeTransforms[i]))
                {
                    activeTransforms[i].localPosition = Vector3.Lerp(activeTransforms[i].localPosition, desiredOrientations[activeTransforms[i]].LocalPosition, movementSmoothing);
                    //activeTransforms[i].rotation = Quaternion.Lerp(activeTransforms[i].rotation, Quaternion.Euler(desiredOrientations[activeTransforms[i]].Euler), rotationSmoothing);
                    activeTransforms[i].localEulerAngles = Vector3.Lerp(activeTransforms[i].localEulerAngles, desiredOrientations[activeTransforms[i]].LocalEuler, rotationSmoothing);
                }
            }
        }
    }
    public void SetImmediateLayout()
    {
        for (int i = 0; i < activeTransforms.Count; i++)
        {
            if (activeTransforms[i] != null && desiredOrientations.ContainsKey(activeTransforms[i]))
            {
                activeTransforms[i].localPosition = desiredOrientations[activeTransforms[i]].LocalPosition;
                activeTransforms[i].localEulerAngles = desiredOrientations[activeTransforms[i]].LocalEuler;
            }
        }
    }



    public override void SetLayoutHorizontal()
    {
    }
    public override void SetLayoutVertical()
    {
    }
    public override void CalculateLayoutInputVertical()
    {
        CalculateLayout();
    }
    public override void CalculateLayoutInputHorizontal()
    {
        CalculateLayout();
    }



#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        StackDirection = StackDirection;
        StackPadding = StackPadding;
        ScrollPadding = ScrollPadding;

        CalculateLayout();
    }
#endif




    void CalculateActiveTransforms()
    {
        activeTransforms.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);


            if (child.gameObject.activeInHierarchy && !ignoreList.Contains(child))
            {
                activeTransforms.Add(child);
            }
        }
    }



    void CalculateLayout()
    {

        if (HasFlag(LayoutProperties.Alphabetize))
        {
            List<Transform> childList = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                childList.Add(transform.GetChild(i));
            }

            childList.Sort(delegate (Transform a, Transform b)
            {
                return a.name.CompareTo(b.name);
            });

            for (int i = 0; i < childList.Count; i++)
            {
                childList[i].SetAsLastSibling();
            }
        }


        CalculateActiveTransforms();



        switch (m_LayoutStyle)
        {
            case LayoutStyle.Radial:
                CalculateRadial();
                break;
            case LayoutStyle.Stack:
                CalculateStack();
                break;
            case LayoutStyle.Scrolling:
                CalculateScrolling();
                break;
        }
    }
    void CalculateRadial()
    {
        float fOffsetAngle = maxAngle / activeTransforms.Count;
        float fAngle = startAngle;


        for (int i = activeTransforms.Count - 1; i >= 0; i--)
        {
            Vector3 vPos = Vector3.zero;
            Vector3 rotationEuler = Vector3.zero;


            switch (m_SubStyle)
            {
                case SubStyle.A:
                    vPos = new Vector3(Mathf.Cos(fAngle * Mathf.Deg2Rad), Mathf.Sin(fAngle * Mathf.Deg2Rad), 0);
                    vPos.x *= placementCircle.x;
                    vPos.y *= placementCircle.y;
                    break;
                case SubStyle.B:
                    vPos = new Vector3(Mathf.Cos(fAngle * Mathf.Deg2Rad), 0, Mathf.Sin(fAngle * Mathf.Deg2Rad));
                    vPos.x *= placementCircle.x;
                    vPos.z *= placementCircle.y;
                    break;
            }

            // Debug.Log(string.Format("{0} : {1}", m_PositioningStyle ,vPos));


            Vector3 newPos = transform.InverseTransformPoint(transform.position + anchorPosition) +  vPos;
            newPos.Scale(new Vector3(1f / transform.localScale.x, 1f / transform.localScale.y, 1f / transform.localScale.z));
            //newPos.Scale(transform.localScale);
            // transform.TransformPoint(anchorPosition) + (transform.TransformDirection(vPos).normalized * vPos.magnitude);
            // newPos += transform.right * vPos.x;
            // newPos += transform.up * vPos.y;
            // newPos += transform.forward * vPos.z;


            // Vector3 newPos2 = transform.TransformPoint(anchorPosition) + transform.TransformPoint(vPos);



            if (desiredOrientations.ContainsKey(activeTransforms[i]))
            {
                Orientation _orientation = desiredOrientations[activeTransforms[i]];
                _orientation.LocalPosition = newPos;
                _orientation.LocalEuler = rotationEuler;
                desiredOrientations[activeTransforms[i]] = _orientation;
            }
            else
            {
                desiredOrientations.Add(activeTransforms[i], new Orientation(newPos, rotationEuler, Vector3.one));
            }


           // Debug.Log(string.Format("VPos: {0}. NewPos -- World: {1}. Local: {2}", vPos, newPos, transform.InverseTransformDirection(newPos)));


            fAngle += fOffsetAngle;
        }
    }


    void CalculateStack() 
    {

        if (activeTransforms.Count == 0)
            return;

       
        Vector3 pos;
        Vector3 rotationEuler;


        pos = transform.InverseTransformPoint(transform.position + anchorPosition) + selectedLocalOffset;
        pos.Scale(new Vector3(1f / transform.localScale.x, 1f / transform.localScale.y, 1f / transform.localScale.z));

        // pos.Scale(transform.localScale);


        rotationEuler = Vector3.zero;

        int startIndex = activeTransforms.Count - (shouldIsolateSelected ? 2 : 1);


        if (shouldIsolateSelected)
        {
            if (desiredOrientations.ContainsKey(activeTransforms[activeTransforms.Count - 1]))
            {
                Orientation _orientation = desiredOrientations[activeTransforms[activeTransforms.Count - 1]];
                _orientation.LocalPosition = pos;
                _orientation.LocalEuler = rotationEuler;
                desiredOrientations[activeTransforms[activeTransforms.Count - 1]] = _orientation;
            }
            else
            {
                desiredOrientations.Add(activeTransforms[activeTransforms.Count - 1], new Orientation(selectedLocalOffset, rotationEuler, Vector3.one));
            }
        }



        int stackCount = 0;
        for(int i = startIndex; i >= 0; i--)
        {

            pos = transform.InverseTransformPoint(transform.position + anchorPosition) + stackLocalOffset + (StackDirection * stackPadding * stackCount);
            pos.Scale(new Vector3(1f / transform.localScale.x, 1f / transform.localScale.y, 1f / transform.localScale.z));
            //pos += transform.TransformPoint(anchorPosition) + transform.TransformPoint(pos);

            rotationEuler = stackRotation;



            if (desiredOrientations.ContainsKey(activeTransforms[i]))
            {
                Orientation _orientation = desiredOrientations[activeTransforms[i]];

                _orientation.LocalPosition = pos;
                _orientation.LocalEuler = rotationEuler;
                desiredOrientations[activeTransforms[i]] = _orientation;
            }
            else
            {
                desiredOrientations.Add(activeTransforms[i], new Orientation(pos, rotationEuler, Vector3.one));
            }


            stackCount++;
        }
    }
    void CalculateScrolling()
    {
        switch (m_SubStyle) {
            case SubStyle.A:
                CalculateHorizontalScroll();
                break;
            case SubStyle.B:
                CalculateVerticalScroll();
                break;
        }
    }
    void CalculateHorizontalScroll()
    {
        float totalLength = 0;

        for(int i = 0; i < activeTransforms.Count; i++)
        {
            RectTransform _rect = activeTransforms[i] as RectTransform;

            totalLength += _rect.rect.width;
        }

        totalLength += (activeTransforms.Count - 1) * scrollPadding;

        Vector3 pos = Vector3.right * totalLength * scrollValue * -1;


        for(int i = 0; i < activeTransforms.Count; i++)
        {
            RectTransform _rect = activeTransforms[i] as RectTransform;

            pos += Vector3.right * (_rect.rect.width / 2f);
            //Vector3 pos = offset + (Vector3.right * _rect.rect.width) + (Vector3.right * scrollPadding);
            Vector3 rotationEuler = Vector3.zero;


            if (desiredOrientations.ContainsKey(activeTransforms[i]))
            {
                Orientation _orientation = desiredOrientations[activeTransforms[i]];

                _orientation.LocalPosition = pos;
                _orientation.LocalEuler = rotationEuler;
                desiredOrientations[activeTransforms[i]] = _orientation;
            }
            else
            {
                desiredOrientations.Add(activeTransforms[i], new Orientation(pos, rotationEuler, Vector3.one));
            }

            pos += Vector3.right * (_rect.rect.width / 2f);
            pos += Vector3.right * scrollPadding;
        }
    }
    void CalculateVerticalScroll()
    {
        float totalLength = 0;

        for (int i = 0; i < activeTransforms.Count; i++)
        {
            RectTransform _rect = activeTransforms[i] as RectTransform;

            totalLength += _rect.rect.height;
        }

        totalLength += (activeTransforms.Count - 1) * scrollPadding;

        Vector3 pos = Vector3.up * totalLength * scrollValue * -1;


        for (int i = 0; i < activeTransforms.Count; i++)
        {
            RectTransform _rect = activeTransforms[i] as RectTransform;

            pos += Vector3.up * (_rect.rect.height / 2f);
            Vector3 rotationEuler = Vector3.zero;


            if (desiredOrientations.ContainsKey(activeTransforms[i]))
            {
                Orientation _orientation = desiredOrientations[activeTransforms[i]];

                _orientation.LocalPosition = pos;
                _orientation.LocalEuler = rotationEuler;
                desiredOrientations[activeTransforms[i]] = _orientation;
            }
            else
            {
                desiredOrientations.Add(activeTransforms[i], new Orientation(pos, rotationEuler, Vector3.one));
            }

            pos += Vector3.up * (_rect.rect.width / 2f);
            pos += Vector3.up * scrollPadding;
        }
    }
    /*
    public void RotateClockwise()
    {
        if (transform.childCount <= 1)
            return;

        transform.GetChild(0).SetAsLastSibling();

        /*
        startAngle += 360f / transform.childCount;
        startAngle %= 360;

        CalculateRadial();
        *
    }
    public void RotateCounterClockwise()
    {
        if (transform.childCount <= 1)
            return;

        transform.GetChild(transform.childCount - 1).SetAsFirstSibling();

        /*
        startAngle += 360f / transform.childCount;
        startAngle %= 360;

        CalculateRadial();
        *
    }
*/

    public void Next()
    {
        if (activeTransforms.Count <= 1)
            return;

        activeTransforms[activeTransforms.Count-1].SetAsFirstSibling();
        CalculateLayout();
    }
    public void Previous()
    {

        if (activeTransforms.Count <= 1)
            return;


        activeTransforms[0].SetAsLastSibling();
        CalculateLayout();
    }



    bool HasFlag(LayoutProperties mask)
    {
        return (m_Properties & mask) == mask;
    }








    #region Getters / Setters

    public LayoutStyle MyLayoutStyle
    {
        get { return m_LayoutStyle; }
        set { m_LayoutStyle = value; }
    }
    public SubStyle MySubStyle
    {
        get { return m_SubStyle; }
        set { m_SubStyle = value; }
    }
    public LayoutProperties MyLayoutProperties
    {
        get { return m_Properties; }
        set { m_Properties = value; }
    }


    public Vector3 StackDirection
    {
        get { return stackDirection; }
        set { stackDirection = value.normalized; }
    }
    public float StackPadding
    {
        get { return stackPadding; }
        set { stackPadding = value; }// Mathf.Max(0f, stackPadding); }
    }
    public float ScrollPadding
    {
        get { return scrollPadding; }
        set { scrollPadding = Mathf.Max(0f, scrollPadding); }
    }
    #endregion
}
