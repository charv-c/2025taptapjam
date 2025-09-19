using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 继续按钮调试工具
/// 用于诊断继续按钮显示问题
/// </summary>
public class ContinueButtonDebugger : MonoBehaviour
{
    [Header("调试设置")]
    [SerializeField] private bool enableDebugLog = true;
    [SerializeField] private bool autoRunOnStart = false;
    
    [Header("测试按钮")]
    [SerializeField] private Button testContinueButton;
    [SerializeField] private Button testStartButton;
    
    private void Start()
    {
        if (autoRunOnStart)
        {
            RunFullDiagnostic();
        }
    }
    
    /// <summary>
    /// 运行完整的诊断检查
    /// </summary>
    [ContextMenu("运行完整诊断")]
    public void RunFullDiagnostic()
    {
        LogDebug("=== 继续按钮诊断开始 ===");
        
        // 1. 检查PlayerPrefs
        CheckPlayerPrefs();
        
        // 2. 检查关卡序列
        CheckLevelSequence();
        
        // 3. 检查按钮引用
        CheckButtonReferences();
        
        // 4. 检查LevelProgressManager
        CheckLevelProgressManager();
        
        // 5. 检查场景信息
        CheckSceneInfo();
        
        LogDebug("=== 继续按钮诊断完成 ===");
    }
    
    /// <summary>
    /// 检查PlayerPrefs状态
    /// </summary>
    private void CheckPlayerPrefs()
    {
        LogDebug("--- PlayerPrefs检查 ---");
        
        try
        {
            // 测试PlayerPrefs可用性
            string testKey = "DebugTest_" + System.DateTime.Now.Ticks;
            PlayerPrefs.SetString(testKey, "test");
            PlayerPrefs.Save();
            string result = PlayerPrefs.GetString(testKey, "");
            PlayerPrefs.DeleteKey(testKey);
            PlayerPrefs.Save();
            
            LogDebug($"PlayerPrefs可用性: {result == "test"}");
            
            // 检查关键键值
            string currentLevel = PlayerPrefs.GetString("CurrentLevel", "");
            string completedLevels = PlayerPrefs.GetString("CompletedLevels", "");
            int gameStarted = PlayerPrefs.GetInt("GameStarted", 0);
            
            LogDebug($"CurrentLevel: '{currentLevel}'");
            LogDebug($"CompletedLevels: '{completedLevels}'");
            LogDebug($"GameStarted: {gameStarted}");
            
            // 检查是否有任何存档数据
            bool hasAnySaveData = !string.IsNullOrEmpty(currentLevel) || 
                                 !string.IsNullOrEmpty(completedLevels) || 
                                 gameStarted == 1;
            LogDebug($"是否有存档数据: {hasAnySaveData}");
        }
        catch (System.Exception e)
        {
            LogDebug($"PlayerPrefs检查失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 检查关卡序列
    /// </summary>
    private void CheckLevelSequence()
    {
        LogDebug("--- 关卡序列检查 ---");
        
        try
        {
            // 检查静态序列
            if (PublicData.LevelSequence != null)
            {
                LogDebug($"静态关卡序列: [{string.Join(", ", PublicData.LevelSequence)}]");
            }
            else
            {
                LogDebug("静态关卡序列: null");
            }
            
            // 检查动态序列
            string[] dynamicSequence = PublicData.GetLevelSequence();
            if (dynamicSequence != null)
            {
                LogDebug($"动态关卡序列: [{string.Join(", ", dynamicSequence)}]");
            }
            else
            {
                LogDebug("动态关卡序列: null");
            }
            
            // 检查构建设置中的场景
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            LogDebug($"构建设置中的场景数量: {sceneCount}");
            
            for (int i = 0; i < sceneCount; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                LogDebug($"  场景 {i}: {sceneName} ({scenePath})");
            }
        }
        catch (System.Exception e)
        {
            LogDebug($"关卡序列检查失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 检查按钮引用
    /// </summary>
    private void CheckButtonReferences()
    {
        LogDebug("--- 按钮引用检查 ---");
        
        try
        {
            // 检查LevelProgressManager的按钮引用
            if (LevelProgressManager.Instance != null)
            {
                LogDebug("LevelProgressManager实例存在");
                
                // 通过反射获取私有字段（仅用于调试）
                var type = typeof(LevelProgressManager);
                var startGameField = type.GetField("startGameButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var continueGameField = type.GetField("continueGameButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var startNewGameField = type.GetField("startNewGameButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (startGameField != null)
                {
                    Button startBtn = startGameField.GetValue(LevelProgressManager.Instance) as Button;
                    LogDebug($"startGameButton引用: {startBtn?.name ?? "null"}");
                }
                
                if (continueGameField != null)
                {
                    Button continueBtn = continueGameField.GetValue(LevelProgressManager.Instance) as Button;
                    LogDebug($"continueGameButton引用: {continueBtn?.name ?? "null"}");
                }
                
                if (startNewGameField != null)
                {
                    Button startNewBtn = startNewGameField.GetValue(LevelProgressManager.Instance) as Button;
                    LogDebug($"startNewGameButton引用: {startNewBtn?.name ?? "null"}");
                }
            }
            else
            {
                LogDebug("LevelProgressManager实例不存在");
            }
            
            // 检查场景中的按钮
            Button[] allButtons = FindObjectsOfType<Button>();
            LogDebug($"场景中按钮总数: {allButtons.Length}");
            
            foreach (Button btn in allButtons)
            {
                if (btn.name.ToLowerInvariant().Contains("continue") || 
                    btn.name.ToLowerInvariant().Contains("继续") ||
                    btn.name.ToLowerInvariant().Contains("start") ||
                    btn.name.ToLowerInvariant().Contains("开始"))
                {
                    LogDebug($"  相关按钮: {btn.name} (Active: {btn.gameObject.activeInHierarchy}, Interactable: {btn.interactable})");
                }
            }
        }
        catch (System.Exception e)
        {
            LogDebug($"按钮引用检查失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 检查LevelProgressManager状态
    /// </summary>
    private void CheckLevelProgressManager()
    {
        LogDebug("--- LevelProgressManager检查 ---");
        
        try
        {
            if (LevelProgressManager.Instance != null)
            {
                LogDebug("LevelProgressManager实例存在");
                LogDebug($"游戏已开始: {LevelProgressManager.Instance.HasGameStarted()}");
                LogDebug($"已完成关卡数: {LevelProgressManager.Instance.GetCompletedLevelsCount()}");
                LogDebug($"总关卡数: {LevelProgressManager.Instance.GetTotalLevelsCount()}");
                LogDebug($"进度百分比: {LevelProgressManager.Instance.GetProgressPercentage():F1}%");
                LogDebug($"当前关卡: '{LevelProgressManager.Instance.GetCurrentLevelToLoad()}'");
                LogDebug($"下一个关卡: '{LevelProgressManager.Instance.GetNextLevelName()}'");
            }
            else
            {
                LogDebug("LevelProgressManager实例不存在");
            }
        }
        catch (System.Exception e)
        {
            LogDebug($"LevelProgressManager检查失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 检查场景信息
    /// </summary>
    private void CheckSceneInfo()
    {
        LogDebug("--- 场景信息检查 ---");
        
        try
        {
            Scene currentScene = SceneManager.GetActiveScene();
            LogDebug($"当前场景: {currentScene.name}");
            LogDebug($"场景路径: {currentScene.path}");
            LogDebug($"场景是否已加载: {currentScene.isLoaded}");
            
            // 检查Canvas
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            LogDebug($"场景中Canvas数量: {canvases.Length}");
            
            foreach (Canvas canvas in canvases)
            {
                LogDebug($"  Canvas: {canvas.name} (RenderMode: {canvas.renderMode}, SortOrder: {canvas.sortingOrder})");
            }
        }
        catch (System.Exception e)
        {
            LogDebug($"场景信息检查失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 强制显示继续按钮（测试用）
    /// </summary>
    [ContextMenu("强制显示继续按钮")]
    public void ForceShowContinueButton()
    {
        LogDebug("尝试强制显示继续按钮");
        
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.ForceShowContinueButton();
        }
        else
        {
            LogDebug("LevelProgressManager实例不存在，无法强制显示继续按钮");
        }
    }
    
    /// <summary>
    /// 创建测试存档
    /// </summary>
    [ContextMenu("创建测试存档")]
    public void CreateTestSaveData()
    {
        LogDebug("创建测试存档数据");
        
        try
        {
            PlayerPrefs.SetString("CurrentLevel", "level1");
            PlayerPrefs.SetString("CompletedLevels", "level1");
            PlayerPrefs.SetInt("GameStarted", 1);
            PlayerPrefs.Save();
            
            LogDebug("测试存档创建成功");
            
            // 重新加载LevelProgressManager
            if (LevelProgressManager.Instance != null)
            {
                LevelProgressManager.Instance.UpdateButtonStates();
            }
        }
        catch (System.Exception e)
        {
            LogDebug($"创建测试存档失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 清除所有存档数据
    /// </summary>
    [ContextMenu("清除所有存档")]
    public void ClearAllSaveData()
    {
        LogDebug("清除所有存档数据");
        
        try
        {
            PlayerPrefs.DeleteKey("CurrentLevel");
            PlayerPrefs.DeleteKey("CompletedLevels");
            PlayerPrefs.DeleteKey("GameStarted");
            PlayerPrefs.Save();
            
            LogDebug("存档数据清除成功");
            
            // 重新加载LevelProgressManager
            if (LevelProgressManager.Instance != null)
            {
                LevelProgressManager.Instance.ClearAllProgress();
                LevelProgressManager.Instance.UpdateButtonStates();
            }
        }
        catch (System.Exception e)
        {
            LogDebug($"清除存档数据失败: {e.Message}");
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
            Debug.Log($"[ContinueButtonDebugger] {message}");
        }
    }
}
