using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    private CameraController cameraController;

    private void Start()
    {
        cameraController = FindAnyObjectByType<CameraController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("hallCam"))
        {
            cameraController.EnterHall();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("hallCam"))
        {
            cameraController.ExitHall();
        }
    }
}