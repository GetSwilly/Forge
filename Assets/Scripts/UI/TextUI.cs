using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TextUI : DisplayUI {

    Text m_Text;

    protected override void Awake()
    {
        base.Awake();

        m_Text = GetComponent<Text>();
    }


    public override void SetPercentage(float pctg, bool setImmediately)
    {
    
    }

    public override void SetText(string txt)
    {
        if(m_Text == null)
        {
            m_Text = GetComponent<Text>();
        }


        m_Text.text = txt;
    }


    public override void SetColor(Color _color)
    {
        m_Text.color = _color;
    }
}
