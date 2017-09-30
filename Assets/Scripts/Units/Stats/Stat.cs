using UnityEngine;
using System.Collections;

[System.Serializable]
public class Stat
{

    public static readonly int MAX_LEVEL = 10;
    public event Delegates.StatChanged OnValueChange;


    [SerializeField]
    StatType m_Type;

    [Tooltip("Current Attribute Level")]
    [SerializeField]
    int currentLevel;

    [Tooltip("Maximum possible level for Attribute")]
    [SerializeField]
    int maxLevel = MAX_LEVEL;


    int startingLevel;
    int addedLevels;

    [Tooltip("Should ignore the Max Level?")]
    [SerializeField]
    bool unlimitedLevels = false;


    [Tooltip("Description of the Attribute")]
    [SerializeField]
    string description;

    [Tooltip("Is Attribute locked to level changes?")]
    [SerializeField]
    bool isLevelLocked = false;


    //public Stat(int startL, int maxL, float baseVal, float upgradeDelta, string _description)
    //{
    //    MaxLevel = maxL;
    //    CurrentLevel = startL;


    //    baseValue = baseVal;
    //    valueDeltaPerLevel = upgradeDelta;

    //    description = _description;
    //}
    public Stat(StatType _type)
    {
        Type = _type;
    }
    public Stat(Stat other)
    {
        m_Type = other.Type;

        MaxLevel = other.MaxLevel;
        StartingLevel = other.StartingLevel;
        CurrentLevel = other.CurrentLevel;

        addedLevels = other.AddedLevels;

        description = other.Description;
    }


    public void ModifyLevel(int levelDelta)
    {
        if (IsLevelLocked)
            return;



        int _level = CurrentLevel;
        CurrentLevel += levelDelta;



        addedLevels += currentLevel - _level;

        if (OnValueChange != null)
            OnValueChange(Type, CurrentLevel);
    }
    public bool CanChangeLevel(int levelDelta)
    {
        return CanChangeLevel(levelDelta, false);
    }
    public bool CanChangeLevel(int levelDelta, bool canOvershootMax)
    {
        return !IsLevelLocked && (currentLevel + levelDelta >= 1) && (canOvershootMax || (unlimitedLevels || currentLevel + levelDelta <= maxLevel));
    }

    public void SetLevel(int newLevel)
    {
        ModifyLevel(newLevel - CurrentLevel);
    }




    #region Accessors

    public StatType Type
    {
        get { return m_Type; }
        private set { m_Type = value; }
    }

    public int CurrentLevel
    {
        get { return currentLevel; }
        protected set
        {
            currentLevel = Mathf.Clamp(value, 0, value);

            if (!unlimitedLevels && currentLevel > MaxLevel)
                currentLevel = MaxLevel;
        }
    }
    public int MaxLevel
    {
        get { return maxLevel; }
        protected set
        {
            maxLevel = Mathf.Clamp(value, 0, value);

            CurrentLevel = CurrentLevel;
        }
    }
    public int StartingLevel
    {
        get { return startingLevel; }
        protected set { startingLevel = Mathf.Clamp(value, 0, value); }
    }
    public int AddedLevels
    {
        get { return addedLevels; }
    }

    public string Description
    {
        get { return description; }
    }

    public bool IsLevelLocked
    {
        get { return isLevelLocked; }
        set { isLevelLocked = value; }
    }

    #endregion



    public void Validate()
    {
        MaxLevel = Mathf.Clamp(MaxLevel, 0, Values.STAT_MAX_LEVEL);

        MaxLevel = MaxLevel;
        CurrentLevel = CurrentLevel;

        //switch (Type)
        //{
        //    case StatType.Health:
        //        ValueChangePerLevel = Values.HEALTH_STAT_LEVEL_DELTA;
        //        break;
        //    case StatType.Speed:
        //        ValueChangePerLevel = Values.SPEED_STAT_LEVEL_DELTA;
        //        break;
        //    case StatType.Dexterity:
        //        ValueChangePerLevel = Values.DEXTERITY_STAT_LEVEL_DELTA;
        //        break;
        //    case StatType.Damage:
        //        ValueChangePerLevel = Values.DAMAGE_STAT_LEVEL_DELTA;
        //        break;
        //    case StatType.CriticalDamage:
        //        ValueChangePerLevel = Values.CRITICAL_DAMAGE_STAT_LEVEL_DELTA;
        //        break;
        //    case StatType.Luck:
        //        ValueChangePerLevel = Values.LUCK_STAT_LEVEL_DELTA;
        //        break;
        //}
    }


    public override string ToString()
    {
        return m_Type.ToString() + ". Description: " + Description + ". Level: " + CurrentLevel.ToString();
    }
}
