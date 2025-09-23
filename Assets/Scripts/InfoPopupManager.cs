using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 通用信息弹窗管理器 V1.0
/// 负责实例化和管理一个可复用的信息弹窗，用于显示关卡介绍、通关结语等。
/// </summary>
public class InfoPopupManager : MonoBehaviour
{
    // 单例实例
    public static InfoPopupManager Instance { get; private set; }

    [Header("UI 预制体")]
    [Tooltip("信息弹窗的UI预制体")]
    [SerializeField] private GameObject popupPanelPrefab;

    // 当前活动弹窗的实例
    private GameObject currentPopupInstance;
    private TextMeshProUGUI messageText;
    private Button continueButton;

    // 弹窗显示所需的数据
    private Queue<string> messageQueue;
    private System.Action onCompleteCallback;
    private System.Action<int, string> onMessageShownCallback;
    private int currentMessageIndex;
    private string[] originalMessages;
    private string customButtonText;
    
    // E键监听协程
    private Coroutine eKeyListenerCoroutine;
    
    // 操作禁用状态记录
    private bool operationsDisabledByPopup = false;

    private void Awake()
    {
        // 实现单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 同样设置为跨场景，方便随时调用
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 显示信息弹窗。
    /// </summary>
    /// <param name="messages">要分步显示的消息数组。</param>
    /// <param name="onComplete">所有消息显示完毕并点击按钮后要执行的回调函数。</param>
    /// <param name="onMessageShown">每条消息显示时的回调函数，参数为消息索引和内容。</param>
    /// <param name="customButtonText">自定义按钮文案，为空时使用默认文案"点击继续"。</param>
    public void ShowPopup(string[] messages, System.Action onComplete, System.Action<int, string> onMessageShown = null, string customButtonText = null)
    {
        GameLogger.LogSystem($"InfoPopupManager: ShowPopup被调用，消息数量: {messages?.Length ?? 0}");
        
        if (popupPanelPrefab == null)
        {
            GameLogger.LogError("InfoPopupManager: 未设置弹窗预制体 (popupPanelPrefab)！");
            return;
        }

        if (messages == null || messages.Length == 0)
        {
            GameLogger.LogWarning("InfoPopupManager: 消息数组为空，直接执行回调。");
            onComplete?.Invoke();
            return;
        }

        // 实例化弹窗
        // 确保在顶层Canvas下实例化
        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            GameLogger.LogError("InfoPopupManager: 场景中未找到Canvas！");
            return;
        }
        
        GameLogger.LogSystem($"InfoPopupManager: 找到Canvas: {mainCanvas.name}，开始实例化弹窗预制体");
        currentPopupInstance = Instantiate(popupPanelPrefab, mainCanvas.transform);
        
        // 确保实例化的弹窗是激活状态
        currentPopupInstance.SetActive(true);
        GameLogger.LogSystem("InfoPopupManager: 弹窗预制体已激活");
        
        // 确保弹窗置顶显示 - 添加Canvas组件并设置最高排序顺序
        Canvas popupCanvas = currentPopupInstance.GetComponent<Canvas>();
        if (popupCanvas == null)
        {
            popupCanvas = currentPopupInstance.AddComponent<Canvas>();
        }
        // 开启覆盖排序，使其独立于父Canvas的排序
        popupCanvas.overrideSorting = true;
        // 设置一个非常高的排序值，确保它在所有UI之上
        popupCanvas.sortingOrder = 30000;
        GameLogger.LogSystem("InfoPopupManager: 已设置弹窗Canvas为置顶显示");
        
        // 保持预制体的原始位置配置，不做强制调整
        RectTransform panelRect = currentPopupInstance.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            GameLogger.LogSystem($"InfoPopupManager: 使用预制体原始位置配置 - Anchors: {panelRect.anchorMin} to {panelRect.anchorMax}, Position: {panelRect.anchoredPosition}, 尺寸: {panelRect.sizeDelta}");
        }

        // 获取UI组件引用
        messageText = currentPopupInstance.GetComponentInChildren<TextMeshProUGUI>();
        continueButton = currentPopupInstance.GetComponentInChildren<Button>();

        if (messageText == null || continueButton == null)
        {
            GameLogger.LogError("InfoPopupManager: 弹窗预制体中缺少TextMeshProUGUI或Button组件！");
            GameLogger.LogError($"InfoPopupManager: messageText为null: {messageText == null}, continueButton为null: {continueButton == null}");
            Destroy(currentPopupInstance);
            return;
        }

        GameLogger.LogSystem($"InfoPopupManager: UI组件引用获取成功 - messageText: {messageText.name}, continueButton: {continueButton.name}");

        // 准备数据和回调
        messageQueue = new Queue<string>(messages);
        onCompleteCallback = onComplete;
        onMessageShownCallback = onMessageShown;
        currentMessageIndex = 0;
        originalMessages = messages;
        this.customButtonText = customButtonText;
        
        GameLogger.LogSystem($"InfoPopupManager: 初始化完成，消息总数: {messages.Length}, 自定义按钮文案: '{customButtonText}'");

        // 添加按钮点击事件监听
        continueButton.onClick.AddListener(OnContinueClicked);

        // 禁用所有玩家操作
        DisableAllPlayerOperations();

        // 开始E键监听协程
        eKeyListenerCoroutine = StartCoroutine(EKeyListenerCoroutine());

        // 显示第一条消息
        GameLogger.LogSystem("InfoPopupManager: 准备显示第一条消息");
        ShowNextMessage();
    }

    /// <summary>
    /// “继续”按钮的点击事件处理。
    /// </summary>
    private void OnContinueClicked()
    {
        // 播放UI点击音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick();
        }
        
        ShowNextMessage();
    }

    /// <summary>
    /// 显示队列中的下一条消息，如果队列为空，则完成流程。
    /// </summary>
    private void ShowNextMessage()
    {
        if (messageQueue.Count > 0)
        {
            // 队列中还有消息，显示下一条
            string message = messageQueue.Dequeue();
            messageText.text = message;
            GameLogger.LogSystem($"InfoPopupManager: 显示消息 {currentMessageIndex}: {message}，剩余消息数: {messageQueue.Count}");
            
            // 如果这是最后一条消息且有自定义按钮文案，则设置按钮文案
            if (messageQueue.Count == 0 && !string.IsNullOrEmpty(customButtonText))
            {
                GameLogger.LogSystem($"InfoPopupManager: 准备设置自定义按钮文案: '{customButtonText}'");
                
                // 尝试多种方式找到按钮文字组件
                TextMeshProUGUI buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
                
                if (buttonText == null)
                {
                    // 如果GetComponentInChildren找不到，尝试直接在按钮对象上查找
                    buttonText = continueButton.GetComponent<TextMeshProUGUI>();
                }
                
                if (buttonText != null)
                {
                    string originalText = buttonText.text;
                    buttonText.text = customButtonText;
                    
                    // 强制刷新UI
                    buttonText.SetAllDirty();
                    buttonText.ForceMeshUpdate();
                    
                    GameLogger.LogSystem($"InfoPopupManager: 按钮文案设置成功！原文案: '{originalText}' -> 新文案: '{customButtonText}'");
                    GameLogger.LogSystem($"InfoPopupManager: 按钮文字组件名称: {buttonText.name}，当前文本: '{buttonText.text}'");
                }
                else
                {
                    GameLogger.LogError("InfoPopupManager: 无法找到按钮的TextMeshProUGUI组件！");
                    GameLogger.LogError($"InfoPopupManager: 继续按钮信息 - 名称: {continueButton.name}, 子对象数量: {continueButton.transform.childCount}");
                    
                    // 输出所有子对象信息进行调试
                    for (int i = 0; i < continueButton.transform.childCount; i++)
                    {
                        Transform child = continueButton.transform.GetChild(i);
                        GameLogger.LogError($"InfoPopupManager: 子对象 {i}: {child.name}, 类型: {child.GetComponent<Component>()?.GetType().Name ?? "null"}");
                    }
                }
            }
            else if (!string.IsNullOrEmpty(customButtonText))
            {
                GameLogger.LogSystem($"InfoPopupManager: 有自定义按钮文案但不是最后一条消息，剩余消息数: {messageQueue.Count}");
            }
            
            // 调用消息显示回调
            if (onMessageShownCallback != null)
            {
                onMessageShownCallback.Invoke(currentMessageIndex, message);
                GameLogger.LogSystem($"InfoPopupManager: 已调用消息显示回调，索引: {currentMessageIndex}");
            }
            
            currentMessageIndex++;
        }
        else
        {
            // 所有消息都已显示完毕
            GameLogger.LogSystem("InfoPopupManager: 所有消息已显示完毕，关闭弹窗");
            ClosePopup();
        }
    }

    /// <summary>
    /// 关闭弹窗并执行回调。
    /// </summary>
    private void ClosePopup()
    {
        // 停止E键监听协程
        if (eKeyListenerCoroutine != null)
        {
            StopCoroutine(eKeyListenerCoroutine);
            eKeyListenerCoroutine = null;
        }

        // 移除监听，销毁实例
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }
        if (currentPopupInstance != null)
        {
            Destroy(currentPopupInstance);
        }

        // 恢复所有玩家操作
        EnableAllPlayerOperations();

        // 执行完成回调
        onCompleteCallback?.Invoke();

        // 清理引用
        currentPopupInstance = null;
        messageText = null;
        continueButton = null;
        onCompleteCallback = null;
        onMessageShownCallback = null;
        currentMessageIndex = 0;
        originalMessages = null;
        customButtonText = null;
    }
    
    /// <summary>
    /// E键监听协程
    /// </summary>
    private System.Collections.IEnumerator EKeyListenerCoroutine()
    {
        while (currentPopupInstance != null && currentPopupInstance.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                GameLogger.LogSystem("InfoPopupManager: 检测到E键按下，触发继续");
                OnContinueClicked();
                
                // 等待一小段时间避免重复触发，然后继续监听下一次E键按下
                yield return new WaitForSeconds(0.2f);
            }
            yield return null;
        }
        
        GameLogger.LogSystem("InfoPopupManager: E键监听协程结束");
    }

    /// <summary>
    /// 禁用所有玩家操作
    /// </summary>
    private void DisableAllPlayerOperations()
    {
        if (operationsDisabledByPopup)
        {
            GameLogger.LogSystem("InfoPopupManager: 操作已被禁用，跳过重复禁用");
            return;
        }

        GameLogger.LogSystem("InfoPopupManager: 禁用所有玩家操作");
        
        // 查找PlayerController并禁用所有操作
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            for (int i = 0; i < playerController.GetPlayerCount(); i++)
            {
                Player player = playerController.GetPlayerByIndex(i);
                if (player != null)
                {
                    player.SetInputEnabled(false);
                    player.SetEnterKeyEnabled(false);
                }
            }
            playerController.DisablePlayerSwitching();
            GameLogger.LogSystem("InfoPopupManager: 已禁用PlayerController操作");
        }
        else
        {
            GameLogger.LogWarning("InfoPopupManager: 未找到PlayerController，无法禁用玩家操作");
        }

        operationsDisabledByPopup = true;
    }

    /// <summary>
    /// 恢复所有玩家操作
    /// </summary>
    private void EnableAllPlayerOperations()
    {
        if (!operationsDisabledByPopup)
        {
            GameLogger.LogSystem("InfoPopupManager: 操作未被弹窗禁用，跳过恢复");
            return;
        }

        GameLogger.LogSystem("InfoPopupManager: 恢复所有玩家操作");
        
        // 查找PlayerController并恢复所有操作
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            // 启用所有玩家的移动和回车键响应
            for (int i = 0; i < playerController.GetPlayerCount(); i++)
            {
                Player player = playerController.GetPlayerByIndex(i);
                if (player != null)
                {
                    player.SetInputEnabled(true);
                    player.SetEnterKeyEnabled(true);
                }
            }
            
            // 设置第一个玩家为当前玩家（如果没有设置的话）
            if (playerController.GetPlayerCount() > 0 && playerController.GetCurrentPlayerIndex() < 0)
            {
                playerController.SetCurrentPlayerIndex(0);
            }
            
            // 启用玩家切换功能
            playerController.EnablePlayerSwitching();
            
            // 更新玩家颜色状态
            playerController.UpdatePlayerColors();
            
            GameLogger.LogSystem("InfoPopupManager: 已恢复PlayerController操作");
        }
        else
        {
            GameLogger.LogWarning("InfoPopupManager: 未找到PlayerController，无法恢复玩家操作");
        }

        operationsDisabledByPopup = false;
    }
}
