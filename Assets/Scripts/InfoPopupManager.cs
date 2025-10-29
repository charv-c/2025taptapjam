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
    
    // E键和ESC键监听协程
    private Coroutine keyListenerCoroutine;
    
    // 操作禁用状态记录
    private bool operationsDisabledByPopup = false;
    
    // 外部可读：当前是否有InfoPopup在显示
    public bool IsPopupActive
    {
        get { return currentPopupInstance != null && currentPopupInstance.activeInHierarchy; }
    }
    
    // ESC键禁用状态记录
    private bool escKeyDisabled = false;

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
    /// 查找没有父物体的顶层Canvas
    /// </summary>
    /// <returns>顶层Canvas，如果未找到则返回null</returns>
    private Canvas FindTopLevelCanvas()
    {
        // 获取场景中所有的Canvas
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        
        // 查找没有父物体的Canvas（顶层Canvas）
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.transform.parent == null)
            {
                GameLogger.LogSystem($"InfoPopupManager: 找到顶层Canvas: {canvas.name}");
                return canvas;
            }
        }
        
        GameLogger.LogWarning("InfoPopupManager: 未找到没有父物体的顶层Canvas");
        return null;
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
        // 确保在顶层Canvas下实例化（没有父物体的Canvas）
        Canvas mainCanvas = FindTopLevelCanvas();
        if (mainCanvas == null)
        {
            GameLogger.LogError("InfoPopupManager: 场景中未找到顶层Canvas！");
            return;
        }
        
        GameLogger.LogSystem($"InfoPopupManager: 找到顶层Canvas: {mainCanvas.name}，开始实例化弹窗预制体");
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
        // 设置排序值，确保在退出窗格之下（退出窗格为22，InfoPopup为10）
        popupCanvas.sortingOrder = 10;
        
        // 添加GraphicRaycaster组件确保按钮能接收点击事件
        GraphicRaycaster popupRaycaster = currentPopupInstance.GetComponent<GraphicRaycaster>();
        if (popupRaycaster == null)
        {
            popupRaycaster = currentPopupInstance.AddComponent<GraphicRaycaster>();
        }
        
        GameLogger.LogSystem("InfoPopupManager: 已设置弹窗Canvas为置顶显示，并添加GraphicRaycaster");
        
        // 设置弹窗位置为屏幕顶端
        RectTransform panelRect = currentPopupInstance.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            // 设置为屏幕顶端
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -50f); // 距离顶部50像素
            panelRect.pivot = new Vector2(0.5f, 1f);
            
            GameLogger.LogSystem($"InfoPopupManager: 已设置弹窗位置为屏幕顶端 - Anchors: {panelRect.anchorMin} to {panelRect.anchorMax}, Position: {panelRect.anchoredPosition}");
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
        
        // 确保按钮的交互性不受其他管理器影响
        continueButton.interactable = true;
        
        // 设置按钮名称为 "Continue"
        continueButton.gameObject.name = "Continue";
        
        // 确保按钮的Image组件raycastTarget为true，防止被TutorialManager禁用
        UnityEngine.UI.Image buttonImage = continueButton.GetComponent<UnityEngine.UI.Image>();
        if (buttonImage != null)
        {
            buttonImage.raycastTarget = true;
            GameLogger.LogSystem("InfoPopupManager: 已确保按钮Image的raycastTarget为true");
        }
        
        // 确保按钮的Text组件raycastTarget为false，避免干扰点击检测
        TextMeshProUGUI buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.raycastTarget = false;
            GameLogger.LogSystem("InfoPopupManager: 已设置按钮Text的raycastTarget为false");
        }
        
        // 确保按钮的Transition设置正确，支持悬停效果
        ColorBlock colors = continueButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        colors.selectedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        continueButton.colors = colors;
        
        GameLogger.LogSystem("InfoPopupManager: 已设置按钮颜色和交互性");

        // 禁用所有玩家操作
        DisableAllPlayerOperations();
        
        // 禁用退出窗格（防止ESC键打开退出对话框）
        if (ExitGameManager.Instance != null)
        {
            ExitGameManager.Instance.SetExitDialogDisabled(true);
            GameLogger.LogSystem("InfoPopupManager: 已禁用退出窗格");
        }

        // 开始按键监听协程（E键和ESC键）
        keyListenerCoroutine = StartCoroutine(KeyListenerCoroutine());

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
            
            // 确保按钮始终可交互，防止被其他管理器影响
            if (continueButton != null)
            {
                continueButton.interactable = true;
                continueButton.gameObject.name = "Continue";
                
                UnityEngine.UI.Image buttonImage = continueButton.GetComponent<UnityEngine.UI.Image>();
                if (buttonImage != null)
                {
                    buttonImage.raycastTarget = true;
                }
                
                TextMeshProUGUI buttonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.raycastTarget = false;
                }
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
        // 停止按键监听协程
        if (keyListenerCoroutine != null)
        {
            StopCoroutine(keyListenerCoroutine);
            keyListenerCoroutine = null;
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
        
        // 引导完成后允许退出弹窗
        if (ExitGameManager.Instance != null)
        {
            ExitGameManager.Instance.SetExitDialogDisabled(false);
            GameLogger.LogSystem("InfoPopupManager: 引导完成，已允许退出弹窗");
        }

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
        
        // 重置ESC键禁用状态
        escKeyDisabled = false;
    }
    
    /// <summary>
    /// 按键监听协程（E键和ESC键）
    /// </summary>
    private System.Collections.IEnumerator KeyListenerCoroutine()
    {
        while (currentPopupInstance != null && currentPopupInstance.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.E) && !escKeyDisabled)
            {
                GameLogger.LogSystem("InfoPopupManager: 检测到E键按下，触发继续");
                OnContinueClicked();
                
                // 等待一小段时间避免重复触发，然后继续监听下一次E键按下
                yield return new WaitForSeconds(0.2f);
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameLogger.LogSystem("InfoPopupManager: 检测到ESC键按下，但不执行任何操作");
                // 不关闭弹窗，不打开退出窗格，不禁用Continue按钮，不禁用E键
            }
            yield return null;
        }
        
        GameLogger.LogSystem("InfoPopupManager: 按键监听协程结束");
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
        
        // 禁用字符串按钮选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.DisableCharacterSelection();
            GameLogger.LogSystem("InfoPopupManager: 已禁用字符串按钮选择");
        }
        else
        {
            GameLogger.LogWarning("InfoPopupManager: 未找到ButtonController，无法禁用字符串选择");
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
            // 启用所有玩家的移动和F键响应
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
        
        // 恢复字符串按钮选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.EnableCharacterSelection();
            GameLogger.LogSystem("InfoPopupManager: 已恢复字符串按钮选择");
        }
        else
        {
            GameLogger.LogWarning("InfoPopupManager: 未找到ButtonController，无法恢复字符串选择");
        }

        operationsDisabledByPopup = false;
    }
    
    /// <summary>
    /// 恢复功能（用于退出弹窗取消时调用，现在不需要恢复任何功能）
    /// </summary>
    public void RestoreEKeyAndContinueButton()
    {
        // 现在ESC键不执行任何操作，所以不需要恢复任何功能
        GameLogger.LogSystem("InfoPopupManager: ESC键不执行任何操作，无需恢复功能");
    }
}
