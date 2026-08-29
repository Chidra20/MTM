using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public CinemachineCamera cinemachineCamera;
    public Transform player;

    [Header("Normal Camera")]
    public float normalZoom = 7f;

    [Header("Corridor")]
    public float corridorPadding = 1f;
    public float positionSmoothness = 8f;
    public float zoomSmoothness = 5f;

    private bool inCorridor;

    private float corridorCenterY;
    private float corridorZoom;

    void Start()
    {
        cinemachineCamera.Lens.OrthographicSize = normalZoom;
    }

    void LateUpdate()
    {
        if (inCorridor)
        {
            UpdateCorridorCamera();
        }
        else
        {
            UpdateNormalCamera();
        }
    }

    void UpdateCorridorCamera()
    {
        // Calculate desired position
        Vector3 targetPosition = new Vector3(
            player.position.x,
            corridorCenterY,
            transform.position.z
        );

        // Smooth movement
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            positionSmoothness * Time.deltaTime
        );

        // Smooth zoom
        cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(
            cinemachineCamera.Lens.OrthographicSize,
            corridorZoom,
            zoomSmoothness * Time.deltaTime
        );
    }

    void UpdateNormalCamera()
    {
        // Cinemachine handles normal camera movement.
        cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(
            cinemachineCamera.Lens.OrthographicSize,
            normalZoom,
            zoomSmoothness * Time.deltaTime
        );
    }

    public void EnterCorridor(Collider2D corridor)
    {
        inCorridor = true;

        Bounds bounds = corridor.bounds;

        // Exact middle of corridor
        corridorCenterY = bounds.center.y;

        // Fit entire corridor vertically
        corridorZoom = (bounds.size.y / 2f) + corridorPadding;
    }

    public void ExitCorridor()
    {
        inCorridor = false;
    }
}