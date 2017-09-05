using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EffectRemnant : MonoBehaviour {


    List<GameObject> additionalChildren = new List<GameObject>();


    private Transform followTarget;
    private AudioSource m_Audio;
    private Transform m_Transform;

    private ParticleSystem[] activeParticleSystems;


    void Awake()
    {
        m_Audio = GetComponent<AudioSource>();
        m_Transform = GetComponent<Transform>();
    }
    void OnEnable()
    {
        m_Audio.playOnAwake = false;
    }
    void OnDisable()
    {
        for(int i = 0; i < additionalChildren.Count; i++)
        {
            Destroy(additionalChildren[i]);
        }

        additionalChildren.Clear();
    }
    void Update()
    {
        if (followTarget != null && followTarget.gameObject.activeInHierarchy)
        {
            m_Transform.position = followTarget.position;
        }
    }




    public void AddChild(GameObject obj)
    {
        if (obj == null)
            return;


        obj.transform.SetParent(m_Transform);
        obj.transform.localPosition = Vector3.zero;

        additionalChildren.Add(obj);
    }

    public void PlaySound(SoundClip _sound)
    {
        PlaySound(_sound, null);
    }
    public void PlaySound(SoundClip _sound, Transform _target, params ParticleSystem[] _particleSystems)
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

        activeParticleSystems = _particleSystems;
        for (int i = 0; i < activeParticleSystems.Length; i++)
        {
            activeParticleSystems[i].Play();
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
