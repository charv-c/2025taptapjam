using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 游戏启动引导系统 - 确保核心单例在任意场景启动时都能正常工作
/// 作为资深架构师设计的解决方案，支持单场景测试时的完整初始化
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    public static GameBootstrap Instance { get; private set; }
    
    [Header("预制体引用 - 需要在Inspector中设置")]
    [Tooltip("AudioManager预制体路径")]
    [SerializeField] private GameObject audioManagerPrefab;
    [Tooltip("InfoPopupManager预制体路径")]
    [SerializeField] private GameObject infoPopupManagerPrefab;
    [Tooltip("GameFlowManager预制体路径")]
    [SerializeField] private GameObject gameFlowManagerPrefab;
    [Tooltip("MouseCursorManager预制体路径")]
    [SerializeField] private GameObject mouseCursorManagerPrefab;
    
    [Header("调试设置")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // 初始化完成标志
    public static bool IsInitialized { get; private set; } = false;
    
    private void Awake()
    {
        // 实现单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (enableDebugLogs)
            {
                GameLogger.LogSystem("GameBootstrap: 初始化开始");
            }
            
            StartCoroutine(InitializeGameSystems());
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 静态方法：确保游戏系统已初始化
    /// 任何需要依赖核心系统的脚本都应该调用此方法
    /// </summary>
    public static void EnsureInitialized()
    {
        if (IsInitialized) return;
        
        // 如果GameBootstrap不存在，创建一个临时的
        if (Instance == null)
        {
            GameObject bootstrapObj = new GameObject("GameBootstrap_Runtime");
            GameBootstrap bootstrap = bootstrapObj.AddComponent<GameBootstrap>();
            
            // 尝试从Resources加载预制体
            bootstrap.LoadPrefabsFromResources();
        }
    }
    
    /// <summary>
    /// 从Resources文件夹加载预制体（fallback方案）
    /// </summary>
    private void LoadPrefabsFromResources()
    {
        // 尝试从Resources加载预制体
        if (audioManagerPrefab == null)
        {
            audioManagerPrefab = Resources.Load<GameObject>("Prefabs/AudioManager");
        }
        
        if (infoPopupManagerPrefab == null)
        {
            infoPopupManagerPrefab = Resources.Load<GameObject>("Prefabs/InfoPopupManager");
        }
        
        if (gameFlowManagerPrefab == null)
        {
            gameFlowManagerPrefab = Resources.Load<GameObject>("Prefabs/GameFlowManager");
        }
        
        if (mouseCursorManagerPrefab == null)
        {
            mouseCursorManagerPrefab = Resources.Load<GameObject>("Prefabs/MouseCursorManager");
        }
        
        StartCoroutine(InitializeGameSystems());
    }
    
    /// <summary>
    /// 初始化核心游戏系统
    /// </summary>
    private IEnumerator InitializeGameSystems()
    {
        if (enableDebugLogs)
        {
            GameLogger.LogSystem("GameBootstrap: 开始初始化核心系统");
        }
        
        // 等一帧确保场景完全加载
        yield return null;
        
        // 1. 确保AudioManager存在
        EnsureAudioManager();
        yield return null;
        
        // 2. 确保InfoPopupManager存在
        EnsureInfoPopupManager();
        yield return null;
        
        // 3. 确保GameFlowManager存在
        EnsureGameFlowManager();
        yield return null;
        
        // 4. 确保MouseCursorManager存在
        EnsureMouseCursorManager();
        yield return null;
        
        // 5. 标记初始化完成
        IsInitialized = true;
        
        if (enableDebugLogs)
        {
            GameLogger.LogSystem("GameBootstrap: 所有核心系统初始化完成");
        }
        
        // 5. 通知其他系统初始化完成
        NotifySystemsReady();
    }
    
    /// <summary>
    /// 确保AudioManager存在并正常工作
    /// </summary>
    private void EnsureAudioManager()
    {
        if (AudioManager.Instance == null)
        {
            if (enableDebugLogs)
            {
                GameLogger.LogSystem("GameBootstrap: 创建AudioManager");
            }
            
            GameObject audioManagerObj = null;
            
            if (audioManagerPrefab != null)
            {
                // 使用预制体创建
                audioManagerObj = Instantiate(audioManagerPrefab);
            }
            else
            {
                // 创建空对象并添加组件
                audioManagerObj = new GameObject("AudioManager");
                audioManagerObj.AddComponent<AudioManager>();
                
                // 创建AudioSource组件
                var bgmSource = audioManagerObj.AddComponent<AudioSource>();
                var sfxSource = audioManagerObj.AddComponent<AudioSource>();
                var ambientSource = audioManagerObj.AddComponent<AudioSource>();
                
                // 配置AudioSource
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
                sfxSource.playOnAwake = false;
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
                
                if (enableDebugLogs)
                {
                    GameLogger.LogSystem("GameBootstrap: 已创建基础AudioManager配置");
                }
            }
            
            DontDestroyOnLoad(audioManagerObj);
        }
        else
        {
            if (enableDebugLogs)
            {
                GameLogger.LogSystem("GameBootstrap: AudioManager已存在");
            }
        }
    }
    
    /// <summary>
    /// 确保InfoPopupManager存在并正常工作
    /// </summary>
    private void EnsureInfoPopupManager()
    {
        if (InfoPopupManager.Instance == null)
        {
            if (enableDebugLogs)
            {
                GameLogger.LogSystem("GameBootstrap: 创建InfoPopupManager");
            }
            
            GameObject infoPopupObj = null;
            
            if (infoPopupManagerPrefab != null)
            {
                // 使用预制体创建
                infoPopupObj = Instantiate(infoPopupManagerPrefab);
            }
            else
            {
                // 创建空对象并添加组件
                infoPopupObj = new GameObject("InfoPopupManager");
                var infoPopup = infoPopupObj.AddComponent<InfoPopupManager>();
                
                // 尝试加载默认预制体
                GameObject popupPrefab = Resources.Load<GameObject>("Prefabs/InfoPopupPanel");
                if (popupPrefab != null)
                {
                    // 通过反射设置预制体引用（因为字段是private SerializeField）
                    var field = typeof(InfoPopupManager).GetField("popupPanelPrefab", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    field?.SetValue(infoPopup, popupPrefab);
                    
                    if (enableDebugLogs)
                    {
                        GameLogger.LogSystem("GameBootstrap: 已设置InfoPopupManager预制体引用");
                    }
                }
                else
                {
                    GameLogger.LogWarning("GameBootstrap: 未找到InfoPopupPanel预制体，InfoPopupManager可能无法正常工作");
                }
            }
            
            DontDestroyOnLoad(infoPopupObj);
        }
        else
        {
            if (enableDebugLogs)
            {
                GameLogger.LogSystem("GameBootstrap: InfoPopupManager已存在");
            }
        }
    }
    
    /// <summary>
    /// 确保GameFlowManager存在并正常工作
    /// </summary>
    private void EnsureGameFlowManager()
    {
        if (GameFlowManager.Instance == null)
        {
            if (enableDebugLogs)
            {
                GameLogger.LogSystem("GameBootstrap: 创建GameFlowManager");
            }
            
            GameObject gameFlowObj = null;
            
            if (gameFlowManagerPrefab != null)
            {
                // 使用预制体创建
                gameFlowObj = Instantiate(gameFlowManagerPrefab);
                DontDestroyOnLoad(gameFlowObj);
                if (enableDebugLogs)
                {
                    GameLogger.LogSystem("GameBootstrap: 使用预制体创建GameFlowManager");
                }
            }
            else
            {
                // 创建空对象并添加组件
                gameFlowObj = new GameObject("GameFlowManager");
                // 立即设置DontDestroyOnLoad，避免在Awake中被销毁
                DontDestroyOnLoad(gameFlowObj);
                // 然后添加组件，这样Awake会在DontDestroyOnLoad之后执行
                gameFlowObj.AddComponent<GameFlowManager>();
                if (enableDebugLogs)
                {
                    GameLogger.LogSystem("GameBootstrap: 通过代码创建GameFlowManager");
                }
            }
            
            // 额外验证
            if (GameFlowManager.Instance != null)
            {
                if (enableDebugLogs)
                {
                    GameLogger.LogSystem("GameBootstrap: GameFlowManager创建成功，Instance已设置");
                }
            }
            else
            {
                GameLogger.LogError("GameBootstrap: GameFlowManager创建失败！Instance仍为null");
            }
        }
        else
        {
            if (enableDebugLogs)
            {
                GameLogger.LogSystem("GameBootstrap: GameFlowManager已存在");
            }
        }
    }
    
    /// <summary>
    /// 确保MouseCursorManager存在并正常工作
    /// </summary>
    private void EnsureMouseCursorManager()
    {
        if (MouseCursorManager.Instance == null)
        {
            if (enableDebugLogs)
            {
                GameLogger.LogSystem("GameBootstrap: 创建MouseCursorManager");
            }
            
            GameObject mouseCursorObj = null;
            
            if (mouseCursorManagerPrefab != null)
            {
                // 使用预制体创建
                mouseCursorObj = Instantiate(mouseCursorManagerPrefab);
                DontDestroyOnLoad(mouseCursorObj);
                if (enableDebugLogs)
                {
                    GameLogger.LogSystem("GameBootstrap: 使用预制体创建MouseCursorManager");
                }
            }
            else
            {
                // 创建空对象并添加组件
                mouseCursorObj = new GameObject("MouseCursorManager");
                // 立即设置DontDestroyOnLoad，避免在Awake中被销毁
                DontDestroyOnLoad(mouseCursorObj);
                // 然后添加组件，这样Awake会在DontDestroyOnLoad之后执行
                mouseCursorObj.AddComponent<MouseCursorManager>();
                if (enableDebugLogs)
                {
                    GameLogger.LogSystem("GameBootstrap: 通过代码创建MouseCursorManager");
                }
            }
            
            // 额外验证
            if (MouseCursorManager.Instance != null)
            {
                if (enableDebugLogs)
                {
                    GameLogger.LogSystem("GameBootstrap: MouseCursorManager创建成功，Instance已设置");
                }
            }
            else
            {
                GameLogger.LogError("GameBootstrap: MouseCursorManager创建失败！Instance仍为null");
            }
        }
        else
        {
            if (enableDebugLogs)
            {
                GameLogger.LogSystem("GameBootstrap: MouseCursorManager已存在");
            }
        }
    }
    
    /// <summary>
    /// 通知其他系统初始化完成
    /// </summary>
    private void NotifySystemsReady()
    {
        // 发送初始化完成事件
        var sceneAwareComponents = FindObjectsOfType<MonoBehaviour>();
        foreach (var component in sceneAwareComponents)
        {
            // 检查组件是否实现了IBootstrapAware接口
            if (component is IBootstrapAware bootstrapAware)
            {
                bootstrapAware.OnBootstrapComplete();
            }
        }
        
        if (enableDebugLogs)
        {
            GameLogger.LogSystem($"GameBootstrap: 已通知 {sceneAwareComponents.Length} 个组件系统初始化完成");
        }
    }
    
    /// <summary>
    /// 获取当前场景信息，用于调试
    /// </summary>
    public static string GetCurrentSceneInfo()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        return $"Scene: {currentScene.name}, BuildIndex: {currentScene.buildIndex}";
    }
    
    /// <summary>
    /// 检查所有核心系统是否正常工作
    /// </summary>
    public static bool ValidateCoreSystems()
    {
        bool isValid = true;
        
        if (AudioManager.Instance == null)
        {
            GameLogger.LogError("GameBootstrap: AudioManager.Instance 为空");
            isValid = false;
        }
        
        if (InfoPopupManager.Instance == null)
        {
            GameLogger.LogError("GameBootstrap: InfoPopupManager.Instance 为空");
            isValid = false;
        }
        
        if (GameFlowManager.Instance == null)
        {
            GameLogger.LogError("GameBootstrap: GameFlowManager.Instance 为空");
            isValid = false;
        }
        
        if (MouseCursorManager.Instance == null)
        {
            GameLogger.LogError("GameBootstrap: MouseCursorManager.Instance 为空");
            isValid = false;
        }
        
        return isValid;
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && enableDebugLogs)
        {
            GameLogger.LogSystem($"GameBootstrap: 应用重新获得焦点 - {GetCurrentSceneInfo()}");
        }
    }
}

/// <summary>
/// 接口：实现此接口的组件会在Bootstrap完成时收到通知
/// </summary>
public interface IBootstrapAware
{
    void OnBootstrapComplete();
}
