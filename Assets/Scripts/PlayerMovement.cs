using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float velocidadNormal;
    public float velocidadAumentada;

    private float velocidadActual;

    private Rigidbody2D rb;
    public Vector2 movement = new Vector2(0, 0);
    private Player player;
    private Collider2D col;
    private SpriteRenderer spriteRenderer;

    //para stunear
    public float paralysisDuration = 15f;
    public bool isParalyzed = false;

    public bool isPaused = false;

    private Animator animator;

    //barra estamina
    public Slider barraEstamina;
    public float estaminaMaxima;
    public float consumoEstamina;
    public float recargaEstamina;
    public float velocidadReduccionEstamina;

    private float estaminaActual;
    private bool corriendo = false;
    private bool cansado = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        estaminaActual = estaminaMaxima;
        barraEstamina.value = estaminaActual;
    }

    private void FixedUpdate()
    {
        if (!isPaused)
        {
            if (isParalyzed)
            {
                animator.SetTrigger("Paralyzed");
                player.playerState.state = States.Paralyzed;
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f); // 50% de opacidad

                //detener el movimiento y la interacción del jugador
                StopMovement();
                col.enabled = false;

                paralysisDuration -= Time.deltaTime;
                if (paralysisDuration <= 0f)
                {
                    //volver a la normalidad
                    isParalyzed = false;
                    spriteRenderer.color = Color.white;
                    col.enabled = true;
                    paralysisDuration = 15f;
                    animator.SetTrigger("Idle");
                }
            }
            else
            {
                if (player.user)
                {
                    UpdateMovement();
                    UpdateAnimatorParameters();
                    UpdateEstamina();
                }
            }
        }
    }

    private void UpdateMovement()
    {
        bool isShiftPressed = Input.GetKey(KeyCode.LeftShift);

        if (isShiftPressed) corriendo = true;
        else corriendo = false;

        //establecer la velocidad según si se está presionando SHIFT o no
        velocidadActual = isShiftPressed ? velocidadAumentada : velocidadNormal;

        //cambiar la velocidad de reproducción de la animación
        animator.speed = isShiftPressed ? 1.5f : 1.0f;

        movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        movement.Normalize();

        //si la estamina se ha agotado, reduce la velocidad del jugador
        if (estaminaActual <= 0f)
        {
            cansado = true;
        }
        if (cansado)
        {
            velocidadActual = 1000f;
            animator.speed = 0.75f;
            if (barraEstamina.value == 1f) cansado = false;
        }

        rb.velocity = movement * velocidadActual * Time.deltaTime;
    }

    private void UpdateEstamina()
    {
        if (corriendo)
        {
            estaminaActual -= consumoEstamina * Time.deltaTime;

            if (estaminaActual <= 0f)
            {
                velocidadActual = velocidadNormal * velocidadReduccionEstamina;
            }
            else
            {
                velocidadActual = velocidadAumentada;
            }

            barraEstamina.value = estaminaActual / estaminaMaxima;
        }
        else
        {
            estaminaActual += recargaEstamina * Time.deltaTime;

            estaminaActual = Mathf.Clamp(estaminaActual, 0f, estaminaMaxima);

            velocidadActual = velocidadNormal;

            barraEstamina.value = estaminaActual / estaminaMaxima;
        }
    }

    public void StopMovement()
    {
        rb.velocity = Vector2.zero;
    }

    public void ActivateParalysis()
    {
        isParalyzed = true;
        player.hasPossession = false;
    }

    private void UpdateAnimatorParameters()
    {
        float angulo = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;

        //ajustar angulo
        if (angulo < 0f)
        {
            angulo += 360f;
        }

        //activar los triggers correspondientes según el ángulo
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
            //si no hay movimiento, desactivar la animación de movimiento
            animator.SetTrigger("Idle");
        }
    }
}
