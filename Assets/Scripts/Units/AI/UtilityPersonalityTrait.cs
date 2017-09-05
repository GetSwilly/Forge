using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class UtilityPersonalityTrait {

    public enum PersonalityTrait_Type { Fear, Anger };

    [SerializeField]
    public PersonalityTrait_Type m_Type;


    [Tooltip("Used to distinguish between traits")]
    [SerializeField]
    int personalityID = 1;

    
    float currentValue = 0f;
    
    [Tooltip("Determines how quickly the trait deteriorates per second")]
    [SerializeField]
    DeviatingFloat m_Decay = new DeviatingFloat(0f, 0f);

    float decayRate;


    List<CustomTuple2<Transform, float>> influenceQueue = new List<CustomTuple2<Transform, float>>();
    Dictionary<Transform, float> influenceTracker = new Dictionary<Transform, float>();


    public void Initialize()
    {
        decayRate = (float)Utilities.GetRandomGaussian(m_Decay);

        CurrentValue = 0f;
    }




   
    public void Decay(float deltaTime)
    {
        ModifyValue(DecayRate * deltaTime);
    }


   
    public void ModifyValue(float delta)
    {
        ModifyValue(delta, null);
    }
    public void ModifyValue(float delta,Transform _influencer)
    {
        float newVal = Mathf.Clamp(currentValue + delta, 0f, 100f);

        float diff = newVal - currentValue;
       CurrentValue = newVal;


        if (diff == 0)
            return;


        if (diff < 0)
        {
            while (diff < 0 && influenceQueue.Count > 0)
            {
                float newDiff = diff + influenceQueue[0].Item2;


                if (newDiff < 0)
                {
                    if (influenceTracker.ContainsKey(influenceQueue[0].Item1))
                    {
                        influenceTracker[influenceQueue[0].Item1] -= influenceQueue[0].Item2;

                        if (influenceTracker[influenceQueue[0].Item1] <= 0)
                        {
                            influenceTracker.Remove(influenceQueue[0].Item1);
                        }
                    }

                    influenceQueue.RemoveAt(0);

                   
                }
                else if(influenceTracker.ContainsKey(influenceQueue[0].Item1))
                {
                    influenceTracker[influenceQueue[0].Item1] -= newDiff - diff;

                }

                diff = newDiff;
            }
        }
        else
        {
            influenceQueue.Add(new CustomTuple2<Transform, float>(_influencer, diff));


            if (influenceTracker.ContainsKey(_influencer))
            {
                influenceTracker[_influencer] += diff;
            }
            else
            {
                influenceTracker.Add(_influencer, diff);
            }
        }

    }


    public float GetInfluenceAmount(Transform _influencer)
    {
        return influenceTracker.ContainsKey(_influencer) ? influenceTracker[_influencer] : 0f;
    }



    #region Accessors

    public Transform StrongestInfluencer
    {
        get
        {
            List<Transform> keyList = new List<Transform>(influenceTracker.Keys);

            Transform _transform = null;
            float largestVal = float.MinValue;

            for (int i = 0; i < keyList.Count; i++)
            {
                if (influenceTracker[keyList[i]] > largestVal)
                {
                    _transform = keyList[i];
                    largestVal = influenceTracker[keyList[i]];
                }
            }

            return _transform;
        }
    }
    public Transform WeakestInfluencer
    {
        get
        { 
            List<Transform> keyList = new List<Transform>(influenceTracker.Keys);

            Transform _transform = null;
            float smallestVal = float.MaxValue;

            for (int i = 0; i < keyList.Count; i++)
            {
                if (influenceTracker[keyList[i]] < smallestVal)
                {
                    _transform = keyList[i];
                    smallestVal = influenceTracker[keyList[i]];
                }
            }

            return _transform;
        }
    }
   

    public PersonalityTrait_Type Type
    {
        get { return m_Type; }
    }
    public float CurrentValue
    {
        get { return currentValue; }
        protected set { currentValue = Mathf.Clamp(value, 0f, 100f); }
    }
    public float DecayRate
    {
        get { return decayRate; }
    }

    public int PersonalityID
    {
        get { return personalityID; }
    }


    #endregion
}
