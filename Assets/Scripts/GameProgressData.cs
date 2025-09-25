using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 游戏进度数据类，用于JSON序列化存储
/// </summary>
[System.Serializable]
public class GameProgressData
{
    [Header("基础进度信息")]
    public string currentLevel = "";
    public List<string> completedLevels = new List<string>();
    public bool gameStarted = false;
    
    [Header("关卡状态数据")]
    // Unity JsonUtility 不支持 Dictionary，使用 List 代替
    public List<LevelStateEntry> levelStates = new List<LevelStateEntry>();
    
    [Header("时间戳")]
    public string lastSaveTime = "";
    public string createTime = "";
    
    [Header("版本信息")]
    public string version = "1.0"; // 游戏版本
    public string saveVersion = "2.0"; // 存档格式版本（支持UniqueID和新数据结构）
    
    // 版本兼容性常量
    public const string CURRENT_SAVE_VERSION = "2.0";
    public const string LEGACY_SAVE_VERSION = "1.0";
    public const string MIN_COMPATIBLE_VERSION = "1.0";
    
    /// <summary>
    /// 关卡状态条目（用于序列化）
    /// </summary>
    [System.Serializable]
    public class LevelStateEntry
    {
        public string levelName;
        public LevelStateData stateData;
        
        public LevelStateEntry() { }
        
        public LevelStateEntry(string levelName, LevelStateData stateData)
        {
            this.levelName = levelName;
            this.stateData = stateData;
        }
    }
    
    /// <summary>
    /// 关卡状态数据类 (从GameStateManager迁移而来，作为权威模型)
    /// </summary>
    [System.Serializable]
    public class LevelStateData
    {
        public string levelName;
        // Level内输入/引导等通用状态
        public bool guideCompleted;
        public List<GameObjectState> objectStates;
        public List<string> broadcastHistory;
        public List<string> availableStrings;
        public string currentSeason;
        public List<string> collectedStrings;
        public List<FlyingCharacterData> flyingCharacters;
        public List<string> completedTargets;
        public List<string> currentTargetList;
        public BeachObjectState beachObjectState;
        public float saveTime;
    }

    /// <summary>
    /// 游戏对象状态数据 (从GameStateManager迁移而来)
    /// </summary>
    [System.Serializable]
    public class GameObjectState
    {
        public string objectName;
        public string objectPath; // 保留作为后备，但优先使用uniqueId
        public string uniqueId; // GUID标识符，优先使用
        public bool isActive; 
        public bool isActiveSelf;
        public bool hasHighlight;
        public bool highlightEnabled;
        public string highlightLetter;
        public Vector3 position;
        public bool hasSpriteRenderer;
        public bool spriteRendererEnabled;
        public bool hasCollider2D;
        public bool collider2DEnabled;
        public bool hasRenderer;
        public bool rendererEnabled;
        public bool hasLight2D;
        public bool light2DEnabled;
        public bool hasPlayer;
        public string playerCarryCharacter;
        public bool playerInputEnabled;
        public bool playerEnterKeyEnabled;
    }

    /// <summary>
    /// 飞字物体数据 (从GameStateManager迁移而来)
    /// </summary>
    [System.Serializable]
    public class FlyingCharacterData
    {
        public string character;
        public string targetObjectName;
        public Vector3 targetPosition;
        public float delay;
    }

    /// <summary>
    /// BeachObject状态数据 (从GameStateManager迁移而来)
    /// </summary>
    [System.Serializable]
    public class BeachObjectState
    {
        public bool hasYaBeenPlanted;
    }
    
    /// <summary>
    /// 默认构造函数
    /// </summary>
    public GameProgressData()
    {
        Reset();
    }
    
    /// <summary>
    /// 重置所有数据
    /// </summary>
    public void Reset()
    {
        currentLevel = "";
        completedLevels.Clear();
        gameStarted = false;
        levelStates.Clear();
        lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        createTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        version = "1.0";
    }
    
    /// <summary>
    /// 检查数据是否有效
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(version);
    }
    
    /// <summary>
    /// 获取进度摘要
    /// </summary>
    public string GetProgressSummary()
    {
        return $"当前关卡: {currentLevel}, 已完成: {completedLevels.Count}, 游戏已开始: {gameStarted}";
    }
    
    /// <summary>
    /// 添加已完成的关卡
    /// </summary>
    public void AddCompletedLevel(string levelName)
    {
        if (!string.IsNullOrEmpty(levelName) && !completedLevels.Contains(levelName))
        {
            completedLevels.Add(levelName);
        }
    }
    
    /// <summary>
    /// 检查关卡是否已完成
    /// </summary>
    public bool IsLevelCompleted(string levelName)
    {
        return completedLevels.Contains(levelName);
    }
    
    /// <summary>
    /// 移除已完成的关卡
    /// </summary>
    public void RemoveCompletedLevel(string levelName)
    {
        completedLevels.Remove(levelName);
    }
    
    /// <summary>
    /// 更新保存时间
    /// </summary>
    public void UpdateSaveTime()
    {
        lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    /// <summary>
    /// 获取指定关卡的状态数据
    /// </summary>
    /// <param name="levelName">关卡名称</param>
    /// <returns>关卡状态数据，如果不存在则返回null</returns>
    public LevelStateData GetLevelState(string levelName)
    {
        if (string.IsNullOrEmpty(levelName))
        {
            return null;
        }
        
        var entry = levelStates.Find(e => e.levelName == levelName);
        return entry?.stateData;
    }
    
    /// <summary>
    /// 设置指定关卡的状态数据
    /// </summary>
    /// <param name="levelName">关卡名称</param>
    /// <param name="stateData">状态数据</param>
    public void SetLevelState(string levelName, LevelStateData stateData)
    {
        if (string.IsNullOrEmpty(levelName) || stateData == null)
        {
            return;
        }
        
        stateData.levelName = levelName;
        stateData.saveTime = Time.time;
        
        // 查找现有条目
        var existingEntry = levelStates.Find(e => e.levelName == levelName);
        if (existingEntry != null)
        {
            // 更新现有条目
            existingEntry.stateData = stateData;
        }
        else
        {
            // 添加新条目
            levelStates.Add(new LevelStateEntry(levelName, stateData));
        }
    }
    
    /// <summary>
    /// 清除指定关卡的状态数据
    /// </summary>
    /// <param name="levelName">关卡名称</param>
    public void ClearLevelState(string levelName)
    {
        if (string.IsNullOrEmpty(levelName))
        {
            return;
        }
        
        levelStates.RemoveAll(e => e.levelName == levelName);
    }
    
    /// <summary>
    /// 清除所有关卡状态数据
    /// </summary>
    public void ClearAllLevelStates()
    {
        levelStates.Clear();
    }
    
    /// <summary>
    /// 清除指定关卡及后续关卡的状态数据
    /// </summary>
    /// <param name="levelName">关卡名称</param>
    /// <param name="levelSequence">关卡序列</param>
    public void ClearLevelAndSubsequentStates(string levelName, string[] levelSequence)
    {
        if (string.IsNullOrEmpty(levelName) || levelSequence == null)
        {
            return;
        }
        
        // 找到当前关卡在序列中的位置
        int currentIndex = -1;
        for (int i = 0; i < levelSequence.Length; i++)
        {
            if (levelSequence[i].Equals(levelName, StringComparison.OrdinalIgnoreCase))
            {
                currentIndex = i;
                break;
            }
        }
        
        if (currentIndex == -1)
        {
            return;
        }
        
        // 清除当前关卡及后续关卡的状态
        for (int i = currentIndex; i < levelSequence.Length; i++)
        {
            ClearLevelState(levelSequence[i]);
        }
    }
    
    /// <summary>
    /// 检查是否有任何关卡状态数据
    /// </summary>
    /// <returns>是否有关卡状态数据</returns>
    public bool HasAnyLevelStates()
    {
        return levelStates.Count > 0;
    }
    
    /// <summary>
    /// 获取关卡状态数据摘要
    /// </summary>
    /// <returns>关卡状态数据摘要</returns>
    public string GetLevelStatesSummary()
    {
        if (levelStates.Count == 0)
        {
            return "无关卡状态数据";
        }
        
        var summary = new List<string>();
        foreach (var entry in levelStates)
        {
            summary.Add($"{entry.levelName}: {entry.stateData.objectStates.Count}个物体, {entry.stateData.broadcastHistory.Count}条广播");
        }
        
        return string.Join("; ", summary);
    }
    
    /// <summary>
    /// 检查存档版本兼容性
    /// </summary>
    /// <returns>兼容性结果</returns>
    public SaveVersionCompatibility CheckVersionCompatibility()
    {
        if (string.IsNullOrEmpty(saveVersion))
        {
            // 旧存档没有saveVersion字段，视为1.0版本
            return SaveVersionCompatibility.Legacy;
        }
        
        if (saveVersion == CURRENT_SAVE_VERSION)
        {
            return SaveVersionCompatibility.Current;
        }
        
        if (saveVersion == LEGACY_SAVE_VERSION)
        {
            return SaveVersionCompatibility.Legacy;
        }
        
        // 检查是否在最小兼容版本之上
        if (CompareVersions(saveVersion, MIN_COMPATIBLE_VERSION) >= 0)
        {
            return SaveVersionCompatibility.Compatible;
        }
        
        return SaveVersionCompatibility.Incompatible;
    }
    
    /// <summary>
    /// 升级存档版本到当前版本
    /// </summary>
    public void UpgradeToCurrentVersion()
    {
        var compatibility = CheckVersionCompatibility();
        
        switch (compatibility)
        {
            case SaveVersionCompatibility.Current:
                // 已经是最新版本，无需升级
                break;
                
            case SaveVersionCompatibility.Legacy:
                // 从1.0升级到2.0
                UpgradeFromLegacy();
                break;
                
            case SaveVersionCompatibility.Compatible:
                // 兼容版本，直接更新版本号
                saveVersion = CURRENT_SAVE_VERSION;
                break;
                
            case SaveVersionCompatibility.Incompatible:
                // 不兼容，抛出异常或重置
                throw new System.Exception($"存档版本 {saveVersion} 不兼容，请重新开始游戏");
        }
    }
    
    /// <summary>
    /// 从遗留版本(1.0)升级到当前版本(2.0)
    /// </summary>
    private void UpgradeFromLegacy()
    {
        // 1.0版本的存档缺少uniqueId字段，其他结构基本相同
        // 升级时主要是设置版本号，uniqueId会在运行时自动添加
        
        saveVersion = CURRENT_SAVE_VERSION;
        
        // 可以在这里添加其他升级逻辑，比如：
        // - 数据结构转换
        // - 默认值设置
        // - 清理过时字段等
        
        GameLogger.LogSystem($"存档已从版本 {LEGACY_SAVE_VERSION} 升级到 {CURRENT_SAVE_VERSION}");
    }
    
    /// <summary>
    /// 比较两个版本号
    /// </summary>
    /// <param name="version1">版本1</param>
    /// <param name="version2">版本2</param>
    /// <returns>-1: version1 < version2, 0: 相等, 1: version1 > version2</returns>
    private int CompareVersions(string version1, string version2)
    {
        if (string.IsNullOrEmpty(version1)) version1 = "0.0";
        if (string.IsNullOrEmpty(version2)) version2 = "0.0";
        
        try
        {
            var v1Parts = version1.Split('.').Select(int.Parse).ToArray();
            var v2Parts = version2.Split('.').Select(int.Parse).ToArray();
            
            int maxLength = Math.Max(v1Parts.Length, v2Parts.Length);
            
            for (int i = 0; i < maxLength; i++)
            {
                int v1Part = i < v1Parts.Length ? v1Parts[i] : 0;
                int v2Part = i < v2Parts.Length ? v2Parts[i] : 0;
                
                if (v1Part < v2Part) return -1;
                if (v1Part > v2Part) return 1;
            }
            
            return 0;
        }
        catch
        {
            // 版本号格式错误，使用字符串比较
            return string.Compare(version1, version2, StringComparison.OrdinalIgnoreCase);
        }
    }
    
    /// <summary>
    /// 初始化新存档的版本信息
    /// </summary>
    public void InitializeVersion()
    {
        saveVersion = CURRENT_SAVE_VERSION;
        version = Application.version; // 使用Unity项目版本
        createTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}

/// <summary>
/// 存档版本兼容性枚举
/// </summary>
public enum SaveVersionCompatibility
{
    /// <summary>当前版本</summary>
    Current,
    /// <summary>遗留版本但兼容</summary>
    Legacy,
    /// <summary>兼容的版本</summary>
    Compatible,
    /// <summary>不兼容的版本</summary>
    Incompatible
}
