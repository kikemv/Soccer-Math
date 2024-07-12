using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Team : MonoBehaviour
{
    public List<Player> teamPlayers;
    public List<Player> currentPlayer;
    public bool teamPossession;

    private void Awake()
    {
        teamPlayers = GetComponentsInChildren<Player>().ToList();
        currentPlayer = new List<Player>();
    }

    private void Start()
    {
        //PossessionCheck();
        teamPossession = true;
    }

    private void Update()
    {
        if (!teamPossession)
        {
            currentPlayer[0].shootingCharge = 0.0f;
        }
    }

    public void PossessionCheck()
    {
        foreach (Player player in teamPlayers)
        {
            if (player.hasPossession)
            {
                teamPossession = true;
                return;
            }
        }
        teamPossession = false;
    }
}
