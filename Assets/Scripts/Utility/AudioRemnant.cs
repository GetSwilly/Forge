using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioRemnant : MonoBehaviour {


    private Transform followTarget;
    private AudioSource m_Audio;
    private Transform m_Transform;

 

    void Awake()
    {
        m_Audio = GetComponent<AudioSource>();
        m_Transform = GetComponent<Transform>();
    }
    void OnEnable()
    {
        m_Audio.playOnAwake = false;
    }
    void Update()
    {
        if(followTarget != null && followTarget.gameObject.activeInHierarchy)
        {
            m_Transform.position = followTarget.position;
        }
    }



    public void PlaySound(SoundClip _sound)
    {
        PlaySound(_sound, null);
    }
    public void PlaySound(SoundClip _sound, Transform _target)
    {
        followTarget = _target;

        m_Audio.volume = _sound.Volume;
        m_Audio.pitch = _sound.Pitch;

        if (_sound.IsLooping)
        {
            m_Audio.loop = true;
            m_Audio.clip = _sound.Sound;
            m_Audio.Play();
        }
        else
        {
            m_Audio.loop = false;
            m_Audio.PlayOneShot(_sound.Sound);
        }

      

        StartCoroutine(DelayedDeactivation());
    }

   
    IEnumerator DelayedDeactivation()
    {
        while (m_Audio.isPlaying)
        {
            yield return null;
        }

        gameObject.SetActive(false);
    }

}
