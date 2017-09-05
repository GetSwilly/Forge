using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WeatherSystem : MonoBehaviour {

    static readonly float LIGHT_EULER_Y_DELTA = 0.01f;



    [Header("Time of Day")]
    [Space(5)]

    [Tooltip("Current Time value")]
    [SerializeField]
    [Range(0f, 100f)]
    float currentTime = 50f;

    [Tooltip("Rate at which Time of Day changes")]
    [SerializeField]
    [Range(0f, 20f)]
    float timeRate = 0f;

    [Tooltip("Tilt of light on local x-axis based on CurrentTime")]
    [SerializeField]
    AnimationCurve lightAngleCurve = AnimationCurve.Linear(0f, 0f, 100f, 1f);

    [Tooltip("Intensity of light based on CurrentTime")]
    [SerializeField]
    AnimationCurve lightIntensityCurve = AnimationCurve.Linear(0f, 0f, 100f, 1f);

    [SerializeField]
    Gradient lightColorGradient = new Gradient();


    [SerializeField]
    Light worldLight;


    [Space(15)]
    [Header("Weather")]
    [Space(5)]

    //  [SerializeField]
    // AnimationCurve weatherIntensityCurve = AnimationCurve.Linear(0f, 50f, 100f, 50f);

    [SerializeField]
    [Range(0f, 100f)]
    float weatherIntensity = 25f;

    [Tooltip("Change in Weather intensity over time.")]
    [SerializeField]
    float weatherIntensityDelta;
    

    [SerializeField]
    CloudSystem m_Cloud;

    [SerializeField]
    WindSystem m_Wind;

    [HideInInspector]
    public static WeatherSystem Instance;
    void Awake()
    {
        Instance = this;

        m_Cloud.Setup(transform);
        m_Wind.Setup(transform);
    }




    public void Initialize()
    {
        Terminate();

        WeatherIntensity = Random.Range(0f, 100f);

        StartCoroutine(WeatherUpdate());
        StartCoroutine(TimeCycle());

        m_Cloud.Initialize();
        m_Wind.Initialize();
    }
    public void Terminate()
    {
        StopAllCoroutines();

        m_Cloud.Terminate();
        m_Wind.Terminate();
    }




    #region Time of Day & Lighting
    
    IEnumerator TimeCycle()
    {
        while (true)
        {
            yield return null;

            CurrentTime += timeRate * Time.deltaTime;
            CurrentTime %= 100;

            SetTimeOfDay(false);
        }
    }
    public void SetTimeOfDay(bool shouldRecalculateYRotation)
    {
        SetTimeOfDay(CurrentTime, shouldRecalculateYRotation);
    }
    public void SetTimeOfDay(float newTime, bool shouldRecalculateYRotation)
    {
        if (worldLight == null)
            return;


        CurrentTime = newTime;


        float angle = lightAngleCurve.Evaluate(currentTime);
        float intensity = lightIntensityCurve.Evaluate(currentTime);


        Transform _transform = worldLight.transform;

        _transform.rotation = Quaternion.identity;

        float yRotation = shouldRecalculateYRotation ? UnityEngine.Random.Range(0f, 360f) : worldLight.transform.eulerAngles.y + LIGHT_EULER_Y_DELTA;// : (pseudoRandom == null ? UnityEngine.Random.Range(0f, 360f) : (float)(pseudoRandom.NextDouble() * 360f));

        _transform.Rotate(angle, yRotation, 0f);


        /*
        if (pseudoRandom != null)
        {
            _transform.Rotate(angle, (float)(pseudoRandom.NextDouble() * 360f), 0f);
        }
        else
        {
            _transform.Rotate(angle, UnityEngine.Random.Range(0f, 360f), 0f);
        }
        */

        Light _light = worldLight.GetComponent<Light>();
        _light.intensity = intensity;
        _light.color = lightColorGradient.Evaluate(currentTime / 100f);
    }

    #endregion






    IEnumerator WeatherUpdate()
    {
        while (true)
        {
            yield return null;


            float updateVal = weatherIntensityDelta * Time.deltaTime;
            if (Random.value < 0.5f)
                updateVal *= -1;

            WeatherIntensity += updateVal;

            m_Wind.Update(Time.deltaTime);
            m_Cloud.Update(Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Weather Intensity is locked within interval [0, 100]
    /// </summary>
    public float WeatherIntensity
    {
        get { return weatherIntensity; }
        set
        {
            weatherIntensity = value;

            if (weatherIntensity < 0)
                weatherIntensity = 0;

            if (weatherIntensity > 100)
                weatherIntensity = 100;
        }
    }






    void OnValidate()
    {
        SetTimeOfDay(false);
    }



    #region Getters / Setters

    public Light WorldLight
    {
        get { return worldLight; }
    }

    public float CurrentTime
    {
        get { return currentTime; }
        set { currentTime = Mathf.Clamp(value, 0f, 100f); }
    }

    public Vector3 Wind
    {
        get { return m_Wind.Wind; }
    }

    #endregion
}
