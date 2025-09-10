using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Level3场景的季节类型枚举
/// </summary>
public enum SeasonType
{
    Spring,  // 春季
    Summer   // 夏季
}

/// <summary>
/// Level3场景管理器 - 管理季节切换和相关逻辑
/// </summary>
public class Level3Manager : MonoBehaviour
{
    [Header("季节设置")]
    [SerializeField] private SeasonType currentSeason = SeasonType.Spring;
    
    [Header("季节切换设置")]
    [SerializeField] private float seasonTransitionDuration = 1f;
    [SerializeField] private bool enableSeasonTransition = true;
    
    [Header("调试设置")]
    [SerializeField] private bool showDebugInfo = true;
    
    [Header("收集设置")]
    [SerializeField] private List<string> collectedStrings = new List<string>();
    
    [Header("Level3彩蛋设置")]
    [SerializeField] private bool enableEasterEgg = true;
    [SerializeField] private bool showEasterEggInfo = true;
    
    [Header("特殊对象引用")]
    [SerializeField] private BeachObject beachObject; // 对滩涂对象的引用
    [SerializeField] private BackgroundManager backgroundManager; // 对背景管理器的引用
    
    // 事件：季节切换时触发
    public System.Action<SeasonType> OnSeasonChanged;
    
    // 事件：收集到新字符串时触发
    public System.Action<string> OnStringCollected;
    // 简单的彩蛋状态跟踪
    private static bool easterEggTriggered = false;
    
    private void Start()
    {
        // 播放关卡BGM
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmLevel3);
            
            if (showDebugInfo)
            {
                GameLogger.LogDev("Level3Manager: 开始播放知音篇主题BGM");
            }
        }
        else
        {
            GameLogger.LogWarning("Level3Manager: AudioManager实例未找到，无法播放BGM");
        }
        
        // 初始化季节状态
        InitializeSeason();
        
        // 初始化彩蛋功能
        InitializeEasterEgg();
        
        // 延迟一帧强制启用玩家移动，避免被其他管理器在Start中覆盖
        StartCoroutine(EnsureEnableMovementNextFrame());
        
        if (showDebugInfo)
        {
            GameLogger.LogDev($"Level3Manager: 初始化完成，当前季节: {currentSeason}");
        }
    }

    private System.Collections.IEnumerator EnsureEnableMovementNextFrame()
    {
        yield return new WaitForEndOfFrame();
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.EnableCurrentPlayerMovement();
            // 同时确保所有玩家的回车键与输入开启
            int count = playerController.GetPlayerCount();
            for (int i = 0; i < count; i++)
            {
                Player p = playerController.GetPlayerByIndex(i);
                if (p != null)
                {
                    p.SetInputEnabled(true);
                    p.SetEnterKeyEnabled(true);
                }
            }

            // 参照Level2：设置当前玩家索引、启用切换与颜色更新
            if (playerController.GetPlayerCount() > 0)
            {
                playerController.SetCurrentPlayerIndex(0);
            }
            playerController.EnablePlayerSwitching();
            playerController.UpdatePlayerColors();

            if (showDebugInfo)
            {
                GameLogger.LogDev("Level3Manager: 已启用移动/输入，并更新玩家颜色与切换状态");
            }
        }
        else
        {
            GameLogger.LogWarning("Level3Manager: 未找到PlayerController，无法启用玩家移动");
        }
    }
    
    /// <summary>
    /// 初始化季节状态
    /// </summary>
    private void InitializeSeason()
    {
        // 根据当前季节设置场景状态
        ApplySeasonEffects(currentSeason);
    }
    
    /// <summary>
    /// 切换到指定季节
    /// </summary>
    /// <param name="newSeason">目标季节</param>
    public void SwitchToSeason(SeasonType newSeason)
    {
        if (currentSeason == newSeason)
        {
            if (showDebugInfo)
            {
                GameLogger.LogDev($"Level3Manager: 已经是{newSeason}季节，无需切换");
            }
            return;
        }
        
        SeasonType previousSeason = currentSeason;
        currentSeason = newSeason;
        
        if (showDebugInfo)
        {
            GameLogger.LogDev($"Level3Manager: 季节切换 {previousSeason} -> {currentSeason}");
        }
        
        // 应用季节效果
        if (enableSeasonTransition)
        {
            StartCoroutine(SeasonTransitionCoroutine(previousSeason, currentSeason));
        }
        else
        {
            ApplySeasonEffects(currentSeason);
        }
        
        // 触发季节切换事件
        OnSeasonChanged?.Invoke(currentSeason);

        // 广播季节切换（用于让“芽”->“瓜”等联动）
        if (BroadcastManager.Instance != null && previousSeason == SeasonType.Spring && currentSeason == SeasonType.Summer)
        {
            BroadcastManager.Instance.BroadcastToAll("季夏");
            if (showDebugInfo)
            {
                GameLogger.LogDev("Level3Manager: 已广播'季夏'以触发季节相关联动");
            }
        }
    }
    
    /// <summary>
    /// 切换到春季
    /// </summary>
    public void SwitchToSpring()
    {
        SwitchToSeason(SeasonType.Spring);
    }
    
    /// <summary>
    /// 切换到夏季
    /// </summary>
    public void SwitchToSummer()
    {
        SwitchToSeason(SeasonType.Summer);
    }
    
    /// <summary>
    /// 在春季和夏季之间切换
    /// </summary>
    public void ToggleSeason()
    {
        SeasonType originalSeason = currentSeason;
        currentSeason = (currentSeason == SeasonType.Spring) ? SeasonType.Summer : SeasonType.Spring;
        GameLogger.LogDev($"Level3Manager: 季节已切换为 {currentSeason}");
        
        // 季节切换后，检查是否需要将“芽”变为“瓜”
        if (originalSeason == SeasonType.Spring && currentSeason == SeasonType.Summer)
        {
            if (beachObject != null)
            {
                beachObject.TransformYaToGuaOnSeasonChange();
            }
            else
            {
                GameLogger.LogWarning("Level3Manager: BeachObject引用未设置，无法执行芽变瓜的逻辑。");
            }
        }
    }
    
    /// <summary>
    /// 季节切换协程
    /// </summary>
    private System.Collections.IEnumerator SeasonTransitionCoroutine(SeasonType fromSeason, SeasonType toSeason)
    {
        if (showDebugInfo)
        {
            GameLogger.LogDev($"Level3Manager: 开始季节切换动画 {fromSeason} -> {toSeason}");
        }
        
        // 这里可以添加季节切换的动画效果
        // 例如：淡入淡出、颜色变化等
        
        float elapsedTime = 0f;
        while (elapsedTime < seasonTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / seasonTransitionDuration;
            
            // 可以在这里添加过渡效果
            // 例如：插值颜色、透明度等
            
            yield return null;
        }
        
        // 应用最终季节效果
        ApplySeasonEffects(toSeason);
        
        if (showDebugInfo)
        {
            GameLogger.LogDev($"Level3Manager: 季节切换完成 {toSeason}");
        }
    }
    
    /// <summary>
    /// 应用季节效果
    /// </summary>
    /// <param name="season">要应用的季节</param>
    private void ApplySeasonEffects(SeasonType season)
    {
        switch (season)
        {
            case SeasonType.Spring:
                ApplySpringEffects();
                break;
            case SeasonType.Summer:
                ApplySummerEffects();
                break;
        }
    }
    
    /// <summary>
    /// 应用春季效果
    /// </summary>
    private void ApplySpringEffects()
    {
        if (showDebugInfo)
        {
            GameLogger.LogDev("Level3Manager: 应用春季效果");
        }
        
        // 春季效果实现
        // 例如：改变背景、调整光照、显示春季元素等
        // 不再显隐物体；仅保留季节状态
    }
    
    /// <summary>
    /// 应用夏季效果
    /// </summary>
    private void ApplySummerEffects()
    {
        if (showDebugInfo)
        {
            GameLogger.LogDev("Level3Manager: 应用夏季效果");
        }
        
        // 夏季效果实现
        // 例如：改变背景、调整光照、显示夏季元素等
        // 不再显隐物体；仅保留季节状态
    }
    
    /// <summary>
    /// 启用指定季节的对象
    /// </summary>
    /// <param name="seasonTag">季节标签</param>
    private void EnableSeasonObjects(string seasonTag)
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains(seasonTag))
            {
                obj.SetActive(true);
                
                // 如果有SpriteRenderer，确保启用
                SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = true;
                }
                
                if (showDebugInfo)
                {
                    GameLogger.LogDev($"Level3Manager: 启用{seasonTag}对象: {obj.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// 禁用指定季节的对象
    /// </summary>
    /// <param name="seasonTag">季节标签</param>
    private void DisableSeasonObjects(string seasonTag)
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains(seasonTag))
            {
                obj.SetActive(false);
                
                if (showDebugInfo)
                {
                    GameLogger.LogDev($"Level3Manager: 禁用{seasonTag}对象: {obj.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// 获取当前季节
    /// </summary>
    /// <returns>当前季节</returns>
    public SeasonType GetCurrentSeason()
    {
        return currentSeason;
    }
    
    /// <summary>
    /// 设置当前季节（不触发切换效果）
    /// </summary>
    /// <param name="season">季节</param>
    public void SetCurrentSeason(SeasonType season)
    {
        currentSeason = season;
        ApplySeasonEffects(currentSeason);
        
        if (showDebugInfo)
        {
            GameLogger.LogDev($"Level3Manager: 直接设置季节为: {currentSeason}");
        }
    }
    
    /// <summary>
    /// 检查是否为指定季节
    /// </summary>
    /// <param name="season">要检查的季节</param>
    /// <returns>是否为指定季节</returns>
    public bool IsSeason(SeasonType season)
    {
        return currentSeason == season;
    }
    
    /// <summary>
    /// 检查是否为春季
    /// </summary>
    /// <returns>是否为春季</returns>
    public bool IsSpring()
    {
        return currentSeason == SeasonType.Spring;
    }
    
    /// <summary>
    /// 检查是否为夏季
    /// </summary>
    /// <returns>是否为夏季</returns>
    public bool IsSummer()
    {
        return currentSeason == SeasonType.Summer;
    }
    
    #region Level3 彩蛋功能
    
    /// <summary>
    /// 初始化彩蛋功能
    /// </summary>
    private void InitializeEasterEgg()
    {
        if (!enableEasterEgg)
        {
            if (showEasterEggInfo)
            {
                GameLogger.LogDev("Level3Manager: 彩蛋功能已禁用");
            }
            return;
        }
        
        // 重置彩蛋状态
        easterEggTriggered = false;
        
        if (showEasterEggInfo)
        {
            GameLogger.LogDev("Level3Manager: 彩蛋功能初始化完成，等待广播消息");
        }
    }
    
    /// <summary>
    /// 处理彩蛋触发逻辑
    /// </summary>
    private void HandleEasterEggTriggered()
    {
        easterEggTriggered = true;
        
        if (showEasterEggInfo)
        {
            GameLogger.LogDev("Level3Manager: 彩蛋已触发！玩家发现了隐藏的'王'字彩蛋");
        }
        
        // 播放特殊音效（如果有的话）
        if (AudioManager.Instance != null && AudioManager.Instance.sfxEasterEgg != null)
        {
            // 播放彩蛋音效
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxEasterEgg);
            GameLogger.LogDev("Level3Manager: 播放彩蛋音效");
        }
        else if (AudioManager.Instance != null)
        {
            GameLogger.LogWarning("Level3Manager: 彩蛋音效未配置");
        }
    }
    
    /// <summary>
    /// 检查彩蛋是否已触发
    /// </summary>
    /// <returns>彩蛋是否已触发</returns>
    public bool IsEasterEggTriggered()
    {
        return easterEggTriggered;
    }
    
    /// <summary>
    /// 重置彩蛋状态
    /// </summary>
    public void ResetEasterEgg()
    {
        easterEggTriggered = false;
        if (showEasterEggInfo)
        {
            GameLogger.LogDev("Level3Manager: 彩蛋状态已重置");
        }
    }
    
    /// <summary>
    /// 设置彩蛋启用状态
    /// </summary>
    /// <param name="enabled">是否启用彩蛋</param>
    public void SetEasterEggEnabled(bool enabled)
    {
        enableEasterEgg = enabled;
        if (showEasterEggInfo)
        {
            GameLogger.LogDev($"Level3Manager: 彩蛋功能已{(enabled ? "启用" : "禁用")}");
        }
    }
    
    #endregion
    
    // 调试方法：在Inspector中调用
    [ContextMenu("切换到春季")]
    public void DebugSwitchToSpring()
    {
        SwitchToSpring();
    }
    
    [ContextMenu("切换到夏季")]
    public void DebugSwitchToSummer()
    {
        SwitchToSummer();
    }
    
    [ContextMenu("切换季节")]
    public void DebugToggleSeason()
    {
        ToggleSeason();
    }
    
    [ContextMenu("触发彩蛋测试")]
    public void DebugTriggerEasterEgg()
    {
        if (enableEasterEgg)
        {
            HandleEasterEggTriggered();
        }
        else
        {
            GameLogger.LogDev("Level3Manager: 彩蛋功能未启用，无法测试");
        }
    }
    
    [ContextMenu("重置彩蛋状态")]
    public void DebugResetEasterEgg()
    {
        ResetEasterEgg();
    }
    
    /// <summary>
    /// 添加字符串到可用字符串列表
    /// </summary>
    /// <param name="value">要添加的字符串</param>
    private void AddStringToAvailableList(string value)
    {
        if (string.IsNullOrEmpty(value)) return;
        
        // 查找StringSelector并添加字符串
        StringSelector stringSelector = FindObjectOfType<StringSelector>();
        if (stringSelector != null)
        {
            stringSelector.AddAvailableString(value);
            
            if (showDebugInfo)
            {
                GameLogger.LogDev($"Level3Manager: 已添加字符串 '{value}' 到可用列表");
            }
            
            // 播放取字音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxAcquire);
            }
        }
        else
        {
            GameLogger.LogWarning($"Level3Manager: 未找到StringSelector，无法添加字符串 '{value}'");
        }
    }
    
    /// <summary>
    /// 根据letter删除所有对应的Highlight脚本
    /// </summary>
    /// <param name="letter">要删除的letter值</param>
    private void RemoveHighlightsByLetter(string letter)
    {
        if (string.IsNullOrEmpty(letter)) return;
        
        // 查找所有Highlight组件
        Highlight[] allHighlights = FindObjectsOfType<Highlight>(true);
        int removedCount = 0;
        
        foreach (var highlight in allHighlights)
        {
            if (highlight != null && highlight.letter == letter)
            {
                UnityEngine.Object.Destroy(highlight);
                removedCount++;
            }
        }
        
        if (showDebugInfo)
        {
            GameLogger.LogDev($"Level3Manager: 已从 {removedCount} 个对象上移除 Highlight（letter=='{letter}'）");
        }
    }
    
    // ============= 收集字符串管理 =============
    
    /// <summary>
    /// 添加收集到的字符串
    /// </summary>
    /// <param name="value">要添加的字符串</param>
    public void AddCollectedString(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        
        if (collectedStrings == null) 
            collectedStrings = new List<string>();
            
        if (!collectedStrings.Contains(value))
        {
            collectedStrings.Add(value);
            
            if (showDebugInfo)
            {
                GameLogger.LogDev($"Level3Manager: 收集到新字符串 '{value}'，当前总数: {collectedStrings.Count}");
            }
            
            // 触发收集事件
            OnStringCollected?.Invoke(value);
        }
        else
        {
            if (showDebugInfo)
            {
                GameLogger.LogDev($"Level3Manager: 字符串 '{value}' 已存在，跳过添加");
            }
        }
    }
    
    /// <summary>
    /// 检查是否已收集指定字符串
    /// </summary>
    /// <param name="value">要检查的字符串</param>
    /// <returns>是否已收集</returns>
    public bool HasCollectedString(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        return collectedStrings != null && collectedStrings.Contains(value);
    }
    
    /// <summary>
    /// 获取已收集的字符串列表（只读）
    /// </summary>
    /// <returns>已收集的字符串列表</returns>
    public IReadOnlyList<string> GetCollectedStrings()
    {
        if (collectedStrings == null) return System.Array.Empty<string>();
        return collectedStrings.AsReadOnly();
    }
    
    /// <summary>
    /// 获取已收集的字符串数量
    /// </summary>
    /// <returns>已收集的字符串数量</returns>
    public int GetCollectedCount()
    {
        return collectedStrings?.Count ?? 0;
    }
    
    /// <summary>
    /// 清空已收集的字符串列表
    /// </summary>
    public void ClearCollectedStrings()
    {
        if (collectedStrings != null)
        {
            int count = collectedStrings.Count;
            collectedStrings.Clear();
            
            if (showDebugInfo)
            {
                GameLogger.LogDev($"Level3Manager: 清空了 {count} 个已收集的字符串");
            }
        }
    }
    
    /// <summary>
    /// 检查是否已收集所有目标字符串
    /// </summary>
    /// <param name="targetStrings">目标字符串列表</param>
    /// <returns>是否已收集所有目标</returns>
    public bool HasCollectedAllTargets(List<string> targetStrings)
    {
        if (targetStrings == null || targetStrings.Count == 0) return true;
        if (collectedStrings == null) return false;
        
        foreach (string target in targetStrings)
        {
            if (!collectedStrings.Contains(target))
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 获取未收集的目标字符串
    /// </summary>
    /// <param name="targetStrings">目标字符串列表</param>
    /// <returns>未收集的字符串列表</returns>
    public List<string> GetUncollectedTargets(List<string> targetStrings)
    {
        List<string> uncollected = new List<string>();
        
        if (targetStrings == null || targetStrings.Count == 0) return uncollected;
        if (collectedStrings == null)
        {
            uncollected.AddRange(targetStrings);
            return uncollected;
        }
        
        foreach (string target in targetStrings)
        {
            if (!collectedStrings.Contains(target))
            {
                uncollected.Add(target);
            }
        }
        
        return uncollected;
    }
    
    // 调试方法：在Inspector中调用
    [ContextMenu("清空收集列表")]
    public void DebugClearCollectedStrings()
    {
        ClearCollectedStrings();
    }
    
    [ContextMenu("显示收集列表")]
    public void DebugShowCollectedStrings()
    {
        if (collectedStrings == null || collectedStrings.Count == 0)
        {
            GameLogger.LogDev("Level3Manager: 收集列表为空");
        }
        else
        {
            GameLogger.LogDev($"Level3Manager: 已收集 {collectedStrings.Count} 个字符串: [{string.Join(", ", collectedStrings)}]");
        }
    }

    // ===== 广播接收 =====
    public void ReceiveBroadcast(string broadcastedValue)
    {
        if (string.IsNullOrEmpty(broadcastedValue)) return;
        if (showDebugInfo)
        {
            GameLogger.LogDev($"Level3Manager: 收到广播 '{broadcastedValue}'");
        }

        // 收到"琴季"时，切换季节和背景
        if (broadcastedValue == "琴季")
        {
            if (showDebugInfo)
            {
                GameLogger.LogDev("Level3Manager: 收到'琴季'广播，执行季节和背景切换");
            }
            
            // 切换季节
            ToggleSeason();
            
            // 切换背景
            if (backgroundManager != null)
            {
                backgroundManager.SwitchBackground();
                if (showDebugInfo)
                {
                    GameLogger.LogDev("Level3Manager: 已切换背景");
                }
            }
            else
            {
                GameLogger.LogWarning("Level3Manager: 未找到BackgroundManager，无法切换背景");
            }
        }
        // 收到"琴雅"时，获得"俗"字并删除"隹"对象
        else if (broadcastedValue == "琴雅")
        {
            if (showDebugInfo)
            {
                GameLogger.LogDev("Level3Manager: 收到'琴雅'广播，获得'俗'字");
            }
            
            // 1) 添加"俗"字到可用字符串列表
            AddStringToAvailableList("俗");
            
            // 2) 删除所有letter=="隹"的对象上的Highlight脚本
            RemoveHighlightsByLetter("隹");
        }
        // 收到"琴孤"时，获得"欣"字并删除"瓜"对象
        else if (broadcastedValue == "琴孤")
        {
            if (showDebugInfo)
            {
                GameLogger.LogDev("Level3Manager: 收到'琴孤'广播，获得'欣'字");
            }
            
            // 1) 添加"欣"字到可用字符串列表
            AddStringToAvailableList("欣");
            
            // 2) 删除所有letter=="瓜"的对象上的Highlight脚本
            RemoveHighlightsByLetter("瓜");
        }
        
        // 处理彩蛋广播
        if (enableEasterEgg && broadcastedValue == "拼一土" && !easterEggTriggered)
        {
            HandleEasterEggTriggered();
        }
    }
}
