using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Button playButton;
    public Button exitButton;
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;

    public Slider durationSlider;
    public TextMeshProUGUI timeText;

    public Slider soundVolumeSlider;
    public TextMeshProUGUI soundText;
    public Slider musicVolumeSlider;
    public TextMeshProUGUI musicText;

    public Animator transition;

    public AudioClip menuClick;

    private Color selectedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    private Color defaultColor = Color.white;

    private void Start()
    {
        //inicializar con valores de GameSettings
        durationSlider.value = GameSettings.Instance.GetMatchDuration();
        soundVolumeSlider.value = GameSettings.Instance.GetSoundVolume() * 100;
        musicVolumeSlider.value = GameSettings.Instance.GetMusicVolume() * 100;

        timeText.text = GameSettings.Instance.GetMatchDuration().ToString();
        soundText.text = (GameSettings.Instance.GetSoundVolume() * 100).ToString();
        musicText.text = (GameSettings.Instance.GetMusicVolume() * 100).ToString();

        playButton.onClick.AddListener(() => { PlaySound(); LoadScene(2); });
        exitButton.onClick.AddListener(() => { PlaySound(); LoadScene(0); });
        easyButton.onClick.AddListener(() => { PlaySound(); SetDifficulty(GameSettings.Difficulty.Easy); });
        mediumButton.onClick.AddListener(() => { PlaySound(); SetDifficulty(GameSettings.Difficulty.Medium); });
        hardButton.onClick.AddListener(() => { PlaySound(); SetDifficulty(GameSettings.Difficulty.Hard); });

        durationSlider.onValueChanged.AddListener(value =>
        {
            GameSettings.Instance.SetMatchDuration(Mathf.RoundToInt(value));
            timeText.text = value.ToString();
        });

        soundVolumeSlider.onValueChanged.AddListener(value =>
        {
            GameSettings.Instance.SetSoundVolume(value);
            soundText.text = value.ToString();
        });

        musicVolumeSlider.onValueChanged.AddListener(value =>
        {
            GameSettings.Instance.SetMusicVolume(value);
            musicText.text = value.ToString();
        });

        SetDifficulty(GameSettings.Difficulty.Medium);
    }

    private void SetDifficulty(GameSettings.Difficulty difficulty)
    {
        GameSettings.Instance.SetDifficulty(difficulty);
        UpdateDifficultyButtons(difficulty);
    }

    private void UpdateDifficultyButtons(GameSettings.Difficulty selectedDifficulty)
    {
        easyButton.GetComponent<Image>().color = defaultColor;
        mediumButton.GetComponent<Image>().color = defaultColor;
        hardButton.GetComponent<Image>().color = defaultColor;

        //establecer el color del botón seleccionado
        switch (selectedDifficulty)
        {
            case GameSettings.Difficulty.Easy:
                easyButton.GetComponent<Image>().color = selectedColor;
                break;
            case GameSettings.Difficulty.Medium:
                mediumButton.GetComponent<Image>().color = selectedColor;
                break;
            case GameSettings.Difficulty.Hard:
                hardButton.GetComponent<Image>().color = selectedColor;
                break;
        }
    }

    private void PlaySound()
    {
        SoundController.Instance.PlaySound(menuClick, GameSettings.Instance.GetSoundVolume());
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
}
