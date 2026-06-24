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
    private BoneSlot currentSlot;
    private bool isPlaced;

    public string BoneID => boneID;
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

    public void SetCurrentSlot(BoneSlot slot)
    {
        if (isPlaced) return;
        currentSlot = slot;
    }

    public void ClearCurrentSlot(BoneSlot slot)
    {
        if (isPlaced) return;

        if (currentSlot == slot)
            currentSlot = null;
    }

    public void TryPlaceBone()
    {
        if (isPlaced) return;

        if (currentSlot == null)
        {
            Debug.Log("[BonePiece] No target bone detected: " + boneID);
            return;
        }

        if (currentSlot.IsOccupied)
        {
            Debug.Log("[BonePiece] Target already occupied: " + currentSlot.SlotID);
            return;
        }

        if (boneID == currentSlot.SlotID)
        {
            PlaceBone();
        }
        else
        {
            Debug.Log("[BonePiece] Wrong target. Bone: " + boneID + " Target: " + currentSlot.SlotID);

            if (boneRenderer != null && wrongMaterial != null)
                boneRenderer.material = wrongMaterial;
        }
    }

    private void PlaceBone()
    {
        isPlaced = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if (grabInteractable != null)
            grabInteractable.enabled = false;

        // Главное: передаем материал этой кости в прозрачную кость
        currentSlot.FillWithBone(this);

        if (AssemblyManager.Instance != null)
            AssemblyManager.Instance.AddPlacedBone();

        Debug.Log("[BonePiece] Bone placed and removed: " + boneID);

        Destroy(gameObject);
    }

    public Material[] GetBoneMaterials()
    {
        if (boneRenderer == null)
            return null;

        return boneRenderer.sharedMaterials;
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
            grabInteractable.selectExited.RemoveListener(OnGrabReleased);
    }
}