using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Hall Settings")]
    public float hallZoom = 5f;
    public float normalZoom = 7f;
    public float transitionSpeed = 5f;

    private bool inHall;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographicSize = normalZoom;
    }

    void Update()
    {
        float targetZoom = inHall ? hallZoom : normalZoom;

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            transitionSpeed * Time.deltaTime
        );
    }

    public void EnterHall()
    {
        inHall = true;
    }

    public void ExitHall()
    {
        inHall = false;
    }
}