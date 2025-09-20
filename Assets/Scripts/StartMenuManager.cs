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
    [SerializeField] private Button startNewGameButton; // 从头开始按钮（序列化引用）
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
            // 如果提供了“从头开始”按钮，则优先使用它
            Button startBtn = startNewGameButton != null ? startNewGameButton : startGameButton;
            Button continueBtn = continueGameButton;
            
            // 如果Inspector中没有设置，尝试通过名称查找
            if (startBtn == null)
            {
                // 多重查找策略
                startBtn = FindButtonByMultipleNames(new string[] {
                    "StartNewGameButton", "StartGameButton", "开始游戏", "NewGameButton"
                });
            }
            if (continueBtn == null)
            {
                continueBtn = FindButtonByMultipleNames(new string[] {
                    "ContinueGameButton", "继续游戏", "ContinueButton", "ResumeButton"
                });
            }
            
            if (startBtn != null || continueBtn != null)
            {
                LevelProgressManager.Instance.SetButtonReferences(startBtn, continueBtn);
                LogDebug($"按钮设置成功 - Start: {startBtn?.name ?? "null"}, Continue: {continueBtn?.name ?? "null"}");

                // 覆盖“从头开始”按钮事件为本地清档逻辑
                if (startBtn != null)
                {
                    startBtn.onClick.RemoveAllListeners();
                    startBtn.onClick.AddListener(OnStartNewGameClicked);
                    LogDebug("已绑定从头开始按钮事件到 StartMenuManager.OnStartNewGameClicked");
                }

                // 传递“从头开始”按钮引用，便于显示逻辑隐藏/显示
                if (startNewGameButton != null)
                {
                    LevelProgressManager.Instance.SetStartNewGameButton(startNewGameButton);
                    LogDebug("已传递从头开始按钮引用到LevelProgressManager");
                }
            }
            else
            {
                LogDebug("未找到开始游戏或继续游戏按钮，请确保按钮名称正确或在Inspector中设置引用");
            }
        }
    }
    
    /// <summary>
    /// 通过多个名称查找按钮
    /// </summary>
    /// <param name="names">可能的按钮名称数组</param>
    /// <returns>找到的按钮，如果都没找到则返回null</returns>
    private Button FindButtonByMultipleNames(string[] names)
    {
        foreach (string name in names)
        {
            Button btn = GameObject.Find(name)?.GetComponent<Button>();
            if (btn != null)
            {
                LogDebug($"找到按钮: {name}");
                return btn;
            }
        }
        LogDebug($"未找到按钮，尝试的名称: [{string.Join(", ", names)}]");
        return null;
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
        
        // 确保GameBootstrap已初始化
        GameBootstrap.EnsureInitialized();
        
        // 清空所有本地存档（包括关卡进度与关卡内状态）
        GameStateManager.ClearAllGameStates();
        LevelProgressManager.Instance?.ClearAllProgress();
        PlayerPrefs.SetInt("GameStarted", 0); // 显式清零开关
        PlayerPrefs.Save();
        LogDebug("已清空所有关卡状态与进度，并将GameStarted=0");

        // 调用测试清空器（若场景中挂载）以保持一致的清理逻辑
        ClearAllLocalDataTester tester = FindObjectOfType<ClearAllLocalDataTester>();
        if (tester != null)
        {
            tester.ClearAllLocalData();
            LogDebug("已调用 ClearAllLocalDataTester.ClearAllLocalData()");
        }

        // 重置游戏进度（内存态）
        LevelProgressManager.Instance?.StartNewGame(); // 会清空GameStarted与完成列表

        // 安全地获取关卡序列并加载场景
        StartCoroutine(LoadFirstLevelSafely());
    }
    
    /// <summary>
    /// 安全地加载第一关
    /// </summary>
    private IEnumerator LoadFirstLevelSafely()
    {
        // 等待一帧确保所有清理操作完成
        yield return null;
        
        try
        {
            // 获取关卡序列
            string[] levelSequence = PublicData.GetLevelSequence();
            string level1 = "level1"; // 默认回退场景
            
            if (levelSequence != null && levelSequence.Length > 0)
            {
                level1 = levelSequence[0];
                LogDebug($"从关卡序列获取首关: {level1}");
            }
            else
            {
                LogDebug("关卡序列为空，使用默认场景: level1");
            }
            
            // 验证场景是否存在
            if (IsSceneInBuildSettings(level1))
            {
                LogDebug($"立即加载首关: {level1}");
                PublicData.OnBeforeSceneTransition();
                UnityEngine.SceneManagement.SceneManager.LoadScene(level1);
            }
            else
            {
                LogDebug($"场景 {level1} 不在构建设置中，尝试加载 level1");
                if (IsSceneInBuildSettings("level1"))
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("level1");
                }
                else
                {
                    LogDebug("错误：无法找到可用的关卡场景！");
                    // 可以在这里显示错误提示或回退到主菜单
                }
            }
        }
        catch (System.Exception e)
        {
            LogDebug($"加载场景时发生错误: {e.Message}");
            // 紧急回退：尝试加载 level1
            try
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("level1");
            }
            catch
            {
                LogDebug("紧急回退也失败了，请检查场景配置");
            }
        }
    }
    
    /// <summary>
    /// 检查场景是否在构建设置中
    /// </summary>
    private bool IsSceneInBuildSettings(string sceneName)
    {
        try
        {
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                string scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (sceneNameInBuild.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        catch (System.Exception e)
        {
            LogDebug($"检查场景构建设置时出错: {e.Message}");
        }
        return false;
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
        // 确保GameBootstrap已初始化
        GameBootstrap.EnsureInitialized();
        
        AudioClip clickClip = (AudioManager.Instance != null) ? AudioManager.Instance.sfxButtonClick : null;

        if (AudioManager.Instance != null && clickClip != null)
        {
            AudioManager.Instance.PlaySFX(clickClip);
            yield return new WaitForSeconds(clickClip.length);
        }
        // 若没有可用的AudioManager或音效未配置，则直接进入下一场景

        PublicData.OnBeforeSceneTransition();
        
        // 安全地确定要加载的场景
        string targetScene = GetTargetScene();
        LogDebug($"准备加载场景: {targetScene}");
        
        // 验证场景是否存在
        if (IsSceneInBuildSettings(targetScene))
        {
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            LogDebug($"场景 {targetScene} 不在构建设置中，尝试加载默认场景");
            if (IsSceneInBuildSettings("level1"))
            {
                SceneManager.LoadScene("level1");
            }
            else
            {
                LogDebug("错误：无法找到可用的场景！");
            }
        }
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