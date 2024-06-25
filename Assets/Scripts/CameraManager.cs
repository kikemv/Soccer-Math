using TMPro;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public float offset;

    public Team team;
    private PlayerMovement movPlayer;
    public float smoothSpeed = 2f;
    public TextMeshProUGUI camText;

    public Camera mainCamera;

    private enum CameraMode { Ball, Player, Cursor };
    private CameraMode currentMode = CameraMode.Ball;

    private void Start()
    {
        transform.position = new Vector3(Ball.Instance.transform.position.x, Ball.Instance.transform.position.y, transform.position.z); ;
    }

    void Update()
    {
        movPlayer = team.currentPlayer[0].GetComponent<PlayerMovement>();

        switch (currentMode)
        {
            case CameraMode.Ball:
                FocusOnBall();
                camText.text = "CAM: BALL";
                break;
            case CameraMode.Player:
                FocusOnPlayer();
                camText.text = "CAM: PLAYER";
                break;
            case CameraMode.Cursor:
                FocusOnCursor();
                camText.text = "CAM: CURSOR";
                break;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchCameraMode();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            ToggleCameraSize();
        }
    }

    private void SwitchCameraMode()
    {
        currentMode = (CameraMode)(((int)currentMode + 1) % 3);
    }

    private void ToggleCameraSize()
    {
        if (mainCamera.orthographicSize == 135f)
        {
            mainCamera.orthographicSize = 300f;
        }
        else
        {
            mainCamera.orthographicSize = 135f;
        }
    }

    private void FocusOnBall()
    {
        if (movPlayer.movement.y > 0)
        {
            Vector3 ballPostion = new Vector3(Ball.Instance.transform.position.x, Ball.Instance.transform.position.y + offset*2, transform.position.z);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, ballPostion, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }
        else if (movPlayer.movement.y < 0)
        {
            Vector3 ballPostion = new Vector3(Ball.Instance.transform.position.x, Ball.Instance.transform.position.y - offset, transform.position.z);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, ballPostion, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }
        else
        {
            Vector3 ballPostion = new Vector3(Ball.Instance.transform.position.x, Ball.Instance.transform.position.y + offset / 2, transform.position.z);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, ballPostion, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }
    }

    private void FocusOnPlayer()
    {
        Vector3 playerPostion = new Vector3(team.currentPlayer[0].transform.position.x, team.currentPlayer[0].transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, playerPostion, smoothSpeed * Time.deltaTime);
    }

    private void FocusOnCursor()
    {
        // Obtenemos la posición del cursor en el mundo
        Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorPosition.z = transform.position.z;

        // Interpolamos la posición de la cámara hacia la posición del cursor
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, cursorPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}
