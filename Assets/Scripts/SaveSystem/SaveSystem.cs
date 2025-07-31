using System.IO;
using UnityEngine;

public static class SaveSystem
{
    //Uncomment for final build
    //private static readonly string basePath = Application.persistentDataPath;

    //For testing
    private static readonly string basePath = Path.Combine(Application.dataPath, "../Saves");

    /// <summary>
    /// Saves any serializable object to a file.
    /// </summary>
    public static void Save<T>(string fileName, T data)
    {
        string path = Path.Combine(basePath, fileName);
        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(path, json);
        Debug.Log($"[SaveManager] Saved to: {path}");
    }

    /// <summary>
    /// Loads a file and converts it back into an object of type T.
    /// Returns default(T) if file doesn't exist.
    /// </summary>
    public static T Load<T>(string fileName)
    {
        string path = Path.Combine(basePath, fileName);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveManager] File not found: {path}");
            return default;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<T>(json);
    }

    /// <summary>
    /// Deletes a saved file.
    /// </summary>
    public static void Delete(string fileName)
    {
        string path = Path.Combine(basePath, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[SaveManager] Deleted: {path}");
        }
    }

    /// <summary>
    /// Checks if a save file exists.
    /// </summary>
    public static bool Exists(string fileName)
    {
        return File.Exists(Path.Combine(basePath, fileName));
    }
}
