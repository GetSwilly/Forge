using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public abstract class AutoActivate : MonoBehaviour {

	public enum ActivateType {NONE, TIME, ENTER, STAY, EXIT};
    [SerializeField]
    protected ActivateType activateProtocol = ActivateType.STAY;
    [SerializeField]
    AnimationCurve activateTimeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);


	public enum DeactivateType {NONE, TIME, ENTER, STAY, EXIT};
    
    [SerializeField]
    protected DeactivateType deactivateProtocol = DeactivateType.EXIT;
    [SerializeField]
    AnimationCurve deactivateTimeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    public virtual void Start()
    {
        if (activateProtocol == ActivateType.TIME)
            StartCoroutine(TimeActivation());
    }

	public abstract void Activate(GameObject activatingObject);
	public abstract void Deactivate(GameObject deactivatingObject);



    protected IEnumerator TimeActivation()
    {
        while (activateProtocol == ActivateType.TIME)
        {
            yield return new WaitForSeconds(activateTimeCurve.Evaluate(Random.Range(0f, 1f)));
            Activate(this.gameObject);
            yield return new WaitForSeconds(deactivateTimeCurve.Evaluate(Random.Range(0f, 1f)));
            Deactivate(this.gameObject);
        }
    }



	void OnTriggerEnter(Collider coll){

		if(!coll.isTrigger){
			if(activateProtocol == ActivateType.ENTER){
				Activate(coll.gameObject);
			}

			if(deactivateProtocol == DeactivateType.ENTER){
				Deactivate(coll.gameObject);
			}
		}
	}

	void OnTriggerStay(Collider coll){
		if(!coll.isTrigger){
			if(activateProtocol == ActivateType.STAY){
				Activate(coll.gameObject);
			}
			
			if(deactivateProtocol == DeactivateType.STAY){
				Deactivate(coll.gameObject);
			}
		}
	}

	void OnTriggerExit(Collider coll){
		if(!coll.isTrigger){
			if(activateProtocol == ActivateType.EXIT){
				Activate(coll.gameObject);
			}
			
			if(deactivateProtocol == DeactivateType.EXIT){
				Deactivate(coll.gameObject);
			}
		}
	}
}
