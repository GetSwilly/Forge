using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitStats {

    [Tooltip("Stat Attributes that affect unit capablities")]
    [SerializeField]
    private List<Stat> m_Stats = new List<Stat>();

    public event Delegates.StatChanged OnLevelChange;

    /// <summary>
    /// Check if unit has provided StatType
    /// </summary>
    public bool HasStat(StatType _type)
    {
        return GetStat(_type) != null;
    }

    /// <summary>
    /// Retrieve Stat object of provided StatType
    /// </summary>
	public Stat GetStat(StatType _type)
    {
        for (int i = 0; i < m_Stats.Count; i++)
        {
            if (m_Stats[i].Type == _type)
            {
                return m_Stats[i];
            }

        }

        return null;
    }


    /// <summary>
    /// Check if capable of altering Stat object corresponding to provided StatType by provided delta
    /// </summary>
    public bool CanChangeStatLevel(StatType _type, int _delta)
    {
        return CanChangeStatLevel(_type, _delta, true);
    }
    public bool CanChangeStatLevel(StatType _type, int _delta, bool canOvershootMax)
    {
        Stat _stat = GetStat(_type);

        if (_stat != null)
            return _stat.CanChangeLevel(_delta, canOvershootMax);

        return false;
    }

    /// <summary>
    /// Alter Stat object corresponding to provided StatType by provided delta
    /// </summary>
    public void ChangeStat(StatType _type, int _delta)
    {

        Stat _stat = GetStat(_type);

        if (_stat != null)
        {
            _stat.ModifyLevel(_delta);

            if (OnLevelChange != null)
            {
                OnLevelChange(_type, _stat.CurrentLevel);
            }
            // UpdateStatEffects(_type);
        }
    }


    /// <summary>
    /// Retrieve level of Stat object corresponding to provided StatType
    /// </summary>
    public int GetCurrentStatLevel(StatType _type)
    {
        Stat _stat = GetStat(_type);

        if (_stat != null)
        {
            return _stat.CurrentLevel;
        }


        return -1;
    }

    /// <summary>
    /// Retrieve maximum possible level of Stat object corresponding to provided StatType
    /// </summary>
    public int GetMaxStatLevel(StatType _type)
    {
        Stat _stat = GetStat(_type);

        if (_stat != null)
        {
            return _stat.MaxLevel;
        }


        return -1;
    }

    
    /// <summary>
    /// Validates Stats to ensure all possible StatTypes are implemented uniquelly
    /// </summary>
    public void Validate()
    {
        HashSet<StatType> unencounteredSet = new HashSet<StatType>(Enum.GetValues(typeof(StatType)) as StatType[]);

        for (int i = 0; i < m_Stats.Count; i++)
        {
            if (!unencounteredSet.Contains(m_Stats[i].Type))
            {
                m_Stats.RemoveAt(i);
                i--;
                continue;
            }

            m_Stats[i].Validate();
            unencounteredSet.Remove(m_Stats[i].Type);
        }


        HashSet<StatType>.Enumerator _enumerator = unencounteredSet.GetEnumerator();
        while (_enumerator.MoveNext())
        {
            Stat _newStat = new Stat(_enumerator.Current);
            m_Stats.Add(_newStat);

            _newStat.Validate();
        }

    }
}
