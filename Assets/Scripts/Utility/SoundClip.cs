using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SoundClip {

    [SerializeField]
    AudioClip sound;

    [SerializeField]
    [Range(0f,1f)]
    float volume;

    [SerializeField]
    [Range(-3f,3f)]
    float pitch;

    [SerializeField]
    bool isLooping;

    [SerializeField]
    bool useRemnant;

    public SoundClip(AudioClip _clip, float _vol, float _pitch, bool _looping, bool _remnant)
    {
        sound = _clip;
        volume = _vol;
        pitch = _pitch;
        isLooping = _looping;
        useRemnant = _remnant;
    }



    public void Play(AudioSource _source)
    {
        if (_source == null)
            return;


        _source.volume = Volume;
        _source.pitch = Pitch;

        if (IsLooping)
        {
            _source.loop = true;
            _source.clip = Sound;
            _source.Play();
        }
        else
        {
            _source.loop = false;
            _source.PlayOneShot(Sound);
        }
    }

    public AudioClip Sound
    {
        get { return sound; }
    }
    public float Volume
    {
        get { return volume; }
    }
    public float Pitch
    {
        get { return pitch; }
    }
    public bool IsLooping
    {
        get { return isLooping; }
    }
    public bool UseRemnant
    {
        get { return useRemnant; }
    }

    public override string ToString()
    {
        return string.Format("AudioClip: {0}. Volume: {1}. Pitch: {2}.", Sound, Volume, Pitch);
    }
}
