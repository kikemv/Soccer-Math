using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShootMinigame : MonoBehaviour
{
    public Image goalBackground; // Imagen de fondo de la portería

    private Animator animator;

    //cuenta atras
    public RectTransform timeBar; // Referencia al RectTransform de la barra de tiempo
    public Vector2 startPos; // Posición inicial de la barra de tiempo
    public Vector2 endPos; // Posición final de la barra de tiempo
    private float totalTime = 20f; // Tiempo total de la cuenta atrás
    private float currentTime; // Tiempo actual restante

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
        endPos = new Vector2(-timeBar.rect.width, startPos.y); // Calcula la posición final hacia la izquierda
        GenerateShootProblem();

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
                Invoke("ResumeWithGoal", 5.5f);
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
                Invoke("ResumeWithSave", 5.5f);
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
                Invoke("ResumeWithSave", 5.5f);
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
                Invoke("ResumeWithGoal", 5.5f);
            }
            
        }

    }

    private void ResumeWithGoal()
    {
        StopGame(); //pongo esto antes para que en el saque de centro estén parados
        if (localShoot)
        {
            GameManager.Instance.CenterKickAway();
            GameScore.Instance.AddLocalGoal();
        }
        else
        {
            GameManager.Instance.CenterKickLocal();
            GameScore.Instance.AddAwayGoal();
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

        // Genera respuestas incorrectas cercanas a la respuesta correcta
        int minIncorrect = correctAnswer - 3; // Valor mínimo para las respuestas incorrectas
        int maxIncorrect = correctAnswer + 3; // Valor máximo para las respuestas incorrectas
        List<int> incorrectAnswers = new List<int>(); // Lista para almacenar las respuestas incorrectas
        for (int i = minIncorrect; i <= maxIncorrect; i++)
        {
            if (i != correctAnswer) // Evita que la respuesta incorrecta coincida con la correcta
            {
                incorrectAnswers.Add(i);
            }
        }

        // Asigna las respuestas incorrectas a las opciones disponibles
        List<Image> optionImages = new List<Image> { option1, option2, option3, option4 };
        for (int i = 0; i < 4; i++)
        {
            if (i < incorrectAnswers.Count)
            {
                // Selecciona una respuesta incorrecta aleatoria
                int randomIndex = Random.Range(0, incorrectAnswers.Count);
                int incorrectAnswer = incorrectAnswers[randomIndex];
                // Asigna la respuesta incorrecta a una imagen de opción
                if (incorrectAnswer < 0) //si una opcion es negativa, que salga el sprite de otro numero
                {
                    optionImages[i].sprite = GetSpriteForNumber(correctAnswer + 5);
                }
                else
                {
                    optionImages[i].sprite = GetSpriteForNumber(incorrectAnswer); // Obtener sprite para el número
                }
                incorrectAnswers.RemoveAt(randomIndex); // Elimina la respuesta incorrecta usada
            }
            else
            {
                // Si no hay suficientes respuestas incorrectas, deja la opción vacía
                optionImages[i].sprite = null;
            }
        }
        // Busca la primera opción que aún no tenga asignada una imagen y asigna la respuesta correcta a esa opción
        foreach (Image optionImage in optionImages)
        {
            if (optionImage.sprite == null)
            {
                optionImage.sprite = GetSpriteForNumber(correctAnswer);
                break;
            }
        }

    }
    private int GetCorrectOptionIndex()
    {
        // Obtiene las imágenes de las opciones en una lista
        List<Image> optionImages = new List<Image> { option1, option2, option3, option4 };

        // Itera sobre las imágenes de las opciones para encontrar la que contiene la respuesta correcta
        for (int i = 0; i < optionImages.Count; i++)
        {
            if (optionImages[i].sprite == GetSpriteForNumber(correctAnswer))
            {
                return i; // Devuelve el índice de la opción que contiene la respuesta correcta
            }
        }

        return -1; // Si no se encuentra la respuesta correcta, devuelve -1
    }

    // Método para obtener el sprite asociado a un número
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
    }
}
