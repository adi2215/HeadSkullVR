using UnityEngine;

public class BoneNameData : MonoBehaviour
{
    [Header("Bone Name")]
    [SerializeField] private string displayName;

    public string DisplayName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(displayName))
                return displayName;

            return CleanName(gameObject.name);
        }
    }

    private string CleanName(string rawName)
    {
        return rawName
            .Replace("(Clone)", "")
            .Replace("_", " ")
            .Replace("-", " ")
            .Trim();
    }
}