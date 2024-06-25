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
    public Slider barraEstamina; // Referencia a la barra de estamina en la interfaz de usuario

    public float estaminaMaxima; // Estamina máxima del jugador
    public float consumoEstamina; // Consumo de estamina por segundo
    public float recargaEstamina; // Recarga de estamina por segundo
    public float velocidadReduccionEstamina; // Velocidad de reducción cuando la estamina se agota

    private float estaminaActual; // Estamina actual del jugador
    private bool corriendo = false; // Indica si el jugador está corriendo
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
                // Reducir la opacidad del sprite para simular desactivación
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f); // 50% de opacidad

                // Detener el movimiento y la interacción del jugador
                StopMovement();
                col.enabled = false;

                // Contador para la duración de la parálisis
                paralysisDuration -= Time.deltaTime;
                if (paralysisDuration <= 0f)
                {
                    // Volver a la normalidad después de la duración especificada
                    isParalyzed = false;
                    spriteRenderer.color = Color.white;
                    col.enabled = true;
                    paralysisDuration = 15f; // Restablecer la duración para futuras parálisis
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

        // Verificar si se está pulsando la tecla SHIFT
        bool isShiftPressed = Input.GetKey(KeyCode.LeftShift);

        if (isShiftPressed) corriendo = true;
        else corriendo = false;

        // Establecer la velocidad actual según si se está presionando SHIFT o no
        velocidadActual = isShiftPressed ? velocidadAumentada : velocidadNormal;

        // Cambiar la velocidad de reproducción de la animación
        animator.speed = isShiftPressed ? 1.5f : 1.0f;

        movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // Normalizar el vector de movimiento para asegurarse de que la velocidad diagonal no sea mayor
        movement.Normalize();

        // Si la estamina se ha agotado, reduce la velocidad del jugador
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

        // Actualizar la velocidad del Rigidbody
        rb.velocity = movement * velocidadActual * Time.deltaTime;
    }

    private void UpdateEstamina()
    {
        // Si el jugador está corriendo, consume estamina
        if (corriendo)
        {
            estaminaActual -= consumoEstamina * Time.deltaTime;

            // Si la estamina se agota, reduce la velocidad del jugador
            if (estaminaActual <= 0f)
            {
                velocidadActual = velocidadNormal * velocidadReduccionEstamina;
            }
            else
            {
                velocidadActual = velocidadAumentada;
            }

            // Actualiza la barra de estamina en la interfaz de usuario
            barraEstamina.value = estaminaActual / estaminaMaxima;
        }
        else // Si el jugador no está corriendo, recarga estamina
        {
            estaminaActual += recargaEstamina * Time.deltaTime;

            // Limita la estamina al máximo
            estaminaActual = Mathf.Clamp(estaminaActual, 0f, estaminaMaxima);

            // Restaura la velocidad normal
            velocidadActual = velocidadNormal;

            // Actualiza la barra de estamina en la interfaz de usuario
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
        }
        else
        {
            // Si no hay movimiento, desactivar la animación de movimiento
            animator.SetTrigger("Idle");
        }
    }
}
