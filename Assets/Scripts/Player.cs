using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerRole
{
    Goalkeeper,
    Defense,
    Mid,
    Forward
}
public class Player : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public PlayerState playerState;

    public PlayerRole playerRole;
    public bool sacador = false;

    public Transform ballPosition;
    public bool user;
    public bool hasPossession;

    //sprites para animaciones
    public Sprite winSprite;
    public Sprite loseSprite;

    //posibles pases
    public Player leftBotPlayer;
    public Player rightBotPlayer;
    public Player leftTopPlayer;
    public Player rightTopPlayer;

    private Team team;
    private TeamRival teamRival;

    //indicador
    private GameObject indicador;
    public Sprite ind1;
    public Sprite ind2;
    public SpriteRenderer spriteIndicador;

    //zonas
    public Vector2 startPosition;
    public Transform zone;
    public Transform offensiveZone;
    public Transform defensiveZone;
    public float offset;

    public Rival currentRival;

    //numero
    public int number;
    private SpriteRenderer numberSprite;
    public Sprite[] numbersSpirtesheet;

    //barra tiro
    public Slider shootingSlider;
    public float shootingChargeRate = 0.1f;
    public float maxShootingCharge = 1.0f;
    public float shootingCharge = 0.0f;
    public static float globalShootingCharge;


    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerState = GetComponent<PlayerState>();
        team = GetComponentInParent<Team>();
        indicador = transform.GetChild(0).gameObject;
        spriteIndicador = indicador.GetComponent<SpriteRenderer>();
        numberSprite = transform.Find("number").gameObject.GetComponent<SpriteRenderer>();
        ballPosition = transform.Find("BallPossition");
        teamRival = FindObjectOfType<TeamRival>(); ;

        shootingChargeRate = 0.3f;
    }

    private void Start()
    {
        if (sacador) offset += 150;

        if (playerRole == PlayerRole.Forward)
        {
            startPosition = new Vector3(zone.position.x, 
                                        zone.position.y - offset, 
                                        zone.position.z);
        }
        else if (playerRole == PlayerRole.Mid)
        {
            startPosition = new Vector3(zone.position.x, 
                                        zone.position.y - offset / 2, 
                                        zone.position.z);
        }
        else startPosition = new Vector3(zone.position.x, 
                                         zone.position.y - offset / 6, 
                                         zone.position.z);

        playerMovement.barraEstamina.gameObject.SetActive(false);

        // Desactivar la Slider al principio
        shootingSlider.gameObject.SetActive(false);
    }

    private void Update()
    {

        shootingCharge = globalShootingCharge;
        if (user)
        {
            PosiblesPases();

            // Actualizar Slider si el jugador está en la zona de disparo y tiene posesión
            if (transform.position.y > 650 && hasPossession)
            {
                shootingSlider.gameObject.SetActive(true);
                if (shootingCharge < maxShootingCharge)
                {
                    spriteIndicador.sprite = ind1;
                    shootingCharge += shootingChargeRate * Time.deltaTime;
                    shootingSlider.value = shootingCharge / maxShootingCharge;
                }
                else
                {
                    // Carga completa, cambiar sprite del indicador y permitir el disparo
                    spriteIndicador.sprite = ind2;
                    shootingSlider.value = 1;
                    // tiro
                    if (Input.GetKeyDown(KeyCode.T) && !GameManager.Instance.miniGameActive 
                        && !GameManager.Instance.decisionActive)   
                    {
                        playerMovement.StopMovement();
                        GameManager.Instance.StartShootGame();
                    }
                }
            }
            else if (transform.position.y < 650)
            {
                resetShoot();
            }
            globalShootingCharge = shootingCharge;
        }
        else
        {
            shootingSlider.gameObject.SetActive(false);
        }

    }

    public void resetShoot()
    {
        spriteIndicador.sprite = ind1;
        shootingCharge = 0.0f; // Reiniciar la carga del disparo
        shootingSlider.value = 0.0f; // Reiniciar la posición de la Slider
        shootingSlider.gameObject.SetActive(false);
    }

    public void SetNumberSprite()
    {
        string spriteName = number.ToString();
        foreach (Sprite sprite in numbersSpirtesheet)
        {
            if (sprite.name == spriteName)
            {
                numberSprite.sprite = sprite;
                break;
            }
        }
    }

    public void UserBrain()
    {
        user = true;
        indicador.SetActive(true);
        playerMovement.barraEstamina.gameObject.SetActive(true);

        team.currentPlayer.Insert(0, this);

        if (team.currentPlayer.Count > 2)
            team.currentPlayer.RemoveAt(2);

    }

    public void AiBrain()
    {
        user = false;
        indicador.SetActive(false);
        hasPossession = false;
        playerMovement.StopMovement();      //para que no se mueva en la direccion al pasarla
        playerMovement.barraEstamina.gameObject.SetActive(false);
    }

    //colocar jugador donde empieza
    public void ResetPosition()
    {
        transform.position = startPosition;
    }

    public void InGamePosition()
    {
        transform.position = zone.position;
    }

    public void PosiblesPases()
    {
        var smallestLeft = team.teamPlayers
            .Where(t => t != team.currentPlayer[0] && t.transform.position.x < transform.position.x 
                && t.transform.position.y <= transform.position.y)
            .OrderBy(t => Vector2.Distance(t.transform.position, transform.position))
            .FirstOrDefault();

        leftBotPlayer = smallestLeft;

        var smallestTopLeft = team.teamPlayers
            .Where(t => t != team.currentPlayer[0] && t.transform.position.x < transform.position.x 
                && t.transform.position.y > transform.position.y)
            .OrderBy(t => Vector2.Distance(t.transform.position, transform.position))
            .FirstOrDefault();

        leftTopPlayer = smallestTopLeft;


        var smallestRight = team.teamPlayers
            .Where(t => t != team.currentPlayer[0] && t.transform.position.x > transform.position.x 
                && t.transform.position.y <= transform.position.y)
            .OrderBy(t => Vector2.Distance(t.transform.position, transform.position))
            .FirstOrDefault();

        rightBotPlayer = smallestRight;

        var smallestTopRight = team.teamPlayers
            .Where(t => t != team.currentPlayer[0] && t.transform.position.x > transform.position.x 
                && t.transform.position.y > transform.position.y)
            .OrderBy(t => Vector2.Distance(t.transform.position, transform.position))
            .FirstOrDefault();

        rightTopPlayer = smallestTopRight;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!playerMovement.isParalyzed)
        {
            if (user)
            {
                if (collision.CompareTag("Rival"))
                {
                    currentRival = collision.gameObject.GetComponent<Rival>();
                    if (Ball.Instance.attachedPlayer == this && 
                        !GameManager.Instance.decisionActive && 
                        !currentRival.rivalStates.isParalyzed)
                    {
                        GameManager.Instance.n1 = number;
                        GameManager.Instance.n2 = currentRival.number;
                        GameManager.Instance.ShowDecisionUI();
                    }
                }
            }
            if (collision.CompareTag("Ball"))
            {
                // Si la pelota no está pegada y se colisiona con ella, la pega al jugador
                if (!Ball.Instance.isBallAttached)
                {
                    Ball.Instance.isBallAttached = true;
                    Ball.Instance.attachedPlayer = this;
                }
            }
        }


    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!playerMovement.isParalyzed)
        {
            if (user && !GameManager.Instance.miniGameActive)
            {
                if (collision.CompareTag("Rival"))
                {
                    currentRival = collision.gameObject.GetComponent<Rival>();
                    if (Ball.Instance.attachedPlayer == this && 
                        !GameManager.Instance.decisionActive && 
                        !currentRival.rivalStates.isParalyzed)
                    {
                        GameManager.Instance.n1 = number;
                        GameManager.Instance.n2 = currentRival.number;
                        GameManager.Instance.ShowDecisionUI();
                    }
                }
            }
            if (collision.CompareTag("Ball"))
            {
                if (Ball.Instance.isBallAttached && Ball.Instance.attachedPlayer == this)
                {
                    GainPossession();

                    //para cuando gane la posesion un jugador que no esta siendo manejado
                    if (!user)
                    {
                        UserBrain();
                        team.currentPlayer[1].AiBrain();
                    }
                }
            }
        }
    }

    public void GainPossession()
    {
        Ball.Instance.transform.position = ballPosition.position;
        Ball.Instance.transform.SetParent(ballPosition);
        Ball.Instance.rb.constraints = RigidbodyConstraints2D.FreezeAll;
        Ball.Instance.attachedPlayer = this;
        Ball.Instance.attachedRival = null;
        hasPossession = true;
        team.teamPossession = true;
        teamRival.teamPossession = false;
    }
}