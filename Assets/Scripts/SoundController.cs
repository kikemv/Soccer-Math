using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public static SoundController Instance;

    public AudioSource soundSource;
    public AudioSource musicSource;

    private const float MinVolume = 0.1f; // Define el volumen m�nimo permitido

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        soundSource.volume = Mathf.Max(GameSettings.Instance.soundVolume, MinVolume);
        musicSource.volume = Mathf.Max(GameSettings.Instance.musicVolume, MinVolume);

        if (GameSettings.Instance.musicVolume == 0) musicSource.volume = 0;
    }

    public void PlaySound(AudioClip sound, float volume)
    {
        if (GameSettings.Instance.soundVolume != 0)
        {
            volume = Mathf.Max(volume, MinVolume); // Asegura que el volumen no sea inferior a MinVolume
            soundSource.PlayOneShot(sound, volume);
        }
    }

    public void PlayMusic(AudioClip music, float volume)
    {
        if (GameSettings.Instance.musicVolume != 0)
        {
            volume = Mathf.Max(volume, MinVolume); // Asegura que el volumen no sea inferior a MinVolume
            musicSource.clip = music;
            musicSource.loop = true;
            musicSource.volume = volume;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}

