using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingInfo : MonoBehaviour {

	[SerializeField] Text titleText;
	[SerializeField] Text infoText;
	[SerializeField] Slider progressSlider;

	public void UpdateTitle(string text)
    {
		titleText.text = text;
	}

	public void UpdateInfo(string text)
    {
		infoText.text = text;
	}

	public void UpdateProgress(float amt)
    {
		progressSlider.value = Mathf.Clamp01(amt);
	}
}
