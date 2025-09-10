using UnityEngine;

/// <summary>
/// 琴互动测试器 - 帮助诊断琴互动问题
/// </summary>
public class QinInteractionTester : MonoBehaviour
{
    [Header("测试设置")]
    [SerializeField] private bool enableDebugLog = true;
    
    private QinSpecialLogic qinLogic;
    private PlayerController playerController;
    
    private void Start()
    {
        // 获取组件引用
        qinLogic = FindObjectOfType<QinSpecialLogic>();
        playerController = FindObjectOfType<PlayerController>();
        
        if (enableDebugLog)
        {
            LogQinStatus();
        }
    }
    
    /// <summary>
    /// 记录琴状态
    /// </summary>
    private void LogQinStatus()
    {
        Debug.Log("=== 琴互动测试器状态 ===");
        
        if (qinLogic == null)
        {
            Debug.LogError("QinSpecialLogic组件未找到！");
            return;
        }
        
        Debug.Log($"QinSpecialLogic GameObject: {qinLogic.gameObject.name}");
        Debug.Log($"QinSpecialLogic GameObject Active: {qinLogic.gameObject.activeInHierarchy}");
        Debug.Log($"QinSpecialLogic Component Enabled: {qinLogic.enabled}");
        
        if (playerController == null)
        {
            Debug.LogError("PlayerController组件未找到！");
        }
        else
        {
            Player player1 = playerController.GetPlayerByIndex(0);
            if (player1 != null)
            {
                Debug.Log($"Player1携带字符: {player1.CarryCharacter}");
            }
        }
        
        Debug.Log("========================");
    }
    
    /// <summary>
    /// 测试琴默认提示
    /// </summary>
    [ContextMenu("测试琴默认提示")]
    public void TestQinDefaultHint()
    {
        if (qinLogic == null)
        {
            Debug.LogError("QinSpecialLogic组件未找到，无法测试");
            return;
        }
        
        Debug.Log("=== 测试琴默认提示 ===");
        
        // 测试携带非特殊字符
        if (playerController != null)
        {
            Player player1 = playerController.GetPlayerByIndex(0);
            if (player1 != null)
            {
                string initialChar = player1.GetInitialCarryCharacter();
                qinLogic.OnPlayerInteract(initialChar);
            }
            else
            {
                qinLogic.OnPlayerInteract("人"); // 备用默认值
            }
        }
        else
        {
            qinLogic.OnPlayerInteract("人"); // 备用默认值
        }
        
        Debug.Log("========================");
    }
    
    /// <summary>
    /// 测试琴特殊互动
    /// </summary>
    [ContextMenu("测试琴特殊互动")]
    public void TestQinSpecialInteraction()
    {
        if (qinLogic == null)
        {
            Debug.LogError("QinSpecialLogic组件未找到，无法测试");
            return;
        }
        
        Debug.Log("=== 测试琴特殊互动 ===");
        
        // 测试携带特殊字符
        string[] specialChars = { "季", "雅", "孤" };
        foreach (string character in specialChars)
        {
            Debug.Log($"测试字符: {character}");
            qinLogic.OnPlayerInteract(character);
        }
        
        Debug.Log("========================");
    }
    
    /// <summary>
    /// 测试PublicData字典
    /// </summary>
    [ContextMenu("测试琴相关字典")]
    public void TestQinDictionary()
    {
        Debug.Log("=== 测试琴相关字典 ===");
        
        if (PublicData.autoHintDict == null)
        {
            Debug.LogError("PublicData.autoHintDict为null！");
            return;
        }
        
        Debug.Log($"autoHintDict条目数量: {PublicData.autoHintDict.Count}");
        
        // 测试琴相关键
        string[] qinKeys = { "琴默认提示", "琴季", "琴雅", "琴孤" };
        foreach (string key in qinKeys)
        {
            if (PublicData.autoHintDict.TryGetValue(key, out string value))
            {
                Debug.Log($"键 '{key}' -> 值 '{value}'");
            }
            else
            {
                Debug.LogWarning($"键 '{key}' 不存在于字典中");
            }
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
        
        if (BroadcastManager.Instance == null)
        {
            Debug.LogError("BroadcastManager.Instance为null！");
            return;
        }
        
        Debug.Log("BroadcastManager可用，测试发送广播");
        BroadcastManager.Instance.BroadcastToAll("琴默认提示");
        Debug.Log("已发送'琴默认提示'广播");
        
        Debug.Log("========================");
    }
    
    /// <summary>
    /// 设置玩家携带字符
    /// </summary>
    /// <param name="character">字符</param>
    [ContextMenu("设置玩家为初始字符")]
    public void SetPlayerToInitial()
    {
        if (playerController != null)
        {
            Player player1 = playerController.GetPlayerByIndex(0);
            if (player1 != null)
            {
                string initialChar = player1.GetInitialCarryCharacter();
                SetPlayerCarryCharacter(initialChar);
            }
            else
            {
                SetPlayerCarryCharacter("人"); // 备用默认值
            }
        }
        else
        {
            SetPlayerCarryCharacter("人"); // 备用默认值
        }
    }
    
    [ContextMenu("设置玩家为'季'")]
    public void SetPlayerToJi()
    {
        SetPlayerCarryCharacter("季");
    }
    
    [ContextMenu("设置玩家为'雅'")]
    public void SetPlayerToYa()
    {
        SetPlayerCarryCharacter("雅");
    }
    
    [ContextMenu("设置玩家为'孤'")]
    public void SetPlayerToGu()
    {
        SetPlayerCarryCharacter("孤");
    }
    
    private void SetPlayerCarryCharacter(string character)
    {
        if (playerController == null)
        {
            Debug.LogError("PlayerController组件未找到，无法设置玩家字符");
            return;
        }
        
        Player player1 = playerController.GetPlayerByIndex(0);
        if (player1 != null)
        {
            player1.SetCarryCharacter(character);
            Debug.Log($"已设置Player1携带字符为: {character}");
        }
        else
        {
            Debug.LogError("Player1未找到，无法设置携带字符");
        }
    }
}
