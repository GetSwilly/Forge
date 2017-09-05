using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class DynamicLight : MonoBehaviour {

    static readonly float YIELD_TIME = 0.2f;

    [SerializeField]
    AnimationCurve m_TimeOfDayModifier = AnimationCurve.Linear(0f, 1f, 1f, 1f);

    Light m_Light;


    void Awake()
    {
        m_Light = GetComponent<Light>();
    }
    void Start()
    {
        StartCoroutine(UpdateLightRoutine());
    }


    IEnumerator UpdateLightRoutine()
    {
        while (true)
        {
            yield return null;

            m_Light.intensity = WeatherSystem.Instance == null ? 1f : m_TimeOfDayModifier.Evaluate(WeatherSystem.Instance.CurrentTime);
        }
    }
}
