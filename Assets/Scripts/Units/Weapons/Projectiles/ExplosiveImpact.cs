using UnityEngine;
using System.Collections;

[RequireComponent(typeof(IProjectile))]
public class ExplosiveImpact : MonoBehaviour {

    IProjectile myProjectile;

    void Awake()
    {
        myProjectile = GetComponent<IProjectile>();
        myProjectile.SubscribeToOnImpact(Explode);
    }


    void Explode(Health casualtyHealth)
    {

    }
}
