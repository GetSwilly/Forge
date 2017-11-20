using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatSubscriptions
{

    [System.Serializable]
    class Subscription
    {
        [SerializeField]
        StatType m_Type;

        [SerializeField]
        public AnimationCurve statCurve = AnimationCurve.Linear(0f, Stat._MaxLevel, 0f, 0f);

        public Subscription(StatType _type)
        {
            m_Type = _type;
        }

        public StatType Type
        {
            get { return m_Type; }
        }

        public void Validate()
        {
            Keyframe[] oldKeys = statCurve.keys;

            Keyframe[] newKeys = new Keyframe[Stat._MaxLevel];

            for (int i = 0; i < newKeys.Length; i++)
            {
                bool foundKey = false;

                for (int k = 0; k < oldKeys.Length; k++)
                {
                    if (oldKeys[k].time.Equals(i))
                    {
                        newKeys[i] = oldKeys[k];
                        newKeys[i].tangentMode = i;

                        foundKey = true;
                    }
                }

                if (!foundKey)
                {
                    newKeys[i] = new Keyframe(i, 0f);
                }
            }

            statCurve.keys = newKeys;
        }
    }


    [SerializeField]
    List<Subscription> m_Subscriptions = new List<Subscription>();


    public float GetValue(StatType type, int level)
    {
        for(int i =0; i < m_Subscriptions.Count; i++)
        {
            if(type == m_Subscriptions[i].Type)
            {
                return m_Subscriptions[i].statCurve.Evaluate(level);
            }
        }

        return 0f;
    }

    public void Validate()
    {
        AddMissingSubscriptions();
        ValidateSubscriptions();
    }

    void AddMissingSubscriptions()
    {
        HashSet<StatType> unencounteredSet = new HashSet<StatType>(Enum.GetValues(typeof(StatType)) as StatType[]);

        for (int i = 0; i < m_Subscriptions.Count; i++)
        {
            if (!unencounteredSet.Contains(m_Subscriptions[i].Type))
            {
                m_Subscriptions.RemoveAt(i);
                i--;
                continue;
            }

            m_Subscriptions[i].Validate();
            unencounteredSet.Remove(m_Subscriptions[i].Type);
        }


        HashSet<StatType>.Enumerator _enumerator = unencounteredSet.GetEnumerator();
        while (_enumerator.MoveNext())
        {
            Subscription _subscription = new Subscription(_enumerator.Current);
            m_Subscriptions.Add(_subscription);
        }
    }
    void ValidateSubscriptions()
    {
        m_Subscriptions.ForEach(s => s.Validate());
    }
}
