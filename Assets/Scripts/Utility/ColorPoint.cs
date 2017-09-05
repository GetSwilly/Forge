using UnityEngine;
using System.Collections;

[System.Serializable]
public class ColorPoint {

	[SerializeField] Color baseColor = Color.white;

    [SerializeField]
    float hueVariation;

    [SerializeField]
    float saturationVariation;

    [SerializeField]
    float valueVariation;

	public Color GetColor(){
		return Utilities.GetSimilarColor(baseColor, hueVariation, saturationVariation, valueVariation);
	}
}
