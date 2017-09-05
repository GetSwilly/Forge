using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HubUIController : MonoBehaviour {

    [SerializeField]
    Text levelIndicator;


    void Update()
    {
        if(GameManager.Instance != null)
        {
            string txt = GameManager.Instance.CurrentLevel == 0 ? "Start Game" : "World " + GameManager.Instance.CurrentLevel.ToString();

            levelIndicator.text = txt;
        }
       
    }
}
