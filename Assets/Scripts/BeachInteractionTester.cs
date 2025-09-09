using UnityEngine;

/// <summary>
/// 滩涂互动测试器 - 用于测试滩涂互动逻辑
/// </summary>
public class BeachInteractionTester : MonoBehaviour
{
    [Header("测试设置")]
    [SerializeField] private bool enableDebugLog = true;
    
    private BeachObject beachObject;
    private PlayerController playerController;
    private Level3Manager level3Manager;
    
    private void Start()
    {
        // 获取组件引用
        beachObject = FindObjectOfType<BeachObject>();
        playerController = FindObjectOfType<PlayerController>();
        level3Manager = FindObjectOfType<Level3Manager>();
        
        if (enableDebugLog)
        {
            LogComponentStatus();
        }
    }
    
    /// <summary>
    /// 记录组件状态
    /// </summary>
    private void LogComponentStatus()
    {
        Debug.Log("=== 滩涂互动测试器组件状态 ===");
        Debug.Log($"BeachObject: {(beachObject != null ? "找到" : "未找到")}");
        Debug.Log($"PlayerController: {(playerController != null ? "找到" : "未找到")}");
        Debug.Log($"Level3Manager: {(level3Manager != null ? "找到" : "未找到")}");
        
        if (playerController != null)
        {
            Player player1 = playerController.GetPlayerByIndex(0);
            if (player1 != null)
            {
                Debug.Log($"Player1携带字符: {player1.CarryCharacter}");
            }
        }
        
        if (level3Manager != null)
        {
            Debug.Log($"当前季节: {(level3Manager.IsSpring() ? "春季" : "夏季")}");
        }
        
        Debug.Log("================================");
    }
    
    /// <summary>
    /// 测试芽在春季的互动
    /// </summary>
    [ContextMenu("测试芽+春季")]
    public void TestYaInSpring()
    {
        if (!CanTest()) return;
        
        Debug.Log("=== 测试：芽 + 春季 ===");
        
        // 设置玩家携带字符为"芽"
        SetPlayer1CarryCharacter("芽");
        
        // 设置季节为春季
        SetSeasonToSpring();
        
        // 执行滩涂互动
        ExecuteBeachInteraction();
    }
    
    /// <summary>
    /// 测试芽在夏季的互动
    /// </summary>
    [ContextMenu("测试芽+夏季")]
    public void TestYaInSummer()
    {
        if (!CanTest()) return;
        
        Debug.Log("=== 测试：芽 + 夏季 ===");
        
        // 设置玩家携带字符为"芽"
        SetPlayer1CarryCharacter("芽");
        
        // 设置季节为夏季
        SetSeasonToSummer();
        
        // 执行滩涂互动
        ExecuteBeachInteraction();
    }
    
    /// <summary>
    /// 测试其他字符的互动
    /// </summary>
    [ContextMenu("测试其他字符")]
    public void TestOtherCharacter()
    {
        if (!CanTest()) return;
        
        Debug.Log("=== 测试：其他字符 ===");
        
        // 设置玩家携带字符为"人"
        SetPlayer1CarryCharacter("人");
        
        // 执行滩涂互动
        ExecuteBeachInteraction();
    }
    
    /// <summary>
    /// 检查是否可以测试
    /// </summary>
    /// <returns>是否可以测试</returns>
    private bool CanTest()
    {
        if (beachObject == null)
        {
            Debug.LogError("BeachObject组件未找到，无法测试");
            return false;
        }
        
        if (playerController == null)
        {
            Debug.LogError("PlayerController组件未找到，无法测试");
            return false;
        }
        
        if (level3Manager == null)
        {
            Debug.LogError("Level3Manager组件未找到，无法测试");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 设置player1的携带字符
    /// </summary>
    /// <param name="character">字符</param>
    private void SetPlayer1CarryCharacter(string character)
    {
        Player player1 = playerController.GetPlayerByIndex(0);
        if (player1 != null)
        {
            player1.SetCarryCharacter(character);
            Debug.Log($"已设置Player1携带字符为: {character}");
        }
    }
    
    /// <summary>
    /// 设置季节为春季
    /// </summary>
    private void SetSeasonToSpring()
    {
        level3Manager.SetCurrentSeason(SeasonType.Spring);
        Debug.Log("已设置季节为春季");
    }
    
    /// <summary>
    /// 设置季节为夏季
    /// </summary>
    private void SetSeasonToSummer()
    {
        level3Manager.SetCurrentSeason(SeasonType.Summer);
        Debug.Log("已设置季节为夏季");
    }
    
    /// <summary>
    /// 执行滩涂互动
    /// </summary>
    private void ExecuteBeachInteraction()
    {
        beachObject.ExecuteBeachInteraction();
        Debug.Log("已执行滩涂互动");
    }
    
    /// <summary>
    /// 重置测试环境
    /// </summary>
    [ContextMenu("重置测试环境")]
    public void ResetTestEnvironment()
    {
        Debug.Log("=== 重置测试环境 ===");
        
        // 重置玩家携带字符
        SetPlayer1CarryCharacter("人");
        
        // 重置季节
        SetSeasonToSpring();
        
        // 隐藏花和隹物体
        HideFlowerAndZhuiObjects();
        
        Debug.Log("测试环境已重置");
    }
    
    /// <summary>
    /// 隐藏花和隹物体
    /// </summary>
    private void HideFlowerAndZhuiObjects()
    {
        // 隐藏花物体
        Highlight[] allHighlights = FindObjectsOfType<Highlight>(true);
        foreach (Highlight highlight in allHighlights)
        {
            if (highlight != null)
            {
                if (highlight.letter == "花" || highlight.letter == "隹")
                {
                    highlight.HideObject();
                    Debug.Log($"已隐藏物体: {highlight.letter}");
                }
            }
        }
    }
}
