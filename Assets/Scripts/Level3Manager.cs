using UnityEngine;

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
    
    [Header("Level3彩蛋设置")]
    [SerializeField] private bool enableEasterEgg = true;
    [SerializeField] private bool showEasterEggInfo = true;
    
    // 事件：季节切换时触发
    public System.Action<SeasonType> OnSeasonChanged;
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
        
        if (showDebugInfo)
        {
            GameLogger.LogDev($"Level3Manager: 初始化完成，当前季节: {currentSeason}");
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
        SeasonType newSeason = currentSeason == SeasonType.Spring ? SeasonType.Summer : SeasonType.Spring;
        SwitchToSeason(newSeason);
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
        
        // 查找并启用春季相关的对象
        EnableSeasonObjects("Spring");
        DisableSeasonObjects("Summer");
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
        
        // 查找并启用夏季相关的对象
        EnableSeasonObjects("Summer");
        DisableSeasonObjects("Spring");
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
            if (obj.name.Contains(seasonTag) || obj.CompareTag(seasonTag))
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
            if (obj.name.Contains(seasonTag) || obj.CompareTag(seasonTag))
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
    /// 广播接收器 - 处理彩蛋广播消息
    /// </summary>
    /// <param name="message">广播消息</param>
    public void ReceiveBroadcast(string message)
    {
        if (!enableEasterEgg) return;
        
        if (message == "拼一土" && !easterEggTriggered)
        {
            HandleEasterEggTriggered();
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
}
