using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class ButtonController : MonoBehaviour
{
    [Header("按钮设置")]
    [SerializeField] private Button splitButton, combineButton;
    // [SerializeField] private float hideDelay = 0.1f; // 已移除未使用的字段
    
    [Header("UI提示设置")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float messageDisplayTime = 3f;
    
    [Header("选择器引用")]
    [SerializeField] private StringSelector stringSelector;
    
    [Header("飞行动画设置")]
    [SerializeField] private float spiralTurns = 3f; // 螺旋圈数
    [SerializeField] private Transform targetPosition; // 目标位置
    [SerializeField] private Canvas targetCanvas; // 目标Canvas
    // [SerializeField] private float flyDuration = 1.5f; // 已移除未使用的字段
    // [SerializeField] private float spiralRadius = 50f; // 已移除未使用的字段
    
    [Header("字体设置")]
    [SerializeField] private TMP_FontAsset chineseFont; // 中文字体
    [SerializeField] private float flyingFontSize = 50f; // 飞行动画中文字大小
    
    [Header("彩蛋提示框设置")]
    [SerializeField] private GameObject easterEggMask; // 彩蛋遮罩层（阻止背景交互）
    [SerializeField] private GameObject easterEggPanel; // 彩蛋提示框面板
    [SerializeField] private TextMeshProUGUI easterEggText; // 彩蛋提示文本
    [SerializeField] private Image easterEggGuideImage; // 彩蛋引导员图像
    [SerializeField] private Button easterEggContinueButton; // 彩蛋继续按钮
    
    // 单例模式，方便其他脚本访问
    public static ButtonController Instance { get; private set; }
    
    // 飞行动画状态
    private bool isFlyingAnimationActive = false;
    private bool isLevel1Flying = false; // Level1飞舞状态
    
    // 教程模式控制
    private bool isTutorialMode = false;

    // 对外只读：是否处于飞字动画中（用于ESC弹窗禁用逻辑）
    public bool IsFlyingAnimationActive => isFlyingAnimationActive;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // 确保广播管理器存在
        EnsureBroadcastManagerExists();
        
        if (messageText != null) messageText.gameObject.SetActive(false);
        
        // 初始化彩蛋提示框和遮罩（确保开始时隐藏）
        if (easterEggMask != null)
        {
            easterEggMask.SetActive(false);
        }
        if (easterEggPanel != null)
        {
            easterEggPanel.SetActive(false);
        }
        
        UpdateButtonStates(0);
        
        if (splitButton != null)
        {
            splitButton.onClick.AddListener(OnSplitButtonClicked);
        }
        
        if (combineButton != null)
        {
            combineButton.onClick.AddListener(OnCombineButtonClicked);
        }
        
        // 订阅StringSelector的可用字符串变化事件
        if (stringSelector != null)
        {
            stringSelector.OnAvailableStringsChanged += OnAvailableStringsChanged;
            GameLogger.LogDev("ButtonController: 已订阅StringSelector的可用字符串变化事件");
        }
    }

    private void OnSplitButtonClicked()
    {
        GameLogger.LogDev("ButtonController: OnSplitButtonClicked() 开始执行");
        // 飞行动画期间禁止操作
        if (isFlyingAnimationActive) return;
        
        if (AudioManager.Instance != null && AudioManager.Instance.sfxUIClick != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxUIClick);
        }
        splitletter();
    }
    
    private void OnCombineButtonClicked()
    {
        // 飞行动画期间禁止操作
        if (isFlyingAnimationActive) return;
        
        if (AudioManager.Instance != null && AudioManager.Instance.sfxUIClick != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxUIClick);
        }
        combineletter();
    }
    
    private int GetCurrentSelectionCount()
    {
        if (stringSelector != null)
        {
            return stringSelector.GetSelectionCount();
        }
        return 0;
    }
    
    public void UpdateButtonStates(int selectedCount)
    {
        // 飞行动画期间禁用所有按钮
        if (isFlyingAnimationActive)
        {
            if (splitButton != null)
            {
                splitButton.interactable = false;
            }
            
            if (combineButton != null)
            {
                combineButton.interactable = false;
            }
            return;
        }
        
        // 在教程模式下，按钮的状态由TutorialManager控制
        if (!isTutorialMode)
        {
            if (splitButton != null)
            {
                splitButton.interactable = selectedCount == 1;
            }
            
            if (combineButton != null)
            {
                combineButton.interactable = selectedCount == 2;
            }
        }
        
        // 通知TutorialManager字符选择发生变化
        NotifyTutorialManagerOfSelectionChange();
    }
    
    private void HideAllButtons()
    {
        if (splitButton != null) splitButton.gameObject.SetActive(false);
        if (combineButton != null) combineButton.gameObject.SetActive(false);
        
    }
    
    private void ShowSplitAndCombineButtons()
    {
        if (splitButton != null) splitButton.gameObject.SetActive(true);
        if (combineButton != null) combineButton.gameObject.SetActive(true);
        
        if (stringSelector != null)
        {
            UpdateButtonStates(stringSelector.GetSelectionCount());
        }
    }
    
    // 确保广播管理器存在
    private void EnsureBroadcastManagerExists()
    {
        if (BroadcastManager.Instance == null)
        {
            // 创建空对象
            GameObject managerObject = new GameObject("BroadcastManager");

            // 添加BroadcastManager组件
            BroadcastManager manager = managerObject.AddComponent<BroadcastManager>();

            GameLogger.LogDev("ButtonController: 已创建广播管理器");
        }
        else
        {
            GameLogger.LogDev("ButtonController: 广播管理器已存在");
        }
    }
    
    private void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(true);
            StartCoroutine(HideMessageAfterDelay());
        }
    }
    
    private void HideMessage()
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }
    
    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDisplayTime);
        HideMessage();
    }
    
    private void splitletter()
    {
        GameLogger.LogDev("ButtonController: splitletter() 开始执行");
        
        if (stringSelector != null)
        {
            int selectedCount = stringSelector.GetSelectionCount();
            GameLogger.LogDev($"ButtonController: 当前选中字符数量: {selectedCount}");
            
            if (selectedCount != 1)
            {
                GameLogger.LogWarning($"ButtonController: 选中字符数量不正确，期望1个，实际{selectedCount}个，清除选择");
                stringSelector.ClearSelection();
                return;
            }
            
            string selectedString = stringSelector.FirstSelectedString;
            GameLogger.LogDev($"ButtonController: 选中的字符: '{selectedString}'");
            
            if (!string.IsNullOrEmpty(selectedString))
            {
                GameLogger.LogDev($"ButtonController: 检查字符 '{selectedString}' 是否可以拆分");
                
                if (PublicData.CanSplitString(selectedString))
                {
                    var (part1, part2) = PublicData.GetStringSplit(selectedString);
                    GameLogger.LogDev($"ButtonController: 字符 '{selectedString}' 可以拆分，结果为: '{part1}' 和 '{part2}'");
                    
                    if (AudioManager.Instance != null && AudioManager.Instance.sfxSplitSuccess != null)
                    {
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxSplitSuccess);
                        GameLogger.LogDev("ButtonController: 播放拆分成功音效");
                    }

                    // 记录被拆字符的原始索引
                    int oldIndex = stringSelector.IndexOfAvailableString(selectedString);
                    if (oldIndex < 0) oldIndex = 0;

                    // 清除选择
                    stringSelector.ClearSelection();
                    GameLogger.LogDev("ButtonController: 清除选择");

                    // 在原位置替换为拆分结果：先移除原字符，再按顺序插入两个结果
                    stringSelector.RemoveAvailableStringAt(oldIndex);
                    GameLogger.LogDev($"ButtonController: 从索引 {oldIndex} 处移除 '{selectedString}'");

                    stringSelector.InsertAvailableStringAt(part1, oldIndex);
                    GameLogger.LogDev($"ButtonController: 在索引 {oldIndex} 插入 '{part1}'");

                    stringSelector.InsertAvailableStringAt(part2, oldIndex + 1);
                    GameLogger.LogDev($"ButtonController: 在索引 {oldIndex + 1} 插入 '{part2}'");
                    
                    stringSelector.SetMaxSelectionCount(2);
                    GameLogger.LogDev("ButtonController: 设置最大选择数量为2");
                    
                    // 发送拆分成功广播：兼容旧事件 + 新格式"拆{被拆字符}"
                    if (BroadcastManager.Instance != null)
                    {
                        BroadcastManager.Instance.BroadcastToAll("split_success");
                        BroadcastManager.Instance.BroadcastToAll($"拆{selectedString}");
                        GameLogger.LogUser($"ButtonController: 发送拆分成功广播 (split_success, 拆{selectedString})");
                    }
                    
                    GameLogger.LogUser("ButtonController: 拆分操作完成");
                }
                else
                {
                    GameLogger.LogWarning($"ButtonController: 字符 '{selectedString}' 无法拆分");
                    
                    if (AudioManager.Instance != null && AudioManager.Instance.sfxOperationFailure != null)
                    {
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxOperationFailure);
                        GameLogger.LogDev("ButtonController: 播放操作失败音效");
                    }
                    stringSelector.ClearSelection();
                    GameLogger.LogDev("ButtonController: 清除选择");
                }
            }
            else
            {
                GameLogger.LogWarning("ButtonController: 选中的字符为空");
            }
        }
        else
        {
            GameLogger.LogError("ButtonController: stringSelector为空，无法执行拆分操作");
        }
    }
    
    private void combineletter()
    {
        GameLogger.LogDev("ButtonController: combineletter() 开始执行");
        
        if (stringSelector != null)
        {
            int selectedCount = stringSelector.GetSelectionCount();
            GameLogger.LogDev($"ButtonController: 当前选中字符数量: {selectedCount}");
            
            if (selectedCount != 2)
            {
                GameLogger.LogWarning($"ButtonController: 选中字符数量不正确，期望2个，实际{selectedCount}个，清除选择");
                stringSelector.ClearSelection();
                return;
            }
            
            List<string> selectedStrings = stringSelector.SelectedStrings;
            string firstString = selectedStrings[0];
            string secondString = selectedStrings[1];
            GameLogger.LogDev($"ButtonController: 选中的字符: '{firstString}' 和 '{secondString}'");
            
            // Level3彩蛋检测：一+土=王
            if (IsLevel3Scene() && IsEasterEggCombination(firstString, secondString))
            {
                HandleLevel3EasterEgg(firstString, secondString);
                return;
            }
            
            string originalString = PublicData.FindOriginalString(firstString, secondString);
            GameLogger.LogDev($"ButtonController: 查找原始字符，结果: '{originalString}'");
            
            if (originalString != null)
            {
                GameLogger.LogDev($"ButtonController: 找到原始字符 '{originalString}'，开始组合操作");
                
                if (AudioManager.Instance != null && AudioManager.Instance.sfxCombineSuccess != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxCombineSuccess);
                    GameLogger.LogDev("ButtonController: 播放组合成功音效");
                }

                // 记录两个选中字符中靠前的索引，作为结果插入位置
                List<int> indices = stringSelector.GetSelectedIndices();
                indices.Sort();
                int insertIndex = indices.Count > 0 ? indices[0] : 0;

                // 清除选择
                stringSelector.ClearSelection();
                GameLogger.LogDev("ButtonController: 清除选择");

                // 先按索引大的先移除，避免下标偏移
                if (indices.Count >= 2)
                {
                    int idxA = indices[1];
                    stringSelector.RemoveAvailableStringAt(idxA);
                    GameLogger.LogDev($"ButtonController: 从索引 {idxA} 处移除第二个选中字符");
                }
                if (indices.Count >= 1)
                {
                    int idxB = indices[0];
                    stringSelector.RemoveAvailableStringAt(idxB);
                    GameLogger.LogDev($"ButtonController: 从索引 {idxB} 处移除第一个选中字符");
                }
                
                if (PublicData.IsCharacterInTargetList(originalString))
                {
                    GameLogger.LogDev($"ButtonController: 字符 '{originalString}' 在目标列表中");
                    
                    // 合成目标字成功的那一刻立即禁用ESC弹窗功能
                    if (ExitGameManager.Instance != null)
                    {
                        ExitGameManager.Instance.SetExitDialogDisabled(true);
                        GameLogger.LogDev("ButtonController: 合成目标字成功，已禁用ESC弹窗功能");
                    }
                    
                    Transform targetPosition = PublicData.GetTargetPositionForCharacter(originalString);
                    GameLogger.LogDev($"ButtonController: 获取目标位置: {targetPosition?.name ?? "null"}");
                    
                    if (targetPosition != null)
                    {
                        GameLogger.LogDev($"ButtonController: 目标位置有效，准备播放飞行动画");
                        
                        // 在原位置插入合成结果
                        stringSelector.InsertAvailableStringAt(originalString, insertIndex);
                        GameLogger.LogDev($"ButtonController: 在索引 {insertIndex} 插入 '{originalString}'");
                        
                        // 延迟一秒后播放飞行动画
                        StartCoroutine(DelayedFlyingAnimation(originalString, targetPosition));
                        GameLogger.LogDev($"ButtonController: 启动飞行动画协程，字符: '{originalString}'");
                    }
                    else
                    {
                        GameLogger.LogWarning($"ButtonController: 目标位置为空，直接添加字符 '{originalString}'");
                        stringSelector.InsertAvailableStringAt(originalString, insertIndex);
                        
                        // 如果没有飞行动画，立即重新启用ESC弹窗功能
                        if (ExitGameManager.Instance != null)
                        {
                            ExitGameManager.Instance.SetExitDialogDisabled(false);
                            GameLogger.LogDev("ButtonController: 无飞行动画，已重新启用ESC弹窗功能");
                        }
                    }
                }
                else
                {
                    GameLogger.LogDev($"ButtonController: 字符 '{originalString}' 不在目标列表中，直接添加");
                    stringSelector.AddAvailableString(originalString);
                }
                
                stringSelector.RecreateAllButtonsPublic();
                GameLogger.LogDev("ButtonController: 重新创建所有按钮");
                
                stringSelector.SetMaxSelectionCount(2);
                GameLogger.LogDev("ButtonController: 设置最大选择数量为2");
                
                stringSelector.ClearSelection();
                GameLogger.LogDev("ButtonController: 清除选择");
                
                // 发送组合成功广播：兼容旧事件 + 新格式"拼{部件1}{部件2}"用于提示显示
                if (BroadcastManager.Instance != null)
                {
                    BroadcastManager.Instance.BroadcastToAll("combine_success");
                    
                    // 按字典序排序确保广播消息一致性，避免选择顺序影响提示触发
                    // 使用 System.StringComparison.Ordinal 确保排序规则在所有环境下都一致
                    string sortedFirstString, sortedSecondString;
                    if (string.Compare(firstString, secondString, System.StringComparison.Ordinal) <= 0)
                    {
                        sortedFirstString = firstString;
                        sortedSecondString = secondString;
                    }
                    else
                    {
                        sortedFirstString = secondString;
                        sortedSecondString = firstString;
                    }
                    
                    string combineMessage = $"拼{sortedFirstString}{sortedSecondString}";
                    BroadcastManager.Instance.BroadcastToAll(combineMessage);
                    GameLogger.LogUser($"ButtonController: 发送组合成功广播 (combine_success, {combineMessage}) [原顺序: {firstString}+{secondString}]");
                }
                GameLogger.LogUser($"合成结果: {originalString}");
                GameLogger.LogUser("ButtonController: 组合操作完成");
            }
            else
            {
                GameLogger.LogWarning($"ButtonController: 无法找到字符 '{firstString}' 和 '{secondString}' 的组合结果");
                
                if (AudioManager.Instance != null && AudioManager.Instance.sfxOperationFailure != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxOperationFailure);
                    GameLogger.LogDev("ButtonController: 播放操作失败音效");
                }
                stringSelector.ClearSelection();
                GameLogger.LogDev("ButtonController: 清除选择");
            }
        }
        else
        {
            GameLogger.LogError("ButtonController: stringSelector为空，无法执行组合操作");
        }
    }
    
    public void ShowAllButtons()
    {
        if (splitButton != null) splitButton.gameObject.SetActive(true);
        if (combineButton != null) combineButton.gameObject.SetActive(true);
    }
    
    public void SetAllButtonsInteractable(bool interactable)
    {
        if (splitButton != null) splitButton.interactable = interactable;
        if (combineButton != null) combineButton.interactable = interactable;
    }
    
    public bool AreAllButtonsInteractable()
    {
        return (splitButton != null && splitButton.interactable) &&
               (combineButton != null && combineButton.interactable);
    }
    
    public void TriggerSplitButton()
    {
        OnSplitButtonClicked();
    }
    
    public void SetStringSelector(StringSelector selector)
    {
        // 如果之前有订阅，先取消订阅
        if (stringSelector != null)
        {
            stringSelector.OnAvailableStringsChanged -= OnAvailableStringsChanged;
        }
        
        stringSelector = selector;
        
        // 订阅新的事件
        if (stringSelector != null)
        {
            stringSelector.OnAvailableStringsChanged += OnAvailableStringsChanged;
            GameLogger.LogDev("ButtonController: 已订阅新的StringSelector的可用字符串变化事件");
        }
    }
    
    public StringSelector GetStringSelector()
    {
        return stringSelector;
    }
    
    // 原有的同名方法已移除，改用属性 IsFlyingAnimationActive
    
    // 设置飞行动画状态
    public void SetFlyingAnimationActive(bool active)
    {
        isFlyingAnimationActive = active;
        
        // 更新按钮状态
        UpdateButtonStates(stringSelector != null ? stringSelector.GetSelectionCount() : 0);
    }
    
    /// <summary>
    /// 通知TutorialManager字符选择发生变化
    /// </summary>
    private void NotifyTutorialManagerOfSelectionChange()
    {
        // 查找场景中的TutorialManager
        TutorialManager tutorialManager = FindObjectOfType<TutorialManager>();
        if (tutorialManager != null)
        {
            tutorialManager.OnCharacterSelectionChanged();
        }
    }
    
    /// <summary>
    /// 设置教程模式
    /// </summary>
    /// <param name="tutorialMode">是否为教程模式</param>
    public void SetTutorialMode(bool tutorialMode)
    {
        isTutorialMode = tutorialMode;
        
        // 在教程模式中禁用字符选择，在非教程模式中启用字符选择
        if (stringSelector != null)
        {
            stringSelector.SetAllCharacterButtonsInteractable(!tutorialMode);
        }
        
        GameLogger.LogDev($"ButtonController: 教程模式已设置为 {tutorialMode}");
    }
    
    /// <summary>
    /// 禁用字符选择功能
    /// </summary>
    public void DisableCharacterSelection()
    {
        if (stringSelector != null)
        {
            stringSelector.DisableAllCharacterButtons();
        }
        GameLogger.LogDev("ButtonController: 已禁用字符选择功能");
    }
    
    /// <summary>
    /// 启用字符选择功能
    /// </summary>
    public void EnableCharacterSelection()
    {
        if (stringSelector != null)
        {
            stringSelector.EnableAllCharacterButtons();
        }
        GameLogger.LogDev("ButtonController: 已启用字符选择功能");
    }
    
    /// <summary>
    /// 获取当前是否为教程模式
    /// </summary>
    /// <returns>是否为教程模式</returns>
    public bool IsTutorialMode()
    {
        return isTutorialMode;
    }
    
    // 处理可用字符串变化事件
    private void OnAvailableStringsChanged()
    {
        GameLogger.LogDev("ButtonController: 收到可用字符串变化事件，刷新按钮显示");
        
        // 刷新按钮显示
        RefreshButtonDisplay();
        
        // 通知TutorialManager字符选择发生变化
        NotifyTutorialManagerOfSelectionChange();
    }
    
    // 刷新按钮显示
    private void RefreshButtonDisplay()
    {
        if (stringSelector != null)
        {
            // 重新创建所有按钮
            stringSelector.RecreateAllButtonsPublic();
            
            // 更新按钮状态
            UpdateButtonStates(stringSelector.GetSelectionCount());
            
            GameLogger.LogDev($"ButtonController: 按钮显示已刷新，当前可用字符串数量: {stringSelector.GetAvailableStringCount()}");
        }
    }
    
    // 在销毁时取消订阅事件
    private void OnDestroy()
    {
        if (stringSelector != null)
        {
            stringSelector.OnAvailableStringsChanged -= OnAvailableStringsChanged;
            GameLogger.LogDev("ButtonController: 已取消订阅StringSelector的可用字符串变化事件");
        }
    }
    
    private void CreateFlyingCharacter(string character, Transform targetPosition)
    {
        // 使用协程等待解字台按钮生成完成，避免起点回退到屏幕中心
        StartCoroutine(CreateFlyingCharacterCoroutine(character, targetPosition));
    }

    /// <summary>
    /// 协程：等待按钮容器与对应按钮就绪后，再创建飞字并启动动画
    /// </summary>
    private System.Collections.IEnumerator CreateFlyingCharacterCoroutine(string character, Transform targetPosition)
    {
        // 从可用字符串列表中移除该字符并刷新按钮（与原逻辑一致）
        if (stringSelector != null)
        {
            stringSelector.RemoveAvailableString(character);
            stringSelector.RecreateAllButtonsPublic();
        }

        // 最多等待0.5秒让 StringSelector 生成按钮
        float timeout = 0.5f;
        float elapsed = 0f;
        Vector2 startPosition = Vector2.zero;
        bool found = false;
        while (elapsed < timeout)
        {
            // 仅检测是否已生成对应按钮，真正的Canvas坐标稍后由Canvas计算
            RectTransform btnRect = FindCharacterButtonRect(character);
            if (btnRect != null)
            {
                found = true;
                break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 创建UI对象而不是GameObject
        GameObject flyingCharacter = new GameObject($"Flying_{character}");

        // 挂到Canvas
        Canvas canvas = targetCanvas != null ? targetCanvas : FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            flyingCharacter.transform.SetParent(canvas.transform, false);
        }

        // 添加RectTransform与TextMeshPro
        RectTransform rectTransform = flyingCharacter.AddComponent<RectTransform>();
        TMPro.TextMeshProUGUI textMesh = flyingCharacter.AddComponent<TMPro.TextMeshProUGUI>();
        textMesh.text = character;
        textMesh.fontSize = Mathf.RoundToInt(flyingFontSize);
        textMesh.alignment = TMPro.TextAlignmentOptions.Center;
        textMesh.color = Color.black;
        if (chineseFont != null)
        {
            textMesh.font = chineseFont;
        }
        else if (stringSelector != null && stringSelector.GetChineseFont() != null)
        {
            textMesh.font = stringSelector.GetChineseFont();
        }
        textMesh.ForceMeshUpdate();

        // 计算起点：将按钮世界坐标转换为Canvas本地坐标
        if (found && canvas != null)
        {
            startPosition = GetCharacterButtonPositionInCanvas(canvas, character);
        }
        // 设置起点：若未找到按钮，则保持为(0,0)作为兜底
        rectTransform.anchoredPosition = startPosition;

        // 启动动画
        StartCoroutine(FlyToTargetUI(flyingCharacter, targetPosition, character));
    }
    
    // 查找字符按钮的位置
    private Vector2 FindCharacterButtonPosition(string character)
    {
        if (stringSelector != null)
        {
            Transform buttonContainer = stringSelector.GetButtonContainer();
            if (buttonContainer != null)
            {
                GameLogger.LogDev($"查找字符按钮: {character}, 按钮容器子物体数量: {buttonContainer.childCount}");
                
                // 遍历所有按钮找到对应字符的按钮
                for (int i = 0; i < buttonContainer.childCount; i++)
                {
                    Transform buttonTransform = buttonContainer.GetChild(i);
                    if (buttonTransform != null)
                    {
                        // 检查按钮上的文本组件
                        TMPro.TextMeshProUGUI buttonText = buttonTransform.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                        if (buttonText != null)
                        {
                            GameLogger.LogDev($"按钮 {i}: 文本={buttonText.text}");
                            if (buttonText.text == character)
                            {
                                RectTransform buttonRectTransform = buttonTransform as RectTransform;
                                if (buttonRectTransform != null)
                                {
                                    // 返回Canvas坐标中的起点
                                    Canvas canvas = FindObjectOfType<Canvas>();
                                    if (canvas != null)
                                    {
                                        Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, buttonRectTransform.position);
                                        RectTransform canvasRect = canvas.transform as RectTransform;
                                        Vector2 localPoint;
                                        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out localPoint))
                                        {
                                            GameLogger.LogDev($"找到字符按钮: {character}, Canvas起点: {localPoint}");
                                            return localPoint;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        GameLogger.LogDev($"未找到字符按钮: {character}, 使用默认位置");
        // 如果找不到按钮位置，使用屏幕中央
        return Vector2.zero;
    }

    // 返回字符按钮的RectTransform（若存在）
    private RectTransform FindCharacterButtonRect(string character)
    {
        if (stringSelector == null) return null;
        Transform buttonContainer = stringSelector.GetButtonContainer();
        if (buttonContainer == null) return null;
        for (int i = 0; i < buttonContainer.childCount; i++)
        {
            Transform buttonTransform = buttonContainer.GetChild(i);
            if (buttonTransform == null) continue;
            TMPro.TextMeshProUGUI buttonText = buttonTransform.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (buttonText != null && buttonText.text == character)
            {
                return buttonTransform as RectTransform;
            }
        }
        return null;
    }

    // 将字符按钮位置转换为Canvas坐标
    private Vector2 GetCharacterButtonPositionInCanvas(Canvas canvas, string character)
    {
        RectTransform rect = FindCharacterButtonRect(character);
        if (rect == null) return Vector2.zero;
        Camera cam = canvas.worldCamera; // Screen Space - Camera 需要使用该相机
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, rect.position);
        RectTransform canvasRect = canvas.transform as RectTransform;
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, cam, out localPoint))
        {
            return localPoint;
        }
        return Vector2.zero;
    }

    private IEnumerator FlyToTargetUI(GameObject flyingCharacter, Transform targetPosition, string character)
    {
        RectTransform rectTransform = flyingCharacter.GetComponent<RectTransform>();
        Vector2 startPosition = rectTransform.anchoredPosition;
        
        // 获取目标位置的UI坐标
        Vector2 targetUIPosition = GetTargetUIPosition(targetPosition);
        
        // 调试信息
        GameLogger.LogDev($"飞行动画开始: 字符={character}, 起始位置={startPosition}, 目标位置={targetUIPosition}");
        
        float duration = 1.5f; // 增加动画时长
        float elapsedTime = 0f;
        
        if (AudioManager.Instance != null && AudioManager.Instance.sfxGoalFlyIn != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxGoalFlyIn);
            GameLogger.LogDev("ButtonController: 播放目标飞入音效");
        }
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            float easeProgress = Mathf.SmoothStep(0f, 1f, progress);
            
            // 螺旋轨迹计算
            Vector2 spiralPosition = CalculateSpiralPosition(startPosition, targetUIPosition, progress);
            rectTransform.anchoredPosition = spiralPosition;
            
            // 缩放动画
            float scale = 1f + Mathf.Sin(progress * Mathf.PI * 2) * 0.3f;
            rectTransform.localScale = Vector3.one * scale;
            
            yield return null;
        }
        
        rectTransform.anchoredPosition = targetUIPosition;
        rectTransform.localScale = Vector3.one;
        
        GameLogger.LogUser($"飞行动画完成: 字符={character}");
        
        // 标记目标字符为已完成
        PublicData.MarkTargetAsCompleted(character);
        
        // 字符保持在原地，不销毁
        // Destroy(flyingCharacter); // 注释掉销毁代码
        
        // 飞行动画结束，解锁按钮
        SetFlyingAnimationActive(false);
        
        // 飞行动画完成后重新启用ESC弹窗功能
        if (ExitGameManager.Instance != null)
        {
            ExitGameManager.Instance.SetExitDialogDisabled(false);
            GameLogger.LogDev("ButtonController: 飞行动画完成，已重新启用ESC弹窗功能");
        }
    }
    
    // 计算螺旋轨迹位置
    private Vector2 CalculateSpiralPosition(Vector2 startPos, Vector2 endPos, float progress)
    {
        // 基础直线插值
        Vector2 linearPosition = Vector2.Lerp(startPos, endPos, progress);
        
        // 计算螺旋偏移
        float spiralRadius = 50f; // 螺旋半径
        float angle = progress * spiralTurns * 2f * Mathf.PI;
        
        // 螺旋偏移向量
        Vector2 spiralOffset = new Vector2(
            Mathf.Cos(angle) * spiralRadius * (1f - progress), // 半径随进度减小
            Mathf.Sin(angle) * spiralRadius * (1f - progress)
        );
        
        // 返回螺旋位置
        return linearPosition + spiralOffset;
    }
    
    // 获取目标位置的UI坐标
    private Vector2 GetTargetUIPosition(Transform targetPosition)
    {
        GameLogger.LogDev($"获取目标位置: {targetPosition?.name}");
        
        // 如果目标位置是UI元素，直接获取其anchoredPosition
        RectTransform targetRectTransform = targetPosition as RectTransform;
        if (targetRectTransform != null)
        {
            Vector2 position = targetRectTransform.anchoredPosition;
            GameLogger.LogDev($"目标位置是RectTransform: {position}");
            return position;
        }
        
        // 如果目标位置不是UI元素，尝试获取其子物体的RectTransform
        RectTransform childRectTransform = targetPosition.GetComponentInChildren<RectTransform>();
        if (childRectTransform != null)
        {
            Vector2 position = childRectTransform.anchoredPosition;
            GameLogger.LogDev($"目标位置子物体是RectTransform: {position}");
            return position;
        }
        
        GameLogger.LogDev($"未找到有效的目标位置，使用默认位置");
        // 如果都找不到，返回屏幕中央
        return Vector2.zero;
    }

    private IEnumerator DelayedFlyingAnimation(string character, Transform targetPosition)
    {
        yield return new WaitForSeconds(1f); // 延迟一秒
        
        // 开始飞行动画，锁定按钮
        SetFlyingAnimationActive(true);
        
        CreateFlyingCharacter(character, targetPosition);
    }
    
    #region Level1 飞舞功能
    
    /// <summary>
    /// 开始Level1字符飞舞动画
    /// </summary>
    /// <param name="character">要飞舞的字符</param>
    /// <param name="startPosition">起始位置</param>
    /// <param name="endPosition">终点位置</param>
    public void StartLevel1CharacterFly(string character, Vector2 startPosition, Vector2 endPosition)
    {
        if (isLevel1Flying)
        {
            GameLogger.LogWarning("ButtonController: 已有Level1字符在飞行中，忽略新的飞行请求");
            return;
        }
        
        // 使用新的统一飞行接口，终点使用 Inspector 指定的 targetPosition
        if (targetPosition == null)
        {
            GameLogger.LogError("ButtonController: 目标位置未设置");
            return;
        }
        Fly(character, targetPosition);
    }
    
    /// <summary>
    /// 开始Level1字符飞舞动画（使用Inspector中设置的目标位置）
    /// </summary>
    /// <param name="character">要飞舞的字符</param>
    /// <param name="startPosition">起始位置</param>
    public void StartLevel1CharacterFly(string character, Vector2 startPosition)
    {
        if (isLevel1Flying)
        {
            GameLogger.LogWarning("ButtonController: 已有Level1字符在飞行中，忽略新的飞行请求");
            return;
        }
        
        if (targetPosition == null)
        {
            GameLogger.LogError("ButtonController: 目标位置未设置");
            return;
        }
        
        Fly(character, targetPosition);
    }
    
    /// <summary>
    /// Level1字符飞舞协程
    /// </summary>
    private IEnumerator Level1FlyCharacterCoroutine(string character, Vector2 startPosition, Vector2 endPosition)
    {
        isLevel1Flying = true;
        
        // 使用新接口执行飞行
        if (targetPosition != null)
        {
            Fly(character, targetPosition);
        }
        // 简单等待，避免重复触发
        yield return new WaitForSeconds(2f);
        
        isLevel1Flying = false;
    }
    

    

    

    
    /// <summary>
    /// 检查Level1是否正在飞行
    /// </summary>
    public bool IsLevel1Flying()
    {
        return isLevel1Flying;
    }
    
    /// <summary>
    /// 设置Level1目标位置
    /// </summary>
    public void SetLevel1TargetPosition(Transform target)
    {
        targetPosition = target;
    }
    
    /// <summary>
    /// 设置Level1目标Canvas
    /// </summary>
    public void SetLevel1TargetCanvas(Canvas canvas)
    {
        targetCanvas = canvas;
    }
    
    #endregion

    // 统一飞行动画接口：从 Canvas 中心出发，在 Canvas 坐标系内飞向终点
    public void Fly(string character, Transform endTransform)
    {
        if (targetCanvas == null)
        {
            targetCanvas = FindObjectOfType<Canvas>();
        }

        if (targetCanvas == null)
        {
            GameLogger.LogError("ButtonController: 未找到Canvas，无法执行飞行动画");
            return;
        }

        // 在 Canvas 下创建文本对象
        GameObject flying = new GameObject($"Flying_{character}");
        flying.transform.SetParent(targetCanvas.transform, false);
        RectTransform rect = flying.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero; // 屏幕（Canvas）中心为起点

        TMPro.TextMeshProUGUI text = flying.AddComponent<TMPro.TextMeshProUGUI>();
        text.text = character;
        text.fontSize = Mathf.RoundToInt(flyingFontSize);
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.color = Color.black;
        if (chineseFont != null)
        {
            text.font = chineseFont;
        }
        else if (stringSelector != null && stringSelector.GetChineseFont() != null)
        {
            text.font = stringSelector.GetChineseFont();
        }
        text.ForceMeshUpdate();

        // 终点坐标（Canvas 坐标系）
        Vector2 endAnchored = Vector2.zero;
        RectTransform endRect = endTransform as RectTransform;
        if (endRect != null)
        {
            endAnchored = endRect.anchoredPosition;
        }
        else
        {
            // 兜底：不在UI下的Transform，转换为Canvas本地坐标（使用Canvas的worldCamera）
            Camera cam = targetCanvas.worldCamera;
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, endTransform.position);
            RectTransform canvasRect = targetCanvas.transform as RectTransform;
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, cam, out localPoint))
            {
                endAnchored = localPoint;
            }
        }

        // 启动动画
        StartCoroutine(FlyUIRoutine(flying, endAnchored, character));
    }

    private IEnumerator FlyUIRoutine(GameObject flyingCharacter, Vector2 endAnchored, string character)
    {
        SetFlyingAnimationActive(true);

        RectTransform rect = flyingCharacter.GetComponent<RectTransform>();
        Vector2 startAnchored = rect.anchoredPosition; // 已是中心

        if (AudioManager.Instance != null && AudioManager.Instance.sfxGoalFlyIn != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxGoalFlyIn);
        }

        float duration = 1.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // 使用现有的螺旋轨迹计算
            Vector2 pos = CalculateSpiralPosition(startAnchored, endAnchored, t);
            rect.anchoredPosition = pos;

            // 轻微缩放动效
            float scale = 1f + Mathf.Sin(t * Mathf.PI * 2f) * 0.3f;
            rect.localScale = Vector3.one * scale;

            yield return null;
        }

        rect.anchoredPosition = endAnchored;
        rect.localScale = Vector3.one;

        SetFlyingAnimationActive(false);
        
        // 飞行动画完成后重新启用ESC弹窗功能
        if (ExitGameManager.Instance != null)
        {
            ExitGameManager.Instance.SetExitDialogDisabled(false);
            GameLogger.LogDev("ButtonController: 统一飞行动画完成，已重新启用ESC弹窗功能");
        }
    }
    
    #region Level3 彩蛋功能
    
    /// <summary>
    /// 检测当前场景是否为Level3
    /// </summary>
    private bool IsLevel3Scene()
    {
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isLevel3 = currentSceneName.ToLower().Contains("level3") || currentSceneName.ToLower().Contains("3");
        GameLogger.LogDev($"ButtonController: 当前场景: {currentSceneName}, 是否为Level3: {isLevel3}");
        return isLevel3;
    }
    
    /// <summary>
    /// 检测是否为彩蛋组合：一+土
    /// </summary>
    private bool IsEasterEggCombination(string first, string second)
    {
        bool isEasterEgg = (first == "一" && second == "土") || (first == "土" && second == "一");
        if (isEasterEgg)
        {
            GameLogger.LogDev($"ButtonController: 检测到Level3彩蛋组合: '{first}' + '{second}'");
        }
        return isEasterEgg;
    }
    
    /// <summary>
    /// 处理Level3彩蛋逻辑
    /// </summary>
    private void HandleLevel3EasterEgg(string first, string second)
    {
        GameLogger.LogUser("ButtonController: 触发Level3彩蛋！一+土=王");
        
        // 播放成功音效
        if (AudioManager.Instance != null && AudioManager.Instance.sfxCombineSuccess != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxCombineSuccess);
            GameLogger.LogDev("ButtonController: 播放彩蛋成功音效");
        }
        
        // 记录选中字符的索引位置
        List<int> indices = stringSelector.GetSelectedIndices();
        indices.Sort();
        int insertIndex = indices.Count > 0 ? indices[0] : 0;
        
        // 清除选择
        stringSelector.ClearSelection();
        GameLogger.LogDev("ButtonController: 清除选择");
        
        // 移除选中的字符（按索引大的先移除，避免下标偏移）
        if (indices.Count >= 2)
        {
            int idxA = indices[1];
            stringSelector.RemoveAvailableStringAt(idxA);
            GameLogger.LogDev($"ButtonController: 从索引 {idxA} 处移除第二个选中字符");
        }
        if (indices.Count >= 1)
        {
            int idxB = indices[0];
            stringSelector.RemoveAvailableStringAt(idxB);
            GameLogger.LogDev($"ButtonController: 从索引 {idxB} 处移除第一个选中字符");
        }
        
        // 在原位置添加"王"字作为彩蛋奖励
        stringSelector.InsertAvailableStringAt("王", insertIndex);
        GameLogger.LogDev($"ButtonController: 在索引 {insertIndex} 插入彩蛋奖励字符 '王'");
        
        // 重新创建按钮显示
        stringSelector.RecreateAllButtonsPublic();
        GameLogger.LogDev("ButtonController: 重新创建所有按钮");
        
        // 重置选择状态
        stringSelector.SetMaxSelectionCount(2);
        stringSelector.ClearSelection();
        
        // 显示彩蛋提示框
        ShowEasterEggNotification("知音难觅，亦如王者之路。将这份「王」者之证好生收藏，或许在未来的旅途中另有他用。");
        
        // 同时发送广播消息（保持兼容性）
        if (BroadcastManager.Instance != null)
        {
            BroadcastManager.Instance.BroadcastToAll("combine_success");
            BroadcastManager.Instance.BroadcastToAll("拼一土");
            GameLogger.LogUser("ButtonController: 发送彩蛋广播消息 (combine_success, 拼一土)");
        }
        
        GameLogger.LogUser("ButtonController: Level3彩蛋处理完成，获得'王'字！");
    }
    
    /// <summary>
    /// 显示彩蛋通知提示框
    /// </summary>
    /// <param name="message">提示消息</param>
    private void ShowEasterEggNotification(string message)
    {
        if (easterEggPanel == null)
        {
            GameLogger.LogWarning("ButtonController: 彩蛋提示框未设置，无法显示彩蛋通知");
            return;
        }
        
        // 先显示遮罩层（阻止背景交互）
        if (easterEggMask != null)
        {
            easterEggMask.SetActive(true);
            GameLogger.LogDev("ButtonController: 显示彩蛋遮罩，阻止背景交互");
        }
        else
        {
            GameLogger.LogWarning("ButtonController: 彩蛋遮罩未设置，可能无法完全阻止背景交互");
        }
        
        // 显示彩蛋面板
        easterEggPanel.SetActive(true);
        
        // 设置提示文本
        if (easterEggText != null)
        {
            easterEggText.text = message;
        }
        
        // 设置继续按钮事件
        if (easterEggContinueButton != null)
        {
            easterEggContinueButton.onClick.RemoveAllListeners();
            easterEggContinueButton.onClick.AddListener(HideEasterEggNotification);
        }
        
        // 开始E键监听协程
        StartCoroutine(EasterEggEKeyListener());
        
        GameLogger.LogDev($"ButtonController: 显示彩蛋通知: {message}");
    }
    
    /// <summary>
    /// E键监听协程
    /// </summary>
    private System.Collections.IEnumerator EasterEggEKeyListener()
    {
        while (easterEggPanel != null && easterEggPanel.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                HideEasterEggNotification();
                yield break;
            }
            yield return null;
        }
    }
    
    /// <summary>
    /// 隐藏彩蛋通知提示框
    /// </summary>
    private void HideEasterEggNotification()
    {
        // 隐藏彩蛋面板
        if (easterEggPanel != null)
        {
            easterEggPanel.SetActive(false);
        }
        
        // 隐藏遮罩层（恢复背景交互）
        if (easterEggMask != null)
        {
            easterEggMask.SetActive(false);
            GameLogger.LogDev("ButtonController: 隐藏彩蛋遮罩，恢复背景交互");
        }
        
        // 在隐藏提示框后，将解字台中的"王"字移除
        RemoveWangCharacterFromSelector();
        
        GameLogger.LogDev("ButtonController: 隐藏彩蛋通知");
    }
    
    /// <summary>
    /// 从解字台中移除"王"字
    /// </summary>
    private void RemoveWangCharacterFromSelector()
    {
        if (stringSelector != null)
        {
            stringSelector.RemoveAvailableString("王");
            stringSelector.RecreateAllButtonsPublic();
            GameLogger.LogDev("ButtonController: 已从解字台移除王字");
        }
        else
        {
            GameLogger.LogWarning("ButtonController: stringSelector为空，无法移除王字");
        }
    }
    
    #endregion
}
