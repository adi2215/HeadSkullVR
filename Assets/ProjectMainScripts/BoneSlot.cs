using UnityEngine;

public class BoneSlot : MonoBehaviour
{
    [Header("Slot ID")]
    [SerializeField] private string slotID;

    [Header("Target Bone Renderer")]
    [SerializeField] private Renderer slotRenderer;

    [Header("Neighbor Slots")]
    [SerializeField] private BoneSlot[] neighborSlots;

    [Header("Materials")]
    [SerializeField] private Material transparentMaterial;
    [SerializeField] private Material unlockedMaterial;
    [SerializeField] private Material correctPreviewMaterial;
    [SerializeField] private Material wrongPreviewMaterial;
    [SerializeField] private Material lockedMaterial;

    private bool isOccupied;

    public string SlotID => slotID;
    public string NormalizedSlotID => BonePiece.NormalizeID(slotID);
    public bool IsOccupied => isOccupied;
    public BoneSlot[] NeighborSlots => neighborSlots;

    private void Awake()
    {
        if (slotRenderer == null)
            slotRenderer = GetComponentInChildren<Renderer>();

        if (transparentMaterial != null)
            SetSingleMaterial(transparentMaterial);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isOccupied)
            return;

        BonePiece bone = other.GetComponentInParent<BonePiece>();

        if (bone == null || bone.IsPlaced)
            return;

        bone.EnterSlot(this);

        bool correctID = bone.NormalizedBoneID == NormalizedSlotID;
        bool canPlaceByRule = AssemblyManager.Instance == null || AssemblyManager.Instance.CanPlaceInSlot(this);

        Debug.Log($"[BoneSlot] ENTER Slot={slotID}, Bone={bone.BoneID}, CorrectID={correctID}, CanRule={canPlaceByRule}");

        if (correctID && canPlaceByRule)
        {
            if (correctPreviewMaterial != null)
                SetSingleMaterial(correctPreviewMaterial);
        }
        else
        {
            if (wrongPreviewMaterial != null)
                SetSingleMaterial(wrongPreviewMaterial);

            if (!correctID)
                Debug.LogWarning($"[BoneSlot] Wrong ID. Bone='{bone.BoneID}' Slot='{slotID}'");

            if (!canPlaceByRule)
                Debug.LogWarning("[BoneSlot] Locked by neighbor rule: " + slotID);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isOccupied)
            return;

        BonePiece bone = other.GetComponentInParent<BonePiece>();

        if (bone == null || bone.IsPlaced)
            return;

        bone.ExitSlot(this);

        bool isUnlocked = AssemblyManager.Instance != null && AssemblyManager.Instance.CanPlaceInSlot(this);

        if (isUnlocked && unlockedMaterial != null)
            SetSingleMaterial(unlockedMaterial);
        else if (transparentMaterial != null)
            SetSingleMaterial(transparentMaterial);
    }

    public bool CanAcceptBone(BonePiece bone)
    {
        if (bone == null)
            return false;

        if (isOccupied)
            return false;

        if (bone.NormalizedBoneID != NormalizedSlotID)
            return false;

        if (AssemblyManager.Instance != null)
            return AssemblyManager.Instance.CanPlaceInSlot(this);

        return true;
    }

    public void FillWithBone(BonePiece bone)
    {
        if (!CanAcceptBone(bone))
        {
            if (wrongPreviewMaterial != null)
                SetSingleMaterial(wrongPreviewMaterial);

            Debug.LogWarning("[BoneSlot] Cannot fill slot: " + slotID);
            return;
        }

        isOccupied = true;

        Material[] boneMaterials = bone.GetBoneMaterials();

        if (slotRenderer != null && boneMaterials != null)
            slotRenderer.sharedMaterials = boneMaterials;

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        if (AssemblyManager.Instance != null)
            AssemblyManager.Instance.RegisterPlacedSlot(this);

        Debug.Log("[BoneSlot] Filled: " + slotID);
    }

    public void SetUnlockedVisual()
    {
        if (isOccupied) return;

        if (unlockedMaterial != null)
            SetSingleMaterial(unlockedMaterial);
    }

    public void SetOccupiedVisual()
    {
        // Материал уже меняется на материал настоящей кости.
    }

    private void SetSingleMaterial(Material mat)
    {
        if (slotRenderer != null && mat != null)
            slotRenderer.material = mat;
    }
}