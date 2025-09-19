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
        LogDebug("=== 开始初始化进度数据 ===");
        
        // 添加PlayerPrefs可用性检查
        if (!IsPlayerPrefsAvailable())
        {
            LogDebug("警告：PlayerPrefs不可用，使用默认状态");
            ShowStartGameOnly();
            return;
        }
        
        LoadProgress();
        
        // 自动修复无效的存档数据
        FixInvalidSaveData();
        
        // 自动管理按钮状态
        if (autoManageButtons)
        {
            UpdateButtonStates();
        }
        
        LogDebug("=== 进度数据初始化完成 ===");
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
    
    // 规范化关卡名：去首尾空格并转为小写
    private static string NormalizeLevelName(string levelName)
    {
        return string.IsNullOrEmpty(levelName) ? "" : levelName.Trim().ToLowerInvariant();
    }
    
    /// <summary>
    /// 检查PlayerPrefs是否可用
    /// </summary>
    /// <returns>PlayerPrefs是否可用</returns>
    private bool IsPlayerPrefsAvailable()
    {
        try
        {
            // 测试写入和读取
            string testKey = "PlayerPrefsTest_" + System.DateTime.Now.Ticks;
            PlayerPrefs.SetString(testKey, "test");
            PlayerPrefs.Save();
            string result = PlayerPrefs.GetString(testKey, "");
            PlayerPrefs.DeleteKey(testKey);
            PlayerPrefs.Save();
            
            bool isAvailable = result == "test";
            LogDebug($"PlayerPrefs可用性测试: {isAvailable}");
            return isAvailable;
        }
        catch (System.Exception e)
        {
            LogDebug($"PlayerPrefs不可用，错误: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 从本地存储加载游戏进度
    /// </summary>
    private void LoadProgress()
    {
        // 加载当前关卡并规范化
        currentLevel = NormalizeLevelName(PlayerPrefs.GetString(CURRENT_LEVEL_KEY, ""));
        
        // 加载已完成的关卡列表（按规范化存储）
        string completedLevelsString = PlayerPrefs.GetString(COMPLETED_LEVELS_KEY, "");
        completedLevels = new HashSet<string>();
        
        if (!string.IsNullOrEmpty(completedLevelsString))
        {
            string[] levels = completedLevelsString.Split(',');
            foreach (string level in levels)
            {
                if (!string.IsNullOrEmpty(level))
                {
                    string trimmedLevel = NormalizeLevelName(level);
                    // 验证关卡名称是否在有效序列中
                    if (IsValidLevelName(trimmedLevel))
                    {
                        completedLevels.Add(trimmedLevel);
                    }
                    else
                    {
                        LogDebug($"警告：发现无效的关卡名称 '{level}', 规范化为'{trimmedLevel}'后仍无效，已忽略");
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
        // 保存当前关卡（已为规范化值）
        PlayerPrefs.SetString(CURRENT_LEVEL_KEY, currentLevel);
        
        // 保存已完成的关卡列表（规范化值）
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
            string[] levelSequence = PublicData.GetLevelSequence();
            if (levelSequence.Length > 0)
            {
                SceneManager.LoadScene(levelSequence[0]);
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
            string[] levelSequence = PublicData.GetLevelSequence();
            foreach (string level in levelSequence)
            {
                if (!completedLevels.Contains(NormalizeLevelName(level)))
                {
                    LogDebug($"当前关卡为空，找到第一个未完成关卡: '{level}'");
                    return level;
                }
            }
            
            // 如果所有关卡都已完成，返回最后一个关卡
            if (levelSequence.Length > 0)
            {
                string lastLevel = levelSequence[levelSequence.Length - 1];
                LogDebug($"所有关卡都已完成，返回最后一个关卡: '{lastLevel}'");
                return lastLevel;
            }
        }
        else
        {
            // 检查当前关卡是否已完成
            if (completedLevels.Contains(NormalizeLevelName(currentLevel)))
            {
                // 当前关卡已完成，找到下一个未完成的关卡
                string[] levelSequence = PublicData.GetLevelSequence();
                int currentIndex = System.Array.IndexOf(levelSequence, currentLevel);
                if (currentIndex >= 0 && currentIndex < levelSequence.Length - 1)
                {
                    string nextLevel = levelSequence[currentIndex + 1];
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
        
        currentLevel = NormalizeLevelName(levelName);
        SaveProgress();
        LogDebug($"设置当前关卡为: '{currentLevel}'");
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
        
        string normalized = NormalizeLevelName(levelName);
        completedLevels.Add(normalized);
        SaveProgress();
        LogDebug($"关卡 '{normalized}' 已标记为完成");
    }
    
    /// <summary>
    /// 检查关卡是否已完成
    /// </summary>
    /// <param name="levelName">关卡名称</param>
    /// <returns>是否已完成</returns>
    public bool IsLevelCompleted(string levelName)
    {
        return completedLevels.Contains(NormalizeLevelName(levelName));
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
        return PublicData.GetLevelSequence().Length;
    }
    
    /// <summary>
    /// 检查是否所有关卡都已完成
    /// </summary>
    /// <returns>是否所有关卡都已完成</returns>
    public bool AreAllLevelsCompleted()
    {
        string[] levelSequence = PublicData.GetLevelSequence();
        if (levelSequence == null || levelSequence.Length == 0)
        {
            return false;
        }
        
        // 检查关卡序列中的每个关卡是否都已完成
        foreach (string level in levelSequence)
        {
            if (!completedLevels.Contains(NormalizeLevelName(level)))
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
        
        string[] levelSequence = PublicData.GetLevelSequence();
        if (levelSequence == null || levelSequence.Length == 0)
        {
            LogDebug("警告：关卡序列未初始化");
            return false;
        }
        
        // 使用规范化比较
        string norm = NormalizeLevelName(levelName);
        foreach (string validLevel in levelSequence)
        {
            if (norm == NormalizeLevelName(validLevel))
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
        
        // 在序列中按规范化比较
        string normCurrent = NormalizeLevelName(currentLevel);
        string[] levelSequence = PublicData.GetLevelSequence();
        for (int i = 0; i < levelSequence.Length; i++)
        {
            if (NormalizeLevelName(levelSequence[i]) == normCurrent)
            {
                return i;
            }
        }
        return -1;
    }
    
    /// <summary>
    /// 获取下一个关卡名称
    /// </summary>
    /// <returns>下一个关卡名称，如果没有则返回空字符串</returns>
    public string GetNextLevelName()
    {
        int currentIndex = GetCurrentLevelIndex();
        string[] levelSequence = PublicData.GetLevelSequence();
        if (currentIndex >= 0 && currentIndex < levelSequence.Length - 1)
        {
            return levelSequence[currentIndex + 1];
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
        LogDebug("=== 按钮状态更新开始 ===");
        LogDebug($"PlayerPrefs可用性: {IsPlayerPrefsAvailable()}");
        LogDebug($"游戏已开始: {HasGameStarted()}");
        
        // 检查关卡序列是否有效（使用动态获取）
        string[] levelSequence = PublicData.GetLevelSequence();
        if (levelSequence == null || levelSequence.Length == 0)
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
        
        LogDebug($"已完成关卡数: {GetCompletedLevelsCount()}, 总关卡数: {GetTotalLevelsCount()}, 所有关卡完成: {allLevelsCompleted}, 显示继续游戏: {shouldShowContinue}");
        LogDebug($"当前关卡: '{currentLevel}', 已完成关卡: [{string.Join(", ", completedLevels)}]");
        LogDebug($"关卡序列: [{string.Join(", ", levelSequence)}]");
        
        // 检查按钮引用
        LogDebug($"按钮引用状态 - Start: {startGameButton?.name ?? "null"}, New: {startNewGameButton?.name ?? "null"}, Continue: {continueGameButton?.name ?? "null"}");

        if (shouldShowContinue)
        {
            // 有进度但未全部完成，显示继续游戏和从头开始
            LogDebug("决定显示：继续游戏 + 从头开始按钮");
            ShowContinueAndNewGame();
        }
        else
        {
            // 无进度或所有关卡都已完成，仅显示"开始游戏"
            LogDebug("决定显示：仅开始游戏按钮");
            ShowStartGameOnly();
        }
        
        LogDebug("=== 按钮状态更新完成 ===");
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
        
        // 先清空所有本地存档
        ClearAllProgress();
        
        // 清空后根据需要刷新按钮显示
        if (autoManageButtons)
        {
            UpdateButtonStates();
        }
        
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
        if (continueGameButton != null)
        {
            continueGameButton.gameObject.SetActive(true);
            LogDebug("强制显示继续游戏按钮");
        }
        else
        {
            LogDebug("错误：继续游戏按钮引用为空");
        }
    }
    
    /// <summary>
    /// 强制显示继续按钮（用于调试和兼容性）
    /// </summary>
    [ContextMenu("强制显示继续按钮")]
    public void ForceShowContinueButton()
    {
        if (continueGameButton != null)
        {
            continueGameButton.gameObject.SetActive(true);
            LogDebug("强制显示继续游戏按钮");
        }
        else
        {
            LogDebug("错误：继续游戏按钮引用为空");
        }
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
