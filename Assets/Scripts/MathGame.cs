using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class MathGame : MonoBehaviour
{
    public TMP_InputField playerInputField;
    public bool result;

    public Image suma;
    public Image resta;
    public Image mult;
    public Image div;
    
    public Image numimage1;
    public Sprite[] sprites1;
    public Image numimage2;
    public Sprite[] sprites2;

    //cuenta atras
    public RectTransform timeBar;
    public Vector2 startPos;
    public Vector2 endPos;
    private float totalTime = 17.5f;
    private float currentTime;

    private Animator animator;

    private int num1;
    private int num2;
    private int correctAnswer;

    //answer
    public GameObject answer;
    public Image fondo;
    public Image texto;
    public Image jug1;
    public Image jug2;
    public Sprite correctFondo;
    public Sprite correctText;
    public Sprite failFondo;
    public Sprite failText;

    private GameManager gameManager;

    public Team team;


    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        team = FindObjectOfType<Team>();
        animator = GetComponent<Animator>();
        
        suma.enabled = false;
        resta.enabled = false;
        mult.enabled = false;
        div.enabled = false;

    }

    private void Start()
    {
        currentTime = totalTime;
        startPos = new Vector2(0, -111);
        endPos = new Vector2(-timeBar.rect.width, startPos.y);
        // Calcula la posición final hacia la izquierda

    }

    private void Update()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;

            // Realiza diferentes acciones dependiendo del tiempo restante
            if (currentTime > 15.0f)
            {
                //nada
            }
            else
            {
                // Interpola entre la posición inicial y final según el tiempo restante
                timeBar.gameObject.SetActive(true);
                float normalizedTime = Mathf.Clamp01(currentTime / 15.0f);
                timeBar.anchoredPosition = Vector2.Lerp(endPos, startPos, normalizedTime);
            }
        }
        else
        {
            result = false;
            gameManager.ResumeGame();
            team.currentPlayer[0].playerMovement.ActivateParalysis();
            animator.SetTrigger("Close");
            Invoke("DeactivateMiniGame", 2f);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            CheckAnswer();
        }
    }

    public void CheckAnswer()
    {
        int playerAnswer;
        if (int.TryParse(playerInputField.text, out playerAnswer))
        {
           gameManager.ResumeGame(); //se hace resume game antes para luego congelar jugador
           if (playerAnswer == correctAnswer)
           {
                result = true;
                team.currentPlayer[0].currentRival.rivalStates.ActivateParalysis();
                
            }
           else
           {
                result = false;
                team.currentPlayer[0].playerMovement.ActivateParalysis();
                Ball.Instance.attachedPlayer = null;
                Ball.Instance.isBallAttached = false;
           }
            
            
            ResetMinigame();
            animator.SetTrigger("Close");
            timeBar.gameObject.SetActive(false);
            currentTime = 17.5f;
            Invoke("CallAnswer", 0.5f);

        }
    }

    public void CallAnswer()
    {
        answer.SetActive(true);
        if (result)
        {
            fondo.sprite = correctFondo;
            texto.sprite = correctText;
            jug1.sprite = team.currentPlayer[0].winSprite;
            jug2.sprite = team.currentPlayer[0].currentRival.loseSprite;
            GameManager.Instance.points += 100;
            GameManager.Instance.operationsSolved += 1;
        }
        else
        {
            fondo.sprite = failFondo;
            texto.sprite = failText;
            jug1.sprite = team.currentPlayer[0].currentRival.winSprite;
            jug2.sprite = team.currentPlayer[0].loseSprite;
            GameManager.Instance.points -= 50;
        }
        Invoke("DeactivateFondo", 2f);
        Invoke("DeactivateMiniGame", 3f);
    }

    private void DeactivateFondo()
    {
        gameManager.fondoCesped.SetActive(false);
    }

    private void DeactivateMiniGame()
    {
        answer.SetActive(false);
        gameObject.SetActive(false);
        gameManager.miniGameActive = false;
        gameManager.decisionActive = false;
        gameManager.miniGameInProgress = false;
        SoundController.Instance.StopMusic();
    }

    private void ResetMinigame()
    {
        suma.enabled = false;
        resta.enabled = false;
        mult.enabled = false;
        div.enabled = false;
        currentTime = 17.5f;
    }


    public void GenerateMathProblem()
    {
        ResetMinigame();
        
        num1 = GameManager.Instance.n1;
        num2 = GameManager.Instance.n2;

        string spriteName = num1.ToString();
        foreach (Sprite sprite in sprites1)
        {
            if (sprite.name == spriteName)
            {
                numimage1.sprite = sprite;
                break;
            }
        }
        string spriteName2 = num2.ToString();
        foreach (Sprite sprite in sprites2)
        {
            if (sprite.name == spriteName2)
            {
                numimage2.sprite = sprite;
                break;
            }
        }


        int operation = Random.Range(0, 4); // 0: suma, 1: resta, 2: multiplicación, 3: división
        switch (GameSettings.Instance.difficulty)
        {
            case GameSettings.Difficulty.Easy:
                operation = Random.Range(0, 2);
                break;
            case GameSettings.Difficulty.Medium:
                operation = Random.Range(0, 4);
                break;
            case GameSettings.Difficulty.Hard:
                operation = Random.Range(0, 4);
                break;
        }

        switch (operation)
        {
            case 0:
                correctAnswer = num1 + num2;
                suma.enabled = true;
                break;
            case 1:
                if (num1 >= num2)
                {
                    correctAnswer = num1 - num2;
                }
                else
                {
                    //cambio los sprites de orden
                    Sprite auxSprite = numimage1.sprite;
                    numimage1.sprite = numimage2.sprite;
                    numimage2.sprite = auxSprite;
                    correctAnswer = num2 - num1;
                }
                resta.enabled = true;
                break;
            case 2:
                correctAnswer = num1 * num2;
                mult.enabled = true;
                break;
            case 3:
                if (num1 % num2 == 0)
                {
                    correctAnswer = num1 / num2;
                    div.enabled = true;
                }
                else if(num2 % num1 == 0)
                {
                    //cambio los sprites de orden
                    Sprite auxSprite = numimage1.sprite;
                    numimage1.sprite = numimage2.sprite;
                    numimage2.sprite = auxSprite;
                    correctAnswer = num2 / num1;
                    div.enabled = true;
                }
                else
                {
                    correctAnswer = num1 + num2;
                    suma.enabled = true;
                }
                break;

        }

        playerInputField.text = ""; // Limpia el campo de entrada del jugador
        playerInputField.Select();
    }
}
