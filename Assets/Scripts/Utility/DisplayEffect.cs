using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DisplayEffect {

    [SerializeField]
    ParticleSystem m_ParticleSystem;

    [SerializeField]
    AudioSource m_Audio;

    [SerializeField]
    SoundClip m_Sound;
    

    public void Play()
    {

        if(m_ParticleSystem != null)
        {
            m_ParticleSystem.Play(true);
        }


        m_Sound.Play(m_Audio);
    }
}

