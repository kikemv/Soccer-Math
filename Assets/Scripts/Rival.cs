using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Rival : MonoBehaviour
{
    public Transform ballPosition;
    public bool hasPossession;

    public PlayerRole playerRole;
    public bool sacador = false;

    private Team team;
    private TeamRival teamRival;

    public Vector2 startPosition;
    public Transform zone;
    public Transform offensiveZone;
    public Transform defensiveZone;
    public float offset;

    public int number;
    private SpriteRenderer numberSprite;
    public Sprite[] numbersSpirtesheet;

    public RivalState rivalStates;

    // Barra de carga para el disparo del rival
    public Slider shootingSlider;
    public float shootingChargeRate = 0.2f;
    public float maxShootingCharge = 1.0f;
    private float shootingCharge = 0.0f;

    //sprites para animaciones
    public Sprite winSprite;
    public Sprite loseSprite;

    private void Awake()
    {
        teamRival = GetComponentInParent<TeamRival>();
        team = FindObjectOfType<Team>();
        rivalStates = GetComponent<RivalState>();
        ballPosition = transform.Find("BallPossition");
        numberSprite = transform.Find("number").gameObject.GetComponent<SpriteRenderer>();
        shootingChargeRate = 0.4f;
    }

    void Start()
    {
        if (sacador) offset += 150;

        if (playerRole == PlayerRole.Forward)
        {
            startPosition = new Vector3(zone.position.x, zone.position.y + offset, zone.position.z);
        }
        else if (playerRole == PlayerRole.Mid)
        {
            startPosition = new Vector3(zone.position.x, zone.position.y + offset / 2, zone.position.z);
        }
        else startPosition = new Vector3(zone.position.x, zone.position.y + offset / 6, zone.position.z);
        ResetPosition();

        // Desactivar la Slider al principio
        shootingSlider.gameObject.SetActive(false);
    }

    void Update()
    {
        if (hasPossession)
        {
            if (transform.position.y < 400 && !GameManager.Instance.gamePaused && !rivalStates.isParalyzed)
            {
                shootingSlider.gameObject.SetActive(true);
                if (shootingCharge < maxShootingCharge)
                {
                    shootingCharge += shootingChargeRate * Time.deltaTime;
                    shootingSlider.value = shootingCharge / maxShootingCharge;
                }
                else if(!GameManager.Instance.miniGameActive && !GameManager.Instance.decisionActive
                         && !GameManager.Instance.shootGameActive)
                {
                    Debug.Log("Llamando al shootgame");
                    GameManager.Instance.StartShootGame();
                }
            }
            else
            {
                ResetShoot();
            }
        }
        else
        {
            shootingSlider.gameObject.SetActive(false);
        }

    }

    public void ResetShoot()
    {
        shootingCharge = 0.0f; // Reiniciar la carga del disparo
        shootingSlider.value = 0.0f; // Reiniciar la posici�n de la Slider
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

    public void ResetPosition()
    {
        transform.position = startPosition;
    }
    public void InGamePosition()
    {
        transform.position = zone.position;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!rivalStates.isParalyzed)
        {
            if (other.CompareTag("Ball"))
            {
                if (Ball.Instance.attachedRival == this)
                    GainPossession();
            }
        }
        else
        {
            Ball.Instance.isBallAttached = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!rivalStates.isParalyzed)
        {
            if (other.CompareTag("Ball"))
            {
                if (!Ball.Instance.isBallAttached && Ball.Instance.lastRival!=this)
                {
                    Ball.Instance.isBallAttached = true;
                    Ball.Instance.attachedRival = this;
                }
            }
        }
    }

    public void GainPossession()
    {
        Ball.Instance.transform.position = ballPosition.position;
        Ball.Instance.transform.SetParent(ballPosition);
        Ball.Instance.rb.constraints = RigidbodyConstraints2D.FreezeAll;
        Ball.Instance.attachedRival = this;
        Ball.Instance.attachedPlayer = null;
        hasPossession = true;
        team.teamPossession = false;
        teamRival.teamPossession = true;
    }

}