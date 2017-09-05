using UnityEngine;
using System.Collections;

public class ObjectColorManager : MonoBehaviour {
	
	public Color baseColor = Color.black;
	public Color primaryColor = Color.black;
	public Color secondaryColor = Color.black;

	public Renderer[] baseColorRenderers = new Renderer[0];
	public Renderer[] primaryColorRenderers = new Renderer[0];
	public Renderer[] secondaryColorRenderers = new Renderer[0];
	
	
	void Start(){
		baseColor = ProceduralColorManager.Instance.baseColor;
		primaryColor = ProceduralColorManager.Instance.primaryColor;
		secondaryColor = ProceduralColorManager.Instance.secondaryColor;
		
		SetColors();
	}
	
	public void SetColors(){

		for(int i = 0; i < baseColorRenderers.Length; i++){

			if(baseColorRenderers[i].sharedMaterial != null)
				baseColorRenderers[i].sharedMaterial.color = baseColor;
		}
		
		for(int i = 0; i < primaryColorRenderers.Length; i++){
			if(primaryColorRenderers[i].sharedMaterial != null)
				primaryColorRenderers[i].sharedMaterial.color = primaryColor;
		}
		
		for(int i = 0; i < secondaryColorRenderers.Length; i++){
			if(secondaryColorRenderers[i].sharedMaterial != null)
				secondaryColorRenderers[i].sharedMaterial.color = secondaryColor;
		}

	}
	void OnValidate(){
		SetColors();
	}
}
