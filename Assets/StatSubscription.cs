using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatSubscription {

    [SerializeField]
    StatType m_Type;

    [SerializeField]
    AnimationCurve statCurve = AnimationCurve.Linear(0f, Stat.MAX_LEVEL, 0f, 0f);


    public void Validate()
    {
        Keyframe[] oldKeys = statCurve.keys;

        Keyframe[] newKeys = new Keyframe[Stat.MAX_LEVEL];
        
        for(int i = 0; i < newKeys.Length; i++)
        {
            bool foundKey = false;

            for(int k = 0; k < oldKeys.Length; k++)
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
