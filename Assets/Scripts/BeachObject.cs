using UnityEngine;
using System.Collections;

/// <summary>
/// 滩涂物体脚本 - 处理所有滩涂相关的特殊逻辑
/// 此脚本应附加到letter为"滩"的物体上
/// </summary>
public class BeachObject : MonoBehaviour
{
    [Header("物体引用")]
    [SerializeField] private GameObject flowerObject;  // 花物体
    [SerializeField] private GameObject zhuiObject;    // 隹物体
    
    [Header("延迟设置")]
    [SerializeField] private float delayBeforeShowZhui = 0.5f; // 显示隹物体前的延迟时间（秒）
    
    [Header("调试设置")]
    [SerializeField] private bool enableDebugLog = true;
    
    private PlayerController playerController;
    private Level3Manager level3Manager;
    private AutoHint autoHint;
    
    private void Start()
    {
        // 获取必要的组件引用
        playerController = FindObjectOfType<PlayerController>();
        level3Manager = FindObjectOfType<Level3Manager>();
        autoHint = FindObjectOfType<AutoHint>();
        
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
        Debug.Log("=== 滩涂物体组件状态 ===");
        Debug.Log($"PlayerController: {(playerController != null ? "找到" : "未找到")}");
        Debug.Log($"Level3Manager: {(level3Manager != null ? "找到" : "未找到")}");
        Debug.Log($"AutoHint: {(autoHint != null ? "找到" : "未找到")}");
        Debug.Log($"花物体引用: {(flowerObject != null ? "已设置" : "未设置")}");
        Debug.Log($"隹物体引用: {(zhuiObject != null ? "已设置" : "未设置")}");
        Debug.Log("==========================");
    }
    
    /// <summary>
    /// 执行滩涂互动逻辑
    /// 由Highlight脚本调用
    /// </summary>
    public void ExecuteBeachInteraction()
    {
        if (enableDebugLog)
        {
            Debug.Log("BeachObject: 开始执行滩涂互动逻辑");
        }
        
        // 获取player1的携带字符
        string player1Char = GetPlayer1CarryCharacter();
        
        if (player1Char == "芽")
        {
            // 检查当前季节
            bool isSpring = IsCurrentSeasonSpring();
            
            if (isSpring)
            {
                // 春季：显示等待提示
                ShowAutoHint("「芽」喜盛夏，待季节更迭再试吧");
            }
            else
            {
                // 非春季（夏季）：执行芽的绽放逻辑
                ExecuteYaBloomingLogic();
            }
        }
        else
        {
            // 其他字符：显示滩涂描述
            ShowAutoHint("一片湿润滩涂，土质肥沃，适合生命成长");
        }
    }
    
    /// <summary>
    /// 获取player1的携带字符
    /// </summary>
    /// <returns>player1的携带字符</returns>
    private string GetPlayer1CarryCharacter()
    {
        if (playerController != null)
        {
            // 获取第一个玩家（player1）的携带字符
            Player player1 = playerController.GetPlayerByIndex(0);
            if (player1 != null)
            {
                string carryChar = player1.CarryCharacter;
                if (enableDebugLog)
                {
                    Debug.Log($"BeachObject: Player1携带字符 = {carryChar}");
                }
                return carryChar;
            }
        }
        
        if (enableDebugLog)
        {
            Debug.LogWarning("BeachObject: 无法获取Player1的携带字符");
        }
        return "";
    }
    
    /// <summary>
    /// 检查当前季节是否为春季
    /// </summary>
    /// <returns>是否为春季</returns>
    private bool IsCurrentSeasonSpring()
    {
        if (level3Manager != null)
        {
            bool isSpring = level3Manager.IsSpring();
            if (enableDebugLog)
            {
                Debug.Log($"BeachObject: 当前季节 = {(isSpring ? "春季" : "夏季")}");
            }
            return isSpring;
        }
        
        if (enableDebugLog)
        {
            Debug.LogWarning("BeachObject: 无法获取季节信息");
        }
        return false;
    }
    
    /// <summary>
    /// 执行芽的绽放逻辑
    /// </summary>
    private void ExecuteYaBloomingLogic()
    {
        if (enableDebugLog)
        {
            Debug.Log("BeachObject: 执行芽的绽放逻辑");
        }
        
        // 显示花物体
        ShowFlowerObject();
        
        // 显示提示
        ShowAutoHint("「芽」逢盛夏，终得绽放成「花」");
        
        // 延迟显示隹物体
        StartCoroutine(ShowZhuiObjectAfterDelay());
    }
    
    /// <summary>
    /// 显示花物体
    /// </summary>
    private void ShowFlowerObject()
    {
        if (flowerObject != null)
        {
            // 使用Highlight脚本显示物体
            Highlight flowerHighlight = flowerObject.GetComponent<Highlight>();
            if (flowerHighlight != null)
            {
                flowerHighlight.ShowObject();
                if (enableDebugLog)
                {
                    Debug.Log($"BeachObject: 已显示花物体 - {flowerObject.name}");
                }
            }
            else
            {
                // 如果没有Highlight脚本，直接激活GameObject
                flowerObject.SetActive(true);
                if (enableDebugLog)
                {
                    Debug.Log($"BeachObject: 已激活花物体 - {flowerObject.name}");
                }
            }
        }
        else
        {
            // 如果没有设置引用，尝试查找场景中的花物体
            FindAndShowFlowerObject();
        }
    }
    
    /// <summary>
    /// 查找并显示花物体
    /// </summary>
    private void FindAndShowFlowerObject()
    {
        // 查找场景中所有letter为"花"的Highlight对象
        Highlight[] allHighlights = FindObjectsOfType<Highlight>(true);
        foreach (Highlight highlight in allHighlights)
        {
            if (highlight != null && highlight.letter == "花")
            {
                highlight.ShowObject();
                if (enableDebugLog)
                {
                    Debug.Log($"BeachObject: 已显示花物体 - {highlight.gameObject.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// 延迟显示隹物体
    /// </summary>
    /// <returns>协程</returns>
    private IEnumerator ShowZhuiObjectAfterDelay()
    {
        if (enableDebugLog)
        {
            Debug.Log($"BeachObject: 等待 {delayBeforeShowZhui} 秒后显示隹物体");
        }
        
        // 等待指定时间
        yield return new WaitForSeconds(delayBeforeShowZhui);
        
        // 显示隹物体
        ShowZhuiObject();
    }
    
    /// <summary>
    /// 显示隹物体
    /// </summary>
    private void ShowZhuiObject()
    {
        if (zhuiObject != null)
        {
            // 使用Highlight脚本显示物体
            Highlight zhuiHighlight = zhuiObject.GetComponent<Highlight>();
            if (zhuiHighlight != null)
            {
                zhuiHighlight.ShowObject();
                if (enableDebugLog)
                {
                    Debug.Log($"BeachObject: 已显示隹物体 - {zhuiObject.name}");
                }
            }
            else
            {
                // 如果没有Highlight脚本，直接激活GameObject
                zhuiObject.SetActive(true);
                if (enableDebugLog)
                {
                    Debug.Log($"BeachObject: 已激活隹物体 - {zhuiObject.name}");
                }
            }
        }
        else
        {
            // 如果没有设置引用，尝试查找场景中的隹物体
            FindAndShowZhuiObject();
        }
    }
    
    /// <summary>
    /// 查找并显示隹物体
    /// </summary>
    private void FindAndShowZhuiObject()
    {
        // 查找场景中所有letter为"隹"的Highlight对象
        Highlight[] allHighlights = FindObjectsOfType<Highlight>(true);
        foreach (Highlight highlight in allHighlights)
        {
            if (highlight != null && highlight.letter == "隹")
            {
                highlight.ShowObject();
                if (enableDebugLog)
                {
                    Debug.Log($"BeachObject: 已显示隹物体 - {highlight.gameObject.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// 显示自动提示
    /// </summary>
    /// <param name="message">提示消息</param>
    private void ShowAutoHint(string message)
    {
        if (autoHint != null)
        {
            autoHint.ReceiveBroadcast(message);
            if (enableDebugLog)
            {
                Debug.Log($"BeachObject: 已显示提示 - {message}");
            }
        }
        else
        {
            Debug.LogWarning("BeachObject: AutoHint组件为空，无法显示提示");
        }
    }
    
    /// <summary>
    /// 在Inspector中测试滩涂互动
    /// </summary>
    [ContextMenu("测试滩涂互动")]
    public void TestBeachInteraction()
    {
        Debug.Log("BeachObject: 开始测试滩涂互动");
        ExecuteBeachInteraction();
    }
    
    /// <summary>
    /// 重置滩涂状态
    /// </summary>
    [ContextMenu("重置滩涂状态")]
    public void ResetBeachState()
    {
        if (enableDebugLog)
        {
            Debug.Log("BeachObject: 重置滩涂状态");
        }
        
        // 隐藏花和隹物体
        HideFlowerAndZhuiObjects();
    }
    
    /// <summary>
    /// 隐藏花和隹物体
    /// </summary>
    private void HideFlowerAndZhuiObjects()
    {
        // 隐藏花物体
        if (flowerObject != null)
        {
            Highlight flowerHighlight = flowerObject.GetComponent<Highlight>();
            if (flowerHighlight != null)
            {
                flowerHighlight.HideObject();
            }
            else
            {
                flowerObject.SetActive(false);
            }
        }
        
        // 隐藏隹物体
        if (zhuiObject != null)
        {
            Highlight zhuiHighlight = zhuiObject.GetComponent<Highlight>();
            if (zhuiHighlight != null)
            {
                zhuiHighlight.HideObject();
            }
            else
            {
                zhuiObject.SetActive(false);
            }
        }
        
        // 也查找场景中的花和隹物体并隐藏
        Highlight[] allHighlights = FindObjectsOfType<Highlight>(true);
        foreach (Highlight highlight in allHighlights)
        {
            if (highlight != null && (highlight.letter == "花" || highlight.letter == "隹"))
            {
                highlight.HideObject();
            }
        }
        
        if (enableDebugLog)
        {
            Debug.Log("BeachObject: 已隐藏花和隹物体");
        }
    }
}
