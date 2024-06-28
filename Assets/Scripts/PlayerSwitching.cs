using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSwitching : MonoBehaviour
{
    private Team team;

    public float passForce;

    public AudioClip passSound;

    private void Awake()
    {
        team = GetComponent<Team>();
    }

    void Start()
    {
        SelectPlayerOnStart();
    }


    void Update()
    {
        Player player = team.currentPlayer[0];

        if (Ball.Instance.isBallAttached)
        {

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (Input.GetAxis("Vertical") > 0 && player.leftTopPlayer != null)
                {
                    if (player.hasPossession)
                    {
                        Ball.Instance.isBallAttached = false;
                        Passing(player.leftTopPlayer);
                    }
                    player.leftTopPlayer.UserBrain();
                    team.currentPlayer[1].AiBrain();
                }
                else if (Input.GetAxis("Vertical") <= 0 && player.leftBotPlayer != null)
                {
                    if (player.hasPossession)
                    {
                        Ball.Instance.isBallAttached = false;
                        Passing(player.leftBotPlayer);
                    }
                    player.leftBotPlayer.UserBrain();
                    team.currentPlayer[1].AiBrain();
                }
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (Input.GetAxis("Vertical") > 0 && player.rightTopPlayer != null)
                {
                    if (player.hasPossession)
                    {
                        Ball.Instance.isBallAttached = false;
                        Passing(player.rightTopPlayer);
                    }
                    player.rightTopPlayer.UserBrain();
                    team.currentPlayer[1].AiBrain();
                }
                else if (Input.GetAxis("Vertical") <= 0 && player.rightBotPlayer != null)
                {
                    if (player.hasPossession)
                    {
                        Ball.Instance.isBallAttached = false;
                        Passing(player.rightBotPlayer);
                    }
                    player.rightBotPlayer.UserBrain();
                    team.currentPlayer[1].AiBrain();
                }
            }
        }
    }

    private void SelectPlayerOnStart()
    {
        var smallestDistance = team.teamPlayers
            .OrderBy(t => Vector2.Distance(Ball.Instance.transform.position, t.transform.position))
            .FirstOrDefault();

        if (smallestDistance != null)
            smallestDistance.UserBrain();
    }

    private void Passing(Player receiver)
    {
        SoundController.Instance.PlaySound(passSound, 0.3f);

        var direction = (receiver.transform.position - Ball.Instance.transform.position).normalized;

        Ball.Instance.rb.constraints = RigidbodyConstraints2D.None;
        Ball.Instance.transform.SetParent(null);
        Ball.Instance.rb.AddForce(direction * passForce, ForceMode2D.Impulse);
        team.currentPlayer[0].hasPossession = false;

        //para la toma de decision
        Time.timeScale = 1;
        GameManager.Instance.HideDecisionUI();

        //por si tiene la shoot bar
        team.currentPlayer[0].shootingSlider.gameObject.SetActive(false);

        //para el saque de centro
        GameManager.Instance.ResumeGame();

    }
}