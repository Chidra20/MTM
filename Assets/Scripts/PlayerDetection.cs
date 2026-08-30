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
        if (cameraController == null)
            return;

        if (other.CompareTag("corridorArea"))
        {
            cameraController.EnterCorridor(other);
        }
        else if (other.CompareTag("ledgeArea"))
        {
            cameraController.EnterLedge();
        }
        else if (other.CompareTag("bossArea"))
        {
            cameraController.EnterBossArea(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (cameraController == null)
            return;

        if (other.CompareTag("corridorArea"))
        {
            cameraController.ExitArea();
        }
        else if (other.CompareTag("ledgeArea"))
        {
            cameraController.ExitArea();
        }
        else if (other.CompareTag("bossArea"))
        {
            cameraController.ExitArea();
        }
    }
}