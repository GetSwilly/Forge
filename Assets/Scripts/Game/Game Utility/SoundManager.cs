using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{


    [SerializeField]
    SoundClip uiSound;

    AudioSource musicAudio;
    AudioSource soundAudio;

    AudioClip lastMusicPlayed;
    AudioClip lastSoundPlayed;

    [HideInInspector]
    public static SoundManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Debug.Log(Instance.gameObject);
            Destroy(this);
        }

        Instance = this;
        musicAudio = gameObject.AddComponent<AudioSource>();
        soundAudio = gameObject.AddComponent<AudioSource>();

        musicAudio.playOnAwake = false;
        soundAudio.playOnAwake = false;
    }

    public void PlayMusic(AudioClip newMusic)
    {
        if (newMusic == null)
            return;

        musicAudio.Stop();
        musicAudio.clip = newMusic;
        musicAudio.Play();
    }

    public void PlaySound(SoundClip sound)
    {
        if (soundAudio.isPlaying && sound.Sound == lastSoundPlayed)
            return;

        soundAudio.pitch = sound.Pitch;
        soundAudio.loop = sound.IsLooping;
        soundAudio.volume = sound.Volume;

        soundAudio.Stop();
        soundAudio.PlayOneShot(sound.Sound);

        lastSoundPlayed = sound.Sound;
    }
    public void PlaySound(AudioClip newSound)
    {
        PlaySound(newSound, 1f);
    }
    public void PlaySound(AudioClip newSound, float volumeScale)
    {
        if (newSound == null)
            return;

        soundAudio.Stop();
        soundAudio.PlayOneShot(newSound, volumeScale);
    }

    public void SetMusicVolume(float vol)
    {
        if (musicAudio != null)
            musicAudio.volume = vol;
    }
    public void SetSoundVolume(float vol)
    {
        if (soundAudio != null)
            soundAudio.volume = vol;
    }




    public SoundClip UI_Sound
    {
        get { return uiSound; }
    }
}
