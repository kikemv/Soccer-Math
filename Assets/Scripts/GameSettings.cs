using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    public Difficulty difficulty = Difficulty.Easy;
    public int matchDurationInMinutes = 5;

    public float soundVolume = 100f; // Volumen de efectos de sonido
    public float musicVolume = 100f; // Volumen de la música

    public Animator transition;
    public AudioClip menuMusic;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (SoundController.Instance != null)
        {
            SoundController.Instance.PlayMusic(menuMusic, musicVolume);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reproduce la música en ajustes
        if (scene.buildIndex == 1)
        {
            if (SoundController.Instance != null)
            {
                SoundController.Instance.PlayMusic(menuMusic, musicVolume);
            }
        }
    }

    public void LoadScene(int levelIndex)
    {
        if (transition != null)
        {
            transition.SetTrigger("Start");
        }
        StartCoroutine(LoadTransition(levelIndex));
    }

    private IEnumerator LoadTransition(int levelIndex)
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(levelIndex);
    }

    public void SetDifficulty(Difficulty newDifficulty)
    {
        difficulty = newDifficulty;
    }

    public void SetMatchDuration(int minutes)
    {
        matchDurationInMinutes = minutes;
    }

    public void SetSoundVolume(float volume)
    {
        soundVolume = volume / 100;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume / 100;
    }

    public Difficulty GetDifficulty()
    {
        return difficulty;
    }

    public int GetMatchDuration()
    {
        return matchDurationInMinutes;
    }

    public float GetSoundVolume()
    {
        return soundVolume;
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }
}
