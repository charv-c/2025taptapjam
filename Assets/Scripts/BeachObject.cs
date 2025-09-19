using UnityEngine;
using System.Collections;
using System.Linq;

/// <summary>
/// 滩涂物体脚本 - 处理所有滩涂相关的特殊逻辑
/// 此脚本应附加到letter为"滩"的物体上
/// </summary>
public class BeachObject : MonoBehaviour
{
    [Header("物体引用")]
    [SerializeField] private GameObject flowerObject;  // 花物体
    [SerializeField] private GameObject zhuiObject;    // 隹物体
    [SerializeField] private GameObject yaObjectForZi; // “子”区域的“芽”物体
    [SerializeField] private GameObject guaObject;     // “瓜”物体

    [Header("延迟设置")]
    [SerializeField] private float delayBeforeShowZhui = 0.5f; // 显示隹物体前的延迟时间（秒）
    
    [Header("调试设置")]
    [SerializeField] private bool enableDebugLog = true;
    
    private PlayerController playerController;
    private Level3Manager level3Manager;
    private AutoHint autoHint;
    
    // 状态标记：跟踪芽是否真正被种下过
    private bool hasYaBeenPlanted = false;
    
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
        GameLogger.LogDev("=== 滩涂物体组件状态 ===");
        GameLogger.LogDev($"PlayerController: {(playerController != null ? "找到" : "未找到")}");
        GameLogger.LogDev($"Level3Manager: {(level3Manager != null ? "找到" : "未找到")}");
        GameLogger.LogDev($"AutoHint: {(autoHint != null ? "找到" : "未找到")}");
        GameLogger.LogDev($"花物体引用: {(flowerObject != null ? "已设置" : "未设置")}");
        GameLogger.LogDev($"隹物体引用: {(zhuiObject != null ? "已设置" : "未设置")}");
        GameLogger.LogDev("==========================");
    }
    
    
    /// <summary>
    /// 显示季节提示
    /// </summary>
    /// <param name="hintKey">提示键</param>
    private void ShowSeasonHint(string hintKey)
    {
        GameLogger.LogDev($"ShowSeasonHint: 显示季节提示，键: {hintKey}");
        
        // 通过广播系统显示季节提示
        if (BroadcastManager.Instance != null)
        {
            BroadcastManager.Instance.BroadcastToAll(hintKey);
            GameLogger.LogDev($"ShowSeasonHint: 已发送广播 '{hintKey}'");
        }
        else
        {
            GameLogger.LogWarning("ShowSeasonHint: 无法显示季节提示，BroadcastManager不可用");
        }
    }
    
    /// <summary>
    /// 启用指定字母的Highlight组件
    /// </summary>
    /// <param name="letter">字母</param>
    private void EnableHighlightByLetter(string letter)
    {
        // 查找场景中所有带有Highlight脚本的对象
        Highlight[] allHighlights = FindObjectsOfType<Highlight>();
        
        foreach (Highlight highlight in allHighlights)
        {
            if (highlight != null && highlight.letter == letter)
            {
                // 启用Highlight组件
                highlight.enabled = true;
                
                // 显示对象
                highlight.ShowObject();
                
                GameLogger.LogDev($"EnableHighlightByLetter: 已启用并显示字母 '{letter}' 的Highlight组件");
                return;
            }
        }
        
        GameLogger.LogWarning($"EnableHighlightByLetter: 未找到字母 '{letter}' 的Highlight组件");
    }
    
    /// <summary>
    /// 执行滩涂互动逻辑
    /// 由Highlight脚本调用
    /// </summary>
    /// <param name="carryCharacter">玩家携带的字符</param>
    public void ExecuteBeachInteraction(string carryCharacter = "")
    {
        if (enableDebugLog)
        {
            GameLogger.LogDev($"BeachObject: 开始执行滩涂互动逻辑，玩家携带字符: '{carryCharacter}'");
        }
        
        // 如果没有传递字符参数，尝试获取当前激活玩家的携带字符
        string playerChar = carryCharacter;
        
        if (playerChar == "芽")
        {
            // 检查当前季节
            bool isSpring = IsCurrentSeasonSpring();
            
            if (isSpring)
            {
                // 春季：显示等待提示
                ShowAutoHint("芽春季");
            }
            else
            {
                // 非春季（夏季）：执行芽的绽放逻辑
                ExecuteYaBloomingLogic();
            }
        }
        else if (playerChar == "籽")
        {
            // 检查当前季节
            bool isSpring = IsCurrentSeasonSpring();

            if (isSpring)
            {
                // 春季：显示“子”区域的“芽”
                ExecuteZiPlantingLogic();
            }
            else
            {
                // 夏季：显示等待提示
                ShowAutoHint("籽夏季");
            }
        }
        else
        {
            // 其他字符：显示滩涂描述
            ShowAutoHint("滩涂描述");
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
                    GameLogger.LogDev($"BeachObject: Player1携带字符 = {carryChar}");
                }
                return carryChar;
            }
        }
        
        if (enableDebugLog)
        {
            GameLogger.LogWarning("BeachObject: 无法获取Player1的携带字符");
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
                GameLogger.LogDev($"BeachObject: 当前季节 = {(isSpring ? "春季" : "夏季")}");
            }
            return isSpring;
        }
        
        if (enableDebugLog)
        {
            GameLogger.LogWarning("BeachObject: 无法获取季节信息");
        }
        return false;
    }
    
    /// <summary>
    /// 执行"籽"的种植逻辑
    /// </summary>
    private void ExecuteZiPlantingLogic()
    {
        if (enableDebugLog)
        {
            GameLogger.LogDev("BeachObject: 执行“籽”的种植逻辑");
        }

        // 显示"子"区域的"芽"物体（"芽"不可交互）
        ShowObject(yaObjectForZi, "芽");
        
        // 播放籽发芽音效 (Level3)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySeedSprout();
            if (enableDebugLog)
            {
                GameLogger.LogDev("BeachObject: 播放籽发芽音效");
            }
        }
        
        // 设置芽已种下的标记
        hasYaBeenPlanted = true;
        
        if (enableDebugLog)
        {
            GameLogger.LogDev("BeachObject: 芽已被种下，设置hasYaBeenPlanted = true");
        }
        
        // 显示提示
        ShowAutoHint("籽春季");
    }

    /// <summary>
    /// 当季节从春季切换到夏季时，由Level3Manager调用
    /// </summary>
    public void TransformYaToGuaOnSeasonChange()
    {
        if (enableDebugLog)
        {
            GameLogger.LogDev($"BeachObject: TransformYaToGuaOnSeasonChange 被调用");
            GameLogger.LogDev($"BeachObject: hasYaBeenPlanted = {hasYaBeenPlanted}");
            GameLogger.LogDev($"BeachObject: yaObjectForZi = {(yaObjectForZi != null ? yaObjectForZi.name : "null")}");
            GameLogger.LogDev($"BeachObject: yaObjectForZi.activeInHierarchy = {(yaObjectForZi != null ? yaObjectForZi.activeInHierarchy.ToString() : "null")}");
        }
        
        // 检查芽是否真正被种下过，而不仅仅是检查GameObject是否激活
        if (hasYaBeenPlanted && yaObjectForZi != null && yaObjectForZi.activeInHierarchy)
        {
            if (enableDebugLog)
            {
                GameLogger.LogDev("BeachObject: 检测到季节切换（春->夏）且\"子\"区域的\"芽\"已种下，执行变换逻辑。");
            }

            // 隐藏"芽"
            HideObject(yaObjectForZi, "芽");
            
            // 显示"瓜"（可交互）
            ShowObject(guaObject, "瓜");

            // 显示提示
            ShowAutoHint("芽变瓜");
        }
        else
        {
            if (enableDebugLog)
            {
                GameLogger.LogWarning("BeachObject: 芽变瓜条件不满足，跳过变换逻辑");
            }
        }
    }

    /// <summary>
    /// 执行芽的绽放逻辑
    /// </summary>
    private void ExecuteYaBloomingLogic()
    {
        if (enableDebugLog)
        {
            GameLogger.LogDev("BeachObject: 执行芽的绽放逻辑");
        }
        
        // 显示花物体
        ShowObject(flowerObject, "花");
        
        // 显示提示
        ShowAutoHint("芽夏季");
        
        // 延迟显示隹物体
        StartCoroutine(ShowZhuiObjectAfterDelay());
    }
    
    /// <summary>
    /// 显示花物体
    /// </summary>
    private void ShowFlowerObject()
    {
        ShowObject(flowerObject, "花");
    }

    private void ShowObject(GameObject obj, string objectName)
    {
        if (obj != null)
        {
            // 使用Highlight脚本显示物体
            Highlight highlight = obj.GetComponent<Highlight>();
            if (highlight != null)
            {
                // 根据物体类型决定是否启用交互
                // "隹"和"瓜"应该可交互，使用ShowObject()
                // "花"和"芽"不可交互，使用Show()
                if (objectName == "隹" || objectName == "瓜")
                {
                    highlight.ShowObject(); // 显示并启用交互
                    if (enableDebugLog)
                    {
                        GameLogger.LogDev($"BeachObject: 已显示可交互{objectName}物体 - {obj.name}");
                    }
                }
                else
                {
                    highlight.Show(); // 仅显示，不启用交互
                    if (enableDebugLog)
                    {
                        GameLogger.LogDev($"BeachObject: 已显示不可交互{objectName}物体 - {obj.name}");
                    }
                }
            }
            else
            {
                // 如果没有Highlight脚本，直接激活GameObject
                obj.SetActive(true);
                if (enableDebugLog)
                {
                    GameLogger.LogDev($"BeachObject: 已激活{objectName}物体 - {obj.name}");
                }
            }
        }
        else
        {
            GameLogger.LogWarning($"BeachObject: {objectName}物体的引用未设置，无法显示。");
        }
    }

    private void HideObject(GameObject obj, string objectName)
    {
        if (obj != null)
        {
            Highlight highlight = obj.GetComponent<Highlight>();
            if (highlight != null)
            {
                highlight.Hide();
                if (enableDebugLog)
                {
                    GameLogger.LogDev($"BeachObject: 已通过Highlight隐藏{objectName}物体 - {obj.name}");
                }
            }
            else
            {
                obj.SetActive(false);
                if (enableDebugLog)
                {
                    GameLogger.LogDev($"BeachObject: 已禁用{objectName}物体 - {obj.name}");
                }
            }
        }
        else
        {
            GameLogger.LogWarning($"BeachObject: {objectName}物体的引用未设置，无法隐藏。");
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
                    GameLogger.LogDev($"BeachObject: 已显示花物体 - {highlight.gameObject.name}");
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
            GameLogger.LogDev($"BeachObject: 等待 {delayBeforeShowZhui} 秒后显示隹物体");
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
        ShowObject(zhuiObject, "隹");
        
        // 播放鸟叫音效 (Level3)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBirdCall();
            if (enableDebugLog)
            {
                GameLogger.LogDev("ShowZhuiObject: 播放鸟叫音效");
            }
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
                    GameLogger.LogDev($"BeachObject: 已显示隹物体 - {highlight.gameObject.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// 显示自动提示 - 通过广播系统触发autoHint字典中的对应提示
    /// </summary>
    /// <param name="hintKey">提示键（对应autoHint字典中的键）</param>
    private void ShowAutoHint(string hintKey)
    {
        if (enableDebugLog)
        {
            GameLogger.LogDev($"BeachObject: 尝试显示提示，键: '{hintKey}'");
        }
        
        // 检查autoHintDict中是否有对应的键
        
        if (PublicData.autoHintDict != null && PublicData.autoHintDict.ContainsKey(hintKey))
        {
            string hintText = PublicData.autoHintDict[hintKey];
            if (enableDebugLog)
            {
                GameLogger.LogDev($"BeachObject: 找到提示文本: '{hintText}'");
            }
            
            // 直接调用AutoHint显示提示
            if (autoHint != null)
            {
                autoHint.ReceiveBroadcast(hintKey);
                if (enableDebugLog)
                {
                    GameLogger.LogDev($"BeachObject: 已调用AutoHint显示提示: '{hintText}'");
                }
            }
            else
            {
                GameLogger.LogWarning("BeachObject: AutoHint组件为空，无法显示提示");
            }
        }
        else
        {
            if (enableDebugLog)
            {
                GameLogger.LogWarning($"BeachObject: autoHintDict中未找到键 '{hintKey}'");
            }
        }
    }
    
    /// <summary>
    /// 在Inspector中测试滩涂互动
    /// </summary>
    [ContextMenu("测试滩涂互动")]
    public void TestBeachInteraction()
    {
        GameLogger.LogDev("BeachObject: 开始测试滩涂互动");
        ExecuteBeachInteraction();
    }
    
    /// <summary>
    /// 检查瓜对象是否已完成互动（玩家已化字为"孤"）
    /// </summary>
    /// <returns>如果瓜对象已完成互动则返回true</returns>
    public bool IsGuaInteractionCompleted()
    {
        // 检查瓜对象是否存在且可见
        if (guaObject == null)
        {
            return true; // 瓜对象不存在，认为互动已完成
        }
        
        Highlight guaHighlight = guaObject.GetComponent<Highlight>();
        if (guaHighlight == null)
        {
            return true; // 瓜对象没有Highlight组件，认为互动已完成
        }
        
        // 检查瓜对象是否已被隐藏或禁用（通常意味着互动已完成）
        SpriteRenderer guaSpriteRenderer = guaObject.GetComponent<SpriteRenderer>();
        if (guaSpriteRenderer != null && !guaSpriteRenderer.enabled)
        {
            if (enableDebugLog)
            {
                GameLogger.LogDev("BeachObject: 瓜对象已被隐藏，互动已完成");
            }
            return true;
        }
        
        if (!guaHighlight.enabled || !guaObject.activeInHierarchy)
        {
            if (enableDebugLog)
            {
                GameLogger.LogDev("BeachObject: 瓜对象组件已禁用或GameObject未激活，互动已完成");
            }
            return true;
        }
        
        // 检查是否有玩家已化字为"孤"（表示已与瓜互动）
        if (playerController != null)
        {
            for (int i = 0; i < 2; i++) // 检查两个玩家
            {
                Player player = playerController.GetPlayerByIndex(i);
                if (player != null && player.CarryCharacter == "孤")
                {
                    if (enableDebugLog)
                    {
                        GameLogger.LogDev($"BeachObject: 检测到玩家{i + 1}已化字为'孤'，瓜互动已完成");
                    }
                    return true;
                }
            }
        }
        
        if (enableDebugLog)
        {
            GameLogger.LogDev("BeachObject: 瓜对象互动尚未完成");
        }
        return false;
    }
    
    /// <summary>
    /// 检查瓜对象是否可见且可交互
    /// </summary>
    /// <returns>如果瓜对象可见且可交互则返回true</returns>
    public bool IsGuaObjectVisible()
    {
        if (guaObject == null) return false;
        
        // 检查GameObject是否激活
        if (!guaObject.activeInHierarchy) return false;
        
        // 检查SpriteRenderer是否启用
        SpriteRenderer spriteRenderer = guaObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && !spriteRenderer.enabled) return false;
        
        // 检查Highlight组件是否启用
        Highlight highlight = guaObject.GetComponent<Highlight>();
        if (highlight != null && !highlight.enabled) return false;
        
        return true;
    }
    
    /// <summary>
    /// 当瓜对象互动完成后调用，用于恢复滩涂的正常交互状态
    /// </summary>
    public void OnGuaInteractionCompleted()
    {
        if (enableDebugLog)
        {
            GameLogger.LogDev("BeachObject: 瓜互动完成，恢复滩涂交互状态");
        }
        
        // 查找滩涂对象并恢复其交互状态
        Highlight[] allHighlights = FindObjectsOfType<Highlight>();
        foreach (Highlight highlight in allHighlights)
        {
            if (highlight != null && highlight.letter == "滩")
            {
                // 确保滩涂对象保持可交互状态
                if (!highlight.enabled)
                {
                    highlight.enabled = true;
                    if (enableDebugLog)
                    {
                        GameLogger.LogDev($"BeachObject: 恢复滩涂对象交互功能: '{highlight.gameObject.name}'");
                    }
                }
                break;
            }
        }
    }
    
    /// <summary>
    /// 重置滩涂状态
    /// </summary>
    [ContextMenu("重置滩涂状态")]
    public void ResetBeachState()
    {
        if (enableDebugLog)
        {
            GameLogger.LogDev("BeachObject: 重置滩涂状态");
        }
        
        // 重置芽种植状态标记
        hasYaBeenPlanted = false;
        
        // 隐藏芽和瓜物体
        if (yaObjectForZi != null)
        {
            HideObject(yaObjectForZi, "芽");
        }
        if (guaObject != null)
        {
            HideObject(guaObject, "瓜");
        }
        
        // 隐藏花和隹物体
        HideFlowerAndZhuiObjects();
        
        // 恢复滩涂交互状态
        OnGuaInteractionCompleted();
    }
    
    /// <summary>
    /// 隐藏花和隹物体
    /// </summary>
    private void HideFlowerAndZhuiObjects()
    {
        // 隐藏花物体
        HideObject(flowerObject, "花");
        
        // 隐藏隹物体
        HideObject(zhuiObject, "隹");
        
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
            GameLogger.LogDev("BeachObject: 已隐藏花和隹物体");
        }
    }
    
    /// <summary>
    /// 获取芽是否已被种下的状态（用于存档）
    /// </summary>
    public bool GetHasYaBeenPlanted()
    {
        return hasYaBeenPlanted;
    }
    
    /// <summary>
    /// 设置芽是否已被种下的状态（用于存档恢复）
    /// </summary>
    public void SetHasYaBeenPlanted(bool planted)
    {
        hasYaBeenPlanted = planted;
        if (enableDebugLog)
        {
            GameLogger.LogDev($"BeachObject: 设置hasYaBeenPlanted = {planted}");
        }
    }
}
