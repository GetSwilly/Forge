using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TextMesh))]
public class DynamicInfoScript : MonoBehaviour {

    [SerializeField]
    float displayTime = 0.75f;
    [SerializeField]
    float baseSpeed = 1f;

    [SerializeField]
    AnimationCurve speedCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    [SerializeField]
    AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    Transform myTransform;
    TextMesh infoText;
	Color infoColor;
	Vector3 moveDir;

	
	void Awake()
    {
		myTransform = GetComponent<Transform>();
		infoText = GetComponent<TextMesh>();
	}

	
	public void Initialize(int numToDisplay, Color textColor)
    {
		Initialize(numToDisplay.ToString(), textColor);
	}
	public void Initialize(string textToDisplay, Color textColor)
    {

		infoText.text = textToDisplay;

		myTransform.rotation = Camera.main.transform.rotation;

		infoColor = textColor;

		Vector3 randomDir = Random.insideUnitSphere;
		randomDir.y = Mathf.Abs(randomDir.y);
        randomDir.z = Mathf.Abs(randomDir.z);
        
		moveDir = randomDir; //(myTransform.right * randomDir.x) + (myTransform.up * randomDir.y);
		moveDir.Normalize();

		StartCoroutine(DisplayText());
	}



	void OnDisable()
    {
		StopAllCoroutines();
	}
	
	
	IEnumerator DisplayText()
    {

		infoColor.a = fadeCurve.Evaluate(0f); 
		infoText.color = infoColor;

		float timer = 0f;

		while(timer <= displayTime)
        {
            yield return null;


			timer += Time.deltaTime;

			infoColor.a = fadeCurve.Evaluate(timer/displayTime);  //= Utilities.FadeColor(infoColor, 1/(fadeTime/2));
			infoText.color = infoColor;

			myTransform.Translate(moveDir * baseSpeed * speedCurve.Evaluate(timer/displayTime) * Time.deltaTime);
		}

		
		gameObject.SetActive(false);
	}

}
