using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 退出弹窗测试辅助工具
/// 用于诊断和测试退出弹窗的按钮显示问题
/// </summary>
public class ExitDialogTestHelper : MonoBehaviour
{
    [Header("测试设置")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool autoTestOnStart = false;
    
    [Header("按钮预制体（用于测试）")]
    [SerializeField] private GameObject testConfirmButtonPrefab;
    [SerializeField] private GameObject testCancelButtonPrefab;
    
    [Header("确认对话框预制体（用于测试）")]
    [SerializeField] private GameObject testConfirmationDialogPrefab;
    
    private void Start()
    {
        if (autoTestOnStart)
        {
            TestExitDialogFunctionality();
        }
    }
    
    /// <summary>
    /// 测试退出弹窗功能
    /// </summary>
    [ContextMenu("测试退出弹窗功能")]
    public void TestExitDialogFunctionality()
    {
        LogDebug("=== 开始测试退出弹窗功能 ===");
        
        if (ExitGameManager.Instance == null)
        {
            LogDebug("ExitGameManager实例不存在");
            return;
        }
        
        // 测试1：检查ExitGameManager状态
        LogDebug("测试1：检查ExitGameManager状态");
        ExitGameManager.Instance.CheckButtonStatus();
        
        // 测试2：设置测试预制体（如果提供了）
        if (testConfirmButtonPrefab != null && testCancelButtonPrefab != null)
        {
            LogDebug("测试2：设置测试按钮预制体");
            ExitGameManager.Instance.SetButtonPrefabs(testConfirmButtonPrefab, testCancelButtonPrefab);
        }
        
        // 测试3：强制重新初始化UI
        LogDebug("测试3：强制重新初始化UI");
        ExitGameManager.Instance.ForceReinitializeUI();
        
        // 测试4：再次检查按钮状态
        LogDebug("测试4：重新检查按钮状态");
        ExitGameManager.Instance.CheckButtonStatus();
        
        // 测试5：模拟显示退出弹窗
        LogDebug("测试5：模拟显示退出弹窗");
        ExitGameManager.Instance.TestExitGame();
        
        // 等待一帧后检查状态
        StartCoroutine(CheckStatusAfterDialog());
        
        LogDebug("=== 退出弹窗功能测试完成 ===");
    }
    
    /// <summary>
    /// 检查弹窗显示后的状态
    /// </summary>
    private System.Collections.IEnumerator CheckStatusAfterDialog()
    {
        yield return new WaitForSeconds(0.1f); // 等待弹窗显示
        
        LogDebug("弹窗显示后检查按钮状态：");
        ExitGameManager.Instance.CheckButtonStatus();
        
        // 等待几秒后自动关闭弹窗
        yield return new WaitForSeconds(2f);
        
        LogDebug("自动关闭弹窗");
        // 这里可以添加关闭弹窗的逻辑，如果需要的话
    }
    
    /// <summary>
    /// 测试按钮预制体设置
    /// </summary>
    [ContextMenu("测试按钮预制体设置")]
    public void TestButtonPrefabSetup()
    {
        LogDebug("=== 开始测试按钮预制体设置 ===");
        
        if (ExitGameManager.Instance == null)
        {
            LogDebug("ExitGameManager实例不存在");
            return;
        }
        
        if (testConfirmButtonPrefab == null)
        {
            LogDebug("测试确认按钮预制体未设置");
        }
        else
        {
            Button confirmButton = testConfirmButtonPrefab.GetComponent<Button>();
            if (confirmButton == null)
            {
                LogDebug("测试确认按钮预制体没有Button组件");
            }
            else
            {
                LogDebug("测试确认按钮预制体有效");
            }
        }
        
        if (testCancelButtonPrefab == null)
        {
            LogDebug("测试取消按钮预制体未设置");
        }
        else
        {
            Button cancelButton = testCancelButtonPrefab.GetComponent<Button>();
            if (cancelButton == null)
            {
                LogDebug("测试取消按钮预制体没有Button组件");
            }
            else
            {
                LogDebug("测试取消按钮预制体有效");
            }
        }
        
        // 设置预制体
        ExitGameManager.Instance.SetButtonPrefabs(testConfirmButtonPrefab, testCancelButtonPrefab);
        
        LogDebug("=== 按钮预制体设置测试完成 ===");
    }
    
    /// <summary>
    /// 测试场景切换后的按钮状态
    /// </summary>
    [ContextMenu("测试场景切换后按钮状态")]
    public void TestButtonStateAfterSceneChange()
    {
        LogDebug("=== 开始测试场景切换后按钮状态 ===");
        
        if (ExitGameManager.Instance == null)
        {
            LogDebug("ExitGameManager实例不存在");
            return;
        }
        
        LogDebug("场景切换后检查按钮状态：");
        ExitGameManager.Instance.CheckButtonStatus();
        
        LogDebug("尝试重新创建按钮：");
        ExitGameManager.Instance.RecreateButtons();
        
        LogDebug("重新创建后检查按钮状态：");
        ExitGameManager.Instance.CheckButtonStatus();
        
        LogDebug("=== 场景切换后按钮状态测试完成 ===");
    }
    
    /// <summary>
    /// 创建简单的测试按钮
    /// </summary>
    [ContextMenu("创建简单测试按钮")]
    public void CreateSimpleTestButtons()
    {
        LogDebug("=== 开始创建简单测试按钮 ===");
        
        // 创建简单的确认按钮预制体
        if (testConfirmButtonPrefab == null)
        {
            testConfirmButtonPrefab = CreateSimpleButton("TestConfirmButton", "确认", Color.red);
            LogDebug("已创建测试确认按钮预制体");
        }
        
        // 创建简单的取消按钮预制体
        if (testCancelButtonPrefab == null)
        {
            testCancelButtonPrefab = CreateSimpleButton("TestCancelButton", "取消", Color.green);
            LogDebug("已创建测试取消按钮预制体");
        }
        
        // 设置预制体
        ExitGameManager.Instance?.SetButtonPrefabs(testConfirmButtonPrefab, testCancelButtonPrefab);
        
        LogDebug("=== 简单测试按钮创建完成 ===");
    }
    
    /// <summary>
    /// 创建简单的按钮
    /// </summary>
    private GameObject CreateSimpleButton(string name, string text, Color color)
    {
        GameObject buttonObj = new GameObject(name);
        
        // 添加RectTransform
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(120, 40);
        
        // 添加Image组件
        Image image = buttonObj.AddComponent<Image>();
        image.color = color;
        
        // 添加Button组件
        Button button = buttonObj.AddComponent<Button>();
        
        // 创建文本子对象
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        Text textComponent = textObj.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.fontSize = 14;
        textComponent.color = Color.white;
        textComponent.alignment = TextAnchor.MiddleCenter;
        
        return buttonObj;
    }
    
    /// <summary>
    /// 调试日志输出
    /// </summary>
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[ExitDialogTestHelper] {message}");
        }
    }
}
