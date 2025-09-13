using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 场景初始化器 - 自动确保每个场景都有必要的系统初始化
/// 这个组件应该作为预制体添加到每个场景中，或者通过编辑器脚本自动添加
/// </summary>
public class SceneInitializer : MonoBehaviour
{
    [Header("场景配置")]
    [SerializeField] private bool isTestingScene = false; // 标记是否为测试场景
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool waitForBootstrap = true; // 是否等待Bootstrap完成
    
    [Header("场景特定设置")]
    [SerializeField] private string sceneName;
    [SerializeField] private bool autoDetectSceneName = true;
    
    // 初始化完成事件
    public static System.Action<string> OnSceneInitialized;
    
    private void Awake()
    {
        // 自动检测场景名称
        if (autoDetectSceneName || string.IsNullOrEmpty(sceneName))
        {
            sceneName = SceneManager.GetActiveScene().name;
        }
        
        if (enableDebugLogs)
        {
            GameLogger.LogSystem($"SceneInitializer: 开始初始化场景 '{sceneName}'");
            if (isTestingScene)
            {
                GameLogger.LogSystem("SceneInitializer: 检测到测试场景启动，确保完整系统初始化");
            }
        }
    }
    
    private void Start()
    {
        StartCoroutine(InitializeSceneCoroutine());
    }
    
    /// <summary>
    /// 场景初始化协程
    /// </summary>
    private IEnumerator InitializeSceneCoroutine()
    {
        // 1. 确保GameBootstrap存在并完成初始化
        yield return StartCoroutine(EnsureBootstrapInitialized());
        
        // 2. 等待一帧，确保其他Start()方法执行完毕
        yield return null;
        
        // 3. 验证核心系统
        if (GameBootstrap.ValidateCoreSystems())
        {
            if (enableDebugLogs)
            {
                GameLogger.LogSystem($"SceneInitializer: 场景 '{sceneName}' 初始化完成，所有核心系统正常");
            }
        }
        else
        {
            GameLogger.LogError($"SceneInitializer: 场景 '{sceneName}' 初始化失败，部分核心系统缺失");
        }
        
        // 4. 发送场景初始化完成事件
        OnSceneInitialized?.Invoke(sceneName);
        
        // 5. 如果是测试场景，输出额外调试信息
        if (isTestingScene)
        {
            LogTestingSceneInfo();
        }
    }
    
    /// <summary>
    /// 确保Bootstrap已完成初始化
    /// </summary>
    private IEnumerator EnsureBootstrapInitialized()
    {
        // 确保GameBootstrap存在
        GameBootstrap.EnsureInitialized();
        
        if (waitForBootstrap)
        {
            // 等待Bootstrap完成初始化
            float timeout = 5f; // 5秒超时
            float elapsed = 0f;
            
            while (!GameBootstrap.IsInitialized && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
            
            if (!GameBootstrap.IsInitialized)
            {
                GameLogger.LogError($"SceneInitializer: Bootstrap初始化超时 ({timeout}s)，场景: {sceneName}");
            }
            else if (enableDebugLogs)
            {
                GameLogger.LogSystem($"SceneInitializer: Bootstrap初始化完成，耗时: {elapsed:F1}s");
            }
        }
    }
    
    /// <summary>
    /// 输出测试场景的调试信息
    /// </summary>
    private void LogTestingSceneInfo()
    {
        GameLogger.LogSystem("=== 测试场景启动信息 ===");
        GameLogger.LogSystem($"当前场景: {sceneName}");
        GameLogger.LogSystem($"Bootstrap状态: {(GameBootstrap.IsInitialized ? "已初始化" : "未初始化")}");
        GameLogger.LogSystem($"AudioManager: {(AudioManager.Instance != null ? "正常" : "缺失")}");
        GameLogger.LogSystem($"InfoPopupManager: {(InfoPopupManager.Instance != null ? "正常" : "缺失")}");
        GameLogger.LogSystem($"GameFlowManager: {(GameFlowManager.Instance != null ? "正常" : "缺失")}");
        
        // 检查PlayerController
        PlayerController playerController = FindObjectOfType<PlayerController>();
        GameLogger.LogSystem($"PlayerController: {(playerController != null ? "找到" : "未找到")}");
        
        // 检查关卡管理器
        LevelManager levelManager = FindObjectOfType<LevelManager>();
        GameLogger.LogSystem($"LevelManager: {(levelManager != null ? "找到" : "未找到")}");
        
        GameLogger.LogSystem("========================");
    }
    
    /// <summary>
    /// 静态方法：为场景添加初始化器（编辑器用）
    /// </summary>
    [System.Obsolete("此方法仅供编辑器使用")]
    public static SceneInitializer AddToScene(bool isTestingScene = false)
    {
        // 检查场景中是否已存在SceneInitializer
        SceneInitializer existing = FindObjectOfType<SceneInitializer>();
        if (existing != null)
        {
            existing.isTestingScene = isTestingScene;
            return existing;
        }
        
        // 创建新的SceneInitializer
        GameObject initializerObj = new GameObject("SceneInitializer");
        SceneInitializer initializer = initializerObj.AddComponent<SceneInitializer>();
        initializer.isTestingScene = isTestingScene;
        
        return initializer;
    }
    
    private void OnValidate()
    {
        // 在编辑器中自动设置场景名称
        if (autoDetectSceneName && Application.isPlaying)
        {
            sceneName = SceneManager.GetActiveScene().name;
        }
    }
}

/// <summary>
/// 场景初始化器的编辑器扩展属性
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Class)]
public class RequiresSceneInitializerAttribute : System.Attribute
{
    public bool IsTestingSupported { get; }
    
    public RequiresSceneInitializerAttribute(bool isTestingSupported = true)
    {
        IsTestingSupported = isTestingSupported;
    }
}
