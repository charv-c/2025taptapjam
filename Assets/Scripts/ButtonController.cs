using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class ButtonController : MonoBehaviour
{
    [Header("按钮设置")]
    [SerializeField] private Button splitButton, combineButton,confirmButton, cancelButton;
    [SerializeField] private float hideDelay = 0.1f;
    
    [Header("UI提示设置")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float messageDisplayTime = 3f;
    
    [Header("选择器引用")]
    [SerializeField] private StringSelector stringSelector;
    
    // 单例模式，方便其他脚本访问
    public static ButtonController Instance { get; private set; }
    
    // 飞行动画状态
    private bool isFlyingAnimationActive = false;
    
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
        if (confirmButton != null) confirmButton.gameObject.SetActive(false);
        if (cancelButton != null) cancelButton.gameObject.SetActive(false);
        
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
    }

    private void OnSplitButtonClicked()
    {
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
        if (confirmButton != null) confirmButton.gameObject.SetActive(false);
        if (cancelButton != null) cancelButton.gameObject.SetActive(false);
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
        if (stringSelector != null)
        {
            int selectedCount = stringSelector.GetSelectionCount();
            if (selectedCount != 1)
            {
                stringSelector.ClearSelection();
                return;
            }
            
            string selectedString = stringSelector.FirstSelectedString;
            if (!string.IsNullOrEmpty(selectedString))
            {
                if (PublicData.CanSplitString(selectedString))
                {
                    var (part1, part2) = PublicData.GetStringSplit(selectedString);
                    
                    if (AudioManager.Instance != null && AudioManager.Instance.sfxSplitSuccess != null)
                    {
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxSplitSuccess);
                    }

                    stringSelector.ClearSelection();
                    stringSelector.RemoveAvailableString(selectedString);
                    stringSelector.AddAvailableString(part1);
                    stringSelector.AddAvailableString(part2);
                    stringSelector.RecreateAllButtonsPublic();
                    stringSelector.SetMaxSelectionCount(2);
                }
                else
                {
                    if (AudioManager.Instance != null && AudioManager.Instance.sfxOperationFailure != null)
                    {
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxOperationFailure);
                    }
                    stringSelector.ClearSelection();
                }
            }
        }
    }
    
    private void combineletter()
    {
        if (stringSelector != null)
        {
            int selectedCount = stringSelector.GetSelectionCount();
            if (selectedCount != 2)
            {
                stringSelector.ClearSelection();
                return;
            }
            
            List<string> selectedStrings = stringSelector.SelectedStrings;
            string firstString = selectedStrings[0];
            string secondString = selectedStrings[1];
            
            string originalString = PublicData.FindOriginalString(firstString, secondString);
            if (originalString != null)
            {
                if (AudioManager.Instance != null && AudioManager.Instance.sfxCombineSuccess != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxCombineSuccess);
                }

                stringSelector.ClearSelection();
                stringSelector.RemoveAvailableString(firstString);
                stringSelector.RemoveAvailableString(secondString);
                
                if (PublicData.IsCharacterInTargetList(originalString))
                {
                    Transform targetPosition = PublicData.GetTargetPositionForCharacter(originalString);
                    if (targetPosition != null)
                    {
                        // 先添加到可用字符串列表
                        stringSelector.AddAvailableString(originalString);
                        stringSelector.RecreateAllButtonsPublic();
                        
                        // 延迟一秒后播放飞行动画
                        StartCoroutine(DelayedFlyingAnimation(originalString, targetPosition));
                    }
                    else
                    {
                        stringSelector.AddAvailableString(originalString);
                    }
                }
                else
                {
                    stringSelector.AddAvailableString(originalString);
                }
                
                stringSelector.RecreateAllButtonsPublic();
                stringSelector.SetMaxSelectionCount(2);
                stringSelector.ClearSelection();
            }
            else
            {
                if (AudioManager.Instance != null && AudioManager.Instance.sfxOperationFailure != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxOperationFailure);
                }
                stringSelector.ClearSelection();
            }
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
        stringSelector = selector;
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
        textMesh.fontSize = 72;
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
        float spiralTurns = 3f; // 螺旋圈数
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
}
