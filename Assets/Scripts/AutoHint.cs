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

    private List<TMP_Text> childTexts = new List<TMP_Text>();
    private CanvasGroup canvasGroup;
    private Image selfImage;
    private Coroutine flowCoroutine;

    private void Awake()
    {
        CacheChildTexts();
        EnsureCanvasGroup();
        selfImage = GetComponent<Image>();
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
        string content = ResolveValue(message);
        // 中断并立即隐藏
        StopFlow();
        ImmediateHide();

        // 设置文本/图片；若为空则保持隐藏
        if (!string.IsNullOrEmpty(content))
        {
            bool hasTextReceiver = ApplyTextToChildren(content);
            // 若没有文本接收者，尝试将值映射为字符图片到自身 Image
            if (!hasTextReceiver && selfImage != null)
            {
                var sprite = PublicData.GetCharacterSprite(content);
                if (sprite != null)
                {
                    selfImage.sprite = sprite;
                    selfImage.enabled = true;
                    selfImage.preserveAspect = true;
                }
            }
            // 启动淡入-停留-淡出流程
            flowCoroutine = StartCoroutine(Flow());
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
        if (string.IsNullOrEmpty(message)) return string.Empty;
        if (PublicData.autoHintDict != null && PublicData.autoHintDict.TryGetValue(message, out string value))
        {
            return value ?? string.Empty;
        }
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
        }
        foreach (var t in childTexts)
        {
            if (t != null)
            {
                t.text = content;
                t.ForceMeshUpdate();
                applied = true;
            }
        }
        return applied;
    }
}
