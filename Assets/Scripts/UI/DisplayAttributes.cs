using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FollowTarget))]
[RequireComponent(typeof(LineRenderer))]
public class DisplayAttributes : MonoBehaviour
{
    
    [SerializeField]
    List<DisplayProperties> activeAttributes = new List<DisplayProperties>();


    [SerializeField]
    bool shouldDrawLines = true;


    [System.Serializable]
    public class DisplayProperties
    {
        [SerializeField]
        string m_Name;

        // [SerializeField]
        //  Transform m_Transform;

        [SerializeField]
        DisplayUI m_UI;



        [SerializeField]
        Orientation m_Orientation;


        public DisplayProperties(string _name, Orientation _orientation, DisplayUI _ui)
        {
            m_Name = _name;
           // m_Transform = _transform;
            m_Orientation = _orientation;
            m_UI = _ui;
        }


        public string Name
        {
            get { return m_Name; }
        }
        /*
        public Transform Transform
        {
            get { return m_Transform; }
        }
        */
        public DisplayUI UI
        {
            get { return m_UI; }
        }
        public Orientation Orientation
        {
            get { return m_Orientation; }
            set { m_Orientation = value; }
        }
      
    }





    LineRenderer m_Line;

    void Awake()
    {
        m_Line = GetComponent<LineRenderer>();
        m_Line.useWorldSpace = true;
    }

    void OnEnable()
    {
        List<DisplayProperties> initialAttr = activeAttributes;
        activeAttributes = new List<DisplayProperties>();
        for(int i = 0; i < initialAttr.Count; i++)
        {
            if (initialAttr[i].UI == null)
                continue;


            AddAttribute(initialAttr[i], initialAttr[i].UI.transform.parent);
        }
    }
    void Update()
    {
        m_Line.positionCount = 0;

        if (shouldDrawLines)
        {
            Vector3[] lineVertices = new Vector3[activeAttributes.Count * 2];
            m_Line.positionCount = lineVertices.Length;

            for(int i = 0; i < activeAttributes.Count; i++)
            {
                lineVertices[i * 2] = transform.position;
                lineVertices[(i * 2) + 1] = activeAttributes[i].UI.transform.position;
            }

            m_Line.SetPositions(lineVertices);
        }
    }





    public void AddAttribute(DisplayProperties newAttribute)
    {
        AddAttribute(newAttribute, transform);
    }
    public void AddAttribute(DisplayProperties newAttribute, Transform newParent)
    {
        if (HasAttribute(newAttribute.Name))
            return;


        Transform _transform = newAttribute.UI.transform;
        Orientation _orientation = newAttribute.Orientation;

        _transform.SetParent(newParent);
        _transform.localPosition = _orientation.LocalPosition;
        _transform.localEulerAngles = _orientation.LocalEuler;
        _transform.localScale = _orientation.LocalScale;

        activeAttributes.Add(newAttribute);

    }
    public void UpdateAttribute(string attrName, float pctg, bool setImmediately)
    {
        DisplayProperties attr = GetAttribute(attrName);

        if (attr == null || attr.UI == null)
            return;
        
         attr.UI.SetPercentage(pctg,setImmediately);
    }
    public void UpdateAttribute(string attrName, Vector3 localPos)
    {
        //throw new NotImplementedException();
    }
    public void RemoveAttribute(string attrName)
    {
        DisplayProperties attr = GetAttribute(attrName);

        if (attr == null)
            return;


      

        Destroy(attr.UI.gameObject);
        
        for(int i = 0; i < activeAttributes.Count; i++)
        {
            if(activeAttributes[i].Name == attrName)
            {
                activeAttributes.RemoveAt(i);
                break; 
            }
        }


        //ProgressBarController controller = attr.Controller;
        //controller.FadeOutAndDestroy();

    }



    DisplayProperties GetAttribute(string attrName)
    {
        for (int i = 0; i < activeAttributes.Count; i++)
        {
            if (activeAttributes[i].Name == attrName)
                return activeAttributes[i];
        }


       // Debug.Log(attrName + " not found.");
        return null;
    }
    public Transform GetAttributeTransform(string attrName)
    {
        DisplayProperties attr = GetAttribute(attrName);

        return attr == null ? null : attr.UI.transform;
    }
    public DisplayUI GetAttributeController(string attrName)
    {
        DisplayProperties attr = GetAttribute(attrName);

        return attr == null ? null : attr.UI;
    }
    public bool HasAttribute(string attrName)
    {
        return GetAttribute(attrName) != null;
    }




    void OnValidate()
    {
        for(int i = 0; i < activeAttributes.Count; i++)
        {
            DisplayUI _ui = activeAttributes[i].UI;
            
            if (_ui == null)
                continue;


            Transform _transform = _ui.transform;

            Orientation _orientation = activeAttributes[i].Orientation;
            _orientation.LocalPosition = _transform.localPosition;
            _orientation.LocalEuler = _transform.localEulerAngles;
            _orientation.LocalScale = _transform.localScale;

            activeAttributes[i].Orientation = _orientation;
        }
    }
}
