using UnityEngine;

/// <summary>
/// 琴默认提示测试器 - 专门测试"子"和"牙"字符的琴交互
/// </summary>
public class QinDefaultHintTester : MonoBehaviour
{
    [Header("调试设置")]
    [SerializeField] private bool enableDebugLog = true;
    
    private QinSpecialLogic qinLogic;
    private PlayerController playerController;
    private AutoHint autoHint;
    private BroadcastManager broadcastManager;
    
    private void Start()
    {
        // 获取组件引用
        qinLogic = FindObjectOfType<QinSpecialLogic>();
        playerController = FindObjectOfType<PlayerController>();
        autoHint = FindObjectOfType<AutoHint>();
        broadcastManager = BroadcastManager.Instance;
        
        if (enableDebugLog)
        {
            LogSystemStatus();
        }
    }
    
    /// <summary>
    /// 记录系统状态
    /// </summary>
    private void LogSystemStatus()
    {
        Debug.Log("=== 琴默认提示测试器状态 ===");
        
        Debug.Log($"QinSpecialLogic: {(qinLogic != null ? "找到" : "未找到")}");
        Debug.Log($"PlayerController: {(playerController != null ? "找到" : "未找到")}");
        Debug.Log($"AutoHint: {(autoHint != null ? "找到" : "未找到")}");
        Debug.Log($"BroadcastManager: {(broadcastManager != null ? "找到" : "未找到")}");
        
        Debug.Log("=============================");
    }
    
    /// <summary>
    /// 测试"子"字符的琴交互
    /// </summary>
    [ContextMenu("测试'子'字符琴交互")]
    public void TestZiCharacterInteraction()
    {
        Debug.Log("=== 测试'子'字符琴交互 ===");
        
        if (qinLogic == null)
        {
            Debug.LogError("QinSpecialLogic组件未找到！");
            return;
        }
        
        // 直接调用琴交互逻辑
        qinLogic.OnPlayerInteract("子");
        Debug.Log("已调用qinLogic.OnPlayerInteract('子')");
        
        // 检查是否应该显示默认提示
        Debug.Log("'子'不是有效字符（季、雅、孤），应该显示默认提示");
        
        Debug.Log("========================");
    }
    
    /// <summary>
    /// 测试"牙"字符的琴交互
    /// </summary>
    [ContextMenu("测试'牙'字符琴交互")]
    public void TestYaCharacterInteraction()
    {
        Debug.Log("=== 测试'牙'字符琴交互 ===");
        
        if (qinLogic == null)
        {
            Debug.LogError("QinSpecialLogic组件未找到！");
            return;
        }
        
        // 直接调用琴交互逻辑
        qinLogic.OnPlayerInteract("牙");
        Debug.Log("已调用qinLogic.OnPlayerInteract('牙')");
        
        // 检查是否应该显示默认提示
        Debug.Log("'牙'不是有效字符（季、雅、孤），应该显示默认提示");
        
        Debug.Log("========================");
    }
    
    /// <summary>
    /// 测试有效字符的琴交互（对比测试）
    /// </summary>
    [ContextMenu("测试有效字符琴交互")]
    public void TestValidCharacterInteraction()
    {
        Debug.Log("=== 测试有效字符琴交互 ===");
        
        if (qinLogic == null)
        {
            Debug.LogError("QinSpecialLogic组件未找到！");
            return;
        }
        
        // 测试有效字符
        string[] validChars = { "季", "雅", "孤" };
        foreach (string character in validChars)
        {
            Debug.Log($"测试有效字符: {character}");
            qinLogic.OnPlayerInteract(character);
        }
        
        Debug.Log("========================");
    }
    
    /// <summary>
    /// 测试广播系统
    /// </summary>
    [ContextMenu("测试广播系统")]
    public void TestBroadcastSystem()
    {
        Debug.Log("=== 测试广播系统 ===");
        
        if (broadcastManager == null)
        {
            Debug.LogError("BroadcastManager.Instance为null！");
            return;
        }
        
        Debug.Log("BroadcastManager可用，测试发送'琴默认提示'广播");
        broadcastManager.BroadcastToAll("琴默认提示");
        Debug.Log("已发送'琴默认提示'广播");
        
        Debug.Log("========================");
    }
    
    /// <summary>
    /// 测试AutoHint直接调用
    /// </summary>
    [ContextMenu("测试AutoHint直接调用")]
    public void TestAutoHintDirectCall()
    {
        Debug.Log("=== 测试AutoHint直接调用 ===");
        
        if (autoHint == null)
        {
            Debug.LogError("AutoHint组件未找到！");
            return;
        }
        
        // 直接调用AutoHint
        autoHint.ReceiveBroadcast("琴默认提示");
        Debug.Log("已直接调用AutoHint显示'琴默认提示'");
        
        Debug.Log("========================");
    }
    
    /// <summary>
    /// 检查PublicData字典
    /// </summary>
    [ContextMenu("检查PublicData字典")]
    public void CheckPublicDataDictionary()
    {
        Debug.Log("=== 检查PublicData字典 ===");
        
        if (PublicData.autoHintDict == null)
        {
            Debug.LogError("PublicData.autoHintDict为null！");
            return;
        }
        
        Debug.Log($"autoHintDict条目数量: {PublicData.autoHintDict.Count}");
        
        // 检查琴默认提示
        if (PublicData.autoHintDict.TryGetValue("琴默认提示", out string value))
        {
            Debug.Log($"✅ '琴默认提示' -> '{value}'");
        }
        else
        {
            Debug.LogError("❌ '琴默认提示' 不存在于字典中");
        }
        
        // 检查不应该存在的键
        string[] shouldNotExist = { "琴子", "琴牙" };
        foreach (string key in shouldNotExist)
        {
            if (PublicData.autoHintDict.TryGetValue(key, out string val))
            {
                Debug.LogWarning($"⚠️ 意外发现键 '{key}' -> '{val}'");
            }
            else
            {
                Debug.Log($"✅ 键 '{key}' 不存在（正确）");
            }
        }
        
        Debug.Log("========================");
    }
    
    /// <summary>
    /// 设置玩家携带字符并测试
    /// </summary>
    /// <param name="character">字符</param>
    [ContextMenu("设置玩家为'子'并测试")]
    public void SetPlayerToZiAndTest()
    {
        SetPlayerAndTest("子");
    }
    
    [ContextMenu("设置玩家为'牙'并测试")]
    public void SetPlayerToYaAndTest()
    {
        SetPlayerAndTest("牙");
    }
    
    [ContextMenu("设置玩家为'季'并测试")]
    public void SetPlayerToJiAndTest()
    {
        SetPlayerAndTest("季");
    }
    
    private void SetPlayerAndTest(string character)
    {
        Debug.Log($"=== 设置玩家为'{character}'并测试 ===");
        
        // 设置玩家携带字符
        if (playerController != null)
        {
            Player player1 = playerController.GetPlayerByIndex(0);
            if (player1 != null)
            {
                player1.SetCarryCharacter(character);
                Debug.Log($"已设置Player1携带字符为: {character}");
                
                // 测试琴交互
                if (qinLogic != null)
                {
                    qinLogic.OnPlayerInteract(character);
                    Debug.Log($"已测试琴交互，玩家携带字符: {character}");
                }
            }
            else
            {
                Debug.LogError("Player1未找到");
            }
        }
        else
        {
            Debug.LogError("PlayerController未找到");
        }
        
        Debug.Log("========================");
    }
    
    /// <summary>
    /// 完整测试流程
    /// </summary>
    [ContextMenu("完整测试流程")]
    public void CompleteTestFlow()
    {
        Debug.Log("=== 完整测试流程 ===");
        
        // 1. 检查字典
        CheckPublicDataDictionary();
        
        // 2. 测试广播系统
        TestBroadcastSystem();
        
        // 3. 测试AutoHint直接调用
        TestAutoHintDirectCall();
        
        // 4. 测试各种字符
        Debug.Log("4. 测试各种字符的琴交互");
        SetPlayerAndTest("子");
        SetPlayerAndTest("牙");
        SetPlayerAndTest("季");
        
        Debug.Log("========================");
    }
}
