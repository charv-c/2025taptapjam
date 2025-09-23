using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 提供用于存档系统的编辑器工具，方便测试。
/// </summary>
public static class SaveSystemTools
{
    private const string SAVE_FOLDER_NAME = "GameSave";
    private const string PROGRESS_FILE_NAME = "game_progress.json";

    /// <summary>
    /// 获取存档文件所在的完整目录路径。
    /// </summary>
    private static string SaveDirectory => Path.Combine(Application.persistentDataPath, SAVE_FOLDER_NAME);
    
    /// <summary>
    /// 获取主存档文件的完整路径。
    /// </summary>
    private static string ProgressFilePath => Path.Combine(SaveDirectory, PROGRESS_FILE_NAME);

    /// <summary>
    /// 在文件浏览器中打开存档文件所在的文件夹。
    /// </summary>
    [MenuItem("工具/存档系统/打开存档位置")]
    public static void OpenSaveLocation()
    {
        // 如果目录不存在，则先创建，避免打开一个不存在的路径
        if (!Directory.Exists(SaveDirectory))
        {
            Directory.CreateDirectory(SaveDirectory);
            Debug.Log($"存档目录尚不存在，已在以下位置创建: {SaveDirectory}");
        }
        
        // 在Finder(macOS)或Explorer(Windows)中显示该文件夹
        EditorUtility.RevealInFinder(ProgressFilePath);
    }

    /// <summary>
    /// 删除所有存档文件（包括备份），以便模拟新玩家状态。
    /// </summary>
    [MenuItem("工具/存档系统/删除全部存档")]
    public static void DeleteAllSaves()
    {
        // 显示一个确认对话框，防止误操作
        if (EditorUtility.DisplayDialog(
            "确认删除存档",
            $"你确定要删除所有游戏存档吗？\n此操作不可撤销。\n\n存档位置: {SaveDirectory}",
            "确认删除",
            "取消"))
        {
            // 调用现有的存档管理器逻辑来删除文件，确保行为一致
            if (JsonStorageManager.DeleteGameProgress())
            {
                Debug.Log("存档已成功删除。");
            }
            else
            {
                Debug.Log("没有找到存档文件，或删除过程中发生错误。");
            }
        }
    }

    /// <summary>
    /// 在Unity控制台中打印出完整的存档文件路径。
    /// </summary>
    [MenuItem("工具/存档系统/打印存档路径到控制台")]
    public static void PrintSavePath()
    {
        Debug.Log($"存档文件路径: {ProgressFilePath}");
        // 同时打印目录路径，方便访问
        Debug.Log($"存档文件夹路径: {SaveDirectory}");
    }
}
