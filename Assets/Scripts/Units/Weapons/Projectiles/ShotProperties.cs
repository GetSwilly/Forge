using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShotProperties {

    public enum SoundType { Single, Multiple }

    [SerializeField]
    SoundType m_SoundType = SoundType.Multiple;

    [SerializeField]
    List<WeightedObjectOfSoundClip> m_Sounds = new List<WeightedObjectOfSoundClip>();



    [SerializeField]
    List<Shot> m_Shots = new List<Shot>();

    [SerializeField]
    GameObject m_Prefab;

    [SerializeField]
    AmmoSystem m_Ammo;


    public bool CanShoot()
    {
        float count = 0;

        for (int i = 0; i < m_Shots.Count; i++)
        {
            count += m_Shots[i].AmmoCost;
        }
        return m_Ammo.CanFire(count);
    }


    public GameObject Projectile
    {
        get { return m_Prefab; }
        set { m_Prefab = value; }
    }
    public SoundType MySoundType
    {
        get { return m_SoundType; }
    }
    public AmmoSystem Ammo
    {
        get { return m_Ammo; }
    }
    public SoundClip Sound
    {
        get
        {
            return Utilities.WeightedSelection<SoundClip>(m_Sounds.ToArray(),0f);
        }
    }
    public List<Shot> Shots
    {
        get { return m_Shots; }
    }
}
