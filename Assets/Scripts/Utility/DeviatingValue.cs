using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DeviatingValue {

}


[System.Serializable]
public class DeviatingFloat
{

    [SerializeField]
    float m_Mean = 0f;

    [SerializeField]
    float m_Sigma = 0f;

    public DeviatingFloat(float m, float s)
    {
        Mean = m;
        Sigma = s;
    }


    public float Mean
    {
        get { return m_Mean; }
        set { m_Mean = value; }
    }
    public float Sigma
    {
        get { return m_Sigma; }
        set
        {
            m_Sigma = value;

            if (m_Sigma < 0f)
                m_Sigma = 0f;
        }
    }
}


[System.Serializable]
public class DeviatingInteger
{
    [SerializeField]
    int m_Mean = 0;

    [SerializeField]
    int m_Sigma = 0;

    public DeviatingInteger(int m, int s)
    {
        Mean = m;
        Sigma = s;
    }


    public int Mean
    {
        get { return m_Mean; }
        set { m_Mean = value; }
    }
    public int Sigma
    {
        get { return m_Sigma; }
        set
        {
            m_Sigma = value;

            if (m_Sigma < 0)
                m_Sigma = 0;
        }
    }
}


[System.Serializable]
public class DeviatingAnimationCurve
{
    [SerializeField]
    AnimationCurve m_Mean;

    [SerializeField]
    AnimationCurve m_Sigma;

    public DeviatingAnimationCurve(AnimationCurve m, AnimationCurve s)
    {
        Mean = m;
        Sigma = s;
    }


    public AnimationCurve Mean
    {
        get { return m_Mean; }
        set { m_Mean = value; }
    }
    public AnimationCurve Sigma
    {
        get { return m_Sigma; }
        set { m_Sigma = value; }
        
    }
}