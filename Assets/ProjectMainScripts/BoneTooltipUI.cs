using UnityEngine;
using TMPro;

public class BoneTooltipUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI titleText;

    private Camera targetCamera;

    private void Awake()
    {
        if (canvas == null)
            canvas = GetComponentInChildren<Canvas>();
    }

    public void Init(Camera camera)
    {
        targetCamera = camera;

        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = targetCamera;
        }
    }

    public void SetTitle(string title)
    {
        if (titleText != null)
            titleText.text = title;
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
            return;

        transform.rotation = Quaternion.LookRotation(
            transform.position - targetCamera.transform.position,
            targetCamera.transform.up
        );
    }
}