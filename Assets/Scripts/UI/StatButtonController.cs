using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatButtonController : MenuButton {

    public delegate void UpgradeClickAction(StatType _type);
    public UpgradeClickAction OnUpgradeClicked;


    [SerializeField]
    private Color maxLevelColor;
    

    Color prgOriginalColor;

    [SerializeField] Button m_UpgradeButton;


    private StatType m_StatType;
    private int m_CurrentLevel = 0;
    private int m_MaxLevel = 0;
    private ProgressBarController m_Progress;


    protected override void Awake()
    {
        base.Awake();

        m_Progress = GetComponentInChildren<ProgressBarController>();
        prgOriginalColor = m_Progress.Color;
    }

    public void Initialize(Menu _menu, StatType _type, int _currentLevel, int _maxLevel)
    {
        base.Initialize(_menu);

        StatType = _type;
        SetName();

        CurrentLevel = _currentLevel;
        MaxLevel = _maxLevel;
    }


    public void Upgrade()
    {
        if (OnUpgradeClicked != null)
            OnUpgradeClicked(StatType);
    }


    public void UpdateLevels(int _curLevel, int _maxLevel)
    {
        MaxLevel = _maxLevel;
        CurrentLevel = _curLevel;
    }
  
    

    private void SetProgress()
    {
        SecondaryText = string.Format("{0} / {1}", CurrentLevel, MaxLevel);

        if(CurrentLevel >= MaxLevel)
        {
            m_UpgradeButton.interactable = false;
            m_Progress.SetColor(maxLevelColor);
        }
        else
        {
            m_UpgradeButton.interactable = true;
            m_Progress.SetColor(prgOriginalColor);
        }

        if (m_Progress == null)
            return;

      if(MaxLevel == 0)
        {
            m_Progress.SetPercentage(0f, false);
        }

        float levelPctg = CurrentLevel / (float)MaxLevel;

        if (levelPctg > 1f)
            levelPctg = 1f;


        m_Progress.SetPercentage(levelPctg, false);
    }


    public void SetName()
    {
        string s = "";


        switch (m_StatType)
        {
            case StatType.Health:
                s = "Health";
                break;
            case StatType.Speed:
                s = "Speed";
                break;
            case StatType.Dexterity:
                s = "Dexterity";
                break;
            case StatType.Damage:
                s = "Damage";
                break;
            case StatType.CriticalDamage:
                s = "Critical Damage";
                break;
            case StatType.Luck:
                s = "Luck";
                break;

        }

        MainText = s;
    }


  
    public StatType StatType
    {
        get { return m_StatType; }
        set { m_StatType = value; }
    }
    public int CurrentLevel
    {
        get { return m_CurrentLevel; }
        set
        {
            m_CurrentLevel = value;

            if (m_CurrentLevel < 0)
            {
                m_CurrentLevel = 0;
            }



            SetProgress();
        }
    }
    public int MaxLevel
    {
        get { return m_MaxLevel; }
        set
        {
            m_MaxLevel = value;

            if(m_MaxLevel < 0)
            {
                m_MaxLevel = 0;
            }

            SetProgress();
        }
    }
}
