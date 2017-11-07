using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthModifierTurret : ForgeableObject
{

    [SerializeField]
    float rate;

    void Update()
    {
        ApplyHealthModification(Time.deltaTime);
    }

    void ApplyHealthModification(float deltaTime)
    {
        IEnumerator<GameObject> sightedObjects = m_Sight.DetectedObjects.GetEnumerator();

        while (sightedObjects.MoveNext())
        {
            GameObject obj = sightedObjects.Current;

            Team objTeam = obj.GetComponent<Team>();
            Health objHealth = obj.GetComponent<Health>();

            if (objTeam != null && objHealth != null && ShouldAffect(objTeam))
            {
                objHealth.HealthArithmetic(rate * deltaTime, false, m_Transform);
            }
        }
    }
}
