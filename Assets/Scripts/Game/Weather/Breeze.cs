using UnityEngine;
using System.Collections;


[RequireComponent(typeof(SphereCollider))]
public class Breeze : MonoBehaviour {
 

    [SerializeField]
    AnimationCurve forceFalloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);


    Vector3 velocityVector;
    float breezeForce = 0f;


    Transform myTransform;
    SphereCollider myCollider;

    void Awake()
    {
        myTransform = GetComponent<Transform>();
        myCollider = GetComponent<SphereCollider>();
    }
    void OnDisable()
    {
        StopAllCoroutines();
    }


    public void Initialize(float _range, Vector3 _velocity, float _force, float _lifetime)
    {
        myCollider.radius = _range;
        velocityVector = _velocity;
        breezeForce = _force;

        StartCoroutine(MovementRoutine(_lifetime));
    }



    IEnumerator MovementRoutine(float breezeLifetime)
    {
        float timer = breezeLifetime;

        while (timer > 0)
        {
            yield return null;

            timer -= Time.deltaTime;

            myTransform.position += velocityVector * Time.deltaTime;
        }

        gameObject.SetActive(false);
    }



    void OnValidate()
    {
        Utilities.ValidateCurve_Times(forceFalloff, 0f, 1f);
    }



    void OnTriggerStay(Collider coll)
    {
        if (coll.isTrigger)
            return;

        Rigidbody _rigid = coll.GetComponent<Rigidbody>();

        if (_rigid == null)
            return;

        float fallOffAmount = forceFalloff.Evaluate(Mathf.Clamp01(Vector3.Distance(myTransform.position, coll.transform.position) / myCollider.radius));
        _rigid.AddForce(velocityVector.normalized * breezeForce * fallOffAmount * Time.deltaTime, ForceMode.Force);
    }
}
