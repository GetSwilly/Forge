using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Rune : MonoBehaviour
{
    bool isRuneActive = false;
    protected Transform m_Transform;
    protected UnitController m_Owner;

    void Awake()
    {
        m_Transform = GetComponent<Transform>();
    }



    public virtual void Initialize(UnitController _unit)
    {
        if (_unit == null || IsRuneActive)
            return;

        Owner = _unit;
        
        Collider[] colliders = GetComponents<Collider>();
        for(int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        Renderer[] renderers = GetComponents<Renderer>();
        for(int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = false;
        }

        m_Transform.SetParent(Owner.transform);
        m_Transform.localPosition = Vector3.zero;

        IsRuneActive = true;
    }
    public virtual void Terminate()
    {
        if (Owner == null || !IsRuneActive)
            return;


        UnitController _controller = Owner;
        Owner = null;

        Collider[] colliders = GetComponents<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        Renderer[] renderers = GetComponents<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = false;
        }
        
        IsRuneActive = false;
    }


    protected UnitController Owner
    {
        get { return m_Owner; }
        set { m_Owner = value; }
    }

    public bool IsRuneActive
    {
        get { return isRuneActive; }
        protected set { isRuneActive = value; }
    }
}
