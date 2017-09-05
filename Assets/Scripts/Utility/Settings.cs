using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings {

    [SerializeField]
    [Range(30, 300)]
    int m_FrameRate = 60;

    
    public static Settings Instance { get; private set; }

    public Settings()
    {
        Instance = this;
    }



    public int FrameRate
    {
        get { return m_FrameRate; }
        set
        {
            m_FrameRate = value;

            if (m_FrameRate < 30)
                m_FrameRate = 30;

            if (m_FrameRate > 300)
                m_FrameRate = 300;
        }
    }
}
