using UnityEngine;

/// <summary>
/// AutoHint调试器 - 帮助诊断AutoHint显示问题
/// </summary>
public class AutoHintDebugger : MonoBehaviour
{
    [Header("调试设置")]
    [SerializeField] private bool enableDebugLog = true;
    
    private AutoHint autoHint;
    private CanvasGroup canvasGroup;
    
    private void Start()
    {
        // 获取AutoHint组件
        autoHint = FindObjectOfType<AutoHint>();
        if (autoHint != null)
        {
            canvasGroup = autoHint.GetComponent<CanvasGroup>();
        }
        
        if (enableDebugLog)
        {
            LogAutoHintStatus();
        }
    }
    
    /// <summary>
    /// 记录AutoHint状态
    /// </summary>
    private void LogAutoHintStatus()
    {
        Debug.Log("=== AutoHint调试信息 ===");
        
        if (autoHint == null)
        {
            Debug.LogError("AutoHint组件未找到！");
            return;
        }
        
        Debug.Log($"AutoHint GameObject: {autoHint.gameObject.name}");
        Debug.Log($"AutoHint GameObject Active: {autoHint.gameObject.activeInHierarchy}");
        Debug.Log($"AutoHint Component Enabled: {autoHint.enabled}");
        
        if (canvasGroup != null)
        {
            Debug.Log($"CanvasGroup Alpha: {canvasGroup.alpha}");
            Debug.Log($"CanvasGroup BlocksRaycasts: {canvasGroup.blocksRaycasts}");
            Debug.Log($"CanvasGroup Interactable: {canvasGroup.interactable}");
        }
        else
        {
            Debug.LogWarning("CanvasGroup组件未找到！");
        }
        
        // 检查子物体
        var childTexts = autoHint.GetComponentsInChildren<TMPro.TMP_Text>(true);
        Debug.Log($"子物体TMP_Text数量: {childTexts.Length}");
        for (int i = 0; i < childTexts.Length; i++)
        {
            var text = childTexts[i];
            Debug.Log($"  TMP_Text[{i}]: {text.gameObject.name}, Active: {text.gameObject.activeInHierarchy}, Enabled: {text.enabled}");
        }
        
        // 检查Image组件
        var image = autoHint.GetComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            Debug.Log($"Image组件: Enabled={image.enabled}, Sprite={image.sprite?.name}");
        }
        
        Debug.Log("========================");
    }
    
    /// <summary>
    /// 测试AutoHint显示
    /// </summary>
    [ContextMenu("测试AutoHint显示")]
    public void TestAutoHintDisplay()
    {
        if (autoHint == null)
        {
            Debug.LogError("AutoHint组件未找到，无法测试");
            return;
        }
        
        Debug.Log("=== 测试AutoHint显示 ===");
        
        // 测试直接显示
        autoHint.ReceiveBroadcast("芽春季");
        
        // 延迟检查状态
        Invoke(nameof(CheckAutoHintStateAfterTest), 0.1f);
    }
    
    /// <summary>
    /// 测试后检查AutoHint状态
    /// </summary>
    private void CheckAutoHintStateAfterTest()
    {
        Debug.Log("=== 测试后AutoHint状态 ===");
        
        if (canvasGroup != null)
        {
            Debug.Log($"CanvasGroup Alpha: {canvasGroup.alpha}");
            Debug.Log($"CanvasGroup BlocksRaycasts: {canvasGroup.blocksRaycasts}");
            Debug.Log($"CanvasGroup Interactable: {canvasGroup.interactable}");
        }
        
        // 检查子物体文本
        var childTexts = autoHint.GetComponentsInChildren<TMPro.TMP_Text>(true);
        for (int i = 0; i < childTexts.Length; i++)
        {
            var text = childTexts[i];
            Debug.Log($"  TMP_Text[{i}]: Text='{text.text}', Active: {text.gameObject.activeInHierarchy}");
        }
        
        Debug.Log("========================");
    }
    
    /// <summary>
    /// 测试PublicData字典
    /// </summary>
    [ContextMenu("测试PublicData字典")]
    public void TestPublicDataDict()
    {
        Debug.Log("=== 测试PublicData字典 ===");
        
        if (PublicData.autoHintDict == null)
        {
            Debug.LogError("PublicData.autoHintDict为null！");
            return;
        }
        
        Debug.Log($"autoHintDict条目数量: {PublicData.autoHintDict.Count}");
        
        // 测试特定键
        string[] testKeys = { "芽春季", "芽夏季", "滩涂描述" };
        foreach (string key in testKeys)
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
    /// 强制显示AutoHint
    /// </summary>
    [ContextMenu("强制显示AutoHint")]
    public void ForceShowAutoHint()
    {
        if (autoHint == null)
        {
            Debug.LogError("AutoHint组件未找到，无法强制显示");
            return;
        }
        
        Debug.Log("=== 强制显示AutoHint ===");
        
        // 直接设置CanvasGroup
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            Debug.Log("已强制设置CanvasGroup为显示状态");
        }
        
        // 激活所有子物体
        var childTexts = autoHint.GetComponentsInChildren<TMPro.TMP_Text>(true);
        foreach (var text in childTexts)
        {
            text.gameObject.SetActive(true);
            text.text = "测试文本";
        }
        Debug.Log($"已激活 {childTexts.Length} 个子物体文本");
        
        Debug.Log("========================");
    }
    
    /// <summary>
    /// 重置AutoHint状态
    /// </summary>
    [ContextMenu("重置AutoHint状态")]
    public void ResetAutoHintState()
    {
        if (autoHint == null)
        {
            Debug.LogError("AutoHint组件未找到，无法重置");
            return;
        }
        
        Debug.Log("=== 重置AutoHint状态 ===");
        
        // 重置CanvasGroup
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            Debug.Log("已重置CanvasGroup为隐藏状态");
        }
        
        // 清空子物体文本
        var childTexts = autoHint.GetComponentsInChildren<TMPro.TMP_Text>(true);
        foreach (var text in childTexts)
        {
            text.text = "";
        }
        Debug.Log($"已清空 {childTexts.Length} 个子物体文本");
        
        Debug.Log("========================");
    }
}
