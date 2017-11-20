using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatSubscription {
    static readonly float EPSILON = 0.1f;

    [SerializeField]
    StatType m_Type;

    [SerializeField]
    AnimationCurve statCurve = AnimationCurve.Linear(0f, Stat._MaxLevel, 0f, 0f);


    public void Validate()
    {
        Keyframe[] oldKeys = statCurve.keys;

        Keyframe[] newKeys = new Keyframe[Stat._MaxLevel];
        
        for(int i = 0; i < newKeys.Length; i++)
        {
            bool foundKey = false;

            for(int k = 0; k < oldKeys.Length; k++)
            {
                if (Mathf.Abs(i - oldKeys[k].time) <= EPSILON)
                {
                    newKeys[i] = oldKeys[k];
                    newKeys[i].time = i;

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
