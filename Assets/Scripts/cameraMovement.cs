using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public CinemachineCamera cinemachineCamera;
    public Transform cameraPointTop;
    public Transform cameraPointBottom;
    public Transform player;

    [Header("Normal")]
    public float normalZoom = 7f;

    [Header("Area Settings")]
    public float areaPadding = 1f;
    public float positionSmoothness = 5f;
    public float corridorLookAhead = 3f;
    public float zoomSmoothness = 5f;
    public float corridorPositionSmoothness = 10f;

    private enum CameraMode
    {
        Normal,
        Corridor,
        Ledge,
        Boss
    }

    private CameraMode currentMode = CameraMode.Normal;

    private float areaCenterY;
    private float areaCenterX;
    private float areaZoom;
    private float lastDirection = 1f;
    private float previousPlayerX;

    private Transform cameraTarget;

    void Start()
    {
        cinemachineCamera.Follow = CreateCameraTarget();
        cinemachineCamera.Lens.OrthographicSize = normalZoom;
        previousPlayerX = player.position.x;
    }

    void LateUpdate()
    {
        UpdateCameraTarget();

       float targetZoom;

switch (currentMode)
{
    case CameraMode.Corridor:
    case CameraMode.Boss:
        targetZoom = areaZoom;
        break;

    default:
        targetZoom = normalZoom;
        break;
}

cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(
    cinemachineCamera.Lens.OrthographicSize,
    targetZoom,
    zoomSmoothness * Time.deltaTime
);

        cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(
            cinemachineCamera.Lens.OrthographicSize,
            targetZoom,
            zoomSmoothness * Time.deltaTime
        );
    }

  private void UpdateCameraTarget()
{
    Vector3 desiredPosition;

    // Figure out which direction the player is actually moving
    float playerDeltaX = player.position.x - previousPlayerX;

    if (Mathf.Abs(playerDeltaX) > 0.01f)
    {
        lastDirection = Mathf.Sign(playerDeltaX);
    }

    previousPlayerX = player.position.x;

    switch (currentMode)
    {
        case CameraMode.Normal:
            desiredPosition = cameraPointTop.position;
            break;

        case CameraMode.Ledge:
            desiredPosition = cameraPointBottom.position;
            break;

        case CameraMode.Corridor:

            desiredPosition = new Vector3(
                player.position.x + (corridorLookAhead * lastDirection),
                areaCenterY,
                cameraTarget.position.z
            );

            break;

        case CameraMode.Boss:
            desiredPosition = new Vector3(
                areaCenterX,
                areaCenterY,
                cameraTarget.position.z
            );
            break;

        default:
            desiredPosition = cameraPointTop.position;
            break;
    }

 float smoothness = currentMode == CameraMode.Corridor
    ? corridorPositionSmoothness
    : positionSmoothness;

cameraTarget.position = Vector3.Lerp(
    cameraTarget.position,
    desiredPosition,
    smoothness * Time.deltaTime
);
}

    private Transform CreateCameraTarget()
    {
        GameObject targetObject = new GameObject("CameraTarget");
        cameraTarget = targetObject.transform;
        cameraTarget.position = cameraPointTop.position;
        return cameraTarget;
    }

    public void EnterCorridor(Collider2D corridor)
    {
        currentMode = CameraMode.Corridor;

        Bounds bounds = corridor.bounds;
        areaCenterY = bounds.center.y;

        float height = bounds.size.y;
        areaZoom = (height * 0.5f) + areaPadding;
    }

    public void EnterLedge()
    {
        currentMode = CameraMode.Ledge;
    }

    public void EnterBossArea(Collider2D bossArea)
    {
        currentMode = CameraMode.Boss;

        Bounds bounds = bossArea.bounds;
        areaCenterX = bounds.center.x;
        areaCenterY = bounds.center.y;

        float height = bounds.size.y;
        areaZoom = (height * 0.5f) + areaPadding;
    }

    public void ExitArea()
    {
        currentMode = CameraMode.Normal;
    }
}