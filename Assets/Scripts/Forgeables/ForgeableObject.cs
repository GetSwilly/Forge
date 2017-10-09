using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Team))]
public abstract class ForgeableObject : MonoBehaviour, IMemorable
{

    [SerializeField]
    protected bool showDebug = false;

    [SerializeField]
    string myName;

    [SerializeField]
    protected Team m_Team;

    [SerializeField]
    protected Transform m_Pivot;

    [SerializeField]
    float buildTime;


    protected Transform m_Transform;



    protected virtual void Awake()
    {
        m_Transform = GetComponent<Transform>();

        m_Team = GetComponent<Team>();
    }
    void OnEnable()
    {
        m_Pivot.localRotation = Quaternion.identity;
    }


    public virtual void Initialize(ForgeSite forgeActivator)
    {
        Initialize(forgeActivator, null);
    }

    public abstract void Initialize(ForgeSite activator, Team team);


    #region Accessors

    public string Name
    {
        get { return myName; }
    }
    public Transform Transform
    {
        get { return m_Transform; }
    }
    public GameObject GameObject
    {
        get { return this.gameObject; }
    }

    public float BuildTime
    {
        get { return buildTime; }
        private set { buildTime = Mathf.Clamp(value, 0f, value); }
    }

    #endregion

    protected virtual void OnValidate()
    {
        BuildTime = BuildTime;
    }
}
