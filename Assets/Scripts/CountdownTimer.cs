using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 倒计时组件 - 独立的倒计时功能
/// 可以附加到任何GameObject上，提供倒计时显示功能
/// </summary>
public class CountdownTimer : MonoBehaviour
{
    [Header("倒计时设置")]
    [SerializeField] private float countdownDuration = 10f; // 倒计时持续时间（秒）
    [SerializeField] private bool enableCountdownLogging = true; // 是否启用倒计时日志
    [SerializeField] private float countdownUIHeight; // 倒计时UI在对象头顶的高度
    [SerializeField] private int countdownFontSize = 12; // 倒计时字体大小
    [SerializeField] private Color countdownTextColor = Color.red; // 倒计时文字颜色
    
    [Header("UI设置")]
    [SerializeField] private bool autoCreateUI = true; // 是否自动创建UI
    [SerializeField] private bool followTarget = true; // 是否跟随目标对象移动
    
    [Header("蛇状态Sprite设置")]
    [SerializeField] private Sprite snakeSprite; // 蛇状态时的sprite
    
    // 倒计时相关变量
    private bool isCountdownActive = false;
    private float countdownTimer = 0f;
    private GameObject countdownUI; // 倒计时UI对象
    private TextMeshProUGUI countdownText; // 倒计时文本组件
    private Canvas countdownCanvas; // 倒计时Canvas
    private Camera mainCamera; // 主摄像机引用
    
    // 目标对象引用（用于跟随移动）
    private Transform targetTransform;
    
    // 暂停相关变量
    private bool isPaused = false;
    private float pausedTime = 0f; // 暂停时的时间
    
    // Player组件引用和sprite管理
    private Player playerComponent;
    private SpriteRenderer playerSpriteRenderer;
    private Sprite originalPlayerSprite; // 存储原始sprite
    private bool isSnakeState = false; // 当前是否为蛇状态
    
    // 音效播放标志
    private bool hasPlayedFiveSecondSound = false; // 是否已播放5秒倒计时音效
    private bool hasPlayedStartSound = false; // 是否已播放开始倒计时音效
    private float xionghuangSoundDuration = 2f; // 雄黄酒音效持续时间（预估）
    
    void Start()
    {
        // 获取主摄像机
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameLogger.LogWarning($"CountdownTimer: 无法找到主摄像机 - {gameObject.name}");
        }
        
        // 设置目标对象为当前对象
        targetTransform = transform;
        
        // 初始化Player组件引用
        InitializePlayerReference();
        
        // 如果启用自动创建UI，则初始化
        if (autoCreateUI)
        {
            InitializeCountdownUI();
        }
        
        // 检查初始倒计时状态，如果为0则隐藏
        CheckInitialCountdownState();
    }
    
    void Update()
    {
        // 检查退出弹窗状态并暂停倒计时
        CheckExitDialogState();
        
        // 检查Player状态变化并切换sprite
        CheckPlayerStateAndUpdateSprite();
        
        // 更新倒计时
        UpdateCountdown();
    }
    
    /// <summary>
    /// 初始化Player组件引用
    /// </summary>
    private void InitializePlayerReference()
    {
        // 获取Player组件
        playerComponent = GetComponent<Player>();
        if (playerComponent == null)
        {
            // 如果当前对象没有Player组件，尝试在父对象中查找
            playerComponent = GetComponentInParent<Player>();
        }
        
        if (playerComponent != null)
        {
            // 获取Player的SpriteRenderer组件
            playerSpriteRenderer = playerComponent.GetComponent<SpriteRenderer>();
            if (playerSpriteRenderer != null)
            {
                // 保存原始sprite
                originalPlayerSprite = playerSpriteRenderer.sprite;
                
                if (enableCountdownLogging)
                {
                    GameLogger.LogDev($"CountdownTimer: 成功初始化Player引用 - {gameObject.name}");
                }
            }
            else
            {
                GameLogger.LogWarning($"CountdownTimer: Player组件没有SpriteRenderer - {gameObject.name}");
            }
        }
        else
        {
            GameLogger.LogWarning($"CountdownTimer: 未找到Player组件 - {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 初始化倒计时UI
    /// </summary>
    private void InitializeCountdownUI()
    {
        // 自动创建倒计时UI
        CreateCountdownUI();
        
        if (enableCountdownLogging)
        {
            GameLogger.LogDev($"CountdownTimer: 倒计时UI初始化完成 - {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 创建倒计时UI
    /// </summary>
    private void CreateCountdownUI()
    {
        // 创建Canvas
        GameObject canvasObj = new GameObject("CountdownCanvas");
        canvasObj.transform.SetParent(transform);
        countdownCanvas = canvasObj.AddComponent<Canvas>();
        countdownCanvas.renderMode = RenderMode.WorldSpace;
        countdownCanvas.sortingOrder = 5; // 确保在退出弹窗(22)和InfoPopup(10)之下
        
        // 绑定Camera到Canvas（修复倒计时显示问题）
        if (mainCamera != null)
        {
            countdownCanvas.worldCamera = mainCamera;
        }
        else
        {
            // 如果mainCamera为空，尝试获取主摄像机
            Camera camera = Camera.main;
            if (camera != null)
            {
                countdownCanvas.worldCamera = camera;
            }
            else
            {
                GameLogger.LogWarning($"CountdownTimer: 无法找到主摄像机，倒计时可能无法正常显示 - {gameObject.name}");
            }
        }
        
        // 设置Canvas位置（目标对象头顶）
        Vector3 canvasPosition = targetTransform.position + Vector3.up * countdownUIHeight;
        canvasObj.transform.position = canvasPosition;
        
        // 设置Canvas缩放
        canvasObj.transform.localScale = new Vector3(0.015f, 0.015f, 1f);
        
        // 创建CanvasScaler组件
        CanvasScaler canvasScaler = canvasObj.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        
        // 创建GraphicRaycaster组件
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // 创建倒计时文本对象
        countdownUI = new GameObject("CountdownText");
        countdownUI.transform.SetParent(canvasObj.transform, false);
        
        // 添加TextMeshPro组件
        countdownText = countdownUI.AddComponent<TextMeshProUGUI>();
        countdownText.text = "0";
        countdownText.font = LoadNumberFont();
        countdownText.fontSize = countdownFontSize;
        countdownText.color = countdownTextColor;
        countdownText.alignment = TextAlignmentOptions.Center;
        
        // 设置文本位置和大小
        RectTransform textRect = countdownText.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(100, 50);
        textRect.anchoredPosition = Vector2.zero;
        
        // 初始时隐藏倒计时UI
        countdownUI.SetActive(false);
        
        if (enableCountdownLogging)
        {
            GameLogger.LogDev($"CountdownTimer: 自动创建倒计时UI完成 - {gameObject.name}");
            GameLogger.LogDev($"CountdownTimer: Canvas renderMode={countdownCanvas.renderMode}, worldCamera={countdownCanvas.worldCamera?.name}");
        }
    }
    
    /// <summary>
    /// 更新倒计时
    /// </summary>
    private void UpdateCountdown()
    {
        // 检查倒计时是否为0，如果是则隐藏UI
        if (countdownTimer <= 0f && isCountdownActive)
        {
            OnCountdownFinished();
            return;
        }
        
        if (isCountdownActive && !isPaused)
        {
            countdownTimer -= Time.deltaTime;
            
            // 更新倒计时文本显示
            if (countdownText != null)
            {
                countdownText.text = Mathf.Ceil(countdownTimer).ToString();
            }
            
            // 播放5秒倒计时音效（只播放一次）
            if (countdownTimer <= 5f && countdownTimer > 4.9f && !hasPlayedFiveSecondSound)
            {
                hasPlayedFiveSecondSound = true;
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayClock();
                    if (enableCountdownLogging)
                    {
                        GameLogger.LogDev($"CountdownTimer: 播放5秒倒计时音效 - {gameObject.name}");
                    }
                }
            }
            
            // 更新倒计时UI位置，跟随目标对象移动
            if (followTarget && countdownCanvas != null && targetTransform != null)
            {
                Vector3 canvasPosition = targetTransform.position + Vector3.up * countdownUIHeight;
                countdownCanvas.transform.position = canvasPosition;
            }
            
            // 检查倒计时是否结束
            if (countdownTimer <= 0f)
            {
                OnCountdownFinished();
            }
        }
    }
    
    /// <summary>
    /// 开始倒计时
    /// </summary>
    /// <param name="duration">倒计时持续时间（秒），如果为-1则使用默认时间</param>
    public void StartCountdown(float duration = -1f)
    {
        // 如果传入了自定义时间，使用自定义时间；否则使用默认时间
        float actualDuration = duration > 0f ? duration : countdownDuration;
        
        countdownTimer = actualDuration;
        isCountdownActive = true;
        
        // 重置音效播放标志
        hasPlayedFiveSecondSound = false;
        hasPlayedStartSound = false;
        
        // 检查倒计时是否为0，如果为0则隐藏UI
        if (countdownTimer <= 0f)
        {
            isCountdownActive = false;
            if (countdownUI != null)
            {
                countdownUI.SetActive(false);
            }
            
            if (enableCountdownLogging)
            {
                GameLogger.LogDev($"CountdownTimer: 倒计时为0，隐藏UI - {gameObject.name}");
            }
            return;
        }
        
        // 显示倒计时UI
        if (countdownUI != null)
        {
            countdownUI.SetActive(true);
        }
        
        // 确保倒计时UI跟随目标对象位置
        if (countdownCanvas != null && targetTransform != null)
        {
            Vector3 canvasPosition = targetTransform.position + Vector3.up * countdownUIHeight;
            countdownCanvas.transform.position = canvasPosition;
        }
        
        // 延迟播放倒计时开始音效（等待雄黄酒音效播放完）
        StartCoroutine(PlayStartSoundDelayed());
        
        if (enableCountdownLogging)
        {
            GameLogger.LogDev($"CountdownTimer: 开始倒计时 {actualDuration} 秒 - {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 延迟播放倒计时开始音效的协程
    /// </summary>
    private System.Collections.IEnumerator PlayStartSoundDelayed()
    {
        // 等待雄黄酒音效播放完
        yield return new WaitForSeconds(xionghuangSoundDuration);
        
        // 播放倒计时开始音效
        if (!hasPlayedStartSound && AudioManager.Instance != null)
        {
            hasPlayedStartSound = true;
            AudioManager.Instance.PlayClock();
            
            if (enableCountdownLogging)
            {
                GameLogger.LogDev($"CountdownTimer: 播放倒计时开始音效 - {gameObject.name}");
            }
        }
    }
    
    /// <summary>
    /// 停止倒计时
    /// </summary>
    public void StopCountdown()
    {
        isCountdownActive = false;
        isPaused = false;
        countdownTimer = 0f;
        
        // 重置音效播放标志
        hasPlayedFiveSecondSound = false;
        hasPlayedStartSound = false;
        
        // 隐藏倒计时UI
        HideCountdownUI();
        
        if (enableCountdownLogging)
        {
            GameLogger.LogDev($"CountdownTimer: 停止倒计时 - {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 暂停倒计时
    /// </summary>
    public void PauseCountdown()
    {
        if (isCountdownActive && !isPaused)
        {
            isPaused = true;
            pausedTime = countdownTimer;
            
            if (enableCountdownLogging)
            {
                GameLogger.LogDev($"CountdownTimer: 暂停倒计时，剩余时间: {pausedTime} 秒 - {gameObject.name}");
            }
        }
    }
    
    /// <summary>
    /// 恢复倒计时
    /// </summary>
    public void ResumeCountdown()
    {
        if (isCountdownActive && isPaused)
        {
            isPaused = false;
            countdownTimer = pausedTime;
            
            if (enableCountdownLogging)
            {
                GameLogger.LogDev($"CountdownTimer: 恢复倒计时，剩余时间: {countdownTimer} 秒 - {gameObject.name}");
            }
        }
    }
    
    /// <summary>
    /// 倒计时结束时的处理
    /// </summary>
    private void OnCountdownFinished()
    {
        isCountdownActive = false;
        
        // 隐藏倒计时UI
        HideCountdownUI();
        
        if (enableCountdownLogging)
        {
            GameLogger.LogDev($"CountdownTimer: 倒计时结束 - {gameObject.name}");
        }
        
        // 触发倒计时结束事件
        OnCountdownExpired();
    }
    
    /// <summary>
    /// 倒计时过期时的处理（可被子类重写或外部调用）
    /// </summary>
    protected virtual void OnCountdownExpired()
    {
     // 播放恢复人形音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBack();
            if (enableCountdownLogging)
            {
                GameLogger.LogDev($"CountdownTimer: 播放恢复人形音效 - {gameObject.name}");
        // 检查是否是从蛇状态结束
        Player player = GetComponent<Player>();
        if (player != null && player.CarryCharacter == "蛇")
        {
            // 发送蛇形态结束广播
            if (BroadcastManager.Instance != null)
            {
                BroadcastManager.Instance.BroadcastToAll("蛇形态结束");
                
                if (enableCountdownLogging)
                {
                    GameLogger.LogDev($"CountdownTimer: 蛇形态结束，已发送广播 '蛇形态结束' - {gameObject.name}");
                }
            }
            else
            {
                GameLogger.LogWarning("CountdownTimer: 未找到BroadcastManager实例，无法发送蛇形态结束广播");
            }
        }
        
        // 重置携带字符为初始值
        ResetCarryCharacterToInitial();
        
        // 重置音效播放标志
        hasPlayedFiveSecondSound = false;
        hasPlayedStartSound = false;
        
        // 子类可以重写此方法来实现自定义的倒计时结束逻辑
        // 例如：重置状态、播放音效、触发事件等
    }
    
    /// <summary>
    /// 设置目标对象（用于跟随移动）
    /// </summary>
    /// <param name="target">目标对象</param>
    public void SetTarget(Transform target)
    {
        targetTransform = target;
    }
    
    /// <summary>
    /// 设置倒计时持续时间
    /// </summary>
    /// <param name="duration">持续时间（秒）</param>
    public void SetCountdownDuration(float duration)
    {
        countdownDuration = duration;
    }
    
    /// <summary>
    /// 设置倒计时UI高度
    /// </summary>
    /// <param name="height">高度</param>
    public void SetUIHeight(float height)
    {
        countdownUIHeight = height;
    }
    
    /// <summary>
    /// 设置倒计时文字颜色
    /// </summary>
    /// <param name="color">颜色</param>
    public void SetTextColor(Color color)
    {
        countdownTextColor = color;
        if (countdownText != null)
        {
            countdownText.color = color;
        }
    }
    
    /// <summary>
    /// 设置倒计时字体大小
    /// </summary>
    /// <param name="fontSize">字体大小</param>
    public void SetFontSize(int fontSize)
    {
        countdownFontSize = fontSize;
        if (countdownText != null)
        {
            countdownText.fontSize = fontSize;
        }
    }
    
    /// <summary>
    /// 隐藏倒计时UI
    /// </summary>
    private void HideCountdownUI()
    {
        if (countdownUI != null)
        {
            countdownUI.SetActive(false);
        }
    }
    
    /// <summary>
    /// 重置携带字符为初始值
    /// </summary>
    private void ResetCarryCharacterToInitial()
    {
        // 获取Player组件
        Player player = GetComponent<Player>();
        if (player != null)
        {
            // 重置为初始携带字符
            player.ResetToInitialCarryCharacter();
            
            if (enableCountdownLogging)
            {
                GameLogger.LogDev($"CountdownTimer: 倒计时结束，已将玩家 '{player.gameObject.name}' 携带字符重置为初始值 '{player.GetInitialCarryCharacter()}'");
            }
        }
        else
        {
            if (enableCountdownLogging)
            {
                GameLogger.LogWarning($"CountdownTimer: 倒计时结束，但未找到Player组件，无法重置携带字符 - {gameObject.name}");
            }
        }
    }
    
    /// <summary>
    /// 获取当前倒计时状态
    /// </summary>
    /// <returns>是否正在倒计时</returns>
    public bool IsCountdownActive()
    {
        return isCountdownActive;
    }
    
    /// <summary>
    /// 获取剩余时间
    /// </summary>
    /// <returns>剩余时间（秒）</returns>
    public float GetRemainingTime()
    {
        return countdownTimer;
    }
    
    /// <summary>
    /// 获取倒计时进度（0-1）
    /// </summary>
    /// <returns>倒计时进度</returns>
    public float GetCountdownProgress()
    {
        if (countdownDuration <= 0f) return 0f;
        return Mathf.Clamp01(countdownTimer / countdownDuration);
    }
    
    /// <summary>
    /// 检查倒计时是否暂停
    /// </summary>
    /// <returns>是否暂停</returns>
    public bool IsPaused()
    {
        return isPaused;
    }
    
    /// <summary>
    /// 获取倒计时状态数据（用于存档）
    /// </summary>
    /// <returns>倒计时状态数据</returns>
    public GameProgressData.CountdownTimerState GetStateData()
    {
        return new GameProgressData.CountdownTimerState
        {
            objectName = gameObject.name,
            objectPath = GetObjectPath(),
            uniqueId = GetUniqueId(),
            isActive = isCountdownActive,
            isPaused = isPaused,
            remainingTime = countdownTimer,
            totalDuration = countdownDuration,
            position = countdownCanvas != null ? countdownCanvas.transform.position : Vector3.zero
        };
    }
    
    /// <summary>
    /// 从状态数据恢复倒计时（用于读档）
    /// </summary>
    /// <param name="stateData">倒计时状态数据</param>
    public void RestoreFromStateData(GameProgressData.CountdownTimerState stateData)
    {
        if (stateData == null) return;
        
        // 恢复倒计时状态
        isCountdownActive = stateData.isActive;
        isPaused = stateData.isPaused;
        countdownTimer = stateData.remainingTime;
        countdownDuration = stateData.totalDuration;
        
        // 检查倒计时是否为0或已结束
        if (countdownTimer <= 0f)
        {
            // 倒计时为0时，隐藏UI并设置为非激活状态
            isCountdownActive = false;
            HideCountdownUI();
            
            if (enableCountdownLogging)
            {
                GameLogger.LogDev($"CountdownTimer: 读档时倒计时为0，隐藏UI - {gameObject.name}");
            }
        }
        else if (isCountdownActive && countdownUI != null)
        {
            // 倒计时大于0且激活时，显示UI
            countdownUI.SetActive(true);
        }
        
        // 恢复UI位置
        if (countdownCanvas != null && stateData.position != Vector3.zero)
        {
            countdownCanvas.transform.position = stateData.position;
        }
        
        if (enableCountdownLogging)
        {
            GameLogger.LogDev($"CountdownTimer: 从存档恢复倒计时 - {gameObject.name}, 剩余时间: {countdownTimer}秒, 暂停: {isPaused}");
        }
    }
    
    /// <summary>
    /// 获取对象路径
    /// </summary>
    /// <returns>对象路径</returns>
    private string GetObjectPath()
    {
        return GetFullPath(transform);
    }
    
    /// <summary>
    /// 获取完整路径
    /// </summary>
    /// <param name="transform">Transform对象</param>
    /// <returns>完整路径</returns>
    private string GetFullPath(Transform transform)
    {
        if (transform.parent == null)
            return transform.name;
        return GetFullPath(transform.parent) + "/" + transform.name;
    }
    
    /// <summary>
    /// 获取唯一标识符
    /// </summary>
    /// <returns>唯一标识符</returns>
    private string GetUniqueId()
    {
        // 尝试获取现有的UniqueID组件
        var uniqueIdComponent = GetComponent<UniqueID>();
        if (uniqueIdComponent != null)
        {
            return uniqueIdComponent.ID;
        }
        
        // 如果没有UniqueID组件，使用对象路径作为后备
        return GetObjectPath();
    }
    
    /// <summary>
    /// 加载Number字体（TextMeshPro版本）
    /// </summary>
    /// <returns>Number字体，如果加载失败返回默认字体</returns>
    private TMP_FontAsset LoadNumberFont()
    {
        // 尝试从Assets/Font文件夹加载Number字体
        TMP_FontAsset numberFont = LoadFontAssetFromAssets("Font/Number");
        if (numberFont != null)
        {
            if (enableCountdownLogging)
            {
                GameLogger.LogDev($"CountdownTimer: 成功加载Number字体 - {gameObject.name}");
            }
            return numberFont;
        }
        
        // 如果找不到Number字体，使用默认字体作为后备
        if (enableCountdownLogging)
        {
            GameLogger.LogWarning($"CountdownTimer: 未找到Number字体，使用默认字体作为后备 - {gameObject.name}");
        }
        return Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
    }
    
    /// <summary>
    /// 从Assets文件夹加载TextMeshPro字体资源
    /// </summary>
    /// <param name="fontPath">字体路径（相对于Assets文件夹）</param>
    /// <returns>加载的TextMeshPro字体资源，如果失败返回null</returns>
    private TMP_FontAsset LoadFontAssetFromAssets(string fontPath)
    {
#if UNITY_EDITOR
        // 在编辑器中，使用AssetDatabase加载
        string fullPath = "Assets/" + fontPath;
        TMP_FontAsset fontAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fullPath);
        if (fontAsset != null)
        {
            return fontAsset;
        }
        
        // 尝试不同的文件扩展名
        string[] extensions = { ".asset", ".fontsettings" };
        foreach (string ext in extensions)
        {
            fontAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fullPath + ext);
            if (fontAsset != null)
            {
                return fontAsset;
            }
        }
#else
        // 在运行时，尝试从Resources加载（如果字体被移动到了Resources文件夹）
        TMP_FontAsset fontAsset = Resources.Load<TMP_FontAsset>(fontPath);
        if (fontAsset != null)
        {
            return fontAsset;
        }
        
        // 尝试从Resources根目录加载
        fontAsset = Resources.Load<TMP_FontAsset>("Number");
        if (fontAsset != null)
        {
            return fontAsset;
        }
#endif
        return null;
    }
    
    /// <summary>
    /// 检查初始倒计时状态，如果为0则隐藏
    /// </summary>
    private void CheckInitialCountdownState()
    {
        if (countdownTimer <= 0f)
        {
            // 倒计时为0时，隐藏UI并设置为非激活状态
            isCountdownActive = false;
            if (countdownUI != null)
            {
                countdownUI.SetActive(false);
            }
            
            if (enableCountdownLogging)
            {
                GameLogger.LogDev($"CountdownTimer: 初始倒计时为0，隐藏UI - {gameObject.name}");
            }
        }
    }
    
    /// <summary>
    /// 检查退出弹窗状态并暂停倒计时
    /// </summary>
    private void CheckExitDialogState()
    {
        // 检查退出弹窗是否显示
        bool exitDialogVisible = IsExitDialogVisible();
        
        if (exitDialogVisible && isCountdownActive && !isPaused)
        {
            // 退出弹窗显示时暂停倒计时
            PauseCountdown();
            
            if (enableCountdownLogging)
            {
                GameLogger.LogDev($"CountdownTimer: 退出弹窗显示，暂停倒计时 - {gameObject.name}");
            }
        }
        else if (!exitDialogVisible && isPaused)
        {
            // 退出弹窗隐藏时恢复倒计时
            ResumeCountdown();
            
            if (enableCountdownLogging)
            {
                GameLogger.LogDev($"CountdownTimer: 退出弹窗隐藏，恢复倒计时 - {gameObject.name}");
            }
        }
    }
    
    /// <summary>
    /// 检查退出弹窗是否可见
    /// </summary>
    /// <returns>退出弹窗是否可见</returns>
    private bool IsExitDialogVisible()
    {
        // 检查ExitGameManager的确认对话框是否显示
        if (ExitGameManager.Instance != null)
        {
            // 通过检查Time.timeScale是否为0来判断是否有弹窗显示
            // 或者直接检查ExitGameManager的对话框状态
            return Time.timeScale == 0f;
        }
        
        return false;
    }
    
    /// <summary>
    /// 检查Player状态变化并更新sprite
    /// </summary>
    private void CheckPlayerStateAndUpdateSprite()
    {
        if (playerComponent == null || playerSpriteRenderer == null)
        {
            return;
        }
        
        // 检查当前携带字符是否为"蛇"
        bool currentIsSnake = playerComponent.CarryCharacter == "蛇";
        
        // 如果状态发生变化
        if (currentIsSnake != isSnakeState)
        {
            isSnakeState = currentIsSnake;
            
            if (isSnakeState)
            {
                // 变为蛇状态，切换到蛇sprite
                SwitchToSnakeSprite();
            }
            else
            {
                // 不是蛇状态，恢复原始sprite
                RestoreOriginalSprite();
            }
        }
    }
    
    /// <summary>
    /// 切换到蛇状态sprite
    /// </summary>
    private void SwitchToSnakeSprite()
    {
        if (playerSpriteRenderer != null && snakeSprite != null)
        {
            playerSpriteRenderer.sprite = snakeSprite;
            
            if (enableCountdownLogging)
            {
                GameLogger.LogDev($"CountdownTimer: 已切换到蛇状态sprite - {gameObject.name}");
            }
        }
        else if (snakeSprite == null)
        {
            GameLogger.LogWarning($"CountdownTimer: 蛇状态sprite未设置 - {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 恢复原始sprite
    /// </summary>
    private void RestoreOriginalSprite()
    {
        if (playerSpriteRenderer != null && originalPlayerSprite != null)
        {
            playerSpriteRenderer.sprite = originalPlayerSprite;
            
            if (enableCountdownLogging)
            {
                GameLogger.LogDev($"CountdownTimer: 已恢复原始sprite - {gameObject.name}");
            }
        }
        else if (originalPlayerSprite == null)
        {
            GameLogger.LogWarning($"CountdownTimer: 原始sprite未保存 - {gameObject.name}");
        }
    }
    
}
