using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class ObjectPoolerManager : MonoBehaviour {
	
	[HideInInspector]
	public ObjectPooler DeathAnimationPooler;
	[HideInInspector]
	public ObjectPooler DynamicInfoPooler;
    [HideInInspector]
    public ObjectPooler Unit_UIPooler;
    [HideInInspector]
    public ObjectPooler GenericUIPooler;
    [HideInInspector]
    public ObjectPooler InteractableUIPooler;
    [HideInInspector]
    public ObjectPooler AudioRemnantPooler;

    [SerializeField]
    GameObject DeathAnimationPrefab;
    [SerializeField]
    GameObject DynamicInfoPrefab;
    [SerializeField]
    GameObject Unit_UIPrefab;
    [SerializeField]
    GameObject GenericUIPrefab;
    [SerializeField]
    GameObject InteractableUIPrefab;
    [SerializeField]
    GameObject AudioRemnantPrefab;


    [HideInInspector]
	public static ObjectPoolerManager Instance {get;private set;}
	void Awake()
	{
		Instance = this;

		GameObject pools = new GameObject ("Pools");
	
		
		if (DeathAnimationPooler == null && DeathAnimationPrefab != null)
		{
			GameObject go = new GameObject("DeathAnimationPooler");
			DeathAnimationPooler = go.AddComponent<ObjectPooler>();
			DeathAnimationPooler.PooledObject = DeathAnimationPrefab;
            DeathAnimationPooler.PoolLength = 3;

            GameObject anims = new GameObject("Death Animations");
			anims.transform.parent = pools.transform;

			DeathAnimationPooler.Parent = anims.transform;
			go.transform.parent = this.gameObject.transform;
			DeathAnimationPooler.Initialize();
		}


		if (DynamicInfoPooler == null && DynamicInfoPrefab != null)
		{
			GameObject go = new GameObject("DynamicInfoPooler");
			DynamicInfoPooler = go.AddComponent<ObjectPooler>();
			DynamicInfoPooler.PooledObject = DynamicInfoPrefab;
			
			GameObject infos = new GameObject("Dynamic Infos");
			infos.transform.parent = pools.transform;
			
			DynamicInfoPooler.Parent = infos.transform;
			go.transform.parent = this.gameObject.transform;
			DynamicInfoPooler.Initialize();
		}

        if (Unit_UIPooler == null && Unit_UIPrefab != null)
        {
            GameObject go = new GameObject("Unit_UIPooler");
            Unit_UIPooler = go.AddComponent<ObjectPooler>();
            Unit_UIPooler.PooledObject = Unit_UIPrefab;
            Unit_UIPooler.PoolLength = 3;

            GameObject uis = new GameObject("Unit UIs");
            uis.transform.parent = pools.transform;

            Unit_UIPooler.Parent = uis.transform;
            go.transform.parent = this.gameObject.transform;
            Unit_UIPooler.Initialize();
        }

        if (GenericUIPooler == null && GenericUIPrefab != null)
        {
            GameObject go = new GameObject("GenericUIPooler");
            GenericUIPooler = go.AddComponent<ObjectPooler>();
            GenericUIPooler.PooledObject = GenericUIPrefab;
            GenericUIPooler.PoolLength = 3;


            GameObject uis = new GameObject("Generic UIs");
            uis.transform.parent = pools.transform;

            GenericUIPooler.Parent = uis.transform;
            go.transform.parent = this.gameObject.transform;
            GenericUIPooler.Initialize();
        }

        if (InteractableUIPooler == null && InteractableUIPrefab != null)
        {
            GameObject go = new GameObject("InteractableUIPooler");
            InteractableUIPooler = go.AddComponent<ObjectPooler>();
            InteractableUIPooler.PooledObject = InteractableUIPrefab;
            InteractableUIPooler.PoolLength = 3;


            GameObject uis = new GameObject("Interactable UIs");
            uis.transform.parent = pools.transform;

            InteractableUIPooler.Parent = uis.transform;
            go.transform.parent = this.gameObject.transform;
            InteractableUIPooler.Initialize();
        }

        if (AudioRemnantPooler == null && AudioRemnantPrefab != null)
        {
            GameObject go = new GameObject("AudioRemnantPooler");
            AudioRemnantPooler = go.AddComponent<ObjectPooler>();
            AudioRemnantPooler.PooledObject = AudioRemnantPrefab;
            AudioRemnantPooler.PoolLength = 3;


            GameObject audioRemnants = new GameObject("Audio Remnants");
            audioRemnants.transform.parent = pools.transform;

            AudioRemnantPooler.Parent = audioRemnants.transform;
            go.transform.parent = this.gameObject.transform;
            AudioRemnantPooler.Initialize();
        }
    }
}
