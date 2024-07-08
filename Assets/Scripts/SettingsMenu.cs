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

    private void Start()
    {
        // Inicializar UI con valores actuales de GameSettings
        durationSlider.value = GameSettings.Instance.GetMatchDuration();
        soundVolumeSlider.value = GameSettings.Instance.GetSoundVolume() * 100;
        musicVolumeSlider.value = GameSettings.Instance.GetMusicVolume() * 100;

        timeText.text = GameSettings.Instance.GetMatchDuration().ToString();
        soundText.text = (GameSettings.Instance.GetSoundVolume() * 100).ToString();
        musicText.text = (GameSettings.Instance.GetMusicVolume() * 100).ToString();

        // Asignar listeners
        playButton.onClick.AddListener(() => LoadScene(2));
        exitButton.onClick.AddListener(() => LoadScene(0));
        easyButton.onClick.AddListener(() => GameSettings.Instance.SetDifficulty(GameSettings.Difficulty.Easy));
        mediumButton.onClick.AddListener(() => GameSettings.Instance.SetDifficulty(GameSettings.Difficulty.Medium));
        hardButton.onClick.AddListener(() => GameSettings.Instance.SetDifficulty(GameSettings.Difficulty.Hard));

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
