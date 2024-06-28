using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public Team team;
    public TeamRival teamRival;

    //puntos
    public int points;
    public int operationsSolved;

    public bool gamePaused = false;
    public bool miniGameActive = false;
    public bool miniGameInProgress = false;  //para que no salte varias veces el decisionUI

    //minigame
    public MathGame mathGame;
    public GameObject decisionPanel;
    public bool decisionActive = false;
    public int n1;
    public int n2;

    public GameObject decisionPanelRival;

    //shootgame
    public ShootMinigame shootGame;
    public bool shootGameActive = false;

    //animaciones
    public Animator mathAnimator;
    public Animator decisionAnimator;
    public Animator versusAnimator;

    //versus
    public GameObject versus;
    public Image num1;
    public Image num2;
    public Image jug1;
    public Image jug2;
    public Sprite[] spritesnum1;
    public Sprite[] spritesnum2;
    public GameObject fondoCesped;

    //pause
    public bool pauseOpened;
    public GameObject pausePanel;

    public bool serveTeam = true;   //para ver quien saca
    public Transform center;

    public GameSettings gameSettings;

    //sonidos
    public AudioClip referee;
    public AudioClip minigameMusic;
    public AudioClip decisionSound;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        gameSettings = GameSettings.Instance;
        
        center = GameObject.FindGameObjectWithTag("Field").transform;
        team = FindObjectOfType<Team>();
        teamRival = FindObjectOfType<TeamRival>();

        points = 0;
        operationsSolved = 0;
    }

    private void Start()
    {
        CenterKickLocal();
    }

    private void Update()
    {
        if (decisionActive)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Time.timeScale = 1f;
                decisionAnimator.SetTrigger("Rselect");
                PauseGame();
                Invoke("StartVersus", 2f);
            }
        }

        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (!pauseOpened)
            {
                Time.timeScale = 0f;
                pauseOpened = true;
                pausePanel.SetActive(true);
            }
            else
            {
                ClosePause();
            }
        }
        
    }

    public void StartMathRival()
    {
        Time.timeScale = 1f;
        PauseGame();
        decisionPanelRival.SetActive(false);
        StartVersus();
    }

    public void StartVersus()
    {
        HideDecisionUI();
        miniGameInProgress = true;
        GameScore.Instance.gameObject.SetActive(false);
        versus.gameObject.SetActive(true);
        num1.sprite = spritesnum1[n1];
        num2.sprite = spritesnum2[n2];
        jug1.sprite = team.currentPlayer[0].GetComponent<SpriteRenderer>().sprite;
        jug2.sprite = team.currentPlayer[0].currentRival.GetComponent<SpriteRenderer>().sprite;
        fondoCesped.SetActive(true);
        Invoke("CallMathGame", 3f);
    }

    public void CallMathGame()
    {
        versus.gameObject.SetActive(false);
        StartMathGame();
    }

    public void StartMathGame()
    {
        if (!miniGameActive)
        {
            mathGame.gameObject.SetActive(true);
            mathAnimator.SetTrigger("Open");
            miniGameActive = true;
            mathGame.GenerateMathProblem();
        }
    }

    public void StartShootGame()
    {
        if (!shootGameActive)
        {
            PauseGame();
            shootGame.gameObject.SetActive(true);
            shootGameActive = true;
            if (team.teamPossession)
            {
                shootGame.localShoot = true;
            }
            else
            {
                shootGame.localShoot = false;
            }
            shootGame.StartShootGame();
        }
    }

    public void CenterKickLocal()
    {
        PauseGame();

        Player jugadorActual = null;

        foreach (Player player in team.teamPlayers)
        {
            player.ResetPosition();
            if (player.sacador) jugadorActual = player;
        }

        foreach (Rival rival in teamRival.teamRivals)
        {
            rival.ResetPosition();
            rival.hasPossession = false;
        }

        // Colocar al jugador que est� siendo manejado en el centro del campo junto al bal�n
        Debug.Log(jugadorActual);
        jugadorActual.transform.position = center.position;
        jugadorActual.UserBrain();
        team.currentPlayer[1].AiBrain();
        jugadorActual.playerMovement.isPaused = true;
        jugadorActual.GainPossession();

        // Congelar el movimiento del jugador que est� siendo manejado
        jugadorActual.playerMovement.StopMovement();

        Invoke("RefereeSound", 1f);
    }

    public void CenterKickAway()
    {
        PauseGame();

        Rival jugadorActual = null;

        foreach (Player player in team.teamPlayers)
        {
            player.ResetPosition();
        }

        foreach (Rival rival in teamRival.teamRivals)
        {
            rival.ResetPosition();
            if (rival.sacador) jugadorActual = rival;
        }

        // Colocar al jugador que est� siendo manejado en el centro del campo junto al bal�n y darle la posesion
        Debug.Log(jugadorActual);
        jugadorActual.transform.position = center.position;
        jugadorActual.GainPossession();
        
        //desparalizar si hay alguien paralizado
        foreach (Rival rival in teamRival.teamRivals)
        {
            if (rival.rivalStates.isParalyzed) rival.rivalStates.Deparalyze();
        }

        Invoke("RefereeSound", 1f);
    }

    public void PauseGame()
    {
        Time.timeScale = 1f;
        foreach (Player player in team.teamPlayers)
        {
            player.playerMovement.StopMovement();
            player.playerMovement.isPaused = true;
            player.playerState.state = States.Idle;
        }
        foreach (Rival rival in teamRival.teamRivals)
        {
            rival.rivalStates.state = RivalStates.Idle;

        }
        gamePaused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        gamePaused = false;
        GameScore.Instance.isPaused = false;
        foreach (Player player in team.teamPlayers)
        {
            player.playerMovement.isPaused = false;
            player.playerState.state = States.Defending;
        }
        foreach (Rival rival in teamRival.teamRivals)
        {
            if(!rival.rivalStates.isParalyzed) rival.rivalStates.state = RivalStates.Defending;
        }
        GameScore.Instance.gameObject.SetActive(true);
    }

    public void ShowDecisionUI()
    {
        if (!miniGameInProgress)
        {
            SoundController.Instance.PlaySound(decisionSound, gameSettings.soundVolume);
            decisionPanel.SetActive(true);
            decisionActive = true;
            Time.timeScale = 0.05f;
        }
    }

    public void ShowDecisionRival()
    {
        if (!miniGameInProgress)
        {
            decisionPanelRival.SetActive(true);
            decisionActive = true;
            Time.timeScale = 0.05f;
        }
    }

    public void HideDecisionUI()
    {
        decisionPanel.SetActive(false);
        decisionActive = false;
        Time.timeScale = 1f;
    }

    public void ResetPositions()
    {
        foreach (Player player in team.teamPlayers)
        {
            player.InGamePosition();
        }
        foreach (Rival rival in teamRival.teamRivals)
        {
            rival.InGamePosition();
        }
    }

    //botones pausa
    public void ClosePause()
    {
        Time.timeScale = 1f;
        pauseOpened = false;
        pausePanel.SetActive(false);
    }

    public void RestartMatch()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        CenterKickLocal();
    }

    public void OpenSettingsMenu()
    {
        //SceneManager.LoadScene("Prematch");
        //ajustes de sonido
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    //sonidos
    public void RefereeSound()
    {
        SoundController.Instance.PlaySound(referee, 0.3f);
    }

    public void MinigameMusic()
    {
        SoundController.Instance.PlayMusic(minigameMusic);
    }
}
