using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class ProgressBarController : DisplayUI {

    static readonly float FADE_TIME = 0.7f;


	public enum Enable_Method { FullToFull, FullToDesired, FullToEmpty, EmptyToEmpty, EmptyToDesired,EmptyToFull};
	public Enable_Method startingMethod;

    [SerializeField]
    Color barColor = Color.white;

    [SerializeField]
    Text progressText;

	public bool showPercentage = false;

    [SerializeField]
    float progressSpeed = 0.4f;

	float desiredPercentage = 1f;
	float curPercentage = 1f;


    [SerializeField]
    Image progessBarImage;

    bool isFading = false;
    

    Camera mainCam;
  

   

	void Start()
    {
		SetColor(barColor);

		//switch(startingMethod)
  //      {
		//case Enable_Method.FullToFull:
		//	desiredPercentage = 1f;
		//	SetFillPercentage(1f);
		//	break;
		//case Enable_Method.FullToDesired:
		//	SetFillPercentage(1f);
		//	break;
		//case Enable_Method.FullToEmpty:
		//	desiredPercentage = 0f;
		//	SetFillPercentage(1f);
		//	break;
		//case Enable_Method.EmptyToFull:
		//	desiredPercentage = 1f;
		//	SetFillPercentage(0f);
		//	break;
		//case Enable_Method.EmptyToDesired:
		//	SetFillPercentage(0f);
		//	break;
		//case Enable_Method.EmptyToEmpty:
		//	desiredPercentage = 0f;
		//	SetFillPercentage(0f);
		//	break;
		//}


        mainCam = Camera.main;
	}

	void Update()
    {
		if(curPercentage != desiredPercentage)
        {
			float diff = desiredPercentage - curPercentage;

			if(diff > 0)
            {
				curPercentage += progressSpeed * Time.deltaTime;

				if(curPercentage > desiredPercentage)
					curPercentage = desiredPercentage;
			}
            else
            {
				curPercentage -= progressSpeed * Time.deltaTime;
				
				if(curPercentage < desiredPercentage)
					curPercentage = desiredPercentage;
			}

			SetFillPercentage(curPercentage);
		}
       
    }





    public override void SetPercentage(float pctg, bool setImmediately)
    {
        if (setImmediately)
        {
            SetFillPercentage(pctg);
        }
        else
        {
            SetDesiredPercentage(pctg);
        }
    }

    void SetDesiredPercentage(float _desired)
    {
		desiredPercentage = Mathf.Clamp01(_desired);
	}
	void SetFillPercentage(float _fill)
    {
		curPercentage = Mathf.Clamp01(_fill);

		if(progessBarImage == null)
			return;


        progessBarImage.fillAmount = curPercentage;

		int percent = (int)(progessBarImage.fillAmount * 100);

		if(showPercentage)
			SetText(percent.ToString() + "%");
	}

	public override void SetText(string txt)
    {
		if(progressText != null)
			progressText.text = txt;
	}
	
	public override void SetColor(Color newColor)
    {
        barColor = newColor;


		if(progessBarImage == null)
			return;

        progessBarImage.color = newColor;
	}



    public Color Color
    {
        get { return barColor; }
        set
        {
            barColor = value;

            SetColor(barColor);
        }
    }
}
