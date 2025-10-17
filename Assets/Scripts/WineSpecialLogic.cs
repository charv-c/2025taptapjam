using UnityEngine;

/// <summary>
/// 酒对象的特殊逻辑脚本
/// 负责处理酒的特殊互动逻辑
/// </summary>
public class WineSpecialLogic : MonoBehaviour
{
    [Header("Sprite设置")]
    [SerializeField] private Sprite normalSprite; // 正常状态的sprite
    [SerializeField] private Sprite highlightedSprite; // 高亮状态的sprite
    
    [Header("调试设置")]
    [SerializeField] private bool enableLogging = true;
    
    private bool isPlayerInRange = false;
    private SpriteRenderer wineSpriteRenderer;
    
    private void Start()
    {
        // 获取SpriteRenderer组件
        wineSpriteRenderer = GetComponent<SpriteRenderer>();
        if (wineSpriteRenderer == null)
        {
            GameLogger.LogError($"WineSpecialLogic: 酒对象 '{gameObject.name}' 没有SpriteRenderer组件");
        }
        
        // 初始化时设置为正常状态
        SetToNormalState();
        
        if (enableLogging)
        {
            GameLogger.LogDev($"WineSpecialLogic: 酒对象初始化 - {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 当玩家进入触发区域时调用
    /// </summary>
    public void OnPlayerEnter()
    {
        if (enableLogging)
        {
            GameLogger.LogDev($"WineSpecialLogic: 玩家进入酒的触发区域 - {gameObject.name}");
        }
        
        isPlayerInRange = true;
        SetToHighlightedState();
    }
    
    /// <summary>
    /// 当玩家离开触发区域时调用
    /// </summary>
    public void OnPlayerExit()
    {
        if (enableLogging)
        {
            GameLogger.LogDev($"WineSpecialLogic: 玩家离开酒的触发区域 - {gameObject.name}");
        }
        
        isPlayerInRange = false;
        SetToNormalState();
    }
    
    /// <summary>
    /// 当玩家与酒互动时调用
    /// </summary>
    /// <param name="playerCarryCharacter">玩家携带的字符</param>
    public void OnPlayerInteract(string playerCarryCharacter)
    {
        if (enableLogging)
        {
            GameLogger.LogDev($"WineSpecialLogic: 玩家与酒互动，携带字符: '{playerCarryCharacter}'");
        }
        
        // 获取Player组件
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            // 设置玩家携带字符为"蛇"
            player.SetCarryCharacter("蛇");
            
            // 开始倒计时
            player.StartCountdown();
            
            if (enableLogging)
            {
                GameLogger.LogDev("WineSpecialLogic: 已将玩家携带字符设置为'蛇'并开始倒计时");
            }
        }
        else
        {
            GameLogger.LogWarning("WineSpecialLogic: 未找到Player组件");
        }
        
        // 发送广播"酒"
        if (BroadcastManager.Instance != null)
        {
            BroadcastManager.Instance.BroadcastToAll("酒");
            
            if (enableLogging)
            {
                GameLogger.LogDev("WineSpecialLogic: 已发送广播 '酒'");
            }
        }
        else
        {
            GameLogger.LogWarning("WineSpecialLogic: 未找到BroadcastManager实例");
        }
    }
    
    /// <summary>
    /// 设置为正常状态
    /// </summary>
    private void SetToNormalState()
    {
        if (wineSpriteRenderer != null && normalSprite != null)
        {
            wineSpriteRenderer.sprite = normalSprite;
            
            if (enableLogging)
            {
                GameLogger.LogDev("WineSpecialLogic: 酒设置为正常状态");
            }
        }
    }
    
    /// <summary>
    /// 设置为高亮状态
    /// </summary>
    private void SetToHighlightedState()
    {
        if (wineSpriteRenderer != null && highlightedSprite != null)
        {
            wineSpriteRenderer.sprite = highlightedSprite;
            
            if (enableLogging)
            {
                GameLogger.LogDev("WineSpecialLogic: 酒设置为高亮状态");
            }
        }
    }
}
