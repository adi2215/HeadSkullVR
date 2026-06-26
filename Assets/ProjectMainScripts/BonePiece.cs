using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BonePiece : MonoBehaviour
{
    [Header("Bone ID")]
    [SerializeField] private string boneID;

    [Header("Renderer")]
    [SerializeField] private Renderer boneRenderer;

    [Header("Materials")]
    [SerializeField] private Material wrongMaterial;

    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;
    private bool isPlaced;

    private readonly List<BoneSlot> slotsInside = new List<BoneSlot>();

    public string BoneID => boneID;
    public string NormalizedBoneID => NormalizeID(boneID);
    public bool IsPlaced => isPlaced;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (boneRenderer == null)
            boneRenderer = GetComponentInChildren<Renderer>();

        if (grabInteractable != null)
            grabInteractable.selectExited.AddListener(OnGrabReleased);
    }

    private void OnGrabReleased(SelectExitEventArgs args)
    {
        TryPlaceBone();
    }

    public void EnterSlot(BoneSlot slot)
    {
        if (isPlaced || slot == null)
            return;

        if (!slotsInside.Contains(slot))
            slotsInside.Add(slot);

        Debug.Log($"[BonePiece] {boneID} entered slot: {slot.SlotID}");
    }

    public void ExitSlot(BoneSlot slot)
    {
        if (isPlaced || slot == null)
            return;

        if (slotsInside.Contains(slot))
            slotsInside.Remove(slot);

        Debug.Log($"[BonePiece] {boneID} exited slot: {slot.SlotID}");
    }

    public void TryPlaceBone()
    {
        if (isPlaced)
            return;

        Debug.Log($"[BonePiece] Trying to place: {boneID}. Slots inside count: {slotsInside.Count}");

        for (int i = slotsInside.Count - 1; i >= 0; i--)
        {
            if (slotsInside[i] == null || slotsInside[i].IsOccupied)
                slotsInside.RemoveAt(i);
        }

        BoneSlot correctSlot = null;

        foreach (BoneSlot slot in slotsInside)
        {
            Debug.Log($"[BonePiece] Checking slot: BoneID={NormalizedBoneID}, SlotID={slot.NormalizedSlotID}, RawSlot={slot.SlotID}");

            if (slot.CanAcceptBone(this))
            {
                correctSlot = slot;
                break;
            }
        }

        if (correctSlot == null)
        {
            Debug.LogWarning($"[BonePiece] No correct slot found for: {boneID}");

            foreach (BoneSlot slot in slotsInside)
            {
                Debug.LogWarning($"[BonePiece] Nearby slot: {slot.SlotID} | Bone: {boneID} | SameID: {NormalizedBoneID == slot.NormalizedSlotID}");
            }

            if (boneRenderer != null && wrongMaterial != null)
                boneRenderer.material = wrongMaterial;

            return;
        }

        PlaceBone(correctSlot);
    }

    private void PlaceBone(BoneSlot slot)
    {
        isPlaced = true;

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if (grabInteractable != null)
            grabInteractable.enabled = false;

        slot.FillWithBone(this);

        Debug.Log("[BonePiece] Bone placed and removed: " + boneID);

        Destroy(gameObject);
    }

    public Material[] GetBoneMaterials()
    {
        if (boneRenderer == null)
            return null;

        return boneRenderer.sharedMaterials;
    }

    public static string NormalizeID(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return "";

        return id.Trim().ToLowerInvariant().Replace(" ", "").Replace("_", "").Replace("-", "");
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
            grabInteractable.selectExited.RemoveListener(OnGrabReleased);
    }
}