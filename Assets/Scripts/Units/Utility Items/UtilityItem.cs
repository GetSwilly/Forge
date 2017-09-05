using UnityEngine;
using System.Collections.Generic;

public abstract class UtilityItem : MonoBehaviour {

    [SerializeField]
    [EnumFlags]
    InputType m_InputType = InputType.Hold;

    [SerializeField]
    bool shouldBeThrown = true;

    protected LayerMask friendlyMask;
    protected Transform owner;


    protected Transform myTransform;

    public virtual void Awake()
    {
        myTransform = GetComponent<Transform>();
    }


    public abstract void Activate(Transform owner, List<Stat> stats);




    public bool ShouldBeThrown
    {
        get { return shouldBeThrown; }
    }

    public InputType InputType
    {
        get { return m_InputType; }
    }
}
