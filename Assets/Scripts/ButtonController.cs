using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class ButtonController : MonoBehaviour
{
    [Header("按钮设置")]
    [SerializeField] private Button splitButton, combineButton;
    [SerializeField] private float hideDelay = 0.1f;
    
    [Header("UI提示设置")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float messageDisplayTime = 3f;
    
    [Header("选择器引用")]
    [SerializeField] private StringSelector stringSelector;
    
    [Header("飞行动画设置")]
    [SerializeField] private float spiralTurns = 3f; // 螺旋圈数
    [SerializeField] private Transform targetPosition; // 目标位置
    [SerializeField] private Canvas targetCanvas; // 目标Canvas
    [SerializeField] private float flyDuration = 1.5f; // 飞行时长
    [SerializeField] private float spiralRadius = 50f; // 螺旋半径
    
    [Header("字体设置")]
    [SerializeField] private TMP_FontAsset chineseFont; // 中文字体
    
    // 单例模式，方便其他脚本访问
    public static ButtonController Instance { get; private set; }
    
    // 飞行动画状态
    private bool isFlyingAnimationActive = false;
    private bool isLevel1Flying = false; // Level1飞舞状态
    
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
            Debug.Log("ButtonController: 已订阅StringSelector的可用字符串变化事件");
        }
    }

    private void OnSplitButtonClicked()
    {
        Debug.Log("ButtonController: OnSplitButtonClicked() 开始执行");
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
        
        if (splitButton != null)
        {
            splitButton.interactable = selectedCount == 1;
        }
        
        if (combineButton != null)
        {
            combineButton.interactable = selectedCount == 2;
        }
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

            Debug.Log("ButtonController: 已创建广播管理器");
        }
        else
        {
            Debug.Log("ButtonController: 广播管理器已存在");
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
        Debug.Log("ButtonController: splitletter() 开始执行");
        
        if (stringSelector != null)
        {
            int selectedCount = stringSelector.GetSelectionCount();
            Debug.Log($"ButtonController: 当前选中字符数量: {selectedCount}");
            
            if (selectedCount != 1)
            {
                Debug.LogWarning($"ButtonController: 选中字符数量不正确，期望1个，实际{selectedCount}个，清除选择");
                stringSelector.ClearSelection();
                return;
            }
            
            string selectedString = stringSelector.FirstSelectedString;
            Debug.Log($"ButtonController: 选中的字符: '{selectedString}'");
            
            if (!string.IsNullOrEmpty(selectedString))
            {
                Debug.Log($"ButtonController: 检查字符 '{selectedString}' 是否可以拆分");
                
                if (PublicData.CanSplitString(selectedString))
                {
                    var (part1, part2) = PublicData.GetStringSplit(selectedString);
                    Debug.Log($"ButtonController: 字符 '{selectedString}' 可以拆分，结果为: '{part1}' 和 '{part2}'");
                    
                    if (AudioManager.Instance != null && AudioManager.Instance.sfxSplitSuccess != null)
                    {
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxSplitSuccess);
                        Debug.Log("ButtonController: 播放拆分成功音效");
                    }

                    stringSelector.ClearSelection();
                    Debug.Log("ButtonController: 清除选择");
                    
                    stringSelector.RemoveAvailableString(selectedString);
                    Debug.Log($"ButtonController: 从可用字符串列表中移除 '{selectedString}'");
                    
                    stringSelector.AddAvailableString(part1);
                    Debug.Log($"ButtonController: 添加字符 '{part1}' 到可用字符串列表");
                    
                    stringSelector.AddAvailableString(part2);
                    Debug.Log($"ButtonController: 添加字符 '{part2}' 到可用字符串列表");
                    
                    stringSelector.RecreateAllButtonsPublic();
                    Debug.Log("ButtonController: 重新创建所有按钮");
                    
                    stringSelector.SetMaxSelectionCount(2);
                    Debug.Log("ButtonController: 设置最大选择数量为2");
                    
                    // 发送拆分成功广播
                    if (BroadcastManager.Instance != null)
                    {
                        BroadcastManager.Instance.BroadcastToAll("split_success");
                        Debug.Log("ButtonController: 发送拆分成功广播");
                    }
                    
                    Debug.Log("ButtonController: 拆分操作完成");
                }
                else
                {
                    Debug.LogWarning($"ButtonController: 字符 '{selectedString}' 无法拆分");
                    
                    if (AudioManager.Instance != null && AudioManager.Instance.sfxOperationFailure != null)
                    {
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxOperationFailure);
                        Debug.Log("ButtonController: 播放操作失败音效");
                    }
                    stringSelector.ClearSelection();
                    Debug.Log("ButtonController: 清除选择");
                }
            }
            else
            {
                Debug.LogWarning("ButtonController: 选中的字符为空");
            }
        }
        else
        {
            Debug.LogError("ButtonController: stringSelector为空，无法执行拆分操作");
        }
    }
    
    private void combineletter()
    {
        Debug.Log("ButtonController: combineletter() 开始执行");
        
        if (stringSelector != null)
        {
            int selectedCount = stringSelector.GetSelectionCount();
            Debug.Log($"ButtonController: 当前选中字符数量: {selectedCount}");
            
            if (selectedCount != 2)
            {
                Debug.LogWarning($"ButtonController: 选中字符数量不正确，期望2个，实际{selectedCount}个，清除选择");
                stringSelector.ClearSelection();
                return;
            }
            
            List<string> selectedStrings = stringSelector.SelectedStrings;
            string firstString = selectedStrings[0];
            string secondString = selectedStrings[1];
            Debug.Log($"ButtonController: 选中的字符: '{firstString}' 和 '{secondString}'");
            
            string originalString = PublicData.FindOriginalString(firstString, secondString);
            Debug.Log($"ButtonController: 查找原始字符，结果: '{originalString}'");
            
            if (originalString != null)
            {
                Debug.Log($"ButtonController: 找到原始字符 '{originalString}'，开始组合操作");
                
                if (AudioManager.Instance != null && AudioManager.Instance.sfxCombineSuccess != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxCombineSuccess);
                    Debug.Log("ButtonController: 播放组合成功音效");
                }

                stringSelector.ClearSelection();
                Debug.Log("ButtonController: 清除选择");
                
                stringSelector.RemoveAvailableString(firstString);
                Debug.Log($"ButtonController: 从可用字符串列表中移除 '{firstString}'");
                
                stringSelector.RemoveAvailableString(secondString);
                Debug.Log($"ButtonController: 从可用字符串列表中移除 '{secondString}'");
                
                if (PublicData.IsCharacterInTargetList(originalString))
                {
                    Debug.Log($"ButtonController: 字符 '{originalString}' 在目标列表中");
                    
                    Transform targetPosition = PublicData.GetTargetPositionForCharacter(originalString);
                    Debug.Log($"ButtonController: 获取目标位置: {targetPosition?.name ?? "null"}");
                    
                    if (targetPosition != null)
                    {
                        Debug.Log($"ButtonController: 目标位置有效，准备播放飞行动画");
                        
                        // 先添加到可用字符串列表
                        stringSelector.AddAvailableString(originalString);
                        Debug.Log($"ButtonController: 添加字符 '{originalString}' 到可用字符串列表");
                        
                        stringSelector.RecreateAllButtonsPublic();
                        Debug.Log("ButtonController: 重新创建所有按钮");
                        
                        // 延迟一秒后播放飞行动画
                        StartCoroutine(DelayedFlyingAnimation(originalString, targetPosition));
                        Debug.Log($"ButtonController: 启动飞行动画协程，字符: '{originalString}'");
                    }
                    else
                    {
                        Debug.LogWarning($"ButtonController: 目标位置为空，直接添加字符 '{originalString}'");
                        stringSelector.AddAvailableString(originalString);
                    }
                }
                else
                {
                    Debug.Log($"ButtonController: 字符 '{originalString}' 不在目标列表中，直接添加");
                    stringSelector.AddAvailableString(originalString);
                }
                
                stringSelector.RecreateAllButtonsPublic();
                Debug.Log("ButtonController: 重新创建所有按钮");
                
                stringSelector.SetMaxSelectionCount(2);
                Debug.Log("ButtonController: 设置最大选择数量为2");
                
                stringSelector.ClearSelection();
                Debug.Log("ButtonController: 清除选择");
                
                // 发送组合成功广播
                if (BroadcastManager.Instance != null)
                {
                    BroadcastManager.Instance.BroadcastToAll("combine_success");
                    Debug.Log("ButtonController: 发送组合成功广播");
                }
                Debug.Log($"合成结果: {originalString}");
                Debug.Log("ButtonController: 组合操作完成");
            }
            else
            {
                Debug.LogWarning($"ButtonController: 无法找到字符 '{firstString}' 和 '{secondString}' 的组合结果");
                
                if (AudioManager.Instance != null && AudioManager.Instance.sfxOperationFailure != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxOperationFailure);
                    Debug.Log("ButtonController: 播放操作失败音效");
                }
                stringSelector.ClearSelection();
                Debug.Log("ButtonController: 清除选择");
            }
        }
        else
        {
            Debug.LogError("ButtonController: stringSelector为空，无法执行组合操作");
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
            Debug.Log("ButtonController: 已订阅新的StringSelector的可用字符串变化事件");
        }
    }
    
    public StringSelector GetStringSelector()
    {
        return stringSelector;
    }
    
    // 检查飞行动画是否激活
    public bool IsFlyingAnimationActive()
    {
        return isFlyingAnimationActive;
    }
    
    // 设置飞行动画状态
    public void SetFlyingAnimationActive(bool active)
    {
        isFlyingAnimationActive = active;
        
        // 更新按钮状态
        UpdateButtonStates(stringSelector != null ? stringSelector.GetSelectionCount() : 0);
    }
    
    // 处理可用字符串变化事件
    private void OnAvailableStringsChanged()
    {
        Debug.Log("ButtonController: 收到可用字符串变化事件，刷新按钮显示");
        
        // 刷新按钮显示
        RefreshButtonDisplay();
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
            
            Debug.Log($"ButtonController: 按钮显示已刷新，当前可用字符串数量: {stringSelector.GetAvailableStringCount()}");
        }
    }
    
    // 在销毁时取消订阅事件
    private void OnDestroy()
    {
        if (stringSelector != null)
        {
            stringSelector.OnAvailableStringsChanged -= OnAvailableStringsChanged;
            Debug.Log("ButtonController: 已取消订阅StringSelector的可用字符串变化事件");
        }
    }
    
    private void CreateFlyingCharacter(string character, Transform targetPosition)
    {
        // 从可用字符串列表中移除该字符
        if (stringSelector != null)
        {
            stringSelector.RemoveAvailableString(character);
            stringSelector.RecreateAllButtonsPublic();
        }
        
        // 创建UI对象而不是GameObject
        GameObject flyingCharacter = new GameObject($"Flying_{character}");
        
        // 添加Canvas组件（如果还没有）
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            flyingCharacter.transform.SetParent(canvas.transform, false);
        }
        
        // 添加RectTransform组件
        RectTransform rectTransform = flyingCharacter.AddComponent<RectTransform>();
        
        // 添加TextMeshPro组件
        TMPro.TextMeshProUGUI textMesh = flyingCharacter.AddComponent<TMPro.TextMeshProUGUI>();
        textMesh.text = character;
        textMesh.fontSize = 50;
        textMesh.alignment = TMPro.TextAlignmentOptions.Center;
        textMesh.color = Color.black; // 设置文字为黑色
        
        // 设置字体（如果有中文字体）
        if (stringSelector != null && stringSelector.GetChineseFont() != null)
        {
            textMesh.font = stringSelector.GetChineseFont();
        }
        
        // 强制更新文本网格
        textMesh.ForceMeshUpdate();
        
        // 查找字符按钮的位置作为起点
        Vector2 startPosition = FindCharacterButtonPosition(character);
        rectTransform.anchoredPosition = startPosition;
        
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
                Debug.Log($"查找字符按钮: {character}, 按钮容器子物体数量: {buttonContainer.childCount}");
                
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
                            Debug.Log($"按钮 {i}: 文本={buttonText.text}");
                            if (buttonText.text == character)
                            {
                                RectTransform buttonRectTransform = buttonTransform as RectTransform;
                                if (buttonRectTransform != null)
                                {
                                    Vector2 position = buttonRectTransform.anchoredPosition;
                                    Debug.Log($"找到字符按钮: {character}, 位置: {position}");
                                    return position;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        Debug.Log($"未找到字符按钮: {character}, 使用默认位置");
        // 如果找不到按钮位置，使用屏幕中央
        return Vector2.zero;
    }

    private IEnumerator FlyToTargetUI(GameObject flyingCharacter, Transform targetPosition, string character)
    {
        RectTransform rectTransform = flyingCharacter.GetComponent<RectTransform>();
        Vector2 startPosition = rectTransform.anchoredPosition;
        
        // 获取目标位置的UI坐标
        Vector2 targetUIPosition = GetTargetUIPosition(targetPosition);
        
        // 调试信息
        Debug.Log($"飞行动画开始: 字符={character}, 起始位置={startPosition}, 目标位置={targetUIPosition}");
        
        float duration = 1.5f; // 增加动画时长
        float elapsedTime = 0f;
        
        if (AudioManager.Instance != null && AudioManager.Instance.sfxGoalFlyIn != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxGoalFlyIn);
            Debug.Log("ButtonController: 播放目标飞入音效");
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
        
        Debug.Log($"飞行动画完成: 字符={character}");
        
        // 标记目标字符为已完成
        PublicData.MarkTargetAsCompleted(character);
        
        // 字符保持在原地，不销毁
        // Destroy(flyingCharacter); // 注释掉销毁代码
        
        // 飞行动画结束，解锁按钮
        SetFlyingAnimationActive(false);
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
        Debug.Log($"获取目标位置: {targetPosition?.name}");
        
        // 如果目标位置是UI元素，直接获取其anchoredPosition
        RectTransform targetRectTransform = targetPosition as RectTransform;
        if (targetRectTransform != null)
        {
            Vector2 position = targetRectTransform.anchoredPosition;
            Debug.Log($"目标位置是RectTransform: {position}");
            return position;
        }
        
        // 如果目标位置不是UI元素，尝试获取其子物体的RectTransform
        RectTransform childRectTransform = targetPosition.GetComponentInChildren<RectTransform>();
        if (childRectTransform != null)
        {
            Vector2 position = childRectTransform.anchoredPosition;
            Debug.Log($"目标位置子物体是RectTransform: {position}");
            return position;
        }
        
        Debug.Log($"未找到有效的目标位置，使用默认位置");
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
            Debug.LogWarning("ButtonController: 已有Level1字符在飞行中，忽略新的飞行请求");
            return;
        }
        
        StartCoroutine(Level1FlyCharacterCoroutine(character, startPosition, endPosition));
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
            Debug.LogWarning("ButtonController: 已有Level1字符在飞行中，忽略新的飞行请求");
            return;
        }
        
        if (targetPosition == null)
        {
            Debug.LogError("ButtonController: 目标位置未设置");
            return;
        }
        
        Vector2 endPosition = GetTargetUIPosition(targetPosition);
        StartCoroutine(Level1FlyCharacterCoroutine(character, startPosition, endPosition));
    }
    
    /// <summary>
    /// Level1字符飞舞协程
    /// </summary>
    private IEnumerator Level1FlyCharacterCoroutine(string character, Vector2 startPosition, Vector2 endPosition)
    {
        isLevel1Flying = true;
        
        // 创建飞舞的字符对象，使用统一的CreateFlyingCharacter方法
        CreateFlyingCharacter(character, targetPosition);
        
        // 等待动画完成
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
}
