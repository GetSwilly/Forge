using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnvironmentArea : MonoBehaviour {

	[SerializeField] NodeType environmentType;
	[SerializeField] Material meshMaterial;
	[SerializeField] PhysicMaterial meshPhysicMaterial;
	//[SerializeField] [Range(0f,2f)] float movementSpeedup = 1f;
	//[SerializeField] [Range(0f,2f)] float accelerationSpeedup;// = 1f;

	[SerializeField] int damage = 0;
	[SerializeField] float damageRate = 3f;


	Dictionary<Health, float> damageObjects = new Dictionary<Health, float>();

	void Awake(){
		MeshRenderer _renderer = GetComponent<MeshRenderer>();

		if(_renderer == null)
			_renderer = gameObject.AddComponent<MeshRenderer>();


		_renderer.material = meshMaterial;

		_renderer.sortingLayerName = "Default";
		_renderer.sortingOrder = 0;
    }


    void Start(){
        MeshCollider _collider = GetComponent<MeshCollider>();

		if(_collider != null){
			//_collider.isTrigger = true;
			_collider.material = meshPhysicMaterial;
		}
	}

	void OnCollisionStay(Collision coll){

        if (coll.gameObject.tag == "Ground")
            return;

		if(coll.collider.isTrigger)
            return;



		if(damage != 0){
			Health collHealth = coll.collider.GetComponent<Health>();
			if(collHealth != null){
				if(damageObjects.ContainsKey(collHealth)){
					float _timer = damageObjects[collHealth];
					_timer -= Time.deltaTime;

					if(_timer <= 0){
						collHealth.HealthArithmetic(-damage, false, transform);
						_timer = damageRate;
					}

					damageObjects[collHealth] = _timer;
				}else{
					//collHealth.HealthArithmetic(-damage, false, transform);
					damageObjects.Add(collHealth, damageRate);
				}
			}
		}
		
	}



	void OnCollisionExit(Collision coll){

        if (coll.gameObject.tag == "Ground")
            return;

		if(coll.collider.isTrigger)
            return;



		Health collHealth = coll.collider.GetComponent<Health>();
		if(collHealth != null && damageObjects.ContainsKey(collHealth)){
			damageObjects.Remove(collHealth);
		}
		
	}


	public NodeType EnvironmentType{
		get { return environmentType; }
	}

    
	public float MovementWeight {
		get {
            float val = meshPhysicMaterial == null ? 1f : (meshPhysicMaterial.dynamicFriction + meshPhysicMaterial.staticFriction) / 2f;

            if (val != 0)
                val = 1f / val;

            return val;
        }
	}
	
}
