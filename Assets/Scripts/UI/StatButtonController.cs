using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatButtonController : MenuButton
{

    public delegate void UpgradeClickAction(StatType _type);
    public UpgradeClickAction OnUpgradeClicked;


    [SerializeField]
    private Color maxLevelColor;


    Color prgOriginalColor;


    private StatType m_StatType;
    private int currentLevel = 0;
    private int maxLevel = 0;
    private ProgressBarController m_Progress;


    protected void Awake()
    {

        m_Progress = GetComponentInChildren<ProgressBarController>();
        prgOriginalColor = m_Progress.Color;
    }

    public void Initialize(StatType _type, int _currentLevel, int _maxLevel)
    {
        base.Initialize(_type.ToString(), string.Format("{0} / {1}", _currentLevel, _maxLevel));

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
        SetSecondaryText(string.Format("{0} / {1}", CurrentLevel, MaxLevel));

        if (CurrentLevel >= MaxLevel)
        {
            mainButton.interactable = false;
            m_Progress.Color = maxLevelColor;
        }
        else
        {
            mainButton.interactable = true;
            m_Progress.Color = prgOriginalColor;
        }

        if (m_Progress == null)
            return;

        if (MaxLevel == 0)
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
        string name = "";


        switch (m_StatType)
        {
            case StatType.Health:
                name = "Health";
                break;
            case StatType.Speed:
                name = "Speed";
                break;
            case StatType.Dexterity:
                name = "Dexterity";
                break;
            case StatType.Damage:
                name = "Damage";
                break;
            case StatType.CriticalDamage:
                name = "Critical Damage";
                break;
            case StatType.Luck:
                name = "Luck";
                break;

        }

        SetMainText(name);
    }



    public StatType StatType
    {
        get { return m_StatType; }
        set { m_StatType = value; }
    }
    public int CurrentLevel
    {
        get { return currentLevel; }
        set
        {
            currentLevel = Mathf.Clamp(value, 0, value);
            SetProgress();
        }
    }
    public int MaxLevel
    {
        get { return maxLevel; }
        set
        {
            maxLevel = Mathf.Clamp(value, 0, value);
            SetProgress();
        }
    }
}
