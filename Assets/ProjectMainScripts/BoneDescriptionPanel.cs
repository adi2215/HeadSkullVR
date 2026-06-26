using UnityEngine;
using TMPro;

public class BoneDescriptionPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Settings")]
    [SerializeField] private bool hideWhenEmpty = true;

    private void Awake()
    {
        Hide();
    }

    public void SetInfo(string title, string description)
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);

        if (descriptionText != null)
            descriptionText.text = description;
    }

    public void Hide()
    {
        if (!hideWhenEmpty)
            return;

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }
}