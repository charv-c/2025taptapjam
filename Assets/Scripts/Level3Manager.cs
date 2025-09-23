using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Level3场景的季节类型枚举
/// </summary>
public enum SeasonType
{
    Spring,  // 春季
    Summer   // 夏季
}

/// <summary>
/// Level3场景管理器 V2.0 (Bootstrap兼容版) - 管理季节切换和相关逻辑
/// 支持从任意场景启动，自动处理依赖初始化。
/// </summary>
public class Level3Manager : MonoBehaviour, IBootstrapAware
{
    [Header("季节设置")]
    [SerializeField] private SeasonType currentSeason = SeasonType.Spring;
    
    [Header("季节切换设置")]
    [SerializeField] private float seasonTransitionDuration = 1f;
    [SerializeField] private bool enableSeasonTransition = true;
    
    [Header("背景图片设置")]
    [Tooltip("左半边背景 - 春季")] [SerializeField] private Sprite leftSpringSprite;
    [Tooltip("右半边背景 - 春季")] [SerializeField] private Sprite rightSpringSprite;
    [Tooltip("左半边背景 - 夏季")] [SerializeField] private Sprite leftSummerSprite;
    [Tooltip("右半边背景 - 夏季")] [SerializeField] private Sprite rightSummerSprite;
    [Space(4)]
    [Tooltip("左半边背景对象（可为SpriteRenderer或Image）")] [SerializeField] private GameObject leftBackgroundObject;
    [Tooltip("右半边背景对象（可为SpriteRenderer或Image）")] [SerializeField] private GameObject rightBackgroundObject;
    
    [Header("调试设置")]
    [SerializeField] private bool showDebugInfo = true;
    
    [Header("收集设置")]
    [SerializeField] private List<string> collectedStrings = new List<string>();
    
    [Header("Level3彩蛋设置")]
    [SerializeField] private bool enableEasterEgg = true;
    [SerializeField] private bool showEasterEggInfo = true;
    
    [Header("特殊对象引用")]
    [SerializeField] private BeachObject beachObject; // 对滩涂对象的引用
    [SerializeField] private BackgroundManager backgroundManager; // 对背景管理器的引用
    [SerializeField] private SeasonParticleManager seasonParticleManager; // 对季节粒子管理器的引用
    
    [Header("开场白箭头设置")]
    [Tooltip("古琴对象的Transform引用，用于第三句开场白时箭头指向")]
    [SerializeField] private Transform guqinTransform; // 古琴对象
    [Tooltip("箭头Image组件")]
    [SerializeField] private Image arrowImage; // 箭头图像
    [Tooltip("箭头指向左方的Sprite")]
    [SerializeField] private Sprite arrowLeft; // 向左箭头
    [Tooltip("箭头指向左下方的Sprite")]
    [SerializeField] private Sprite arrowDownLeft; // 向左下箭头
    
    // 关卡流程控制
    private LevelManager levelManager;
    
    // Bootstrap状态
    private bool bootstrapCompleted = false;
    private bool sceneInitialized = false;

    // 关卡开场白文案
    private readonly string[] openingMessages =
    {
        "欢迎来到江城武汉，这里曾诞生过高山流水的佳话",
        "我们将在龟山汉水之间，感受伯牙与子期的知音故事",
        "古琴台上有把【解语琴】，似乎能读懂一些文字的“弦外之音”"
    };

    
    // 事件：季节切换时触发
    public System.Action<SeasonType> OnSeasonChanged;
    
    // 事件：收集到新字符串时触发
    public System.Action<string> OnStringCollected;
    // 简单的彩蛋状态跟踪
    private static bool easterEggTriggered = false;
    
    private void Start()
    {
        GameLogger.LogSystem("Level3Manager: 开始初始化level3场景");
        
        // 确保Bootstrap系统初始化
        GameBootstrap.EnsureInitialized();
        
        // 设置当前关卡进度
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.SetCurrentLevel("level3");
            GameLogger.LogSystem("Level3Manager: 已设置当前关卡为 level3");
        }
        
        // 初始化季节状态
        InitializeSeason();
        
        // 初始化彩蛋功能
        InitializeEasterEgg();
        
        // --- 关卡流程控制 ---
        levelManager = GetComponent<LevelManager>();
        if (levelManager != null)
        {
            levelManager.OnLevelCompleted += HandleLevelCompletion;
        }

        // 关卡开始时禁用操作，等待Bootstrap和开场白
        DisableAllOperations();
        
        // 开始初始化协程
        StartCoroutine(InitializeLevel3Coroutine());
    }

    private void OnDestroy()
    {
        if (levelManager != null)
        {
            levelManager.OnLevelCompleted -= HandleLevelCompletion;
        }
    }
    
    /// <summary>
    /// 初始化Level3的协程 - 等待Bootstrap完成后开始
    /// </summary>
    private IEnumerator InitializeLevel3Coroutine()
    {
        // 等待Bootstrap完成
        while (!GameBootstrap.IsInitialized)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        GameLogger.LogSystem("Level3Manager: Bootstrap完成，开始场景初始化");
        
        // 设置BGM
        SetupLevel3BGM();
        
        // 等待一帧确保所有系统就绪
        yield return null;
        
        // 标记Bootstrap完成
        bootstrapCompleted = true;
        
        // 开始场景内容
        InitializeSceneContent();
    }
    
    /// <summary>
    /// IBootstrapAware接口实现 - Bootstrap完成时调用
    /// </summary>
    public void OnBootstrapComplete()
    {
        bootstrapCompleted = true;
        GameLogger.LogSystem("Level3Manager: 收到Bootstrap完成通知");
        
        if (!sceneInitialized)
        {
            InitializeSceneContent();
        }
    }
    
    /// <summary>
    /// 设置Level3的BGM
    /// </summary>
    private void SetupLevel3BGM()
    {
        if (AudioManager.Instance != null)
        {
            if (AudioManager.Instance.bgmLevel3 != null)
            {
                AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmLevel3);
                
                if (showDebugInfo)
                {
                    GameLogger.LogDev("Level3Manager: 开始播放知音篇主题BGM");
                }
            }
            else
            {
                GameLogger.LogWarning("Level3Manager: bgmLevel3音频片段未设置");
            }
        }
        else
        {
            GameLogger.LogWarning("Level3Manager: AudioManager实例未找到，无法播放BGM");
        }
    }
    
    /// <summary>
    /// 初始化场景内容 - 显示开场白并开始游戏
    /// </summary>
    private void InitializeSceneContent()
    {
        if (sceneInitialized) return;
        
        sceneInitialized = true;
        GameLogger.LogSystem("Level3Manager: 开始显示开场白");
        
        // 显示开场白，结束后正式开始关卡
        if (InfoPopupManager.Instance != null)
        {
            InfoPopupManager.Instance.ShowPopup(openingMessages, () => {
                OnOpeningCompleted(); // 先处理开场白结束逻辑
                StartLevel(); // 再开始关卡
            }, OnOpeningMessageShown);
        }
        else
        {
            GameLogger.LogWarning("Level3Manager: InfoPopupManager仍然为null，直接开始关卡");
            StartLevel();
        }
    }

    /// <summary>
    /// 关卡正式开始（开场白结束后调用）
    /// </summary>
    private void StartLevel()
    {
        GameLogger.LogSystem("Level3Manager: 开场白结束，正式开始关卡。");
        // 强制启用玩家移动，避免被其他管理器在Start中覆盖
        StartCoroutine(EnsureEnableMovementNextFrame());
    }

    /// <summary>
    /// 处理关卡完成事件
    /// </summary>
    private void HandleLevelCompletion()
    {
        GameLogger.LogSystem("Level3Manager: 关卡完成，直接进入通关界面。");
        DisableAllOperations();

        // 直接完成关卡，跳过通关结语
        FinishLevel();
    }

    /// <summary>
    /// 关卡收尾，通知GameFlowManager
    /// </summary>
    private void FinishLevel()
    {
        GameLogger.LogSystem("Level3Manager: 开始关卡收尾流程");
        
        // 验证Bootstrap状态
        if (!GameBootstrap.IsInitialized)
        {
            GameLogger.LogWarning("Level3Manager: Bootstrap未完成初始化，尝试重新初始化");
            GameBootstrap.EnsureInitialized();
        }
        
        // 获取正确的场景名
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        GameLogger.LogSystem($"Level3Manager: 当前场景名: {currentSceneName}");
        
        if (GameFlowManager.Instance != null)
        {
            GameLogger.LogSystem("Level3Manager: GameFlowManager实例存在，调用CompleteLevel");
            GameFlowManager.Instance.CompleteLevel(currentSceneName);
        }
        else
        {
            GameLogger.LogError("Level3Manager: 未找到GameFlowManager实例，无法完成关卡。");
            GameLogger.LogError($"Level3Manager: Bootstrap状态: {GameBootstrap.IsInitialized}");
            GameLogger.LogError($"Level3Manager: AudioManager实例: {(AudioManager.Instance != null ? "存在" : "null")}");
            GameLogger.LogError($"Level3Manager: InfoPopupManager实例: {(InfoPopupManager.Instance != null ? "存在" : "null")}");
            
            // 尝试最后一次创建GameFlowManager
            GameLogger.LogSystem("Level3Manager: 尝试紧急创建GameFlowManager");
            GameObject emergencyGameFlow = new GameObject("GameFlowManager_Emergency");
            emergencyGameFlow.AddComponent<GameFlowManager>();
            DontDestroyOnLoad(emergencyGameFlow);
            
            if (GameFlowManager.Instance != null)
            {
                GameLogger.LogSystem("Level3Manager: 紧急创建成功，继续完成关卡");
                GameFlowManager.Instance.CompleteLevel(currentSceneName);
            }
            else
            {
                GameLogger.LogError("Level3Manager: 紧急创建也失败了！跳过场景切换。");
            }
        }
    }
    
    /// <summary>
    /// 开场白消息显示时的回调处理
    /// </summary>
    /// <param name="messageIndex">消息索引</param>
    /// <param name="message">消息内容</param>
    private void OnOpeningMessageShown(int messageIndex, string message)
    {
        GameLogger.LogSystem($"Level3Manager: 显示开场白第 {messageIndex + 1} 句: {message}");
        
        // 第三句话时显示箭头指向古琴
        if (messageIndex == 2) // 索引从0开始，第三句是索引2
        {
            ShowArrowPointingToGuqin();
        }
        else
        {
            // 其他消息时隐藏箭头
            HideArrow();
        }
    }
    
    /// <summary>
    /// 开场白结束时的处理（在StartLevel之前调用）
    /// </summary>
    private void OnOpeningCompleted()
    {
        // 确保在开场白结束后隐藏箭头
        HideArrow();
        GameLogger.LogSystem("Level3Manager: 开场白结束，已隐藏箭头");
    }
    
    /// <summary>
    /// 显示箭头指向古琴
    /// </summary>
    private void ShowArrowPointingToGuqin()
    {
        if (arrowImage == null || guqinTransform == null)
        {
            GameLogger.LogWarning("Level3Manager: arrowImage 或 guqinTransform 未设置，无法显示箭头");
            return;
        }
        
        GameLogger.LogSystem("Level3Manager: 显示箭头指向古琴");
        arrowImage.gameObject.SetActive(true);
        
        // 获取古琴的屏幕坐标
        Vector3 guqinScreenPos = Camera.main.WorldToScreenPoint(guqinTransform.position);
        
        // 计算箭头位置和角度（参考TutorialManager的逻辑）
        SetupArrowForWorldObject(guqinScreenPos);
    }
    
    /// <summary>
    /// 隐藏箭头
    /// </summary>
    private void HideArrow()
    {
        if (arrowImage != null)
        {
            arrowImage.gameObject.SetActive(false);
            GameLogger.LogSystem("Level3Manager: 已隐藏箭头");
        }
    }
    
    /// <summary>
    /// 为世界物体设置箭头位置和角度（参考TutorialManager实现）
    /// </summary>
    /// <param name="targetScreenPos">目标的屏幕坐标</param>
    private void SetupArrowForWorldObject(Vector3 targetScreenPos)
    {
        if (arrowImage == null)
        {
            GameLogger.LogWarning("Level3Manager: arrowImage 为空，无法设置箭头");
            return;
        }
        
        // 使用屏幕中心作为起点（简化处理）
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        
        // 计算方向向量
        Vector3 direction = (targetScreenPos - screenCenter).normalized;
        
        // 箭头偏移距离（按分辨率缩放，基于1080高度）
        float arrowOffset = GetScaledOffset(80f);
        Vector3 arrowScreenPos = targetScreenPos - direction * arrowOffset;
        
        // 计算角度
        Vector3 arrowToTarget = (targetScreenPos - arrowScreenPos).normalized;
        float angle = Mathf.Atan2(arrowToTarget.y, arrowToTarget.x) * Mathf.Rad2Deg;
        
        // 箭头素材是向左的，调整角度
        angle += 180f;
        
        // 根据角度选择合适的Sprite
        if (Mathf.Abs(angle % 360f) > 135f || Mathf.Abs(angle % 360f) < 45f)
        {
            if (arrowLeft != null) arrowImage.sprite = arrowLeft;
        }
        else
        {
            if (arrowDownLeft != null) arrowImage.sprite = arrowDownLeft;
        }
        
        // 转换屏幕坐标到UI坐标并设置箭头位置
        if (TrySetArrowAnchoredFromScreen(arrowScreenPos, angle))
        {
            GameLogger.LogSystem($"Level3Manager: 箭头设置成功 - 屏幕坐标: {arrowScreenPos}, 角度: {angle}");
        }
        else
        {
            GameLogger.LogWarning("Level3Manager: 箭头坐标转换失败");
        }
    }
    
    /// <summary>
    /// 将屏幕坐标转换为箭头所在Canvas的局部坐标并设置位置
    /// </summary>
    /// <param name="screenPos">屏幕坐标</param>
    /// <param name="angle">箭头角度</param>
    /// <returns>是否成功设置位置</returns>
    private bool TrySetArrowAnchoredFromScreen(Vector3 screenPos, float angle)
    {
        if (arrowImage == null || arrowImage.canvas == null) 
        {
            GameLogger.LogWarning("Level3Manager: arrowImage 或 canvas 为空");
            return false;
        }

        var canvas = arrowImage.canvas;
        RectTransform canvasRect = canvas.transform as RectTransform;
        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCam, out Vector2 localPos))
        {
            RectTransform arrowRect = arrowImage.rectTransform;
            arrowRect.anchoredPosition = localPos;
            arrowRect.localRotation = Quaternion.Euler(0f, 0f, angle);
            arrowRect.localScale = Vector3.one;

            // 防止Z偏移
            Vector3 lp = arrowRect.localPosition;
            arrowRect.localPosition = new Vector3(lp.x, lp.y, 0f);
            
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// 将以1080高度为基准的偏移按当前屏幕高度缩放
    /// </summary>
    /// <param name="referenceOffset">在1920x1080下的参考偏移量</param>
    /// <returns>缩放后的偏移量</returns>
    private float GetScaledOffset(float referenceOffset)
    {
        float scale = Screen.height > 0 ? (float)Screen.height / 1080f : 1f;
        return referenceOffset * scale;
    }

    private System.Collections.IEnumerator EnsureEnableMovementNextFrame()
    {
        yield return new WaitForEndOfFrame();
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.EnableCurrentPlayerMovement();
            // 同时确保所有玩家的回车键与输入开启
            int count = playerController.GetPlayerCount();
            for (int i = 0; i < count; i++)
            {
                Player p = playerController.GetPlayerByIndex(i);
                if (p != null)
                {
                    p.SetInputEnabled(true);
                    p.SetEnterKeyEnabled(true);
                }
            }

            // 参照Level2：设置当前玩家索引、启用切换与颜色更新
            if (playerController.GetPlayerCount() > 0)
            {
                playerController.SetCurrentPlayerIndex(0);
            }
            playerController.EnablePlayerSwitching();
            playerController.UpdatePlayerColors();

            if (showDebugInfo)
            {
                GameLogger.LogDev("Level3Manager: 已启用移动/输入，并更新玩家颜色与切换状态");
            }
        }
        else
        {
            GameLogger.LogWarning("Level3Manager: 未找到PlayerController，无法启用玩家移动");
        }
    }
    
    /// <summary>
    /// 禁用所有玩家操作
    /// </summary>
    private void DisableAllOperations()
    {
        GameLogger.LogSystem("Level3Manager: 禁用所有操作。");
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
        }
        else
        {
            GameLogger.LogWarning("Level3Manager: PlayerController为null，无法禁用操作。");
        }
    }
    
    /// <summary>
    /// 初始化季节状态
    /// </summary>
    private void InitializeSeason()
    {
        // 如果有存档，避免用默认季节与背景覆盖恢复结果
        if (GameStateManager.Instance != null && GameStateManager.Instance.HasSavedStateForActiveScene())
        {
            GameLogger.LogDev("Level3Manager: 检测到当前场景存在存档，跳过InitializeSeason对季节与背景的覆盖");
            return;
        }
        // 无存档时，根据当前季节设置场景状态
        ApplySeasonEffects(currentSeason);
    }
    
    /// <summary>
    /// 切换到指定季节
    /// </summary>
    /// <param name="newSeason">目标季节</param>
    public void SwitchToSeason(SeasonType newSeason)
    {
        if (currentSeason == newSeason)
        {
            if (showDebugInfo)
            {
                GameLogger.LogDev($"Level3Manager: 已经是{newSeason}季节，无需切换");
            }
            return;
        }
        
        SeasonType previousSeason = currentSeason;
        currentSeason = newSeason;
        
        if (showDebugInfo)
        {
            GameLogger.LogDev($"Level3Manager: 季节切换 {previousSeason} -> {currentSeason}");
        }

        // 立即更新左右背景，避免过渡期间或其他管理器覆盖导致不同步
        SetBackgroundSpritesForSeason(currentSeason);
        
        // 应用季节效果
        if (enableSeasonTransition)
        {
            StartCoroutine(SeasonTransitionCoroutine(previousSeason, currentSeason));
        }
        else
        {
            ApplySeasonEffects(currentSeason);
        }
        
        // 触发季节切换事件
        OnSeasonChanged?.Invoke(currentSeason);

        // 广播季节切换（用于让“芽”->“瓜”等联动）
        if (BroadcastManager.Instance != null && previousSeason == SeasonType.Spring && currentSeason == SeasonType.Summer)
        {
            BroadcastManager.Instance.BroadcastToAll("季夏");
            if (showDebugInfo)
            {
                GameLogger.LogDev("Level3Manager: 已广播'季夏'以触发季节相关联动");
            }
        }
    }
    
    /// <summary>
    /// 切换到春季
    /// </summary>
    public void SwitchToSpring()
    {
        SwitchToSeason(SeasonType.Spring);
    }
    
    /// <summary>
    /// 切换到夏季
    /// </summary>
    public void SwitchToSummer()
    {
        SwitchToSeason(SeasonType.Summer);
    }
    
    /// <summary>
    /// 在春季和夏季之间切换
    /// </summary>
    public void ToggleSeason()
    {
        SeasonType originalSeason = currentSeason;
        SeasonType targetSeason = (currentSeason == SeasonType.Spring) ? SeasonType.Summer : SeasonType.Spring;
        
        // 使用统一入口，确保应用季节效果与事件/过渡
        SwitchToSeason(targetSeason);

        // 季节切换后，检查是否需要将"芽"变为"瓜"
        if (originalSeason == SeasonType.Spring && targetSeason == SeasonType.Summer)
        {
            if (beachObject != null)
            {
                // BeachObject内部会检查芽是否真正被种下且显示，这里直接调用
                beachObject.TransformYaToGuaOnSeasonChange();
                if (showDebugInfo)
                {
                    GameLogger.LogDev("Level3Manager: 已调用BeachObject的芽变瓜逻辑");
                }
            }
            else
            {
                GameLogger.LogWarning("Level3Manager: BeachObject引用未设置，无法执行芽变瓜的逻辑。");
            }
        }
    }
    
    /// <summary>
    /// 季节切换协程
    /// </summary>
    private System.Collections.IEnumerator SeasonTransitionCoroutine(SeasonType fromSeason, SeasonType toSeason)
    {
        if (showDebugInfo)
        {
            GameLogger.LogDev($"Level3Manager: 开始季节切换动画 {fromSeason} -> {toSeason}");
        }
        
        // 这里可以添加季节切换的动画效果
        // 例如：淡入淡出、颜色变化等
        
        float elapsedTime = 0f;
        while (elapsedTime < seasonTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / seasonTransitionDuration;
            
            // 可以在这里添加过渡效果
            // 例如：插值颜色、透明度等
            
            yield return null;
        }
        
        // 应用最终季节效果
        ApplySeasonEffects(toSeason);
        
        if (showDebugInfo)
        {
            GameLogger.LogDev($"Level3Manager: 季节切换完成 {toSeason}");
        }
    }
    
    /// <summary>
    /// 应用季节效果
    /// </summary>
    /// <param name="season">要应用的季节</param>
    private void ApplySeasonEffects(SeasonType season)
    {
        switch (season)
        {
            case SeasonType.Spring:
                ApplySpringEffects();
                break;
            case SeasonType.Summer:
                ApplySummerEffects();
                break;
        }
    }
    
    /// <summary>
    /// 应用春季效果
    /// </summary>
    private void ApplySpringEffects()
    {
        if (showDebugInfo)
        {
            GameLogger.LogDev("Level3Manager: 应用春季效果");
        }
        
        // 春季效果实现
        // 例如：改变背景、调整光照、显示春季元素等
        // 不再显隐物体；仅保留季节状态
        SetBackgroundSpritesForSeason(SeasonType.Spring);
    }
    
    /// <summary>
    /// 应用夏季效果
    /// </summary>
    private void ApplySummerEffects()
    {
        if (showDebugInfo)
        {
            GameLogger.LogDev("Level3Manager: 应用夏季效果");
        }
        
        // 夏季效果实现
        // 例如：改变背景、调整光照、显示夏季元素等
        // 不再显隐物体；仅保留季节状态
        SetBackgroundSpritesForSeason(SeasonType.Summer);
    }
    
    /// <summary>
    /// 启用指定季节的对象
    /// </summary>
    /// <param name="seasonTag">季节标签</param>
    private void EnableSeasonObjects(string seasonTag)
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains(seasonTag))
            {
                obj.SetActive(true);
                
                // 如果有SpriteRenderer，确保启用
                SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = true;
                }
                
                if (showDebugInfo)
                {
                    GameLogger.LogDev($"Level3Manager: 启用{seasonTag}对象: {obj.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// 禁用指定季节的对象
    /// </summary>
    /// <param name="seasonTag">季节标签</param>
    private void DisableSeasonObjects(string seasonTag)
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains(seasonTag))
            {
                obj.SetActive(false);
                
                if (showDebugInfo)
                {
                    GameLogger.LogDev($"Level3Manager: 禁用{seasonTag}对象: {obj.name}");
                }
            }
        }
    }
    
    /// <summary>
    /// 获取当前季节
    /// </summary>
    /// <returns>当前季节</returns>
    public SeasonType GetCurrentSeason()
    {
        return currentSeason;
    }
    
    /// <summary>
    /// 设置当前季节（不触发切换效果）
    /// </summary>
    /// <param name="season">季节</param>
    public void SetCurrentSeason(SeasonType season)
    {
        currentSeason = season;
        ApplySeasonEffects(currentSeason);
        
        if (showDebugInfo)
        {
            GameLogger.LogDev($"Level3Manager: 直接设置季节为: {currentSeason}");
        }
    }
    
    /// <summary>
    /// 检查是否为指定季节
    /// </summary>
    /// <param name="season">要检查的季节</param>
    /// <returns>是否为指定季节</returns>
    public bool IsSeason(SeasonType season)
    {
        return currentSeason == season;
    }
    
    /// <summary>
    /// 检查是否为春季
    /// </summary>
    /// <returns>是否为春季</returns>
    public bool IsSpring()
    {
        return currentSeason == SeasonType.Spring;
    }
    
    /// <summary>
    /// 检查是否为夏季
    /// </summary>
    /// <returns>是否为夏季</returns>
    public bool IsSummer()
    {
        return currentSeason == SeasonType.Summer;
    }
    
    #region Level3 彩蛋功能
    
    /// <summary>
    /// 初始化彩蛋功能
    /// </summary>
    private void InitializeEasterEgg()
    {
        if (!enableEasterEgg)
        {
            if (showEasterEggInfo)
            {
                GameLogger.LogDev("Level3Manager: 彩蛋功能已禁用");
            }
            return;
        }
        
        // 重置彩蛋状态
        easterEggTriggered = false;
        
        if (showEasterEggInfo)
        {
            GameLogger.LogDev("Level3Manager: 彩蛋功能初始化完成，等待广播消息");
        }
    }
    
    /// <summary>
    /// 处理彩蛋触发逻辑
    /// </summary>
    private void HandleEasterEggTriggered()
    {
        easterEggTriggered = true;
        
        if (showEasterEggInfo)
        {
            GameLogger.LogDev("Level3Manager: 彩蛋已触发！玩家发现了隐藏的'王'字彩蛋");
        }
        
        // 播放特殊音效（如果有的话）
        if (AudioManager.Instance != null && AudioManager.Instance.sfxEasterEgg != null)
        {
            // 播放彩蛋音效
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxEasterEgg);
            GameLogger.LogDev("Level3Manager: 播放彩蛋音效");
        }
        else if (AudioManager.Instance != null)
        {
            GameLogger.LogWarning("Level3Manager: 彩蛋音效未配置");
        }
    }
    
    /// <summary>
    /// 检查彩蛋是否已触发
    /// </summary>
    /// <returns>彩蛋是否已触发</returns>
    public bool IsEasterEggTriggered()
    {
        return easterEggTriggered;
    }
    
    /// <summary>
    /// 重置彩蛋状态
    /// </summary>
    public void ResetEasterEgg()
    {
        easterEggTriggered = false;
        if (showEasterEggInfo)
        {
            GameLogger.LogDev("Level3Manager: 彩蛋状态已重置");
        }
    }
    
    /// <summary>
    /// 设置彩蛋启用状态
    /// </summary>
    /// <param name="enabled">是否启用彩蛋</param>
    public void SetEasterEggEnabled(bool enabled)
    {
        enableEasterEgg = enabled;
        if (showEasterEggInfo)
        {
            GameLogger.LogDev($"Level3Manager: 彩蛋功能已{(enabled ? "启用" : "禁用")}");
        }
    }
    
    #endregion
    
    // 调试方法：在Inspector中调用
    [ContextMenu("切换到春季")]
    public void DebugSwitchToSpring()
    {
        SwitchToSpring();
    }
    
    [ContextMenu("切换到夏季")]
    public void DebugSwitchToSummer()
    {
        SwitchToSummer();
    }
    
    [ContextMenu("切换季节")]
    public void DebugToggleSeason()
    {
        ToggleSeason();
    }
    
    [ContextMenu("触发彩蛋测试")]
    public void DebugTriggerEasterEgg()
    {
        if (enableEasterEgg)
        {
            HandleEasterEggTriggered();
        }
        else
        {
            GameLogger.LogDev("Level3Manager: 彩蛋功能未启用，无法测试");
        }
    }
    
    [ContextMenu("重置彩蛋状态")]
    public void DebugResetEasterEgg()
    {
        ResetEasterEgg();
    }
    
    /// <summary>
    /// 添加字符串到可用字符串列表
    /// </summary>
    /// <param name="value">要添加的字符串</param>
    private void AddStringToAvailableList(string value)
    {
        if (string.IsNullOrEmpty(value)) return;
        
        // 查找StringSelector并添加字符串
        StringSelector stringSelector = FindObjectOfType<StringSelector>();
        if (stringSelector != null)
        {
            stringSelector.AddAvailableString(value);
            
            if (showDebugInfo)
            {
                GameLogger.LogDev($"Level3Manager: 已添加字符串 '{value}' 到可用列表");
            }
            
            // 播放取字音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxAcquire);
            }
            
            // 飞行动画由ButtonController自动处理，这里不需要重复实现
            if (showDebugInfo)
            {
                GameLogger.LogDev($"Level3Manager: 已添加字符 '{value}' 到可用列表，飞行动画由ButtonController处理");
            }
        }
        else
        {
            GameLogger.LogWarning($"Level3Manager: 未找到StringSelector，无法添加字符串 '{value}'");
        }
    }
    
    
    /// <summary>
    /// 根据letter删除所有对应的Highlight脚本
    /// </summary>
    /// <param name="letter">要删除的letter值</param>
    private void RemoveHighlightsByLetter(string letter)
    {
        if (string.IsNullOrEmpty(letter)) return;
        
        // 查找所有Highlight组件
        Highlight[] allHighlights = FindObjectsOfType<Highlight>(true);
        int removedCount = 0;
        
        foreach (var highlight in allHighlights)
        {
            if (highlight != null && highlight.letter == letter)
            {
                UnityEngine.Object.Destroy(highlight);
                removedCount++;
            }
        }
        
        if (showDebugInfo)
        {
            GameLogger.LogDev($"Level3Manager: 已从 {removedCount} 个对象上移除 Highlight（letter=='{letter}'）");
        }
    }
    
    // ============= 收集字符串管理 =============
    
    /// <summary>
    /// 添加收集到的字符串
    /// </summary>
    /// <param name="value">要添加的字符串</param>
    public void AddCollectedString(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        
        if (collectedStrings == null) 
            collectedStrings = new List<string>();
            
        if (!collectedStrings.Contains(value))
        {
            collectedStrings.Add(value);
            
            if (showDebugInfo)
            {
                GameLogger.LogDev($"Level3Manager: 收集到新字符串 '{value}'，当前总数: {collectedStrings.Count}");
            }
            
            // 触发收集事件
            OnStringCollected?.Invoke(value);
        }
        else
        {
            if (showDebugInfo)
            {
                GameLogger.LogDev($"Level3Manager: 字符串 '{value}' 已存在，跳过添加");
            }
        }
    }
    
    /// <summary>
    /// 检查是否已收集指定字符串
    /// </summary>
    /// <param name="value">要检查的字符串</param>
    /// <returns>是否已收集</returns>
    public bool HasCollectedString(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        return collectedStrings != null && collectedStrings.Contains(value);
    }
    
    /// <summary>
    /// 获取已收集的字符串列表（只读）
    /// </summary>
    /// <returns>已收集的字符串列表</returns>
    public IReadOnlyList<string> GetCollectedStrings()
    {
        if (collectedStrings == null) return System.Array.Empty<string>();
        return collectedStrings.AsReadOnly();
    }
    
    /// <summary>
    /// 获取已收集的字符串数量
    /// </summary>
    /// <returns>已收集的字符串数量</returns>
    public int GetCollectedCount()
    {
        return collectedStrings?.Count ?? 0;
    }
    
    /// <summary>
    /// 清空已收集的字符串列表
    /// </summary>
    public void ClearCollectedStrings()
    {
        if (collectedStrings != null)
        {
            int count = collectedStrings.Count;
            collectedStrings.Clear();
            
            if (showDebugInfo)
            {
                GameLogger.LogDev($"Level3Manager: 清空了 {count} 个已收集的字符串");
            }
        }
    }
    
    /// <summary>
    /// 检查是否已收集所有目标字符串
    /// </summary>
    /// <param name="targetStrings">目标字符串列表</param>
    /// <returns>是否已收集所有目标</returns>
    public bool HasCollectedAllTargets(List<string> targetStrings)
    {
        if (targetStrings == null || targetStrings.Count == 0) return true;
        if (collectedStrings == null) return false;
        
        foreach (string target in targetStrings)
        {
            if (!collectedStrings.Contains(target))
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 获取未收集的目标字符串
    /// </summary>
    /// <param name="targetStrings">目标字符串列表</param>
    /// <returns>未收集的字符串列表</returns>
    public List<string> GetUncollectedTargets(List<string> targetStrings)
    {
        List<string> uncollected = new List<string>();
        
        if (targetStrings == null || targetStrings.Count == 0) return uncollected;
        if (collectedStrings == null)
        {
            uncollected.AddRange(targetStrings);
            return uncollected;
        }
        
        foreach (string target in targetStrings)
        {
            if (!collectedStrings.Contains(target))
            {
                uncollected.Add(target);
            }
        }
        
        return uncollected;
    }
    
    // 调试方法：在Inspector中调用
    [ContextMenu("清空收集列表")]
    public void DebugClearCollectedStrings()
    {
        ClearCollectedStrings();
    }
    
    [ContextMenu("显示收集列表")]
    public void DebugShowCollectedStrings()
    {
        if (collectedStrings == null || collectedStrings.Count == 0)
        {
            GameLogger.LogDev("Level3Manager: 收集列表为空");
        }
        else
        {
            GameLogger.LogDev($"Level3Manager: 已收集 {collectedStrings.Count} 个字符串: [{string.Join(", ", collectedStrings)}]");
        }
    }

    // ===== 广播接收 =====
    public void ReceiveBroadcast(string broadcastedValue)
    {
        if (string.IsNullOrEmpty(broadcastedValue)) return;
        if (showDebugInfo)
        {
            GameLogger.LogDev($"Level3Manager: 收到广播 '{broadcastedValue}'");
        }

        // 收到"琴季"时，切换季节和背景
        if (broadcastedValue == "琴季")
        {
            if (showDebugInfo)
            {
                GameLogger.LogDev("Level3Manager: 收到'琴季'广播，执行季节和背景切换");
            }
            
            // 切换季节
            ToggleSeason();
            
            // 切换背景
            if (backgroundManager != null)
            {
                backgroundManager.SwitchBackground();
                if (showDebugInfo)
                {
                    GameLogger.LogDev("Level3Manager: 已切换背景");
                }

                // 防止BackgroundManager内部替换导致左右图片不同步，强制同步一次
                SetBackgroundSpritesForSeason(currentSeason);
            }
            else
            {
                GameLogger.LogWarning("Level3Manager: 未找到BackgroundManager，无法切换背景");
            }

            // 触发当前季节对应的粒子效果
            TriggerSeasonParticles();
        }
        // 收到"琴雅"时，获得"俗"字并删除"隹"对象
        else if (broadcastedValue == "琴雅")
        {
            if (showDebugInfo)
            {
                GameLogger.LogDev("Level3Manager: 收到'琴雅'广播，获得'俗'字");
            }
            
            // 1) 添加"俗"字到可用字符串列表
            AddStringToAvailableList("俗");
            
            // 2) 删除所有letter=="隹"的对象上的Highlight脚本
            RemoveHighlightsByLetter("隹");
        }
        // 收到"琴孤"时，获得"欣"字并删除"瓜"对象
        else if (broadcastedValue == "琴孤")
        {
            if (showDebugInfo)
            {
                GameLogger.LogDev("Level3Manager: 收到'琴孤'广播，获得'欣'字");
            }
            
            // 1) 添加"欣"字到可用字符串列表
            AddStringToAvailableList("欣");
            
            // 2) 删除所有letter=="瓜"的对象上的Highlight脚本
            RemoveHighlightsByLetter("瓜");
        }
        
        // 处理彩蛋广播
        if (enableEasterEgg && broadcastedValue == "拼一土" && !easterEggTriggered)
        {
            HandleEasterEggTriggered();
        }
    }

    /// <summary>
    /// 根据季节设置左右两侧背景图片
    /// </summary>
    /// <param name="season">季节</param>
    private void SetBackgroundSpritesForSeason(SeasonType season)
    {
        Sprite targetLeft = null;
        Sprite targetRight = null;
        switch (season)
        {
            case SeasonType.Spring:
                targetLeft = leftSpringSprite;
                targetRight = rightSpringSprite;
                break;
            case SeasonType.Summer:
                targetLeft = leftSummerSprite;
                targetRight = rightSummerSprite;
                break;
        }

        // 左侧
        if (leftBackgroundObject != null)
        {
            if (!TryApplySpriteToObject(leftBackgroundObject, targetLeft) && showDebugInfo)
            {
                GameLogger.LogWarning("Level3Manager: 左侧背景对象未找到SpriteRenderer或Image组件");
            }
        }
        else if (showDebugInfo)
        {
            GameLogger.LogWarning("Level3Manager: 左侧背景对象未设置");
        }

        // 右侧
        if (rightBackgroundObject != null)
        {
            if (!TryApplySpriteToObject(rightBackgroundObject, targetRight) && showDebugInfo)
            {
                GameLogger.LogWarning("Level3Manager: 右侧背景对象未找到SpriteRenderer或Image组件");
            }
        }
        else if (showDebugInfo)
        {
            GameLogger.LogWarning("Level3Manager: 右侧背景对象未设置");
        }
    }

    /// <summary>
    /// 尝试将Sprite应用到指定对象（支持SpriteRenderer或UI Image）
    /// </summary>
    /// <param name="targetObject">目标对象</param>
    /// <param name="sprite">要设置的图片</param>
    /// <returns>是否成功设置</returns>
    private bool TryApplySpriteToObject(GameObject targetObject, Sprite sprite)
    {
        if (targetObject == null) return false;

        var sr = targetObject.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = sprite;
            return true;
        }

        var img = targetObject.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = sprite;
            img.SetNativeSize();
            return true;
        }

        return false;
    }

    /// <summary>
    /// 触发与当前季节匹配的粒子效果
    /// </summary>
    private void TriggerSeasonParticles()
    {
        if (seasonParticleManager == null)
        {
            if (showDebugInfo)
            {
                GameLogger.LogWarning("Level3Manager: 未设置SeasonParticleManager，无法触发粒子效果");
            }
            return;
        }

        // 直接触发对应季节播放（SeasonParticleManager内部也会在季节切换事件中自动播放，这里做显式触发确保收到广播时一定播）
        if (currentSeason == SeasonType.Spring)
        {
            seasonParticleManager.ForcePlaySpringParticles();
        }
        else if (currentSeason == SeasonType.Summer)
        {
            seasonParticleManager.ForcePlaySummerParticles();
        }
    }
}
