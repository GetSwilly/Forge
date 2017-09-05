using UnityEngine;
using System.Collections;

[System.Serializable]
public class Stat : LevelableAttribute{

	[SerializeField]
    StatType m_Type;




    public Stat(StatType _type) : this(_type, 0, 0, 0, 0, "") { }
    public Stat(StatType _type, int startL, int maxL, float baseVal, float upgradeDelta, string _description) :base(startL, Mathf.Min(maxL, Values.STAT_MAX_LEVEL), baseVal, upgradeDelta, _description)
    {
        m_Type = _type;
    }
    


    public void Copy(Stat other)
    {
        base.Copy(other as LevelableAttribute);

        m_Type = other.Type;
    }




    #region Accessors

    public StatType Type
    {
        get { return m_Type; }
    }
    
    #endregion


    public override void Validate()
    {
        MaxLevel = Mathf.Clamp(MaxLevel, 0, Values.STAT_MAX_LEVEL);

        base.Validate();

        switch (Type)
        {
            case StatType.Health:
                ValueDeltaPerLevel = Values.HEALTH_STAT_LEVEL_DELTA;
                break;
            case StatType.Speed:
                ValueDeltaPerLevel = Values.SPEED_STAT_LEVEL_DELTA;
                break;
            case StatType.Dexterity:
                ValueDeltaPerLevel = Values.DEXTERITY_STAT_LEVEL_DELTA;
                break;
            case StatType.Damage:
                ValueDeltaPerLevel = Values.DAMAGE_STAT_LEVEL_DELTA;
                break;
            case StatType.CriticalDamage:
                ValueDeltaPerLevel = Values.CRITICAL_DAMAGE_STAT_LEVEL_DELTA;
                break;
            case StatType.Luck:
                ValueDeltaPerLevel = Values.LUCK_STAT_LEVEL_DELTA;
                break;
        }
    }

    public override string ToString()
    {
		return m_Type.ToString() + ". Description: " + Description + ". Level: " + CurrentLevel.ToString();
	}
}
