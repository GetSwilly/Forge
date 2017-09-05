using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ProjectileScript : MonoBehaviour {

  

	LayerMask friendlyMask;
	
	Transform owner;
	Vector3 direction;
	float maxRange = 50f;
	float maxTime = 7f;
	int power;
	bool isCritical;
	
	public delegate void AlertImpact(Health _casualtyHealth);
	public AlertImpact OnImpact;

	[SerializeField] AudioClip impactSound;
	
	Vector3 previousPosition;
    float totalDist;

	float timer;
	
	Transform myTransform;
	Rigidbody myRigidbody;
	TrailRenderer myTrailRenderer;
	
	void Awake () {
		myTransform = GetComponent<Transform>();
		myRigidbody = GetComponent<Rigidbody>();
		myTrailRenderer = GetComponent<TrailRenderer>();
	}
	
	public void Initialize(Transform newOwner, LayerMask newFriendly, Vector3 dir, int newPower, bool _critical, float newSpeed, float newRange)
    {
		
		if(myTransform == null)
			myTransform = GetComponent<Transform>();
		
		if(myRigidbody == null)
			myRigidbody = GetComponent<Rigidbody>();
		
		owner = newOwner;
		
		if(owner != null){
			SpriteRenderer myRenderer = GetComponent<SpriteRenderer>();
			SpriteRenderer ownerRenderer = owner.GetComponent<SpriteRenderer>();
			
			if(myRenderer != null && ownerRenderer != null)
            {
				myRenderer.color = ownerRenderer.color;
			}
		}
		
		
		previousPosition = myTransform.position;
        totalDist = 0;
		
		friendlyMask = newFriendly;
		
		power = newPower;
		
		isCritical = _critical;
		
		myRigidbody.velocity = dir.normalized * newSpeed;
		
		maxRange = newRange;
        //maxTime = newTime;
		
		Validate();
		
		timer = 0;

		StartCoroutine(ResetTrail());

		myTrailRenderer.enabled = true;

	}
	IEnumerator ResetTrail()
    {

		float trailTime = myTrailRenderer.time;
		myTrailRenderer.time = 0;

		yield return new WaitForSeconds(0f);

		myTrailRenderer.time = trailTime;   
	}


	void Update ()
    {
		timer += Time.deltaTime;

        totalDist += Vector3.Distance(previousPosition, myTransform.position);
        previousPosition = myTransform.position;
		
		if(timer > maxTime || totalDist > maxRange){
			myTrailRenderer.enabled = false;
			gameObject.SetActive(false);
		}
	}
	void Validate()
    {
		maxTime = Mathf.Abs(maxTime);
		//maxRange = Mathf.Abs(maxRange);
	}

	public LayerMask FriendlyMask
    {
		get { return friendlyMask; }
	}
	
    /*
	void OnTriggerEnter(Collider other){
		if(other.isTrigger){
			AI otherAI = other.GetComponent<AI>();
			
			if(otherAI != null){
				otherAI.UnitAntagonized(owner);
			}
		}
	}
	*/
	
	void OnCollisionEnter(Collision coll)
    {
		
		if(!Utilities.IsInLayerMask(coll.gameObject, friendlyMask)){
			Health otherHealth = coll.gameObject.GetComponent<Health>();
			
			if (otherHealth != null)
            {
				otherHealth.HealthArithmetic(-power, isCritical, owner, myRigidbody.velocity.normalized);

				AudioSource _audio = GetComponent<AudioSource>();

				if(_audio != null && impactSound != null)
                {
					_audio.PlayOneShot(impactSound);
				}
				
				if(OnImpact != null)
					OnImpact(otherHealth);
			}
			
		}

		gameObject.SetActive(false);
	}

	void OnDisable()
    {
		OnImpact = null;
	}
}
