using System.Collections; // 必须引用此命名空间才能使用协程
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 开始菜单的管理器脚本
/// 负责处理UI交互，如点击"开始游戏"按钮，并播放相关音效
/// 集成了关卡进度管理功能
/// </summary>
public class StartMenuManager : MonoBehaviour
{
    [Header("场景设置")]
    public string sceneToLoad = "FormalLevel_Cowherd";
    
    [Header("进度管理")]
    [SerializeField] private bool enableDebugLog = true;
    
    [Header("按钮引用")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button continueGameButton;

    private void Start()
    {
        // 进入开始场景时播放菜单BGM（通过AudioManager统一控制）
        if (AudioManager.Instance != null && AudioManager.Instance.bgmMenu != null)
        {
            AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmMenu);
        }
        
        // 设置LevelProgressManager的按钮引用（如果存在）
        SetupProgressManagerButtons();
    }
    
    /// <summary>
    /// 设置LevelProgressManager的按钮引用
    /// </summary>
    private void SetupProgressManagerButtons()
    {
        if (LevelProgressManager.Instance != null)
        {
            // 优先使用Inspector中设置的按钮引用
            Button startBtn = startGameButton;
            Button continueBtn = continueGameButton;
            
            // 如果Inspector中没有设置，尝试通过名称查找
            if (startBtn == null)
            {
                startBtn = GameObject.Find("StartGameButton")?.GetComponent<Button>();
            }
            if (continueBtn == null)
            {
                continueBtn = GameObject.Find("ContinueGameButton")?.GetComponent<Button>();
            }
            
            if (startBtn != null || continueBtn != null)
            {
                LevelProgressManager.Instance.SetButtonReferences(startBtn, continueBtn);
                LogDebug("已设置LevelProgressManager按钮引用");
            }
            else
            {
                LogDebug("未找到开始游戏或继续游戏按钮，请确保按钮名称正确或在Inspector中设置引用");
            }
        }
    }
    
    /// <summary>
    /// 手动设置按钮引用
    /// </summary>
    /// <param name="startBtn">开始游戏按钮</param>
    /// <param name="continueBtn">继续游戏按钮</param>
    public void SetButtonReferences(Button startBtn, Button continueBtn)
    {
        startGameButton = startBtn;
        continueGameButton = continueBtn;
        
        // 如果LevelProgressManager存在，立即设置引用
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.SetButtonReferences(startBtn, continueBtn);
            LogDebug("已手动设置按钮引用");
        }
    }

    public void OnStartGameClicked()
    {
        // 统一经由AudioManager播放点击音效，并在音效结束后加载场景
        StartCoroutine(LoadSceneAfterSound());
    }
    
    /// <summary>
    /// 开始新游戏
    /// </summary>
    public void OnStartNewGameClicked()
    {
        LogDebug("开始新游戏");
        
        // 重置游戏进度
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.StartNewGame();
        }
        
        // 播放音效并加载第一个关卡
        StartCoroutine(LoadSceneAfterSound());
    }
    
    /// <summary>
    /// 继续游戏
    /// </summary>
    public void OnContinueGameClicked()
    {
        LogDebug("继续游戏");
        
        // 从上次进度继续
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.ContinueGame();
        }
        else
        {
            LogDebug("LevelProgressManager未找到，使用默认场景");
            StartCoroutine(LoadSceneAfterSound());
        }
    }

    /// <summary>
    /// 播放点击音效并等待其播放完毕后再加载场景
    /// </summary>
    private IEnumerator LoadSceneAfterSound()
    {
        AudioClip clickClip = (AudioManager.Instance != null) ? AudioManager.Instance.sfxButtonClick : null;

        if (AudioManager.Instance != null && clickClip != null)
        {
            AudioManager.Instance.PlaySFX(clickClip);
            yield return new WaitForSeconds(clickClip.length);
        }
        // 若没有可用的AudioManager或音效未配置，则直接进入下一场景

        PublicData.OnBeforeSceneTransition();
        
        // 确定要加载的场景
        string targetScene = GetTargetScene();
        LogDebug($"加载场景: {targetScene}");
        SceneManager.LoadScene(targetScene);
    }
    
    /// <summary>
    /// 获取目标场景名称
    /// </summary>
    /// <returns>要加载的场景名称</returns>
    private string GetTargetScene()
    {
        // 如果有LevelProgressManager，优先使用其逻辑
        if (LevelProgressManager.Instance != null)
        {
            string progressScene = LevelProgressManager.Instance.GetCurrentLevelToLoad();
            if (!string.IsNullOrEmpty(progressScene))
            {
                return progressScene;
            }
        }
        
        // 如果没有进度或LevelProgressManager不可用，使用默认场景
        return sceneToLoad;
    }

    public void PlayHoverSound()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.sfxButtonHover != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxButtonHover);
        }
    }
    
    /// <summary>
    /// 检查是否有游戏进度可以继续
    /// </summary>
    /// <returns>是否有进度可以继续</returns>
    public bool HasGameProgress()
    {
        if (LevelProgressManager.Instance != null)
        {
            return LevelProgressManager.Instance.HasGameStarted();
        }
        return false;
    }
    
    /// <summary>
    /// 获取游戏进度信息
    /// </summary>
    /// <returns>进度信息字符串</returns>
    public string GetProgressInfo()
    {
        if (LevelProgressManager.Instance != null)
        {
            int completed = LevelProgressManager.Instance.GetCompletedLevelsCount();
            int total = LevelProgressManager.Instance.GetTotalLevelsCount();
            float percentage = LevelProgressManager.Instance.GetProgressPercentage();
            return $"已完成 {completed}/{total} 关卡 ({percentage:F0}%)";
        }
        return "无进度信息";
    }
    
    /// <summary>
    /// 调试日志输出
    /// </summary>
    /// <param name="message">日志消息</param>
    private void LogDebug(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log($"[StartMenuManager] {message}");
        }
    }
}