using UnityEngine;
using TMPro;

public class AssemblyManager : MonoBehaviour
{
    public static AssemblyManager Instance;

    [Header("Progress")]
    [SerializeField] private int totalBones = 3;
    [SerializeField] private TextMeshProUGUI progressText;

    private int placedBones;

    private void Awake()
    {
        Instance = this;
        UpdateUI();
    }

    public void AddPlacedBone()
    {
        placedBones++;

        if (placedBones > totalBones)
            placedBones = totalBones;

        UpdateUI();

        if (placedBones >= totalBones)
        {
            if (progressText != null)
                progressText.text = "Череп собран!";

            Debug.Log("[AssemblyManager] Skull complete!");
        }
    }

    private void UpdateUI()
    {
        if (progressText != null)
            progressText.text = $"Собрано: {placedBones}/{totalBones}";
    }
}