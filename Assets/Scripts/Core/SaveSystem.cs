using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private const string SaveFileName = "mascotchi_save.json";
    private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public static bool SaveExists() => File.Exists(SavePath);

    public static SaveData Load()
    {
        if (!SaveExists())
        {
            Debug.LogWarning("[SaveSystem] No save file found.");
            return null;
        }
        SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
        Debug.Log($"[SaveSystem] Loaded save from {SavePath}");
        return data;
    }

    public static void Save(SaveData data)
    {
        data.lastSessionTimestamp = System.DateTime.UtcNow.ToString("O");
        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        Debug.Log($"[SaveSystem] Saved to {SavePath}");
    }

    public static void DeleteSave()
    {
        if (SaveExists())
        {
            File.Delete(SavePath);
            Debug.Log("[SaveSystem] Save file deleted.");
        }
    }
}