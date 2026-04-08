using UnityEngine;
using System.IO;

/// <summary>
/// 存档系统 - JSON序列化
/// </summary>
public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static void Save(PlayerData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[SaveSystem] 存档保存至: {SavePath}");
    }

    public static PlayerData Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("[SaveSystem] 无存档，创建新数据");
            return new PlayerData();
        }

        string json = File.ReadAllText(SavePath);
        var data = JsonUtility.FromJson<PlayerData>(json);
        Debug.Log("[SaveSystem] 存档加载成功");
        return data;
    }

    public static void Delete()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("[SaveSystem] 存档已删除");
        }
    }
}
