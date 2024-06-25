using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public static Ball Instance;

    [HideInInspector] public Rigidbody2D rb;
    public Transform center;

    public bool isBallAttached = false;

    private Animator animator;
    public Player attachedPlayer;
    public Rival attachedRival;
    private SpriteRenderer sprite;

    private Team team;
    private TeamRival teamRival;

   //saques
    private Vector3 lastExitPosition;
    private float bordeizq = -541;
    private float bordeder = 523;
    private float bordesup = 1309;
    public Goalkeeper goalkeeper;
    public Goalkeeper goalkeeperRival;
    private float passForce = 100;

    //ultimos jugadores
    public Player lastPlayer;
    public Rival lastRival;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Asegura que solo haya una instancia de la pelota
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        team = FindObjectOfType<Team>();
        teamRival = FindObjectOfType<TeamRival>();
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        ResetPosition();
    }

    //para poner la pelota en el medio
    public void ResetPosition()
    {
        transform.position = center.position;
    }

    private void Update()
    {
        SetAnimation();

        if (!isBallAttached)
        {
            attachedPlayer = null;
            attachedRival = null;
        }
        else
        {
            if (attachedPlayer != null)
            {
                lastPlayer = attachedPlayer;
            }
            if (attachedRival != null)
            {
                lastRival = attachedRival;
            }
        }
    }

    void SetAnimation()
    {
        if (isBallAttached)
        {
            if(attachedPlayer != null)
            {
                Vector2 direccionMovimientoJugador = attachedPlayer.playerMovement.movement;
                // Calcula el ángulo entre la dirección de movimiento del jugador y el eje X
                float angulo = Vector2.SignedAngle(Vector2.right, direccionMovimientoJugador);

                if (angulo < 0f)
                {
                    angulo += 360f;
                }

                // Determina qué animación reproducir basándose en el ángulo
                if (direccionMovimientoJugador.magnitude > 0.1f) // Asegúrate de que haya movimiento significativo
                {
                    if (angulo >= 45f && angulo < 135f)
                    {
                        animator.SetTrigger("Up");
                        sprite.sortingOrder = 1;
                        attachedPlayer.ballPosition.transform.localPosition = new Vector3(15, 16, 0);
                    }
                    else if (angulo >= 135f && angulo < 225f)
                    {
                        animator.SetTrigger("Left");
                        sprite.sortingOrder = 5;
                        attachedPlayer.ballPosition.transform.localPosition = new Vector3(-25, 6, 0);
                    }
                    else if (angulo >= 225f && angulo < 315f)
                    {
                        animator.SetTrigger("Down");
                        sprite.sortingOrder = 5;
                        attachedPlayer.ballPosition.transform.localPosition = new Vector3(12, -1, 0);
                    }
                    else
                    {
                        animator.SetTrigger("Right");
                        sprite.sortingOrder = 5;
                        attachedPlayer.ballPosition.transform.localPosition = new Vector3(25, 6, 0);
                    }
                }
                else
                {
                    // Si no hay movimiento, detén la animación de caminar
                    animator.SetTrigger("Idle");
                    sprite.sortingOrder = 5;
                    attachedPlayer.ballPosition.transform.localPosition = new Vector3(12, -1, 0);
                }
            }
            else
            {
                Vector2 direccionMovimientoJugador = attachedRival.rivalStates.movimiento;
                // Calcula el ángulo entre la dirección de movimiento del jugador y el eje X
                float angulo = Vector2.SignedAngle(Vector2.right, direccionMovimientoJugador);

                if (angulo < 0f)
                {
                    angulo += 360f;
                }

                // Determina qué animación reproducir basándose en el ángulo
                if (direccionMovimientoJugador.magnitude > 0.1f) // Asegúrate de que haya movimiento significativo
                {
                    if (angulo >= 45f && angulo < 135f)
                    {
                        animator.SetTrigger("Up");
                        sprite.sortingOrder = 1;
                        attachedRival.ballPosition.transform.localPosition = new Vector3(15, 16, 0);
                    }
                    else if (angulo >= 135f && angulo < 225f)
                    {
                        animator.SetTrigger("Left");
                        sprite.sortingOrder = 5;
                        attachedRival.ballPosition.transform.localPosition = new Vector3(-25, 6, 0);
                    }
                    else if (angulo >= 225f && angulo < 315f)
                    {
                        animator.SetTrigger("Down");
                        sprite.sortingOrder = 5;
                        attachedRival.ballPosition.transform.localPosition = new Vector3(12, -1, 0);
                    }
                    else
                    {
                        animator.SetTrigger("Right");
                        sprite.sortingOrder = 5;
                        attachedRival.ballPosition.transform.localPosition = new Vector3(25, 6, 0);
                    }
                }
                else
                {
                    // Si no hay movimiento, detén la animación de caminar
                    animator.SetTrigger("Idle");
                    sprite.sortingOrder = 5;
                    attachedRival.ballPosition.transform.localPosition = new Vector3(12, -1, 0);
                }
            }
        }
        else
        {
            // Si la pelota no está adjunta o no tiene jugador adjunto, utiliza el comportamiento normal
            Vector3 direccionMovimiento = rb.velocity.normalized;
            // Calcula el ángulo entre la dirección de movimiento y el eje X
            float angulo = Vector3.Angle(Vector3.right, direccionMovimiento);

            if (angulo < 0f)
            {
                angulo += 360f;
            }

            // Determina qué animación reproducir basándose en el ángulo
            if (direccionMovimiento.magnitude > 0.1f) // Asegúrate de que haya movimiento significativo
            {
                if (angulo >= 45f && angulo < 135f)
                {
                    animator.SetTrigger("Up");
                }
                else if (angulo >= 135f && angulo < 225f)
                {
                    animator.SetTrigger("Left");
                }
                else if (angulo >= 225f && angulo < 315f)
                {
                    animator.SetTrigger("Down");
                }
                else
                {
                    animator.SetTrigger("Right");
                }
            }
            else
            {
                // Si no hay movimiento, detén la animación de caminar
                animator.SetTrigger("Idle");
            }
        }
    }

    // Método para detectar cuando la pelota sale del campo
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Field")) 
        {
            // Guarda la posición de salida de la pelota
            lastExitPosition = transform.position;

            if (lastExitPosition.x < bordeizq || lastExitPosition.x > bordeder) //si sale por los lados, saque de banda
            {
                StartCoroutine(StopGameForBandaSaque());
            }
            else  //si no, saque de puerta
            {
                Invoke("CallSaquePuerta", 0.5f);
            }
            
        }
    }

    private void CallSaquePuerta()
    {
        if (lastExitPosition.y > bordesup)  //si sale por arriba, saca rival
        {
            StopGameForPuertaSaque(goalkeeperRival);
        }
        else
        {
            StopGameForPuertaSaque(goalkeeper);
        }
    }

    public void StopGameForPuertaSaque(Goalkeeper gk)
    {
        if (gk == goalkeeperRival)
        {
            transform.position = goalkeeperRival.transform.position;

            //posesion para rival
            teamRival.teamPossession = true;
            team.teamPossession = false;
            team.currentPlayer[0].hasPossession = false;
            attachedPlayer = null;

            GameManager.Instance.ResetPositions();

            List<Rival> receiver = FindNearestRivals(1);
            var direction = (receiver[0].transform.position - transform.position).normalized;
            rb.constraints = RigidbodyConstraints2D.None;
            transform.SetParent(null);
            rb.AddForce(direction * passForce, ForceMode2D.Impulse);
        }

        else
        {
            transform.position = goalkeeper.transform.position;

            //posesion para team
            teamRival.teamPossession = false;
            team.teamPossession = true;
            teamRival.currentRival[0].hasPossession = false;
            attachedRival = null;

            GameManager.Instance.ResetPositions();

            List<Player> receiver = FindNearestPlayers(1);
            var direction = (receiver[0].transform.position - transform.position).normalized;
            rb.constraints = RigidbodyConstraints2D.None;
            transform.SetParent(null);
            rb.AddForce(direction * passForce, ForceMode2D.Impulse);
        }

    }

    private IEnumerator StopGameForBandaSaque()
    {
        yield return new WaitForSecondsRealtime(0.5f);  // Espera medio segundo

        // Sitúa la pelota en la posición de salida
        transform.position = lastExitPosition;

        GameManager.Instance.ResetPositions();

        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        //si la tira fuera el equipo rival, saca el jugador
        if (!team.teamPossession)
        {
            teamRival.teamPossession = false;
            team.teamPossession = true;
            List<Player> nearPlayers = FindNearestPlayers(3);
            nearPlayers[0].transform.position = lastExitPosition;
            //poner receptores si hace falta
        }
        else       //si la tira fuera el jugador, saca el equipo rival
        {
            teamRival.teamPossession = true;
            team.teamPossession = false;
            List<Rival> nearRivals = FindNearestRivals(3);
            nearRivals[0].transform.position = lastExitPosition;
            nearRivals[0].rivalStates.state = RivalStates.ThrowIn;
            if (lastExitPosition.x >= center.position.x)
            {
                nearRivals[1].transform.position = new Vector3(lastExitPosition.x - 100, lastExitPosition.y - 100, lastExitPosition.z);
                nearRivals[2].transform.position = new Vector3(lastExitPosition.x - 100, lastExitPosition.y + 100, lastExitPosition.z);
            }
            else
            {
                nearRivals[1].transform.position = new Vector3(lastExitPosition.x + 100, lastExitPosition.y - 100, lastExitPosition.z);
                nearRivals[2].transform.position = new Vector3(lastExitPosition.x + 100, lastExitPosition.y + 100, lastExitPosition.z);
            }
        }

    }

    private List<Player> FindNearestPlayers(int count)
    {
        List<Player> nearestPlayers = team.teamPlayers
            .OrderBy(p => Vector2.Distance(transform.position, p.transform.position))
            .Take(count)
            .ToList();

        return nearestPlayers;
    }

    private List<Rival> FindNearestRivals(int count)
    {
        List<Rival> nearestRivals = teamRival.teamRivals
            .OrderBy(r => Vector2.Distance(transform.position, r.transform.position))
            .Take(count)
            .ToList();

        return nearestRivals;
    }
}
