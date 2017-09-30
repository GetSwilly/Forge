using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UtilityPsyche {


    public enum Personality_Precedence { Random, Ordered }
    public Personality_Precedence precedence = Personality_Precedence.Random;

    [SerializeField]
    List<UtilityPersonalityTrait> personalityTraits = new List<UtilityPersonalityTrait>();


    protected List<CustomTuple2<Transform, float>> damageQueue = new List<CustomTuple2<Transform, float>>();


    public void Initialize(Health _health)
    {
        if (personalityTraits.Count == 0)
            return;



        List<UtilityPersonalityTrait> tempTraits = new List<UtilityPersonalityTrait>();

        if (precedence == Personality_Precedence.Ordered)
        {

            personalityTraits.Sort(delegate (UtilityPersonalityTrait a, UtilityPersonalityTrait b)
            {
                return a.PersonalityID - b.PersonalityID;
            });
        }
        else
        {

            while (personalityTraits.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, personalityTraits.Count);
                tempTraits.Add(personalityTraits[index]);

                personalityTraits.RemoveAt(index);
            }
        }

        personalityTraits = tempTraits;


        for (int i = 0; i < personalityTraits.Count; i++)
        {
            personalityTraits[i].Initialize();
        }

       
        if (_health != null)
            _health.OnHealthChange += UpdateDamageQueue;
    }


    public void Update(float deltaTime)
    {
        personalityTraits.ForEach(t =>
        {
            t.Decay(deltaTime);
        });
    }


    


    public void ModifyTrait(UtilityPersonalityTrait.PersonalityTrait_Type _type, float delta, Transform influencerTransform)
    {
        UtilityPersonalityTrait _trait = GetTrait(_type);

        if (_trait == null)
            return;

        _trait.ModifyValue(delta, influencerTransform);
    }










    public UtilityPersonalityTrait GetTrait(UtilityPersonalityTrait.PersonalityTrait_Type _type)
    {

        for (int i = 0; i < personalityTraits.Count; i++)
        {
            if (personalityTraits[i].Type == _type)
                return personalityTraits[i];
        }

        return null;
    }




    void UpdateDamageQueue(Health _health)
    {
        float healthDelta = _health.LastHealthChange;
        Transform attackerTransform = _health.LastAttacker;

        if (healthDelta == 0)
            return;


        if (healthDelta > 0)
        {
            float diff = healthDelta;

            while (diff > 0 && damageQueue.Count > 0)
            {
                diff += damageQueue[0].Item2;

                if (diff > 0)
                    damageQueue.RemoveAt(0);
            }
        }
        else
        {
            damageQueue.Add(new CustomTuple2<Transform, float>(attackerTransform, healthDelta));
        }
    }
}
