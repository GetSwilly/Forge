using UnityEngine;
using System.Collections;

public class ClearLineOfSight : MonoBehaviour {

	[SerializeField] LayerMask blockingMask;

    [SerializeField] [Range(0f,1f)]
    float targetTransparency = 0.5f;

    [SerializeField] [Range(0f,3f)]
    float fadeTime = 0.5f;

    Transform playerTransform;
    Transform myTransform;



    void Start () {
		myTransform = this.GetComponent<Transform>();
		
		if(playerTransform == null){
			GameObject g = GameObject.FindGameObjectWithTag("Player");

			playerTransform = g == null ? null : g.transform;
		}
	}
	
	void LateUpdate () {

		if(playerTransform == null)
			return;


		Vector3 rayDirection = playerTransform.position - myTransform.position;
		RaycastHit[] hits = Physics.RaycastAll(new Ray(myTransform.position, rayDirection), rayDirection.magnitude, blockingMask);
		
		for(int i = 0; i < hits.Length; i++){
            if (hits[i].transform.tag == "Player" || hits[i].transform.gameObject == MapGenerator.Instance.GroundObject)
				continue;
			
			Renderer R = hits[i].collider.GetComponent<Renderer>();
			// no renderer attached? go to next hit 
			if (R == null) 
				continue;
			
			AutoTransparent AT = R.GetComponent<AutoTransparent>();
			if (AT == null) // if no script is attached, attach one
			{
				AT = R.gameObject.AddComponent<AutoTransparent>();
			}


			AT.BeTransparent(targetTransparency, fadeTime); 
		}
	}
}
