using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 场景类型枚举
/// </summary>
public enum SceneType
{
    Level2,
    Level3,
    Level4
}
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
    [SerializeField] private float animationDuration = 0.1f; // 动画持续时间
    
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
    
    [Header("自动隐藏设置")]
    [SerializeField] private float autoHideDelay = 1f;  // 自动隐藏延迟时间（秒）
    private Coroutine autoHideCoroutine;                // 自动隐藏协程
    private bool isMouseOverHint = false;               // 鼠标是否悬停在提示区域
    
    [Header("按钮状态切换")]
    [SerializeField] private Sprite expandedButtonSprite;  // 展开时的按钮底图
    
    [Header("提示池可视化（只读）")]
    [SerializeField, TextArea(10, 20)] private string hintPoolDisplay = "";
    
    [SerializeField]
    private TextMeshProUGUI hintText;

    [Header("字体设置")]
    [SerializeField] private TMP_FontAsset chineseFont; // 中文字体（可选）

    [Header("场景设置")]
    [SerializeField] private SceneType currentScene = SceneType.Level2; // 当前场景类型
    public SceneType CurrentSceneType { get { return currentScene; } }
    
    [Header("Level2场景目标引用")]
    [SerializeField] private GameObject rainObject;     // 雨
    [SerializeField] private GameObject childObject;    // 孩
    [SerializeField] private GameObject hunterObject;   // 猎
    [SerializeField] private GameObject kingObject;     // 王
    [SerializeField] private List<GameObject> sunObjects = new List<GameObject>(); // 多个日（任一显示即可）
    
    [Header("Level3场景目标引用")]
    [SerializeField] private GameObject leafObject;     // 叶
    [SerializeField] private GameObject oldObject;      // 老
    [SerializeField] private GameObject lifeObject;     // 生


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
        
        // 限制仅响应鼠标点击：移除所有 onClick 监听并禁用导航（屏蔽键盘/手柄触发）
        if (hintButton != null)
        {
            hintButton.onClick.RemoveAllListeners();
            var nav = hintButton.navigation;
            nav.mode = Navigation.Mode.None;
            hintButton.navigation = nav;

            // 仅响应鼠标左键点击
            var trigger = hintButton.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = hintButton.gameObject.AddComponent<EventTrigger>();
            }
            // 避免重复添加
            if (trigger.triggers == null)
            {
                trigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();
            }
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((eventData) =>
            {
                var pd = eventData as PointerEventData;
                if (pd != null && pd.button == PointerEventData.InputButton.Left)
                {
                    OnHintButtonClicked();
                }
            });
            trigger.triggers.Add(entry);
        }
        else
        {
            Debug.LogWarning("HintManager: 未找到hintButton，无法设置导航模式");
        }

        // 获取玩家控制器引用
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }

        // 确保中文字体可用
        EnsureChineseFontForHintText();
        
        // 设置鼠标悬停检测
        SetupMouseHoverDetection();
    }
    
    /// <summary>
    /// 设置鼠标悬停检测
    /// </summary>
    private void SetupMouseHoverDetection()
    {
        // 为hintButton添加鼠标悬停检测
        if (hintButton != null)
        {
            var buttonTrigger = hintButton.gameObject.GetComponent<EventTrigger>();
            if (buttonTrigger == null)
            {
                buttonTrigger = hintButton.gameObject.AddComponent<EventTrigger>();
            }
            if (buttonTrigger.triggers == null)
            {
                buttonTrigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();
            }
            
            // 鼠标进入事件
            var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener((eventData) => OnMouseEnterHint());
            buttonTrigger.triggers.Add(enterEntry);
            
            // 鼠标离开事件
            var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener((eventData) => OnMouseExitHint());
            buttonTrigger.triggers.Add(exitEntry);
        }
        
        // 为hintImage添加鼠标悬停检测
        if (hintImage != null)
        {
            var imageTrigger = hintImage.gameObject.GetComponent<EventTrigger>();
            if (imageTrigger == null)
            {
                imageTrigger = hintImage.gameObject.AddComponent<EventTrigger>();
            }
            if (imageTrigger.triggers == null)
            {
                imageTrigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();
            }
            
            // 鼠标进入事件
            var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener((eventData) => OnMouseEnterHint());
            imageTrigger.triggers.Add(enterEntry);
            
            // 鼠标离开事件
            var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener((eventData) => OnMouseExitHint());
            imageTrigger.triggers.Add(exitEntry);
        }
    }
    
    /// <summary>
    /// 鼠标进入提示区域
    /// </summary>
    private void OnMouseEnterHint()
    {
        isMouseOverHint = true;
        // 取消自动隐藏计时器
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }
        //Debug.Log("HintManager: 鼠标进入提示区域，取消自动隐藏");
    }
    
    /// <summary>
    /// 鼠标离开提示区域
    /// </summary>
    private void OnMouseExitHint()
    {
        isMouseOverHint = false;
        // 如果提示已展开且鼠标离开，立即启动自动隐藏计时器
        if (isExpanded && hintImage != null && hintImage.gameObject.activeSelf)
        {
            StartAutoHideTimer();
            //Debug.Log("HintManager: 鼠标离开提示区域，启动自动隐藏计时器");
        }
    }
    
    /// <summary>
    /// 启动自动隐藏计时器
    /// </summary>
    private void StartAutoHideTimer()
    {
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
        }
        autoHideCoroutine = StartCoroutine(AutoHideCoroutine());
    }
    
    /// <summary>
    /// 自动隐藏协程 - 从鼠标离开提示区域开始计时
    /// </summary>
    private IEnumerator AutoHideCoroutine()
    {
        yield return new WaitForSeconds(autoHideDelay);
        
        // 检查是否仍然需要隐藏（鼠标不在提示区域且提示仍然展开）
        if (!isMouseOverHint && isExpanded && hintImage != null && hintImage.gameObject.activeSelf)
        {
            //Debug.Log("HintManager: 鼠标离开提示区域超过1秒，自动隐藏提示");
            // 切换按钮底图为初始状态
            if (hintButton != null && hintButton.image != null && initialButtonSprite != null)
            {
                hintButton.image.sprite = initialButtonSprite;
            }
            // 隐藏文字并收起
            HideHintText();
            StartCollapseAnimation();
        }
        
        autoHideCoroutine = null;
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
            
            //Debug.Log($"HintManager: 提示图片初始化完成，初始宽度: {initialWidth}");
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
        //Debug.Log("HintManager: 提示按钮被点击");
        
        if (isAnimating)
        {
            //Debug.Log("HintManager: 动画正在进行中，忽略点击");
            return;
        }

        // 若当前已展开，则收起，并切回按钮初始底图
        if (hintImage != null && hintImage.gameObject.activeSelf && isExpanded)
        {
            // 取消自动隐藏计时器
            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
                autoHideCoroutine = null;
            }
            
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
        
        // 切换按钮底图到展开状态
        if (hintButton != null && hintButton.image != null && expandedButtonSprite != null)
        {
            hintButton.image.sprite = expandedButtonSprite;
        }
        
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
            //Debug.Log("HintManager: 提示图片已显示");
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
        string carry = GetCurrentCarryCharacter();

        // 根据场景类型获取不同的目标状态
        if (currentScene == SceneType.Level2)
        {
            candidates = GetLevel2HintCandidates(carry);
            
            // Level2：检查是否需要状态重置（玩家处于化字状态，且该形态对应互动均已完成）
            string initialCarryCharacter = GetCurrentPlayerInitialCarryCharacter();
            if (!string.IsNullOrEmpty(carry) && carry != initialCarryCharacter)
            {
                if (IsCarryFormInteractionsCompleted(carry))
                {
                    // 唯一的"状态重置提示"
                    return $"此形态之事已毕，先回归「{initialCarryCharacter}」再继续吧";
                }
            }

            if (candidates.Count == 0)
            {
                // Level2兜底提示：根据场景中是否仍有可收集且启用显示的对象给出不同文案
                Highlight[] allHighlights = FindObjectsOfType<Highlight>();
                bool anyCollectableActive = false;
                for (int i = 0; i < allHighlights.Length; i++)
                {
                    Highlight h = allHighlights[i];
                    if (h != null && h.IsCollectableActive())
                    {
                        anyCollectableActive = true;
                        break;
                    }
                }

                if (anyCollectableActive)
                {
                    return "天地间似乎还有文字，仔细观察一番吧";
                }
                else
                {
                    return "万物齐备，看看目标字如何拆分组合吧";
                }
            }

            int idx = Random.Range(0, candidates.Count);
            return candidates[idx];
        }
        else if (currentScene == SceneType.Level3)
        {
            // Level3：使用新的二级优先级逻辑
            return GetLevel3HintText();
        }
        else if (currentScene == SceneType.Level4)
        {
            // Level4：使用新的二级优先级逻辑
            return GetLevel4HintText();
        }

        // 其他场景的默认兜底逻辑
        return "请仔细观察周围，或许有所发现";
    }

    // 获取Level2场景的提示候选列表
    private List<string> GetLevel2HintCandidates(string carry)
    {
        List<string> candidates = new List<string>();

        bool rainVisible = IsObjectEnabled(rainObject);
        bool childVisible = IsObjectEnabled(childObject);
        bool hunterVisible = IsObjectEnabled(hunterObject);
        bool kingVisible = IsObjectEnabled(kingObject);
        bool sunVisible = IsAnyObjectEnabled(sunObjects); // 多个"日"，任一启用即可

        // 让雨"停"
        if (rainVisible)
        {
            candidates.Add("大雨拦路，想办法让雨「停」下吧");
        }

        // 寻找"伙"伴（孩启用）
        if (childVisible)
        {
            candidates.Add("孩童孤单，若能为他寻个「伙」伴……");
        }

        // 让猎人"休"息（猎启用）
        if (hunterVisible)
        {
            candidates.Add("猎人终日巡守，让他稍作「休」息吧");
        }

        // 需要"侠"士（王启用，且玩家当前carry != 侠）
        if (kingVisible && carry != "侠")
        {
            candidates.Add("猛虎当道，恐怕需要一位「侠」士相助");
        }

        // 需要"仙"人（雨未启用但日启用）
        if (!rainVisible && sunVisible && carry != "仙")
        {
            candidates.Add("日轮高悬，凡人难及，或可化「仙」探寻");
        }

        // "仙"的全场景提示（玩家已化身为"仙"，但仍有"日"未获取）
        if (carry == "仙" && sunVisible)
        {
            candidates.Add("既已成「仙」，何不遍寻天地间的「日」轮");
        }

        return candidates;
    }

    /// <summary>
    /// Level3 二级优先级提示逻辑
    /// </summary>
    /// <returns>提示文案</returns>
    private string GetLevel3HintText()
    {
        // 第一优先级：核心谜题检查
        List<string> hintPool = GetLevel3CorePuzzlePool();
        if (hintPool.Count > 0)
        {
            // 从提示池中随机选择一条提示
            int randomIndex = Random.Range(0, hintPool.Count);
            string selectedHint = hintPool[randomIndex];
            GameLogger.LogDev($"Level3提示: 从{hintPool.Count}条核心谜题提示中选择: {selectedHint}");
            return selectedHint;
        }
        
        // 第二优先级：收集与收尾检查
        return GetLevel3CollectionAndFinishingHint();
    }

    /// <summary>
    /// Level4 二级优先级提示逻辑
    /// </summary>
    /// <returns>提示文案</returns>
    private string GetLevel4HintText()
    {
        // 第一优先级：核心谜题检查
        List<string> hintPool = GetLevel4CorePuzzlePool();
        if (hintPool.Count > 0)
        {
            // 从提示池中随机选择一条提示
            int randomIndex = Random.Range(0, hintPool.Count);
            string selectedHint = hintPool[randomIndex];
            GameLogger.LogDev($"Level4提示: 从{hintPool.Count}条核心谜题提示中选择: {selectedHint}");
            return selectedHint;
        }
        
        // 第二优先级：收集与收尾检查
        return GetLevel4CollectionAndFinishingHint();
    }

    /// <summary>
    /// Level3 收集与收尾检查
    /// </summary>
    /// <returns>收集或合成提示</returns>
    private string GetLevel3CollectionAndFinishingHint()
    {
        // 检查场景中是否还有未拾取的文字
        if (HasLevel3UnpickedItems())
        {
            GameLogger.LogDev("Level3提示: 检测到未拾取的文字，给出收集提示");
            return "龟山汉水间，尚有文字未拾取，再观察一番吧";
        }
        else
        {
            GameLogger.LogDev("Level3提示: 所有文字已收集，给出最终合成提示");
            return "材料齐备，在解字台看看如何拆分组合";
        }
    }

    /// <summary>
    /// Level4 收集与收尾检查
    /// </summary>
    /// <returns>收集或合成提示</returns>
    private string GetLevel4CollectionAndFinishingHint()
    {
        // 检查场景中是否还有未拾取的文字
        if (HasLevel4UnpickedItems())
        {
            GameLogger.LogDev("Level4提示: 检测到未拾取的文字，给出收集提示");
            return "湖光山色间，似乎还藏着文字。";
        }
        else
        {
            GameLogger.LogDev("Level4提示: 所有文字已收集，给出最终合成提示");
            return "材料已齐，去解字台组合吧。";
        }
    }

    /// <summary>
    /// 检查Level3场景中是否还有未拾取的文字
    /// </summary>
    /// <returns>是否存在未拾取的文字</returns>
    private bool HasLevel3UnpickedItems()
    {
        // 检查场景中是否还有可收集但未拾取的文字（通过Highlight组件判断）
        Highlight[] allHighlights = FindObjectsOfType<Highlight>();
        foreach (Highlight highlight in allHighlights)
        {
            if (highlight != null && highlight.IsCollectableActive())
            {
                GameLogger.LogDev($"发现未拾取的文字: {highlight.letter}");
                return true;
            }
        }
        
        GameLogger.LogDev("所有文字已被拾取");
        return false;
    }

    /// <summary>
    /// 检查Level4场景中是否还有未拾取的文字
    /// </summary>
    /// <returns>是否存在未拾取的文字</returns>
    private bool HasLevel4UnpickedItems()
    {
        // 检查场景中是否还有可收集但未拾取的文字（通过Highlight组件判断）
        Highlight[] allHighlights = FindObjectsOfType<Highlight>();
        foreach (Highlight highlight in allHighlights)
        {
            if (highlight != null && highlight.IsCollectableActive())
            {
                GameLogger.LogDev($"发现未拾取的文字: {highlight.letter}");
                return true;
            }
        }
        
        GameLogger.LogDev("所有文字已被拾取");
        return false;
    }


    /// <summary>
    /// Level3 核心谜题提示池 - 按照新的详细逻辑构建
    /// </summary>
    /// <returns>所有满足条件的核心谜题提示</returns>
    private List<string> GetLevel3CorePuzzlePool()
    {
        List<string> hintPool = new List<string>();
        string carry = GetCurrentCarryCharacter();
        
        GameLogger.LogDev($"构建Level3核心谜题提示池, carry='{carry}'");
        
        // 1. 默认场景目标检查（书生、藤蔓）
        AddDefaultSceneTargets(hintPool);
        
        // 2. 琴互动检查（季、雅、孤）
        AddQinInteractionHintsNew(hintPool, carry);
        
        // 3. 老人相关检查
        AddOldManHints(hintPool);
        
        // 4. 滩涂互动检查（籽、芽）
        AddBeachInteractionHintsNew(hintPool, carry);
        
        GameLogger.LogDev($"Level3核心谜题提示池构建完成，共{hintPool.Count}条提示");
        UpdateHintPoolDisplay(hintPool);
        
        return hintPool;
    }

    /// <summary>
    /// Level4 核心谜题提示池 - 按照新的详细逻辑构建
    /// </summary>
    /// <returns>所有满足条件的核心谜题提示</returns>
    private List<string> GetLevel4CorePuzzlePool()
    {
        List<string> hintPool = new List<string>();
        string carry = GetCurrentCarryCharacter();
        
        GameLogger.LogDev($"构建Level4核心谜题提示池, carry='{carry}'");
        
        // 1. 酒互动检查
        AddWineInteractionHints(hintPool, carry);
        
        // 2. 蛇相关检查
        AddSnakeHints(hintPool, carry);
        
        // 3. 皇民相关检查
        AddEmperorHints(hintPool, carry);
        
        // 4. 清枯花相关检查
        AddQingKuhuaHints(hintPool, carry);
        
        // 5. 睛商相关检查
        AddJingShangHints(hintPool, carry);
        
        // 6. 柏鼠相关检查
        AddBaiShuHints(hintPool, carry);
        
        // 7. 帛商相关检查
        AddBoShangHints(hintPool, carry);
        
        GameLogger.LogDev($"Level4核心谜题提示池构建完成，共{hintPool.Count}条提示");
        UpdateHintPoolDisplay(hintPool);
        
        return hintPool;
    }
    
    /// <summary>
    /// 添加默认场景目标提示（书生、藤蔓）
    /// </summary>
    private void AddDefaultSceneTargets(List<string> hintPool)
    {
        // 书生：未互动过，则默认加入提示
        if (HasUninteractedTarget("生"))
        {
            hintPool.Add("书生苦思不解，或可请「孟」子点拨一二");
            GameLogger.LogDev("添加默认场景目标提示: 书生");
        }
        
        // 藤蔓（叶）：未互动过，则默认加入提示
        if (HasUninteractedTarget("叶"))
        {
            hintPool.Add("藤蔓遮蔽了山体，让「蚜」虫来帮忙吧");
            GameLogger.LogDev("添加默认场景目标提示: 藤蔓");
        }
    }
    
    /// <summary>
    /// 添加新的琴互动提示（季、雅、孤）
    /// </summary>
    private void AddQinInteractionHintsNew(List<string> hintPool, string carry)
    {
        // 季：当前仍有需要改变季节才能完成的事项
        if (HasSeasonDependentTasks())
        {
            hintPool.Add("古琴台上，或可以琴扭转「季」节");
            GameLogger.LogDev("添加琴互动提示: 季节切换");
        }
        
        // 雅：隹出现，且"雅"与琴未互动过
        if (IsTargetObjectVisible("隹") && !HasBroadcastHistory("琴雅"))
        {
            hintPool.Add("江城旧事，「雅」音或有反转玄机");
            GameLogger.LogDev("添加琴互动提示: 雅");
        }
        
        // 孤：瓜出现，且"孤"与琴未互动过
        if (IsTargetObjectVisible("瓜") && !HasBroadcastHistory("琴孤"))
        {
            hintPool.Add("「孤」身落寞，或许弹琴能带来「欣」喜");
            GameLogger.LogDev("添加琴互动提示: 孤");
        }
    }
    
    /// <summary>
    /// 添加老人相关提示
    /// </summary>
    private void AddOldManHints(List<string> hintPool)
    {
        // 老：穴已出现（即蚜完成了与叶的互动），且穿未与老人互动过
        if (IsTargetObjectVisible("穴") && !HasBroadcastHistory("穿"))
        {
            hintPool.Add("这位老人，似乎可以「穿」越时光的阻隔");
            GameLogger.LogDev("添加老人提示");
        }
    }
    
    /// <summary>
    /// 添加新的滩涂互动提示（籽、芽）
    /// </summary>
    private void AddBeachInteractionHintsNew(List<string> hintPool, string carry)
    {
        // 籽：玩家变身籽，且芽物品未出现过
        if (!HasObjectEverAppeared("芽"))
        {
            hintPool.Add("春雨可助「籽」在滩涂上悄悄发芽");
            GameLogger.LogDev("添加滩涂互动提示: 籽变芽");
        }
        
        // 芽：玩家变身芽，且花物品未出现过
        if (!HasObjectEverAppeared("花"))
        {
            hintPool.Add("夏日是「芽」在滩涂上生长之时");
            GameLogger.LogDev("添加滩涂互动提示: 芽变花");
        }
    }
    
    /// <summary>
    /// 检查是否存在需要改变季节的任务
    /// </summary>
    private bool HasSeasonDependentTasks()
    {
        // 检查是否有籽字变芽物品的需求（春季需求）
        bool needsSpring = HasPlayerWithCarryCharacter("籽") && !HasObjectEverAppeared("芽");
        
        // 检查是否有芽字变花物品的需求（夏季需求）
        bool needsSummer = HasPlayerWithCarryCharacter("芽") && !HasObjectEverAppeared("花");
        
        // 检查是否有芽物品变瓜物品的需求（夏季需求）
        bool needsSummerForGuaTransform = IsTargetObjectVisible("芽") && !IsTargetObjectVisible("瓜");
        
        bool hasTasks = needsSpring || needsSummer || needsSummerForGuaTransform;
        GameLogger.LogDev($"季节相关任务检查: needsSpring={needsSpring}, needsSummer={needsSummer}, needsSummerForGuaTransform={needsSummerForGuaTransform}, result={hasTasks}");
        
        return hasTasks;
    }
    
    /// <summary>
    /// 检查目标对象是否可见
    /// </summary>
    private bool IsTargetObjectVisible(string targetName)
    {
        // 通过Highlight组件检查
        Highlight[] allHighlights = FindObjectsOfType<Highlight>();
        foreach (Highlight highlight in allHighlights)
        {
            if (highlight != null && highlight.letter == targetName && highlight.IsCollectableActive())
            {
                GameLogger.LogDev($"目标对象可见: {targetName}");
                return true;
            }
        }
        
        // 也检查非Highlight的GameObject（如场景物体）
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue;
            if (obj.name.Contains(targetName) && obj.activeInHierarchy)
            {
                var sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null && sr.enabled)
                {
                    GameLogger.LogDev($"目标对象可见（非Highlight）: {targetName}");
                    return true;
                }
            }
        }
        
        GameLogger.LogDev($"目标对象不可见: {targetName}");
        return false;
    }
    
    /// <summary>
    /// 检查对象是否曾经出现过（通过相关互动的广播历史判断）
    /// </summary>
    private bool HasObjectEverAppeared(string objectName)
    {
        bool everAppeared = false;
        
        switch (objectName)
        {
            case "芽":
                // 芽是通过"滩籽"互动产生的，它的出现是后续“花”或“瓜”的前提。
                // 因此，只要“芽”或它的后续产物（“瓜”、“花”）中任意一个可见，都代表“芽”曾经出现过。
                everAppeared = HasBroadcastHistory("滩籽") || IsTargetObjectVisible("芽") || IsTargetObjectVisible("瓜") || IsTargetObjectVisible("花");
                break;
            case "花":
                // 花是通过"滩芽"互动产生的
                everAppeared = HasBroadcastHistory("滩芽") || IsTargetObjectVisible("花");
                break;
            case "瓜":
                // 瓜是由芽自动变化产生的，所以如果芽曾经出现过（通过滩籽互动），瓜就算出现过
                everAppeared = HasBroadcastHistory("滩籽") || IsTargetObjectVisible("瓜");
                break;
            default:
                // 其他对象使用当前可见性判断
                everAppeared = IsTargetObjectVisible(objectName);
                break;
        }
        
        GameLogger.LogDev($"对象是否曾出现过检查: {objectName} = {everAppeared}");
        return everAppeared;
    }


    
    
    // 检查广播历史中是否存在指定广播
    private bool HasBroadcastHistory(string broadcastMessage)
    {
        if (BroadcastManager.Instance != null)
        {
            return BroadcastManager.Instance.HasBroadcastHistory(broadcastMessage);
        }
        return false;
    }
    
    // 检查"芽"物体是否可见
    private bool IsYaObjectVisible()
    {
        Highlight[] allHighlights = FindObjectsOfType<Highlight>();
        foreach (Highlight highlight in allHighlights)
        {
            if (highlight != null && highlight.letter == "芽" && highlight.IsCollectableActive())
            {
                return true;
            }
        }
        return false;
    }

    // 检查场景中是否存在“无Highlight脚本的芽物体”，且其处于显示（activeInHierarchy 且 SpriteRenderer.enabled）
    private bool IsNonHighlightYaObjectVisible()
    {
        // 查找所有GameObject，过滤无Highlight组件者
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue;
            if (obj.GetComponent<Highlight>() != null) continue; // 跳过有Highlight的

            // 名称或自定义标识包含“芽”
            if (obj.name.Contains("芽"))
            {
                if (!obj.activeInHierarchy) continue;
                var sr = obj.GetComponent<SpriteRenderer>();
                if (sr == null || !sr.enabled) continue;
                return true;
            }
        }
        return false;
    }
    
    // 检查当前季节是否为春季
    private bool IsCurrentSeasonSpring()
    {
        Level3Manager level3Manager = FindObjectOfType<Level3Manager>();
        return level3Manager != null && level3Manager.IsSpring();
    }

    // 检查当前季节是否为夏季
    private bool IsCurrentSeasonSummer()
    {
        Level3Manager level3Manager = FindObjectOfType<Level3Manager>();
        return level3Manager != null && level3Manager.IsSummer();
    }
    
    // 检查是否存在未互动的目标
    private bool HasUninteractedTarget(string targetLetter)
    {
        GameLogger.LogDev($"HasUninteractedTarget: 检查目标 '{targetLetter}'");
        
        // 首先检查目标是否存在
        Highlight[] allHighlights = FindObjectsOfType<Highlight>();
        bool targetExists = false;
        
        GameLogger.LogDev($"找到 {allHighlights.Length} 个Highlight对象");
        foreach (Highlight highlight in allHighlights)
        {
            if (highlight != null)
            {
                GameLogger.LogDev($"Highlight: letter='{highlight.letter}', IsCollectableActive={highlight.IsCollectableActive()}");
                if (highlight.letter == targetLetter)
                {
                    targetExists = true;
                    GameLogger.LogDev($"找到匹配的目标: {targetLetter}");
                    break;
                }
            }
        }
        
        // 如果目标不存在，返回false
        if (!targetExists)
        {
            GameLogger.LogDev($"目标 '{targetLetter}' 不存在");
            return false;
        }
        
        // 检查是否已经互动过（通过广播历史判断）
        switch (targetLetter)
        {
            case "生":
                // 检查是否有"孟"广播（孟子会广播"孟"来点拨书生）
                bool shengInteracted = HasBroadcastHistory("孟");
                GameLogger.LogDev($"书生互动检查: 孟={shengInteracted}");
                return !shengInteracted;
            case "叶":
                // 检查是否有"蚜"广播（蚜虫会广播"蚜"来清理叶子）
                bool yeInteracted = HasBroadcastHistory("蚜");
                GameLogger.LogDev($"藤蔓互动检查: 蚜={yeInteracted}");
                return !yeInteracted;
            case "老":
                // 检查是否有"穿"广播（穿字符会广播"穿"来穿越时光）
                bool laoInteracted = HasBroadcastHistory("穿");
                GameLogger.LogDev($"老人互动检查: 穿={laoInteracted}");
                return !laoInteracted;
            default:
                // 其他目标暂时返回true
                GameLogger.LogDev($"未知目标 '{targetLetter}'，返回true");
                return true;
        }
    }

    // 是否仍有可收集的文字（基于Highlight的可收集状态）
    private bool AnyCollectableActiveInScene()
    {
        Highlight[] allHighlights = FindObjectsOfType<Highlight>();
        for (int i = 0; i < allHighlights.Length; i++)
        {
            Highlight h = allHighlights[i];
            if (h != null && h.IsCollectableActive())
            {
                return true;
            }
        }
        return false;
    }

    // 判断某化字形态对应的场景互动是否均完成
    private bool IsCarryFormInteractionsCompleted(string carry)
    {
        // PublicData.stringKeyValuePairs 定义了形态到目标的映射，如 仙->日、停->雨 等
        string target;
        if (!PublicData.stringKeyValuePairs.TryGetValue(carry, out target))
        {
            // 未定义映射则视为无需重置
            return false;
        }

        if (currentScene == SceneType.Level2)
        {
            return IsLevel2TargetCompleted(target);
        }
        else if (currentScene == SceneType.Level3)
        {
            return IsLevel3TargetCompleted(target);
        }
        else if (currentScene == SceneType.Level4)
        {
            return IsLevel4TargetCompleted(target);
        }

        return false;
    }

    // 判断Level2场景的目标是否完成
    private bool IsLevel2TargetCompleted(string target)
    {
        switch (target)
        {
            case "雨":
                return !IsObjectEnabled(rainObject);
            case "猎":
                return !IsObjectEnabled(hunterObject);
            case "孩":
                return !IsObjectEnabled(childObject);
            case "王":
                return !IsObjectEnabled(kingObject);
            case "日":
                return !IsAnyObjectEnabled(sunObjects);
            default:
                return false;
        }
    }

    // 判断Level3场景的目标是否完成
    private bool IsLevel3TargetCompleted(string target)
    {
        switch (target)
        {
            case "叶":
                return !IsObjectEnabled(leafObject);
            case "老":
                return !IsObjectEnabled(oldObject);
            case "生":
                return !IsObjectEnabled(lifeObject);
            default:
                return false;
        }
    }

    // 判断Level4场景的目标是否完成
    private bool IsLevel4TargetCompleted(string target)
    {
        // Level4的目标字符：桥、难、断、续
        // 这里可以根据具体的Level4目标实现逻辑
        // 目前暂时返回false，表示Level4的目标检查逻辑待实现
        switch (target)
        {
            case "桥":
            case "难":
            case "断":
            case "续":
                // 可以根据具体的Level4目标完成条件来实现
                return false;
            default:
                return false;
        }
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
    /// 获取当前玩家的初始携带字符
    /// </summary>
    /// <returns>当前玩家的初始携带字符</returns>
    private string GetCurrentPlayerInitialCarryCharacter()
    {
        if (playerController != null)
        {
            int currentPlayerIndex = playerController.GetCurrentPlayerIndex();
            return playerController.GetInitialCarryCharacter(currentPlayerIndex);
        }
        return "人"; // 默认值
    }

    /// <summary>
    /// 检查两个玩家中是否有任何一个的carryCharacter等于指定字符
    /// </summary>
    /// <param name="character">要检查的字符</param>
    /// <returns>是否有玩家携带该字符</returns>
    private bool HasPlayerWithCarryCharacter(string character)
    {
        if (playerController != null)
        {
            // 检查所有玩家
            for (int i = 0; i < playerController.GetPlayerCount(); i++)
            {
                Player player = playerController.GetPlayerByIndex(i);
                if (player != null && player.CarryCharacter == character)
                {
                    GameLogger.LogDev($"找到玩家 {i + 1} 携带字符 '{character}'");
                    return true;
                }
            }
        }
        GameLogger.LogDev($"没有玩家携带字符 '{character}'");
        return false;
    }

    #region Level4 提示检查方法

    /// <summary>
    /// 添加酒互动提示
    /// </summary>
    private void AddWineInteractionHints(List<string> hintPool, string carry)
    {
        // player和酒互动，场景中存在"骄"或"蝴"
        if (IsTargetObjectVisible("骄") || IsTargetObjectVisible("蝴"))
        {
            hintPool.Add("饮下雄黄酒，或可显露真身。");
            GameLogger.LogDev("添加酒互动提示: 雄黄酒");
        }
    }

    /// <summary>
    /// 添加蛇相关提示
    /// </summary>
    private void AddSnakeHints(List<string> hintPool, string carry)
    {
        if (carry == "蛇")
        {
            if (IsTargetObjectVisible("骄"))
            {
                hintPool.Add("「蛇」之本性，或可吞食骄马。");
                GameLogger.LogDev("添加蛇提示: 骄马");
            }
            
            if (IsTargetObjectVisible("蝴"))
            {
                hintPool.Add("「蛇」之本性，或可吞食虫蝶。");
                GameLogger.LogDev("添加蛇提示: 虫蝶");
            }
        }
    }

    /// <summary>
    /// 添加皇民相关提示
    /// </summary>
    private void AddEmperorHints(List<string> hintPool, string carry)
    {
        if (carry == "皇" && IsTargetObjectVisible("民"))
        {
            hintPool.Add("「皇」者临「民」，化为天之「骄」子。");
            GameLogger.LogDev("添加皇民提示");
        }
    }

    /// <summary>
    /// 添加清枯花相关提示
    /// </summary>
    private void AddQingKuhuaHints(List<string> hintPool, string carry)
    {
        if (carry == "清" && IsTargetObjectVisible("枯花"))
        {
            hintPool.Add("「清」泉或可让枯萎的花盛开。");
            GameLogger.LogDev("添加清枯花提示");
        }
    }

    /// <summary>
    /// 添加睛商相关提示
    /// </summary>
    private void AddJingShangHints(List<string> hintPool, string carry)
    {
        if (carry == "睛" && HasBroadcastHistory("商"))
        {
            hintPool.Add("以「睛」辨「玉」，方知其源。");
            GameLogger.LogDev("添加睛商提示");
        }
    }

    /// <summary>
    /// 添加柏鼠相关提示
    /// </summary>
    private void AddBaiShuHints(List<string> hintPool, string carry)
    {
        if (carry == "柏" && IsTargetObjectVisible("鼠"))
        {
            hintPool.Add("观「柏」间松鼠，其戏耍之绳或可一用。");
            GameLogger.LogDev("添加柏鼠提示");
        }
    }

    /// <summary>
    /// 添加帛商相关提示
    /// </summary>
    private void AddBoShangHints(List<string> hintPool, string carry)
    {
        if (carry == "帛" && !HasBroadcastHistory("商"))
        {
            hintPool.Add("以「帛」易物，乃商人之道。");
            GameLogger.LogDev("添加帛商提示");
        }
    }

    #endregion

    /// <summary>
    /// 更新提示池可视化显示
    /// </summary>
    /// <param name="hintPool">提示池列表</param>
    private void UpdateHintPoolDisplay(List<string> hintPool)
    {
        if (hintPool == null || hintPool.Count == 0)
        {
            hintPoolDisplay = "当前提示池为空";
            return;
        }
        
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"提示池内容 (共 {hintPool.Count} 条):");
        sb.AppendLine("========================");
        
        for (int i = 0; i < hintPool.Count; i++)
        {
            sb.AppendLine($"{i + 1}. {hintPool[i]}");
        }
        
        sb.AppendLine("========================");
        sb.AppendLine($"最后更新: {System.DateTime.Now:HH:mm:ss}");
        
        hintPoolDisplay = sb.ToString();
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
        
        // 清理自动隐藏计时器
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }
        
        isAnimating = false;
        isExpanded = false;
        isMouseOverHint = false;
        
        // 重置按钮底图为初始状态
        if (hintButton != null && hintButton.image != null && initialButtonSprite != null)
        {
            hintButton.image.sprite = initialButtonSprite;
        }
        
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
        
        // 清理自动隐藏协程
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
        }
        
        // 移除按钮事件监听
        if (hintButton != null)
        {
            hintButton.onClick.RemoveListener(OnHintButtonClicked);
        }
    }

}
