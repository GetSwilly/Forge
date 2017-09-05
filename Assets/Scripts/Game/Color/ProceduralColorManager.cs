using UnityEngine;
using System.Collections;

public class ProceduralColorManager : MonoBehaviour {

	static readonly int maxIterations = 10;
	static readonly float minEuclideanDistanceFromWhite = 100f;
	//static readonly int INFINITY = 999999999;

	public Color baseColor;
	public Color primaryColor;
	public Color secondaryColor;

	[SerializeField] [Range(1f, 3f)] float saturationMultiplier = 1f;
	[SerializeField] [Range(1f, 3f)] float valueMultiplier = 1f;

	//[SerializeField] float minEuclideanDistance = 50f;
	[SerializeField] [Range(0f,180f)] float hueVariation = 20f;

	[HideInInspector]
	public static ProceduralColorManager Instance { get; private set; }
	
	void Awake(){

		if(Instance != null)
			Destroy(this);


		Instance = this;
	}

	public void GenerateRandomColors(){
	
		//int countA = 0;
		//do{
		//	int countB = 0;
		//	do{
		//		countB++;
				primaryColor = GetRandomColor();
		//	}while(countB < maxIterations && IsWhite(primaryColor));

	// 		countA++;
			//primaryColor = GetSimilarColor(Vector4.zero, 0, INFINITY);
			primaryColor = SaturateColor(primaryColor);
			baseColor = DesaturateColor(primaryColor);
	//	}while(countA < maxIterations && IsWhite(baseColor) || IsWhite(primaryColor));
		
		secondaryColor = Utilities.GetComplementaryColor(primaryColor, hueVariation);//SaturateColor(GetSimilarColor(primaryColor, minEuclideanDistance, INFINITY));
	}
	public void GenerateThemedColors(Color themeColor){
		//primaryColor = Utilities.GetSimilarColor(themeColor, hueVariation);

		//Debug.Log(themeColor.ToString());


		primaryColor = SaturateColor(themeColor);
		//Debug.Log("Primary Color: " + primaryColor.ToString());

		baseColor = DesaturateColor(primaryColor);

		secondaryColor = Utilities.GetComplementaryColor(primaryColor, hueVariation);//SaturateColor(Get

	}

	public void SaturateGeneratedColors(float satMultiplier, float valMultiplier){
		primaryColor = SaturateColor(primaryColor, satMultiplier, valMultiplier);
		baseColor = SaturateColor(baseColor, satMultiplier, valMultiplier);
		secondaryColor = SaturateColor(secondaryColor, satMultiplier, valMultiplier);
	}
	

	public Color GetSimilarColor(Color stableColor, float minDifference, float maxDifference){

		Color newColor;

		int count = 0;
		do{
			newColor = GetRandomColor();
			count++;
		}while(count < maxIterations && !IsWhite(newColor) && Vector4.Distance(newColor,stableColor) < minDifference && Vector4.Distance(newColor,stableColor) > maxDifference);

		return newColor;
	}
	public Color GetRandomColor(){
		return new Color(Random.value, Random.value, Random.value);
	}

	public bool IsWhite(Color toCheck){
		return (Vector4.Distance(toCheck, Color.white) < minEuclideanDistanceFromWhite);
	}
	public bool IsBlack(Color toCheck){
		return (Vector4.Distance(toCheck, Color.black) < minEuclideanDistanceFromWhite);
	}




	public Color SaturateColor(Color toSaturate){
		return SaturateColor(toSaturate, saturationMultiplier, valueMultiplier);
	}
	public Color DesaturateColor(Color toDesaturate){
		return SaturateColor(toDesaturate, 1/saturationMultiplier, 1/valueMultiplier);
	}


	public Color SaturateColor(Color toSaturate, float satMultiplier, float valMultiplier){
		Vector3 hsvColor = Utilities.RGBtoHSV(toSaturate);
		hsvColor.y *= satMultiplier; // = Mathf.Clamp(hsvColor.y, 0.6f, 1f);
		hsvColor.z *= valMultiplier; // = Mathf.Clamp01(hsvColor.y + 0.2f);
		return Utilities.HSVtoRGB(hsvColor);
	}

	/*
	public Color DesaturateColor(Color toDesaturate){
		Vector3 hsvColor = Utilities.RGBtoHSV(toDesaturate);
		hsvColor.y /= saturationMultiplier; //= Mathf.Clamp(hsvColor.y, 0, 0.7f);
		hsvColor.z /= valueMultiplier; // = Mathf.Clamp01(hsvColor.y - 0.15f);
		return Utilities.HSVtoRGB(hsvColor);
	}*/
}
