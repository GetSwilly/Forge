using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShotContainer
{

    [SerializeField]
    List<WeightedObjectOfSoundClip> m_Sounds = new List<WeightedObjectOfSoundClip>();

    [SerializeField]
    float chargeEffect;

    [Space(5)]
    [Header("Projectile")]
    [Space(5)]

    [SerializeField]
    List<Shot> m_ProjectileShots = new List<Shot>();

    [SerializeField]
    GameObject m_ProjectilePrefab;

    [SerializeField]
    AmmoSystem m_ProjectileAmmo;



    public bool CanShoot()
    {
        float count = 0;

        m_ProjectileShots.ForEach(s => count += s.AmmoCost);


        if (m_ProjectileAmmo == null)
        {
            return count == 0;
        }

        return m_ProjectileAmmo.CanFire(count);
    }




    public SoundClip Sound
    {
        get
        {
            return Utilities.WeightedSelection<SoundClip>(m_Sounds.ToArray(), 0f);
        }
    }
    public AmmoSystem Ammo
    {
        get { return m_ProjectileAmmo; }
    }

    public float ChargeEffect
    {
        get { return chargeEffect; }
    }

    public GameObject ProjectileObject
    {
        get { return m_ProjectilePrefab; }
        set { m_ProjectilePrefab = value; }
    }
   

    public List<Shot> ProjectileShots
    {
        get { return m_ProjectileShots; }
    }

}
