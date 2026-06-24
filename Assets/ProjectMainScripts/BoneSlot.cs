using UnityEngine;

public class BoneSlot : MonoBehaviour
{
    [Header("Slot ID")]
    [SerializeField] private string slotID;

    [Header("Target Bone Renderer")]
    [SerializeField] private Renderer slotRenderer;

    [Header("Preview Materials")]
    [SerializeField] private Material transparentMaterial;
    [SerializeField] private Material correctPreviewMaterial;
    [SerializeField] private Material wrongPreviewMaterial;

    private bool isOccupied;

    public string SlotID => slotID;
    public bool IsOccupied => isOccupied;

    private void Awake()
    {
        if (slotRenderer == null)
            slotRenderer = GetComponentInChildren<Renderer>();

        if (transparentMaterial != null)
            SetSingleMaterial(transparentMaterial);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isOccupied) return;

        BonePiece bone = other.GetComponentInParent<BonePiece>();

        if (bone == null || bone.IsPlaced)
            return;

        bone.SetCurrentSlot(this);

        if (bone.BoneID == slotID)
        {
            if (correctPreviewMaterial != null)
                SetSingleMaterial(correctPreviewMaterial);

            Debug.Log("[BoneSlot] Correct bone entered: " + bone.BoneID);
        }
        else
        {
            if (wrongPreviewMaterial != null)
                SetSingleMaterial(wrongPreviewMaterial);

            Debug.Log("[BoneSlot] Wrong bone entered: " + bone.BoneID);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isOccupied) return;

        BonePiece bone = other.GetComponentInParent<BonePiece>();

        if (bone == null || bone.IsPlaced)
            return;

        bone.ClearCurrentSlot(this);

        if (transparentMaterial != null)
            SetSingleMaterial(transparentMaterial);
    }

    public void FillWithBone(BonePiece bone)
    {
        if (isOccupied) return;

        isOccupied = true;

        Material[] boneMaterials = bone.GetBoneMaterials();

        if (slotRenderer != null && boneMaterials != null)
        {
            slotRenderer.sharedMaterials = boneMaterials;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        Debug.Log("[BoneSlot] Target filled: " + slotID);
    }

    private void SetSingleMaterial(Material mat)
    {
        if (slotRenderer != null && mat != null)
            slotRenderer.material = mat;
    }
}