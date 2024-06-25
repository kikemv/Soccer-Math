using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class GameplayManeger : MonoBehaviour
{
    public static GameplayManeger Instance;

    public List<Transform> localNeutralZones;
    public List<Transform> localOffensiveZones;
    public List<Transform> localDefensiveZones;

    public List<Transform> awayNeutralZones;
    public List<Transform> awayOffensiveZones;
    public List<Transform> awayDefensiveZones;


    public Team localTeam;
    public TeamRival awayTeam;

    private List<int> assignedNumbers;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);


        localTeam = FindObjectOfType<Team>();
        awayTeam = FindObjectOfType<TeamRival>(); ;
        SetZones();
    }

    // Start is called before the first frame update
    void Start()
    {
        assignedNumbers = new List<int>();
        SetRandomNumber();
    }

    private void SetZones()
{
    // Genera una lista de índices de la misma longitud que las listas de zonas
    var indices = Enumerable.Range(0, localNeutralZones.Count).ToList();
    // Mezcla aleatoriamente los índices
    indices = indices.OrderBy(a => Random.value).ToList();

    // Asigna las zonas usando los índices mezclados
    int cont = 0;
    foreach (Player player in localTeam.teamPlayers)
    {
        int index = indices[cont];
        player.zone = localNeutralZones[index];
        player.offensiveZone = localOffensiveZones[index];
        player.defensiveZone = localDefensiveZones[index];
        cont++;

        if (player.zone.name == "zone (4)" || player.zone.name == "zone (5)" || player.zone.name == "zone (6)")
        {
            player.playerRole = PlayerRole.Mid;
        }
        else if (player.zone.name == "zone (7)" || player.zone.name == "zone (8)" || player.zone.name == "zone (9)")
        {
            player.playerRole = PlayerRole.Forward;
            if (player.zone.name == "zone (8)")
                player.sacador = true;
        }
    }

    // Genera y mezcla la lista de índices para el equipo rival
    indices = Enumerable.Range(0, awayNeutralZones.Count).ToList();
    indices = indices.OrderBy(a => Random.value).ToList();

    cont = 0;
    foreach (Rival rival in awayTeam.teamRivals)
    {
        int index = indices[cont];
        rival.zone = awayNeutralZones[index];
        rival.offensiveZone = awayOffensiveZones[index];
        rival.defensiveZone = awayDefensiveZones[index];
        cont++;

        if (rival.zone.name == "zone (4)" || rival.zone.name == "zone (5)" || rival.zone.name == "zone (6)")
        {
            rival.playerRole = PlayerRole.Mid;
        }
        else if (rival.zone.name == "zone (7)" || rival.zone.name == "zone (8)" || rival.zone.name == "zone (9)")
        {
            rival.playerRole = PlayerRole.Forward;
            if (rival.zone.name == "zone (8)")
                rival.sacador = true;
        }
    }
}


    private void SetRandomNumber()
    {
        // Determinar el rango de números según la dificultad
        int maximNumber = 11;
        switch (GameSettings.Instance.difficulty)
        {
            case GameSettings.Difficulty.Easy:
                maximNumber = 11;
                break;
            case GameSettings.Difficulty.Medium:
                maximNumber = 11;
                break;
            case GameSettings.Difficulty.Hard:
                maximNumber = 21;
                break;
        }

        // Generar números aleatorios únicos para cada jugador
        foreach (Player player in localTeam.teamPlayers)
        {
            int numeroAleatorio;
            do
            {
                numeroAleatorio = Random.Range(1, maximNumber);
            } while (assignedNumbers.Contains(numeroAleatorio));

            // Asignar el número aleatorio al jugador y agregarlo a la lista de números asignados
            player.number = numeroAleatorio;
            player.SetNumberSprite();
            assignedNumbers.Add(numeroAleatorio);
        }

        assignedNumbers = new List<int>();

        foreach (Rival rival in awayTeam.teamRivals)
        {
            int numeroAleatorio;
            do
            {
                numeroAleatorio = Random.Range(1, maximNumber);
            } while (assignedNumbers.Contains(numeroAleatorio));

            // Asignar el número aleatorio al jugador y agregarlo a la lista de números asignados
            rival.number = numeroAleatorio;
            rival.SetNumberSprite();
            assignedNumbers.Add(numeroAleatorio);
        }
    }
}