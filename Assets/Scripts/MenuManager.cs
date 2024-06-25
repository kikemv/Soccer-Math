using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public Button playButton;
    public Sprite playSprite;
    public Sprite playSprite2;

    public Button settingsButton;
    public Sprite settingsSprite;
    public Sprite settingsSprite2;

    public Button exitButton;
    public Sprite exitSprite;
    public Sprite exitSprite2;

    public Animator transition;

    //tutorial
    public GameObject tutorialObject;
    public Button tutorialButton;
    public Sprite tutoSprite;
    public Sprite tutoSprite2;
    public GameObject[] tutorialPanels;
    public int currentPanelIndex = 0;

    public AudioClip menuMusic;

    void Start()
    {
        playButton.onClick.AddListener(() => LoadScene(1));     //cambiar luego a escena partido
        settingsButton.onClick.AddListener(() => LoadScene(1));
        exitButton.onClick.AddListener(() => CloseGame());
        setPressedButtons();

        tutorialButton.onClick.AddListener(ToggleTutorial);
        HideTutorial();

        SoundController.Instance.PlayMusic(menuMusic);
    }

    void ToggleTutorial()
    {
        bool isActive = !tutorialObject.activeSelf;
        tutorialObject.SetActive(isActive);
        if (isActive)
        {
            currentPanelIndex = 0;
            ShowPanel(currentPanelIndex);
        }
    }

    public void HideTutorial()
    {
        tutorialObject.SetActive(false);
    }

    public void ShowNextPanel()
    {
        currentPanelIndex = Mathf.Min(currentPanelIndex + 1, tutorialPanels.Length - 1);
        ShowPanel(currentPanelIndex);
    }

    public void ShowPreviousPanel()
    {
        currentPanelIndex = Mathf.Max(currentPanelIndex - 1, 0);
        ShowPanel(currentPanelIndex);
    }

    void ShowPanel(int index)
    {
        for (int i = 0; i < tutorialPanels.Length; i++)
        {
            tutorialPanels[i].SetActive(i == index);
        }
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

    void playButtonPressed()
    {
        playButton.image.sprite = playSprite2;
    }

    void playButtonNormal()
    {
        playButton.image.sprite = playSprite;
    }

    void tutoButtonPressed()
    {
        tutorialButton.image.sprite = tutoSprite2;
    }

    void tutoButtonNormal()
    {
        tutorialButton.image.sprite = tutoSprite;
    }

    void settingsButtonPressed()
    {
        settingsButton.image.sprite = settingsSprite2;
    }

    void settingsButtonNormal()
    {
        settingsButton.image.sprite = settingsSprite;
    }

    void exitButtonPressed()
    {
        exitButton.image.sprite = exitSprite2;
    }

    void exitButtonNormal()
    {
        exitButton.image.sprite = exitSprite;
    }
    void setPressedButtons()
    {
        //play button
        EventTrigger trigger = playButton.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { playButtonPressed(); });
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => { playButtonNormal(); });
        trigger.triggers.Add(entryEnter);
        trigger.triggers.Add(entryExit);

        //settings button
        EventTrigger trigger2 = settingsButton.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entryEnter2 = new EventTrigger.Entry();
        entryEnter2.eventID = EventTriggerType.PointerEnter;
        entryEnter2.callback.AddListener((data) => { settingsButtonPressed(); });
        EventTrigger.Entry entryExit2 = new EventTrigger.Entry();
        entryExit2.eventID = EventTriggerType.PointerExit;
        entryExit2.callback.AddListener((data) => { settingsButtonNormal(); });
        trigger2.triggers.Add(entryEnter2);
        trigger2.triggers.Add(entryExit2);

        //exit button
        EventTrigger trigger3 = exitButton.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entryEnter3 = new EventTrigger.Entry();
        entryEnter3.eventID = EventTriggerType.PointerEnter;
        entryEnter3.callback.AddListener((data) => { exitButtonPressed(); });
        EventTrigger.Entry entryExit3 = new EventTrigger.Entry();
        entryExit3.eventID = EventTriggerType.PointerExit;
        entryExit3.callback.AddListener((data) => { exitButtonNormal(); });
        trigger3.triggers.Add(entryEnter3);
        trigger3.triggers.Add(entryExit3);

        //tutorial button
        EventTrigger trigger4 = tutorialButton.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entryEnter4 = new EventTrigger.Entry();
        entryEnter4.eventID = EventTriggerType.PointerEnter;
        entryEnter4.callback.AddListener((data) => { tutoButtonPressed(); });
        EventTrigger.Entry entryExit4 = new EventTrigger.Entry();
        entryExit4.eventID = EventTriggerType.PointerExit;
        entryExit4.callback.AddListener((data) => { tutoButtonNormal(); });
        trigger4.triggers.Add(entryEnter4);
        trigger4.triggers.Add(entryExit4);
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}
