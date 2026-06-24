using UnityEngine;

public class MouseBoneGrabber : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint;

    [Header("Grab")]
    [SerializeField] private float grabDistance = 5f;
    [SerializeField] private LayerMask boneLayer;

    [Header("Hold")]
    [SerializeField] private float positionSmooth = 20f;

    [Header("Rotation")]
    [SerializeField] private float rotateSpeed = 90f;

    [Header("Debug")]
    [SerializeField] private bool showDebugRay = true;

    private BonePiece heldBone;
    private Rigidbody heldRb;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    private void Update()
    {
        if (ControlModeManager.Instance != null && !ControlModeManager.Instance.IsPC)
            return;

        DrawRayDebug();

        if (Input.GetMouseButtonDown(0))
            TryGrab();

        if (Input.GetMouseButtonUp(0))
            Drop();

        if (heldBone != null)
        {
            MoveHeldBone();
            RotateHeldBone();
        }
    }

    private void TryGrab()
    {
        if (heldBone != null)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance, boneLayer))
        {
            BonePiece bone = hit.collider.GetComponentInParent<BonePiece>();

            if (bone == null || bone.IsPlaced)
                return;

            heldBone = bone;
            heldRb = bone.GetComponent<Rigidbody>();

            if (heldRb != null)
            {
                heldRb.useGravity = false;
                heldRb.isKinematic = true;
            }

            Debug.Log("[MouseBoneGrabber] Взял кость: " + bone.BoneID);
        }
        else
        {
            Debug.LogWarning("[MouseBoneGrabber] Raycast не попал в кость.");
        }
    }

    private void MoveHeldBone()
    {
        if (heldBone == null || holdPoint == null)
            return;

        heldBone.transform.position = Vector3.Lerp(
            heldBone.transform.position,
            holdPoint.position,
            Time.deltaTime * positionSmooth
        );
    }

    private void Drop()
    {
        if (heldBone == null)
            return;

        Debug.Log("[MouseBoneGrabber] Отпустил кость: " + heldBone.BoneID);

        BonePiece boneToDrop = heldBone;
        Rigidbody rbToDrop = heldRb;

        heldBone = null;
        heldRb = null;

        boneToDrop.TryPlaceBone();

        // Если не поставилась правильно — просто оставляем на месте без физики
        if (!boneToDrop.IsPlaced && rbToDrop != null)
        {
            rbToDrop.useGravity = false;
            rbToDrop.isKinematic = true;
        }
    }

    private void RotateHeldBone()
    {
        if (heldBone == null)
            return;

        if (Input.GetKey(KeyCode.Q))
            heldBone.transform.Rotate(Vector3.up, -rotateSpeed * Time.deltaTime, Space.World);

        if (Input.GetKey(KeyCode.E))
            heldBone.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);

        if (Input.GetKey(KeyCode.Z))
            heldBone.transform.Rotate(Vector3.right, -rotateSpeed * Time.deltaTime, Space.World);

        if (Input.GetKey(KeyCode.X))
            heldBone.transform.Rotate(Vector3.right, rotateSpeed * Time.deltaTime, Space.World);
    }

    private void DrawRayDebug()
    {
        if (!showDebugRay || playerCamera == null)
            return;

        Color rayColor = heldBone == null ? Color.red : Color.green;
        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * grabDistance, rayColor);
    }
}