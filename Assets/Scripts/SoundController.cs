using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public static SoundController Instance;

    public AudioSource soundSource;
    public AudioSource musicSource;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        soundSource.volume = GameSettings.Instance.soundVolume;
        musicSource.volume = GameSettings.Instance.musicVolume;
    }

    public void PlaySound(AudioClip sound, float volume)
    {
        soundSource.PlayOneShot(sound, volume);
    }

    public void PlayMusic(AudioClip music)
    {
        musicSource.clip = music;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}
