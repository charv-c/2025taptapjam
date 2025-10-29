using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Level4场景管理器 - 管理角色切换和相关逻辑
/// 支持从任意场景启动，自动处理依赖初始化。
/// </summary>
public class Level4Manager : MonoBehaviour, IBootstrapAware
{
    [Header("调试设置")]
    [SerializeField] private bool showDebugInfo = true;
    
    // 关卡流程控制
    private LevelManager levelManager;
    private PlayerController playerController;
    
    // Bootstrap状态
    // private bool bootstrapCompleted = false; // 已移除未使用的字段
    private bool sceneInitialized = false;
    
    // 引导（开场流程）完成标志：用于控制F键互动与玩家切换
    // Level4有开场白，所以一开始设为false
    private bool guideCompleted = false;

    // 关卡开场白文案
    private readonly string[] openingMessages =
    {
        "执笔人，断桥情缘正遭劫难。",
        "请化身白青二蛇，守护这千年之恋。",
    };

    private void Start()
    {
        GameLogger.LogSystem("Level4Manager: 开始初始化level4场景");
        
        // 确保Bootstrap系统初始化
        GameBootstrap.EnsureInitialized();
        
        // 设置Level4的目标列表
        SetLevel4Targets();
        
        // 设置当前关卡进度
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.SetCurrentLevel("level4");
            GameLogger.LogSystem("Level4Manager: 已设置当前关卡为 level4");
        }
        
        // 获取对其他管理器的引用
        playerController = FindObjectOfType<PlayerController>();
        levelManager = GetComponent<LevelManager>();
        if (levelManager != null)
        {
            // 订阅关卡完成事件
            levelManager.OnLevelCompleted += HandleLevelCompletion;
            GameLogger.LogSystem("Level4Manager: 已订阅LevelManager的OnLevelCompleted事件");
        }
        else
        {
            GameLogger.LogError("Level4Manager: 未找到LevelManager组件！请确保LevelManager组件已附加到Level4Manager GameObject上。");
            // 尝试添加LevelManager组件
            levelManager = gameObject.AddComponent<LevelManager>();
            levelManager.OnLevelCompleted += HandleLevelCompletion;
            GameLogger.LogSystem("Level4Manager: 已自动添加LevelManager组件并订阅事件");
        }

        // 关卡开始时禁用操作，等待Bootstrap和开场白
        DisableAllOperations();
        
        // 开始初始化协程
        StartCoroutine(InitializeLevel4Coroutine());
    }

    private void OnDestroy()
    {
        if (levelManager != null)
        {
            levelManager.OnLevelCompleted -= HandleLevelCompletion;
        }
    }
    
    /// <summary>
    /// 初始化Level4的协程 - 等待Bootstrap完成后开始
    /// </summary>
    private IEnumerator InitializeLevel4Coroutine()
    {
        // 等待Bootstrap完成
        while (!GameBootstrap.IsInitialized)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        GameLogger.LogSystem("Level4Manager: Bootstrap完成，开始场景初始化");
        
        // 设置BGM
        SetupLevel4BGM();
        
        // 等待一帧确保所有系统就绪
        yield return null;
        
        // 标记Bootstrap完成
        // bootstrapCompleted = true; // 已移除未使用的字段
        
        // 开始场景内容
        InitializeSceneContent();
    }
    
    /// <summary>
    /// IBootstrapAware接口实现 - Bootstrap完成时调用
    /// </summary>
    public void OnBootstrapComplete()
    {
        // bootstrapCompleted = true; // 已移除未使用的字段
        GameLogger.LogSystem("Level4Manager: 收到Bootstrap完成通知");
        
        if (!sceneInitialized)
        {
            InitializeSceneContent();
        }
    }
    
    /// <summary>
    /// 设置Level4的目标列表
    /// </summary>
    private void SetLevel4Targets()
    {
        // Level4的目标字符：桥、难、湖、续
        List<string> level4Targets = new List<string> { "桥", "难", "湖", "续" };
        
        // 重置目标完成状态
        PublicData.ResetTargetCompletion();
        
        // 设置Level4的目标列表
        PublicData.SetCurrentTargetList(level4Targets);
        
        GameLogger.LogSystem($"Level4Manager: 已设置Level4目标列表: [{string.Join(", ", level4Targets)}]");
        
        if (showDebugInfo)
        {
            GameLogger.LogDev($"Level4Manager: 目标列表设置完成，当前目标: [{string.Join(", ", PublicData.GetCurrentTargetList())}]");
        }
    }
    
    /// <summary>
    /// 设置Level4的BGM
    /// </summary>
    private void SetupLevel4BGM()
    {
        if (AudioManager.Instance != null)
        {
            // 播放Level4专用BGM
            if (AudioManager.Instance.bgmLevel4 != null)
            {
                AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmLevel4);
                
                if (showDebugInfo)
                {
                    GameLogger.LogDev("Level4Manager: 开始播放Level4 BGM");
                }
            }
            else
            {
                GameLogger.LogWarning("Level4Manager: Level4 BGM音频片段未设置");
            }
        }
        else
        {
            GameLogger.LogWarning("Level4Manager: AudioManager实例未找到，无法播放BGM");
        }
    }
    
    /// <summary>
    /// 初始化场景内容 - Level4有开场白，先显示开场白再开始游戏
    /// </summary>
    private void InitializeSceneContent()
    {
        if (sceneInitialized) return;
        
        sceneInitialized = true;
        
        // 检查是否已经完成过开场白（从存档恢复的情况）
        if (guideCompleted)
        {
            GameLogger.LogSystem("Level4Manager: 检测到开场白已完成，直接开始关卡");
            StartLevel();
            return;
        }
        
        GameLogger.LogSystem("Level4Manager: 开始显示Level4开场白");
        
        // 显示开场白，结束后再正式开始关卡
        if (InfoPopupManager.Instance != null)
        {
            InfoPopupManager.Instance.ShowPopup(openingMessages, () => {
                OnOpeningCompleted(); // 先处理开场白结束逻辑
                StartLevel(); // 再开始关卡
            });
        }
        else
        {
            GameLogger.LogWarning("Level4Manager: InfoPopupManager仍然为null，直接开始关卡");
            StartLevel();
        }
    }

    /// <summary>
    /// 开场白结束时的处理（在StartLevel之前调用）
    /// </summary>
    private void OnOpeningCompleted()
    {
        GameLogger.LogSystem("Level4Manager: 开场白结束，设置引导完成状态");
        // 视为Level4引导完成
        guideCompleted = true;
    }

    /// <summary>
    /// 关卡正式开始
    /// </summary>
    private void StartLevel()
    {
        GameLogger.LogSystem("Level4Manager: 开场白结束，正式开始关卡。");
        // 强制启用玩家移动，避免被其他管理器在Start中覆盖
        StartCoroutine(EnsureEnableMovementNextFrame());
    }

    /// <summary>
    /// 处理关卡完成事件
    /// </summary>
    private void HandleLevelCompletion()
    {
        GameLogger.LogSystem("Level4Manager: 关卡完成，直接进入通关界面。");
        DisableAllOperations();

        // 直接完成关卡，跳过通关结语
        FinishLevel();
    }

    /// <summary>
    /// 关卡收尾，通知GameFlowManager
    /// </summary>
    private void FinishLevel()
    {
        GameLogger.LogSystem("Level4Manager: 开始关卡收尾流程");
        
        // 验证Bootstrap状态
        if (!GameBootstrap.IsInitialized)
        {
            GameLogger.LogWarning("Level4Manager: Bootstrap未完成初始化，尝试重新初始化");
            GameBootstrap.EnsureInitialized();
        }
        
        // 获取正确的场景名
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        GameLogger.LogSystem($"Level4Manager: 当前场景名: {currentSceneName}");
        
        if (GameFlowManager.Instance != null)
        {
            GameLogger.LogSystem("Level4Manager: GameFlowManager实例存在，调用CompleteLevel");
            GameFlowManager.Instance.CompleteLevel(currentSceneName);
        }
        else
        {
            GameLogger.LogError("Level4Manager: 未找到GameFlowManager实例，无法完成关卡。");
            GameLogger.LogError($"Level4Manager: Bootstrap状态: {GameBootstrap.IsInitialized}");
            GameLogger.LogError($"Level4Manager: AudioManager实例: {(AudioManager.Instance != null ? "存在" : "null")}");
            GameLogger.LogError($"Level4Manager: InfoPopupManager实例: {(InfoPopupManager.Instance != null ? "存在" : "null")}");
            
            // 尝试最后一次创建GameFlowManager
            GameLogger.LogSystem("Level4Manager: 尝试紧急创建GameFlowManager");
            GameObject emergencyGameFlow = new GameObject("GameFlowManager_Emergency");
            emergencyGameFlow.AddComponent<GameFlowManager>();
            DontDestroyOnLoad(emergencyGameFlow);
            
            if (GameFlowManager.Instance != null)
            {
                GameLogger.LogSystem("Level4Manager: 紧急创建成功，继续完成关卡");
                GameFlowManager.Instance.CompleteLevel(currentSceneName);
            }
            else
            {
                GameLogger.LogError("Level4Manager: 紧急创建也失败了！跳过场景切换。");
            }
        }
    }

    private System.Collections.IEnumerator EnsureEnableMovementNextFrame()
    {
        yield return new WaitForEndOfFrame();
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.EnableCurrentPlayerMovement();
            // 始终开启移动/基础输入，但按引导完成与否控制F键与切换
            int count = playerController.GetPlayerCount();
            for (int i = 0; i < count; i++)
            {
                Player p = playerController.GetPlayerByIndex(i);
                if (p != null)
                {
                    p.SetInputEnabled(true);
                    p.SetFKeyEnabled(guideCompleted);
                }
            }

            // 参照Level2：设置当前玩家索引、启用切换与颜色更新
            if (playerController.GetPlayerCount() > 0)
            {
                playerController.SetCurrentPlayerIndex(0);
            }
            if (guideCompleted)
            {
                playerController.EnablePlayerSwitching();
            }
            else
            {
                playerController.DisablePlayerSwitching();
            }
            playerController.UpdatePlayerColors();

            if (showDebugInfo)
            {
                GameLogger.LogDev("Level4Manager: 已启用移动/输入，并更新玩家颜色与切换状态");
            }
        }
        else
        {
            GameLogger.LogWarning("Level4Manager: 未找到PlayerController，无法启用玩家移动");
        }
    }
    
    /// <summary>
    /// 禁用所有玩家操作
    /// </summary>
    private void DisableAllOperations()
    {
        GameLogger.LogSystem("Level4Manager: 禁用所有操作。");
        if (playerController != null)
        {
            for (int i = 0; i < playerController.GetPlayerCount(); i++)
            {
                Player player = playerController.GetPlayerByIndex(i);
                if (player != null)
                {
                    player.SetInputEnabled(false);
                    player.SetFKeyEnabled(false);
                }
            }
            playerController.DisablePlayerSwitching();
        }
        else
        {
            GameLogger.LogWarning("Level4Manager: PlayerController为null，无法禁用操作。");
        }
    }
    
    /// <summary>
    /// 启用所有玩家操作
    /// </summary>
    private void EnableAllOperations()
    {
        GameLogger.LogSystem("Level4Manager: 启用所有操作");
        
        if (playerController != null)
        {
            // 启用所有玩家的移动和F键响应
            for (int i = 0; i < playerController.GetPlayerCount(); i++)
            {
                Player player = playerController.GetPlayerByIndex(i);
                if (player != null)
                {
                    // 启用移动
                    player.SetInputEnabled(true);
                    // 根据引导完成状态启用F键响应
                    player.SetFKeyEnabled(guideCompleted);
                }
            }
            
            // 设置第一个玩家为当前玩家
            if (playerController.GetPlayerCount() > 0)
            {
                playerController.SetCurrentPlayerIndex(0);
            }
            
            // 根据引导完成状态启用玩家切换功能
            if (guideCompleted)
            {
                playerController.EnablePlayerSwitching();
            }
            else
            {
                playerController.DisablePlayerSwitching();
            }
            
            // 更新玩家颜色状态（当前操控的玩家正常颜色，其他玩家灰色）
            playerController.UpdatePlayerColors();
            
            GameLogger.LogSystem($"Level4Manager: 已启用所有移动操作，F键和切换状态: {guideCompleted}");
        }
        else
        {
            GameLogger.LogWarning("Level4Manager: PlayerController为null，无法启用操作");
        }
    }

    /// <summary>
    /// 是否已完成Level4引导
    /// </summary>
    public bool IsGuideCompleted()
    {
        return guideCompleted;
    }

    /// <summary>
    /// 设置Level4引导完成标志（用于存档恢复）
    /// </summary>
    public void SetGuideCompleted(bool completed)
    {
        guideCompleted = completed;
        GameLogger.LogSystem($"Level4Manager: 引导完成状态设置为 {completed}");
    }
    
    /// <summary>
    /// 存档恢复后设置玩家控制状态（参考Level2和Level3的实现）
    /// </summary>
    public void SetupPlayerControlsAfterRestore()
    {
        GameLogger.LogSystem("Level4Manager: 存档恢复后设置玩家控制状态");
        
        if (playerController != null)
        {
            // 始终确保可移动
            playerController.EnableCurrentPlayerMovement();
            
            // 根据引导状态控制F键与切换
            for (int i = 0; i < playerController.GetPlayerCount(); i++)
            {
                Player player = playerController.GetPlayerByIndex(i);
                if (player != null)
                {
                    player.SetInputEnabled(true);
                    player.SetFKeyEnabled(guideCompleted);
                }
            }
            
            if (playerController.GetPlayerCount() > 0)
            {
                playerController.SetCurrentPlayerIndex(0);
            }
            
            if (guideCompleted)
            {
                playerController.EnablePlayerSwitching();
            }
            else
            {
                playerController.DisablePlayerSwitching();
            }
            
            playerController.UpdatePlayerColors();
            GameLogger.LogSystem($"Level4Manager: 存档恢复后玩家控制设置完成 - guideCompleted={guideCompleted}");
        }
        else
        {
            GameLogger.LogWarning("Level4Manager: PlayerController为null，无法设置玩家控制状态");
        }
    }
}
