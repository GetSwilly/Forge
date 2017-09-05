using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class WorldLight : MonoBehaviour
{


    protected bool isActive = false;


    protected Light m_Light;
    protected Collider m_Collider;



    void Awake()
    {
        m_Light = GetComponent<Light>();

        SphereCollider _coll = gameObject.AddComponent<SphereCollider>();
        _coll.isTrigger = true;
        _coll.radius = m_Light.range * (1 / transform.localScale.z);

        m_Collider = _coll;

        isActive = m_Light.enabled;
        m_Collider.enabled = m_Light.enabled;
    }


    public void SetActivationState(bool _state)
    {
        m_Light.enabled = _state;
        m_Collider.enabled = _state;

        isActive = _state;
    }


    public bool IsIlluminatingObject(GameObject otherObject)
    {
        return otherObject == null ? false : IsIlluminatingObject(otherObject.transform);
    }
    public bool IsIlluminatingObject(Transform otherTransform)
    {

        if (otherTransform == null)
            return false;

        if (Vector3.Distance(otherTransform.position, transform.position) > m_Light.range)
            return false;

        if (m_Light.type == LightType.Spot && Vector3.Angle((otherTransform.position - transform.position), transform.forward) > m_Light.spotAngle)
            return false;



        return true;
    }


    public bool IsActive
    {
        get { return isActive; }
    }

    public Light Light
    {
        get { return m_Light; }
    }
}