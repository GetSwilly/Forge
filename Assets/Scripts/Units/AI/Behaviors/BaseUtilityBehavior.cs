using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[RequireComponent(typeof(UtilityActor))]
public abstract class BaseUtilityBehavior : MonoBehaviour
{
    private static readonly float MINIMUM_DELAY = 0.25f;


    public delegate void AlertBehaviorEnd(BaseUtilityBehavior behavior);
    public AlertBehaviorEnd OnBehaviorEnd;

    [Tooltip("Can behavior be utilized?")]
    [SerializeField]
    private bool isUsable = true;
    private bool isActive = false;

    [Tooltip("Does behavior require a super-behavior to activate it?")]
    [SerializeField]
    protected bool needsSuperBehavior = false;

    [Tooltip("Super-behaviors capable of activating this behavior.")]
    [SerializeField]
    List<BaseUtilityBehavior> m_RequiredSuperBehaviors = new List<BaseUtilityBehavior>();

    [Tooltip("Defines utility value of behavior depending on behavior-specific circumstance.")]
    [SerializeField]
    protected AnimationCurve utilityCurve = AnimationCurve.Linear(0f, 0f, 1f, 100f);

    protected BaseUtilityBehavior superBehavior;
    protected BaseUtilityBehavior subBehavior;

    [Tooltip("Possible sub-behaviors this behavior is capable of activating.")]
    [SerializeField]
    List<BaseUtilityBehavior> viableSubBehaviors = new List<BaseUtilityBehavior>();

    [Tooltip("Necessary amount of time to wait between stopping and starting behavior.")]
    [SerializeField]
    float minimumRechargeDelay = 0f;

    protected UtilityActor m_Actor;
    protected UtilityMind m_Mind;
    protected Transform m_Transform;


    public virtual void Awake()
    {
        m_Transform = GetComponent<Transform>();
        m_Actor = GetComponent<UtilityActor>();
        m_Mind = GetComponent<UtilityMind>();
    }
    


    public abstract void StartBehavior();
    public virtual void StartBehavior(BaseUtilityBehavior _newSuperBehavior)
    {
        superBehavior = _newSuperBehavior;
        StartBehavior();
    }
    public virtual void StartSubBehavior(BaseUtilityBehavior _newSubBehavior)
    {
        if (_newSubBehavior == subBehavior)
            return;


        if (subBehavior != null)
        {
            subBehavior.EndBehavior(false, false);
        }



        subBehavior = _newSubBehavior;
        subBehavior.StartBehavior(this);
    }
    public virtual bool TryStartSubBehavior(BaseUtilityBehavior _newSubBehavior)
    {
        /*
        if (!CanStartSubBehavior())
            return false;
            */

        if (subBehavior != null && !subBehavior.IsActive)
            subBehavior = null;


        int _index = viableSubBehaviors.IndexOf(_newSubBehavior);

        bool isValidSubBehavior = _index != -1 && viableSubBehaviors[_index].CanStartBehavior; //viableSubBehaviors.Contains(_newSubBehavior);


        if(subBehavior == null)
        {
            if (isValidSubBehavior)
            {
                StartSubBehavior(_newSubBehavior);
                return true;
            }
           
        }
        else if(subBehavior.TryStartSubBehavior(_newSubBehavior))
        {
            return true;
        }
        else if(subBehavior.CanEndBehavior)
        {

            if (isValidSubBehavior)
            {
                StartSubBehavior(_newSubBehavior);
                return true;
            }
        }


        return false;
    }
    public virtual void EndBehavior(bool shouldNotifySuper, bool shouldNotifyActor)
    {
        if (m_Actor.ShowDebug)
        {
            Debug.Log(m_Transform.name + " -- EndBehavior(). " + shouldNotifySuper + ". " + shouldNotifyActor + ". " + this.ToString());
        }


        IsActive = false;

        if (subBehavior != null)
        {
            subBehavior.EndBehavior(false, false);
        }

        if (shouldNotifySuper && superBehavior != null)
        {
            BaseUtilityBehavior _behavior = superBehavior;
            superBehavior = null;
            _behavior.NotifySubBehaviorEnded(this);

            if (OnBehaviorEnd != null)
            {
                OnBehaviorEnd(this);
            }
        }

        StartCoroutine(RechargeBehavior());

        if (shouldNotifyActor)
        {
            m_Actor.NotifyBehaviorEnded(this);
        }
    }

    

    public abstract void NotifySubBehaviorEnded(BaseUtilityBehavior _behavior);

    public abstract float GetBehaviorScore();

    
    public void Enable()
    {
        IsUsable = true;
    }
    public void Disable()
    {
        IsUsable = false;
    }


    private IEnumerator RechargeBehavior()
    {
        yield return StartCoroutine(RechargeBehavior(MinimumRechargeDelay));
    }
    private IEnumerator RechargeBehavior(float time)
    {
        bool _usableStatus = IsUsable;

        IsUsable = false;

        yield return new WaitForSeconds(time);

        IsUsable = _usableStatus;
    }


    #region Accessors

    protected float MinimumRechargeDelay
    {
        get { return minimumRechargeDelay; }
        set
        {
            minimumRechargeDelay = Mathf.Clamp(value, MINIMUM_DELAY, minimumRechargeDelay);
        }
    }
    public virtual bool CanStartBehavior
    {
        get { return isUsable && !isActive; }
    }
    public abstract bool CanStartSubBehavior { get; }
    public virtual bool CanEndBehavior
    {
        get { return isUsable && isActive; }
    }



    public bool IsActive
    {
        get { return isActive; }
        protected set { isActive = value; }
    }

    public bool IsUsable
    {
        get { return isUsable; }
        set { isUsable = value; }
    }

    public List<BaseUtilityBehavior> ViableSubBehaviors
    {
        get{ return viableSubBehaviors; }
    }
    public bool NeedsSuperBehavior
    {
        get { return needsSuperBehavior; }
    }

    public BaseUtilityBehavior SuperBehavior
    {
        get { return superBehavior; }
    }
    public BaseUtilityBehavior SubBehavior
    {
        get { return subBehavior; }
    }

    #endregion


   protected virtual void OnValidate()
    {
        MinimumRechargeDelay = MinimumRechargeDelay;
    }


    public override abstract string ToString();
}
