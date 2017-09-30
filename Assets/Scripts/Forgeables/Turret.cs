using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Gun))]
public class Turret : MonoBehaviour, IForgeable, IMemorable, ITeamMember
{
    [SerializeField]
    string turretName;

    [SerializeField]
    float sightRange = 10f;

    [SerializeField]
    Team m_Team;

    [SerializeField]
    float minimumFireAngle = 1f;

    [SerializeField]
    float rotationSmoothing = 1f;

    [SerializeField]
    Transform m_Pivot;

    [SerializeField]
    bool showDebug = false;


    List<GameObject> nearbyTargets = new List<GameObject>();

    GameObject target;
    Gun m_Gun;
    Transform m_Transform;

    void Awake()
    {
        m_Transform = GetComponent<Transform>();
        m_Gun = GetComponent<Gun>();

        SphereCollider sightCollider = m_Transform.gameObject.AddComponent<SphereCollider>();
        sightCollider.isTrigger = true;

        //Make Sight distance independent of scale
        Vector3 _scale = m_Transform.lossyScale;
        float maxVal = _scale.x;
        maxVal = maxVal > _scale.y ? maxVal : _scale.y;
        maxVal = maxVal > _scale.z ? maxVal : _scale.z;

        sightCollider.radius = SightRange / maxVal;
    }
    void OnEnable()
    {
        m_Pivot.localRotation = Quaternion.identity;
    }


    public void Initialize(ForgeSite forgeActivator)
    {
        Initialize(forgeActivator, null);
    }
    public void Initialize(ForgeSite forgeActivator, ITeamMember teamMember)
    {
        if (teamMember != null)
        {
            m_Team.SetCurrentTeamTag(teamMember.GetCurrentTeam().Team);
            m_Team.SetEnemyTeams(teamMember.GetEnemyTeams());
        }
    }



    void Update()
    {
        if (target == null)
        {
            SelectTarget();
        }
        else if (!target.gameObject.activeInHierarchy)
        {
            RemoveTarget(target);
            SelectTarget();
        }

        if (target == null)
            return;

        Vector3 targetVector = target.transform.position - m_Pivot.position;

        Quaternion targetRotation = Quaternion.LookRotation(targetVector);
        m_Pivot.rotation = Quaternion.Slerp(m_Pivot.rotation, targetRotation, RotationSmoothing * Time.deltaTime);


        if (Vector3.Angle(m_Pivot.forward, targetVector) <= MinimumFireAngle)
        {
            if (showDebug)
            {
                Debug.DrawRay(m_Pivot.position, m_Pivot.forward * 10f, Color.yellow);
            }


            Fire();
        }


        if (showDebug)
        {
            Debug.DrawLine(m_Pivot.position, target.transform.position);
        }
    }


    public void Fire()
    {
        m_Gun.ActivatePrimary();
    }

    public void SelectTarget()
    {
        if (target != null || nearbyTargets.Count == 0)
            return;

        target = nearbyTargets[0];
    }

    void AddTarget(GameObject _target)
    {
        if (_target == null || !_target.activeInHierarchy || nearbyTargets.Contains(_target))
            return;

        nearbyTargets.Add(_target);
    }
    void RemoveTarget(GameObject _target)
    {
        if (_target == null)
            return;

        nearbyTargets.Remove(_target);

        if (_target == target)
        {
            target = null;
        }
    }

    #region Team

    public Team GetTeam()
    {
        return m_Team;
    }
    public SingleTeamClassification GetCurrentTeam()
    {
        return m_Team.CurrentTeam;
    }
    public TeamClassification[] GetFriendlyTeams()
    {
        return m_Team.FriendlyTeams;
    }
    public TeamClassification[] GetEnemyTeams()
    {
        return m_Team.EnemyTeams;
    }
    #endregion

    #region Accessors

    public string Name
    {
        get { return turretName; }
        set { turretName = value; }
    }
    public GameObject GameObject
    {
        get { return this.gameObject; }
    }
    public Transform Transform
    {
        get { return m_Transform; }
    }
    public float SightRange
    {
        get { return sightRange; }
        private set { sightRange = Mathf.Clamp(value, 0f, value); }
    }
    public float MinimumFireAngle
    {
        get { return minimumFireAngle; }
        private set { minimumFireAngle = Mathf.Clamp(value, 0f, 180f); }
    }
    public float RotationSmoothing
    {
        get { return rotationSmoothing; }
        private set { rotationSmoothing = Mathf.Clamp(value, 0f, value); }
    }

    #endregion


    void OnTriggerStay(Collider coll)
    {
        if (coll.isTrigger)
            return;


        ITeamMember teamMember = coll.gameObject.GetComponent<ITeamMember>();

        if (teamMember != null && TeamUtility.IsEnemy(this, teamMember.GetCurrentTeam()))
        {
            AddTarget(coll.gameObject);
        }

    }
    void OnTriggerExit(Collider coll)
    {
        if (coll.isTrigger)
            return;

        ITeamMember teamMember = coll.gameObject.GetComponent<ITeamMember>();

        if (teamMember != null && TeamUtility.IsEnemy(this, teamMember.GetCurrentTeam()))
        {
            RemoveTarget(coll.gameObject);
        }

    }
    void OnValidate()
    {
        SightRange = SightRange;
        MinimumFireAngle = MinimumFireAngle;
        RotationSmoothing = RotationSmoothing;
    }
}
