using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景加载测试辅助工具
/// 用于测试清空进度后的场景加载功能
/// </summary>
public class SceneLoadingTestHelper : MonoBehaviour
{
    [Header("测试设置")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool autoTestOnStart = false;
    
    private void Start()
    {
        if (autoTestOnStart)
        {
            TestSceneLoadingAfterProgressClear();
        }
    }
    
    /// <summary>
    /// 测试清空进度后的场景加载
    /// </summary>
    [ContextMenu("测试清空进度后场景加载")]
    public void TestSceneLoadingAfterProgressClear()
    {
        LogDebug("=== 开始测试清空进度后的场景加载 ===");
        
        // 1. 清空所有进度
        LogDebug("步骤1：清空所有进度");
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.ClearAllProgress();
            LogDebug("✓ 已清空LevelProgressManager进度");
        }
        
        GameStateManager.ClearAllGameStates();
        LogDebug("✓ 已清空GameStateManager状态");
        
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        LogDebug("✓ 已清空PlayerPrefs");
        
        // 2. 测试PublicData.GetLevelSequence()
        LogDebug("步骤2：测试PublicData.GetLevelSequence()");
        string[] levelSequence = PublicData.GetLevelSequence();
        if (levelSequence != null && levelSequence.Length > 0)
        {
            LogDebug($"✓ 关卡序列获取成功: [{string.Join(", ", levelSequence)}]");
        }
        else
        {
            LogDebug("✗ 关卡序列获取失败或为空");
        }
        
        // 3. 测试GameBootstrap初始化
        LogDebug("步骤3：测试GameBootstrap初始化");
        GameBootstrap.EnsureInitialized();
        if (GameBootstrap.IsInitialized)
        {
            LogDebug("✓ GameBootstrap初始化成功");
        }
        else
        {
            LogDebug("✗ GameBootstrap初始化失败");
        }
        
        // 4. 测试场景存在性检查
        LogDebug("步骤4：测试场景存在性检查");
        if (levelSequence != null && levelSequence.Length > 0)
        {
            string firstLevel = levelSequence[0];
            bool sceneExists = IsSceneInBuildSettings(firstLevel);
            if (sceneExists)
            {
                LogDebug($"✓ 场景 {firstLevel} 存在于构建设置中");
            }
            else
            {
                LogDebug($"✗ 场景 {firstLevel} 不存在于构建设置中");
            }
        }
        
        LogDebug("=== 测试完成 ===");
    }
    
    /// <summary>
    /// 测试关卡特定的缓存清理功能
    /// </summary>
    [ContextMenu("测试关卡特定缓存清理")]
    public void TestLevelSpecificCacheClear()
    {
        LogDebug("=== 开始测试关卡特定缓存清理 ===");
        
        // 测试在level1中清空后续关卡缓存
        LogDebug("测试1：模拟在level1中清空level2和level3缓存");
        PublicData.ClearLevelAndSubsequentCache("level2");
        PublicData.ClearLevelAndSubsequentCache("level3");
        
        // 测试在level2中清空level3缓存
        LogDebug("测试2：模拟在level2中清空level3缓存");
        PublicData.ClearLevelAndSubsequentCache("level3");
        
        // 测试在level3中不清理任何缓存
        LogDebug("测试3：模拟在level3中（应该不清理任何缓存）");
        // level3是最后一关，不需要清理
        
        LogDebug("=== 关卡特定缓存清理测试完成 ===");
    }
    
    /// <summary>
    /// 测试自动缓存清理功能
    /// </summary>
    [ContextMenu("测试自动缓存清理")]
    public void TestAutoCacheClear()
    {
        LogDebug("=== 开始测试自动缓存清理 ===");
        
        // 模拟场景加载事件
        string currentScene = SceneManager.GetActiveScene().name;
        LogDebug($"当前场景: {currentScene}");
        
        if (LevelProgressManager.Instance != null)
        {
            // 调用自动缓存清理逻辑
            LogDebug("调用LevelProgressManager的自动缓存清理");
            // 这里我们直接调用PublicData的方法来模拟
            PublicData.ClearLevelAndSubsequentCache(currentScene);
        }
        else
        {
            LogDebug("LevelProgressManager实例不存在");
        }
        
        LogDebug("=== 自动缓存清理测试完成 ===");
    }
    
    /// <summary>
    /// 测试JSON存储功能
    /// </summary>
    [ContextMenu("测试JSON存储功能")]
    public void TestJsonStorage()
    {
        LogDebug("=== 开始测试JSON存储功能 ===");
        
        // 测试1：创建测试进度数据
        LogDebug("测试1：创建测试进度数据");
        var testProgress = new GameProgressData();
        testProgress.currentLevel = "level1";
        testProgress.AddCompletedLevel("level1");
        testProgress.gameStarted = true;
        testProgress.UpdateSaveTime();
        
        LogDebug($"测试进度数据: {testProgress.GetProgressSummary()}");
        
        // 测试2：保存进度数据
        LogDebug("测试2：保存进度数据");
        bool saveResult = JsonStorageManager.SaveGameProgress(testProgress);
        LogDebug($"保存结果: {saveResult}");
        
        // 测试3：加载进度数据
        LogDebug("测试3：加载进度数据");
        var loadedProgress = JsonStorageManager.LoadGameProgress();
        if (loadedProgress != null)
        {
            LogDebug($"加载的进度数据: {loadedProgress.GetProgressSummary()}");
            LogDebug($"创建时间: {loadedProgress.createTime}");
            LogDebug($"最后保存时间: {loadedProgress.lastSaveTime}");
        }
        else
        {
            LogDebug("加载进度数据失败");
        }
        
        // 测试4：检查存储信息
        LogDebug("测试4：检查存储信息");
        LogDebug($"存储信息:\n{JsonStorageManager.GetStorageInfo()}");
        
        // 测试5：检查是否有进度数据
        LogDebug("测试5：检查是否有进度数据");
        bool hasProgress = JsonStorageManager.HasGameProgress();
        LogDebug($"是否有进度数据: {hasProgress}");
        
        LogDebug("=== JSON存储功能测试完成 ===");
    }
    
    /// <summary>
    /// 测试JSON存储的导入导出功能
    /// </summary>
    [ContextMenu("测试JSON导入导出")]
    public void TestJsonImportExport()
    {
        LogDebug("=== 开始测试JSON导入导出功能 ===");
        
        // 创建测试进度数据
        var testProgress = new GameProgressData();
        testProgress.currentLevel = "level2";
        testProgress.AddCompletedLevel("level1");
        testProgress.AddCompletedLevel("level2");
        testProgress.gameStarted = true;
        
        // 保存测试数据
        JsonStorageManager.SaveGameProgress(testProgress);
        LogDebug("已保存测试进度数据");
        
        // 导出到桌面（如果可能）
        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        string exportPath = System.IO.Path.Combine(desktopPath, "game_progress_export.json");
        
        LogDebug($"尝试导出到: {exportPath}");
        bool exportResult = JsonStorageManager.ExportProgress(exportPath);
        LogDebug($"导出结果: {exportResult}");
        
        // 删除当前进度数据
        JsonStorageManager.DeleteGameProgress();
        LogDebug("已删除当前进度数据");
        
        // 重新导入
        LogDebug("尝试重新导入进度数据");
        bool importResult = JsonStorageManager.ImportProgress(exportPath);
        LogDebug($"导入结果: {importResult}");
        
        // 验证导入的数据
        if (importResult)
        {
            var importedProgress = JsonStorageManager.LoadGameProgress();
            if (importedProgress != null)
            {
                LogDebug($"导入验证成功: {importedProgress.GetProgressSummary()}");
            }
        }
        
        LogDebug("=== JSON导入导出功能测试完成 ===");
    }
    
    /// <summary>
    /// 测试关卡状态存储功能
    /// </summary>
    [ContextMenu("测试关卡状态存储")]
    public void TestLevelStateStorage()
    {
        LogDebug("=== 开始测试关卡状态存储功能 ===");
        
        if (LevelProgressManager.Instance == null)
        {
            LogDebug("LevelProgressManager实例不存在");
            return;
        }
        
        // 测试1：创建测试关卡状态数据
        LogDebug("测试1：创建测试关卡状态数据");
        var testStateData = new GameProgressData.LevelStateData();
        testStateData.levelName = "level2";
        testStateData.objectStates.Add(new GameProgressData.GameObjectStateData
        {
            objectName = "TestObject1",
            isActive = true,
            highlightEnabled = true,
            position = new Vector3(1, 2, 3),
            rotation = new Vector3(0, 90, 0),
            scale = Vector3.one
        });
        testStateData.objectStates.Add(new GameProgressData.GameObjectStateData
        {
            objectName = "TestObject2",
            isActive = false,
            highlightEnabled = false
        });
        testStateData.broadcastHistory.Add("测试广播消息1");
        testStateData.broadcastHistory.Add("测试广播消息2");
        testStateData.availableStrings.Add("可用字符串1");
        testStateData.availableStrings.Add("可用字符串2");
        testStateData.currentSeason = "夏季";
        testStateData.collectedStrings.Add("收集的字符串1");
        testStateData.collectedStrings.Add("收集的字符串2");
        testStateData.completedTargets.Add("已完成目标1");
        testStateData.currentTargetList.Add("当前目标1");
        testStateData.currentTargetList.Add("当前目标2");
        
        LogDebug($"测试状态数据创建完成 - 物体数量: {testStateData.objectStates.Count}, 广播数量: {testStateData.broadcastHistory.Count}");
        
        // 测试2：保存关卡状态
        LogDebug("测试2：保存关卡状态");
        LevelProgressManager.Instance.SaveLevelState("level2", testStateData);
        
        // 测试3：加载关卡状态
        LogDebug("测试3：加载关卡状态");
        var loadedState = LevelProgressManager.Instance.LoadLevelState("level2");
        if (loadedState != null)
        {
            LogDebug($"加载成功 - 物体数量: {loadedState.objectStates.Count}, 广播数量: {loadedState.broadcastHistory.Count}");
            LogDebug($"当前季节: {loadedState.currentSeason}, 收集字符串数量: {loadedState.collectedStrings.Count}");
        }
        else
        {
            LogDebug("加载失败");
        }
        
        // 测试4：检查是否有任何关卡状态
        LogDebug("测试4：检查是否有任何关卡状态");
        bool hasStates = LevelProgressManager.Instance.HasAnyLevelStates();
        LogDebug($"有关卡状态: {hasStates}");
        
        // 测试5：显示当前进度信息
        LogDebug("测试5：显示当前进度信息");
        LevelProgressManager.Instance.ShowCurrentProgress();
        
        // 测试6：清空关卡状态
        LogDebug("测试6：清空关卡状态");
        LevelProgressManager.Instance.ClearLevelState("level2");
        
        // 测试7：再次检查是否有任何关卡状态
        LogDebug("测试7：再次检查是否有任何关卡状态");
        hasStates = LevelProgressManager.Instance.HasAnyLevelStates();
        LogDebug($"清空后有关卡状态: {hasStates}");
        
        LogDebug("=== 关卡状态存储功能测试完成 ===");
    }
    
    /// <summary>
    /// 测试关卡状态自动清理功能
    /// </summary>
    [ContextMenu("测试关卡状态自动清理")]
    public void TestLevelStateAutoClear()
    {
        LogDebug("=== 开始测试关卡状态自动清理功能 ===");
        
        if (LevelProgressManager.Instance == null)
        {
            LogDebug("LevelProgressManager实例不存在");
            return;
        }
        
        // 创建多个关卡的测试状态数据
        string[] testLevels = { "level1", "level2", "level3" };
        
        foreach (string level in testLevels)
        {
            var testStateData = new GameProgressData.LevelStateData();
            testStateData.levelName = level;
            testStateData.objectStates.Add(new GameProgressData.GameObjectStateData
            {
                objectName = $"TestObject_{level}",
                isActive = true,
                highlightEnabled = true
            });
            testStateData.broadcastHistory.Add($"广播消息_{level}");
            testStateData.availableStrings.Add($"字符串_{level}");
            
            LevelProgressManager.Instance.SaveLevelState(level, testStateData);
            LogDebug($"已保存 {level} 的测试状态数据");
        }
        
        // 显示所有关卡状态
        LogDebug("保存后的状态:");
        LevelProgressManager.Instance.ShowCurrentProgress();
        
        // 模拟进入level2，应该清空level2的状态
        LogDebug("模拟进入level2，测试自动清理");
        LevelProgressManager.Instance.ClearLevelState("level2");
        
        // 显示清理后的状态
        LogDebug("清理后的状态:");
        LevelProgressManager.Instance.ShowCurrentProgress();
        
        // 清空所有状态
        LogDebug("清空所有关卡状态");
        LevelProgressManager.Instance.ClearAllLevelStates();
        
        // 最终状态
        LogDebug("最终状态:");
        LevelProgressManager.Instance.ShowCurrentProgress();
        
        LogDebug("=== 关卡状态自动清理功能测试完成 ===");
    }
    
    /// <summary>
    /// 测试场景加载功能
    /// </summary>
    [ContextMenu("测试场景加载功能")]
    public void TestSceneLoading()
    {
        LogDebug("=== 开始测试场景加载功能 ===");
        
        // 确保GameBootstrap已初始化
        GameBootstrap.EnsureInitialized();
        
        // 获取关卡序列
        string[] levelSequence = PublicData.GetLevelSequence();
        if (levelSequence != null && levelSequence.Length > 0)
        {
            string firstLevel = levelSequence[0];
            LogDebug($"准备加载场景: {firstLevel}");
            
            if (IsSceneInBuildSettings(firstLevel))
            {
                LogDebug($"✓ 场景 {firstLevel} 验证通过，开始加载");
                SceneManager.LoadScene(firstLevel);
            }
            else
            {
                LogDebug($"✗ 场景 {firstLevel} 不存在，尝试加载 level1");
                if (IsSceneInBuildSettings("level1"))
                {
                    SceneManager.LoadScene("level1");
                }
                else
                {
                    LogDebug("✗ level1 也不存在，加载失败");
                }
            }
        }
        else
        {
            LogDebug("✗ 无法获取关卡序列");
        }
    }
    
    /// <summary>
    /// 检查场景是否在构建设置中
    /// </summary>
    private bool IsSceneInBuildSettings(string sceneName)
    {
        try
        {
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < sceneCount; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
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
    /// 显示当前系统状态
    /// </summary>
    [ContextMenu("显示当前系统状态")]
    public void ShowSystemStatus()
    {
        LogDebug("=== 当前系统状态 ===");
        LogDebug($"GameBootstrap.IsInitialized: {GameBootstrap.IsInitialized}");
        LogDebug($"LevelProgressManager.Instance: {(LevelProgressManager.Instance != null ? "存在" : "不存在")}");
        LogDebug($"AudioManager.Instance: {(AudioManager.Instance != null ? "存在" : "不存在")}");
        LogDebug($"InfoPopupManager.Instance: {(InfoPopupManager.Instance != null ? "存在" : "不存在")}");
        LogDebug($"GameFlowManager.Instance: {(GameFlowManager.Instance != null ? "存在" : "不存在")}");
        
        string[] levelSequence = PublicData.GetLevelSequence();
        LogDebug($"关卡序列: {(levelSequence != null ? $"[{string.Join(", ", levelSequence)}]" : "null")}");
        
        LogDebug($"当前场景: {SceneManager.GetActiveScene().name}");
        LogDebug("==================");
    }
    
    /// <summary>
    /// 调试日志输出
    /// </summary>
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[SceneLoadingTestHelper] {message}");
        }
    }
}
