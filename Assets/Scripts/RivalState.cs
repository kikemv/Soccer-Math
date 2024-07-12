using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RivalStates
{
    Idle,
    ThrowIn,
    Paralyzed,
    WithBall,
    Defending,
    Attacking
}

public enum TeamState
{
    Offensive,
    Neutral,
    Defensive
}

public class RivalState : MonoBehaviour
{
    public RivalStates state;
    public TeamState teamState;

    private Rival rival;
    private TeamRival teamrival;
    private Animator animator;

    public Vector3 movimiento;

    public Goalkeeper goalkeeper;

    //zona por la que se puede move
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

    //para seguir la posicion de la pelota
    public float distanciaX;
    private Transform ballWithDistance;
    public float velocidadIA;
    public float evasionForce = 1.0f;
    public float distanciaDeDeteccion = 2.0f;

    //para pasar el balon
    public LayerMask playerLayer;
    public float detectionRadius = 5f;
    private int passProbability;
    private int randomNum;
    private bool decisionTaken = false;
    private Vector3 directionToOpponent;
    private Vector3 passDirection;

    public Transform porteria;

    //stun
    private Collider2D col;
    private SpriteRenderer spriteRenderer;
    public float paralysisDuration = 15f;
    public bool isParalyzed = false;

    private bool isPassing = false;
    public AudioClip passSound;

    private void Awake()
    {
        rival = GetComponent<Rival>();
        teamrival = GetComponentInParent<TeamRival>();
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        ballWithDistance = new GameObject().transform;
        distanciaX = 200;
        porteria = GameObject.Find("Porteria").transform;

    }

    private void Start()
    {
        teamState = TeamState.Neutral;
        state = RivalStates.Idle;

        patrolSpeed = Random.Range(3f, 6f);
        GeneratePatrolPoints();
    }

    void Update()
    {
        //actualizar el comportamiento en funci�n del estado del equipo
        switch (teamState)
        {
            case TeamState.Offensive:
                passProbability = 8;
                velocidadIA = 40;

                centroZona = rival.offensiveZone.position;
                tamanoZona = rival.offensiveZone.GetComponent<BoxCollider2D>().size;
                limiteSuperior = new Vector3(centroZona.x + tamanoZona.x / 2, 
                                             centroZona.y + tamanoZona.y / 2, 0f);
                limiteInferior = new Vector3(centroZona.x - tamanoZona.x / 2, 
                                             centroZona.y - tamanoZona.y / 2, 0f);

                UpdateOffensiveBehavior();

                if (GameScore.Instance.localGoals == GameScore.Instance.awayGoals)
                {
                    teamState = TeamState.Neutral;
                }
                break;
            case TeamState.Neutral:
                passProbability = 5;
                velocidadIA = 30;

                centroZona = rival.zone.position;
                tamanoZona = rival.zone.GetComponent<BoxCollider2D>().size;
                limiteSuperior = new Vector3(centroZona.x + tamanoZona.x / 2, 
                                             centroZona.y + tamanoZona.y / 2, 0f);
                limiteInferior = new Vector3(centroZona.x - tamanoZona.x / 2, 
                                             centroZona.y - tamanoZona.y / 2, 0f);

                UpdateNeutralBehavior();

                if (GameScore.Instance.localGoals > GameScore.Instance.awayGoals)
                {
                    teamState = TeamState.Offensive;
                }
                if (GameScore.Instance.localGoals < GameScore.Instance.awayGoals)
                {
                    teamState = TeamState.Defensive;
                }
                break;
            case TeamState.Defensive:
                passProbability = 3;
                velocidadIA = 30;

                centroZona = rival.defensiveZone.position;
                tamanoZona = rival.defensiveZone.GetComponent<BoxCollider2D>().size;
                limiteSuperior = new Vector3(centroZona.x + tamanoZona.x / 2, 
                                             centroZona.y + tamanoZona.y / 2, 0f);
                limiteInferior = new Vector3(centroZona.x - tamanoZona.x / 2, 
                                             centroZona.y - tamanoZona.y / 2, 0f);

                UpdateDefensiveBehavior();

                if (GameScore.Instance.localGoals == GameScore.Instance.awayGoals)
                {
                    teamState = TeamState.Neutral;
                }
                break;
            default:
                break;
        }
    }

    private void UpdateNeutralBehavior()
    {
        switch (state)
        {
            case RivalStates.Idle:
                animator.SetTrigger("Idle");

                break;
            case RivalStates.ThrowIn:
                animator.SetTrigger("Idle");

                if (!isPassing)    //para evitar m�ltiples llamadas al pase
                {
                    Invoke("PassAfterDelay", 2f);
                    isPassing = true;
                }

                break;
            case RivalStates.Paralyzed:
                animator.SetTrigger("Paralyzed");
                Paralysis();
                break;
            case RivalStates.WithBall:
                MoveTo(porteria);

                if (GameManager.Instance.decisionActive)
                {
                    if (!decisionTaken)
                    {
                        decisionTaken = true;
                        randomNum = Random.Range(0, 10);
                        Invoke("TakeDecision", 0.1f);
                    }
                }
                
                if (!rival.hasPossession)
                {
                    state = RivalStates.Defending;
                }

                break;
            case RivalStates.Defending:
                //solo presiona el jugador mas cercano al balon
                //los dem�s se quedan en sus posiciones de defensa
                List<Rival> nearestRivals = FindNearestRivalsToBall(1);
                if (rival == nearestRivals[0])
                {
                   MoveTo(Ball.Instance.transform);
                }
                else if (Ball.Instance.transform.position.x < limiteInferior.x || 
                         Ball.Instance.transform.position.x > limiteSuperior.x || 
                         Ball.Instance.transform.position.y < limiteInferior.y || 
                         Ball.Instance.transform.position.y > limiteSuperior.y)
                {
                    PatrolZone();
                }
                else
                {
                   MoveTo(rival.zone);
                }

                if (teamrival.teamPossession)
                    state = RivalStates.Attacking;
                
                if (rival.hasPossession)
                    state = RivalStates.WithBall;

                break;
            case RivalStates.Attacking:
                if (limiteInferior.y <= Ball.Instance.transform.position.y && 
                    Ball.Instance.transform.position.y <= limiteSuperior.y)
                {
                    if (Ball.Instance.transform.position.x > transform.position.x)
                    {
                        ballWithDistance.position = 
                            new Vector3(Ball.Instance.transform.position.x - distanciaX, 
                                        Ball.Instance.transform.position.y, 
                                        Ball.Instance.transform.position.z);
                    }
                    if (Ball.Instance.transform.position.x < transform.position.x)
                    {
                        ballWithDistance.position = 
                            new Vector3(Ball.Instance.transform.position.x + distanciaX, 
                                        Ball.Instance.transform.position.y, 
                                        Ball.Instance.transform.position.z);
                    }
                    MoveTo(ballWithDistance);
                }
                else
                {
                    PatrolZone();
                }

                if (!teamrival.teamPossession)
                    state = RivalStates.Defending;

                if (rival.hasPossession)
                    state = RivalStates.WithBall;

                break;
            default:
                break;
        }
    }

    private void UpdateOffensiveBehavior()
    {
        switch (state)
        {
            case RivalStates.Idle:
                animator.SetTrigger("Idle");

                break;
            case RivalStates.ThrowIn:
                animator.SetTrigger("Idle");

                if (!isPassing)
                {
                    Invoke("PassAfterDelay", 1f);   //saca mas rapido al ir perdiendo
                    isPassing = true;
                }

                break;
            case RivalStates.Paralyzed:

                animator.SetTrigger("Paralyzed");
                Paralysis();
                break;

            case RivalStates.WithBall:
                MoveTo(porteria);

                if (GameManager.Instance.decisionActive)
                {
                    if (!decisionTaken)
                    {
                        decisionTaken = true;
                        randomNum = Random.Range(0, 10);
                        Invoke("TakeDecision", 0.1f);
                    }
                }

                if (!rival.hasPossession)
                {
                    state = RivalStates.Defending;
                }

                break;
            case RivalStates.Defending:
                //en ofensivo dos jugadores presionan
                List<Rival> nearestRivals = FindNearestRivalsToBall(2);
                if (rival == nearestRivals[0] || rival == nearestRivals[1])
                {
                    MoveTo(Ball.Instance.transform);
                }
                else if (Ball.Instance.transform.position.x < limiteInferior.x || 
                         Ball.Instance.transform.position.x > limiteSuperior.x || 
                         Ball.Instance.transform.position.y < limiteInferior.y || 
                         Ball.Instance.transform.position.y > limiteSuperior.y)
                {
                    PatrolZone();
                }
                else
                {
                    MoveTo(rival.offensiveZone);
                }

                if (teamrival.teamPossession)
                    state = RivalStates.Attacking;

                if (rival.hasPossession)
                    state = RivalStates.WithBall;

                break;
            case RivalStates.Attacking:
                if (limiteInferior.y <= Ball.Instance.transform.position.y && 
                    Ball.Instance.transform.position.y <= limiteSuperior.y)
                {
                    if (Ball.Instance.transform.position.x > transform.position.x)
                    {
                        ballWithDistance.position = 
                            new Vector3(Ball.Instance.transform.position.x - distanciaX, 
                                        Ball.Instance.transform.position.y, 
                                        Ball.Instance.transform.position.z);
                    }
                    if (Ball.Instance.transform.position.x < transform.position.x)
                    {
                        ballWithDistance.position = 
                            new Vector3(Ball.Instance.transform.position.x + distanciaX, 
                                        Ball.Instance.transform.position.y, 
                                        Ball.Instance.transform.position.z);
                    }
                    MoveTo(ballWithDistance);
                }
                else
                {
                    PatrolZone();
                }

                if (!teamrival.teamPossession)
                    state = RivalStates.Defending;

                if (rival.hasPossession)
                    state = RivalStates.WithBall;

                break;
            default:
                break;
        }
    }

    private void UpdateDefensiveBehavior()
    {
        switch (state)
        {
            case RivalStates.Idle:
                animator.SetTrigger("Idle");

                break;
            case RivalStates.ThrowIn:
                animator.SetTrigger("Idle");

                if (!isPassing)
                {
                    Invoke("PassAfterDelay", 3f);   //como va ganando tarda mas en sacar
                    isPassing = true;
                }

                break;
            case RivalStates.Paralyzed:

                animator.SetTrigger("Paralyzed");
                Paralysis();
                break;

            case RivalStates.WithBall:
                MoveTo(porteria);
                
                if (GameManager.Instance.decisionActive)
                {
                    if (!decisionTaken)
                    {
                        decisionTaken = true;
                        randomNum = Random.Range(0, 10);
                        Invoke("TakeDecision", 0.1f);
                    }
                }

                if (!rival.hasPossession)
                {
                    state = RivalStates.Defending;
                }

                break;
            case RivalStates.Defending:
                //presiona un jugador
                List<Rival> nearestRivals = FindNearestRivalsToBall(1);
                if (rival == nearestRivals[0])
                {
                    MoveTo(Ball.Instance.transform);
                }
                else if (Ball.Instance.transform.position.x < limiteInferior.x || 
                         Ball.Instance.transform.position.x > limiteSuperior.x || 
                         Ball.Instance.transform.position.y < limiteInferior.y || 
                         Ball.Instance.transform.position.y > limiteSuperior.y)
                {
                    PatrolZone();
                }
                else
                {
                    MoveTo(rival.defensiveZone);
                }

                if (teamrival.teamPossession)
                    state = RivalStates.Attacking;

                if (rival.hasPossession)
                    state = RivalStates.WithBall;

                break;
            case RivalStates.Attacking:
                if (limiteInferior.y <= Ball.Instance.transform.position.y && 
                    Ball.Instance.transform.position.y <= limiteSuperior.y)
                {
                    if (Ball.Instance.transform.position.x > transform.position.x)
                    {
                        ballWithDistance.position = 
                            new Vector3(Ball.Instance.transform.position.x - distanciaX, 
                                        Ball.Instance.transform.position.y, 
                                        Ball.Instance.transform.position.z);
                    }
                    if (Ball.Instance.transform.position.x < transform.position.x)
                    {
                        ballWithDistance.position = 
                            new Vector3(Ball.Instance.transform.position.x + distanciaX, 
                                        Ball.Instance.transform.position.y, 
                                        Ball.Instance.transform.position.z);
                    }
                    MoveTo(ballWithDistance);
                }
                else
                {
                    PatrolZone();
                }

                if (!teamrival.teamPossession)
                    state = RivalStates.Defending;

                if (rival.hasPossession)
                    state = RivalStates.WithBall;

                break;
            default:
                break;
        }
    }

    private void MoveTo(Transform Objective)
    {
        Vector3 direccion = (Objective.position - transform.position).normalized;
        movimiento = direccion * velocidadIA * Time.deltaTime;

        bool estaEnElLimiteDerecho = transform.position.x >= limiteSuperior.x;
        bool estaEnElLimiteIzquierdo = transform.position.x <= limiteInferior.x;
        bool estaEnElLimiteSuperior = transform.position.y >= limiteSuperior.y;
        bool estaEnElLimiteInferior = transform.position.y <= limiteInferior.y;

        //detiene el movimiento en el eje correspodiente si est� en el l�mite
        if (!rival.hasPossession)
        {
            if (estaEnElLimiteDerecho && Ball.Instance.transform.position.x > transform.position.x 
                && Objective.position.x > transform.position.x)
                movimiento.x = 0f;

            if (estaEnElLimiteIzquierdo && Ball.Instance.transform.position.x < transform.position.x 
                && Objective.position.x < transform.position.x)
                movimiento.x = 0f;

            if (estaEnElLimiteSuperior && Ball.Instance.transform.position.y > transform.position.y 
                && Objective.position.y > transform.position.y)
                movimiento.y = 0f;

            if (estaEnElLimiteInferior && Ball.Instance.transform.position.y < transform.position.y 
                && Objective.position.y < transform.position.y)
                movimiento.y = 0f;
        }
       
        transform.Translate(movimiento);

        UpdateAnimatorParameters(movimiento);
    }

    private void PatrolZone()
    {
        if (teamState == TeamState.Neutral)
        {
            //verificar si hemos llegado al punto actual
            if (Vector3.Distance(transform.position, currentPatrolTarget) < 0.5f)
            {
                //seleccionar un nuevo punto de patrulla al azar
                currentPatrolPointIndex = Random.Range(0, patrolPointsNeutral.Length);
                currentPatrolTarget = patrolPointsNeutral[currentPatrolPointIndex];
            }
        }
        else if (teamState == TeamState.Offensive)
        {
            if (Vector3.Distance(transform.position, currentPatrolTarget) < 0.5f)
            {
                currentPatrolPointIndex = Random.Range(0, patrolPointsOffensive.Length);
                currentPatrolTarget = patrolPointsOffensive[currentPatrolPointIndex];
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, currentPatrolTarget) < 0.5f)
            {
                currentPatrolPointIndex = Random.Range(0, patrolPointsDefensive.Length);
                currentPatrolTarget = patrolPointsDefensive[currentPatrolPointIndex];
            }
        }
        GameObject tempTarget = new GameObject("TempPatrolTarget");
        tempTarget.transform.position = currentPatrolTarget;
        MoveTo(tempTarget.transform);
        Destroy(tempTarget);
    }


    private void GeneratePatrolPoints()
    {
        //puntos para neutral
        centroZona = rival.zone.position;
        tamanoZona = rival.zone.GetComponent<BoxCollider2D>().size;
        limiteSuperior = new Vector3(centroZona.x + tamanoZona.x / 2, 
                                     centroZona.y + tamanoZona.y / 2, 
                                     0f);
        limiteInferior = new Vector3(centroZona.x - tamanoZona.x / 2, 
                                     centroZona.y - tamanoZona.y / 2, 
                                     0f);

        patrolPointsNeutral = new Vector3[4];
        for (int i = 0; i < patrolPointsNeutral.Length; i++)
        {
            float randomX = Random.Range(limiteInferior.x, limiteSuperior.x);
            float randomY = Random.Range(limiteInferior.y, limiteSuperior.y);
            patrolPointsNeutral[i] = new Vector3(randomX, randomY, 0f);
        }

        //puntos para offensive
        centroZona = rival.offensiveZone.position;
        tamanoZona = rival.offensiveZone.GetComponent<BoxCollider2D>().size;
        limiteSuperior = new Vector3(centroZona.x + tamanoZona.x / 2, 
                                     centroZona.y + tamanoZona.y / 2, 
                                     0f);
        limiteInferior = new Vector3(centroZona.x - tamanoZona.x / 2, 
                                     centroZona.y - tamanoZona.y / 2, 
                                     0f);

        patrolPointsOffensive = new Vector3[4];
        for (int i = 0; i < patrolPointsOffensive.Length; i++)
        {
            float randomX = Random.Range(limiteInferior.x, limiteSuperior.x);
            float randomY = Random.Range(limiteInferior.y, limiteSuperior.y);
            patrolPointsOffensive[i] = new Vector3(randomX, randomY, 0f);
        }

        //puntos para defensive
        centroZona = rival.defensiveZone.position;
        tamanoZona = rival.defensiveZone.GetComponent<BoxCollider2D>().size;
        limiteSuperior = new Vector3(centroZona.x + tamanoZona.x / 2, 
                                     centroZona.y + tamanoZona.y / 2, 
                                     0f);
        limiteInferior = new Vector3(centroZona.x - tamanoZona.x / 2, 
                                     centroZona.y - tamanoZona.y / 2, 
                                     0f);

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

        movement.Normalize();

        float angulo = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;

        //ajustar el �ngulo
        if (angulo < 0f)
        {
            angulo += 360f;
        }

        //activar los triggers correspondientes seg�n el �ngulo
        if (movement.magnitude > 0f)
        {
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
        }
        else
        {
            //si no hay movimiento, desactivar la animaci�n de movimiento
            animator.SetTrigger("Idle");
        }
        spriteRenderer.color = Color.white;
    }

    private void PassAfterDelay()
    {
        GameManager.Instance.ResumeGame();
        Passing(120f, FindNearestTeammate());
        state = RivalStates.Attacking;
        isPassing = false;
    }

    public void Passing(float passForce, Rival receiver)
    {
        SoundController.Instance.PlaySound(passSound, GameSettings.Instance.soundVolume - 0.3f);

        Ball.Instance.isBallAttached = false;
        Ball.Instance.attachedRival = null;

        var direction = (receiver.transform.position - Ball.Instance.transform.position).normalized;

        Ball.Instance.rb.constraints = RigidbodyConstraints2D.None;
        Ball.Instance.transform.SetParent(null);
        Ball.Instance.rb.AddForce(direction * passForce, ForceMode2D.Impulse);
        rival.hasPossession = false;
    }

    public Rival FindNearestTeammate()
    {
        Rival nearestTeammate = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Rival teammate in teamrival.teamRivals)
        {
            if (teammate != GetComponent<Rival>())
            {
                float distanceToTeammate = Vector3.Distance(transform.position, 
                                                            teammate.transform.position);
                if (distanceToTeammate < shortestDistance)
                {
                    shortestDistance = distanceToTeammate;
                    nearestTeammate = teammate;
                }
            }
                
        }
        return nearestTeammate;
    }

    public void ActivateParalysis()
    {
        state = RivalStates.Paralyzed;
    }

    public void Paralysis()
    {
        isParalyzed = true;

        spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f); // 50% de opacidad

        col.enabled = false;

        //dar posesion al otro equipo
        if (rival.hasPossession)
        {
            Ball.Instance.attachedRival = null;
            Ball.Instance.isBallAttached = false;
            rival.hasPossession = false;
            Ball.Instance.transform.SetParent(null);
        }
        GameManager.Instance.team.teamPossession = true;
        teamrival.teamPossession = false;

        paralysisDuration -= Time.deltaTime;
        if (paralysisDuration <= 0f)
        {
            Deparalyze();
        }
    }

    public void Deparalyze()
    {
        isParalyzed = false;
        spriteRenderer.color = Color.white;
        col.enabled = true;
        paralysisDuration = 15f;
        state = RivalStates.Defending;
    }

    private List<Rival> FindNearestRivalsToBall(int numRivals)
    {
        List<Rival> nearestRivals = new List<Rival>();

        List<float> distances = new List<float>();
        List<Rival> rivals = new List<Rival>();

        //calcular las distancias de todos los rivales a la pelota
        foreach (Rival rival in teamrival.teamRivals)
        {
            float distance = Vector3.Distance(rival.transform.position, 
                                              Ball.Instance.transform.position);
            distances.Add(distance);
            rivals.Add(rival);
        }

        //ordenar las distancias y los rivales correspondientes
        for (int i = 0; i < distances.Count; i++)
        {
            for (int j = i + 1; j < distances.Count; j++)
            {
                if (distances[i] > distances[j])
                {
                    float tempDistance = distances[i];
                    distances[i] = distances[j];
                    distances[j] = tempDistance;

                    Rival tempRival = rivals[i];
                    rivals[i] = rivals[j];
                    rivals[j] = tempRival;
                }
            }
        }

        for (int i = 0; i < Mathf.Min(numRivals, rivals.Count); i++)
        {
            nearestRivals.Add(rivals[i]);
        }

        return nearestRivals;
    }

    public Rival FindNearestTeammateInDirection(Vector3 direction)
    {
        Rival nearestTeammate = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Rival teammate in teamrival.teamRivals)
        {
            if (teammate != GetComponent<Rival>())
            {
                Vector3 toTeammate = teammate.transform.position - transform.position;
                if (Vector3.Dot(toTeammate.normalized, direction) > 0)
                {
                    float distanceToTeammate = Vector3.Distance(transform.position, teammate.transform.position);
                    if (distanceToTeammate < shortestDistance)
                    {
                        shortestDistance = distanceToTeammate;
                        nearestTeammate = teammate;
                    }
                }
            }
        }
        return nearestTeammate;
    }


    private void TakeDecision()
    {
        Time.timeScale = 1f;
        
        //si no hay pase posible, se regatea
        if (FindNearestTeammateInDirection(passDirection) == null)
        {
            GameManager.Instance.StartMathRival();
        }
        else
        {
            if (randomNum <= passProbability)
            {
                Passing(140f, FindNearestTeammateInDirection(passDirection));
            }
            else
            {
                GameManager.Instance.StartMathRival();
            }
        }
       
        GameManager.Instance.decisionPanelRival.SetActive(false);
        GameManager.Instance.decisionActive = false;
        decisionTaken = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (state == RivalStates.WithBall)
        {
            if (collision.CompareTag("Jugador") && !GameManager.Instance.decisionActive && 
                !collision.gameObject.GetComponent<Player>().playerMovement.isParalyzed
                && !GameManager.Instance.miniGameActive)
            {
                Player currentPlayer = collision.gameObject.GetComponent<Player>();
                GameManager.Instance.n1 = currentPlayer.number;
                GameManager.Instance.n2 = rival.number;
                GameManager.Instance.ShowDecisionRival();
                
                //detectar lado oponente
                directionToOpponent = collision.transform.position - transform.position;
                passDirection = Vector3.zero;

                if (Mathf.Abs(directionToOpponent.x) > Mathf.Abs(directionToOpponent.y))
                { 
                    //oponente est� a la derecha o izquierda
                    passDirection = directionToOpponent.x > 0 ? Vector3.left : Vector3.right;
                }
                else
                { 
                    //oponente est� arriba o abajo
                    passDirection = directionToOpponent.y > 0 ? Vector3.down : Vector3.up;
                }
            }
        }
    }
}
