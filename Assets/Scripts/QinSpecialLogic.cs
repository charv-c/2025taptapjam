using UnityEngine;

/// <summary>
/// 琴对象的特殊逻辑脚本
/// 负责处理琴的sprite切换：在选中和未选中状态之间切换
/// </summary>
public class QinSpecialLogic : MonoBehaviour
{
    [Header("琴Sprite设置")]
    [Tooltip("琴的选中状态sprite")]
    [SerializeField] private Sprite qinSelectedSprite;
    
    [Tooltip("琴的未选中状态sprite")]
    [SerializeField] private Sprite qinUnselectedSprite;
    
    [Header("调试设置")]
    [SerializeField] private bool enableLogging = true;
    
    private SpriteRenderer qinSpriteRenderer;
    private bool isPlayerInRange = false;
    
    private void Awake()
    {
        // 获取琴对象的SpriteRenderer组件
        qinSpriteRenderer = GetComponent<SpriteRenderer>();
        if (qinSpriteRenderer == null)
        {
            Debug.LogError($"QinSpecialLogic: 琴对象 '{gameObject.name}' 没有SpriteRenderer组件");
        }
    }
    
    private void Start()
    {
        // 初始化时设置为未选中状态
        SetToUnselectedState();
    }
    
    /// <summary>
    /// 当玩家进入触发区域时调用
    /// </summary>
    public void OnPlayerEnter()
    {
        if (enableLogging)
        {
            Debug.Log($"QinSpecialLogic: 玩家进入琴的触发区域 - {gameObject.name}");
        }
        
        isPlayerInRange = true;
        SetToSelectedState();
    }
    
    /// <summary>
    /// 当玩家离开触发区域时调用
    /// </summary>
    public void OnPlayerExit()
    {
        if (enableLogging)
        {
            Debug.Log($"QinSpecialLogic: 玩家离开琴的触发区域 - {gameObject.name}");
        }
        
        isPlayerInRange = false;
        SetToUnselectedState();
    }
    
    /// <summary>
    /// 当玩家与琴交互时调用
    /// </summary>
    /// <param name="carryCharacter">玩家携带的字符</param>
    public void OnPlayerInteract(string carryCharacter)
    {
        if (enableLogging)
        {
            Debug.Log($"QinSpecialLogic: 玩家与琴交互，携带字符: '{carryCharacter}' - {gameObject.name}");
        }
        
        // 确保琴处于选中状态
        if (!isPlayerInRange)
        {
            SetToSelectedState();
        }
        
        // 检查是否满足特殊条件（季、雅、孤）
        bool isValid = IsValidCharacter(carryCharacter);
        if (enableLogging)
        {
            Debug.Log($"QinSpecialLogic: 字符 '{carryCharacter}' 是否有效: {isValid}");
        }
        
        if (isValid)
        {
            // 满足条件，发送广播消息
            string broadcastMessage = $"琴{carryCharacter}";
            if (BroadcastManager.Instance != null)
            {
                BroadcastManager.Instance.BroadcastToAll(broadcastMessage);
                if (enableLogging)
                {
                    Debug.Log($"QinSpecialLogic: 发送广播 '{broadcastMessage}'");
                }
            }
            else
            {
                Debug.LogWarning("QinSpecialLogic: BroadcastManager.Instance为null，无法发送广播");
            }

            // 直接调用AutoHint作为兜底，避免广播路由缺失导致不显示
            var autoHint = FindObjectOfType<AutoHint>();
            if (autoHint != null)
            {
                autoHint.ReceiveBroadcast(broadcastMessage);
                if (enableLogging)
                {
                    Debug.Log($"QinSpecialLogic: 已直接调用AutoHint显示 '{broadcastMessage}'");
                }
            }
            else if (enableLogging)
            {
                Debug.LogWarning("QinSpecialLogic: 未找到AutoHint组件，无法直接显示提示");
            }
        }
        else
        {
            // 不满足条件，显示默认提示
            if (enableLogging)
            {
                Debug.Log($"QinSpecialLogic: 字符 '{carryCharacter}' 不满足特殊条件，显示默认提示");
            }
            ShowDefaultHint();
        }
    }
    
    /// <summary>
    /// 设置为选中状态
    /// </summary>
    private void SetToSelectedState()
    {
        if (qinSpriteRenderer == null) return;
        
        if (qinSelectedSprite != null)
        {
            qinSpriteRenderer.sprite = qinSelectedSprite;
            if (enableLogging)
            {
                Debug.Log($"QinSpecialLogic: 已切换到选中状态 - {gameObject.name}");
            }
        }
        else
        {
            Debug.LogWarning($"QinSpecialLogic: 选中状态sprite未设置 - {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 设置为未选中状态
    /// </summary>
    private void SetToUnselectedState()
    {
        if (qinSpriteRenderer == null) return;
        
        if (qinUnselectedSprite != null)
        {
            qinSpriteRenderer.sprite = qinUnselectedSprite;
            if (enableLogging)
            {
                Debug.Log($"QinSpecialLogic: 已切换到未选中状态 - {gameObject.name}");
            }
        }
        else
        {
            Debug.LogWarning($"QinSpecialLogic: 未选中状态sprite未设置 - {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 获取当前是否在选中状态
    /// </summary>
    /// <returns>是否在选中状态</returns>
    public bool IsSelected()
    {
        return isPlayerInRange;
    }
    
    /// <summary>
    /// 手动设置选中状态sprite（用于运行时调试）
    /// </summary>
    /// <param name="sprite">要设置的sprite</param>
    public void SetSelectedSprite(Sprite sprite)
    {
        qinSelectedSprite = sprite;
        if (enableLogging)
        {
            Debug.Log($"QinSpecialLogic: 已设置选中状态sprite - {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 手动设置未选中状态sprite（用于运行时调试）
    /// </summary>
    /// <param name="sprite">要设置的sprite</param>
    public void SetUnselectedSprite(Sprite sprite)
    {
        qinUnselectedSprite = sprite;
        if (enableLogging)
        {
            Debug.Log($"QinSpecialLogic: 已设置未选中状态sprite - {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 检查字符是否有效（季、雅、孤）
    /// </summary>
    /// <param name="character">要检查的字符</param>
    /// <returns>是否有效</returns>
    private bool IsValidCharacter(string character)
    {
        return character == "季" || character == "雅" || character == "孤";
    }
    
    /// <summary>
    /// 显示默认提示
    /// </summary>
    private void ShowDefaultHint()
    {
        if (enableLogging)
        {
            Debug.Log($"QinSpecialLogic: 显示默认提示");
        }
        
        // 通过广播系统显示默认提示
        if (BroadcastManager.Instance != null)
        {
            BroadcastManager.Instance.BroadcastToAll("琴默认提示");
            if (enableLogging)
            {
                Debug.Log("QinSpecialLogic: 通过广播系统发送'琴默认提示'广播");
            }
        }
        else
        {
            Debug.LogWarning("QinSpecialLogic: 无法显示默认提示，BroadcastManager不可用");
        }
    }
    
    /// <summary>
    /// 强制刷新当前状态（用于调试）
    /// </summary>
    [ContextMenu("刷新当前状态")]
    public void RefreshCurrentState()
    {
        if (isPlayerInRange)
        {
            SetToSelectedState();
        }
        else
        {
            SetToUnselectedState();
        }
    }
}
