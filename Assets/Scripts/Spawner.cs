using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour {

	public enum ActivationType { Continuous, Trigger}
	public ActivationType activateType;

	public enum SpawnType { Radius, Points }
	public SpawnType spawnType;

	public LayerMask activationMask;

	[SerializeField] float spawnRate = 1f;
	float spawnTimer = 0f;

	bool canSpawn = false;

	[SerializeField] Vector2 spawnRange = Vector2.zero;
	[SerializeField] List<Vector2> spawnPoints = new List<Vector2>();

	[SerializeField] List<WeightedObjectOfGameObject> spawnObjects = new List<WeightedObjectOfGameObject>();

	//ObjectPooler[] spawnPoolers;

	Transform myTransform;

	void Awake()
    {

		myTransform = GetComponent<Transform>();

		/*
		spawnPoolers = new ObjectPooler[spawnObjects.Length];
		for(int i = 0; i < spawnObjects.Length; i++){
			spawnPoolers[i] = gameObject.AddComponent<ObjectPooler>();
			spawnPoolers[i].PooledObject = spawnObjects[i];
			spawnPoolers[i].Parent = transform;
			spawnPoolers[i].Initialize();
		}*/
	}

	void Start()
    {
		spawnTimer = spawnRate;
	}

	void Update()
    {
		if(spawnTimer > 0)
			spawnTimer -= Time.deltaTime;

		if(activateType == ActivationType.Continuous && CanSpawnObject())
			StartCoroutine(SpawnObject());
	}

	public IEnumerator SpawnObject()
    {

		canSpawn = false;

		//ObjectPooler toSpawnPooler = (ObjectPooler)Utilities.WeightedSelection(spawnPoolers, spawnProbabilities);
		GameObject toSpawn = Utilities.WeightedSelection(spawnObjects.ToArray(), 0f);
		toSpawn  = (GameObject)GameObject.Instantiate(toSpawn);//, spawnPos, placementQuaternion);
		Vector3 spawnPos;
		Quaternion placementQuaternion;
		Vector3 bounds;
		bool spaceOccupied;


		do
        {
			yield return null;

			spawnPos = GetRandomPosition();
			placementQuaternion = Quaternion.AngleAxis(Random.Range(-180f,180f), Vector3.forward);
			bounds = Utilities.CalculateObjectBounds(toSpawn, false);
			
			Collider2D checkColl = Physics2D.OverlapCircle( spawnPos, Mathf.Max(bounds.x, bounds.y));
			spaceOccupied = checkColl == null ? false : true;
		}while(spaceOccupied);

		toSpawn.transform.position = spawnPos;
		toSpawn.transform.rotation = placementQuaternion;
		toSpawn.SetActive(true);

		canSpawn = true;
	}

	public Vector3 GetRandomPosition()
    {

		switch(spawnType){
		case SpawnType.Points:
			if(spawnPoints.Count == 0)
				break;

			return spawnPoints[Random.Range(0,spawnPoints.Count)];
		case SpawnType.Radius:
			Vector3 positionDelta = Random.insideUnitSphere;
			positionDelta.z = 0;
			positionDelta.Normalize();

			positionDelta *= Random.Range(spawnRange.x, spawnRange.y);
		
			return (transform.position + positionDelta);
		}

		return myTransform.position;
	}

	public bool CanSpawnObject()
    {
		return canSpawn && spawnTimer <= 0f;
	}

	void OnTriggerStay2D(Collider2D coll)
    {
		if(!coll.isTrigger && activateType == ActivationType.Trigger && CanSpawnObject() && Utilities.IsInLayerMask(coll.gameObject.layer, activationMask)){
			StartCoroutine(SpawnObject());
		}
	}

	void OnDrawGizmos()
    {
		Gizmos.color = Color.green;

		Gizmos.DrawWireSphere(transform.position, spawnRange.y);
	}  
}
