using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameScore : MonoBehaviour
{
    public static GameScore Instance;

    public TextMeshProUGUI timeText;
    public TextMeshProUGUI localGoalsText;
    public TextMeshProUGUI awayGoalsText;

    private float currentTime = 0f;
    private float totalTimeFicticio = 90f;
    private float tiempoReal;

    public int localGoals = 0;
    public int awayGoals = 0;

    public bool isPaused = true;

    //final partido
    public TextMeshProUGUI finalText;
    public GameObject finalPanel;
    public TextMeshProUGUI scoreText;

    public AudioClip pitidoFinal;

    //puntos
    public TextMeshProUGUI pointsUI;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        tiempoReal = GameSettings.Instance.matchDurationInMinutes * 60;
    }

    void Update()
    {
        if (!isPaused)
        {
            //actualizar el tiempo
            currentTime += Time.deltaTime * (totalTimeFicticio / tiempoReal);
            if (currentTime >= totalTimeFicticio)
            {
                currentTime = totalTimeFicticio;
                PauseGame();
                EndGame();
            }
            else UpdateTimeUI();
        }

        pointsUI.text = "PTS: " + GameManager.Instance.points.ToString();
    }

    void UpdateTimeUI()
    {
        int minutes = Mathf.FloorToInt(currentTime);
        int seconds = Mathf.FloorToInt((currentTime - minutes) * 60f);
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void PauseGame()
    {
        isPaused = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
    }

    public void AddLocalGoal()
    {
        localGoals++;
        localGoalsText.text = localGoals.ToString();
    }

    public void AddAwayGoal()
    {
        awayGoals++;
        awayGoalsText.text = awayGoals.ToString();
    }

    private void EndGame()
    {
        GameManager.Instance.PauseGame();
        SoundController.Instance.PlaySound(pitidoFinal, GameSettings.Instance.soundVolume - 0.2f);
        finalPanel.SetActive(true);
        string resultado = localGoals + " - " + awayGoals;
        string opSolved = GameManager.Instance.operationsSolved.ToString();

        if (localGoals > awayGoals)
        {
            GameManager.Instance.points += 300;
            finalText.text = "Congratulations. You have won " + resultado;
        }
        else if (localGoals < awayGoals)
        {
            GameManager.Instance.points += 100;
            finalText.text = "You have lost " + resultado;
        }
        else
        {
            finalText.text = "Match ended in a draw";
        }

        string points = GameManager.Instance.points.ToString();
        scoreText.text = "Correct operations: " + opSolved + "\nTotal points scored: " + points;
    }
}
