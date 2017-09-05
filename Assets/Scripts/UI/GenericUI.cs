using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FollowTarget))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(Animator))]
public class GenericUI : MonoBehaviour {


    [SerializeField]
    List<DisplayProperties> staticDisplays = new List<DisplayProperties>();

    List<DisplayProperties> activeDisplays = new List<DisplayProperties>();
  

    [Serializable]
    public class DisplayProperties
    {
        [SerializeField]
        string m_Name;
        
        [SerializeField]
        DisplayUI m_UI;



        [SerializeField]
        Orientation m_Orientation;


        public DisplayProperties(string _name, Orientation _orientation, DisplayUI _ui)
        {
            m_Name = _name;
            m_Orientation = _orientation;
            m_UI = _ui;
        }


        public string Name
        {
            get { return m_Name; }
        }
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



    [SerializeField]
    List<Tuple_StringTransform> displayParents = new List<Tuple_StringTransform>();

    [SerializeField]
    List<Tuple_StringGameobject> displayPrefabs = new List<Tuple_StringGameobject>();


    [SerializeField]
    Transform attributeParent;
 
    bool isFading = false;


    protected LineRenderer m_Line;
    protected FollowTarget m_Follow;
    protected Transform targetTransform;
    Transform mainCamTransform;
    CanvasGroup m_CanvasGroup;
    protected Animator m_Animator;
    protected Transform m_Transform;

    protected virtual void Awake()
    {
        m_Transform = GetComponent<Transform>();

        m_Animator = GetComponent<Animator>();

        m_CanvasGroup = GetComponent<CanvasGroup>();

        m_Line = GetComponent<LineRenderer>();
        m_Line.useWorldSpace = true;

        m_Follow = GetComponent<FollowTarget>();

        mainCamTransform = Camera.main.transform;
    }

    void OnEnable()
    {
        m_Follow.TargetOffset = Vector3.zero;
        
        activeDisplays.Clear();

        Alpha = 0f;
    }
    protected virtual void OnDisable()
    {
        RemoveAll();
        m_Follow.TargetTransform = null;
    }


    void Update()
    {
        m_Line.positionCount = 0;

            List<Vector3> lineVertices = new List<Vector3>();
         

            for (int i = 0; i < transform.childCount; i++)
            {
                if (!transform.GetChild(i).gameObject.activeInHierarchy)
                    continue;


                lineVertices.Add(m_Follow.TargetTransform != null ? m_Follow.TargetTransform.position : transform.position);
                lineVertices.Add(transform.GetChild(i).position);
            }

            m_Line.positionCount = lineVertices.Count;
            m_Line.SetPositions(lineVertices.ToArray());


            /*
            Vector3[] lineVertices = new Vector3[activeDisplays.Count * 2];
            m_Line.numPositions = lineVertices.Length;

            for (int i = 0; i < activeDisplays.Count; i++)
            {
                lineVertices[i * 2] = m_Follow.TargetTransform != null ? m_Follow.TargetTransform.position : transform.position;
                lineVertices[(i * 2) + 1] = activeDisplays[i].UI.transform.position;
            }

            m_Line.SetPositions(lineVertices);
            */
        
    }




    public void Initialize(Transform target, bool shouldShowLines, params DisplayProperties[] initialUIs)
    {
        targetTransform = target;
        m_Follow.TargetTransform = TargetTransform;
        m_Line.enabled = shouldShowLines;
        Alpha = 0f;
        Inflate();

        RemoveAll();

        for(int i = 0; i < initialUIs.Length; i++)
        {
            AddAttribute(initialUIs[i]);
        }
    }
    public void SetFollowOffset(Vector3 offset)
    {
        StopAllCoroutines();

        if (m_Follow != null)
        {
            m_Follow.TargetOffset = offset;
        }
    }



    public void AddAttribute(DisplayProperties newAttribute)
    {
        AddAttribute(newAttribute, attributeParent == null ? transform : attributeParent);
    }
    public void AddAttribute(DisplayProperties newAttribute, string newParentName)
    {
        AddAttribute(newAttribute, GetParentTransform(newParentName));
    }
    public void AddAttribute(DisplayProperties newAttribute, Transform newParent)
    {
        if (HasAttribute(newAttribute.Name))
            return;

       // Debug.Log(string.Format("Adding attribute: {0}. Parent : {1}.", newAttribute.Name, newParent));


        Transform _transform = newAttribute.UI.transform;
        

        Orientation _orientation = newAttribute.Orientation;

        _transform.SetParent(newParent);
        _transform.localPosition = _orientation.LocalPosition;
        _transform.localEulerAngles = _orientation.LocalEuler;
        _transform.localScale = _orientation.LocalScale;

        activeDisplays.Add(newAttribute);

    }
    public void UpdateAttribute(string attrName, float pctg, bool setImmediately)
    {
        DisplayProperties attr = GetAttribute(attrName);

        if (attr == null || attr.UI == null)
        {
            return;
           // AddAttribute(new DisplayProperties(attrName, new Orientation)
        }

        attr.UI.SetPercentage(pctg, setImmediately);
    }
    public void UpdateAttribute(string attrName, string txt)
    {
        DisplayProperties attr = GetAttribute(attrName);

        if (attr == null || attr.UI == null)
            return;

        attr.UI.SetText(txt);
    }
    public void UpdateAttribute(string attrName, Vector3 localPos)
    {
        throw new NotImplementedException();
    }
    public void RemoveAttribute(string attrName)
    {
        DisplayProperties attr = GetAttribute(attrName);

        if (attr == null)
            return;

        if(attr.UI != null)
            Destroy(attr.UI.gameObject);

        for (int i = 0; i < activeDisplays.Count; i++)
        {
            if (activeDisplays[i].Name == attrName)
            {
                activeDisplays.RemoveAt(i);
                break;
            }
        }


        //ProgressBarController controller = attr.Controller;
        //controller.FadeOutAndDestroy();

    }
    void RemoveAll()
    {
        while(activeDisplays.Count > 0)
        {
            RemoveAttribute(activeDisplays[0].Name);
        }
    }

    
    public void SetOrientation(string attrName, Orientation newOrientation)
    {
        DisplayProperties _properties = GetAttribute(attrName);

        if (_properties == null)
            return;

        _properties.Orientation = newOrientation;
    }

    public DisplayProperties GetAttribute(string attrName)
    {
        for(int i = 0; i < staticDisplays.Count; i++)
        {
            if (staticDisplays[i].Name == attrName)
                return staticDisplays[i];
        }


        for (int i = 0; i < activeDisplays.Count; i++)
        {
            if (activeDisplays[i].Name == attrName)
                return activeDisplays[i];
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


    public Transform GetParentTransform(string _name)
    {

        for (int i = 0; i < displayParents.Count; i++)
        {
            if (displayParents[i].Item1.Equals(_name))
                return displayParents[i].Item2;
        }

        return null;
    }
    public GameObject GetPrefab(string _name)
    {
        for(int i = 0; i < displayPrefabs.Count; i++)
        {
            if (displayPrefabs[i].Item1.Equals(_name))
                return Instantiate(displayPrefabs[i].Item2) as GameObject;
        }

        return null;
    }




    public virtual void Inflate()
    {
        m_Animator.SetTrigger("Inflate");
    }
    public void Deflate()
    {
        m_Animator.SetTrigger("Deflate");
    }
   public void SetInactive()
    {
        gameObject.SetActive(false);
    }




    public bool IsFading
    {
        get { return isFading; }
    }
    public virtual float Alpha
    {
        get { return m_CanvasGroup.alpha; }
        set
        {
            float a = Mathf.Clamp01(value);


            m_CanvasGroup.alpha = a;

            Gradient colorGradient = m_Line.colorGradient;
            GradientAlphaKey[] alphaKeys = colorGradient.alphaKeys;
           
            for(int i = 0; i < alphaKeys.Length; i++)
            {
                alphaKeys[i].alpha = a; 
            }

            
            colorGradient.alphaKeys = alphaKeys;
            m_Line.colorGradient = colorGradient;
        }
    }
    public Transform TargetTransform
    {
        get { return targetTransform; }
        set { targetTransform = value; }
    }





    protected virtual void OnValidate()
    {
        for (int i = 0; i < staticDisplays.Count; i++)
        {
            DisplayUI _ui = staticDisplays[i].UI;

            if (_ui == null)
                continue;


            Transform _transform = _ui.transform;

            Orientation _orientation = staticDisplays[i].Orientation;
            //_orientation.LocalPosition = _transform.localPosition;
            //_orientation.LocalEuler = _transform.localEulerAngles;
            //_orientation.LocalScale = _transform.localScale;

            _transform.localPosition = _orientation.LocalPosition;
             _transform.localEulerAngles = _orientation.LocalEuler;
            _transform.localScale = _orientation.LocalScale;


            staticDisplays[i].Orientation = _orientation;
        }

        for (int i = 0; i < activeDisplays.Count; i++)
        {
            DisplayUI _ui = activeDisplays[i].UI;

            if (_ui == null)
                continue;


            Transform _transform = _ui.transform;

            Orientation _orientation = activeDisplays[i].Orientation;
            //_orientation.LocalPosition = _transform.localPosition;
            //_orientation.LocalEuler = _transform.localEulerAngles;
            //_orientation.LocalScale = _transform.localScale;

            _transform.localPosition = _orientation.LocalPosition;
            _transform.localEulerAngles = _orientation.LocalEuler;
            _transform.localScale = _orientation.LocalScale;



            activeDisplays[i].Orientation = _orientation;
        }
    }
}
