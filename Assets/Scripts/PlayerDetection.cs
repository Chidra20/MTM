using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    private CameraController cameraController;

    private void Start()
    {
        cameraController = FindFirstObjectByType<CameraController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("corridorArea"))
        {
            cameraController.EnterCorridor(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("corridorArea"))
        {
            cameraController.ExitCorridor();
        }
    }
}