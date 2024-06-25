using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{
    private AudioSource audioSource;

    void Awake()
    {
        // Asegura que este objeto no se destruya al cargar nuevas escenas
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Obtiene o a�ade un AudioSource al GameObject
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Inicia la reproducci�n del audio
        audioSource.Play();
    }

    void OnDestroy()
    {
        // Detiene la reproducci�n del audio antes de que este GameObject se destruya
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}
