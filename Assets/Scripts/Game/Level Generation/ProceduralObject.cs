using UnityEngine;
using System.Collections.Generic;

public class ProceduralObject : MonoBehaviour {

   static readonly double POPULATION_MULTIPLIER = 0.0000025;
    static readonly float MIN_SCALE = 0.2f;

    [Tooltip("Should ProceduralObject be placed?")]
    [SerializeField]
    bool shouldPlace = true;

    [Tooltip("ID # for Object identification")]
    [SerializeField]
    int m_ObjectID = 0;


    [Tooltip("Acceptable NodeTypes to place object on.")]
    [SerializeField]
    [EnumFlags]
    NodeType viablePlacementNodes = NodeType.BasicGround;


    [Tooltip("Determines probability of placement for each possible height")]
    [SerializeField]
    AnimationCurve heightProbabilityCurve = AnimationCurve.Linear(0f, 0f, Values.MAXIMUM_MAP_HEIGHT, 0f);

    [Tooltip("If probability of a given height is above this value, height is considered valid")]
    [SerializeField]
    [Range(0f, 1f)]
    float validProbabilityCutOff = 0.5f;


    [Tooltip("Used for sorting to determine order of region placement")]
    [SerializeField]
    [Range(0, 3)]
    int placementPriority = 3;

    [Tooltip("IDs that are acceptable to place object in same area as.")]
    [SerializeField]
    List<int> companionIDS = new List<int>();

    [Tooltip("Should check to ensure no area conflicts?")]
    [SerializeField]
    bool shouldCheckArea = true;

    [Tooltip("Should mark area with ID?")]
    [SerializeField]
    bool shouldLeaveMark = true;

    [Tooltip("Probability object aligns with angle of ground")]
    [SerializeField]
    [Range(0f, 1f)]
    float chanceAlignWithGround = 0f;

    [Tooltip("Prosperity rating based on temperature")]
    [SerializeField]
    AnimationCurve temperatureProsperity = AnimationCurve.Linear(0f, 0f, 100f, 1f);

    [Tooltip("Prosperity rating based on moisture")]
    [SerializeField]
    AnimationCurve moistureProsperity = AnimationCurve.Linear(0f, 0f, 100f, 1f);

    [Tooltip("Population percentage based on prosperity. Should be placed in range [0,100]. Resulting value will be artificially diluted by a constant.")]
    [SerializeField]
    DeviatingAnimationCurve m_PopulationDeviation;

    [Tooltip("Maximum population to be placed. A negative number means no max will be enforced.")]
    [SerializeField]
    int populationMax = -1;

    [Tooltip("Should scale population to area size?")]
    [SerializeField]
    bool shouldApplyPopulationMultiplication = true;


    enum ScalingMethod { None, Individual, Universal };

    [Tooltip("Method of scaling object")]
    [SerializeField]
    ScalingMethod my_ScalingMethod = ScalingMethod.Universal;


    [Tooltip("X-axis scaling based on prosperity")]
    [SerializeField]
    DeviatingAnimationCurve m_SizeXDeviation;

    [Tooltip("Y-axis scaling based on prosperity")]
    [SerializeField]
    DeviatingAnimationCurve m_SizeYDeviation;

    [Tooltip("Z-axis scaling based on prosperity")]
    [SerializeField]
    DeviatingAnimationCurve m_SizeZDeviation;

    [Tooltip("Universal axis scaling based on prosperity")]
    [SerializeField]
    DeviatingAnimationCurve m_SizeUniversalDeviation;

   


    [SerializeField]
    Gradient temperatureGradient = new Gradient();

    [SerializeField]
    Gradient moistureGradient = new Gradient();




    public GameObject GetObject(float temp, float moisture)
    {
        GameObject newObj = Instantiate(gameObject);
       // Destroy(newObj.GetComponent<ProceduralObject>());

        float climateMultiplier = temperatureProsperity.Evaluate(Mathf.Clamp(temp, 0, 100)) * moistureProsperity.Evaluate(Mathf.Clamp(moisture, 0, 100));  // GetClimateRating(temp, moisture);

        float xScale = 1f;
        float yScale = 1f;
        float zScale = 1f;

        switch (my_ScalingMethod)
        {
            case ScalingMethod.Individual:
                xScale = +(float)Utilities.GetRandomGaussian(m_SizeXDeviation.Mean.Evaluate(climateMultiplier), m_SizeXDeviation.Sigma.Evaluate(climateMultiplier));
                yScale = (float)Utilities.GetRandomGaussian(m_SizeYDeviation.Mean.Evaluate(climateMultiplier), m_SizeYDeviation.Sigma.Evaluate(climateMultiplier));
                zScale = (float)Utilities.GetRandomGaussian(m_SizeZDeviation.Mean.Evaluate(climateMultiplier), m_SizeZDeviation.Sigma.Evaluate(climateMultiplier));

                if (xScale < MIN_SCALE)
                    xScale = MIN_SCALE;

                if (yScale < MIN_SCALE)
                    yScale = MIN_SCALE;

                if (zScale < MIN_SCALE)
                    zScale = MIN_SCALE;
                break;
            case ScalingMethod.Universal:

                float _universal = (float)Utilities.GetRandomGaussian(m_SizeUniversalDeviation.Mean.Evaluate(climateMultiplier), m_SizeUniversalDeviation.Sigma.Evaluate(climateMultiplier));

                if (_universal < MIN_SCALE)
                    _universal = MIN_SCALE;

                xScale = _universal;
                yScale = _universal;
                zScale = _universal;

                break;
        }


        newObj.transform.localScale = new Vector3(xScale, yScale, zScale);

        return newObj;
    }

    public double GetPopulationPercentage(float temp, float moisture)
    {

        if (!shouldPlace)
            return 0;


        if (m_PopulationDeviation == null)
            return 0;

        if (m_PopulationDeviation.Mean == null || m_PopulationDeviation.Sigma == null)
            return 0;



        float climateMultiplier = GetClimateRating(temp, moisture);

        if (climateMultiplier.Equals(0f))
        {
            return 0;
        }



        float _mean = m_PopulationDeviation.Mean.Evaluate(climateMultiplier);
        float _sigma = m_PopulationDeviation.Sigma.Evaluate(climateMultiplier);

        double _population = Utilities.GetRandomGaussian(_mean, _sigma);

        if (shouldApplyPopulationMultiplication)
        {
            _population *= POPULATION_MULTIPLIER;
        }


        if (_population <= 0)
            _population = 0;

        if (_population > 1)
            _population = 1;

        return _population;
    }
   /* public CompanionObject<float,float>[] GetSizeVariances(float temp, float moisture)
    {
        CompanionObject<float, float>[] sizeVariances = new CompanionObject<float, float>[4];

        float climateMultiplier = temperatureProsperity.Evaluate(Mathf.Clamp(temp, 0, 100)) * moistureProsperity.Evaluate(Mathf.Clamp(moisture, 0, 100));  // GetClimateRating(temp, moisture);
        
        sizeVariances[0] = new CompanionObject<float, float>(sizeX_Mean.Evaluate(climateMultiplier), sizeX_Sigma.Evaluate(climateMultiplier));
        sizeVariances[1] = new CompanionObject<float, float>(sizeY_Mean.Evaluate(climateMultiplier), sizeY_Sigma.Evaluate(climateMultiplier));
        sizeVariances[2] = new CompanionObject<float, float>(sizeZ_Mean.Evaluate(climateMultiplier), sizeZ_Sigma.Evaluate(climateMultiplier));
        sizeVariances[3] = new CompanionObject<float, float>(sizeUniversal_Mean.Evaluate(climateMultiplier), sizeUniversal_Sigma.Evaluate(climateMultiplier));

        return sizeVariances;
    }*/
    public Color GetColor(float temp, float moisture)
    {
        Color tempColor = temperatureGradient.Evaluate(temp);

        Color moistureColor = moistureGradient.Evaluate(moisture);


   
        return (tempColor + moistureColor) / 2;
    }


    public float GetClimateRating(float temp, float moisture)
    {
        return Mathf.Clamp01(temperatureProsperity.Evaluate(Mathf.Clamp(temp, 0, 100)) * moistureProsperity.Evaluate(Mathf.Clamp(moisture, 0, 100))); 
    }


    public float GetProbability(float heightVal)
    {
        return heightProbabilityCurve.Evaluate(heightVal);
    }
   
    public bool IsValidHeight(float heightVal)
    {
        return GetProbability(heightVal) > validProbabilityCutOff;
    }

    public int Compare(ProceduralObject b)
    {

        int diff = this.PlacementPriority - b.PlacementPriority;

        return diff == 0 ? (Random.value <= 0.5f ? -1 : 1) : diff;

    }



    #region Accessors

    public int ObjectID
    {
        get { return m_ObjectID; }
    }
    public NodeType ViablePlacementNodes
    {
        get { return viablePlacementNodes; }
        set { viablePlacementNodes = value; }
    }
    public int PlacementPriority
    {
        get { return placementPriority; }
    }
    public List<int> CompanionIDs
    {
        get { return companionIDS; }
    }

    public bool ShouldCheckArea
    {
        get { return shouldCheckArea; }
    }
    public bool ShouldLeaveMark
    {
        get { return shouldLeaveMark; }
    }

    public float ChanceAlignWithGround
    {
        get { return chanceAlignWithGround; }
    }
    public int PopulationMax
    {
        get { return populationMax < 0 ? int.MaxValue : populationMax; }
    }

    #endregion



    void ValidateCurves()
    {
        Utilities.ValidateCurve(heightProbabilityCurve, 0f, Values.MAXIMUM_MAP_HEIGHT,0f, float.MaxValue);

        Utilities.ValidateCurve(temperatureProsperity, 0f, 100f, 0f, float.MaxValue);
        Utilities.ValidateCurve(moistureProsperity, 0f, 100f, 0f, float.MaxValue);

        Utilities.ValidateCurve_Times(m_PopulationDeviation.Mean, 0f, 1f);
        Utilities.ValidateCurve_Times(m_PopulationDeviation.Sigma, 0f, 1f);

        Utilities.ValidateCurve_Times(m_SizeXDeviation.Mean, 0f, 1f);
        Utilities.ValidateCurve_Times(m_SizeXDeviation.Sigma, 0f, 1f);
        Utilities.ValidateCurve_Times(m_SizeYDeviation.Mean, 0f, 1f);
        Utilities.ValidateCurve_Times(m_SizeYDeviation.Sigma, 0f, 1f);
        Utilities.ValidateCurve_Times(m_SizeZDeviation.Mean, 0f, 1f);
        Utilities.ValidateCurve_Times(m_SizeZDeviation.Sigma, 0f, 1f);
        Utilities.ValidateCurve_Times(m_SizeUniversalDeviation.Mean, 0f, 1f);
        Utilities.ValidateCurve_Times(m_SizeUniversalDeviation.Sigma, 0f, 1f);

    }
    void OnValidate()
    {
        ValidateCurves();
    }
}
