using UnityEngine;

public class ScreenBoneShelf : MonoBehaviour
{
    public static bool IsPointerOverShelf { get; private set; }

    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Bones")]
    [SerializeField] private Transform[] bones;

    [Header("Grid")]
    [SerializeField] private int columns = 3;

    [Range(0f, 1f)]
    [SerializeField] private float startX = 0.68f;

    [Range(0f, 1f)]
    [SerializeField] private float startY = 0.78f;

    [SerializeField] private float xStep = 0.10f;
    [SerializeField] private float yStep = 0.13f;

    [Header("Shelf Area")]
    [Range(0f, 1f)]
    [SerializeField] private float areaMinX = 0.62f;

    [Range(0f, 1f)]
    [SerializeField] private float areaMaxX = 0.98f;

    [Range(0f, 1f)]
    [SerializeField] private float areaMinY = 0.12f;

    [Range(0f, 1f)]
    [SerializeField] private float areaMaxY = 0.88f;

    [Header("Scroll")]
    [SerializeField] private float scrollSpeed = 0.08f;

    [Header("Distance From Camera")]
    [SerializeField] private float depthFromCamera = 1.2f;

    [Header("Shelf Scale")]
    [SerializeField] private float shelfScale = 1f;

    [Header("Rotation")]
    [SerializeField] private Vector3 shelfRotation = Vector3.zero;

    private Vector3[] originalScales;
    private float scrollOffset;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        originalScales = new Vector3[bones.Length];

        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] != null)
                originalScales[i] = bones[i].localScale;
        }
    }

    private void LateUpdate()
    {
        UpdateMouseOverShelf();
        HandleScroll();
        UpdateShelfBones();
    }

    private void UpdateMouseOverShelf()
    {
        if (targetCamera == null)
            return;

        Vector3 mouseViewport = targetCamera.ScreenToViewportPoint(Input.mousePosition);

        IsPointerOverShelf =
            mouseViewport.x >= areaMinX &&
            mouseViewport.x <= areaMaxX &&
            mouseViewport.y >= areaMinY &&
            mouseViewport.y <= areaMaxY;
    }

    private void HandleScroll()
    {
        if (!IsPointerOverShelf)
            return;

        if (MouseBoneScreenDrag.CurrentHeldTransform != null)
            return;

        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) < 0.01f)
            return;

        int visibleBonesCount = CountVisibleBones();
        int totalRows = Mathf.CeilToInt((float)visibleBonesCount / columns);

        float visibleHeight = areaMaxY - areaMinY;
        float contentHeight = Mathf.Max(0f, (totalRows - 1) * yStep);
        float maxScroll = Mathf.Max(0f, contentHeight - visibleHeight + yStep);

        scrollOffset -= scroll * scrollSpeed;
        scrollOffset = Mathf.Clamp(scrollOffset, 0f, maxScroll);
    }

    private void UpdateShelfBones()
    {
        if (targetCamera == null)
            return;

        int visibleIndex = 0;

        for (int i = 0; i < bones.Length; i++)
        {
            Transform bone = bones[i];

            if (bone == null)
                continue;

            BonePiece bonePiece = bone.GetComponent<BonePiece>();

            if (bonePiece != null && bonePiece.IsPlaced)
            {
                SetBoneVisible(bone, false);
                continue;
            }

            if (MouseBoneScreenDrag.CurrentHeldTransform == bone)
            {
                SetBoneVisible(bone, true);
                continue;
            }

            // Когда кость на полке, делаем ее ребенком камеры.
            // Тогда она не будет дергаться при движении камеры.
            if (bone.parent != targetCamera.transform)
                bone.SetParent(targetCamera.transform, true);

            int row = visibleIndex / columns;
            int column = visibleIndex % columns;

            float x = startX + column * xStep;
            float y = startY - row * yStep + scrollOffset;

            bool insideShelfView =
                x >= areaMinX &&
                x <= areaMaxX &&
                y >= areaMinY &&
                y <= areaMaxY;

            SetBoneVisible(bone, insideShelfView);

            Vector3 localPosition = ViewportToCameraLocalPosition(x, y, depthFromCamera);

            bone.localPosition = localPosition;
            bone.localRotation = Quaternion.Euler(shelfRotation);

            if (originalScales != null && i < originalScales.Length)
                bone.localScale = originalScales[i] * shelfScale;

            visibleIndex++;
        }
    }

    private Vector3 ViewportToCameraLocalPosition(float viewportX, float viewportY, float depth)
    {
        float height = 2f * depth * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float width = height * targetCamera.aspect;

        float localX = (viewportX - 0.5f) * width;
        float localY = (viewportY - 0.5f) * height;
        float localZ = depth;

        return new Vector3(localX, localY, localZ);
    }

    private int CountVisibleBones()
    {
        int count = 0;

        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] == null)
                continue;

            BonePiece bonePiece = bones[i].GetComponent<BonePiece>();

            if (bonePiece != null && bonePiece.IsPlaced)
                continue;

            count++;
        }

        return count;
    }

    private void SetBoneVisible(Transform bone, bool visible)
    {
        Renderer[] renderers = bone.GetComponentsInChildren<Renderer>();
        Collider[] colliders = bone.GetComponentsInChildren<Collider>();

        foreach (Renderer renderer in renderers)
            renderer.enabled = visible;

        foreach (Collider collider in colliders)
            collider.enabled = visible;
    }
}