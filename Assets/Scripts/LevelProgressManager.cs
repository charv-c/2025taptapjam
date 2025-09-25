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
    
    // JSON存储的进度数据
    private GameProgressData progressData;
    
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
        
        // 加载JSON进度数据
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
        
        // 在特定关卡中自动清空后续关卡的缓存
        AutoClearSubsequentLevelCache(scene.name);
        
        // 注意：不在这里清空当前关卡状态，因为这会破坏"继续游戏"功能
        // ClearCurrentLevelStateOnNewLevel 应该只在明确开始新游戏时调用
    }
    
    // 规范化关卡名：去首尾空格并转为小写
    private static string NormalizeLevelName(string levelName)
    {
        return string.IsNullOrEmpty(levelName) ? "" : levelName.Trim().ToLowerInvariant();
    }
    
    /// <summary>
    /// 自动清空后续关卡的缓存
    /// 在level1中清空level2、level3的缓存
    /// 在level2中清空level3的缓存
    /// </summary>
    /// <param name="currentSceneName">当前场景名称</param>
    private void AutoClearSubsequentLevelCache(string currentSceneName)
    {
        if (string.IsNullOrEmpty(currentSceneName))
        {
            return;
        }
        
        string normalizedSceneName = NormalizeLevelName(currentSceneName);
        LogDebug($"检查是否需要清空后续关卡缓存，当前场景: {normalizedSceneName}");
        
        // 检查是否是level场景
        if (!normalizedSceneName.StartsWith("level"))
        {
            LogDebug("当前场景不是level场景，跳过缓存清理");
            return;
        }
        
        // 获取关卡序列
        string[] levelSequence = PublicData.GetLevelSequence();
        if (levelSequence == null || levelSequence.Length == 0)
        {
            LogDebug("关卡序列为空，无法进行缓存清理");
            return;
        }
        
        // 检查当前关卡是否在序列中
        bool isCurrentLevelInSequence = false;
        for (int i = 0; i < levelSequence.Length; i++)
        {
            if (NormalizeLevelName(levelSequence[i]) == normalizedSceneName)
            {
                isCurrentLevelInSequence = true;
                break;
            }
        }
        
        if (!isCurrentLevelInSequence)
        {
            LogDebug($"当前场景 {normalizedSceneName} 不在关卡序列中，跳过缓存清理");
            return;
        }
        
        // 根据当前关卡清空后续关卡缓存
        switch (normalizedSceneName)
        {
            case "level1":
                LogDebug("在level1中，清空level2和level3的缓存");
                PublicData.ClearLevelAndSubsequentCache("level2");
                PublicData.ClearLevelAndSubsequentCache("level3");
                break;
                
            case "level2":
                LogDebug("在level2中，清空level3的缓存");
                PublicData.ClearLevelAndSubsequentCache("level3");
                break;
                
            case "level3":
                LogDebug("在level3中，无需清空后续关卡缓存");
                break;
                
            default:
                // 对于其他level场景，也清空后续关卡的缓存
                LogDebug($"在 {normalizedSceneName} 中，清空后续关卡的缓存");
                PublicData.ClearLevelAndSubsequentCache(normalizedSceneName);
                break;
        }
    }
    
    /// <summary>
    /// 在新关卡开始时清空当前关卡的存储状态
    /// </summary>
    /// <param name="currentSceneName">当前场景名称</param>
    private void ClearCurrentLevelStateOnNewLevel(string currentSceneName)
    {
        if (string.IsNullOrEmpty(currentSceneName) || progressData == null)
        {
            return;
        }
        
        string normalizedSceneName = NormalizeLevelName(currentSceneName);
        LogDebug($"检查是否需要清空关卡 {normalizedSceneName} 的存储状态");
        
        // 检查是否是level场景
        if (!normalizedSceneName.StartsWith("level"))
        {
            LogDebug("当前场景不是level场景，跳过状态清理");
            return;
        }
        
        // 获取关卡序列
        string[] levelSequence = PublicData.GetLevelSequence();
        if (levelSequence == null || levelSequence.Length == 0)
        {
            LogDebug("关卡序列为空，无法进行状态清理");
            return;
        }
        
        // 检查当前关卡是否在序列中
        bool isCurrentLevelInSequence = false;
        for (int i = 0; i < levelSequence.Length; i++)
        {
            if (NormalizeLevelName(levelSequence[i]) == normalizedSceneName)
            {
                isCurrentLevelInSequence = true;
                break;
            }
        }
        
        if (!isCurrentLevelInSequence)
        {
            LogDebug($"当前场景 {normalizedSceneName} 不在关卡序列中，跳过状态清理");
            return;
        }
        
        // 清空当前关卡的状态数据
        if (progressData.GetLevelState(normalizedSceneName) != null)
        {
            LogDebug($"清空关卡 {normalizedSceneName} 的存储状态");
            progressData.ClearLevelState(normalizedSceneName);
            SaveProgress();
        }
        else
        {
            LogDebug($"关卡 {normalizedSceneName} 没有存储的状态数据");
        }
    }

    /// <summary>
    /// 从JSON文件加载游戏进度
    /// </summary>
    private void LoadProgress()
    {
        try
        {
            // 从JSON文件加载进度数据
            progressData = JsonStorageManager.LoadGameProgress();
            
            if (progressData == null)
            {
                LogDebug("警告：JSON进度数据加载失败，创建新的进度数据");
                progressData = new GameProgressData();
                progressData.InitializeVersion(); // 初始化版本信息
            }
            else
            {
                // 检查存档版本兼容性
                CheckAndUpgradeProgressVersion();
            }
            
            // 验证并规范化数据
            ValidateAndNormalizeProgressData();
            
            LogDebug($"JSON加载成功 - 当前关卡: '{progressData.currentLevel}', 已完成关卡: [{string.Join(", ", progressData.completedLevels)}]");
            LogDebug($"JSON加载成功 - 游戏已开始: {progressData.gameStarted}, 最后保存时间: {progressData.lastSaveTime}");
            LogDebug($"JSON加载成功 - levelStates字典大小: {progressData.levelStates.Count}");
            if (progressData.levelStates.Count > 0)
            {
                var levelNames = progressData.levelStates.ConvertAll(e => e.levelName);
                LogDebug($"JSON加载成功 - 关卡状态列表中的关卡: [{string.Join(", ", levelNames)}]");
                foreach (var entry in progressData.levelStates)
                {
                    LogDebug($"JSON加载成功 - 关卡 {entry.levelName}: 物体数量 {entry.stateData.objectStates.Count}");
                }
            }
            LogDebug($"关卡序列: [{string.Join(", ", PublicData.GetLevelSequence())}]");
        }
        catch (System.Exception e)
        {
            LogDebug($"加载进度数据时发生错误: {e.Message}，创建新的进度数据");
            progressData = new GameProgressData();
        }
    }
    
    /// <summary>
    /// 检查并升级存档版本
    /// </summary>
    private void CheckAndUpgradeProgressVersion()
    {
        try
        {
            var compatibility = progressData.CheckVersionCompatibility();
            
            LogDebug($"存档版本检查: 当前版本={progressData.saveVersion}, 兼容性={compatibility}");
            
            switch (compatibility)
            {
                case SaveVersionCompatibility.Current:
                    LogDebug("存档版本是最新的，无需升级");
                    break;
                    
                case SaveVersionCompatibility.Legacy:
                    LogDebug($"检测到遗留版本存档 ({progressData.saveVersion})，开始升级...");
                    progressData.UpgradeToCurrentVersion();
                    SaveProgress(); // 保存升级后的存档
                    LogDebug("存档版本升级完成");
                    break;
                    
                case SaveVersionCompatibility.Compatible:
                    LogDebug($"存档版本兼容 ({progressData.saveVersion})，更新到当前版本");
                    progressData.UpgradeToCurrentVersion();
                    SaveProgress(); // 保存升级后的存档
                    break;
                    
                case SaveVersionCompatibility.Incompatible:
                    LogDebug($"存档版本 {progressData.saveVersion} 不兼容，重置进度数据");
                    
                    // 可以选择：1) 抛出异常 2) 重置存档 3) 提示用户
                    // 这里选择重置存档（更用户友好）
                    progressData = new GameProgressData();
                    progressData.InitializeVersion();
                    
                    GameLogger.LogUser($"检测到不兼容的存档版本，已重置游戏进度");
                    break;
            }
        }
        catch (System.Exception e)
        {
            LogDebug($"版本检查过程中发生错误: {e.Message}，重置进度数据");
            progressData = new GameProgressData();
            progressData.InitializeVersion();
        }
    }
    
    /// <summary>
    /// 验证并规范化进度数据
    /// </summary>
    private void ValidateAndNormalizeProgressData()
    {
        // 规范化当前关卡名称
        progressData.currentLevel = NormalizeLevelName(progressData.currentLevel);
        
        // 验证当前关卡是否有效
        if (!string.IsNullOrEmpty(progressData.currentLevel) && !IsValidLevelName(progressData.currentLevel))
        {
            LogDebug($"警告：当前关卡 '{progressData.currentLevel}' 无效，已重置为空");
            progressData.currentLevel = "";
        }
        
        // 验证并规范化已完成的关卡列表
        var validCompletedLevels = new List<string>();
        foreach (string level in progressData.completedLevels)
        {
            if (!string.IsNullOrEmpty(level))
            {
                string trimmedLevel = NormalizeLevelName(level);
                // 验证关卡名称是否在有效序列中
                if (IsValidLevelName(trimmedLevel))
                {
                    validCompletedLevels.Add(trimmedLevel);
                }
                else
                {
                    LogDebug($"警告：发现无效的关卡名称 '{level}', 规范化为'{trimmedLevel}'后仍无效，已忽略");
                }
            }
        }
        progressData.completedLevels = validCompletedLevels;
    }
    
    /// <summary>
    /// 保存游戏进度到JSON文件
    /// </summary>
    private void SaveProgress()
    {
        try
        {
            if (progressData == null)
            {
                progressData = new GameProgressData();
                progressData.InitializeVersion(); // 初始化版本信息
            }
            
            // 确保使用最新的存档版本
            if (string.IsNullOrEmpty(progressData.saveVersion) || 
                progressData.saveVersion != GameProgressData.CURRENT_SAVE_VERSION)
            {
                progressData.saveVersion = GameProgressData.CURRENT_SAVE_VERSION;
                LogDebug($"更新存档版本为: {GameProgressData.CURRENT_SAVE_VERSION}");
            }
            
            // 更新保存时间
            progressData.UpdateSaveTime();
            
            // 保存前验证数据
            LogDebug($"保存前验证 - levelStates字典大小: {progressData.levelStates.Count}");
            if (progressData.levelStates.Count > 0)
            {
                var levelNames = progressData.levelStates.ConvertAll(e => e.levelName);
                LogDebug($"保存前验证 - 关卡状态列表中的关卡: [{string.Join(", ", levelNames)}]");
                foreach (var entry in progressData.levelStates)
                {
                    LogDebug($"保存前验证 - 关卡 {entry.levelName}: 物体数量 {entry.stateData.objectStates.Count}");
                }
            }
            
            // 保存到JSON文件
            bool saveSuccess = JsonStorageManager.SaveGameProgress(progressData);
            
            if (saveSuccess)
            {
                LogDebug($"JSON保存成功 - 当前关卡: '{progressData.currentLevel}', 已完成关卡: [{string.Join(", ", progressData.completedLevels)}]");
                LogDebug($"JSON保存成功 - 游戏已开始: {progressData.gameStarted}, 保存时间: {progressData.lastSaveTime}");
                LogDebug($"JSON保存成功 - levelStates数量: {progressData.levelStates.Count}");
            }
            else
            {
                LogDebug("JSON保存失败");
            }
        }
        catch (System.Exception e)
        {
            LogDebug($"保存进度时发生错误: {e.Message}");
        }
    }
    
    /// <summary>
    /// 开始新游戏（重置进度）
    /// </summary>
    public void StartNewGame()
    {
        LogDebug("开始新游戏，重置所有进度");
        
        // 重置进度数据
        progressData.Reset();
        
        // 保存重置后的数据
        SaveProgress();
        
        // 清除PublicData的关卡序列缓存
        PublicData.ClearLevelSequenceCache();
        
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
        // 确保GameBootstrap已初始化
        GameBootstrap.EnsureInitialized();
        
        // 如果所有关卡都已完成，则直接进入结算页，展示谢幕逻辑
        try
        {
            if (AreAllLevelsCompleted())
            {
                LogDebug("检测到所有关卡均已完成，直接进入 EndLevel 谢幕流程");
                UnityEngine.SceneManagement.SceneManager.LoadScene("EndLevel");
                return;
            }
        }
        catch (System.Exception) { }

        string levelToLoad = GetCurrentLevelToLoad();
        LogDebug($"继续游戏，加载关卡: '{levelToLoad}'");
        
        if (!string.IsNullOrEmpty(levelToLoad))
        {
            // 验证场景是否存在
            if (IsSceneInBuildSettings(levelToLoad))
            {
                SceneManager.LoadScene(levelToLoad);
            }
            else
            {
                LogDebug($"场景 {levelToLoad} 不在构建设置中，回退到第一关");
                LoadFirstLevelSafely();
            }
        }
        else
        {
            LogDebug("没有找到可继续的关卡，开始新游戏");
            StartNewGame();
            LoadFirstLevelSafely();
        }
    }
    
    /// <summary>
    /// 安全地加载第一关
    /// </summary>
    private void LoadFirstLevelSafely()
    {
        string[] levelSequence = PublicData.GetLevelSequence();
        if (levelSequence != null && levelSequence.Length > 0)
        {
            string firstLevel = levelSequence[0];
            if (IsSceneInBuildSettings(firstLevel))
            {
                SceneManager.LoadScene(firstLevel);
            }
            else
            {
                LogDebug($"第一关 {firstLevel} 不在构建设置中，尝试加载 level1");
                if (IsSceneInBuildSettings("level1"))
                {
                    SceneManager.LoadScene("level1");
                }
                else
                {
                    LogDebug("错误：无法找到可用的关卡场景！");
                }
            }
        }
        else
        {
            LogDebug("关卡序列为空，尝试加载 level1");
            if (IsSceneInBuildSettings("level1"))
            {
                SceneManager.LoadScene("level1");
            }
            else
            {
                LogDebug("错误：无法找到可用的关卡场景！");
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
    /// 获取当前应该加载的关卡
    /// </summary>
    /// <returns>关卡场景名称</returns>
    public string GetCurrentLevelToLoad()
    {
        if (progressData == null)
        {
            LogDebug("进度数据为空");
            return "";
        }
        
        // 如果游戏从未开始过，返回空字符串
        if (!progressData.gameStarted)
        {
            LogDebug("游戏从未开始过");
            return "";
        }
        
        // 如果当前关卡为空，尝试从已完成关卡推断
        if (string.IsNullOrEmpty(progressData.currentLevel))
        {
            // 找到第一个未完成的关卡
            string[] levelSequence = PublicData.GetLevelSequence();
            foreach (string level in levelSequence)
            {
                if (!progressData.IsLevelCompleted(NormalizeLevelName(level)))
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
            if (progressData.IsLevelCompleted(NormalizeLevelName(progressData.currentLevel)))
            {
                // 当前关卡已完成，找到下一个未完成的关卡
                string[] levelSequence = PublicData.GetLevelSequence();
                int currentIndex = System.Array.IndexOf(levelSequence, progressData.currentLevel);
                if (currentIndex >= 0 && currentIndex < levelSequence.Length - 1)
                {
                    string nextLevel = levelSequence[currentIndex + 1];
                    LogDebug($"当前关卡 '{progressData.currentLevel}' 已完成，返回下一个关卡: '{nextLevel}'");
                    return nextLevel;
                }
            }
            else
            {
                // 当前关卡未完成，继续当前关卡
                LogDebug($"当前关卡 '{progressData.currentLevel}' 未完成，继续当前关卡");
                return progressData.currentLevel;
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
        if (progressData == null)
        {
            progressData = new GameProgressData();
        }
        
        if (string.IsNullOrEmpty(levelName))
        {
            LogDebug("设置当前关卡失败：关卡名称为空");
            return;
        }
        // 启动场景不参与记录当前关卡与开启进度标记
        string norm = levelName.Trim().ToLowerInvariant();
        if (norm == "startup" || norm == "endlevel")
        {
            LogDebug("忽略设置当前关卡为 startup/endlevel（不标记进度、不存档）");
            return;
        }
        
        progressData.currentLevel = NormalizeLevelName(levelName);
        progressData.gameStarted = true; // 设置当前关卡时标记游戏已开始
        SaveProgress();
        LogDebug($"设置当前关卡为: '{progressData.currentLevel}'");
    }
    
    /// <summary>
    /// 标记关卡为已完成
    /// </summary>
    /// <param name="levelName">关卡名称</param>
    public void CompleteLevel(string levelName)
    {
        if (progressData == null)
        {
            progressData = new GameProgressData();
        }
        
        if (string.IsNullOrEmpty(levelName))
        {
            LogDebug("完成关卡失败：关卡名称为空");
            return;
        }
        
        string normalized = NormalizeLevelName(levelName);
        progressData.AddCompletedLevel(normalized);
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
        if (progressData == null) return false;
        return progressData.IsLevelCompleted(NormalizeLevelName(levelName));
    }
    
    /// <summary>
    /// 检查游戏是否已开始过
    /// </summary>
    /// <returns>是否已开始</returns>
    public bool HasGameStarted()
    {
        if (progressData == null) return false;
        return progressData.gameStarted;
    }
    
    /// <summary>
    /// 获取已完成关卡数量
    /// </summary>
    /// <returns>已完成关卡数量</returns>
    public int GetCompletedLevelsCount()
    {
        if (progressData == null) return 0;
        return progressData.completedLevels.Count;
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
        if (progressData == null) return false;
        
        string[] levelSequence = PublicData.GetLevelSequence();
        if (levelSequence == null || levelSequence.Length == 0)
        {
            return false;
        }
        
        // 检查关卡序列中的每个关卡是否都已完成
        foreach (string level in levelSequence)
        {
            if (!progressData.IsLevelCompleted(NormalizeLevelName(level)))
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
        if (progressData == null || string.IsNullOrEmpty(progressData.currentLevel))
        {
            return -1;
        }
        
        // 在序列中按规范化比较
        string normCurrent = NormalizeLevelName(progressData.currentLevel);
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
        
        // 删除JSON文件
        JsonStorageManager.DeleteGameProgress();
        
        // 重置内存中的进度数据
        progressData = new GameProgressData();
        
        // 清除PublicData的关卡序列缓存，确保重新加载时使用最新数据
        PublicData.ClearLevelSequenceCache();
        
        LogDebug("所有进度数据已清除");
    }
    
    /// <summary>
    /// 保存关卡状态数据
    /// </summary>
    /// <param name="levelName">关卡名称</param>
    /// <param name="stateData">状态数据</param>
    public void SaveLevelState(string levelName, GameProgressData.LevelStateData stateData)
    {
        if (progressData == null)
        {
            progressData = new GameProgressData();
            LogDebug("创建新的 progressData 对象");
        }
        
        if (string.IsNullOrEmpty(levelName) || stateData == null)
        {
            LogDebug("保存关卡状态失败：关卡名称或状态数据为空");
            return;
        }
        
        string normalizedLevelName = NormalizeLevelName(levelName);
        LogDebug($"保存关卡状态 - 原始名称: '{levelName}', 标准化名称: '{normalizedLevelName}'");
        
        // 确保 stateData 内部的 levelName 也被设置
        if (stateData != null)
        {
            stateData.levelName = normalizedLevelName;
        }

        progressData.SetLevelState(normalizedLevelName, stateData);
        LogDebug($"已将状态数据存入 progressData.levelStates，当前字典大小: {progressData.levelStates.Count}");
        
        // 验证数据是否正确存入
        var verifyData = progressData.GetLevelState(normalizedLevelName);
        if (verifyData != null)
        {
            LogDebug($"验证成功：字典中确实存在关卡 {normalizedLevelName} 的数据，物体数量: {verifyData.objectStates.Count}");
        }
        else
        {
            LogDebug($"验证失败：字典中没有找到关卡 {normalizedLevelName} 的数据！");
        }
        
        SaveProgress();
        LogDebug($"关卡 {normalizedLevelName} 的状态数据已保存到JSON文件");
    }
    
    /// <summary>
    /// 加载关卡状态数据
    /// </summary>
    /// <param name="levelName">关卡名称</param>
    /// <returns>关卡状态数据，如果不存在则返回null</returns>
    public GameProgressData.LevelStateData LoadLevelState(string levelName)
    {
        if (progressData == null)
        {
            LogDebug("进度数据为空，尝试重新加载进度数据");
            LoadProgress(); // 尝试重新加载
            
            if (progressData == null)
            {
                LogDebug("重新加载后进度数据仍为空，无法加载关卡状态");
                return null;
            }
        }
        
        if (string.IsNullOrEmpty(levelName))
        {
            LogDebug("关卡名称为空，无法加载关卡状态");
            return null;
        }
        
        string normalizedLevelName = NormalizeLevelName(levelName);
        LogDebug($"加载关卡状态 - 原始名称: '{levelName}', 标准化名称: '{normalizedLevelName}'");
        LogDebug($"当前 progressData.levelStates 字典大小: {progressData.levelStates.Count}");
        
        if (progressData.levelStates.Count > 0)
        {
            var levelNames = progressData.levelStates.ConvertAll(e => e.levelName);
            LogDebug($"关卡状态列表中的关卡: [{string.Join(", ", levelNames)}]");
        }
        
        var stateData = progressData.GetLevelState(normalizedLevelName);
        
        if (stateData != null)
        {
            LogDebug($"关卡 {normalizedLevelName} 的状态数据已加载 - 物体数量: {stateData.objectStates.Count}");
        }
        else
        {
            LogDebug($"关卡 {normalizedLevelName} 没有保存的状态数据");
        }
        
        return stateData;
    }
    
    /// <summary>
    /// 清除指定关卡的状态数据
    /// </summary>
    /// <param name="levelName">关卡名称</param>
    public void ClearLevelState(string levelName)
    {
        if (progressData == null)
        {
            return;
        }
        
        if (string.IsNullOrEmpty(levelName))
        {
            LogDebug("关卡名称为空，无法清除关卡状态");
            return;
        }
        
        string normalizedLevelName = NormalizeLevelName(levelName);
        progressData.ClearLevelState(normalizedLevelName);
        SaveProgress();
        LogDebug($"关卡 {normalizedLevelName} 的状态数据已清除");
    }
    
    /// <summary>
    /// 清除所有关卡的状态数据
    /// </summary>
    public void ClearAllLevelStates()
    {
        if (progressData == null)
        {
            return;
        }
        
        progressData.ClearAllLevelStates();
        SaveProgress();
        LogDebug("所有关卡的状态数据已清除");
    }
    
    /// <summary>
    /// 清除指定关卡及后续关卡的状态数据
    /// </summary>
    /// <param name="levelName">关卡名称</param>
    public void ClearLevelAndSubsequentStates(string levelName)
    {
        if (progressData == null)
        {
            return;
        }
        
        if (string.IsNullOrEmpty(levelName))
        {
            LogDebug("关卡名称为空，无法清除关卡状态");
            return;
        }
        
        string[] levelSequence = PublicData.GetLevelSequence();
        progressData.ClearLevelAndSubsequentStates(levelName, levelSequence);
        SaveProgress();
        LogDebug($"关卡 {levelName} 及后续关卡的状态数据已清除");
    }
    
    /// <summary>
    /// 检查是否有任何关卡状态数据
    /// </summary>
    /// <returns>是否有关卡状态数据</returns>
    public bool HasAnyLevelStates()
    {
        if (progressData == null)
        {
            return false;
        }
        
        return progressData.HasAnyLevelStates();
    }
    
    /// <summary>
    /// 手动清空当前关卡及后续关卡的缓存
    /// </summary>
    [ContextMenu("清空当前关卡及后续关卡缓存")]
    public void ClearCurrentAndSubsequentLevelCache()
    {
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (!string.IsNullOrEmpty(currentSceneName))
        {
            LogDebug($"手动清空当前关卡 {currentSceneName} 及后续关卡的缓存");
            PublicData.ClearLevelAndSubsequentCache(currentSceneName);
        }
        else
        {
            LogDebug("无法获取当前场景名称");
        }
    }
    
    /// <summary>
    /// 手动清空所有关卡状态
    /// </summary>
    [ContextMenu("清空所有关卡状态")]
    public void ClearAllLevelStatesManually()
    {
        ClearAllLevelStates();
    }
    
    /// <summary>
    /// 手动清空当前关卡状态
    /// </summary>
    [ContextMenu("清空当前关卡状态")]
    public void ClearCurrentLevelStateManually()
    {
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (!string.IsNullOrEmpty(currentSceneName))
        {
            ClearLevelState(currentSceneName);
        }
        else
        {
            LogDebug("无法获取当前场景名称");
        }
    }
    
    /// <summary>
    /// 测试关卡状态功能
    /// </summary>
    [ContextMenu("测试关卡状态功能")]
    public void TestLevelStateFunctionality()
    {
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        LogDebug($"=== 测试关卡状态功能 - 当前场景: {currentSceneName} ===");
        
        // 创建测试状态数据
        var testStateData = new GameProgressData.LevelStateData();
        testStateData.levelName = currentSceneName;
        testStateData.objectStates.Add(new GameProgressData.GameObjectState
        {
            objectName = "TestObject",
            isActive = true,
            highlightEnabled = true
        });
        testStateData.broadcastHistory.Add("测试广播消息");
        testStateData.availableStrings.Add("测试字符串");
        testStateData.currentSeason = "春季";
        testStateData.collectedStrings.Add("收集的字符串");
        testStateData.completedTargets.Add("测试目标");
        testStateData.currentTargetList.Add("当前目标");
        
        LogDebug("1. 保存测试关卡状态数据");
        SaveLevelState(currentSceneName, testStateData);
        
        LogDebug("2. 加载关卡状态数据");
        var loadedState = LoadLevelState(currentSceneName);
        if (loadedState != null)
        {
            LogDebug($"加载成功 - 物体数量: {loadedState.objectStates.Count}, 广播数量: {loadedState.broadcastHistory.Count}");
        }
        else
        {
            LogDebug("加载失败");
        }
        
        LogDebug("3. 检查是否有任何关卡状态");
        LogDebug($"有关卡状态: {HasAnyLevelStates()}");
        
        LogDebug("4. 清空当前关卡状态");
        ClearLevelState(currentSceneName);
        
        LogDebug("5. 再次检查是否有任何关卡状态");
        LogDebug($"有关卡状态: {HasAnyLevelStates()}");
        
        LogDebug("=== 关卡状态功能测试完成 ===");
    }
    
    /// <summary>
    /// 修复无效的存档数据
    /// </summary>
    [ContextMenu("修复存档数据")]
    public void FixInvalidSaveData()
    {
        LogDebug("开始修复存档数据");
        
        if (progressData == null)
        {
            progressData = new GameProgressData();
        }
        
        bool needsSave = false;
        
        // 检查并修复当前关卡
        if (!string.IsNullOrEmpty(progressData.currentLevel) && !IsValidLevelName(progressData.currentLevel))
        {
            LogDebug($"修复无效的当前关卡: '{progressData.currentLevel}' -> ''");
            progressData.currentLevel = "";
            needsSave = true;
        }
        
        // 检查并修复已完成的关卡列表
        var validCompletedLevels = new List<string>();
        foreach (string level in progressData.completedLevels)
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
        progressData.completedLevels = validCompletedLevels;
        
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
        
        // 新规则：只有当进度已进入 level2 及以后才显示“继续游戏”
        bool showContinue = HasEnteredLevel2OrLater();
        LogDebug($"是否已进入level2及以后: {showContinue}");

        if (showContinue)
        {
            // 进入了level2或更后关卡，显示“继续游戏”和“从头开始”
            LogDebug("决定显示：继续游戏 + 从头开始按钮");
            ShowContinueAndNewGame();
        }
        else
        {
            // 尚未进入level2，仅显示“开始游戏”
            LogDebug("决定显示：仅开始游戏按钮");
            ShowStartGameOnly();
        }
        
        LogDebug("=== 按钮状态更新完成 ===");
    }

    /// <summary>
    /// 是否已进入 level2 及以后（用于决定是否显示“继续游戏”）
    /// 规则：当前关卡或任一已完成关卡/关卡状态在关卡序列中的索引 >= 1
    /// </summary>
    private bool HasEnteredLevel2OrLater()
    {
        try
        {
            if (progressData == null)
            {
                LoadProgress();
            }

            string[] sequence = PublicData.GetLevelSequence();
            if (sequence == null || sequence.Length == 0) return false;

            // 规范化比较函数
            System.Func<string, int> getIndex = (string levelName) =>
            {
                if (string.IsNullOrEmpty(levelName)) return -1;
                string norm = NormalizeLevelName(levelName);
                for (int i = 0; i < sequence.Length; i++)
                {
                    if (NormalizeLevelName(sequence[i]) == norm)
                    {
                        return i;
                    }
                }
                return -1;
            };

            // 1) 当前关卡
            int currentIdx = getIndex(progressData.currentLevel);
            if (currentIdx >= 1) return true;

            // 2) 已完成关卡
            if (progressData.completedLevels != null)
            {
                foreach (var lvl in progressData.completedLevels)
                {
                    if (getIndex(lvl) >= 1) return true;
                }
            }

            // 3) 关卡内状态（曾经到达过并产生状态）
            if (progressData.levelStates != null)
            {
                foreach (var entry in progressData.levelStates)
                {
                    if (getIndex(entry.levelName) >= 1) return true;
                }
            }
        }
        catch (System.Exception e)
        {
            LogDebug($"HasEnteredLevel2OrLater 检测异常: {e.Message}");
        }

        return false;
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

    // ShouldShowContinue 方法由于逻辑被简化，可以移除或保留为内部参考
    /*
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
    */
    
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

        // 绑定事件
        SetupButtonEvents();
        
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
        
        if (startNewGameButton != null)
        {
            // 为“从头开始”按钮添加事件监听
            startNewGameButton.onClick.RemoveAllListeners();
            startNewGameButton.onClick.AddListener(OnStartGameButtonClicked);
            LogDebug("已设置从头开始按钮事件");
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
    public void OnStartGameButtonClicked()
    {
        LogDebug("开始游戏按钮被点击");
        
        // 确保GameBootstrap已初始化
        GameBootstrap.EnsureInitialized();
        
        // 先清空所有本地存档
        ClearAllProgress();
        
        // 清空后根据需要刷新按钮显示
        if (autoManageButtons)
        {
            UpdateButtonStates();
        }
        
        // 安全地加载第一个关卡
        LoadFirstLevelSafely();
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
        bool showContinue = HasEnteredLevel2OrLater();
        if (showContinue)
        {
            ShowContinueAndNewGame();
        }
        else
        {
            ShowStartGameOnly();
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
            GameLogger.LogDev($"[LevelProgressManager] {message}");
        }
    }
    
    /// <summary>
    /// 在Inspector中显示当前进度信息
    /// </summary>
    [ContextMenu("显示当前进度")]
    public void ShowCurrentProgress()
    {
        LogDebug("=== 当前进度信息 ===");
        if (progressData != null)
        {
            LogDebug($"当前关卡: '{progressData.currentLevel}'");
            LogDebug($"已完成关卡: [{string.Join(", ", progressData.completedLevels)}]");
            LogDebug($"已完成关卡数量: {GetCompletedLevelsCount()}/{GetTotalLevelsCount()}");
            LogDebug($"进度百分比: {GetProgressPercentage():F1}%");
            LogDebug($"游戏已开始: {progressData.gameStarted}");
            LogDebug($"创建时间: {progressData.createTime}");
            LogDebug($"最后保存时间: {progressData.lastSaveTime}");
            LogDebug($"版本: {progressData.version}");
            LogDebug($"关卡状态数据: {progressData.GetLevelStatesSummary()}");
        }
        else
        {
            LogDebug("进度数据为空");
        }
        LogDebug($"下一个关卡: '{GetNextLevelName()}'");
        LogDebug($"是否有任何关卡状态: {HasAnyLevelStates()}");
        LogDebug($"JSON存储信息:\n{JsonStorageManager.GetStorageInfo()}");
        LogDebug("==================");
    }
}
