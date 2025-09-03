using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 提示管理器 - 控制提示图片的显示和宽度渐变动画
/// </summary>
public class HintManager : MonoBehaviour
{
    [Header("UI组件引用")]
    [SerializeField] private Button hintButton;        // 提示按钮
    [SerializeField] private Image hintImage;          // 提示图片
    
    [Header("动画设置")]
    [SerializeField] private float targetWidth = 300f;  // 目标宽度
    [SerializeField] private float animationDuration = 1.5f; // 动画持续时间
    
    [Header("初始设置")]
    [SerializeField] private float initialWidth = 0f;   // 初始宽度
    
    private RectTransform hintImageRect;                // 提示图片的RectTransform
    private bool isAnimating = false;                   // 是否正在播放动画
    private Coroutine widthAnimationCoroutine;          // 宽度动画协程
    
    private void Awake()
    {
        // 获取组件引用
        if (hintImage != null)
        {
            hintImageRect = hintImage.GetComponent<RectTransform>();
        }
        
        // 如果没有手动设置hintButton，尝试在子对象中查找
        if (hintButton == null)
        {
            hintButton = GetComponentInChildren<Button>();
        }
        
        // 如果没有手动设置hintImage，尝试在子对象中查找
        if (hintImage == null)
        {
            hintImage = GetComponentInChildren<Image>();
            if (hintImage != null)
            {
                hintImageRect = hintImage.GetComponent<RectTransform>();
            }
        }
    }
    
    private void Start()
    {
        // 设置初始状态
        InitializeHintImage();
        
        // 绑定按钮点击事件
        if (hintButton != null)
        {
            hintButton.onClick.AddListener(OnHintButtonClicked);
        }
        else
        {
            Debug.LogWarning("HintManager: 未找到hintButton，无法绑定点击事件");
        }
    }
    
    /// <summary>
    /// 初始化提示图片
    /// </summary>
    private void InitializeHintImage()
    {
        if (hintImageRect != null)
        {
            // 设置初始宽度
            Vector2 sizeDelta = hintImageRect.sizeDelta;
            sizeDelta.x = initialWidth;
            hintImageRect.sizeDelta = sizeDelta;
            
            // 初始时隐藏图片
            hintImage.gameObject.SetActive(false);
            
            Debug.Log($"HintManager: 提示图片初始化完成，初始宽度: {initialWidth}");
        }
        else
        {
            Debug.LogError("HintManager: 无法获取hintImage的RectTransform组件");
        }
    }
    
    /// <summary>
    /// 提示按钮点击事件处理
    /// </summary>
    public void OnHintButtonClicked()
    {
        Debug.Log("HintManager: 提示按钮被点击");
        
        if (isAnimating)
        {
            Debug.Log("HintManager: 动画正在进行中，忽略点击");
            return;
        }
        
        // 显示提示图片
        ShowHintImage();
        
        // 开始宽度渐变动画
        StartWidthAnimation();
    }
    
    /// <summary>
    /// 显示提示图片
    /// </summary>
    private void ShowHintImage()
    {
        if (hintImage != null)
        {
            hintImage.gameObject.SetActive(true);
            Debug.Log("HintManager: 提示图片已显示");
        }
    }
    
    /// <summary>
    /// 开始宽度渐变动画
    /// </summary>
    private void StartWidthAnimation()
    {
        if (widthAnimationCoroutine != null)
        {
            StopCoroutine(widthAnimationCoroutine);
        }
        
        widthAnimationCoroutine = StartCoroutine(AnimateWidth());
    }
    
    /// <summary>
    /// 宽度渐变动画协程
    /// </summary>
    private IEnumerator AnimateWidth()
    {
        if (hintImageRect == null)
        {
            Debug.LogError("HintManager: hintImageRect为空，无法执行动画");
            yield break;
        }
        
        isAnimating = true;
        Debug.Log($"HintManager: 开始宽度渐变动画，从 {initialWidth} 到 {targetWidth}，持续时间: {animationDuration}秒");
        
        float elapsedTime = 0f;
        float startWidth = initialWidth;
        float currentWidth = startWidth;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;
            
            // 使用平滑插值计算当前宽度
            currentWidth = Mathf.Lerp(startWidth, targetWidth, progress);
            
            // 更新图片宽度
            Vector2 sizeDelta = hintImageRect.sizeDelta;
            sizeDelta.x = currentWidth;
            hintImageRect.sizeDelta = sizeDelta;
            
            yield return null;
        }
        
        // 确保最终宽度精确
        Vector2 finalSizeDelta = hintImageRect.sizeDelta;
        finalSizeDelta.x = targetWidth;
        hintImageRect.sizeDelta = finalSizeDelta;
        
        isAnimating = false;
        Debug.Log($"HintManager: 宽度渐变动画完成，最终宽度: {targetWidth}");
        
        widthAnimationCoroutine = null;
    }
    
    /// <summary>
    /// 重置提示图片状态
    /// </summary>
    public void ResetHintImage()
    {
        if (isAnimating && widthAnimationCoroutine != null)
        {
            StopCoroutine(widthAnimationCoroutine);
            widthAnimationCoroutine = null;
        }
        
        isAnimating = false;
        
        if (hintImageRect != null)
        {
            // 重置宽度
            Vector2 sizeDelta = hintImageRect.sizeDelta;
            sizeDelta.x = initialWidth;
            hintImageRect.sizeDelta = sizeDelta;
            
            // 隐藏图片
            if (hintImage != null)
            {
                hintImage.gameObject.SetActive(false);
            }
            
            Debug.Log("HintManager: 提示图片状态已重置");
        }
    }
    
    /// <summary>
    /// 设置目标宽度
    /// </summary>
    /// <param name="newTargetWidth">新的目标宽度</param>
    public void SetTargetWidth(float newTargetWidth)
    {
        targetWidth = newTargetWidth;
        Debug.Log($"HintManager: 目标宽度已设置为 {targetWidth}");
    }
    
    /// <summary>
    /// 设置动画持续时间
    /// </summary>
    /// <param name="newDuration">新的动画持续时间</param>
    public void SetAnimationDuration(float newDuration)
    {
        animationDuration = newDuration;
        Debug.Log($"HintManager: 动画持续时间已设置为 {animationDuration}秒");
    }
    
    /// <summary>
    /// 手动触发提示（可在Inspector中调用测试）
    /// </summary>
    [ContextMenu("手动触发提示")]
    public void ManualTriggerHint()
    {
        Debug.Log("HintManager: 手动触发提示");
        OnHintButtonClicked();
    }
    
    /// <summary>
    /// 重置提示状态（可在Inspector中调用测试）
    /// </summary>
    [ContextMenu("重置提示状态")]
    public void ManualResetHint()
    {
        Debug.Log("HintManager: 手动重置提示状态");
        ResetHintImage();
    }
    
    private void OnDestroy()
    {
        // 清理协程
        if (widthAnimationCoroutine != null)
        {
            StopCoroutine(widthAnimationCoroutine);
        }
        
        // 移除按钮事件监听
        if (hintButton != null)
        {
            hintButton.onClick.RemoveListener(OnHintButtonClicked);
        }
    }
}
