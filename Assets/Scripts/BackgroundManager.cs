using UnityEngine;

/// <summary>
/// 背景管理器
/// 负责管理左右两边背景的sprite切换
/// </summary>
public class BackgroundManager : MonoBehaviour
{
    [Header("背景设置")]
    [SerializeField] private SpriteRenderer leftBackground;
    [SerializeField] private SpriteRenderer rightBackground;
    
    [Header("背景图片")]
    [SerializeField] private Sprite initialLeftBackground;
    [SerializeField] private Sprite initialRightBackground;
    [SerializeField] private Sprite swappedLeftBackground;
    [SerializeField] private Sprite swappedRightBackground;
    
    [Header("调试设置")]
    [SerializeField] private bool enableLogging = true;
    
    void Start()
    {
        // 如果没有手动设置，尝试自动获取
        if (leftBackground == null)
        {
            leftBackground = GameObject.Find("LeftBackground")?.GetComponent<SpriteRenderer>();
        }
        
        if (rightBackground == null)
        {
            rightBackground = GameObject.Find("RightBackground")?.GetComponent<SpriteRenderer>();
        }
        
        if (enableLogging)
        {
            GameLogger.LogDev($"BackgroundManager: 初始化完成 - 左背景: {leftBackground?.gameObject.name ?? "null"}, 右背景: {rightBackground?.gameObject.name ?? "null"}");
        }
    }
    
    /// <summary>
    /// 切换到初始背景
    /// </summary>
    public void SwitchToInitialState()
    {
        if (leftBackground != null && initialLeftBackground != null)
        {
            leftBackground.sprite = initialLeftBackground;
        }
        else
        {
            GameLogger.LogWarning("BackgroundManager: 左背景或初始背景图未设置");
        }
        
        if (rightBackground != null && initialRightBackground != null)
        {
            rightBackground.sprite = initialRightBackground;
        }
        else
        {
            GameLogger.LogWarning("BackgroundManager: 右背景或初始背景图未设置");
        }
        GameLogger.LogDev("BackgroundManager: 已切换到初始背景");
    }
    
    /// <summary>
    /// 切换到切换后的背景
    /// </summary>
    public void SwitchToSwappedState()
    {
        if (leftBackground != null && swappedLeftBackground != null)
        {
            leftBackground.sprite = swappedLeftBackground;
        }
        else
        {
            GameLogger.LogWarning("BackgroundManager: 左背景或切换后背景图未设置");
        }
        
        if (rightBackground != null && swappedRightBackground != null)
        {
            rightBackground.sprite = swappedRightBackground;
        }
        else
        {
            GameLogger.LogWarning("BackgroundManager: 右背景或切换后背景图未设置");
        }
        GameLogger.LogDev("BackgroundManager: 已切换到切换后背景");
    }
    
    /// <summary>
    /// 接收广播的方法（必须与广播调用方法名一致）
    /// </summary>
    /// <param name="broadcastedValue">广播的值</param>
    public void ReceiveBroadcast(string broadcastedValue)
    {
        if (GameLogger.IsDevLogEnabled())
        {
            GameLogger.LogDev($"BackgroundManager: 接收到广播: {broadcastedValue}");
        }
        
        // 处理"停"广播，切换到切换后的背景
        if (broadcastedValue == "停")
        {
            SwitchToSwappedState();
        }
    }
    
    /// <summary>
    /// 设置左背景引用
    /// </summary>
    /// <param name="spriteRenderer">左背景的SpriteRenderer</param>
    public void SetLeftBackground(SpriteRenderer spriteRenderer)
    {
        leftBackground = spriteRenderer;
        if (enableLogging)
        {
            GameLogger.LogDev($"BackgroundManager: 已设置左背景: {spriteRenderer?.gameObject.name ?? "null"}");
        }
    }
    
    /// <summary>
    /// 设置右背景引用
    /// </summary>
    /// <param name="spriteRenderer">右背景的SpriteRenderer</param>
    public void SetRightBackground(SpriteRenderer spriteRenderer)
    {
        rightBackground = spriteRenderer;
        if (enableLogging)
        {
            GameLogger.LogDev($"BackgroundManager: 已设置右背景: {spriteRenderer?.gameObject.name ?? "null"}");
        }
    }
    
    /// <summary>
    /// 设置初始背景图片
    /// </summary>
    /// <param name="leftSprite">左初始背景</param>
    /// <param name="rightSprite">右初始背景</param>
    public void SetInitialBackgroundSprites(Sprite leftSprite, Sprite rightSprite)
    {
        initialLeftBackground = leftSprite;
        initialRightBackground = rightSprite;
        if (GameLogger.IsDevLogEnabled())
        {
            GameLogger.LogDev($"BackgroundManager: 已设置初始背景图片 - 左: {leftSprite?.name ?? "null"}, 右: {rightSprite?.name ?? "null"}");
        }
    }
    
    /// <summary>
    /// 设置切换后背景图片
    /// </summary>
    /// <param name="leftSprite">左切换后背景</param>
    /// <param name="rightSprite">右切换后背景</param>
    public void SetSwappedBackgroundSprites(Sprite leftSprite, Sprite rightSprite)
    {
        swappedLeftBackground = leftSprite;
        swappedRightBackground = rightSprite;
        if (GameLogger.IsDevLogEnabled())
        {
            GameLogger.LogDev($"BackgroundManager: 已设置切换后背景图片 - 左: {leftSprite?.name ?? "null"}, 右: {rightSprite?.name ?? "null"}");
        }
    }
    
    /// <summary>
    /// 手动切换到初始背景（用于测试）
    /// </summary>
    [ContextMenu("切换到初始背景")]
    public void TestSwitchToInitial()
    {
        SwitchToInitialState();
    }
    
    /// <summary>
    /// 手动切换到切换后背景（用于测试）
    /// </summary>
    [ContextMenu("切换到切换后背景")]
    public void TestSwitchToSwapped()
    {
        SwitchToSwappedState();
    }
    
    /// <summary>
    /// 模拟"停"广播（用于测试）
    /// </summary>
    [ContextMenu("模拟停广播")]
    public void TestStopBroadcast()
    {
        if (GameLogger.IsDevLogEnabled())
        {
            GameLogger.LogDev("BackgroundManager: 模拟发送'停'广播");
        }
        ReceiveBroadcast("停");
    }

    /// <summary>
    /// 通用背景切换方法 - 切换当前背景状态
    /// </summary>
    public void SwitchBackground()
    {
        if (leftBackground == null || swappedLeftBackground == null || initialLeftBackground == null)
        {
            GameLogger.LogWarning("BackgroundManager: 左背景或背景图引用未设置，无法切换。");
            return;
        }

        // 基于左背景当前的Sprite来判断应切换到哪个状态
        if (leftBackground.sprite == swappedLeftBackground)
        {
            SwitchToInitialState();
        }
        else
        {
            SwitchToSwappedState();
        }
    }
}
