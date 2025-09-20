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
