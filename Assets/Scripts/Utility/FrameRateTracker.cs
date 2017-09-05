using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[System.Serializable]
public class FrameRateTracker {

    static readonly double FRAME_BUFFER = 0.002;


    [SerializeField]
    [Range(30, 300)]
    int m_FrameRate = 60;

    Stopwatch m_Watch;
    //float lastYield;

    public static FrameRateTracker Instance { get; private set; }
    public FrameRateTracker()
    {
        Instance = this;

        m_Watch = new Stopwatch();
        m_Watch.Start();
        //lastYield = Time.realtimeSinceStartup;
    }


    public void Reset()
    {
       m_Watch.Reset();
        m_Watch.Start();
        //lastYield = Time.realtimeSinceStartup; ;
    }

    public bool IsFrameDue()
    {
        //return (Time.realtimeSinceStartup - lastYield) >= (1 / (double)FrameRate);
        return m_Watch.Elapsed.TotalSeconds >= (1 / (double)FrameRate);
    }


    public int FrameRate
    {
        get { return m_FrameRate; }
    }
}
