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

    public float estaminaMaxima; // Estamina m�xima del jugador
    public float consumoEstamina; // Consumo de estamina por segundo
    public float recargaEstamina; // Recarga de estamina por segundo
    public float velocidadReduccionEstamina; // Velocidad de reducci�n cuando la estamina se agota

    private float estaminaActual; // Estamina actual del jugador
    private bool corriendo = false; // Indica si el jugador est� corriendo
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
                // Reducir la opacidad del sprite para simular desactivaci�n
                spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f); // 50% de opacidad

                // Detener el movimiento y la interacci�n del jugador
                StopMovement();
                col.enabled = false;

                // Contador para la duraci�n de la par�lisis
                paralysisDuration -= Time.deltaTime;
                if (paralysisDuration <= 0f)
                {
                    // Volver a la normalidad despu�s de la duraci�n especificada
                    isParalyzed = false;
                    spriteRenderer.color = Color.white;
                    col.enabled = true;
                    paralysisDuration = 15f; // Restablecer la duraci�n para futuras par�lisis
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

        // Verificar si se est� pulsando la tecla SHIFT
        bool isShiftPressed = Input.GetKey(KeyCode.LeftShift);

        if (isShiftPressed) corriendo = true;
        else corriendo = false;

        // Establecer la velocidad actual seg�n si se est� presionando SHIFT o no
        velocidadActual = isShiftPressed ? velocidadAumentada : velocidadNormal;

        // Cambiar la velocidad de reproducci�n de la animaci�n
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
        // Si el jugador est� corriendo, consume estamina
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
        else // Si el jugador no est� corriendo, recarga estamina
        {
            estaminaActual += recargaEstamina * Time.deltaTime;

            // Limita la estamina al m�ximo
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
        // Obtener el �ngulo del movimiento en radianes
        float angulo = Mathf.Atan2(movement.y, movement.x) * Mathf.Rad2Deg;

        // Ajustar el �ngulo para asegurarse de que est� dentro del rango de 0 a 360 grados
        if (angulo < 0f)
        {
            angulo += 360f;
        }

        // Activar los triggers correspondientes seg�n el �ngulo de movimiento
        if (movement.magnitude > 0f)
        {
            // Determinar qu� trigger activar seg�n el �ngulo
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
            // Si no hay movimiento, desactivar la animaci�n de movimiento
            animator.SetTrigger("Idle");
        }
    }
}
