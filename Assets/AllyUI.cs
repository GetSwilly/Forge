using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllyUI : MonoBehaviour {
    
    public Text text;
    public ProgressBarController progressBar;

    public AllyUI(Text text, ProgressBarController progressBar)
    {
        this.text = text;
        this.progressBar = progressBar;
    }
}
