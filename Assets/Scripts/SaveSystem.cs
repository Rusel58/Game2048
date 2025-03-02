using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    private static readonly string FILE_NAME = "save.dat";

    private static readonly string SAVE_FOLDER = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "SaveData"));

    public static void SaveGame(SaveData data)
    {
        try
        {
            // Создаём папку, если её нет
            if (!Directory.Exists(SAVE_FOLDER))
                Directory.CreateDirectory(SAVE_FOLDER);

            string path = Path.Combine(SAVE_FOLDER, FILE_NAME);

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, data);
            }

            Debug.Log($"Game saved at: {Path.GetFullPath(path)}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Save failed! Error: {ex.Message}");
        }
    }

    public static SaveData LoadGame()
    {
        string path = Path.Combine(SAVE_FOLDER, FILE_NAME);

        if (!File.Exists(path))
        {
            Debug.LogWarning("Save file not found!");
            return null;
        }

        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                SaveData data = formatter.Deserialize(fs) as SaveData;
                Debug.Log($"Game loaded from: {Path.GetFullPath(path)}");
                return data;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Load failed! Error: {ex.Message}");
            return null;
        }
    }
}
