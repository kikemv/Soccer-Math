using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamRival : MonoBehaviour
{
    public bool teamPossession;

    public List<Rival> teamRivals;

    private void Awake()
    {
        teamRivals = GetComponentsInChildren<Rival>().ToList();
    }

    private void Start()
    {
    }

    private void Update()
    {
        PossessionCheck();
    }

    public void PossessionCheck()
    {
        foreach (Rival rival in teamRivals)
        {
            if (rival.hasPossession)
            {
                teamPossession = true;
                return;
            }
            teamPossession = false;
        }
    }
}