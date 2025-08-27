using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 文字飞舞按钮控制器
/// 用于快捷触发文字飞舞动画
/// </summary>
public class CharacterFlyButton : MonoBehaviour
{
    [Header("按钮设置")]
    [SerializeField] private Button flyButton;
    [SerializeField] private TextMeshProUGUI buttonText;
    
    [Header("飞舞设置")]
    [SerializeField] private string characterToFly = "蝶"; // 要飞舞的字符
    [SerializeField] private Transform targetPosition; // 目标位置
    [SerializeField] private Canvas targetCanvas; // 目标Canvas
    
    [Header("UI设置")]
    [SerializeField] private string buttonTextContent = "触发飞舞"; // 按钮文字
    
    void Start()
    {
        // 检查ButtonController是否存在
        if (ButtonController.Instance == null)
        {
            Debug.LogError("CharacterFlyButton: 未找到ButtonController实例");
            if (flyButton != null)
            {
                flyButton.interactable = false;
            }
            return;
        }
        
        // 设置按钮文字
        if (buttonText != null)
        {
            buttonText.text = buttonTextContent;
        }
        
        // 设置目标位置和Canvas
        if (targetPosition != null)
        {
            ButtonController.Instance.SetLevel1TargetPosition(targetPosition);
        }
        
        if (targetCanvas != null)
        {
            ButtonController.Instance.SetLevel1TargetCanvas(targetCanvas);
        }
        
        // 添加按钮点击事件
        if (flyButton != null)
        {
            flyButton.onClick.AddListener(OnFlyButtonClicked);
        }
        
        Debug.Log("CharacterFlyButton: 初始化完成");
    }
    
    /// <summary>
    /// 按钮点击事件处理
    /// </summary>
    private void OnFlyButtonClicked()
    {
        // 播放按钮点击音效
        if (AudioManager.Instance != null && AudioManager.Instance.sfxUIClick != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxUIClick);
        }
        
        // 检查是否有TutorialManager，如果有则调用其方法
        if (TutorialManager.Instance != null)
        {
            Debug.Log("CharacterFlyButton: 调用TutorialManager的OnCharacterFlyButtonClicked方法");
            TutorialManager.Instance.OnCharacterFlyButtonClicked();
        }
        else
        {
            // 如果没有TutorialManager，则使用原来的逻辑
            Debug.Log("CharacterFlyButton: 未找到TutorialManager，使用默认飞舞逻辑");
            
            if (ButtonController.Instance == null)
            {
                Debug.LogError("CharacterFlyButton: ButtonController实例为空");
                return;
            }
            
            if (ButtonController.Instance.IsLevel1Flying())
            {
                Debug.Log("CharacterFlyButton: 已有字符在飞行中，忽略点击");
                return;
            }
            
            // 获取起始位置（按钮位置）和目标位置
            Vector2 startPosition = GetButtonPosition();
            Vector2 endPosition = GetTargetPosition();
            
            Debug.Log($"CharacterFlyButton: 开始飞舞动画 - 字符={characterToFly}, 起始位置={startPosition}, 目标位置={endPosition}");
            
            // 开始飞舞动画
            ButtonController.Instance.StartLevel1CharacterFly(characterToFly, startPosition, endPosition);
        }
    }
    
    /// <summary>
    /// 设置要飞舞的字符
    /// </summary>
    public void SetCharacterToFly(string character)
    {
        characterToFly = character;
        Debug.Log($"CharacterFlyButton: 设置飞舞字符为 {character}");
    }
    
    /// <summary>
    /// 设置目标位置
    /// </summary>
    public void SetTargetPosition(Transform target)
    {
        targetPosition = target;
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.SetLevel1TargetPosition(target);
        }
        Debug.Log($"CharacterFlyButton: 设置目标位置为 {target?.name}");
    }
    
    /// <summary>
    /// 设置目标Canvas
    /// </summary>
    public void SetTargetCanvas(Canvas canvas)
    {
        targetCanvas = canvas;
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.SetLevel1TargetCanvas(canvas);
        }
        Debug.Log($"CharacterFlyButton: 设置目标Canvas为 {canvas?.name}");
    }
    
    /// <summary>
    /// 设置按钮文字
    /// </summary>
    public void SetButtonText(string text)
    {
        buttonTextContent = text;
        if (buttonText != null)
        {
            buttonText.text = text;
        }
    }
    
    /// <summary>
    /// 检查是否正在飞行
    /// </summary>
    public bool IsFlying()
    {
        return ButtonController.Instance != null && ButtonController.Instance.IsLevel1Flying();
    }
    
    /// <summary>
    /// 获取按钮的位置
    /// </summary>
    public Vector2 GetButtonPosition()
    {
        if (flyButton != null)
        {
            RectTransform buttonRect = flyButton.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                return buttonRect.anchoredPosition;
            }
        }
        return Vector2.zero;
    }
    
    /// <summary>
    /// 获取目标位置
    /// </summary>
    public Vector2 GetTargetPosition()
    {
        if (targetPosition != null)
        {
            // 直接获取UI坐标
            RectTransform targetRectTransform = targetPosition as RectTransform;
            if (targetRectTransform != null)
            {
                return targetRectTransform.anchoredPosition;
            }
            else
            {
                // 如果不是UI元素，尝试获取其子物体的RectTransform
                RectTransform childRectTransform = targetPosition.GetComponentInChildren<RectTransform>();
                if (childRectTransform != null)
                {
                    return childRectTransform.anchoredPosition;
                }
            }
        }
        return Vector2.zero;
    }
}
