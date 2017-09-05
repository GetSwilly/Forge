using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelableAttribute {

    public delegate void AlertChange(LevelableAttribute attr);
    public event AlertChange OnValueChange;


    [Tooltip("Current Attribute Level")]
    [SerializeField]
    int currentLevel;

    [Tooltip("Maximum possible level for Attribute")]
    [SerializeField]
    int maxLevel;


    int startingLevel;
    int addedLevels;

    [Tooltip("Should ignore the Max Level?")]
    [SerializeField]
    bool unlimitedLevels = false;

    [Tooltip("Base value associated with the Attribute")]
    [SerializeField]
    float baseValue = 0f;

    [Tooltip("Change in base value applied with each successive level")]
    [SerializeField]
    float valueDeltaPerLevel = 0f;

    [Tooltip("Description of the Attribute")]
    [SerializeField]
    string description;

    [Tooltip("Is Attribute locked to level changes?")]
    [SerializeField]
    bool isLocked = false;



    #region Constructors

    public LevelableAttribute(int startL, int maxL, float baseVal, float upgradeDelta, string _description)
    {
        MaxLevel = maxL;
        CurrentLevel = startL;
        

        baseValue = baseVal;
        valueDeltaPerLevel = upgradeDelta;

        description = _description;
    }

    #endregion
    

    public void ChangeLevel(int levelDelta)
    {
        if (IsLocked)
            return;



        int _level = CurrentLevel;
        CurrentLevel += levelDelta;

       

        addedLevels += currentLevel - _level;

        if (OnValueChange != null)
            OnValueChange(this);
    }
    public bool CanChangeLevel(int levelDelta)
    {
        return CanChangeLevel(levelDelta, false);
    }
    public bool CanChangeLevel(int levelDelta, bool canOvershootMax)
    {
        return !IsLocked && (currentLevel + levelDelta >= 1) && (canOvershootMax || (unlimitedLevels || currentLevel + levelDelta <= maxLevel));
    }

    public void SetLevel(int newLevel)
    {
        ChangeLevel(newLevel - CurrentLevel);
    }


    public virtual void Copy(LevelableAttribute other)
    {
        CurrentLevel = other.CurrentLevel;
        StartingLevel = other.StartingLevel;
        MaxLevel = other.MaxLevel;

        addedLevels = other.AddedLevels;

        baseValue = other.BaseValue;
        valueDeltaPerLevel = other.ValueDeltaPerLevel;

        description = other.Description;
    }




    #region Accessors


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
        protected set { startingLevel = Mathf.Clamp(value, 0, MaxLevel); }
    }
    public int AddedLevels
    {
        get { return addedLevels; }
    }


    public float CurrentValue
    {
        get { return BaseValue + ((CurrentLevel - 1) * ValueDeltaPerLevel); }
    }




    public float BaseValue
    {
        get { return baseValue; }
    }
    public float ValueDeltaPerLevel
    {
        get { return valueDeltaPerLevel; }
        protected set { valueDeltaPerLevel = value; }
    }
    public string Description
    {
        get { return description; }
    }

    public bool IsLocked
    {
        get { return isLocked; }
        set { isLocked = value; }
    }
    #endregion


    public virtual void Validate()
    {
        MaxLevel = MaxLevel;
        CurrentLevel = CurrentLevel;
    }

    public override string ToString()
    {
        return string.Format("Description: {0}. Level: {1}.", Description, CurrentLevel.ToString());
    }
}
