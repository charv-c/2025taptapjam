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
    [SerializeField] private Sprite rainyLeftBackground;
    [SerializeField] private Sprite rainyRightBackground;
    [SerializeField] private Sprite sunnyLeftBackground;
    [SerializeField] private Sprite sunnyRightBackground;
    
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
            Debug.Log($"BackgroundManager: 初始化完成 - 左背景: {leftBackground?.gameObject.name ?? "null"}, 右背景: {rightBackground?.gameObject.name ?? "null"}");
        }
    }
    
    /// <summary>
    /// 切换到雨天背景
    /// </summary>
    public void SwitchToRainyBackground()
    {
        if (leftBackground != null && rainyLeftBackground != null)
        {
            leftBackground.sprite = rainyLeftBackground;
            if (enableLogging)
            {
                Debug.Log($"BackgroundManager: 左背景已切换到雨天背景");
            }
        }
        else
        {
            Debug.LogWarning("BackgroundManager: 左背景或雨天背景图片未设置");
        }
        
        if (rightBackground != null && rainyRightBackground != null)
        {
            rightBackground.sprite = rainyRightBackground;
            if (enableLogging)
            {
                Debug.Log($"BackgroundManager: 右背景已切换到雨天背景");
            }
        }
        else
        {
            Debug.LogWarning("BackgroundManager: 右背景或雨天背景图片未设置");
        }
    }
    
    /// <summary>
    /// 切换到晴天背景
    /// </summary>
    public void SwitchToSunnyBackground()
    {
        if (leftBackground != null && sunnyLeftBackground != null)
        {
            leftBackground.sprite = sunnyLeftBackground;
            if (enableLogging)
            {
                Debug.Log($"BackgroundManager: 左背景已切换到晴天背景");
            }
        }
        else
        {
            Debug.LogWarning("BackgroundManager: 左背景或晴天背景图片未设置");
        }
        
        if (rightBackground != null && sunnyRightBackground != null)
        {
            rightBackground.sprite = sunnyRightBackground;
            if (enableLogging)
            {
                Debug.Log($"BackgroundManager: 右背景已切换到晴天背景");
            }
        }
        else
        {
            Debug.LogWarning("BackgroundManager: 右背景或晴天背景图片未设置");
        }
    }
    
    /// <summary>
    /// 接收广播的方法（必须与广播调用方法名一致）
    /// </summary>
    /// <param name="broadcastedValue">广播的值</param>
    public void ReceiveBroadcast(string broadcastedValue)
    {
        if (enableLogging)
        {
            Debug.Log($"BackgroundManager: 接收到广播: {broadcastedValue}");
        }
        
        // 处理"停"广播，切换到晴天背景
        if (broadcastedValue == "停")
        {
            SwitchToSunnyBackground();
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
            Debug.Log($"BackgroundManager: 已设置左背景: {spriteRenderer?.gameObject.name ?? "null"}");
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
            Debug.Log($"BackgroundManager: 已设置右背景: {spriteRenderer?.gameObject.name ?? "null"}");
        }
    }
    
    /// <summary>
    /// 设置雨天背景图片
    /// </summary>
    /// <param name="leftSprite">左雨天背景</param>
    /// <param name="rightSprite">右雨天背景</param>
    public void SetRainyBackgroundSprites(Sprite leftSprite, Sprite rightSprite)
    {
        rainyLeftBackground = leftSprite;
        rainyRightBackground = rightSprite;
        if (enableLogging)
        {
            Debug.Log($"BackgroundManager: 已设置雨天背景图片 - 左: {leftSprite?.name ?? "null"}, 右: {rightSprite?.name ?? "null"}");
        }
    }
    
    /// <summary>
    /// 设置晴天背景图片
    /// </summary>
    /// <param name="leftSprite">左晴天背景</param>
    /// <param name="rightSprite">右晴天背景</param>
    public void SetSunnyBackgroundSprites(Sprite leftSprite, Sprite rightSprite)
    {
        sunnyLeftBackground = leftSprite;
        sunnyRightBackground = rightSprite;
        if (enableLogging)
        {
            Debug.Log($"BackgroundManager: 已设置晴天背景图片 - 左: {leftSprite?.name ?? "null"}, 右: {rightSprite?.name ?? "null"}");
        }
    }
    
    /// <summary>
    /// 手动切换到雨天背景（用于测试）
    /// </summary>
    [ContextMenu("切换到雨天背景")]
    public void TestSwitchToRainy()
    {
        SwitchToRainyBackground();
    }
    
    /// <summary>
    /// 手动切换到晴天背景（用于测试）
    /// </summary>
    [ContextMenu("切换到晴天背景")]
    public void TestSwitchToSunny()
    {
        SwitchToSunnyBackground();
    }
    
    /// <summary>
    /// 模拟"停"广播（用于测试）
    /// </summary>
    [ContextMenu("模拟停广播")]
    public void TestStopBroadcast()
    {
        if (enableLogging)
        {
            Debug.Log("BackgroundManager: 模拟发送'停'广播");
        }
        ReceiveBroadcast("停");
    }
}
