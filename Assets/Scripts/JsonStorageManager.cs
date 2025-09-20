using System;
using System.IO;
using UnityEngine;

/// <summary>
/// JSON文件存储管理器
/// 用于替代PlayerPrefs，提供更好的数据管理
/// </summary>
public static class JsonStorageManager
{
    private const string SAVE_FOLDER_NAME = "GameSave";
    private const string PROGRESS_FILE_NAME = "game_progress.json";
    private const string BACKUP_FILE_NAME = "game_progress_backup.json";
    
    // 存储路径
    private static string SaveDirectory => Path.Combine(Application.persistentDataPath, SAVE_FOLDER_NAME);
    private static string ProgressFilePath => Path.Combine(SaveDirectory, PROGRESS_FILE_NAME);
    private static string BackupFilePath => Path.Combine(SaveDirectory, BACKUP_FILE_NAME);
    
    /// <summary>
    /// 确保保存目录存在
    /// </summary>
    private static void EnsureSaveDirectoryExists()
    {
        if (!Directory.Exists(SaveDirectory))
        {
            try
            {
                Directory.CreateDirectory(SaveDirectory);
                Debug.Log($"[JsonStorageManager] 创建保存目录: {SaveDirectory}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonStorageManager] 创建保存目录失败: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// 保存游戏进度数据
    /// </summary>
    /// <param name="progressData">进度数据</param>
    /// <returns>是否保存成功</returns>
    public static bool SaveGameProgress(GameProgressData progressData)
    {
        if (progressData == null)
        {
            Debug.LogError("[JsonStorageManager] 进度数据为空，无法保存");
            return false;
        }
        
        try
        {
            EnsureSaveDirectoryExists();
            
            // 更新保存时间
            progressData.UpdateSaveTime();
            
            // 序列化为JSON
            string jsonData = JsonUtility.ToJson(progressData, true);
            
            // 如果文件已存在，先备份
            if (File.Exists(ProgressFilePath))
            {
                File.Copy(ProgressFilePath, BackupFilePath, true);
            }
            
            // 写入新数据
            File.WriteAllText(ProgressFilePath, jsonData);
            
            Debug.Log($"[JsonStorageManager] 游戏进度保存成功: {ProgressFilePath}");
            Debug.Log($"[JsonStorageManager] 进度摘要: {progressData.GetProgressSummary()}");
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[JsonStorageManager] 保存游戏进度失败: {e.Message}");
            
            // 尝试从备份恢复
            RestoreFromBackup();
            
            return false;
        }
    }
    
    /// <summary>
    /// 加载游戏进度数据
    /// </summary>
    /// <returns>进度数据，如果加载失败返回新的空数据</returns>
    public static GameProgressData LoadGameProgress()
    {
        try
        {
            // 检查主文件是否存在
            if (File.Exists(ProgressFilePath))
            {
                string jsonData = File.ReadAllText(ProgressFilePath);
                GameProgressData progressData = JsonUtility.FromJson<GameProgressData>(jsonData);
                
                if (progressData != null && progressData.IsValid())
                {
                    Debug.Log($"[JsonStorageManager] 游戏进度加载成功: {ProgressFilePath}");
                    Debug.Log($"[JsonStorageManager] 进度摘要: {progressData.GetProgressSummary()}");
                    return progressData;
                }
                else
                {
                    Debug.LogWarning("[JsonStorageManager] 进度数据无效，尝试从备份恢复");
                    return LoadFromBackup();
                }
            }
            else
            {
                Debug.Log("[JsonStorageManager] 进度文件不存在，创建新的进度数据");
                return new GameProgressData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[JsonStorageManager] 加载游戏进度失败: {e.Message}");
            
            // 尝试从备份恢复
            return LoadFromBackup();
        }
    }
    
    /// <summary>
    /// 从备份文件加载数据
    /// </summary>
    private static GameProgressData LoadFromBackup()
    {
        try
        {
            if (File.Exists(BackupFilePath))
            {
                string jsonData = File.ReadAllText(BackupFilePath);
                GameProgressData progressData = JsonUtility.FromJson<GameProgressData>(jsonData);
                
                if (progressData != null && progressData.IsValid())
                {
                    Debug.Log($"[JsonStorageManager] 从备份文件恢复成功: {BackupFilePath}");
                    
                    // 恢复主文件
                    File.Copy(BackupFilePath, ProgressFilePath, true);
                    
                    return progressData;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[JsonStorageManager] 从备份文件恢复失败: {e.Message}");
        }
        
        Debug.Log("[JsonStorageManager] 备份恢复失败，创建新的进度数据");
        return new GameProgressData();
    }
    
    /// <summary>
    /// 从备份文件恢复
    /// </summary>
    private static void RestoreFromBackup()
    {
        try
        {
            if (File.Exists(BackupFilePath))
            {
                File.Copy(BackupFilePath, ProgressFilePath, true);
                Debug.Log("[JsonStorageManager] 已从备份文件恢复");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[JsonStorageManager] 从备份文件恢复失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 删除游戏进度数据
    /// </summary>
    /// <returns>是否删除成功</returns>
    public static bool DeleteGameProgress()
    {
        try
        {
            bool deleted = false;
            
            if (File.Exists(ProgressFilePath))
            {
                File.Delete(ProgressFilePath);
                deleted = true;
                Debug.Log("[JsonStorageManager] 主进度文件已删除");
            }
            
            if (File.Exists(BackupFilePath))
            {
                File.Delete(BackupFilePath);
                deleted = true;
                Debug.Log("[JsonStorageManager] 备份进度文件已删除");
            }
            
            if (!deleted)
            {
                Debug.Log("[JsonStorageManager] 没有找到要删除的进度文件");
            }
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[JsonStorageManager] 删除游戏进度失败: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 检查游戏进度是否存在
    /// </summary>
    /// <returns>是否存在进度文件</returns>
    public static bool HasGameProgress()
    {
        return File.Exists(ProgressFilePath) || File.Exists(BackupFilePath);
    }
    
    /// <summary>
    /// 获取存储路径信息（用于调试）
    /// </summary>
    public static string GetStorageInfo()
    {
        return $"保存目录: {SaveDirectory}\n" +
               $"主文件: {ProgressFilePath}\n" +
               $"备份文件: {BackupFilePath}\n" +
               $"主文件存在: {File.Exists(ProgressFilePath)}\n" +
               $"备份文件存在: {File.Exists(BackupFilePath)}";
    }
    
    /// <summary>
    /// 导出进度数据到指定路径
    /// </summary>
    /// <param name="exportPath">导出路径</param>
    /// <returns>是否导出成功</returns>
    public static bool ExportProgress(string exportPath)
    {
        try
        {
            if (File.Exists(ProgressFilePath))
            {
                File.Copy(ProgressFilePath, exportPath, true);
                Debug.Log($"[JsonStorageManager] 进度数据导出成功: {exportPath}");
                return true;
            }
            else
            {
                Debug.LogWarning("[JsonStorageManager] 没有找到进度文件，无法导出");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[JsonStorageManager] 导出进度数据失败: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 从指定路径导入进度数据
    /// </summary>
    /// <param name="importPath">导入路径</param>
    /// <returns>是否导入成功</returns>
    public static bool ImportProgress(string importPath)
    {
        try
        {
            if (File.Exists(importPath))
            {
                // 先备份当前数据
                if (File.Exists(ProgressFilePath))
                {
                    File.Copy(ProgressFilePath, BackupFilePath, true);
                }
                
                // 导入新数据
                File.Copy(importPath, ProgressFilePath, true);
                
                Debug.Log($"[JsonStorageManager] 进度数据导入成功: {importPath}");
                return true;
            }
            else
            {
                Debug.LogError($"[JsonStorageManager] 导入文件不存在: {importPath}");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[JsonStorageManager] 导入进度数据失败: {e.Message}");
            
            // 尝试从备份恢复
            RestoreFromBackup();
            
            return false;
        }
    }
}
