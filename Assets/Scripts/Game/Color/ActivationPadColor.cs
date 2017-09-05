using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivationPadColor : MonoBehaviour {


    [System.Serializable]
    struct ParticleSystemColoring
    {
        public Color m_MainColor;
        
        public Color m_StartColor;
        
        public Gradient m_ColorOverLifetime;

        public ParticleSystem m_ParticleSystem;
    }


    [SerializeField]
    List<ParticleSystemColoring> m_Colorings = new List<ParticleSystemColoring>();

    [SerializeField]
    Light activationLight;

    [SerializeField]
    Color lightColor = Color.white;

    [SerializeField]
    AnimationCurve lightIntensity = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [SerializeField]
    float lightTime = 0.1f;
    float currentTime;

    bool isLightIncreasing = false;


    [SerializeField]
    List<string> activatingTags = new List<string>();


    public void OnEnable()
    {
        for (int i = 0; i < m_Colorings.Count; i++)
        {
            ParticleSystem _system = m_Colorings[i].m_ParticleSystem;

            if (_system == null)
                continue;

            ParticleSystem.MainModule _mainModule = _system.main;
            _mainModule.startColor = m_Colorings[i].m_StartColor;

            ParticleSystem.ColorOverLifetimeModule _colorModule = _system.colorOverLifetime;
            _colorModule.color = new ParticleSystem.MinMaxGradient(m_Colorings[i].m_ColorOverLifetime);
        }

        currentTime = 0f;
        if(activationLight != null)
        {
            activationLight.color = lightColor;
            activationLight.intensity = lightIntensity.Evaluate(0f);
        }
    }
    



    void Update()
    {
        CurrentTime += Time.deltaTime * (isLightIncreasing ? 1 : -1);
    }



    void OnTriggerEnter(Collider coll)
    {
        if (coll.isTrigger || !activatingTags.Contains(coll.gameObject.tag))
            return;

        isLightIncreasing = true;
    }
    void OnTriggerExit(Collider coll)
    {
        if (coll.isTrigger || !activatingTags.Contains(coll.gameObject.tag))
            return;

        isLightIncreasing = false;
    }



    public float CurrentTime
    {
        get { return currentTime; }
        set
        {
            currentTime = Mathf.Clamp(value, 0f, lightTime);

            if (activationLight != null && currentTime > 0f)
            {
                activationLight.intensity = lightIntensity.Evaluate(currentTime / lightTime);
            }
        }
    }

    void OnValidate()
    {
        lightTime = Mathf.Max(0.01f, lightTime);
    }
}
