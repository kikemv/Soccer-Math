using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShootMinigame : MonoBehaviour
{
    public Image goalBackground;

    public Animator animator;

    //cuenta atras
    public RectTransform timeBar;
    public Vector2 startPos;
    public Vector2 endPos;
    private float totalTime = 20f;
    private float currentTime;

    private Team team;
    private TeamRival teamRival;

    public Image numimage1;
    public Image numimage2;
    public Image operationSprite;
    public Image option1;
    public Image option2;
    public Image option3;
    public Image option4;

    public Sprite suma;
    public Sprite resta;
    public Sprite mult;
    public Sprite div;
    public int correctAnswer;

    public Sprite[] numSpritesLocal;
    public Sprite[] numSpritesAway;

    //para saber de quien es el tiro
    public bool localShoot;

    //para llamar solo una vez a la solucion cuando se acaba el tiempo
    private bool timeEnded;

    private void Awake()
    {
        team = FindObjectOfType<Team>();
        teamRival = FindObjectOfType<TeamRival>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        currentTime = totalTime;
        startPos = new Vector2(0, -124);
        endPos = new Vector2(-timeBar.rect.width, startPos.y);
        GenerateShootProblem();

    }

    private void Update()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;

            if (currentTime > 15.0f)
            {
                //nada
            }
            else
            {
                //interpola la posición inicial y final según el tiempo restante
                timeBar.gameObject.SetActive(true);
                float normalizedTime = Mathf.Clamp01(currentTime / 15.0f);
                timeBar.anchoredPosition = Vector2.Lerp(endPos, startPos, normalizedTime);
            }
        }
        else if (!timeEnded)
        {
            timeEnded = true;
            if (localShoot)
            {
                animator.SetTrigger("ltsave");
                Invoke("ResumeWithSave", 5.5f);
            }
            else
            {
                animator.SetTrigger("ltgoal");
                Invoke("ResumeWithGoal", 5.5f);
            }
        }
    }

    public void StartShootGame()
    {
        GenerateShootProblem();
    }

    public void StopGame()
    {
        gameObject.SetActive(false);
        GameManager.Instance.shootGameActive = false;
        GameManager.Instance.ResumeGame();
    }

    public void OnAnswerSelected(int selectedOption)
    {
        SoundController.Instance.StopMusic();
        if (selectedOption == GetCorrectOptionIndex())
        {
            if (localShoot)
            {
                if (selectedOption == 0)
                {
                    animator.SetTrigger("ltgoal");
                }
                else if (selectedOption == 1)
                {
                    animator.SetTrigger("lbgoal");
                }
                else if (selectedOption == 2)
                {
                    animator.SetTrigger("rtgoal");
                }
                else if (selectedOption == 3)
                {
                    animator.SetTrigger("rbgoal");
                }
                Invoke("ResumeWithGoal", 5.55f);
            }
            else
            {
                if (selectedOption == 0)
                {
                    animator.SetTrigger("ltsave");
                }
                else if (selectedOption == 1)
                {
                    animator.SetTrigger("lbsave");
                }
                else if (selectedOption == 2)
                {
                    animator.SetTrigger("rtsave");
                }
                else if (selectedOption == 3)
                {
                    animator.SetTrigger("rbsave");
                }
                Invoke("ResumeWithSave", 5.55f);
            }
        }
        else   //si se escoge opcion incorrecta
        {
            if (localShoot)
            {
                if (selectedOption == 0)
                {
                    animator.SetTrigger("ltsave");
                }
                else if (selectedOption == 1)
                {
                    animator.SetTrigger("lbsave");
                }
                else if (selectedOption == 2)
                {
                    animator.SetTrigger("rtsave");
                }
                else if (selectedOption == 3)
                {
                    animator.SetTrigger("rbsave");
                }
                Invoke("ResumeWithSave", 5.55f);
            }
            else
            {
                if (selectedOption == 0)
                {
                    animator.SetTrigger("ltgoal");
                }
                else if (selectedOption == 1)
                {
                    animator.SetTrigger("lbgoal");
                }
                else if (selectedOption == 2)
                {
                    animator.SetTrigger("rtgoal");
                }
                else if (selectedOption == 3)
                {
                    animator.SetTrigger("rbgoal");
                }
                Invoke("ResumeWithGoal", 5.55f);
            }
            
        }

        //resetear barra
        timeBar.gameObject.SetActive(false);
        currentTime = 50f; //mas tiempo para que no se active otra vez
        timeBar.anchoredPosition = startPos;

    }

    private void ResumeWithGoal()
    {
        StopGame(); //pongo esto antes para que en el saque de centro estén parados
        if (localShoot)
        {
            GameManager.Instance.CenterKickAway();
            GameScore.Instance.AddLocalGoal();
            GameManager.Instance.points += 100;
            GameManager.Instance.operationsSolved += 1;
        }
        else
        {
            GameManager.Instance.CenterKickLocal();
            GameScore.Instance.AddAwayGoal();
            GameManager.Instance.points -= 100;
            foreach (Rival rival in teamRival.teamRivals)
            {
                rival.ResetShoot();
            }
        }
        
    }

    private void ResumeWithSave()
    {
        StopGame(); //pongo esto antes para que en el saque de centro estén parados
        Ball.Instance.isBallAttached = false; //quitarsela al jugador que tenia el balon
        if (localShoot)
        {
            Ball.Instance.StopGameForPuertaSaque(Ball.Instance.goalkeeperRival);
            team.currentPlayer[0].resetShoot();
        }
        else
        {
            Ball.Instance.StopGameForPuertaSaque(Ball.Instance.goalkeeper);
            foreach (Rival rival in teamRival.teamRivals)
            {
                rival.ResetShoot();
            }
        }
    }

    private void GenerateShootProblem()
    {
        ResetMinigame();

        int num1 = Random.Range(1, 11);
        int num2 = Random.Range(1, 11);

        numimage1.sprite = GetSpriteForNumber(num1);
        numimage2.sprite = GetSpriteForNumber(num2);

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
                operationSprite.sprite = suma;
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
                operationSprite.sprite = resta;
                break;
            case 2:
                correctAnswer = num1 * num2;
                operationSprite.sprite = mult;
                break;
            case 3:
                if (num1 % num2 == 0)
                {
                    correctAnswer = num1 / num2;
                    operationSprite.sprite = div;
                }
                else if (num2 % num1 == 0)
                {
                    //cambio los sprites de orden
                    Sprite auxSprite = numimage1.sprite;
                    numimage1.sprite = numimage2.sprite;
                    numimage2.sprite = auxSprite;
                    correctAnswer = num2 / num1;
                    operationSprite.sprite = div;
                }
                else
                {
                    correctAnswer = num1 + num2;
                    operationSprite.sprite = suma;
                }
                break;
        }

        //generar respuestas incorrectas cercanas a la respuesta correcta
        int minIncorrect = correctAnswer - 3;
        int maxIncorrect = correctAnswer + 3;
        List<int> incorrectAnswers = new List<int>();
        for (int i = minIncorrect; i <= maxIncorrect; i++)
        {
            if (i != correctAnswer) //evita que la respuesta incorrecta coincida con la correcta
            {
                incorrectAnswers.Add(i);
            }
        }

        //asignar respuestas incorrectas a opciones disponibles
        List<int> allOptions = new List<int> { correctAnswer };

        int margen = 5; //margen para cuando la opción es negativa
        for (int i = 0; i < 3; i++)
        {
            if (incorrectAnswers.Count > 0)
            {
                int randomIndex = Random.Range(0, incorrectAnswers.Count);
                int incorrectAnswer = incorrectAnswers[randomIndex];

                if (incorrectAnswer < 0) //si una opción es negativa, que salga el sprite de otro número
                {
                    allOptions.Add(correctAnswer + margen++);
                }
                else
                {
                    allOptions.Add(incorrectAnswer);
                }

                incorrectAnswers.RemoveAt(randomIndex);
            }
        }

        //mezclar las opciones
        Shuffle(allOptions);

        List<Image> optionImages = new List<Image> { option1, option2, option3, option4 };
        for (int i = 0; i < optionImages.Count; i++)
        {
            optionImages[i].sprite = GetSpriteForNumber(allOptions[i]);
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private int GetCorrectOptionIndex()
    {
        List<Image> optionImages = new List<Image> { option1, option2, option3, option4 };

        //buscar opcion correcta
        for (int i = 0; i < optionImages.Count; i++)
        {
            if (optionImages[i].sprite == GetSpriteForNumber(correctAnswer))
            {
                return i;
            }
        }

        return -1; //si no se encuentra la respuesta correcta, devuelve -1
    }

    private Sprite GetSpriteForNumber(int number)
    {
        string spriteName = number.ToString();
        if (localShoot)
        {
            foreach (Sprite sprite in numSpritesLocal)
            {
                if (sprite.name == spriteName)
                {
                    return sprite;
                }
            }
        }
        else
        {
            foreach (Sprite sprite in numSpritesAway)
            {
                if (sprite.name == spriteName)
                {
                    return sprite;
                }
            }
        }
        return null;
    }

    private void ResetMinigame()
    {
        option1.sprite = null;
        option2.sprite = null;
        option3.sprite = null;
        option4.sprite = null;
        currentTime = 20f;
        timeEnded = false;
    }
}
