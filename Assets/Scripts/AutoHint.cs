using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// AutoHint: 收到广播时，按消息作为键，从 PublicData.autoHintDict 取对应值并显示到自身子物体文字
/// - 默认隐藏（仅 alpha=0，不停用 GameObject）
/// - 收到广播：中断当前流程，立即隐藏 -> 设置文字/图片 -> 淡入 -> 停留 -> 淡出
/// - 期间再次收到广播：立刻中断并按新消息重启流程
/// - 若无TMP文本子物体，尝试将值对应的字符图片设置到自身 Image
/// </summary>
public class AutoHint : MonoBehaviour
{
    [Header("显示设置")]
    [SerializeField] private string joinSeparator = "、"; // 备用：批量显示时的分隔符
    [SerializeField] private bool includeChildrenInactive = true; // 是否包含未激活子物体

    [Header("时序（秒）")]
    [SerializeField] private float fadeInDuration = 0.25f;
    [SerializeField] private float visibleDuration = 1.50f;
    [SerializeField] private float fadeOutDuration = 0.25f;

    [Header("自适应宽度设置")]
    [SerializeField] private RectTransform targetRect; // 需要调整宽度的Rect（通常是自身或背景Image）
    [SerializeField] private float minWidth = 120f;
    [SerializeField] private float maxWidth = 900f;
    [SerializeField] private float contentPadding = 40f; // 左右总内边距

    private List<TMP_Text> childTexts = new List<TMP_Text>();
    private CanvasGroup canvasGroup;
    private Image selfImage;
    private Coroutine flowCoroutine;

    private void Awake()
    {
        CacheChildTexts();
        EnsureCanvasGroup();
        selfImage = GetComponent<Image>();
        if (targetRect == null)
        {
            targetRect = GetComponent<RectTransform>();
        }
        // 默认隐藏（但不禁用对象，确保可接收广播）
        ImmediateHide();
    }

    private void CacheChildTexts()
    {
        childTexts.Clear();
        if (includeChildrenInactive)
        {
            GetComponentsInChildren(true, childTexts);
        }
        else
        {
            GetComponentsInChildren(false, childTexts);
        }
    }

    private void EnsureCanvasGroup()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    // 由广播系统调用
    public void ReceiveBroadcast(string message)
    {
        Debug.Log($"AutoHint: 收到广播消息: '{message}'");
        
        string content = ResolveValue(message);
        Debug.Log($"AutoHint: 解析后的内容: '{content}'");
        
        // 中断并立即隐藏
        StopFlow();
        ImmediateHide();

        // 设置文本/图片；若为空则保持隐藏
        if (!string.IsNullOrEmpty(content))
        {
            bool hasTextReceiver = ApplyTextToChildren(content);
            Debug.Log($"AutoHint: 是否有文本接收者: {hasTextReceiver}");
            Debug.Log($"AutoHint: 子物体文本数量: {childTexts?.Count ?? 0}");
            
            // 自适应宽度：若有文本接收者则按文本宽度调整目标Rect宽度
            if (hasTextReceiver)
            {
                AdjustWidthToText();
                Debug.Log("AutoHint: 已调整宽度到文本");
            }
            
            // 若没有文本接收者，尝试将值映射为字符图片到自身 Image
            if (!hasTextReceiver && selfImage != null)
            {
                var sprite = PublicData.GetCharacterSprite(content);
                if (sprite != null)
                {
                    selfImage.sprite = sprite;
                    selfImage.enabled = true;
                    selfImage.preserveAspect = true;
                    Debug.Log($"AutoHint: 已设置字符图片: {sprite.name}");
                }
                else
                {
                    Debug.LogWarning($"AutoHint: 未找到字符 '{content}' 对应的图片");
                }
            }
            else if (!hasTextReceiver)
            {
                Debug.LogWarning("AutoHint: 没有文本接收者且selfImage为null，无法显示内容");
            }
            
            // 启动淡入-停留-淡出流程
            flowCoroutine = StartCoroutine(Flow());
            Debug.Log("AutoHint: 已启动显示流程");
        }
        else
        {
            Debug.LogWarning($"AutoHint: 内容为空，不显示提示");
        }
    }

    private System.Collections.IEnumerator Flow()
    {
        // 仅控制透明度，不启用/禁用对象
        yield return FadeTo(1f, fadeInDuration);
        // 停留
        float t = 0f;
        while (t < visibleDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }
        // 淡出
        yield return FadeTo(0f, fadeOutDuration);
        flowCoroutine = null;
    }

    private System.Collections.IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (canvasGroup == null) yield break;
        float start = canvasGroup.alpha;
        float t = 0f;
        // 设置交互屏蔽
        bool targetInteractable = targetAlpha > 0.001f;
        if (duration <= 0f)
        {
            canvasGroup.alpha = targetAlpha;
            canvasGroup.blocksRaycasts = targetInteractable;
            canvasGroup.interactable = targetInteractable;
            yield break;
        }
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            canvasGroup.alpha = Mathf.Lerp(start, targetAlpha, p);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
        canvasGroup.blocksRaycasts = targetInteractable;
        canvasGroup.interactable = targetInteractable;
    }

    private void StopFlow()
    {
        if (flowCoroutine != null)
        {
            StopCoroutine(flowCoroutine);
            flowCoroutine = null;
        }
    }

    private void ImmediateHide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    private string ResolveValue(string message)
    {
        Debug.Log($"AutoHint.ResolveValue: 开始解析消息: '{message}'");
        
        if (string.IsNullOrEmpty(message)) 
        {
            Debug.Log("AutoHint.ResolveValue: 消息为空，返回空字符串");
            return string.Empty;
        }
        
        if (PublicData.autoHintDict != null)
        {
            Debug.Log($"AutoHint.ResolveValue: autoHintDict不为空，包含 {PublicData.autoHintDict.Count} 个条目");
            Debug.Log($"AutoHint.ResolveValue: 检查键 '{message}' 是否存在");
            
            if (PublicData.autoHintDict.TryGetValue(message, out string value))
            {
                Debug.Log($"AutoHint.ResolveValue: 找到值: '{value}'");
                return value ?? string.Empty;
            }
            else
            {
                Debug.LogWarning($"AutoHint.ResolveValue: 未找到键 '{message}' 对应的值");
                // 打印字典中的所有键以便调试
                Debug.Log($"AutoHint.ResolveValue: 字典中的键包括: {string.Join(", ", PublicData.autoHintDict.Keys)}");
            }
        }
        else
        {
            Debug.LogError("AutoHint.ResolveValue: PublicData.autoHintDict为空");
        }
        
        // 如果广播内容不在字典中，返回空字符串（不显示提示）
        Debug.Log("AutoHint.ResolveValue: 返回空字符串");
        return string.Empty;
    }

    // 可选：显示所有值（未默认使用）
    private void ShowAllValues()
    {
        if (PublicData.autoHintDict == null)
        {
            ApplyTextToChildren(string.Empty);
            return;
        }
        string content = string.Join(joinSeparator, PublicData.autoHintDict.Values);
        ApplyTextToChildren(content);
    }

    private bool ApplyTextToChildren(string content)
    {
        bool applied = false;
        if (childTexts == null || childTexts.Count == 0)
        {
            CacheChildTexts();
            Debug.Log($"AutoHint: 重新缓存子物体文本，数量: {childTexts?.Count ?? 0}");
        }
        
        Debug.Log($"AutoHint: 开始应用文本到子物体，内容: '{content}'");
        
        foreach (var t in childTexts)
        {
            if (t != null)
            {
                Debug.Log($"AutoHint: 设置文本到子物体: {t.gameObject.name}");
                t.text = content;
                t.ForceMeshUpdate();
                applied = true;
            }
            else
            {
                Debug.LogWarning("AutoHint: 发现null的子物体文本组件");
            }
        }
        
        Debug.Log($"AutoHint: 应用文本结果: {applied}");
        return applied;
    }

    private void AdjustWidthToText()
    {
        if (targetRect == null) return;
        float preferredWidth = 0f;
        for (int i = 0; i < childTexts.Count; i++)
        {
            var t = childTexts[i];
            if (t == null) continue;
            // 确保已更新网格，获取精确首选宽度
            t.ForceMeshUpdate();
            preferredWidth = Mathf.Max(preferredWidth, t.preferredWidth);
        }
        float computed = Mathf.Clamp(preferredWidth + contentPadding, minWidth, maxWidth);
        var size = targetRect.sizeDelta;
        size.x = computed;
        targetRect.sizeDelta = size;
    }
}
