using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEventArgs {

    public UIManager.Component component;
    public string text;

    public float percentage;
    public bool shouldSetImmediately;

    public UIEventArgs(UIManager.Component component, string txt, float percentage, bool shouldSetImmediately)
    {
        this.component = component;
        this.text = txt;
        this.percentage = percentage;
        this.shouldSetImmediately = shouldSetImmediately;
    }

    public override string ToString()
    {
        return string.Format("Component: {0}. Percentage: {1}. ShouldSetImmediately? : {2}", component, percentage, shouldSetImmediately);
    }
}
