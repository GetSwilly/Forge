using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(SphereCollider))]
public class Shield : MonoBehaviour {

    [SerializeField]
    LayerMask ignoreMask;

    [SerializeField]
    [Range(0f,20f)]
    float shieldRange = 5f;

    [SerializeField]
    [Range(0f, 360f)]
    float shieldAngle = 0f;

    /*
    [SerializeField]
    [Range(-180f, 180f)]
    float shieldEndAngle = 0f;
    */

    SphereCollider myCollider = null;


    void OnEnable()
    {
        myCollider.enabled = true;
    }
    void OnDisable()
    {
        myCollider.enabled = false;
    }



    void OnTriggerEnter(Collider coll)
    {
        IProjectile pScript = coll.GetComponent<IProjectile>();

        if (pScript == null || Utilities.IsInLayerMask(pScript.Owner.gameObject, ignoreMask))
            return;

        float _angle = Vector3.Angle(coll.transform.position - transform.position, transform.forward);
        if (_angle > shieldAngle / 2f)
            return;


        GetComponent<Health>().HealthArithmetic(-pScript.Power, pScript.IsCritical, pScript.Owner);

        pScript.Disable(GetComponent<Transform>());
    }

    void OnValidate()
    {
        if (myCollider == null)
            myCollider = GetComponent<SphereCollider>();


        myCollider.isTrigger = true;



        Vector3 _scale = transform.lossyScale;
        float maxVal = _scale.x;
        maxVal = maxVal > _scale.y ? maxVal : _scale.y;
        maxVal = maxVal > _scale.z ? maxVal : _scale.z;

        myCollider.radius = shieldRange / maxVal;
       
    }
}
