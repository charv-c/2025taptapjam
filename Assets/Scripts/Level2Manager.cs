using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Level2场景管理器 V3.0 (Bootstrap兼容版)
/// 负责处理Level2的开场白、BGM，并响应关卡完成事件以显示通关结语。
/// 支持从任意场景启动，自动处理依赖初始化。
/// </summary>
public class Level2Manager : MonoBehaviour, IBootstrapAware
{
    private PlayerController playerController;
    private LevelManager levelManager; // 对通用完成检测器的引用
    
    // Bootstrap状态
    private bool bootstrapCompleted = false;
    private bool sceneInitialized = false;

    // 关卡开场白文案
    private readonly string[] openingMessages = 
    {
        "牛郎与织女被分隔于星河两端，无法相逢",
        "执笔人，请在此处补完诗句，让他们的故事重归圆满"
    };
    

    void Start()
    {
        GameLogger.LogSystem("Level2Manager: 开始初始化level2场景");
        
        // 确保Bootstrap系统初始化
        GameBootstrap.EnsureInitialized();
        
        // 标记level1完成并设置当前关卡为level2（保险，避免某些路径漏记）
        if (LevelProgressManager.Instance != null)
        {
            if (!LevelProgressManager.Instance.IsLevelCompleted("level1"))
            {
                LevelProgressManager.Instance.CompleteLevel("level1");
            }
            LevelProgressManager.Instance.SetCurrentLevel("level2");
            GameLogger.LogSystem("Level2Manager: 已设置当前关卡为 level2（并确保level1已完成）");
        }
        
        // 获取对其他管理器的引用
        playerController = FindObjectOfType<PlayerController>();
        levelManager = GetComponent<LevelManager>();
        if (levelManager != null)
        {
            // 订阅关卡完成事件
            levelManager.OnLevelCompleted += HandleLevelCompletion;
        }

        // 游戏开始时先禁用所有操作，等待Bootstrap和开场白
        DisableAllOperations();
        
        // 开始初始化协程
        StartCoroutine(InitializeLevel2Coroutine());
    }

    /// <summary>
    /// 初始化Level2的协程 - 等待Bootstrap完成后开始
    /// </summary>
    private IEnumerator InitializeLevel2Coroutine()
    {
        // 等待Bootstrap完成
        while (!GameBootstrap.IsInitialized)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        GameLogger.LogSystem("Level2Manager: Bootstrap完成，开始场景初始化");
        
        // 设置BGM
        SetupLevel2BGM();
        
        // 等待一帧确保所有系统就绪
        yield return null;
        
        // 标记Bootstrap完成
        bootstrapCompleted = true;
        
        // 开始场景内容
        InitializeSceneContent();
    }
    
    /// <summary>
    /// IBootstrapAware接口实现 - Bootstrap完成时调用
    /// </summary>
    public void OnBootstrapComplete()
    {
        bootstrapCompleted = true;
        GameLogger.LogSystem("Level2Manager: 收到Bootstrap完成通知");
        
        if (!sceneInitialized)
        {
            InitializeSceneContent();
        }
    }
    
    /// <summary>
    /// 初始化场景内容 - 显示开场白并开始游戏
    /// </summary>
    private void InitializeSceneContent()
    {
        if (sceneInitialized) return;
        
        sceneInitialized = true;
        GameLogger.LogSystem("Level2Manager: 开始显示开场白");
        
        // 若已经看过引导，则跳过
        bool seenLevel2Intro = PlayerPrefs.GetInt("Seen_Level2_Intro", 0) == 1;
        if (seenLevel2Intro)
        {
            GameLogger.LogSystem("Level2Manager: 已看过Level2引导，跳过开场白");
            StartLevel();
            return;
        }
        
        // 显示开场白，结束后再正式开始关卡
        if (InfoPopupManager.Instance != null)
        {
            InfoPopupManager.Instance.ShowPopup(openingMessages, () => {
                PlayerPrefs.SetInt("Seen_Level2_Intro", 1);
                PlayerPrefs.Save();
                StartLevel();
            });
        }
        else
        {
            GameLogger.LogWarning("Level2Manager: InfoPopupManager仍然为null，直接开始关卡");
            StartLevel();
        }
    }

    private void OnDestroy()
    {
        // 在对象销毁时取消订阅，防止内存泄漏
        if (levelManager != null)
        {
            levelManager.OnLevelCompleted -= HandleLevelCompletion;
        }
    }

    /// <summary>
    /// 关卡正式开始的逻辑
    /// </summary>
    private void StartLevel()
    {
        GameLogger.LogSystem("Level2Manager: 开场白结束，正式开始关卡。");
        EnableAllOperations();
    }

    /// <summary>
    /// 处理关卡完成事件
    /// </summary>
    private void HandleLevelCompletion()
    {
        GameLogger.LogSystem("Level2Manager: 关卡完成，直接进入通关界面。");
        DisableAllOperations();
        
        // 直接完成关卡，跳过通关结语
        FinishLevel();
    }
    
    /// <summary>
    /// 关卡收尾，通知GameFlowManager
    /// </summary>
    private void FinishLevel()
    {
        GameLogger.LogSystem("Level2Manager: 开始关卡收尾流程");
        
        // 验证Bootstrap状态
        if (!GameBootstrap.IsInitialized)
        {
            GameLogger.LogWarning("Level2Manager: Bootstrap未完成初始化，尝试重新初始化");
            GameBootstrap.EnsureInitialized();
        }
        
        // 获取正确的场景名
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        GameLogger.LogSystem($"Level2Manager: 当前场景名: {currentSceneName}");
        
        if (GameFlowManager.Instance != null)
        {
            GameLogger.LogSystem("Level2Manager: GameFlowManager实例存在，调用CompleteLevel");
            GameFlowManager.Instance.CompleteLevel(currentSceneName);
        }
        else
        {
            GameLogger.LogError("Level2Manager: 未找到GameFlowManager实例，无法完成关卡。");
            GameLogger.LogError($"Level2Manager: Bootstrap状态: {GameBootstrap.IsInitialized}");
            GameLogger.LogError($"Level2Manager: AudioManager实例: {(AudioManager.Instance != null ? "存在" : "null")}");
            GameLogger.LogError($"Level2Manager: InfoPopupManager实例: {(InfoPopupManager.Instance != null ? "存在" : "null")}");
            
            // 尝试最后一次创建GameFlowManager
            GameLogger.LogSystem("Level2Manager: 尝试紧急创建GameFlowManager");
            GameObject emergencyGameFlow = new GameObject("GameFlowManager_Emergency");
            emergencyGameFlow.AddComponent<GameFlowManager>();
            DontDestroyOnLoad(emergencyGameFlow);
            
            if (GameFlowManager.Instance != null)
            {
                GameLogger.LogSystem("Level2Manager: 紧急创建成功，继续完成关卡");
                GameFlowManager.Instance.CompleteLevel(currentSceneName);
            }
            else
            {
                GameLogger.LogError("Level2Manager: 紧急创建也失败了！跳过场景切换。");
            }
        }
    }
    
    // 设置Level2的BGM
    private void SetupLevel2BGM()
    {
        if (AudioManager.Instance != null)
        {
            // 播放雨天BGM
            if (AudioManager.Instance.bgmRainy != null)
            {
                AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmRainy);
                GameLogger.LogSystem("Level2Manager: 已设置Level2 BGM为bgmRainy");
            }
            else
            {
                GameLogger.LogWarning("Level2Manager: bgmRainy音频片段未设置");
            }
            
            // 播放雨声环境音
            if (AudioManager.Instance.ambientRain != null)
            {
                AudioManager.Instance.PlayAmbient(AudioManager.Instance.ambientRain);
                GameLogger.LogSystem("Level2Manager: 已播放雨声环境音");
            }
            else
            {
                GameLogger.LogWarning("Level2Manager: ambientRain音频片段未设置");
            }
        }
        else
        {
            GameLogger.LogWarning("Level2Manager: 未找到AudioManager实例");
        }
    }

    /// <summary>
    /// 禁用所有玩家操作
    /// </summary>
    private void DisableAllOperations()
    {
        GameLogger.LogSystem("Level2Manager: 禁用所有操作。");
        if (playerController != null)
        {
            for (int i = 0; i < playerController.GetPlayerCount(); i++)
            {
                Player player = playerController.GetPlayerByIndex(i);
                if (player != null)
                {
                    player.SetInputEnabled(false);
                    player.SetEnterKeyEnabled(false);
                }
            }
            playerController.DisablePlayerSwitching();
        }
        else
        {
            GameLogger.LogWarning("Level2Manager: PlayerController为null，无法禁用操作。");
        }
    }
    
    // 启用所有操作（移动、切换、回车、空格）
    private void EnableAllOperations()
    {
        GameLogger.LogSystem("Level2Manager: 启用所有操作");
        
        if (playerController != null)
        {
            // 启用所有玩家的移动和回车键响应
            for (int i = 0; i < playerController.GetPlayerCount(); i++)
            {
                Player player = playerController.GetPlayerByIndex(i);
                if (player != null)
                {
                    // 启用移动
                    player.SetInputEnabled(true);
                    // 启用回车键响应
                    player.SetEnterKeyEnabled(true);
                }
            }
            
            // 设置第一个玩家为当前玩家
            if (playerController.GetPlayerCount() > 0)
            {
                playerController.SetCurrentPlayerIndex(0);
            }
            
            // 启用玩家切换功能
            playerController.EnablePlayerSwitching();
            
            // 更新玩家颜色状态（当前操控的玩家正常颜色，其他玩家灰色）
            playerController.UpdatePlayerColors();
            
            GameLogger.LogSystem("Level2Manager: 已启用所有移动、切换、回车、空格操作，并设置玩家颜色状态");
        }
        else
        {
            GameLogger.LogWarning("Level2Manager: PlayerController为null，无法启用操作");
        }
    }
}
