using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cloud : ParticleSystemEmitter {


    static readonly float DEFAULT_LIFETIME = 15f;
    static readonly float FADE_IN_TIME = 1f;
    static readonly float FADE_OUT_TIME = 2f;


    static readonly float MAX_STRIKE_ANGLE = 35f;


    //Target mask
    [SerializeField]
    LayerMask hitMask;

    //Time range between potential lightning strikes based on WeatherIntensity
    [SerializeField]
    AnimationCurve lightningStrikeDelay = AnimationCurve.Linear(0f, 0f, 100f, 1f);

    //Chance of lightning actually striking based on WeatherIntensity
    [SerializeField]
    AnimationCurve lightningStrikeChance = AnimationCurve.Linear(0f, 0f, 100f, 1f);

    //Amount of damage received at strike point based on WeatherIntensity
    [SerializeField]
    AnimationCurve lightningPower = AnimationCurve.Linear(0f,0f,100f,0f);

    //Spread distance of damage
    [SerializeField]
    float lightningHitRadius = 1f;

    //Falloff for damage over radius
    [SerializeField]
    AnimationCurve lightningIntensityFallOff = AnimationCurve.Linear(0f, 1f, 1f, 1f);


    
    //Chance to spread Effect_Lightning based on WeatherIntensity
    [SerializeField]
    AnimationCurve lightningEffectChance = AnimationCurve.Linear(0f,0f,100f,1f);


    //Amount to add to Effect_Lightning script based on WeatherIntensity
    [SerializeField]
    float lightningEffectDelta = 0f;


    [SerializeField]
    bool showDebug = false;
   
    
    MeshCollider m_Collider;
    Transform m_Transform;

    void Awake()
    {
        m_Transform = GetComponent<Transform>();
        m_Collider = GetComponentInChildren<MeshCollider>();

        if (m_Collider == null)
            this.enabled = false;
    }
    void OnDisable()
    {
        StopAllCoroutines();
    }




    public void Initialize()
    {
        Initialize(DEFAULT_LIFETIME, FADE_IN_TIME, FADE_OUT_TIME);
    }
    public void Initialize(float _lifetime, float fadeInTime, float fadeOutTime)
    {
        StopAllCoroutines();
        StartCoroutine(CloudRoutine(_lifetime, fadeInTime, fadeOutTime));
    }

    IEnumerator CloudRoutine(float _lifetime, float fadeInTime, float fadeOutTime)
    {
        float timer = 0f;
        float cutOffTime = _lifetime - fadeOutTime;

        if (cutOffTime < 0f)
            cutOffTime = 0f;
        


        StartCoroutine(CloudMoveRoutine());
        StartCoroutine(DelayedLightning());

        yield return new WaitForSeconds(_lifetime);
        /*
        Coroutine fadeRoutine;

        
        //Immediately fade out
        yield return StartCoroutine(CloudFadeRoutine(0f, 0.01f));

        //Fade In
        fadeRoutine = StartCoroutine(CloudFadeRoutine(1f, fadeInTime));
        
        
        while (timer < cutOffTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        
 

        //Fade Out
        fadeRoutine = StartCoroutine(CloudFadeRoutine(0f, fadeOutTime));
        
        while ( fadeRoutine != null)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
            */
        StopCoroutine(CloudMoveRoutine());
        StopCoroutine(DelayedLightning());
        gameObject.SetActive(false);
    }
    IEnumerator CloudMoveRoutine()
    {
        while (true)
        {
            Vector3 moveVector = LevelController.Instance == null ? Vector3.zero : LevelController.Instance.Wind;


            m_Transform.position = m_Transform.position + (moveVector * Time.deltaTime);

            yield return null;
        }
   
    }
    IEnumerator CloudFadeRoutine(float desiredAlpha, float fadeTime)
    {
        Renderer[] _renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < _renderers.Length; i++)
        {
            Color _color = _renderers[i].material.color;
            _color.a = desiredAlpha;
            _renderers[i].material.color = _color;
        }

        yield return null;

        //Renderer[] _renderers = GetComponentsInChildren<Renderer>();
        //AutoFade[] _autos = new AutoFade[_renderers.Length];

        //for (int i = 0; i < _renderers.Length; i++)
        //{
        //    Color _color = _renderers[i].material.color;



        //    AutoFade tempAuto = _renderers[i].GetComponent<AutoFade>();

        //    if (tempAuto == null)
        //        tempAuto = _renderers[i].gameObject.AddComponent<AutoFade>();

        //    tempAuto.StopAllCoroutines();


        //    _color.a = desiredAlpha;
        //    tempAuto.Fade(AutoColor.ColorComponent.Material, _color, fadeTime, false);

        //    _autos[i] = tempAuto;
        //}


        //bool isDone = false;

        //while (!isDone)
        //{
        //    yield return null;


        //    isDone = true;

        //    for (int i = 0; i < _autos.Length; i++)
        //    {
        //        if (!_autos[i].IsComplete)
        //            isDone = false;
        //    }
        //}
    }



    IEnumerator DelayedLightning()
    {
        while (true)
        {

            yield return new WaitForSeconds(Random.Range(0f,lightningStrikeDelay.Evaluate(WeatherSystem.Instance.WeatherIntensity)));

            if (Random.value <= lightningStrikeChance.Evaluate(WeatherSystem.Instance.WeatherIntensity))
                StartCoroutine(Strike());

        }
    }

    IEnumerator Strike()
    {
        bool isValidPoint = false;

        do
        {
            yield return null;

            Bounds _bounds = Utilities.CalculateObjectBounds(gameObject);

            
            //Originate strike from somewhere within the cloud
            Vector3 strikeOrigin = _bounds.center;
            strikeOrigin += m_Transform.right * Random.Range(-_bounds.extents.x, _bounds.extents.x);
            strikeOrigin += m_Transform.forward * Random.Range(-_bounds.extents.z, _bounds.extents.z);
            //strikeOrigin.y += _bounds.extents.y;


            //Set origin point at the edge of the cloud's mesh
            /*
            RaycastHit[] hits = Physics.RaycastAll(new Ray(strikeOrigin, Vector3.up), _extents.y);
            isValidPoint = false;

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider == myCollider)
                {
                    strikeOrigin = hits[i].point;
                    isValidPoint = true;
                    break;
                }
            }
            */

          //  if (!isValidPoint)
             //   continue;



       

            //Only strike within a certain angle
            Vector3 strikeDir = Random.insideUnitSphere;

            if (strikeDir.y >= 0)
                strikeDir.y *= -1;



            isValidPoint = Vector3.Angle(Vector3.down, strikeDir) <= MAX_STRIKE_ANGLE;


            if (!isValidPoint)
                continue;



            strikeDir.Normalize();

            RaycastHit[] hits = Physics.RaycastAll(strikeOrigin, strikeDir, strikeOrigin.y * 2f, hitMask);


            
            if (hits.Length == 0)
                continue;



            RaycastHit closestHit = new RaycastHit();

            for (int i = 0; i < hits.Length; i++)
            {
                Debug.DrawLine(strikeOrigin, hits[i].point, Color.red, 3f);

                if (hits[i].transform == m_Transform || hits[i].collider.isTrigger)
                    continue;

                if (closestHit.collider == null || Vector3.Distance(hits[i].point, strikeOrigin) < Vector3.Distance(closestHit.point, strikeOrigin))
                    closestHit = hits[i];
            }


            // Debug.Log("Origin : " + strikeOrigin + ". Hit Point: " + closestHit.point);
            if (showDebug)
            {
                Debug.DrawLine(strikeOrigin, closestHit.point, Color.yellow, 5f);
            }

            Collider[] colls = Physics.OverlapSphere(closestHit.point, lightningHitRadius, hitMask);
            RaycastHit strikeHit;

            for(int i = 0; i < colls.Length; i++)
            {
                if (colls[i].isTrigger)
                    continue;

                if (Physics.Raycast(new Ray(closestHit.point, colls[i].transform.position - closestHit.point), out strikeHit, (colls[i].transform.position - closestHit.point).magnitude * 1.1f) && strikeHit.transform == colls[i].transform)
                {
                    if (showDebug)
                    {
                        Debug.DrawLine(m_Transform.position, strikeHit.point, Color.yellow, 5f);
                    }


                    float intensityFalloff = lightningIntensityFallOff.Evaluate(Mathf.Clamp01(Vector3.Distance(closestHit.point, colls[i].transform.position) / lightningHitRadius));

                    AttributeHandler _handler = colls[i].GetComponent<AttributeHandler>();

                    if (_handler == null)
                        _handler = colls[i].gameObject.AddComponent<AttributeHandler>();

                    float resistanceMultiplier = _handler.GetResistanceMultiplier(Attribute.Shock);

                    Health _health = colls[i].GetComponent<Health>();
                    if(_health != null)
                    {
                        _health.HealthArithmetic(-lightningPower.Evaluate(WeatherSystem.Instance.WeatherIntensity)  * intensityFalloff * resistanceMultiplier, false, m_Transform);
                    }


                   
                    if(Random.value <= lightningEffectChance.Evaluate(WeatherSystem.Instance.WeatherIntensity) * intensityFalloff * resistanceMultiplier)
                    {
                        _handler.ModifyActiveAttribute(Attribute.Shock, lightningEffectDelta * intensityFalloff, m_Transform);
                    }
                }
            }

           


            isValidPoint = true;

        } while (!isValidPoint);
    }
}
