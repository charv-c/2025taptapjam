using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
/// <summary>
/// 提示管理器 - 控制提示图片的显示和宽度渐变动画
/// </summary>
public class HintManager : MonoBehaviour
{
    [Header("UI组件引用")]
    [SerializeField] private Button hintButton;        // 提示按钮
    [SerializeField] private Image hintImage;          // 提示图片
    
    [Header("动画设置")]
    [SerializeField] private float targetWidth = 300f;  // 目标宽度（可被动态计算覆盖）
    [SerializeField] private float animationDuration = 1.5f; // 动画持续时间
    
    [Header("初始设置")]
    [SerializeField] private float initialWidth = 0f;   // 初始宽度
    
    [Header("宽度自适应设置")]
    [SerializeField] private float minWidth = 120f;     // 最小宽度
    [SerializeField] private float maxWidth = 900f;     // 最大宽度
    [SerializeField] private float contentPadding = 40f;// 文本左右总内边距
    
    private RectTransform hintImageRect;                // 提示图片的RectTransform
    private bool isAnimating = false;                   // 是否正在播放动画
    private Coroutine widthAnimationCoroutine;          // 宽度动画协程
    private bool isExpanded = false;                    // 当前是否展开
    private Sprite initialButtonSprite;                 // 按钮初始底图
    
    [SerializeField]
    private TextMeshProUGUI hintText;

    [Header("字体设置")]
    [SerializeField] private TMP_FontAsset chineseFont; // 中文字体（可选）

    [Header("场景目标引用")]
    [SerializeField] private GameObject rainObject;     // 雨
    [SerializeField] private GameObject childObject;    // 孩
    [SerializeField] private GameObject hunterObject;   // 猎
    [SerializeField] private GameObject kingObject;     // 王
    [SerializeField] private List<GameObject> sunObjects = new List<GameObject>(); // 多个日（任一显示即可）

    private PlayerController playerController;

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

        // 记录按钮初始底图
        if (hintButton != null && hintButton.image != null)
        {
            initialButtonSprite = hintButton.image.sprite;
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

        // 获取玩家控制器引用
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }

        // 确保中文字体可用
        EnsureChineseFontForHintText();
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
            
            // 初始时隐藏图片与文字
            hintImage.gameObject.SetActive(false);
            HideHintText();
            isExpanded = false;
            
            Debug.Log($"HintManager: 提示图片初始化完成，初始宽度: {initialWidth}");
        }
        else
        {
            Debug.LogError("HintManager: 无法获取hintImage的RectTransform组件");
        }
    }
    
    /// <summary>
    /// 确保 hintText 使用包含中文字形的字体
    /// </summary>
    private void EnsureChineseFontForHintText()
    {
        if (hintText == null) return;

        // 优先使用 Inspector 指定的中文字体
        if (chineseFont != null)
        {
            hintText.font = chineseFont;
            hintText.ForceMeshUpdate();
            return;
        }

        // 其次尝试从 StringSelector 获取（若存在该脚本并提供方法）
        var selector = FindObjectOfType<StringSelector>();
        if (selector != null)
        {
            try
            {
                var font = selector.GetChineseFont();
                if (font != null)
                {
                    chineseFont = font;
                    hintText.font = chineseFont;
                    hintText.ForceMeshUpdate();
                    return;
                }
            }
            catch { /* 忽略异常，继续走默认回退 */ }
        }

        // 最后尝试从 Resources 加载一个默认的中文字体资源
        TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts/SourceHanSerifCN-Heavy SDF 1");
        if (defaultFont != null)
        {
            chineseFont = defaultFont;
            hintText.font = chineseFont;
            hintText.ForceMeshUpdate();
            Debug.Log("HintManager: 使用默认中文字体资源修复字形缺失（如“虎”“雨”）");
        }
        else
        {
            Debug.LogWarning("HintManager: 未能找到中文字体，请在Inspector为 chineseFont 指定包含全字库的TMP字体");
        }
    }
    
    /// <summary>
    /// 提示按钮点击事件处理（展开/收起切换）
    /// </summary>
    public void OnHintButtonClicked()
    {
        Debug.Log("HintManager: 提示按钮被点击");
        
        if (isAnimating)
        {
            Debug.Log("HintManager: 动画正在进行中，忽略点击");
            return;
        }

        // 若当前已展开，则收起，并切回按钮初始底图
        if (hintImage != null && hintImage.gameObject.activeSelf && isExpanded)
        {
            if (hintButton != null && hintButton.image != null && initialButtonSprite != null)
            {
                hintButton.image.sprite = initialButtonSprite;
            }
            // 收起前隐藏文字
            HideHintText();
            StartCollapseAnimation();
            return;
        }

        // 先选择提示文案（用于计算宽度）
        string text = GetRandomEligibleHintText();
        if (hintText != null)
        {
            hintText.text = text;
            hintText.ForceMeshUpdate();
        }

        // 动态计算目标宽度
        targetWidth = ComputeTargetWidth(text);
        
        // 展开前隐藏文字
        HideHintText();
        
        // 显示并展开
        ShowHintImage();
        StartExpandAnimation();
    }
    
    // 计算给定文本所需的目标宽度（基于TMP首选宽度+内边距，限制最小/最大）
    private float ComputeTargetWidth(string text)
    {
        if (hintText == null)
        {
            return Mathf.Clamp(targetWidth, minWidth, maxWidth);
        }
        // 更新TMP以获得精准preferredWidth
        hintText.text = text;
        hintText.ForceMeshUpdate();
        float preferred = hintText.preferredWidth;
        float computed = preferred + contentPadding;
        return Mathf.Clamp(computed, minWidth, maxWidth);
    }

    // 展开动画（initialWidth -> targetWidth）
    private void StartExpandAnimation()
    {
        if (widthAnimationCoroutine != null)
        {
            StopCoroutine(widthAnimationCoroutine);
        }
        // 确保起始宽度
        Vector2 sizeDelta = hintImageRect.sizeDelta;
        sizeDelta.x = initialWidth;
        hintImageRect.sizeDelta = sizeDelta;

        widthAnimationCoroutine = StartCoroutine(AnimateWidth(initialWidth, targetWidth, false));
    }

    // 收起动画（当前宽度 -> initialWidth），结束后隐藏
    private void StartCollapseAnimation()
    {
        if (widthAnimationCoroutine != null)
        {
            StopCoroutine(widthAnimationCoroutine);
        }
        float current = hintImageRect != null ? hintImageRect.sizeDelta.x : targetWidth;
        widthAnimationCoroutine = StartCoroutine(AnimateWidth(current, initialWidth, true));
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
    /// 宽度渐变动画协程（from -> to）。若 hideAtEnd 为 true，结束时隐藏图片。
    /// </summary>
    private IEnumerator AnimateWidth(float fromWidth, float toWidth, bool hideAtEnd)
    {
        if (hintImageRect == null)
        {
            Debug.LogError("HintManager: hintImageRect为空，无法执行动画");
            yield break;
        }
        
        isAnimating = true;
        isExpanded = !hideAtEnd;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / animationDuration);
            float currentWidth = Mathf.Lerp(fromWidth, toWidth, progress);
            
            Vector2 sizeDelta = hintImageRect.sizeDelta;
            sizeDelta.x = currentWidth;
            hintImageRect.sizeDelta = sizeDelta;
            
            yield return null;
        }
        
        // 确保最终宽度精确
        Vector2 finalSizeDelta = hintImageRect.sizeDelta;
        finalSizeDelta.x = toWidth;
        hintImageRect.sizeDelta = finalSizeDelta;

        if (hideAtEnd)
        {
            if (hintImage != null)
            {
                hintImage.gameObject.SetActive(false);
            }
        }
        else
        {
            // 展开完成后显示文字
            ShowHintText();
        }
        
        isAnimating = false;
        widthAnimationCoroutine = null;
    }

    private void ShowHintText()
    {
        if (hintText != null)
        {
            hintText.gameObject.SetActive(true);
        }
    }

    private void HideHintText()
    {
        if (hintText != null)
        {
            hintText.gameObject.SetActive(false);
        }
    }

    // 根据当前场景与玩家状态，返回一条可触发的提示文案（随机）
    private string GetRandomEligibleHintText()
    {
        List<string> candidates = new List<string>();

        bool rainVisible = IsObjectEnabled(rainObject);
        bool childVisible = IsObjectEnabled(childObject);
        bool hunterVisible = IsObjectEnabled(hunterObject);
        bool kingVisible = IsObjectEnabled(kingObject);
        bool sunVisible = IsAnyObjectEnabled(sunObjects); // 多个“日”，任一启用即可

        string carry = GetCurrentCarryCharacter();

        // 让雨“停”
        if (rainVisible)
        {
            candidates.Add("大雨拦路，想办法让雨「停」下吧");
        }

        // 寻找“伙”伴（孩启用）
        if (childVisible)
        {
            candidates.Add("孩童孤单，若能为他寻个「伙」伴……");
        }

        // 让猎人“休”息（猎启用）
        if (hunterVisible)
        {
            candidates.Add("猎人终日巡守，让他稍作「休」息吧");
        }

        // 需要“侠”士（王启用，且玩家当前carry != 侠）
        if (kingVisible && carry != "侠")
        {
            candidates.Add("猛虎当道，恐怕需要一位「侠」士相助");
        }

        // 需要“仙”人（雨未启用但日启用）
        if (!rainVisible && sunVisible)
        {
            candidates.Add("日轮高悬，凡人难及，或可化「仙」探寻");
        }

        if (candidates.Count == 0)
        {
            return "暂无可用提示";
        }

        int idx = Random.Range(0, candidates.Count);
        return candidates[idx];
    }

    // 判断对象是否“显示”（按启用状态）
    private bool IsObjectEnabled(GameObject obj)
    {
        if (obj == null) return false;

        // UI 图片
        var img = obj.GetComponent<Image>();
        if (img != null) return img.enabled;

        // 2D 精灵
        var sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null) return sr.enabled;

        // 通用渲染器
        var rend = obj.GetComponent<Renderer>();
        if (rend != null) return rend.enabled;

        // CanvasGroup 透明控制
        var cg = obj.GetComponent<CanvasGroup>();
        if (cg != null) return cg.alpha > 0.01f;

        // 文本（如TMP）
        var text = obj.GetComponent<TMP_Text>();
        if (text != null) return text.enabled;

        // 其它Graphic基类
        var graphic = obj.GetComponent<Graphic>();
        if (graphic != null) return graphic.enabled;

        // 回退：若无以上组件，则认为未启用显示
        return false;
    }

    // 列表中任意对象被“启用显示”
    private bool IsAnyObjectEnabled(List<GameObject> objects)
    {
        if (objects == null || objects.Count == 0) return false;
        for (int i = 0; i < objects.Count; i++)
        {
            if (IsObjectEnabled(objects[i])) return true;
        }
        return false;
    }

    // 获取当前玩家携带字符（仅用于“侠”提示的前置条件）
    private string GetCurrentCarryCharacter()
    {
        if (playerController != null && playerController.GetCurrentPlayer() != null)
        {
            return playerController.GetCurrentPlayer().CarryCharacter;
        }
        return string.Empty;
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
        isExpanded = false;
        
        if (hintImageRect != null)
        {
            // 重置宽度
            Vector2 sizeDelta = hintImageRect.sizeDelta;
            sizeDelta.x = initialWidth;
            hintImageRect.sizeDelta = sizeDelta;
            
            // 隐藏图片与文字
            if (hintImage != null)
            {
                hintImage.gameObject.SetActive(false);
            }
            HideHintText();
            
            Debug.Log("HintManager: 提示图片状态已重置");
        }
    }
    
    /// <summary>
    /// 设置目标宽度（可在运行时覆盖自动计算）
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
