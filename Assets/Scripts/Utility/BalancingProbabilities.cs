using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BalancingProbabilities {

    [System.Serializable]
    class Probability
    {
        [SerializeField]
        string m_Identifier;

        [SerializeField]
        [Range(0, 100)]
        int m_Value;

        [SerializeField]
        bool isLocked;


        public string Identifier
        {
            get { return m_Identifier; }
        }
        public int Value
        {
            get { return m_Value; }
            set
            {
                m_Value = value;

                if(m_Value < 0)
                {
                    m_Value = 0;
                }

                if(m_Value > 100)
                {
                    m_Value = 100;
                }
            }
        }
        public bool IsLocked
        {
            get { return isLocked; }
        }
    }



    [SerializeField]
    List<Probability> m_Probabilities = new List<Probability>();


    public void Validate(bool allowFinalDuplicate)
    {
        HashSet<string> identifiers = new HashSet<string>();
        int totalProbability = 0;

        for (int i = 0; i < m_Probabilities.Count; i++)
        {
            if ((i == m_Probabilities.Count - 1 && !allowFinalDuplicate) && identifiers.Contains(m_Probabilities[i].Identifier))
            {
                m_Probabilities.RemoveAt(i);
                i--;
            }
            else
            {
                identifiers.Add(m_Probabilities[i].Identifier);
                totalProbability += m_Probabilities[i].Value;
            }
        }




        for (int i = 0; i < m_Probabilities.Count; i++)
        {
            if (!m_Probabilities[i].IsLocked)
            {
                m_Probabilities[i].Value = totalProbability == 0 ? 0 :(int)((m_Probabilities[i].Value / (float)totalProbability) * 100);
            }
        }
    }
}
