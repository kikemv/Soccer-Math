using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum States
{
    Idle,
    Paralyzed,
    Defending,
    Attacking
}

public class PlayerState : MonoBehaviour
{
    public States state;
    
    private Player player;
    private PlayerMovement playerMovement;
    private Team team;
    private Animator animator;
    private AudioSource audioSource;

    public TextMeshProUGUI formationText;

    //Calculo de la zona por la que se puede mover el jugador
    private Transform zone;
    private Vector3 centroZona;
    private Vector2 tamanoZona;
    private Vector3 limiteSuperior;
    private Vector3 limiteInferior;

    //patrulla
    public float patrolSpeed;
    public Vector3[] patrolPointsNeutral;
    public Vector3[] patrolPointsOffensive;
    public Vector3[] patrolPointsDefensive;
    public int currentPatrolPointIndex;
    public Vector3 currentPatrolTarget;

    //Para seguir la posicion de la pelota
    public float distanciaX;
    private Transform ballWithDistance;

    public float velocidadIA;
    public string teamState;


    private void Awake()
    {
        player = GetComponent<Player>();
        playerMovement = GetComponent<PlayerMovement>();
        team = GetComponentInParent<Team>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        ballWithDistance = new GameObject().transform;
        distanciaX = 200;

    }

    private void Start()
    {
        zone = player.zone;
        centroZona = player.zone.position;
        tamanoZona = player.zone.GetComponent<BoxCollider2D>().size;
        limiteSuperior = new Vector3(centroZona.x + tamanoZona.x / 2, centroZona.y + tamanoZona.y / 2, 0f);
        limiteInferior = new Vector3(centroZona.x - tamanoZona.x / 2, centroZona.y - tamanoZona.y / 2, 0f);

        state = States.Defending;
        patrolSpeed = Random.Range(3f, 6f);
        GeneratePatrolPoints();
    }

    void Update()
    {
        SetFormation();

        // Actualizar el comportamiento en función del estado actual
        switch (state)
        {
            case States.Idle:
                //no hacer nada
                animator.SetTrigger("Idle");
                break;
            case States.Paralyzed:
                animator.SetTrigger("Paralyzed");
                if (!player.playerMovement.isParalyzed)
                    state = States.Defending;
                break;
            case States.Defending:
                // Lógica para el estado de defender
                if (!player.user)
                {
                    PatrolZone();
                }
                    
                if (team.teamPossession)
                    state = States.Attacking;
                break;
            case States.Attacking:
                // Lógica para el estado de atacar (Attacking)
                if (!player.user)
                {
                    //si la pelota esta en la misma linea de ataque, se sigue la posicion de la pelota pero con cierta distancia
                    if (limiteInferior.y <= Ball.Instance.transform.position.y && Ball.Instance.transform.position.y <= limiteSuperior.y)
                    {
                        if (Ball.Instance.transform.position.x > transform.position.x)
                        {
                            ballWithDistance.position = new Vector3(Ball.Instance.transform.position.x - distanciaX, Ball.Instance.transform.position.y, Ball.Instance.transform.position.z);
                        }
                        if (Ball.Instance.transform.position.x < transform.position.x)
                        {
                            ballWithDistance.position = new Vector3(Ball.Instance.transform.position.x + distanciaX, Ball.Instance.transform.position.y, Ball.Instance.transform.position.z);
                        }
                        MoveTo(ballWithDistance);
                    }
                    //si no, se queda en el centro de la zona
                    else
                    {
                        PatrolZone();
                    }
                }
                  
                if (!team.teamPossession)
                    state = States.Defending;

                break;
            default:
                break;
        }
    }

    private void SetFormation()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            zone = player.zone;
            teamState = "neutral";
            ShowFormationMessage("Neutral");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            zone = player.offensiveZone;
            teamState = "offensive";
            ShowFormationMessage("Offensive");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            zone = player.defensiveZone;
            teamState = "defensive";
            ShowFormationMessage("Defensive");
        }
        centroZona = zone.position;
        tamanoZona = zone.GetComponent<BoxCollider2D>().size;
        limiteSuperior = new Vector3(centroZona.x + tamanoZona.x / 2, centroZona.y + tamanoZona.y / 2, 0f);
        limiteInferior = new Vector3(centroZona.x - tamanoZona.x / 2, centroZona.y - tamanoZona.y / 2, 0f);
    }

    private void ShowFormationMessage(string message)
    {
        if (formationText != null)
        {
            formationText.text = message;
            formationText.gameObject.SetActive(true);
            StartCoroutine(HideFormationMessageAfterDelay(2f));
        }
    }

    private IEnumerator HideFormationMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (formationText != null)
        {
            formationText.gameObject.SetActive(false);
        }
    }

    private void MoveTo(Transform Objective)
    {
        Vector3 direccion = (Objective.position - transform.position).normalized;
        Vector3 movimiento = direccion * velocidadIA * Time.deltaTime;

        // Verifica la posición del jugador en relación con los límites de la zona
        bool estaEnElLimiteDerecho = transform.position.x >= limiteSuperior.x;
        bool estaEnElLimiteIzquierdo = transform.position.x <= limiteInferior.x;
        bool estaEnElLimiteSuperior = transform.position.y >= limiteSuperior.y;
        bool estaEnElLimiteInferior = transform.position.y <= limiteInferior.y;

        // Detiene el movimiento en el eje correspodiente si está en el límite
        if (estaEnElLimiteDerecho && Ball.Instance.transform.position.x > transform.position.x && Objective.position.x > transform.position.x)
            movimiento.x = 0f;

        if (estaEnElLimiteIzquierdo && Ball.Instance.transform.position.x < transform.position.x && Objective.position.x < transform.position.x)
            movimiento.x = 0f;

        if (estaEnElLimiteSuperior && Ball.Instance.transform.position.y > transform.position.y && Objective.position.y > transform.position.y)
            movimiento.y = 0f;

        if (estaEnElLimiteInferior && Ball.Instance.transform.position.y < transform.position.y && Objective.position.y < transform.position.y)
            movimiento.y = 0f;

        transform.Translate(movimiento);

        UpdateAnimatorParameters(movimiento);
    }

    private void PatrolZone()
    {
        if (teamState == "neutral")
        {
            // Verificar si hemos llegado al punto actual
            if (Vector3.Distance(transform.position, currentPatrolTarget) < 0.5f)
            {
                // Seleccionar un nuevo punto de patrulla al azar
                currentPatrolPointIndex = Random.Range(0, patrolPointsNeutral.Length);
                currentPatrolTarget = patrolPointsNeutral[currentPatrolPointIndex];
            }
        }
        else if (teamState == "offensive")
        {
            // Verificar si hemos llegado al punto actual
            if (Vector3.Distance(transform.position, currentPatrolTarget) < 0.5f)
            {
                // Seleccionar un nuevo punto de patrulla al azar
                currentPatrolPointIndex = Random.Range(0, patrolPointsOffensive.Length);
                currentPatrolTarget = patrolPointsOffensive[currentPatrolPointIndex];
            }
        }
        else
        {
            // Verificar si hemos llegado al punto actual
            if (Vector3.Distance(transform.position, currentPatrolTarget) < 0.5f)
            {
                // Seleccionar un nuevo punto de patrulla al azar
                currentPatrolPointIndex = Random.Range(0, patrolPointsDefensive.Length);
                currentPatrolTarget = patrolPointsDefensive[currentPatrolPointIndex];
            }
        }

        // Crear un objeto temporal para usar MoveTo(Transform)
        GameObject tempTarget = new GameObject("TempPatrolTarget");
        tempTarget.transform.position = currentPatrolTarget;
        MoveTo(tempTarget.transform);
        Destroy(tempTarget);
    }


    private void GeneratePatrolPoints()
    {
        //puntos para neutral
        centroZona = player.zone.position;
        tamanoZona = player.zone.GetComponent<BoxCollider2D>().size;
        limiteSuperior = new Vector3(centroZona.x + tamanoZona.x / 2, centroZona.y + tamanoZona.y / 2, 0f);
        limiteInferior = new Vector3(centroZona.x - tamanoZona.x / 2, centroZona.y - tamanoZona.y / 2, 0f);

        patrolPointsNeutral = new Vector3[4];
        for (int i = 0; i < patrolPointsNeutral.Length; i++)
        {
            float randomX = Random.Range(limiteInferior.x, limiteSuperior.x);
            float randomY = Random.Range(limiteInferior.y, limiteSuperior.y);
            patrolPointsNeutral[i] = new Vector3(randomX, randomY, 0f);
        }

        //puntos para offensive
        centroZona = player.offensiveZone.position;
        tamanoZona = player.offensiveZone.GetComponent<BoxCollider2D>().size;
        limiteSuperior = new Vector3(centroZona.x + tamanoZona.x / 2, centroZona.y + tamanoZona.y / 2, 0f);
        limiteInferior = new Vector3(centroZona.x - tamanoZona.x / 2, centroZona.y - tamanoZona.y / 2, 0f);

        patrolPointsOffensive = new Vector3[4];
        for (int i = 0; i < patrolPointsOffensive.Length; i++)
        {
            float randomX = Random.Range(limiteInferior.x, limiteSuperior.x);
            float randomY = Random.Range(limiteInferior.y, limiteSuperior.y);
            patrolPointsOffensive[i] = new Vector3(randomX, randomY, 0f);
        }

        //puntos para defensive
        centroZona = player.defensiveZone.position;
        tamanoZona = player.defensiveZone.GetComponent<BoxCollider2D>().size;
        limiteSuperior = new Vector3(centroZona.x + tamanoZona.x / 2, centroZona.y + tamanoZona.y / 2, 0f);
        limiteInferior = new Vector3(centroZona.x - tamanoZona.x / 2, centroZona.y - tamanoZona.y / 2, 0f);

        patrolPointsDefensive = new Vector3[4];
        for (int i = 0; i < patrolPointsDefensive.Length; i++)
        {
            float randomX = Random.Range(limiteInferior.x, limiteSuperior.x);
            float randomY = Random.Range(limiteInferior.y, limiteSuperior.y);
            patrolPointsDefensive[i] = new Vector3(randomX, randomY, 0f);
        }

        //punto para empezar
        currentPatrolPointIndex = Random.Range(0, patrolPointsNeutral.Length);
        currentPatrolTarget = patrolPointsNeutral[currentPatrolPointIndex];
    }

    private void UpdateAnimatorParameters(Vector3 movement)
    {

        // Normalizar el vector de movimiento para asegurarse de que la velocidad diagonal no sea mayor
        movement.Normalize();

        // Obtener el ángulo del movimiento en radianes
        float angulo = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;

        // Ajustar el ángulo para asegurarse de que esté dentro del rango de 0 a 360 grados
        if (angulo < 0f)
        {
            angulo += 360f;
        }

        // Activar los triggers correspondientes según el ángulo de movimiento
        if (movement.magnitude > 0f)
        {

            // Determinar qué trigger activar según el ángulo
            if (angulo >= 45f && angulo < 135f)
            {
                animator.SetTrigger("MoveUp");
            }
            else if (angulo >= 135f && angulo < 225f)
            {
                animator.SetTrigger("MoveLeft");
            }
            else if (angulo >= 225f && angulo < 315f)
            {
                animator.SetTrigger("MoveDown");
            }
            else
            {
                animator.SetTrigger("MoveRight");
            }
            audioSource.volume = 0.015f;
        }
        else
        {
            // Si no hay movimiento, desactivar la animación de movimiento
            animator.SetTrigger("Idle");
            audioSource.volume = 0f;
        }
    }
}
