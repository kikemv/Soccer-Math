using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbienceSound : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip loopingSoundClip;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = loopingSoundClip;
        audioSource.loop = true;
    }

    private void Start()
    {
        audioSource.Play();
    }

    private void Update()
    {
        audioSource.volume = GameSettings.Instance.musicVolume;

        if (GameManager.Instance.pauseOpened && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
        else if (!GameManager.Instance.pauseOpened && !audioSource.isPlaying)
        {
            audioSource.UnPause();
        }
    }
}
