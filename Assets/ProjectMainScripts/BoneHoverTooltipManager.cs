using UnityEngine;

public class BoneHoverTooltipManager : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Raycast")]
    [SerializeField] private LayerMask boneLayer;
    [SerializeField] private float rayDistance = 100f;

    [Header("Tooltip")]
    [SerializeField] private BoneTooltipUI tooltipPrefab;

    [Header("Description Panel")]
    [SerializeField] private BoneDescriptionPanel descriptionPanel;

    [Header("Position")]
    [SerializeField] private bool showOnLeftSide = true;
    [SerializeField] private float sideOffset = 0.45f;
    [SerializeField] private float verticalOffset = 0.12f;
    [SerializeField] private float followSmooth = 25f;

    [Header("Screen Clamp")]
    [SerializeField] private bool clampToScreen = true;
    [SerializeField] private float screenPadding = 0.08f;

    [Header("Options")]
    [SerializeField] private bool hideWhenDraggingBone = true;

    private BoneTooltipUI tooltipInstance;
    private string currentBoneID;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Update()
    {
        if (targetCamera == null || tooltipPrefab == null)
            return;

        if (hideWhenDraggingBone && MouseBoneScreenDrag.CurrentHeldTransform != null)
        {
            HideTooltip();
            return;
        }

        Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, boneLayer, QueryTriggerInteraction.Collide))
        {
            BonePiece bonePiece = hit.collider.GetComponentInParent<BonePiece>();

            if (bonePiece == null)
            {
                HideTooltip();
                return;
            }

            ShowTooltip(bonePiece, hit);
        }
        else
        {
            HideTooltip();
        }
    }

    private void ShowTooltip(BonePiece bonePiece, RaycastHit hit)
    {
        if (SkullBoneDatabase.Instance == null)
        {
            Debug.LogWarning("[BoneHoverTooltipManager] SkullBoneDatabase is missing in scene.");
            HideTooltip();
            return;
        }

        string boneID = bonePiece.BoneID;

        if (!SkullBoneDatabase.Instance.TryGetBoneInfo(boneID, out SkullBoneInfo info))
        {
            HideTooltip();
            return;
        }

        if (tooltipInstance == null)
        {
            tooltipInstance = Instantiate(tooltipPrefab);
            tooltipInstance.Init(targetCamera);
            tooltipInstance.gameObject.SetActive(false);
        }

        if (!tooltipInstance.gameObject.activeSelf)
            tooltipInstance.gameObject.SetActive(true);

        if (currentBoneID != boneID)
        {
            currentBoneID = boneID;

            string title = SkullBoneDatabase.Instance.GetTitle(info);
            string description = SkullBoneDatabase.Instance.GetDescription(info);

            // Маленький World Space UI рядом с костью
            tooltipInstance.SetTitle(title);

            // Отдельный UI Canvas на сцене
            if (descriptionPanel != null)
                descriptionPanel.SetInfo(title, description);
        }

        Vector3 targetPosition = GetTooltipPosition(bonePiece.transform, hit);

        tooltipInstance.transform.position = Vector3.Lerp(
            tooltipInstance.transform.position,
            targetPosition,
            Time.deltaTime * followSmooth
        );
    }

    private Vector3 GetTooltipPosition(Transform boneTransform, RaycastHit hit)
    {
        Renderer renderer = boneTransform.GetComponentInChildren<Renderer>();

        Vector3 basePosition;

        if (renderer != null)
            basePosition = renderer.bounds.center;
        else
            basePosition = hit.point;

        Vector3 sideDirection = showOnLeftSide
            ? -targetCamera.transform.right
            : targetCamera.transform.right;

        Vector3 targetPosition =
            basePosition +
            sideDirection * sideOffset +
            targetCamera.transform.up * verticalOffset;

        if (clampToScreen)
            targetPosition = ClampWorldPositionToScreen(targetPosition);

        return targetPosition;
    }

    private Vector3 ClampWorldPositionToScreen(Vector3 worldPosition)
    {
        Vector3 viewportPoint = targetCamera.WorldToViewportPoint(worldPosition);

        viewportPoint.x = Mathf.Clamp(viewportPoint.x, screenPadding, 1f - screenPadding);
        viewportPoint.y = Mathf.Clamp(viewportPoint.y, screenPadding, 1f - screenPadding);

        return targetCamera.ViewportToWorldPoint(viewportPoint);
    }

    private void HideTooltip()
    {
        currentBoneID = "";

        if (tooltipInstance != null && tooltipInstance.gameObject.activeSelf)
            tooltipInstance.gameObject.SetActive(false);

        if (descriptionPanel != null)
            descriptionPanel.Hide();
    }
}