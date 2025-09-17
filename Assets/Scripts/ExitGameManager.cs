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
    [SerializeField] private Button confirmExitButton;
    [SerializeField] private Button cancelExitButton;
    
    [Header("音效设置")]
    [SerializeField] private bool playExitSound = true;
    
    [Header("调试设置")]
    [SerializeField] private bool enableDebugLog = true;
    
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
        
        // 设置确认对话框按钮
        if (confirmExitButton != null)
        {
            confirmExitButton.onClick.AddListener(OnConfirmExitClicked);
        }
        
        if (cancelExitButton != null)
        {
            cancelExitButton.onClick.AddListener(OnCancelExitClicked);
        }
        
        // 初始隐藏确认对话框
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(false);
        }
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
}
