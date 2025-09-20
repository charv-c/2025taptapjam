using System;
using System.Collections.Generic;
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
    public Dictionary<string, LevelStateData> levelStates = new Dictionary<string, LevelStateData>();
    
    [Header("时间戳")]
    public string lastSaveTime = "";
    public string createTime = "";
    
    [Header("版本信息")]
    public string version = "1.0";
    
    /// <summary>
    /// 关卡状态数据类
    /// </summary>
    [System.Serializable]
    public class LevelStateData
    {
        public string levelName = "";
        public List<GameObjectStateData> objectStates = new List<GameObjectStateData>();
        public List<string> broadcastHistory = new List<string>();
        public List<string> availableStrings = new List<string>();
        public string currentSeason = "";
        public List<string> collectedStrings = new List<string>();
        public List<FlyingCharacterData> flyingCharacters = new List<FlyingCharacterData>();
        public List<string> completedTargets = new List<string>();
        public List<string> currentTargetList = new List<string>();
        public BeachObjectStateData beachObjectState = new BeachObjectStateData();
        public float saveTime = 0f;
        
        /// <summary>
        /// 重置关卡状态数据
        /// </summary>
        public void Reset()
        {
            objectStates.Clear();
            broadcastHistory.Clear();
            availableStrings.Clear();
            currentSeason = "";
            collectedStrings.Clear();
            flyingCharacters.Clear();
            completedTargets.Clear();
            currentTargetList.Clear();
            beachObjectState = new BeachObjectStateData();
            saveTime = Time.time;
        }
    }
    
    /// <summary>
    /// 游戏对象状态数据
    /// </summary>
    [System.Serializable]
    public class GameObjectStateData
    {
        public string objectName = "";
        public bool isActive = true;
        public bool highlightEnabled = true;
        public Vector3 position = Vector3.zero;
        public Vector3 rotation = Vector3.zero;
        public Vector3 scale = Vector3.one;
    }
    
    /// <summary>
    /// 飞字物体数据
    /// </summary>
    [System.Serializable]
    public class FlyingCharacterData
    {
        public string characterName = "";
        public Vector3 position = Vector3.zero;
        public Vector3 rotation = Vector3.zero;
        public Vector3 scale = Vector3.one;
        public bool isActive = true;
        public bool isFlying = false;
    }
    
    /// <summary>
    /// BeachObject状态数据
    /// </summary>
    [System.Serializable]
    public class BeachObjectStateData
    {
        public bool isVisible = true;
        public Vector3 position = Vector3.zero;
        public Vector3 rotation = Vector3.zero;
        public Vector3 scale = Vector3.one;
        public bool isInteractable = true;
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
        if (string.IsNullOrEmpty(levelName) || !levelStates.ContainsKey(levelName))
        {
            return null;
        }
        return levelStates[levelName];
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
        levelStates[levelName] = stateData;
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
        
        if (levelStates.ContainsKey(levelName))
        {
            levelStates.Remove(levelName);
        }
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
        foreach (var kvp in levelStates)
        {
            summary.Add($"{kvp.Key}: {kvp.Value.objectStates.Count}个物体, {kvp.Value.broadcastHistory.Count}条广播");
        }
        
        return string.Join("; ", summary);
    }
}
