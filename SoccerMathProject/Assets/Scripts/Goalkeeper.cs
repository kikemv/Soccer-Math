using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Goalkeeper : MonoBehaviour
{
    public float leftBound;
    public float rightBound;
    public float movementSpeed;

    public bool local;
    public Team team;
    public TeamRival teamRival;

    private void Awake()
    {
        team = FindObjectOfType<Team>();
        teamRival = FindObjectOfType<TeamRival>();
    }

    void Update()
    {
        Vector3 currentPosition = transform.position;

        if (Ball.Instance.transform.position.x < currentPosition.x && currentPosition.x > leftBound)
        {
            currentPosition.x -= movementSpeed * Time.deltaTime;
        }
        else if (Ball.Instance.transform.position.x > currentPosition.x && currentPosition.x < rightBound)
        {
            currentPosition.x += movementSpeed * Time.deltaTime;
        }

        currentPosition.x = Mathf.Clamp(currentPosition.x, leftBound, rightBound);

        transform.position = currentPosition;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            Ball.Instance.transform.position = transform.position;
            Ball.Instance.rb.constraints = RigidbodyConstraints2D.FreezeAll;
            Invoke("Pass", 2);
        }
    }


    private void Pass()
    {
        if (local)
        {
            Player receiver = FindNearestPlayer();
            var direction = (receiver.transform.position - Ball.Instance.transform.position).normalized;
            Ball.Instance.rb.constraints = RigidbodyConstraints2D.None;
            Ball.Instance.transform.SetParent(null);
            Ball.Instance.rb.AddForce(direction * 60, ForceMode2D.Impulse);
        }
        else
        {
            Rival receiver = FindNearestRival();
            var direction = (receiver.transform.position - Ball.Instance.transform.position).normalized;
            Ball.Instance.rb.constraints = RigidbodyConstraints2D.None;
            Ball.Instance.transform.SetParent(null);
            Ball.Instance.rb.AddForce(direction * 60, ForceMode2D.Impulse);
        }
    }

    private Player FindNearestPlayer()
    {
        Player nearestPlayer = team.teamPlayers
            .OrderBy(t => Vector2.Distance(Ball.Instance.transform.position, t.transform.position))
            .FirstOrDefault();

        return nearestPlayer;
    }

    private Rival FindNearestRival()
    {
        Rival nearestPlayer = teamRival.teamRivals
            .OrderBy(t => Vector2.Distance(Ball.Instance.transform.position, t.transform.position))
            .FirstOrDefault();

        return nearestPlayer;
    }
}

