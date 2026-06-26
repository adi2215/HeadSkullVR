using UnityEngine;

public class SketchfabCameraController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Target")]
    [SerializeField] private Transform orbitTarget;

    [Header("Layer")]
    [SerializeField] private LayerMask boneLayer;

    [Header("Orbit")]
    [SerializeField] private float orbitSpeed = 0.25f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    [Header("Zoom")]
    [SerializeField] private float zoomAcceleration = 6f;
    [SerializeField] private float zoomFriction = 10f;
    [SerializeField] private float zoomSmoothTime = 0.08f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 8f;

    [Header("Pan")]
    [SerializeField] private float panSpeed = 0.003f;

    private float yaw;
    private float pitch;

    private float distance;
    private float targetDistance;
    private float zoomSmoothVelocity;
    private float zoomInputVelocity;

    private bool isOrbiting;
    private bool isPanning;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Start()
    {
        if (orbitTarget == null)
        {
            GameObject pivot = new GameObject("CameraPivot");
            pivot.transform.position = Vector3.zero;
            orbitTarget = pivot.transform;
        }

        Vector3 direction = transform.position - orbitTarget.position;

        distance = direction.magnitude;
        targetDistance = distance;

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        HandleMouseInput();
        HandleZoom();
    }

    private void LateUpdate()
    {
        UpdateCameraPosition();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsMouseOverBone())
            {
                isOrbiting = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isOrbiting = false;
        }

        if (Input.GetMouseButtonDown(1))
        {
            isPanning = true;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isPanning = false;
        }

        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        if (isOrbiting)
        {
            yaw += mouseDelta.x * orbitSpeed * 100f;
            pitch -= mouseDelta.y * orbitSpeed * 100f;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        if (isPanning)
        {
            Vector3 right = transform.right;
            Vector3 up = transform.up;

            Vector3 panMove = (-right * mouseDelta.x - up * mouseDelta.y) * panSpeed * distance * 100f;
            orbitTarget.position += panMove;
        }
    }

    private void HandleZoom()
    {
        if (ScreenBoneShelf.IsPointerOverShelf)
            return;

        if (MouseBoneScreenDrag.CurrentHeldTransform != null)
            return;

        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            zoomInputVelocity += scroll * zoomAcceleration;
        }

        targetDistance -= zoomInputVelocity * Time.deltaTime;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

        zoomInputVelocity = Mathf.Lerp(
            zoomInputVelocity,
            0f,
            Time.deltaTime * zoomFriction
        );
    }

    private void UpdateCameraPosition()
    {
        if (orbitTarget == null)
            return;

        distance = Mathf.SmoothDamp(
            distance,
            targetDistance,
            ref zoomSmoothVelocity,
            zoomSmoothTime
        );

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        transform.position = orbitTarget.position - rotation * Vector3.forward * distance;
        transform.rotation = rotation;
    }

    private bool IsMouseOverBone()
    {
        if (targetCamera == null)
        {
            Debug.LogWarning("[SketchfabCameraController] Target Camera is not assigned.");
            return false;
        }

        Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);

        return Physics.Raycast(
            ray,
            out RaycastHit hit,
            100f,
            boneLayer,
            QueryTriggerInteraction.Collide
        );
    }
}