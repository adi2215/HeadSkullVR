using System.Collections.Generic;
using UnityEngine;

public enum BoneLanguage
{
    Russian,
    English,
    Kazakh
}

[System.Serializable]
public class SkullBoneInfo
{
    public string bone_id;
    public string group;
    public string is_paired;

    public string name_ru;
    public string name_kz;
    public string name_eng;
    public string name_lat;

    public string desc_ru;
    public string desc_kz;
    public string desc_eng;
}

public class SkullBoneDatabase : MonoBehaviour
{
    public static SkullBoneDatabase Instance;

    [Header("CSV File")]
    [SerializeField] private TextAsset csvFile;

    [Header("Language")]
    [SerializeField] private BoneLanguage language = BoneLanguage.Russian;

    private readonly Dictionary<string, SkullBoneInfo> bones = new Dictionary<string, SkullBoneInfo>();

    private void Awake()
    {
        Instance = this;
        LoadCSV();
    }

    public bool TryGetBoneInfo(string boneId, out SkullBoneInfo info)
    {
        string key = NormalizeID(boneId);

        if (bones.TryGetValue(key, out info))
            return true;

        Debug.LogWarning("[SkullBoneDatabase] Bone ID not found in CSV: " + boneId);
        return false;
    }

    public string GetTitle(SkullBoneInfo info)
    {
        if (language == BoneLanguage.English)
            return info.name_eng;

        if (language == BoneLanguage.Kazakh)
            return info.name_kz;

        return info.name_ru;
    }

    public string GetDescription(SkullBoneInfo info)
    {
        if (language == BoneLanguage.English)
            return info.desc_eng;

        if (language == BoneLanguage.Kazakh)
            return info.desc_kz;

        return info.desc_ru;
    }

    private void LoadCSV()
    {
        bones.Clear();

        if (csvFile == null)
        {
            Debug.LogError("[SkullBoneDatabase] CSV file is not assigned.");
            return;
        }

        string[] lines = csvFile.text.Split('\n');

        if (lines.Length <= 1)
        {
            Debug.LogError("[SkullBoneDatabase] CSV file is empty.");
            return;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            line = FixLine(line);

            List<string> values = ParseCSVLine(line);

            if (values.Count < 10)
            {
                Debug.LogWarning("[SkullBoneDatabase] Wrong CSV line: " + line);
                continue;
            }

            SkullBoneInfo info = new SkullBoneInfo
            {
                bone_id = values[0],
                group = values[1],
                is_paired = values[2],

                name_ru = values[3],
                name_kz = values[4],
                name_eng = values[5],
                name_lat = values[6],

                desc_ru = values[7],
                desc_kz = values[8],
                desc_eng = values[9]
            };

            string key = NormalizeID(info.bone_id);

            if (!bones.ContainsKey(key))
                bones.Add(key, info);
        }

        Debug.Log("[SkullBoneDatabase] Loaded bones: " + bones.Count);
    }

    private string FixLine(string line)
    {
        // Нужно для CSV формата вида:
        // "Frontal,""Neurocranium"",""False"",..."
        if (line.StartsWith("\"") && line.EndsWith("\""))
        {
            line = line.Substring(1, line.Length - 2);
            line = line.Replace("\"\"", "\"");
        }

        return line;
    }

    private List<string> ParseCSVLine(string line)
    {
        List<string> result = new List<string>();

        bool insideQuotes = false;
        string current = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                insideQuotes = !insideQuotes;
            }
            else if (c == ',' && !insideQuotes)
            {
                result.Add(current.Trim());
                current = "";
            }
            else
            {
                current += c;
            }
        }

        result.Add(current.Trim());

        return result;
    }

    public static string NormalizeID(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return "";

        return id
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", "")
            .Replace("_", "")
            .Replace("-", "");
    }
}