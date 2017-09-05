using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShotContainer
{

    public enum ShotType { Projectile, Projector }

    [SerializeField]
    ShotType m_ShotType = ShotType.Projectile;

    [SerializeField]
    List<WeightedObjectOfSoundClip> m_Sounds = new List<WeightedObjectOfSoundClip>();

    [SerializeField]
    float deteriorationEffect;

    [Space(5)]
    [Header("Projectile")]
    [Space(5)]

    [SerializeField]
    List<Shot> m_ProjectileShots = new List<Shot>();

    [SerializeField]
    GameObject m_ProjectilePrefab;

    [SerializeField]
    AmmoSystem m_ProjectileAmmo;




    [Space(5)]
    [Header("Projector")]
    [Space(5)]

    [SerializeField]
    Shot m_ProjectorShot;

    [SerializeField]
    GameObject m_ProjectorPrefab;
    
    [SerializeField]
    AmmoSystem m_ProjectorAmmo;



    public bool CanShoot()
    {
        switch (m_ShotType)
        {
            case ShotType.Projectile:
                return CanShootProjectile();
            case ShotType.Projector:
                return CanShootProjector();
        }

        return false;
    }



    bool CanShootProjectile()
    {
        float count = 0;

        m_ProjectileShots.ForEach(s => count += s.AmmoCost);


        if (m_ProjectileAmmo == null)
        {
            return count == 0;
        }

        return m_ProjectileAmmo.CanFire(count);
    }

    bool CanShootProjector()
    {
        if(m_ProjectorAmmo == null)
        {
            return ProjectorShot.AmmoCost == 0;
        }

        return m_ProjectorAmmo.CanFire(ProjectorShot.AmmoCost * Time.deltaTime);
    }






    public ShotType Type
    {
        get { return m_ShotType; }
    }
   
    
    public SoundClip Sound
    {
        get
        {
            return Utilities.WeightedSelection<SoundClip>(m_Sounds.ToArray(),0f);
        }
    }
    public AmmoSystem Ammo
    {
        get
        {
            switch (Type)
            {
                case ShotType.Projectile:
                    return m_ProjectileAmmo;
                case ShotType.Projector:
                    return m_ProjectorAmmo;
            }


            return null;
        }
    }

    public float DeteriorationEffect
    {
        get { return deteriorationEffect; }
    }

    public GameObject ProjectileObject
    {
        get { return m_ProjectilePrefab; }
        set { m_ProjectilePrefab = value; }
    }
    public GameObject ProjectorObject
    {
        get { return m_ProjectorPrefab; }
        set { m_ProjectorPrefab = value; }
    }


    //public AmmoSystem ProjectileAmmo
    //{
    //    get { return m_ProjectileAmmo; }
    //    set { m_ProjectileAmmo = value; }
    //}
    //public AmmoSystem ProjectorAmmo
    //{
    //    get { return m_ProjectorAmmo; }
    //    set { m_ProjectorAmmo = value; }
    //}


    public List<Shot> ProjectileShots
    {
        get { return m_ProjectileShots; }
    }
    public Shot ProjectorShot
    {
        get { return m_ProjectorShot; }
    }
}
