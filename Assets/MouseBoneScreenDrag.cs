using UnityEngine;

public class MouseBoneScreenDrag : MonoBehaviour
{
    public static Transform CurrentHeldTransform;

    [Header("References")]
    [SerializeField] private Camera targetCamera;

    [Header("Grab")]
    [SerializeField] private LayerMask boneLayer;
    [SerializeField] private float grabDistance = 100f;

    [Header("Drag")]
    [SerializeField] private float dragSmooth = 25f;
    [SerializeField] private float minDragDepth = 0.8f;
    [SerializeField] private float maxDragDepth = 8f;
    [SerializeField] private float scrollDepthSpeed = 0.5f;

    private BonePiece heldBone;
    private float dragDepth;
    private Vector3 grabOffset;

    public bool IsDragging => heldBone != null;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Update()
    {
        if (ControlModeManager.Instance != null && !ControlModeManager.Instance.IsPC)
            return;

        if (Input.GetMouseButtonDown(0))
            TryGrab();

        if (Input.GetMouseButton(0) && heldBone != null)
            DragBone();

        if (Input.GetMouseButtonUp(0))
            DropBone();
    }

    private void TryGrab()
    {
        if (targetCamera == null)
            return;

        Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance, boneLayer, QueryTriggerInteraction.Collide))
        {
            BonePiece bone = hit.collider.GetComponentInParent<BonePiece>();

            if (bone == null || bone.IsPlaced)
                return;

            heldBone = bone;
            heldBone.transform.SetParent(null, true);
            CurrentHeldTransform = heldBone.transform;

            Rigidbody rb = heldBone.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            Vector3 screenPos = targetCamera.WorldToScreenPoint(heldBone.transform.position);
            dragDepth = Mathf.Clamp(screenPos.z, minDragDepth, maxDragDepth);

            Vector3 mouseWorld = targetCamera.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, dragDepth)
            );

            grabOffset = heldBone.transform.position - mouseWorld;

            Debug.Log("[MouseBoneScreenDrag] Grabbed: " + bone.BoneID);
        }
    }

    private void DragBone()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            dragDepth += scroll * scrollDepthSpeed;
            dragDepth = Mathf.Clamp(dragDepth, minDragDepth, maxDragDepth);
        }

        Vector3 mouseWorld = targetCamera.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, dragDepth)
        );

        Vector3 targetPosition = mouseWorld + grabOffset;

        heldBone.transform.position = Vector3.Lerp(
            heldBone.transform.position,
            targetPosition,
            Time.deltaTime * dragSmooth
        );
    }

    private void DropBone()
    {
        if (heldBone == null)
            return;

        BonePiece droppedBone = heldBone;

        heldBone = null;
        CurrentHeldTransform = null;

        droppedBone.TryPlaceBone();

        Debug.Log("[MouseBoneScreenDrag] Dropped: " + droppedBone.BoneID);
    }
}