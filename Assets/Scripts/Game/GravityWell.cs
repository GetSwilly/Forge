using UnityEngine;
using System.Collections;

public class GravityWell : MonoBehaviour {

    [SerializeField]
    float gravityStrength;

    [SerializeField]
    float gravityRange;


    [SerializeField]
    [Range(0f, 1f)]
    float perpendicularRatio = 0f;

    [SerializeField]
    bool isClockwise = true;

    [SerializeField]
    AnimationCurve strengthFallOff = AnimationCurve.Linear(0f, 1f, 1f, 0f);


    Transform myTransform;


    void Awake()
    {
        myTransform = GetComponent<Transform>();
    }
	void Start () {
        SphereCollider sightCollider = gameObject.AddComponent<SphereCollider>();
        sightCollider.isTrigger = true;


        Vector3 _scale = myTransform.lossyScale;
        float maxVal = _scale.x;
        maxVal = maxVal > _scale.y ? maxVal : _scale.y;
        maxVal = maxVal > _scale.z ? maxVal : _scale.z;

        sightCollider.radius = gravityRange / maxVal;
    }
	
	

    void OnTriggerStay(Collider coll)
    {
        Rigidbody _rigid = coll.GetComponent<Rigidbody>();

        if (_rigid == null || _rigid.isKinematic)
            return;


        Vector3 directVector = myTransform.position - coll.transform.position;


        float _angle = 90f;
        _angle *= isClockwise ? 1f : -1f;

        Vector3 perpendicularVector = Quaternion.AngleAxis(_angle, myTransform.up) * directVector;  //new Vector3(directVector.z, directVector.y, directVector.x);


        Vector3 forceVector = (directVector.normalized * (1f - perpendicularRatio)) + (perpendicularVector.normalized * perpendicularRatio);
        
        float _strength = gravityStrength * strengthFallOff.Evaluate(Vector3.Distance(myTransform.position, coll.transform.position) / gravityRange);
        _rigid.AddForce(forceVector.normalized * _strength, ForceMode.Force);

        Debug.DrawLine(coll.transform.position, (coll.transform.position + forceVector.normalized * _strength), Color.cyan);
    }
}
