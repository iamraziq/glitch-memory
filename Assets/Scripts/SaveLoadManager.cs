using UnityEngine;

public static class SaveLoadManager
{
    private const string SaveKey = "CardMatchSave";

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(SaveKey);
    }

    public static void SaveGame(SaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public static SaveData LoadGame()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            string json = PlayerPrefs.GetString(SaveKey);
            return JsonUtility.FromJson<SaveData>(json);
        }
        return null;
    }

    public static void ClearSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save(); // Ensure it's written immediately
    }
}
