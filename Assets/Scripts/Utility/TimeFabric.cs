using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TimeFabric
{
    [SerializeField]
    [Range(0f, 10f)]
    float m_TimeScale = 1f;

    public TimeFabric()
    {
        SetTimeScale(1f);
    }


    public void SetTimeScale(float ts)
    {
        m_TimeScale = ts;
    }

    public float DeltaTime()
    {
        return DeltaTime(false);
    }
    public float DeltaTime(bool affectedByTimeSkew)
    {
        return Time.deltaTime * (affectedByTimeSkew ? 1f : m_TimeScale);
    }
    
}
