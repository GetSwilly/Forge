using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(FollowTarget))]
public class UIBase : MonoBehaviour {

    [SerializeField]
    Text m_Text;

    Transform target;
    FollowTarget m_FollowTarget;

    protected virtual void Awake()
    {
        m_FollowTarget = GetComponent<FollowTarget>();
    }
    public virtual void Inflate(Transform newTarget)
    {
        Inflate(newTarget, "");
    }
    public virtual void Inflate(Transform newTarget, string text)
    {
        SetTarget(newTarget);
        SetText(text);
    }
    public virtual void Deflate()
    {
        gameObject.SetActive(false);
    }

    public void SetText(string text)
    {
       if(m_Text != null)
        {
            m_Text.text = text;
        }
    }


    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        m_FollowTarget.SetTarget(Target);
    }

    public Transform Target
    {
        get { return target; }
    }
}
