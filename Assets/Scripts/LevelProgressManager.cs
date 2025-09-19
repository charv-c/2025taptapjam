using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 关卡进度管理器
/// 负责管理游戏关卡的进度存储和读取，实现本地存档功能
/// </summary>
public class LevelProgressManager : MonoBehaviour
{
    // 单例实例
    public static LevelProgressManager Instance { get; private set; }
    
    [Header("进度设置")]
    [SerializeField] private bool enableDebugLog = true;
    
    [Header("按钮管理")]
    [SerializeField] private Button startGameButton; // 无存档时的“开始游戏”
    [SerializeField] private Button startNewGameButton; // 有存档时的“从头开始”
    [SerializeField] private Button continueGameButton;
    [SerializeField] private bool autoManageButtons = true;
    
    // PlayerPrefs键名常量
    private const string CURRENT_LEVEL_KEY = "CurrentLevel";
    private const string COMPLETED_LEVELS_KEY = "CompletedLevels";
    private const string GAME_STARTED_KEY = "GameStarted";
    
    // 当前游戏进度
    private string currentLevel;
    private HashSet<string> completedLevels;
    
    private void Awake()
    {
        // 实现单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeProgress();
            LogDebug("LevelProgressManager 初始化并设置为跨场景持久化");
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 初始化进度数据
    /// </summary>
    private void InitializeProgress()
    {
        LoadProgress();
        
        // 自动修复无效的存档数据
        FixInvalidSaveData();
        
        // 自动管理按钮状态
        if (autoManageButtons)
        {
            UpdateButtonStates();
        }
    }

    private void OnEnable()
    {
        // 监听场景加载，切换场景后刷新按钮状态
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // 场景切换后重新加载进度并更新按钮
        LoadProgress();
        if (autoManageButtons)
        {
            UpdateButtonStates();
        }
    }
    
    /// <summary>
    /// 从本地存储加载游戏进度
    /// </summary>
    private void LoadProgress()
    {
        // 加载当前关卡
        currentLevel = PlayerPrefs.GetString(CURRENT_LEVEL_KEY, "");
        
        // 加载已完成的关卡列表
        string completedLevelsString = PlayerPrefs.GetString(COMPLETED_LEVELS_KEY, "");
        completedLevels = new HashSet<string>();
        
        if (!string.IsNullOrEmpty(completedLevelsString))
        {
            string[] levels = completedLevelsString.Split(',');
            foreach (string level in levels)
            {
                if (!string.IsNullOrEmpty(level))
                {
                    string trimmedLevel = level.Trim();
                    // 验证关卡名称是否在有效序列中
                    if (IsValidLevelName(trimmedLevel))
                    {
                        completedLevels.Add(trimmedLevel);
                    }
                    else
                    {
                        LogDebug($"警告：发现无效的关卡名称 '{trimmedLevel}'，已忽略");
                    }
                }
            }
        }
        
        // 验证当前关卡是否有效
        if (!string.IsNullOrEmpty(currentLevel) && !IsValidLevelName(currentLevel))
        {
            LogDebug($"警告：当前关卡 '{currentLevel}' 无效，已重置为空");
            currentLevel = "";
        }
        
        LogDebug($"加载进度 - 当前关卡: '{currentLevel}', 已完成关卡: [{string.Join(", ", completedLevels)}]");
        LogDebug($"关卡序列: [{string.Join(", ", PublicData.LevelSequence)}]");
    }
    
    /// <summary>
    /// 保存游戏进度到本地存储
    /// </summary>
    private void SaveProgress()
    {
        // 保存当前关卡
        PlayerPrefs.SetString(CURRENT_LEVEL_KEY, currentLevel);
        
        // 保存已完成的关卡列表
        string completedLevelsString = string.Join(",", completedLevels);
        PlayerPrefs.SetString(COMPLETED_LEVELS_KEY, completedLevelsString);
        
        // 标记游戏已开始
        PlayerPrefs.SetInt(GAME_STARTED_KEY, 1);
        
        PlayerPrefs.Save();
        
        LogDebug($"保存进度 - 当前关卡: '{currentLevel}', 已完成关卡: [{string.Join(", ", completedLevels)}]");
    }
    
    /// <summary>
    /// 开始新游戏（重置进度）
    /// </summary>
    public void StartNewGame()
    {
        LogDebug("开始新游戏，重置所有进度");
        
        // 重置进度
        currentLevel = "";
        completedLevels.Clear();
        
        // 清除本地存储
        PlayerPrefs.DeleteKey(CURRENT_LEVEL_KEY);
        PlayerPrefs.DeleteKey(COMPLETED_LEVELS_KEY);
        PlayerPrefs.DeleteKey(GAME_STARTED_KEY);
        PlayerPrefs.Save();
        
        // 更新按钮状态
        if (autoManageButtons)
        {
            UpdateButtonStates();
        }
        
        LogDebug("新游戏进度已重置");
    }
    
    /// <summary>
    /// 继续游戏（从上次进度开始）
    /// </summary>
    public void ContinueGame()
    {
        string levelToLoad = GetCurrentLevelToLoad();
        LogDebug($"继续游戏，加载关卡: '{levelToLoad}'");
        
        if (!string.IsNullOrEmpty(levelToLoad))
        {
            SceneManager.LoadScene(levelToLoad);
        }
        else
        {
            LogDebug("没有找到可继续的关卡，开始新游戏");
            StartNewGame();
            // 加载第一个关卡
            if (PublicData.LevelSequence.Length > 0)
            {
                SceneManager.LoadScene(PublicData.LevelSequence[0]);
            }
        }
    }
    
    /// <summary>
    /// 获取当前应该加载的关卡
    /// </summary>
    /// <returns>关卡场景名称</returns>
    public string GetCurrentLevelToLoad()
    {
        // 如果游戏从未开始过，返回空字符串
        if (!HasGameStarted())
        {
            LogDebug("游戏从未开始过");
            return "";
        }
        
        // 如果当前关卡为空，尝试从已完成关卡推断
        if (string.IsNullOrEmpty(currentLevel))
        {
            // 找到第一个未完成的关卡
            foreach (string level in PublicData.LevelSequence)
            {
                if (!completedLevels.Contains(level))
                {
                    LogDebug($"当前关卡为空，找到第一个未完成关卡: '{level}'");
                    return level;
                }
            }
            
            // 如果所有关卡都已完成，返回最后一个关卡
            if (PublicData.LevelSequence.Length > 0)
            {
                string lastLevel = PublicData.LevelSequence[PublicData.LevelSequence.Length - 1];
                LogDebug($"所有关卡都已完成，返回最后一个关卡: '{lastLevel}'");
                return lastLevel;
            }
        }
        else
        {
            // 检查当前关卡是否已完成
            if (completedLevels.Contains(currentLevel))
            {
                // 当前关卡已完成，找到下一个未完成的关卡
                int currentIndex = System.Array.IndexOf(PublicData.LevelSequence, currentLevel);
                if (currentIndex >= 0 && currentIndex < PublicData.LevelSequence.Length - 1)
                {
                    string nextLevel = PublicData.LevelSequence[currentIndex + 1];
                    LogDebug($"当前关卡 '{currentLevel}' 已完成，返回下一个关卡: '{nextLevel}'");
                    return nextLevel;
                }
            }
            else
            {
                // 当前关卡未完成，继续当前关卡
                LogDebug($"当前关卡 '{currentLevel}' 未完成，继续当前关卡");
                return currentLevel;
            }
        }
        
        LogDebug("没有找到合适的关卡，返回空字符串");
        return "";
    }
    
    /// <summary>
    /// 设置当前关卡
    /// </summary>
    /// <param name="levelName">关卡名称</param>
    public void SetCurrentLevel(string levelName)
    {
        if (string.IsNullOrEmpty(levelName))
        {
            LogDebug("设置当前关卡失败：关卡名称为空");
            return;
        }
        // 启动场景不参与记录当前关卡与开启进度标记
        if (levelName.Trim().ToLowerInvariant() == "startup")
        {
            LogDebug("忽略设置当前关卡为 startup（不标记进度、不存档）");
            return;
        }
        
        currentLevel = levelName;
        SaveProgress();
        LogDebug($"设置当前关卡为: '{levelName}'");
    }
    
    /// <summary>
    /// 标记关卡为已完成
    /// </summary>
    /// <param name="levelName">关卡名称</param>
    public void CompleteLevel(string levelName)
    {
        if (string.IsNullOrEmpty(levelName))
        {
            LogDebug("完成关卡失败：关卡名称为空");
            return;
        }
        
        completedLevels.Add(levelName);
        SaveProgress();
        LogDebug($"关卡 '{levelName}' 已标记为完成");
    }
    
    /// <summary>
    /// 检查关卡是否已完成
    /// </summary>
    /// <param name="levelName">关卡名称</param>
    /// <returns>是否已完成</returns>
    public bool IsLevelCompleted(string levelName)
    {
        return completedLevels.Contains(levelName);
    }
    
    /// <summary>
    /// 检查游戏是否已开始过
    /// </summary>
    /// <returns>是否已开始</returns>
    public bool HasGameStarted()
    {
        return PlayerPrefs.GetInt(GAME_STARTED_KEY, 0) == 1;
    }
    
    /// <summary>
    /// 获取已完成关卡数量
    /// </summary>
    /// <returns>已完成关卡数量</returns>
    public int GetCompletedLevelsCount()
    {
        return completedLevels.Count;
    }
    
    /// <summary>
    /// 获取总关卡数量
    /// </summary>
    /// <returns>总关卡数量</returns>
    public int GetTotalLevelsCount()
    {
        return PublicData.LevelSequence.Length;
    }
    
    /// <summary>
    /// 检查是否所有关卡都已完成
    /// </summary>
    /// <returns>是否所有关卡都已完成</returns>
    public bool AreAllLevelsCompleted()
    {
        if (PublicData.LevelSequence == null || PublicData.LevelSequence.Length == 0)
        {
            return false;
        }
        
        // 检查关卡序列中的每个关卡是否都已完成
        foreach (string level in PublicData.LevelSequence)
        {
            if (!completedLevels.Contains(level))
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 验证关卡名称是否有效
    /// </summary>
    /// <param name="levelName">关卡名称</param>
    /// <returns>是否有效</returns>
    private bool IsValidLevelName(string levelName)
    {
        if (string.IsNullOrEmpty(levelName))
        {
            return false;
        }
        
        if (PublicData.LevelSequence == null || PublicData.LevelSequence.Length == 0)
        {
            LogDebug("警告：关卡序列未初始化");
            return false;
        }
        
        // 检查关卡名称是否在有效序列中
        foreach (string validLevel in PublicData.LevelSequence)
        {
            if (string.Equals(levelName, validLevel, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 获取游戏进度百分比
    /// </summary>
    /// <returns>进度百分比 (0-100)</returns>
    public float GetProgressPercentage()
    {
        int total = GetTotalLevelsCount();
        if (total == 0) return 0f;
        
        return (float)GetCompletedLevelsCount() / total * 100f;
    }
    
    /// <summary>
    /// 获取当前关卡在序列中的索引
    /// </summary>
    /// <returns>关卡索引，如果未找到返回-1</returns>
    public int GetCurrentLevelIndex()
    {
        if (string.IsNullOrEmpty(currentLevel))
        {
            return -1;
        }
        
        return System.Array.IndexOf(PublicData.LevelSequence, currentLevel);
    }
    
    /// <summary>
    /// 获取下一个关卡名称
    /// </summary>
    /// <returns>下一个关卡名称，如果没有则返回空字符串</returns>
    public string GetNextLevelName()
    {
        int currentIndex = GetCurrentLevelIndex();
        if (currentIndex >= 0 && currentIndex < PublicData.LevelSequence.Length - 1)
        {
            return PublicData.LevelSequence[currentIndex + 1];
        }
        
        return "";
    }
    
    /// <summary>
    /// 检查是否有下一个关卡
    /// </summary>
    /// <returns>是否有下一个关卡</returns>
    public bool HasNextLevel()
    {
        return !string.IsNullOrEmpty(GetNextLevelName());
    }
    
    /// <summary>
    /// 清除所有进度数据
    /// </summary>
    [ContextMenu("清除所有进度")]
    public void ClearAllProgress()
    {
        LogDebug("清除所有进度数据");
        
        PlayerPrefs.DeleteKey(CURRENT_LEVEL_KEY);
        PlayerPrefs.DeleteKey(COMPLETED_LEVELS_KEY);
        PlayerPrefs.DeleteKey(GAME_STARTED_KEY);
        PlayerPrefs.Save();
        
        currentLevel = "";
        completedLevels.Clear();
        
        LogDebug("所有进度数据已清除");
    }
    
    /// <summary>
    /// 修复无效的存档数据
    /// </summary>
    [ContextMenu("修复存档数据")]
    public void FixInvalidSaveData()
    {
        LogDebug("开始修复存档数据");
        
        bool needsSave = false;
        
        // 检查并修复当前关卡
        if (!string.IsNullOrEmpty(currentLevel) && !IsValidLevelName(currentLevel))
        {
            LogDebug($"修复无效的当前关卡: '{currentLevel}' -> ''");
            currentLevel = "";
            needsSave = true;
        }
        
        // 检查并修复已完成的关卡列表
        var validCompletedLevels = new HashSet<string>();
        foreach (string level in completedLevels)
        {
            if (IsValidLevelName(level))
            {
                validCompletedLevels.Add(level);
            }
            else
            {
                LogDebug($"移除无效的已完成关卡: '{level}'");
                needsSave = true;
            }
        }
        completedLevels = validCompletedLevels;
        
        if (needsSave)
        {
            SaveProgress();
            LogDebug("存档数据已修复并保存");
        }
        else
        {
            LogDebug("存档数据无需修复");
        }
        
        // 更新按钮状态
        if (autoManageButtons)
        {
            UpdateButtonStates();
        }
    }
    
    /// <summary>
    /// 更新按钮状态
    /// 根据游戏进度自动设置按钮显示状态
    /// </summary>
    public void UpdateButtonStates()
    {
        // 检查关卡序列是否有效
        if (PublicData.LevelSequence == null || PublicData.LevelSequence.Length == 0)
        {
            LogDebug("错误：关卡序列未初始化，显示开始游戏按钮");
            ShowStartGameOnly();
            return;
        }
        
        // 检查是否所有关卡都已完成
        bool allLevelsCompleted = AreAllLevelsCompleted();
        
        // 如果有进度但不是所有关卡都完成，显示继续游戏按钮
        bool hasLevelProgress = GetCompletedLevelsCount() > 0;
        bool shouldShowContinue = hasLevelProgress && !allLevelsCompleted;
        
        LogDebug($"更新按钮状态 - 已完成关卡数: {GetCompletedLevelsCount()}, 总关卡数: {GetTotalLevelsCount()}, 所有关卡完成: {allLevelsCompleted}, 显示继续游戏: {shouldShowContinue}");
        LogDebug($"当前关卡: '{currentLevel}', 已完成关卡: [{string.Join(", ", completedLevels)}]");
        LogDebug($"关卡序列: [{string.Join(", ", PublicData.LevelSequence)}]");

        if (shouldShowContinue)
        {
            // 有进度但未全部完成，显示继续游戏和从头开始
            ShowContinueAndNewGame();
        }
        else
        {
            // 无进度或所有关卡都已完成，仅显示"开始游戏"
            ShowStartGameOnly();
        }
    }
    
    /// <summary>
    /// 仅显示开始游戏按钮
    /// </summary>
    private void ShowStartGameOnly()
    {
        if (startGameButton != null) 
        {
            startGameButton.gameObject.SetActive(true);
            LogDebug("显示开始游戏按钮");
        }
        if (startNewGameButton != null) 
        {
            startNewGameButton.gameObject.SetActive(false);
            LogDebug("隐藏从头开始按钮");
        }
        if (continueGameButton != null) 
        {
            continueGameButton.gameObject.SetActive(false);
            LogDebug("隐藏继续游戏按钮");
        }
    }
    
    /// <summary>
    /// 显示继续游戏和从头开始按钮
    /// </summary>
    private void ShowContinueAndNewGame()
    {
        if (startGameButton != null) 
        {
            startGameButton.gameObject.SetActive(false);
            LogDebug("隐藏开始游戏按钮");
        }
        if (startNewGameButton != null) 
        {
            startNewGameButton.gameObject.SetActive(true);
            LogDebug("显示从头开始按钮");
        }
        if (continueGameButton != null) 
        {
            continueGameButton.gameObject.SetActive(true);
            LogDebug("显示继续游戏按钮");
        }
    }

    /// <summary>
    /// 是否应该显示"继续游戏"按钮
    /// 规则：必须已开始游戏，有进度，但不是所有关卡都已完成；否则显示"开始游戏"。
    /// </summary>
    private bool ShouldShowContinue()
    {
        if (!HasGameStarted())
        {
            return false;
        }
        
        // 检查是否有进度
        bool hasProgress = GetCompletedLevelsCount() > 0;
        if (!hasProgress)
        {
            return false;
        }
        
        // 检查是否所有关卡都已完成
        bool allCompleted = AreAllLevelsCompleted();
        if (allCompleted)
        {
            return false; // 所有关卡都完成时显示"开始游戏"
        }
        
        return true; // 有进度但未全部完成时显示"继续游戏"
    }
    
    /// <summary>
    /// 显示继续游戏按钮
    /// </summary>
    private void ShowContinueGameButton()
    {
        if (continueGameButton != null)
        {
            continueGameButton.gameObject.SetActive(true);
            LogDebug("显示继续游戏按钮");
        }
        
        // 当需要仅显示继续时可隐藏开始按钮；在有存档的场景由外层逻辑同时显示
    }
    
    /// <summary>
    /// 显示开始游戏按钮
    /// </summary>
    private void ShowStartGameButton()
    {
        if (startGameButton != null)
        {
            startGameButton.gameObject.SetActive(true);
            LogDebug("显示开始游戏按钮");
        }
        
        if (startNewGameButton != null)
        {
            startNewGameButton.gameObject.SetActive(false);
            LogDebug("隐藏从头开始按钮");
        }
        if (continueGameButton != null)
        {
            continueGameButton.gameObject.SetActive(false);
            LogDebug("隐藏继续游戏按钮");
        }
    }
    
    /// <summary>
    /// 设置按钮引用
    /// </summary>
    /// <param name="startBtn">开始游戏按钮</param>
    /// <param name="continueBtn">继续游戏按钮</param>
    public void SetButtonReferences(Button startBtn, Button continueBtn)
    {
        startGameButton = startBtn;
        continueGameButton = continueBtn;
        
        LogDebug("已设置按钮引用");
        
        // 设置按钮事件
        SetupButtonEvents();
        
        // 更新按钮状态
        if (autoManageButtons)
        {
            UpdateButtonStates();
        }
    }

    /// <summary>
    /// 仅设置“从头开始”按钮引用（可选）。
    /// </summary>
    public void SetStartNewGameButton(Button startNewBtn)
    {
        startNewGameButton = startNewBtn;
        LogDebug("已设置从头开始按钮引用");
        if (autoManageButtons)
        {
            UpdateButtonStates();
        }
    }
    
    /// <summary>
    /// 设置按钮事件
    /// </summary>
    private void SetupButtonEvents()
    {
        if (startGameButton != null)
        {
            // 移除旧的事件监听器
            startGameButton.onClick.RemoveAllListeners();
            // 添加新的事件监听器
            startGameButton.onClick.AddListener(OnStartGameButtonClicked);
            LogDebug("已设置开始游戏按钮事件");
        }
        
        if (continueGameButton != null)
        {
            // 移除旧的事件监听器
            continueGameButton.onClick.RemoveAllListeners();
            // 添加新的事件监听器
            continueGameButton.onClick.AddListener(OnContinueGameButtonClicked);
            LogDebug("已设置继续游戏按钮事件");
        }
    }
    
    /// <summary>
    /// 开始游戏按钮点击事件
    /// </summary>
    private void OnStartGameButtonClicked()
    {
        LogDebug("开始游戏按钮被点击");
        StartNewGame();
        
        // 加载第一个关卡
        if (PublicData.LevelSequence.Length > 0)
        {
            SceneManager.LoadScene(PublicData.LevelSequence[0]);
        }
    }
    
    /// <summary>
    /// 继续游戏按钮点击事件
    /// </summary>
    private void OnContinueGameButtonClicked()
    {
        LogDebug("继续游戏按钮被点击");
        ContinueGame();
    }
    
    /// <summary>
    /// 手动切换按钮状态（用于测试）
    /// </summary>
    [ContextMenu("切换按钮状态")]
    public void ToggleButtonStates()
    {
        bool showContinue = ShouldShowContinue();
        if (showContinue)
        {
            ShowContinueGameButton();
        }
        else
        {
            ShowStartGameButton();
        }
    }
    
    /// <summary>
    /// 强制显示开始游戏按钮
    /// </summary>
    [ContextMenu("强制显示开始游戏按钮")]
    public void ForceShowStartGameButton()
    {
        ShowStartGameButton();
    }
    
    /// <summary>
    /// 强制显示继续游戏按钮
    /// </summary>
    [ContextMenu("强制显示继续游戏按钮")]
    public void ForceShowContinueGameButton()
    {
        ShowContinueGameButton();
    }
    
    /// <summary>
    /// 调试日志输出
    /// </summary>
    /// <param name="message">日志消息</param>
    private void LogDebug(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log($"[LevelProgressManager] {message}");
        }
    }
    
    /// <summary>
    /// 在Inspector中显示当前进度信息
    /// </summary>
    [ContextMenu("显示当前进度")]
    public void ShowCurrentProgress()
    {
        LogDebug("=== 当前进度信息 ===");
        LogDebug($"当前关卡: '{currentLevel}'");
        LogDebug($"已完成关卡: [{string.Join(", ", completedLevels)}]");
        LogDebug($"已完成关卡数量: {GetCompletedLevelsCount()}/{GetTotalLevelsCount()}");
        LogDebug($"进度百分比: {GetProgressPercentage():F1}%");
        LogDebug($"游戏已开始: {HasGameStarted()}");
        LogDebug($"下一个关卡: '{GetNextLevelName()}'");
        LogDebug("==================");
    }
}
