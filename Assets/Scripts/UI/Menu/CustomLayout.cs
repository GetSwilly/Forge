using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CustomLayout : LayoutGroup
{


    public enum LayoutStyle
    {
        Vertical,
        Horizontal,
        Radial
    }

    [System.Flags]
    public enum LayoutProperties
    {
        StartAtOrigin = 1,
        UpdateContinuously = 2,
        Alphabetize = 4
    }





    [SerializeField]
    public LayoutStyle m_LayoutStyle = LayoutStyle.Vertical;

    [SerializeField]
    [EnumFlags]
    public LayoutProperties m_Properties;

    [SerializeField]
    MovementType m_MovementType = MovementType.Lerp;

    [SerializeField]
    float smoothing;

    [SerializeField]
    float speed;
  

    [SerializeField]
    Vector3 originPosition = Vector3.zero;

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

    [SerializeField]
    bool shouldIsolateSelected = true;

    [SerializeField]
    float spacing;

    [SerializeField]
    [Range(0f, 1f)]
    float percentage = 0.5f;

    [SerializeField]
    bool reverseOrder = false;

    //Scrolling Variables
    [SerializeField]
    [Range(0f, 1f)]
    float scrollValue = 0.5f;

    [SerializeField]
    float scrollPadding = 1f;


    Dictionary<Transform, Vector3> desiredPositions = new Dictionary<Transform, Vector3>();

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
                if (activeTransforms[i] != null && desiredPositions.ContainsKey(activeTransforms[i]))
                {
                    switch (m_MovementType)
                    {
                        case MovementType.Lerp:
                            activeTransforms[i].localPosition = Vector3.Lerp(activeTransforms[i].localPosition, desiredPositions[activeTransforms[i]], Smoothing * Time.deltaTime);
                            break;
                        case MovementType.MoveTowards:
                            activeTransforms[i].localPosition = Vector3.MoveTowards(activeTransforms[i].localPosition, desiredPositions[activeTransforms[i]], Speed * Time.deltaTime);
                            break;
                    }
                }
            }
        }
    }

    public void SetImmediateLayout()
    {
        for (int i = 0; i < activeTransforms.Count; i++)
        {
            if (activeTransforms[i] != null && desiredPositions.ContainsKey(activeTransforms[i]))
            {
                activeTransforms[i].localPosition = desiredPositions[activeTransforms[i]];
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


    protected override void OnValidate()
    {
        base.OnValidate();

        ScrollPadding = ScrollPadding;
        Spacing = Spacing;

        Smoothing = Smoothing;
        Speed = Speed;

        CalculateLayout();
    }




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

        if (reverseOrder)
        {
            activeTransforms.Reverse();
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
            case LayoutStyle.Horizontal:
                CalculateStackHorizontal();
                break;
            case LayoutStyle.Vertical:
                CalculateStackVertical();
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

            vPos = new Vector3(Mathf.Cos(fAngle * Mathf.Deg2Rad), Mathf.Sin(fAngle * Mathf.Deg2Rad), 0);
            vPos.x *= placementCircle.x;
            vPos.y *= placementCircle.y;


            //switch (m_SubStyle)
            //{
            //    case SubStyle.A:
            //        vPos = new Vector3(Mathf.Cos(fAngle * Mathf.Deg2Rad), Mathf.Sin(fAngle * Mathf.Deg2Rad), 0);
            //        vPos.x *= placementCircle.x;
            //        vPos.y *= placementCircle.y;
            //        break;
            //    case SubStyle.B:
            //        vPos = new Vector3(Mathf.Cos(fAngle * Mathf.Deg2Rad), 0, Mathf.Sin(fAngle * Mathf.Deg2Rad));
            //        vPos.x *= placementCircle.x;
            //        vPos.z *= placementCircle.y;
            //        break;
            //}


            Vector3 newPos = transform.InverseTransformPoint(transform.position + originPosition) + vPos;
            newPos.Scale(new Vector3(1f / transform.localScale.x, 1f / transform.localScale.y, 1f / transform.localScale.z));

            SetDesiredPositon(activeTransforms[i], newPos);

            fAngle += fOffsetAngle;
        }
    }


    //void CalculateStack() 
    //{
    //    if (activeTransforms.Count == 0)
    //        return;

    //    Vector3 currentPosition = Vector3.zero;

    //    //for(int i = activeTransforms.Count - 1 ; i >= 0; i--)
    //    //{

    //    //    pos = transform.InverseTransformPoint(transform.position + originPosition) + stackLocalOffset + (StackDirection * stackPadding * stackCount);
    //    //    pos.Scale(new Vector3(1f / transform.localScale.x, 1f / transform.localScale.y, 1f / transform.localScale.z));
    //    //    //pos += transform.TransformPoint(anchorPosition) + transform.TransformPoint(pos);

    //    //    rotationEuler = stackRotation;



    //    //    if (desiredOrientations.ContainsKey(activeTransforms[i]))
    //    //    {
    //    //        Orientation _orientation = desiredOrientations[activeTransforms[i]];

    //    //        _orientation.LocalPosition = pos;
    //    //        _orientation.LocalEuler = rotationEuler;
    //    //        desiredOrientations[activeTransforms[i]] = _orientation;
    //    //    }
    //    //    else
    //    //    {
    //    //        desiredOrientations.Add(activeTransforms[i], new Orientation(pos, rotationEuler, Vector3.one));
    //    //    }


    //    //    stackCount++;
    //    //}
    //}
    void CalculateStackHorizontal()
    {
        if (activeTransforms.Count == 0)
            return;

        float adjustmentValue = 0f;
        for (int i = 0; i < activeTransforms.Count; i++)
        {
            RectTransform rectT = activeTransforms[i] as RectTransform;

            adjustmentValue += rectT.rect.width;

            if (i < activeTransforms.Count - 1)
            {
                adjustmentValue += Spacing;
            }
        }
        Vector3 adjustmentVector = new Vector3(adjustmentValue * percentage, 0f);


        Vector3 currentPosition = originPosition;

        for (int i = 0; i < activeTransforms.Count; i++)
        {
            RectTransform rectT = activeTransforms[i] as RectTransform;

            float width = rectT.rect.width;

            currentPosition += Vector3.right * (width / 2f);

            SetDesiredPositon(activeTransforms[i], currentPosition - adjustmentVector);

            currentPosition += Vector3.right * (width / 2f);
            currentPosition += Vector3.right * Spacing;
        }
    }
    void CalculateStackVertical()
    {
        if (activeTransforms.Count == 0)
            return;

        float adjustmentValue = 0f;
        for (int i = 0; i < activeTransforms.Count; i++)
        {
            RectTransform rectT = activeTransforms[i] as RectTransform;

            adjustmentValue += rectT.rect.height;

            if (i < activeTransforms.Count - 1)
            {
                adjustmentValue += Spacing;
            }
        }
        Vector3 adjustmentVector = new Vector3(0f, adjustmentValue * percentage);

        // Debug.DrawLine(this.transform.position, this.transform.TransformPoint(adjustmentVector), Color.magenta, 3f);

        Vector3 currentPosition = originPosition;

        for (int i = 0; i < activeTransforms.Count; i++)
        {
            RectTransform rectT = activeTransforms[i] as RectTransform;

            float height = rectT.rect.height;

            currentPosition += Vector3.up * (height / 2f);

            SetDesiredPositon(activeTransforms[i], currentPosition - adjustmentVector);

            currentPosition += Vector3.up * (height / 2f);
            currentPosition += Vector3.up * Spacing;
        }
    }


    //void CalculateScrolling()
    //{
    //    switch (m_SubStyle) {
    //        case SubStyle.A:
    //            CalculateHorizontalScroll();
    //            break;
    //        case SubStyle.B:
    //            CalculateVerticalScroll();
    //            break;
    //    }
    //}
    //void CalculateHorizontalScroll()
    //{
    //    float totalLength = 0;

    //    for (int i = 0; i < activeTransforms.Count; i++)
    //    {
    //        RectTransform _rect = activeTransforms[i] as RectTransform;

    //        totalLength += _rect.rect.width;
    //    }

    //    totalLength += (activeTransforms.Count - 1) * scrollPadding;

    //    Vector3 pos = Vector3.right * totalLength * scrollValue * -1;


    //    for (int i = 0; i < activeTransforms.Count; i++)
    //    {
    //        RectTransform _rect = activeTransforms[i] as RectTransform;

    //        pos += Vector3.right * (_rect.rect.width / 2f);
    //        //Vector3 pos = offset + (Vector3.right * _rect.rect.width) + (Vector3.right * scrollPadding);
    //        Vector3 rotationEuler = Vector3.zero;


    //        if (desiredOrientations.ContainsKey(activeTransforms[i]))
    //        {
    //            Orientation _orientation = desiredOrientations[activeTransforms[i]];

    //            _orientation.LocalPosition = pos;
    //            _orientation.LocalEuler = rotationEuler;
    //            desiredOrientations[activeTransforms[i]] = _orientation;
    //        }
    //        else
    //        {
    //            desiredOrientations.Add(activeTransforms[i], new Orientation(pos, rotationEuler, Vector3.one));
    //        }

    //        pos += Vector3.right * (_rect.rect.width / 2f);
    //        pos += Vector3.right * scrollPadding;
    //    }
    //}
    //void CalculateVerticalScroll()
    //{
    //    float totalLength = 0;

    //    for (int i = 0; i < activeTransforms.Count; i++)
    //    {
    //        RectTransform _rect = activeTransforms[i] as RectTransform;

    //        totalLength += _rect.rect.height;
    //    }

    //    totalLength += (activeTransforms.Count - 1) * scrollPadding;

    //    Vector3 pos = Vector3.up * totalLength * scrollValue * -1;


    //    for (int i = 0; i < activeTransforms.Count; i++)
    //    {
    //        RectTransform _rect = activeTransforms[i] as RectTransform;

    //        pos += Vector3.up * (_rect.rect.height / 2f);
    //        Vector3 rotationEuler = Vector3.zero;


    //        if (desiredOrientations.ContainsKey(activeTransforms[i]))
    //        {
    //            Orientation _orientation = desiredOrientations[activeTransforms[i]];

    //            _orientation.LocalPosition = pos;
    //            _orientation.LocalEuler = rotationEuler;
    //            desiredOrientations[activeTransforms[i]] = _orientation;
    //        }
    //        else
    //        {
    //            desiredOrientations.Add(activeTransforms[i], new Orientation(pos, rotationEuler, Vector3.one));
    //        }

    //        pos += Vector3.up * (_rect.rect.width / 2f);
    //        pos += Vector3.up * scrollPadding;
    //    }
    //}
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

    void SetDesiredPositon(Transform t, Vector3 localPosition)
    {
        if (desiredPositions.ContainsKey(t))
        {
            desiredPositions[t] = localPosition;
        }
        else
        {
            desiredPositions.Add(t, localPosition);
        }
    }

    public void Next()
    {
        if (activeTransforms.Count <= 1)
            return;

        activeTransforms[activeTransforms.Count - 1].SetAsFirstSibling();
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








    #region Accessors

    public LayoutStyle Style
    {
        get { return m_LayoutStyle; }
        set { m_LayoutStyle = value; }
    }
    public LayoutProperties Properties
    {
        get { return m_Properties; }
        set { m_Properties = value; }
    }

    public float ScrollPadding
    {
        get { return scrollPadding; }
        set { scrollPadding = Mathf.Max(0f, scrollPadding); }
    }
    public float Spacing
    {
        get { return spacing; }
        private set { spacing = Mathf.Clamp(value, 0f, value); }
    }

    public float Smoothing
    {
        get { return smoothing; }
        private set { smoothing = Mathf.Clamp(value, 0f, value); }
    }
    public float Speed
    {
        get { return speed; }
        private set { speed = Mathf.Clamp(value, 0f, value); }
    }
    #endregion
}