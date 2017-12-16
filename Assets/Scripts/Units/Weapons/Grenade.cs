using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Team))]
public class Grenade : UtilityItem
{
    [SerializeField]
    LayerMask collisionMask;

    [SerializeField]
    Vector3 rootPosition;

    [SerializeField]
    float fuseTime;

    [SerializeField]
    float radius;

    [SerializeField]
    float power;

    [SerializeField]
    [Range(0f, 1f)]
    float criticalChance;

    [SerializeField]
    float criticalMultiplier;

    [SerializeField]
    bool ignoreObstacles = false;

    [SerializeField]
    AnimationCurve damageFallOff = AnimationCurve.Linear(0f, 1f, 1f, 1f);

    [SerializeField]
    GameObject effect;


    public override void Activate(Transform _owner, Vector3 launchVector)
    {
        StartCoroutine(FuseDelay());
    }


    IEnumerator FuseDelay()
    {
        yield return new WaitForSeconds(fuseTime);

        Explode();
    }

    void Explode()
    {
        StopAllCoroutines();
        

        //Get all colliders within range
        Collider[] hitColls = Physics.OverlapSphere(m_Transform.TransformPoint(rootPosition), radius, collisionMask);
        
        for (int i = 0; i < hitColls.Length; i++)
        {
            if (hitColls[i].transform == this)
                continue;

            Health hitHealth = hitColls[i].GetComponent<Health>();
            Team hitTeam = hitColls[i].GetComponent<Team>();

            //Ignore triggers, objects without health, and friendly objects
            if (hitColls[i].isTrigger || hitHealth == null || (hitTeam != null && m_Team.IsFriendly(hitTeam)))
            {
                if (showDebug)
                {
                    Debug.DrawLine(m_Transform.TransformPoint(rootPosition), hitColls[i].transform.position, Color.white, 5f);
                }

                continue;
            }

            //If ignoring blocking obstacles
            if (!ignoreObstacles)
            {

                //Check if an object is between the grenade and intended object

                Vector3 toVector = hitColls[i].transform.position - m_Transform.TransformPoint(rootPosition);

                RaycastHit[] rayHits = Physics.RaycastAll(m_Transform.TransformPoint(rootPosition), toVector, toVector.magnitude, collisionMask);
                bool validHit = true;

                for (int j = 0; j < rayHits.Length; j++)
                {
                    if (rayHits[j].collider != hitColls[i])
                    {
                        validHit = false;
                        break;
                    }
                }

                if (!validHit)
                {
                    if (showDebug)
                    {
                        Debug.Log("DIDNT HIT: " + hitColls[i].name);
                        Debug.DrawLine(m_Transform.TransformPoint(rootPosition), hitColls[i].transform.position, Color.white, 5f);
                    }

                    continue;
                }
            }

            //Check if critical hit
            bool isCrit = Random.value <= criticalChance;
            float dmg = power;

            if (isCrit)
                dmg *= criticalMultiplier;

            //Check distance from grenade and apply damage falloff
            float distPercent = Mathf.Clamp01(Vector3.Distance(hitColls[i].transform.position, m_Transform.TransformPoint(rootPosition)) / radius);

            if (distPercent > 1f || distPercent < 0)
                continue;

            dmg *= damageFallOff.Evaluate(distPercent);

            hitHealth.HealthArithmetic(-(int)dmg, isCrit, m_Owner);

            if (showDebug)
            {
                Debug.DrawLine(m_Transform.TransformPoint(rootPosition), hitColls[i].transform.position, Color.red, 5f);
            }
        }

        if (effect != null)
        {
            GameObject obj = Instantiate(effect) as GameObject;
            obj.transform.position = m_Transform.TransformPoint(rootPosition);
            obj.SetActive(true);
        }

        Deactivate();
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }

    #region Accessors

    public float FuseTime
    {
        get { return fuseTime; }
        private set { fuseTime = Mathf.Clamp(value, 0f, value); }
    }
    public float Radius
    {
        get { return radius; }
        private set { radius = Mathf.Clamp(value, 0f, value); }
    }
    public float Power
    {
        get { return power; }
        private set { power = Mathf.Clamp(value, 0f, value); }
    }
    public float CriticalMultiplier
    {
        get { return criticalMultiplier; }
        private set { criticalMultiplier = Mathf.Clamp(value, 0f, value); }
    }

    #endregion

    void OnValidate()
    {
        FuseTime = FuseTime;
        Radius = Radius;
        Power = Power;
        CriticalMultiplier = CriticalMultiplier;

        Utilities.ValidateCurve_Times(damageFallOff, 0f, 1f);
    }

    void OnDrawGizmos()
    {
        if (!showDebug)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(m_Transform.TransformPoint(rootPosition), Radius);
    }
}
