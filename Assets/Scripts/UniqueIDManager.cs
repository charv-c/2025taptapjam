using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 唯一ID管理器 - 管理场景中所有UniqueID组件，确保ID的唯一性
/// </summary>
public static class UniqueIDManager
{
    // 存储所有注册的UniqueID组件
    private static Dictionary<string, UniqueID> registeredObjects = new Dictionary<string, UniqueID>();
    
    // 调试设置
    private static bool enableDebugLog = false;
    
    /// <summary>
    /// 注册UniqueID组件
    /// </summary>
    /// <param name="uniqueID">要注册的UniqueID组件</param>
    public static void RegisterObject(UniqueID uniqueID)
    {
        if (uniqueID == null || !uniqueID.HasValidID)
        {
            LogWarning("尝试注册无效的UniqueID组件");
            return;
        }
        
        string id = uniqueID.ID;
        
        // 检查ID冲突
        if (registeredObjects.ContainsKey(id))
        {
            var existingObject = registeredObjects[id];
            
            // 如果现有对象已被销毁，替换为新对象
            if (existingObject == null)
            {
                registeredObjects[id] = uniqueID;
                LogDebug($"替换已销毁对象的ID: {id}");
            }
            else
            {
                // ID冲突，为新对象生成新ID
                LogWarning($"ID冲突检测: {id} 已被 {existingObject.gameObject.name} 使用，为 {uniqueID.gameObject.name} 生成新ID");
                uniqueID.GenerateNewID();
                RegisterObject(uniqueID); // 递归注册新ID
                return;
            }
        }
        else
        {
            registeredObjects[id] = uniqueID;
            LogDebug($"注册对象: {uniqueID.gameObject.name} -> {id}");
        }
    }
    
    /// <summary>
    /// 注销UniqueID组件
    /// </summary>
    /// <param name="uniqueID">要注销的UniqueID组件</param>
    public static void UnregisterObject(UniqueID uniqueID)
    {
        if (uniqueID == null || !uniqueID.HasValidID)
        {
            return;
        }
        
        string id = uniqueID.ID;
        
        if (registeredObjects.ContainsKey(id) && registeredObjects[id] == uniqueID)
        {
            registeredObjects.Remove(id);
            LogDebug($"注销对象: {uniqueID.gameObject.name} -> {id}");
        }
    }
    
    /// <summary>
    /// 根据ID查找GameObject
    /// </summary>
    /// <param name="id">唯一标识符</param>
    /// <returns>对应的GameObject，如果未找到返回null</returns>
    public static GameObject FindGameObjectByID(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }
        
        if (registeredObjects.TryGetValue(id, out UniqueID uniqueID))
        {
            // 检查对象是否仍然有效
            if (uniqueID != null && uniqueID.gameObject != null)
            {
                return uniqueID.gameObject;
            }
            else
            {
                // 清理无效的引用
                registeredObjects.Remove(id);
                LogDebug($"清理无效引用: {id}");
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 根据ID查找UniqueID组件
    /// </summary>
    /// <param name="id">唯一标识符</param>
    /// <returns>对应的UniqueID组件，如果未找到返回null</returns>
    public static UniqueID FindUniqueIDByID(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }
        
        if (registeredObjects.TryGetValue(id, out UniqueID uniqueID))
        {
            // 检查组件是否仍然有效
            if (uniqueID != null)
            {
                return uniqueID;
            }
            else
            {
                // 清理无效的引用
                registeredObjects.Remove(id);
                LogDebug($"清理无效引用: {id}");
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 验证ID的唯一性
    /// </summary>
    /// <param name="id">要验证的ID</param>
    /// <param name="requester">请求验证的UniqueID组件</param>
    /// <returns>ID是否唯一</returns>
    public static bool ValidateIDUniqueness(string id, UniqueID requester)
    {
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }
        
        if (!registeredObjects.ContainsKey(id))
        {
            return true; // ID未被使用，是唯一的
        }
        
        // 检查是否是同一个对象
        return registeredObjects[id] == requester;
    }
    
    /// <summary>
    /// 获取所有注册的ID
    /// </summary>
    /// <returns>所有注册的ID列表</returns>
    public static List<string> GetAllRegisteredIDs()
    {
        // 清理无效引用
        CleanupInvalidReferences();
        
        return registeredObjects.Keys.ToList();
    }
    
    /// <summary>
    /// 获取注册对象的数量
    /// </summary>
    /// <returns>注册对象数量</returns>
    public static int GetRegisteredObjectCount()
    {
        CleanupInvalidReferences();
        return registeredObjects.Count;
    }
    
    /// <summary>
    /// 清理无效的引用
    /// </summary>
    public static void CleanupInvalidReferences()
    {
        var keysToRemove = new List<string>();
        
        foreach (var kvp in registeredObjects)
        {
            if (kvp.Value == null || kvp.Value.gameObject == null)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        
        foreach (string key in keysToRemove)
        {
            registeredObjects.Remove(key);
        }
        
        if (keysToRemove.Count > 0)
        {
            LogDebug($"清理了 {keysToRemove.Count} 个无效引用");
        }
    }
    
    /// <summary>
    /// 清空所有注册的对象（场景切换时使用）
    /// </summary>
    public static void ClearAllRegistrations()
    {
        int count = registeredObjects.Count;
        registeredObjects.Clear();
        LogDebug($"清空所有注册对象: {count} 个");
    }
    
    /// <summary>
    /// 获取调试信息
    /// </summary>
    /// <returns>调试信息字符串</returns>
    public static string GetDebugInfo()
    {
        CleanupInvalidReferences();
        
        var info = new System.Text.StringBuilder();
        info.AppendLine($"UniqueIDManager 调试信息:");
        info.AppendLine($"注册对象数量: {registeredObjects.Count}");
        
        foreach (var kvp in registeredObjects)
        {
            var obj = kvp.Value;
            if (obj != null && obj.gameObject != null)
            {
                info.AppendLine($"  {kvp.Key} -> {obj.gameObject.name}");
            }
        }
        
        return info.ToString();
    }
    
    /// <summary>
    /// 设置调试日志开关
    /// </summary>
    /// <param name="enabled">是否启用调试日志</param>
    public static void SetDebugLog(bool enabled)
    {
        enableDebugLog = enabled;
    }
    
    /// <summary>
    /// 调试日志
    /// </summary>
    private static void LogDebug(string message)
    {
        if (enableDebugLog)
        {
            GameLogger.LogDev($"[UniqueIDManager] {message}");
        }
    }
    
    /// <summary>
    /// 警告日志
    /// </summary>
    private static void LogWarning(string message)
    {
        GameLogger.LogError($"[UniqueIDManager] {message}");
    }
}
