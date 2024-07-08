using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseSettings : MonoBehaviour
{
    public Slider soundVolumeSlider;
    public TextMeshProUGUI soundText;
    public Slider musicVolumeSlider;
    public TextMeshProUGUI musicText;

    public Button exitButton;

    public Toggle pointsToggle;
    public Toggle camToggle;
    public TextMeshProUGUI camText;

    void Start()
    {
        // Inicializar UI con valores actuales de GameSettings
        soundVolumeSlider.value = GameSettings.Instance.GetSoundVolume() * 100;
        musicVolumeSlider.value = GameSettings.Instance.GetMusicVolume() * 100;

        soundText.text = (GameSettings.Instance.GetSoundVolume() * 100).ToString();
        musicText.text = (GameSettings.Instance.GetMusicVolume() * 100).ToString();

        exitButton.onClick.AddListener(() => gameObject.SetActive(false));

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

        //textos ui
        GameScore.Instance.pointsUI.gameObject.SetActive(pointsToggle.isOn);
        pointsToggle.onValueChanged.AddListener(OnPointsToggleChanged);
        camText.gameObject.SetActive(camToggle.isOn);
        camToggle.onValueChanged.AddListener(OnCamToggleChanged);
    }

    private void OnPointsToggleChanged(bool isOn)
    {
        GameScore.Instance.pointsUI.gameObject.SetActive(isOn);
    }
    private void OnCamToggleChanged(bool isOn)
    {
        camText.gameObject.SetActive(isOn);
    }
}
