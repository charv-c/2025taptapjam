using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 退出游戏管理器 - 负责处理退出游戏的逻辑
/// 集成游戏状态保存功能，确保退出前保存当前进度
/// </summary>
public class ExitGameManager : MonoBehaviour
{
    [Header("UI设置")]
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject confirmationDialog;
    
    [Header("按钮预制体")]
    [SerializeField] private GameObject confirmButtonPrefab;
    [SerializeField] private GameObject cancelButtonPrefab;
    
    [Header("按钮布局设置")]
    [SerializeField] private float buttonSpacing = 20f;
    [SerializeField] private Vector2 buttonSize = new Vector2(120f, 40f);
    [SerializeField] private bool autoLayoutButtons = true;
    
    [Header("音效设置")]
    [SerializeField] private bool playExitSound = true;
    
    [Header("调试设置")]
    [SerializeField] private bool enableDebugLog = true;
    
    // 动态创建的按钮引用
    private Button confirmExitButton;
    private Button cancelExitButton;
    
    private void Start()
    {
        SetupUI();
        SetupInputHandling();
    }
    
    private void Update()
    {
        HandleKeyboardInput();
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
        
        // 创建确认对话框按钮
        CreateConfirmationButtons();
        
        // 初始隐藏确认对话框
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(false);
        }
    }
    
    /// <summary>
    /// 创建确认对话框按钮
    /// </summary>
    private void CreateConfirmationButtons()
    {
        if (confirmationDialog == null)
        {
            LogWarning("确认对话框未设置，无法创建按钮");
            return;
        }
        
        // 查找Canvas或创建按钮容器
        Canvas canvas = confirmationDialog.GetComponentInParent<Canvas>();
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
        Transform existingContainer = confirmationDialog.transform.Find("ButtonContainer");
        if (existingContainer != null)
        {
            return existingContainer.gameObject;
        }
        
        // 创建新的按钮容器
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(confirmationDialog.transform, false);
        
        // 添加RectTransform组件
        RectTransform rectTransform = buttonContainer.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.1f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.1f);
        rectTransform.sizeDelta = new Vector2(300f, 60f);
        rectTransform.anchoredPosition = Vector2.zero;
        
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
        // ESC键退出游戏
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnExitButtonClicked();
        }
        
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
        
        // 播放确认音效
        PlayConfirmSound();
        
        // 退出游戏并保存状态
        ExitGameWithSave();
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
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(true);
            
            // 暂停游戏（可选）
            Time.timeScale = 0f;
            
            LogDebug("显示退出确认对话框");
        }
    }
    
    /// <summary>
    /// 隐藏确认对话框
    /// </summary>
    private void HideConfirmationDialog()
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(false);
            
            // 恢复游戏（可选）
            Time.timeScale = 1f;
            
            LogDebug("隐藏退出确认对话框");
        }
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
        if (confirmationDialog == null) return;
        
        Transform buttonContainer = confirmationDialog.transform.Find("ButtonContainer");
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
}
