using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour
{
    public void PlaySoundFromAnimation(AudioClip clip)
    {
        SoundController.Instance.PlaySound(clip, GameSettings.Instance.soundVolume);
    }

    public void PlayMusicFromAnimation(AudioClip clip)
    {
        SoundController.Instance.PlaySound(clip, GameSettings.Instance.musicVolume);
    }
}
