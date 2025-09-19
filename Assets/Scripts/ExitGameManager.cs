using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 退出游戏管理器 - 负责处理退出游戏的逻辑
/// 集成游戏状态保存功能，确保退出前保存当前进度
/// </summary>
public class ExitGameManager : MonoBehaviour
{
    // 跨场景单例
    public static ExitGameManager Instance { get; private set; }
    [Header("UI设置")]
    [SerializeField] private Button exitButton;
    
    [Header("确认对话框预制体")]
    [SerializeField] private GameObject confirmationDialogPrefab;
    private GameObject confirmationDialogInstance;
    
    [Header("按钮预制体")]
    [SerializeField] private GameObject confirmButtonPrefab;
    [SerializeField] private GameObject cancelButtonPrefab;
    
    [Header("按钮布局设置")]
    [SerializeField] private float buttonSpacing = 20f;
    [SerializeField] private Vector2 buttonSize = new Vector2(120f, 40f);
    [SerializeField] private bool autoLayoutButtons = true;
    [Tooltip("按钮容器锚点的Y值（0=底部, 1=顶部）")]
    [SerializeField, Range(0f,1f)] private float buttonAnchorY = 0.3f;
    [Tooltip("在锚点基础上的Y偏移（像素，正值向上）")]
    [SerializeField] private float buttonYOffset = 24f;
    
    [Header("音效设置")]
    [SerializeField] private bool playExitSound = true;
    
    [Header("调试设置")]
    [SerializeField] private bool enableDebugLog = true;
    
    [Header("全屏控制")]
    [Tooltip("拦截Esc导致的全屏退出（Standalone等可控平台有效）")]
    [SerializeField] private bool preventEscExitFullScreen = true;
    [Tooltip("Windows上使用无边框全屏模式，避免Esc触发系统级退出全屏")]
    [SerializeField] private bool forceBorderlessFullscreenOnWindows = true;
    
    // 期望的全屏状态（用于在部分平台上被动退出时强制恢复）
    private bool desiredFullScreen;
    
    // ESC弹窗禁用状态（用于在飞行动画期间禁用ESC弹窗）
    private bool exitDialogDisabled = false;
    
    // 动态创建的按钮引用
    private Button confirmExitButton;
    private Button cancelExitButton;
    
    private void Awake()
    {
        // 单例与跨场景持久化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // 订阅场景加载事件
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += HandleSceneLoaded;

        // 在Windows上强制使用无边框全屏，避免Esc由系统层退出
#if UNITY_STANDALONE_WIN
        if (forceBorderlessFullscreenOnWindows)
        {
            try
            {
                if (Screen.fullScreenMode != FullScreenMode.FullScreenWindow)
                {
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                }
                if (!Screen.fullScreen)
                {
                    Screen.fullScreen = true;
                }
            }
            catch { }
        }
#endif
    }

    private void Start()
    {
        SetupUI();
        SetupInputHandling();
        desiredFullScreen = Screen.fullScreen;
    }
    
    private void Update()
    {
        HandleKeyboardInput();

        // 在可控平台上强制保持全屏（例如Standalone）。
        // 注：WebGL平台浏览器层面不允许拦截Esc退出全屏，无法强制恢复。
#if !UNITY_WEBGL
        if (preventEscExitFullScreen && desiredFullScreen && !Screen.fullScreen)
        {
            Screen.fullScreen = true;
        }
#endif
    }
    
    /// <summary>
    /// 设置UI组件
    /// </summary>
    private void SetupUI()
    {
        // 设置退出按钮点击事件
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }
        
        // 确保对话框实例存在
        EnsureConfirmationDialogInstance();

        // 创建确认对话框按钮
        CreateConfirmationButtons();
        
        // 初始隐藏确认对话框（若已存在实例）
        if (confirmationDialogInstance != null)
        {
            confirmationDialogInstance.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= HandleSceneLoaded;
        }
    }

    /// <summary>
    /// 场景加载完成后重新初始化UI引用与对话框
    /// </summary>
    private void HandleSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // 启动场景（startup）不创建退出对话框，避免重复或无意义的初始化
        if (scene.name.Trim().ToLowerInvariant() == "startup")
        {
            LogDebug("Startup场景：跳过退出对话框的UI初始化");
            return;
        }
        // 尝试从新场景中查找退出按钮（可选）
        if (exitButton == null)
        {
            GameObject found = GameObject.Find("ExitButton");
            if (found != null)
            {
                exitButton = found.GetComponent<Button>();
            }
        }
        
        // 重新设置UI（重新创建对话框与按钮），放到下一帧执行以确保新场景UI已初始化
        StartCoroutine(RebuildUIAfterSceneLoaded());
    }

    /// <summary>
    /// 场景切换后延迟一帧重建UI，确保Canvas与EventSystem已就绪
    /// </summary>
    private IEnumerator RebuildUIAfterSceneLoaded()
    {
        // 清理上一场景的对话框实例
        if (confirmationDialogInstance != null)
        {
            Destroy(confirmationDialogInstance);
            confirmationDialogInstance = null;
        }

        // 等待一帧，确保场景中的Canvas创建完成
        yield return null;

        // 额外再等一帧，兼容某些加载路径
        yield return null;

        // 尝试多次查找Canvas（最多尝试10帧）
        bool ensured = false;
        for (int i = 0; i < 10; i++)
        {
            if (EnsureConfirmationDialogInstance())
            {
                ensured = true;
                break;
            }
            yield return null;
        }

        if (!ensured)
        {
            LogWarning("场景切换后未能创建确认对话框（未找到Canvas）");
            yield break;
        }

        // 创建按钮并初始化隐藏
        CreateConfirmationButtons();
        if (confirmationDialogInstance != null)
        {
            confirmationDialogInstance.SetActive(false);
        }
    }
    
    /// <summary>
    /// 创建确认对话框按钮
    /// </summary>
    private void CreateConfirmationButtons()
    {
        if (confirmationDialogInstance == null)
        {
            LogWarning("确认对话框实例不存在，无法创建按钮");
            return;
        }
        
        // 先清理旧的按钮，避免在同一场景被多次初始化时出现重复按钮
        ClearExistingButtons();
        
        // 查找Canvas或创建按钮容器
        Canvas canvas = confirmationDialogInstance.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            LogWarning("未找到Canvas，无法创建按钮");
            return;
        }
        
        // 创建按钮容器
        GameObject buttonContainer = CreateButtonContainer(canvas);
        
        // 创建确认按钮
        if (confirmButtonPrefab != null)
        {
            GameObject confirmBtnObj = Instantiate(confirmButtonPrefab, buttonContainer.transform);
            confirmExitButton = confirmBtnObj.GetComponent<Button>();
            if (confirmExitButton != null)
            {
                confirmExitButton.onClick.AddListener(OnConfirmExitClicked);
                LogDebug("确认按钮已创建并设置事件");
            }
        }
        else
        {
            LogWarning("确认按钮预制体未设置");
        }
        
        // 创建取消按钮
        if (cancelButtonPrefab != null)
        {
            GameObject cancelBtnObj = Instantiate(cancelButtonPrefab, buttonContainer.transform);
            cancelExitButton = cancelBtnObj.GetComponent<Button>();
            if (cancelExitButton != null)
            {
                cancelExitButton.onClick.AddListener(OnCancelExitClicked);
                LogDebug("取消按钮已创建并设置事件");
            }
        }
        else
        {
            LogWarning("取消按钮预制体未设置");
        }
        
        // 自动布局按钮
        if (autoLayoutButtons)
        {
            LayoutButtons(buttonContainer);
        }
    }
    
    /// <summary>
    /// 创建按钮容器
    /// </summary>
    /// <param name="canvas">Canvas引用</param>
    /// <returns>按钮容器GameObject</returns>
    private GameObject CreateButtonContainer(Canvas canvas)
    {
        // 查找现有的按钮容器
        Transform existingContainer = confirmationDialogInstance.transform.Find("ButtonContainer");
        if (existingContainer != null)
        {
            // 应用当前锚点与偏移设置到已有容器
            RectTransform exRect = existingContainer.GetComponent<RectTransform>();
            if (exRect != null)
            {
                exRect.anchorMin = new Vector2(0.5f, buttonAnchorY);
                exRect.anchorMax = new Vector2(0.5f, buttonAnchorY);
                exRect.anchoredPosition = new Vector2(0f, buttonYOffset);
            }
            return existingContainer.gameObject;
        }
        
        // 创建新的按钮容器
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(confirmationDialogInstance.transform, false);
        
        // 添加RectTransform组件
        RectTransform rectTransform = buttonContainer.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, buttonAnchorY);
        rectTransform.anchorMax = new Vector2(0.5f, buttonAnchorY);
        rectTransform.sizeDelta = new Vector2(300f, 60f);
        rectTransform.anchoredPosition = new Vector2(0f, buttonYOffset);
        
        // 添加HorizontalLayoutGroup组件用于自动布局
        UnityEngine.UI.HorizontalLayoutGroup layoutGroup = buttonContainer.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        layoutGroup.spacing = buttonSpacing;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;
        
        // 添加ContentSizeFitter组件
        UnityEngine.UI.ContentSizeFitter sizeFitter = buttonContainer.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        sizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        
        LogDebug("按钮容器已创建");
        return buttonContainer;
    }

    /// <summary>
    /// 确保确认对话框实例存在
    /// </summary>
    private bool EnsureConfirmationDialogInstance()
    {
        if (confirmationDialogInstance != null) return true;
        
        if (confirmationDialogPrefab == null)
        {
            LogWarning("确认对话框预制体未设置，无法实例化对话框");
            return false;
        }
        
        // 查找画布
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            LogWarning("场景中未找到Canvas，无法实例化确认对话框");
            return false;
        }
        
        // 实例化对话框到Canvas
        confirmationDialogInstance = Instantiate(confirmationDialogPrefab, canvas.transform);
        confirmationDialogInstance.name = "ConfirmationDialog";
        confirmationDialogInstance.SetActive(false);
        
        LogDebug("已实例化确认对话框预制体");
        return true;
    }
    
    /// <summary>
    /// 布局按钮
    /// </summary>
    /// <param name="buttonContainer">按钮容器</param>
    private void LayoutButtons(GameObject buttonContainer)
    {
        if (buttonContainer == null) return;
        
        // 设置按钮大小
        Button[] buttons = buttonContainer.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                buttonRect.sizeDelta = buttonSize;
            }
        }
        
        LogDebug($"已布局 {buttons.Length} 个按钮");
    }
    
    /// <summary>
    /// 设置输入处理
    /// </summary>
    private void SetupInputHandling()
    {
        // 可以在这里添加其他输入处理逻辑
    }
    
    /// <summary>
    /// 处理键盘输入
    /// </summary>
    private void HandleKeyboardInput()
    {
        // 如果ESC弹窗被禁用，忽略所有退出相关的键盘输入
        if (exitDialogDisabled)
        {
            return;
        }
        
        // 处理退出键
#if UNITY_WEBGL
        // WebGL 平台：浏览器层面会用 ESC 退出全屏，无法拦截。
        // 因此在 WebGL 上忽略 ESC，并改用备用热键（例如 Q）弹出退出对话框。
        if (Input.GetKeyDown(KeyCode.Q))
        {
            OnExitButtonClicked();
        }
#else
        // 非 WebGL 平台：使用 ESC 弹出退出对话框，并强制保持全屏（若已启用防护）。
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (preventEscExitFullScreen)
            {
                desiredFullScreen = true; // 仍然期望保持全屏
                if (!Screen.fullScreen)
                {
                    Screen.fullScreen = true;
                }
            }
            // 延迟一帧再弹窗，避免个别显卡驱动在Esc后瞬时改变全屏状态导致UI重建
            StartCoroutine(ShowExitDialogNextFrame());
        }
#endif
        
        // Alt+F4退出游戏（Windows）
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.F4))
        {
            OnExitButtonClicked();
        }
    }
    
    /// <summary>
    /// 退出按钮点击事件
    /// </summary>
    public void OnExitButtonClicked()
    {
        LogDebug("退出按钮被点击");
        
        // 播放点击音效
        PlayClickSound();
        
        // 显示确认对话框
        ShowConfirmationDialog();
    }
    
    /// <summary>
    /// 确认退出按钮点击事件
    /// </summary>
    public void OnConfirmExitClicked()
    {
        LogDebug("确认退出游戏");
        
        // 立即退出：不播放任何音效，不等待
        ExitGameImmediateNoSound();
    }
    
    /// <summary>
    /// 取消退出按钮点击事件
    /// </summary>
    public void OnCancelExitClicked()
    {
        LogDebug("取消退出游戏");
        
        // 播放取消音效
        PlayCancelSound();
        
        // 隐藏确认对话框
        HideConfirmationDialog();
    }
    
    /// <summary>
    /// 显示确认对话框
    /// </summary>
    private void ShowConfirmationDialog()
    {
        EnsureConfirmationDialogInstance();
        if (confirmationDialogInstance == null) return;
        
        confirmationDialogInstance.SetActive(true);
        
        // 暂停游戏（可选）
        Time.timeScale = 0f;
        
        LogDebug("显示退出确认对话框");
    }
    
    /// <summary>
    /// 隐藏确认对话框
    /// </summary>
    private void HideConfirmationDialog()
    {
        if (confirmationDialogInstance == null) return;
        
        confirmationDialogInstance.SetActive(false);
        
        // 恢复游戏（可选）
        Time.timeScale = 1f;
        
        LogDebug("隐藏退出确认对话框");
    }

    /// <summary>
    /// 对外暴露：保证退出确认对话框处于隐藏状态（用于存档前）
    /// </summary>
    public void EnsureExitDialogHidden()
    {
        if (confirmationDialogInstance != null && confirmationDialogInstance.activeSelf)
        {
            confirmationDialogInstance.SetActive(false);
            LogDebug("EnsureExitDialogHidden: 存档前已强制隐藏退出确认对话框");
        }

        // 确保恢复时间流逝到正常速度（防止上一场景打开对话框把时间冻结在0）
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
            LogDebug("EnsureExitDialogHidden: 恢复 Time.timeScale = 1");
        }
    }

    /// <summary>
    /// 在保存前强制将退出对话框设为隐藏，避免下次加载时被恢复为显示
    /// </summary>
    public void HideDialogForSaving()
    {
        if (confirmationDialogInstance != null && confirmationDialogInstance.activeSelf)
        {
            confirmationDialogInstance.SetActive(false);
            LogDebug("保存前强制隐藏退出确认对话框");
        }
    }
    
    /// <summary>
    /// 协程：下一帧再弹出退出确认对话框
    /// </summary>
    private IEnumerator ShowExitDialogNextFrame()
    {
        yield return null; // 等待一帧，避免Esc触发的全屏切换抖动影响UI
        OnExitButtonClicked();
    }
    
    /// <summary>
    /// 退出游戏并保存状态
    /// </summary>
    private void ExitGameWithSave()
    {
        LogDebug("开始退出游戏流程");
        
        // 保存游戏状态
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SaveGameState();
            LogDebug("游戏状态已保存");
        }
        else
        {
            LogWarning("GameStateManager实例不存在，无法保存状态");
        }
        
        // 保存关卡进度
        if (LevelProgressManager.Instance != null)
        {
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            LevelProgressManager.Instance.SetCurrentLevel(currentScene);
            LogDebug("关卡进度已保存");
        }
        
        // 播放退出音效
        if (playExitSound)
        {
            PlayExitSound();
        }
        
        // 延迟退出，确保保存完成
        StartCoroutine(ExitGameDelayed());
    }

    /// <summary>
    /// 立即退出游戏（无音效、无延迟），但仍保存必要状态
    /// </summary>
    private void ExitGameImmediateNoSound()
    {
        // 保存游戏状态
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SaveGameState();
            LogDebug("游戏状态已保存(立即退出)");
        }
        else
        {
            LogWarning("GameStateManager实例不存在，无法保存状态(立即退出)");
        }
        
        // 保存关卡进度
        if (LevelProgressManager.Instance != null)
        {
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            LevelProgressManager.Instance.SetCurrentLevel(currentScene);
            LogDebug("关卡进度已保存(立即退出)");
        }
        
        // 直接退出
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    /// <summary>
    /// 延迟退出游戏
    /// </summary>
    private IEnumerator ExitGameDelayed()
    {
        yield return new WaitForSeconds(0.5f); // 等待音效播放
        
        LogDebug("退出游戏");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    /// <summary>
    /// 播放点击音效
    /// </summary>
    private void PlayClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
    
    /// <summary>
    /// 播放确认音效
    /// </summary>
    private void PlayConfirmSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
    
    /// <summary>
    /// 播放取消音效
    /// </summary>
    private void PlayCancelSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonHover();
        }
    }
    
    /// <summary>
    /// 播放退出音效
    /// </summary>
    private void PlayExitSound()
    {
        if (AudioManager.Instance != null)
        {
            // 播放退出音效，如果没有专门的退出音效，可以使用其他音效
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxWin); // 临时使用胜利音效
        }
    }
    
    /// <summary>
    /// 调试日志输出
    /// </summary>
    private void LogDebug(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log($"[ExitGameManager] {message}");
        }
    }
    
    /// <summary>
    /// 警告日志输出
    /// </summary>
    private void LogWarning(string message)
    {
        Debug.LogWarning($"[ExitGameManager] {message}");
    }
    
    /// <summary>
    /// 测试退出功能
    /// </summary>
    [ContextMenu("测试退出游戏")]
    public void TestExitGame()
    {
        LogDebug("测试退出游戏功能");
        OnExitButtonClicked();
    }
    
    /// <summary>
    /// 强制退出（不显示确认对话框）
    /// </summary>
    [ContextMenu("强制退出游戏")]
    public void ForceExitGame()
    {
        LogDebug("强制退出游戏");
        ExitGameWithSave();
    }
    
    /// <summary>
    /// 设置按钮预制体
    /// </summary>
    /// <param name="confirmPrefab">确认按钮预制体</param>
    /// <param name="cancelPrefab">取消按钮预制体</param>
    public void SetButtonPrefabs(GameObject confirmPrefab, GameObject cancelPrefab)
    {
        confirmButtonPrefab = confirmPrefab;
        cancelButtonPrefab = cancelPrefab;
        LogDebug("按钮预制体已设置");
    }
    
    /// <summary>
    /// 重新创建按钮
    /// </summary>
    [ContextMenu("重新创建按钮")]
    public void RecreateButtons()
    {
        LogDebug("重新创建确认对话框按钮");
        
        // 清除现有按钮
        ClearExistingButtons();
        
        // 重新创建按钮
        CreateConfirmationButtons();
    }
    
    /// <summary>
    /// 清除现有按钮
    /// </summary>
    private void ClearExistingButtons()
    {
        if (confirmationDialogInstance == null) return;
        
        Transform buttonContainer = confirmationDialogInstance.transform.Find("ButtonContainer");
        if (buttonContainer != null)
        {
            DestroyImmediate(buttonContainer.gameObject);
            LogDebug("已清除现有按钮");
        }
        
        confirmExitButton = null;
        cancelExitButton = null;
    }
    
    /// <summary>
    /// 设置按钮布局参数
    /// </summary>
    /// <param name="spacing">按钮间距</param>
    /// <param name="size">按钮大小</param>
    public void SetButtonLayout(float spacing, Vector2 size)
    {
        buttonSpacing = spacing;
        buttonSize = size;
        LogDebug($"按钮布局参数已设置 - 间距: {spacing}, 大小: {size}");
    }
    
    /// <summary>
    /// 获取按钮引用
    /// </summary>
    /// <returns>按钮引用数组 [确认按钮, 取消按钮]</returns>
    public Button[] GetButtonReferences()
    {
        return new Button[] { confirmExitButton, cancelExitButton };
    }
    
    /// <summary>
    /// 设置ESC弹窗禁用状态
    /// </summary>
    /// <param name="disabled">是否禁用ESC弹窗</param>
    public void SetExitDialogDisabled(bool disabled)
    {
        exitDialogDisabled = disabled;
        LogDebug($"ESC弹窗禁用状态已设置为: {disabled}");
    }
    
    /// <summary>
    /// 获取ESC弹窗禁用状态
    /// </summary>
    /// <returns>是否禁用ESC弹窗</returns>
    public bool IsExitDialogDisabled()
    {
        return exitDialogDisabled;
    }
}
