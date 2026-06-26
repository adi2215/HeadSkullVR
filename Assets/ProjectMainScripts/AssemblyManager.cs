using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AssemblyManager : MonoBehaviour
{
    public static AssemblyManager Instance;

    [Header("Progress")]
    [SerializeField] private int totalBones = 3;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Assembly Rules")]
    [SerializeField] private bool firstBoneCanBeAny = true;

    private int placedBones;

    private readonly HashSet<BoneSlot> placedSlots = new HashSet<BoneSlot>();
    private readonly HashSet<BoneSlot> unlockedSlots = new HashSet<BoneSlot>();

    public int PlacedBones => placedBones;

    private void Awake()
    {
        Instance = this;
        UpdateUI();
    }

    public bool CanPlaceInSlot(BoneSlot slot)
    {
        if (slot == null)
            return false;

        if (slot.IsOccupied)
            return false;

        if (placedBones == 0 && firstBoneCanBeAny)
            return true;

        return unlockedSlots.Contains(slot);
    }

    public void RegisterPlacedSlot(BoneSlot placedSlot)
    {
        if (placedSlot == null)
            return;

        if (placedSlots.Contains(placedSlot))
            return;

        placedSlots.Add(placedSlot);

        placedBones++;

        if (placedBones > totalBones)
            placedBones = totalBones;

        foreach (BoneSlot neighbor in placedSlot.NeighborSlots)
        {
            if (neighbor == null)
                continue;

            if (!neighbor.IsOccupied)
            {
                unlockedSlots.Add(neighbor);
                neighbor.SetUnlockedVisual();
            }
        }

        UpdateUI();

        if (placedBones >= totalBones)
        {
            if (progressText != null)
                progressText.text = "Skull Assembled!";

            Debug.Log("[AssemblyManager] Skull complete!");
        }
    }

    private void UpdateUI()
    {
        if (progressText != null)
            progressText.text = $"Collected: {placedBones}/{totalBones}";
    }
}