using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class ObjectPoolerManager : MonoBehaviour {
	
	[HideInInspector]
	public ObjectPooler DynamicInfoPooler;
    [HideInInspector]
    public ObjectPooler InteractableUIPooler;
    [HideInInspector]
    public ObjectPooler TimerUIPooler;
    [HideInInspector]
    public ObjectPooler AudioRemnantPooler;
    [HideInInspector]
    public ObjectPooler VoxelPooler;
    
    [SerializeField]
    GameObject DynamicInfoPrefab;
    [SerializeField]
    GameObject InteractableUIPrefab;
    [SerializeField]
    GameObject TimerUIPrefab;
    [SerializeField]
    GameObject AudioRemnantPrefab;
    [SerializeField]
    GameObject VoxelObjectPrefab;

    [HideInInspector]
	public static ObjectPoolerManager Instance {get;private set;}
	void Awake()
	{
		Instance = this;

		GameObject pools = new GameObject ("Pools");


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

        if (TimerUIPooler == null && TimerUIPrefab != null)
        {
            GameObject go = new GameObject("TimerUIPooler");
            TimerUIPooler = go.AddComponent<ObjectPooler>();
            TimerUIPooler.PooledObject = TimerUIPrefab;
            TimerUIPooler.PoolLength = 3;


            GameObject uis = new GameObject("Timer UIs");
            uis.transform.parent = pools.transform;

            TimerUIPooler.Parent = uis.transform;
            go.transform.parent = this.gameObject.transform;
            TimerUIPooler.Initialize();
        }

        if (AudioRemnantPooler == null && AudioRemnantPrefab != null)
        {
            GameObject go = new GameObject("AudioRemnantPooler");
            AudioRemnantPooler = go.AddComponent<ObjectPooler>();
            AudioRemnantPooler.PooledObject = AudioRemnantPrefab;
            AudioRemnantPooler.PoolLength = 15;


            GameObject audioRemnants = new GameObject("Audio Remnants");
            audioRemnants.transform.parent = pools.transform;

            AudioRemnantPooler.Parent = audioRemnants.transform;
            go.transform.parent = this.gameObject.transform;
            AudioRemnantPooler.Initialize();
        }

        if (VoxelPooler == null && VoxelObjectPrefab != null)
        {
            GameObject go = new GameObject("VoxelPooler");
            VoxelPooler = go.AddComponent<ObjectPooler>();
            VoxelPooler.PooledObject = VoxelObjectPrefab;
            VoxelPooler.PoolLength = 25;


            GameObject voxels = new GameObject("Voxels");
            voxels.transform.parent = pools.transform;

            VoxelPooler.Parent = voxels.transform;
            go.transform.parent = this.gameObject.transform;

            VoxelPooler.Initialize();
        }
    }
}
