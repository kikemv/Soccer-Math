using System.Collections;
using System.Collections.Generic;
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

    //botones
    public Button playButton;
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;

    public Slider durationSlider;
    public TextMeshProUGUI timeText;


    public float soundVolume = 1.0f; // Volumen de efectos de sonido
    public float musicVolume = 1.0f; // Volumen de la música
    public Slider soundVolumeSlider; // Nuevo slider para ajustar el volumen de efectos de sonido
    public TextMeshProUGUI soundText;
    public Slider musicVolumeSlider; // Nuevo slider para ajustar el volumen de la música
    public TextMeshProUGUI musicText;

    public Animator transition;
    public AudioClip menuMusic;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        playButton.onClick.AddListener(() => LoadScene(2)); //cambiar luego al menu principal
        easyButton.onClick.AddListener(EasyPressed);
        mediumButton.onClick.AddListener(MediumPressed);
        hardButton.onClick.AddListener(HardPressed);

        durationSlider.onValueChanged.AddListener(UpdateMatchDuration);
        soundVolumeSlider.onValueChanged.AddListener(UpdateSoundVolume); // Agrega listener para ajustar el volumen de efectos de sonido
        musicVolumeSlider.onValueChanged.AddListener(UpdateMusicVolume); // Agrega listener para ajustar el volumen de la música

        SoundController.Instance.PlayMusic(menuMusic);
    }

    void LoadScene(int levelIndex)
    {
        transition.SetTrigger("Start");
        StartCoroutine(LoadTransition(levelIndex));
    }

    IEnumerator LoadTransition(int levelIndex)
    {

        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(levelIndex);
    }

    void EasyPressed()
    {
        difficulty = Difficulty.Easy;
    }

    void MediumPressed()
    {
        difficulty = Difficulty.Medium;
    }

    void HardPressed()
    {
        difficulty = Difficulty.Hard;
    }

    void UpdateMatchDuration(float value)
    {
        matchDurationInMinutes = Mathf.RoundToInt(value);
        timeText.text = value.ToString();
    }

    void UpdateSoundVolume(float value)
    {
        soundVolume = value/100;
        soundText.text = value.ToString();
    }

    void UpdateMusicVolume(float value)
    {
        musicVolume = value/100;
        musicText.text = value.ToString();
    }
}
