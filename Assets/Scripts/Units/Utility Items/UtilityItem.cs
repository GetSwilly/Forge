using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Team))]
public abstract class UtilityItem : MonoBehaviour {

    [SerializeField]
    [EnumFlags]
    InputType m_InputType = InputType.Hold;

    [SerializeField]
    bool shouldBeThrown = true;

    [SerializeField]
    protected bool showDebug = false;

    protected Transform m_Owner;
    
    protected Transform m_Transform;
    protected Team m_Team;

    protected virtual void Awake()
    {
        m_Transform = GetComponent<Transform>();
        m_Team = GetComponent<Team>();
    }


    public void Activate(Transform owner)
    {
        Activate(owner, Vector3.zero);
    }
    public virtual void Activate(Transform owner, Vector3 launchVector)
    {
        m_Owner = owner;

        Team team = m_Owner.GetComponent<Team>();
        m_Team.Copy(team);
        // m_Rigid.AddForce(launchVector, ForceMode.Impulse);
    }




    public bool ShouldBeThrown
    {
        get { return shouldBeThrown; }
    }

    public InputType InputType
    {
        get { return m_InputType; }
    }
}
