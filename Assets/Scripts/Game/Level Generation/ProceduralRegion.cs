using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(EnvironmentArea))]
public class ProceduralRegion : MonoBehaviour {
    
    [Tooltip("Should ProceduralRegion be placed?")]
    [SerializeField]
    bool shouldPlace = true;


    [Tooltip("Type of region")]
    [SerializeField]
    [EnumFlags]
    NodeType environmentType = NodeType.BasicGround;

    
    [Tooltip("Used for sorting to determine order of region placement")]
    [SerializeField]
    [Range(0, 3)]
    int placementPriority = 3;

    [Tooltip("Prosperity rating based on temperature")]
    [SerializeField]
    AnimationCurve temperatureRating = AnimationCurve.Linear(0f, 0f, 100f, 1f);

    [Tooltip("Prosperity rating based on moisture")]
    [SerializeField]
    AnimationCurve moistureRating = AnimationCurve.Linear(0f, 0f, 100f, 1f);

    [Tooltip("Scaling to apply to region to improve performance")]
    [SerializeField]
    [Range(1, 10)]
    int scaleFactor = 4;

    [Tooltip("Does region need to be on land?")]
    [SerializeField]
    bool needsLand = true;

    [Tooltip("Should region leave a mark?")]
    [SerializeField]
    bool shouldSignMap = true;


    [Tooltip("Probability of placement based on NodeType")]
    [SerializeField]
    List<NodeTypeProbability> companionNodes = new List<NodeTypeProbability>();


    [Tooltip("Level of smoothing to apply to region")]
    [SerializeField]
    [Range(0,10)]
    int smoothingLevel = 2;

    [Tooltip("Should add collider to region?")]
    [SerializeField]
    bool shouldAddCollider = true;




    [System.Serializable]
    public class NodeTypeProbability
    {
        [SerializeField]
        NodeType m_Type;

        [SerializeField]
        AnimationCurve heightProbabilityCurve = AnimationCurve.Linear(0f, 0f, Values.MAXIMUM_MAP_HEIGHT, 0f);


        public NodeTypeProbability(NodeType _type, AnimationCurve _probability)
        {
            m_Type = _type;
            heightProbabilityCurve = _probability;
        }

        public float GetProbability(float heightVal)
        {
            return heightProbabilityCurve.Evaluate(heightVal);
        }


        public NodeType Type
        {
            get { return m_Type; }
        }



        public void Validate()
        {
            Utilities.ValidateCurve_Times(heightProbabilityCurve, 0f, Values.MAXIMUM_MAP_HEIGHT);
        }
    }




    void Awake()
    {
        environmentType = GetComponent<EnvironmentArea>().EnvironmentType;
    }
    

    public GameObject GetObject(int[][] _map, int scaleAmount)
    {
        GameObject regionObj = (GameObject)GameObject.Instantiate(this.gameObject);
        regionObj.transform.position = Vector3.zero;

        MeshGenerator.ColliderAdditionType collAddition = ShouldAddCollider ? MeshGenerator.ColliderAdditionType.MeshCollider : MeshGenerator.ColliderAdditionType.None;
        MeshGenerator.Instance.GenerateMesh(regionObj, null, _map, scaleAmount, collAddition);
      
        return regionObj;
    }
    public GameObject GetObject(bool[][] _map, int scaleAmount)
    {
        GameObject regionObj = (GameObject)GameObject.Instantiate(this.gameObject);
        regionObj.transform.position = Vector3.zero;

        MeshGenerator.ColliderAdditionType collAddition = ShouldAddCollider ? MeshGenerator.ColliderAdditionType.MeshCollider : MeshGenerator.ColliderAdditionType.None;
        MeshGenerator.Instance.GenerateMesh(regionObj, null, _map, scaleAmount, collAddition);


        return regionObj;
    }



    public float GetClimateMultiplier(float temperature, float moisture)
    {
        if (!shouldPlace)
            return 0;

        return temperatureRating.Evaluate(temperature) * moistureRating.Evaluate(moisture);
    }






    public int Compare(ProceduralRegion r)
    {
        int diff = this.PlacementPriority - r.PlacementPriority;

        return diff == 0 ? (Random.value <= 0.5f ? -1 : 1) : diff;
    }



    #region Accessors

    public bool NeedsLand
    {
        get { return needsLand; }
    }
    public bool ShouldSignMap
    {
        get { return shouldSignMap; }
    }
    public bool ShouldAddCollider
    {
        get{ return shouldAddCollider; }
    }

    public int PlacementPriority
    {
        get { return placementPriority; }
    }

    public int ScaleFactor
    {
        get { return scaleFactor; }
    }
    public int SmoothingLevel
    {
        get { return smoothingLevel; }
    }
    public NodeType EnvironmentType
    {
        get { return environmentType; }
    }

    public List<NodeTypeProbability> CompanionNodes
    {
        get { return companionNodes; }
    }

    #endregion


    void OnValidate()
    {
        Utilities.ValidateCurve(temperatureRating, 0f, 100f,0f, float.MaxValue);
        Utilities.ValidateCurve(moistureRating, 0f, 100f, 0f, float.MaxValue);


        companionNodes.ForEach(n => n.Validate());
    }
}
