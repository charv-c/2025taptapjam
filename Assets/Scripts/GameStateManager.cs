using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 游戏状态管理器 - 负责保存和恢复关卡内的详细状态
/// 包括物体显隐、Highlight脚本状态、广播历史、可用字符串等
/// </summary>
public class GameStateManager : MonoBehaviour
{
    // 单例实例
    public static GameStateManager Instance { get; private set; }
    
    [Header("保存设置")]
    [SerializeField] private bool enableDebugLog = true;
    [SerializeField] private bool autoSaveOnExit = true;
    
    // PlayerPrefs键名常量 (将被废弃)
    /*
    private const string GAME_STATE_KEY = "GameState_";
    private const string BROADCAST_HISTORY_KEY = "BroadcastHistory_";
    private const string AVAILABLE_STRINGS_KEY = "AvailableStrings_";
    private const string CURRENT_SEASON_KEY = "CurrentSeason_";
    private const string COLLECTED_STRINGS_KEY = "CollectedStrings_";
    */
    
    // 当前关卡名称
    private string currentLevelName;
    
    // 游戏状态数据类 (已迁移到 GameProgressData.cs, 此处定义将被移除)
    /*
    [System.Serializable]
    public class GameObjectState
    {
        public string objectName;
        public string objectPath;
        public bool isActive;
        public bool isActiveSelf;
        public bool hasHighlight;
        public bool highlightEnabled;
        public string highlightLetter;
        public Vector3 position;
        public bool hasSpriteRenderer;
        public bool spriteRendererEnabled;
        public bool hasCollider2D;
        public bool collider2DEnabled;
        public bool hasRenderer;
        public bool rendererEnabled;
        public bool hasLight2D;
        public bool light2DEnabled;
        public bool hasPlayer;
        public string playerCarryCharacter;
        public bool playerInputEnabled;
        public bool playerEnterKeyEnabled;
    }
    
    [System.Serializable]
    public class FlyingCharacterData
    {
        public string character;
        public string targetObjectName;
        public Vector3 targetPosition;
        public float delay;
    }
    
    [System.Serializable]
    public class BeachObjectState
    {
        public bool hasYaBeenPlanted;
    }
    
    [System.Serializable]
    public class GameStateData
    {
        public string levelName;
        public List<GameObjectState> objectStates;
        public List<string> broadcastHistory;
        public List<string> availableStrings;
        public string currentSeason;
        public List<string> collectedStrings;
        public List<FlyingCharacterData> flyingCharacters;
        public List<string> completedTargets;
        public List<string> currentTargetList;
        public BeachObjectState beachObjectState;
        public float saveTime;
    }
    */
    
    private void Awake()
    {
        // 实现单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentLevelName = SceneManager.GetActiveScene().name;
            GameLogger.LogSystem("GameStateManager 初始化并设置为跨场景持久化");
            // 订阅场景加载事件，确保每次进入场景后尝试恢复该场景的存档
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // 检查是否需要恢复游戏状态
        if (ShouldRestoreGameState())
        {
            StartCoroutine(RestoreGameStateDelayed());
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            // 取消订阅，避免重复绑定或空引用
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    /// <summary>
    /// 场景加载完成时回调：尝试恢复对应场景的存档
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 延迟一点点再恢复，确保场景物体与各系统初始化完成
        if (ShouldRestoreGameState())
        {
            StartCoroutine(RestoreGameStateDelayed());
        }
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && autoSaveOnExit)
        {
            SaveGameState();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && autoSaveOnExit)
        {
            SaveGameState();
        }
    }
    
    /// <summary>
    /// 保存当前游戏状态
    /// </summary>
    public void SaveGameState()
    {
        currentLevelName = SceneManager.GetActiveScene().name;
        if (IsStartupScene(currentLevelName) || IsLevel1Scene(currentLevelName) || IsEndLevelScene(currentLevelName))
        {
            LogDebug($"跳过保存，场景不参与存档: {currentLevelName}");
            return;
        }
        
        // 存档前，确保退出弹框隐藏，避免下次进入时立即显示
        if (ExitGameManager.Instance != null)
        {
            ExitGameManager.Instance.EnsureExitDialogHidden();
        }

        // 检查 LevelProgressManager 是否存在
        if (LevelProgressManager.Instance == null)
        {
            LogError("LevelProgressManager 不存在，无法保存关卡状态！");
            return;
        }

        LogDebug($"开始收集关卡 {currentLevelName} 的状态数据");
        
        var objectStates = CollectObjectStates();
        var broadcastHistory = GetBroadcastHistory();
        var availableStrings = GetAvailableStrings();
        var collectedStrings = GetCollectedStrings();
        var flyingCharacters = CollectFlyingCharacters();
        var completedTargets = GetCompletedTargets();
        var currentTargetList = GetCurrentTargetList();
        var beachObjectState = GetBeachObjectState();
        var countdownTimerStates = CollectCountdownTimerStates();
        
        var stateData = new GameProgressData.LevelStateData
        {
            levelName = currentLevelName,
            guideCompleted = GetLevel3GuideCompletedIfAny(currentLevelName) || GetLevel2GuideCompletedIfAny(currentLevelName),
            objectStates = objectStates,
            broadcastHistory = broadcastHistory,
            availableStrings = availableStrings,
            currentSeason = GetCurrentSeason(),
            collectedStrings = collectedStrings,
            flyingCharacters = flyingCharacters,
            completedTargets = completedTargets,
            currentTargetList = currentTargetList,
            beachObjectState = beachObjectState,
            countdownTimerStates = countdownTimerStates,
            saveTime = Time.time
        };
        
        LogDebug($"状态数据收集完成: 物体{objectStates.Count}个, 广播{broadcastHistory.Count}条, 飞字{flyingCharacters.Count}个, 可用字符串{availableStrings.Count}个, 已收集字符串{collectedStrings.Count}个, 倒计时{countdownTimerStates.Count}个");
        
        // **重构核心**：调用 LevelProgressManager 来保存关卡状态，而不是写入PlayerPrefs
        LevelProgressManager.Instance.SaveLevelState(currentLevelName, stateData);
        
        LogDebug($"游戏状态已通过 LevelProgressManager 保存 - 关卡: {currentLevelName}, 物体数量: {stateData.objectStates.Count}");
    }
    
    /// <summary>
    /// 恢复游戏状态
    /// </summary>
    public void RestoreGameState()
    {
        currentLevelName = SceneManager.GetActiveScene().name;
        if (IsStartupScene(currentLevelName) || IsLevel1Scene(currentLevelName) || IsEndLevelScene(currentLevelName))
        {
            LogDebug($"跳过恢复，场景不参与存档: {currentLevelName}");
            return;
        }
        
        // **重构核心**: 从 LevelProgressManager 加载关卡状态数据
        if (LevelProgressManager.Instance == null)
        {
            LogError("LevelProgressManager 不存在，无法恢复关卡状态！");
            return;
        }
        
        var stateData = LevelProgressManager.Instance.LoadLevelState(currentLevelName);

        if (stateData == null)
        {
            LogDebug($"没有找到关卡 {currentLevelName} 的保存数据");
            return;
        }
        
        try
        {
            ApplyGameState(stateData);
            LogDebug($"游戏状态已恢复 - 关卡: {currentLevelName}, 物体数量: {stateData.objectStates.Count}");
        }
        catch (System.Exception e)
        {
            LogError($"恢复游戏状态失败: {e.Message}");
        }
    }
    
    /// <summary>
    /// 清除当前关卡的保存数据
    /// </summary>
    public void ClearGameState()
    {
        currentLevelName = SceneManager.GetActiveScene().name;
        if (IsStartupScene(currentLevelName) || IsLevel1Scene(currentLevelName))
        {
            LogDebug($"跳过清理，场景不参与存档: {currentLevelName}");
            return;
        }
        
        // **重构核心**: 调用 LevelProgressManager 来清除关卡状态
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.ClearLevelState(currentLevelName);
            LogDebug($"关卡 {currentLevelName} 的保存数据已通过 LevelProgressManager 清除");
        }
        else
        {
            LogError($"LevelProgressManager 不存在，无法清除关卡 {currentLevelName} 的保存数据");
        }
    }
    
    /// <summary>
    /// 检查是否应该恢复游戏状态
    /// </summary>
    private bool ShouldRestoreGameState()
    {
        // **重构核心**: 从 LevelProgressManager 检查是否存在存档
        currentLevelName = SceneManager.GetActiveScene().name;
        if (IsStartupScene(currentLevelName) || IsLevel1Scene(currentLevelName) || IsEndLevelScene(currentLevelName)) return false;

        if (LevelProgressManager.Instance != null)
        {
            return LevelProgressManager.Instance.LoadLevelState(currentLevelName) != null;
        }
        
        return false;
    }
    
    /// <summary>
    /// 延迟恢复游戏状态（等待场景完全加载）
    /// </summary>
    private System.Collections.IEnumerator RestoreGameStateDelayed()
    {
        yield return new WaitForSeconds(0.5f); // 等待场景完全加载
        RestoreGameState();
    }
    
    /// <summary>
    /// 收集所有物体的状态
    /// </summary>
    private List<GameProgressData.GameObjectState> CollectObjectStates()
    {
        var objectStates = new List<GameProgressData.GameObjectState>();

        var activeScene = SceneManager.GetActiveScene();
        LogDebug($"开始收集物体状态 - 场景: {activeScene.name}");

        // 遍历场景的所有根对象（包含未激活），并递归收集（包含未激活的子物体）
        var roots = activeScene.GetRootGameObjects();
        LogDebug($"场景根对象数量: {roots.Length}");
        
        foreach (var root in roots)
        {
            int countBefore = objectStates.Count;
            CollectFromHierarchyRecursive(root, objectStates);
            int countAfter = objectStates.Count;
            LogDebug($"从根对象 {root.name} 收集到 {countAfter - countBefore} 个物体状态");
        }

        LogDebug($"收集完成 - 总计收集到 {objectStates.Count} 个物体状态");
        return objectStates;
    }

    /// <summary>
    /// 递归收集层级内所有对象（包含未激活对象）
    /// </summary>
    private void CollectFromHierarchyRecursive(GameObject obj, List<GameProgressData.GameObjectState> collector)
    {
        if (obj == null) return;

        // 获取UniqueID（如果存在）
        UniqueID uniqueIDComponent = obj.GetComponent<UniqueID>();
        
        var state = new GameProgressData.GameObjectState
        {
            objectName = obj.name,
            objectPath = GetGameObjectPath(obj), // 保留作为后备
            uniqueId = uniqueIDComponent?.ID ?? "", // 优先使用UniqueID
            // 兼容：保留旧字段，同时新增activeSelf
            isActive = obj.activeInHierarchy,
            isActiveSelf = obj.activeSelf,
            position = obj.transform.position
        };

        // 检查Highlight组件
        Highlight highlight = obj.GetComponent<Highlight>();
        if (highlight != null)
        {
            state.hasHighlight = true;
            state.highlightEnabled = highlight.enabled;
            state.highlightLetter = highlight.letter;
        }

        // 检查SpriteRenderer组件
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            state.hasSpriteRenderer = true;
            state.spriteRendererEnabled = spriteRenderer.enabled;
        }

        // 检查Collider2D组件（取第一个）
        Collider2D collider2D = obj.GetComponent<Collider2D>();
        if (collider2D != null)
        {
            state.hasCollider2D = true;
            state.collider2DEnabled = collider2D.enabled;
        }

        // 检查通用Renderer（如MeshRenderer、SpriteRenderer的基类）
        Renderer baseRenderer = obj.GetComponent<Renderer>();
        if (baseRenderer != null)
        {
            state.hasRenderer = true;
            state.rendererEnabled = baseRenderer.enabled;
        }

        // 检查Light2D组件（URP）
        Light2D light2D = obj.GetComponent<Light2D>();
        if (light2D != null)
        {
            state.hasLight2D = true;
            state.light2DEnabled = light2D.enabled;
        }

        // 检查Player组件（保存携带字符和输入状态）
        Player player = obj.GetComponent<Player>();
        if (player != null)
        {
            state.hasPlayer = true;
            state.playerCarryCharacter = player.GetCarryCharacter();
            state.playerInputEnabled = player.IsInputEnabled();
            state.playerEnterKeyEnabled = player.IsEnterKeyEnabled();
        }

        collector.Add(state);

        // 递归子节点（包含未激活）
        Transform t = obj.transform;
        for (int i = 0; i < t.childCount; i++)
        {
            CollectFromHierarchyRecursive(t.GetChild(i).gameObject, collector);
        }
    }
    
    /// <summary>
    /// 获取GameObject的完整路径
    /// </summary>
    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
    
    /// <summary>
    /// 根据路径查找GameObject
    /// </summary>
    private GameObject FindGameObjectByPath(string path)
    {
        string[] pathParts = path.Split('/');
        if (pathParts.Length == 0) return null;

        // 在当前场景根对象中查找首段（包含未激活）
        var activeScene = SceneManager.GetActiveScene();
        var roots = activeScene.GetRootGameObjects();
        GameObject root = null;
        foreach (var r in roots)
        {
            if (r.name == pathParts[0])
            {
                root = r;
                break;
            }
        }
        if (root == null) return null;

        // 逐级在子层级中查找（Transform.Find 在已有父引用时能找到未激活子物体）
        Transform current = root.transform;
        for (int i = 1; i < pathParts.Length; i++)
        {
            Transform child = current.Find(pathParts[i]);
            if (child == null) 
            {
                // 如果Transform.Find找不到，尝试通过遍历所有子物体来查找
                child = FindChildByName(current, pathParts[i]);
                if (child == null) return null;
            }
            current = child;
        }

        return current.gameObject;
    }
    
    /// <summary>
    /// 根据状态数据查找GameObject（优先使用UniqueID，后备使用路径）
    /// </summary>
    private GameObject FindGameObjectByStateData(GameProgressData.GameObjectState stateData)
    {
        GameObject obj = null;
        
        // 1. 优先尝试使用UniqueID查找
        if (!string.IsNullOrEmpty(stateData.uniqueId))
        {
            obj = UniqueIDManager.FindGameObjectByID(stateData.uniqueId);
            if (obj != null)
            {
                LogDebug($"通过UniqueID找到对象: {stateData.objectName} ({stateData.uniqueId})");
                return obj;
            }
            else
            {
                LogDebug($"UniqueID未找到对象: {stateData.objectName} ({stateData.uniqueId})，尝试路径查找");
            }
        }
        
        // 2. 后备：使用路径查找
        if (!string.IsNullOrEmpty(stateData.objectPath))
        {
            obj = FindGameObjectByPath(stateData.objectPath);
            if (obj != null)
            {
                LogDebug($"通过路径找到对象: {stateData.objectName} ({stateData.objectPath})");
                
                // 如果对象没有UniqueID组件，考虑为其添加
                UniqueID uniqueIDComponent = obj.GetComponent<UniqueID>();
                if (uniqueIDComponent == null)
                {
                    LogDebug($"为对象 {obj.name} 添加UniqueID组件");
                    uniqueIDComponent = obj.AddComponent<UniqueID>();
                }
                
                return obj;
            }
        }
        
        // 3. 最后尝试：按名称在场景中查找（最不可靠）
        if (!string.IsNullOrEmpty(stateData.objectName))
        {
            obj = GameObject.Find(stateData.objectName);
            if (obj != null)
            {
                LogDebug($"通过名称找到对象: {stateData.objectName}（警告：这种方式不可靠）");
                
                // 为找到的对象添加UniqueID
                UniqueID uniqueIDComponent = obj.GetComponent<UniqueID>();
                if (uniqueIDComponent == null)
                {
                    LogDebug($"为对象 {obj.name} 添加UniqueID组件");
                    uniqueIDComponent = obj.AddComponent<UniqueID>();
                }
                
                return obj;
            }
        }
        
        return null; // 所有方法都失败
    }
    
    /// <summary>
    /// 通过遍历所有子物体来查找指定名称的子物体
    /// </summary>
    private Transform FindChildByName(Transform parent, string childName)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == childName)
            {
                return child;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 应用游戏状态
    /// </summary>
    private void ApplyGameState(GameProgressData.LevelStateData stateData)
    {
        // 先建立存档中的路径集合
        var savedPaths = new HashSet<string>();
        foreach (var s in stateData.objectStates)
        {
            if (!string.IsNullOrEmpty(s.objectPath)) savedPaths.Add(s.objectPath);
        }

        // 恢复物体状态
        foreach (var objState in stateData.objectStates)
        {
            GameObject obj = FindGameObjectByStateData(objState);
            if (obj == null) 
            {
                LogDebug($"未找到对象: UniqueID={objState.uniqueId}, Path={objState.objectPath}, Name={objState.objectName}");
                continue;
            }
            
            // 恢复GameObject激活状态：兼容新旧存档
            // 新版使用 activeSelf；旧版只有 isActive（activeInHierarchy）。
            // 采用二者“或”以避免旧存档缺省字段导致的反转。
            bool targetActive = objState.isActiveSelf || objState.isActive;
            obj.SetActive(targetActive);
            
            // 恢复位置：跳过UI元素（RectTransform），避免破坏解字台等UI的布局
            if (obj.GetComponent<RectTransform>() == null)
            {
                obj.transform.position = objState.position;
            }
            
            // 恢复Highlight组件状态
            if (objState.hasHighlight)
            {
                Highlight highlight = obj.GetComponent<Highlight>();
                if (highlight != null)
                {
                    highlight.enabled = objState.highlightEnabled;
                    // 注意：letter属性通常不需要恢复，因为它应该在场景中已经设置好
                }
            }
            
            // 恢复SpriteRenderer状态
            if (objState.hasSpriteRenderer)
            {
                SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = objState.spriteRendererEnabled;
                }
            }

            // 恢复Collider2D状态
            if (objState.hasCollider2D)
            {
                Collider2D collider2D = obj.GetComponent<Collider2D>();
                if (collider2D != null)
                {
                    collider2D.enabled = objState.collider2DEnabled;
                }
            }

            // 恢复Renderer状态
            if (objState.hasRenderer)
            {
                Renderer baseRenderer = obj.GetComponent<Renderer>();
                if (baseRenderer != null)
                {
                    baseRenderer.enabled = objState.rendererEnabled;
                }
            }

            // 恢复Light2D状态
            if (objState.hasLight2D)
            {
                Light2D light2D = obj.GetComponent<Light2D>();
                if (light2D != null)
                {
                    light2D.enabled = objState.light2DEnabled;
                }
            }

            // 恢复Player携带字符、输入状态并更新米字格
            if (objState.hasPlayer)
            {
                Player player = obj.GetComponent<Player>();
                if (player != null)
                {
                    if (!string.IsNullOrEmpty(objState.playerCarryCharacter))
                    {
                        player.SetCarryCharacter(objState.playerCarryCharacter);
                    }
                    // 恢复输入状态
                    player.SetInputEnabled(objState.playerInputEnabled);
                    player.SetEnterKeyEnabled(objState.playerEnterKeyEnabled);
                }
            }
        }
        
        // 恢复广播历史（不重放），避免重复触发AutoHint等逻辑
        if (BroadcastManager.Instance != null)
        {
            BroadcastManager.Instance.ReplaceHistory(stateData.broadcastHistory);
            LogDebug($"已恢复广播历史记录（不重播），数量: {stateData.broadcastHistory?.Count ?? 0}");
        }
        
        // 恢复可用字符串
        RestoreAvailableStrings(stateData.availableStrings);
        
        // 恢复当前季节
        RestoreCurrentSeason(stateData.currentSeason);
        
        // 恢复收集的字符串
        RestoreCollectedStrings(stateData.collectedStrings);

        // 恢复飞字物体
        RestoreFlyingCharacters(stateData.flyingCharacters);

        // 恢复目标完成情况
        RestoreTargetCompletion(stateData.completedTargets, stateData.currentTargetList);

        // 恢复BeachObject状态
        RestoreBeachObjectState(stateData.beachObjectState);

        // 恢复倒计时状态
        RestoreCountdownTimerStates(stateData.countdownTimerStates);

        // 清理存档中不存在的场景对象
        DestroyObjectsNotInSave(savedPaths);
        
        // 如果是Level3，先将引导完成标志同步到管理器
        if ((stateData.levelName ?? "").Trim().ToLowerInvariant().Contains("level3"))
        {
            Level3Manager l3 = FindObjectOfType<Level3Manager>();
            if (l3 != null)
            {
                l3.SetGuideCompleted(stateData.guideCompleted);
            }
        }
        
        // 如果是Level2，先将引导完成标志同步到管理器
        if ((stateData.levelName ?? "").Trim().ToLowerInvariant().Contains("level2"))
        {
            Level2Manager l2 = FindObjectOfType<Level2Manager>();
            if (l2 != null)
            {
                l2.SetGuideCompleted(stateData.guideCompleted);
            }
        }
        
        // 如果是Level4，先将引导完成标志同步到管理器
        if ((stateData.levelName ?? "").Trim().ToLowerInvariant().Contains("level4"))
        {
            Level4Manager l4 = FindObjectOfType<Level4Manager>();
            if (l4 != null)
            {
                l4.SetGuideCompleted(stateData.guideCompleted);
            }
        }

        // 特殊处理：Level3场景恢复后按引导标志设置回车与玩家切换
        if (currentLevelName.ToLower().Contains("level3"))
        {
            StartCoroutine(CheckLevel3PlayerMovementAfterRestore());
        }
        else if (currentLevelName.ToLower().Contains("level2"))
        {
            // Level2场景恢复后按引导标志设置回车与玩家切换
            StartCoroutine(CheckLevel2PlayerMovementAfterRestore());
        }
        else if (currentLevelName.ToLower().Contains("level4"))
        {
            // Level4场景恢复后按引导标志设置回车与玩家切换
            StartCoroutine(CheckLevel4PlayerMovementAfterRestore());
        }
        else
        {
            // 其他场景：确保PlayerController正确设置当前玩家的输入状态
            StartCoroutine(EnsurePlayerControllerStateAfterRestore());
        }
    }

    /// <summary>
    /// 检查Level3场景恢复后是否需要重新启用玩家移动
    /// </summary>
    private System.Collections.IEnumerator CheckLevel3PlayerMovementAfterRestore()
    {
        // 等待一帧确保所有系统初始化完成
        yield return null;
        
        // 检查Level3Manager是否存在且场景已初始化
        Level3Manager level3Manager = FindObjectOfType<Level3Manager>();
        if (level3Manager != null)
        {
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                bool guideCompleted = level3Manager.IsGuideCompleted();

                // 始终确保可移动
                playerController.EnableCurrentPlayerMovement();

                // 根据引导状态控制回车与切换
                for (int i = 0; i < playerController.GetPlayerCount(); i++)
                {
                    Player player = playerController.GetPlayerByIndex(i);
                    if (player != null)
                    {
                        player.SetInputEnabled(true);
                        player.SetEnterKeyEnabled(guideCompleted);
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
                LogDebug($"Level3恢复：guideCompleted={guideCompleted}，已应用回车/切换权限");
            }
        }
    }
    
    /// <summary>
    /// 检查Level2场景恢复后是否需要重新启用玩家移动
    /// </summary>
    private System.Collections.IEnumerator CheckLevel2PlayerMovementAfterRestore()
    {
        // 等待一帧确保所有系统初始化完成
        yield return null;
        
        // 检查Level2Manager是否存在且场景已初始化
        Level2Manager level2Manager = FindObjectOfType<Level2Manager>();
        if (level2Manager != null)
        {
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                bool guideCompleted = level2Manager.IsGuideCompleted();

                // 始终确保可移动
                playerController.EnableCurrentPlayerMovement();

                // 根据引导状态控制回车与切换
                for (int i = 0; i < playerController.GetPlayerCount(); i++)
                {
                    Player player = playerController.GetPlayerByIndex(i);
                    if (player != null)
                    {
                        player.SetInputEnabled(true);
                        player.SetEnterKeyEnabled(guideCompleted);
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
                LogDebug($"Level2恢复：guideCompleted={guideCompleted}，已应用回车/切换权限");
            }
        }
    }
    
    /// <summary>
    /// 检查Level4场景恢复后是否需要重新启用玩家移动
    /// </summary>
    private System.Collections.IEnumerator CheckLevel4PlayerMovementAfterRestore()
    {
        // 等待一帧确保所有系统初始化完成
        yield return null;
        
        // 检查Level4Manager是否存在且场景已初始化
        Level4Manager level4Manager = FindObjectOfType<Level4Manager>();
        if (level4Manager != null)
        {
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                bool guideCompleted = level4Manager.IsGuideCompleted();

                // 始终确保可移动
                playerController.EnableCurrentPlayerMovement();

                // 根据引导状态控制回车与切换
                for (int i = 0; i < playerController.GetPlayerCount(); i++)
                {
                    Player player = playerController.GetPlayerByIndex(i);
                    if (player != null)
                    {
                        player.SetInputEnabled(true);
                        player.SetEnterKeyEnabled(guideCompleted);
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
                LogDebug($"Level4恢复：guideCompleted={guideCompleted}，已应用回车/切换权限");
            }
        }
    }
    
    /// <summary>
    /// 确保PlayerController在存档恢复后正确设置当前玩家的输入状态
    /// </summary>
    private System.Collections.IEnumerator EnsurePlayerControllerStateAfterRestore()
    {
        // 等待一帧确保所有系统初始化完成
        yield return null;
        
        // 查找PlayerController
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            LogDebug("未找到PlayerController，跳过状态检查");
            yield break;
        }
        
        // 检查是否有任何玩家输入被禁用
        bool hasDisabledPlayers = false;
        for (int i = 0; i < playerController.GetPlayerCount(); i++)
        {
            Player player = playerController.GetPlayerByIndex(i);
            if (player != null && !player.IsInputEnabled())
            {
                hasDisabledPlayers = true;
                break;
            }
        }
        
        // 如果有玩家输入被禁用，说明这是存档恢复后的状态，需要重新启用
        if (hasDisabledPlayers)
        {
            LogDebug("检测到存档恢复后玩家输入被禁用，重新启用所有玩家移动");
            
            // 强制启用所有玩家输入
            playerController.ForceEnableAllPlayerInput();
            
            LogDebug("存档恢复后已重新启用所有玩家移动");
        }
        else
        {
            LogDebug("所有玩家输入已启用，无需额外处理");
        }
    }
    
    /// <summary>
    /// 销毁不在存档中的场景对象（仅限当前激活场景）
    /// </summary>
    private void DestroyObjectsNotInSave(HashSet<string> savedPaths)
    {
        var activeScene = SceneManager.GetActiveScene();
        var roots = activeScene.GetRootGameObjects();
        foreach (var root in roots)
        {
            DestroyIfNotSavedRecursive(root, savedPaths);
        }
    }

    private void DestroyIfNotSavedRecursive(GameObject obj, HashSet<string> savedPaths)
    {
        if (obj == null) return;

        // 先处理子节点，避免父先销毁导致迭代异常
        Transform t = obj.transform;
        List<GameObject> children = new List<GameObject>(t.childCount);
        for (int i = 0; i < t.childCount; i++) children.Add(t.GetChild(i).gameObject);
        foreach (var child in children)
        {
            DestroyIfNotSavedRecursive(child, savedPaths);
        }

        string path = GetGameObjectPath(obj);
        if (!savedPaths.Contains(path) && ShouldDestroyObject(obj))
        {
            LogDebug($"移除未在存档中的对象: {path}");
            Object.Destroy(obj);
        }
    }

    /// <summary>
    /// 判断对象是否允许被销毁（排除关键系统/管理器等）
    /// </summary>
    private bool ShouldDestroyObject(GameObject obj)
    {
        if (obj == null) return false;

        // 排除：隐藏在DontDestroyOnLoad场景的对象（不属于当前关卡）
        if (obj.scene.name != SceneManager.GetActiveScene().name) return false;

        // 排除：常见的关键系统/管理器（根据项目内常用类型排除）
        var excludedTypes = new System.Type[]
        {
            typeof(GameStateManager),
            typeof(LevelProgressManager),
            typeof(StartMenuManager),
            typeof(GameFlowManager),
            typeof(BroadcastManager),
            typeof(AudioManager),
            typeof(InfoPopupManager),
            typeof(ButtonController),
            typeof(PublicData),
            typeof(Level3Manager),
            typeof(PlayerController),
            typeof(SeasonParticleManager),
            typeof(BackgroundManager),
            typeof(TutorialManager),
            typeof(LevelManager)
        };

        foreach (var type in excludedTypes)
        {
            if (obj.GetComponent(type) != null) return false;
        }

        // 排除：场景主摄像机和EventSystem
        if (obj.GetComponent<UnityEngine.Camera>() != null) return false;
        if (obj.GetComponent<UnityEngine.EventSystems.EventSystem>() != null) return false;

        return true;
    }
    
    /// <summary>
    /// 获取广播历史
    /// </summary>
    private List<string> GetBroadcastHistory()
    {
        List<string> history = new List<string>();
        
        // 从BroadcastManager获取历史
        if (BroadcastManager.Instance != null)
        {
            history = BroadcastManager.Instance.GetBroadcastHistoryCopy();
        }
        
        return history;
    }
    
    /// <summary>
    /// 获取可用字符串
    /// </summary>
    private List<string> GetAvailableStrings()
    {
        List<string> availableStrings = new List<string>();
        
        // 从StringSelector获取可用字符串
        StringSelector stringSelector = FindObjectOfType<StringSelector>();
        if (stringSelector != null)
        {
            availableStrings = stringSelector.GetAvailableStrings();
        }
        
        return availableStrings;
    }
    
    /// <summary>
    /// 获取当前季节
    /// </summary>
    private string GetCurrentSeason()
    {
        string season = "";
        
        // 从Level3Manager获取当前季节
        Level3Manager level3Manager = FindObjectOfType<Level3Manager>();
        if (level3Manager != null)
        {
            season = level3Manager.GetCurrentSeason().ToString();
        }
        
        return season;
    }

    /// <summary>
    /// 读取Level3引导完成标志（仅当当前关卡为level3时有效）
    /// </summary>
    private bool GetLevel3GuideCompletedIfAny(string levelName)
    {
        if (string.IsNullOrEmpty(levelName)) return false;
        string lower = levelName.Trim().ToLowerInvariant();
        if (!lower.Contains("level3")) return false;

        Level3Manager l3 = FindObjectOfType<Level3Manager>();
        if (l3 != null)
        {
            return l3.IsGuideCompleted();
        }
        return false;
    }
    
    /// <summary>
    /// 读取Level2引导完成标志（仅当当前关卡为level2时有效）
    /// </summary>
    private bool GetLevel2GuideCompletedIfAny(string levelName)
    {
        if (string.IsNullOrEmpty(levelName)) return false;
        string lower = levelName.Trim().ToLowerInvariant();
        if (!lower.Contains("level2")) return false;

        Level2Manager l2 = FindObjectOfType<Level2Manager>();
        if (l2 != null)
        {
            return l2.IsGuideCompleted();
        }
        return false;
    }
    
    /// <summary>
    /// 获取收集的字符串
    /// </summary>
    private List<string> GetCollectedStrings()
    {
        List<string> collectedStrings = new List<string>();
        
        // 从Level3Manager获取收集的字符串
        Level3Manager level3Manager = FindObjectOfType<Level3Manager>();
        if (level3Manager != null)
        {
            var collected = level3Manager.GetCollectedStrings();
            collectedStrings.AddRange(collected);
        }
        
        return collectedStrings;
    }
    
    /// <summary>
    /// 收集当前场景中的飞字物体信息
    /// </summary>
    private List<GameProgressData.FlyingCharacterData> CollectFlyingCharacters()
    {
        var flyingCharacters = new List<GameProgressData.FlyingCharacterData>();
        
        // 查找所有以"Flying_"开头的GameObject
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        LogDebug($"开始收集飞字动画 - 场景总物体数: {allObjects.Length}");
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("Flying_"))
            {
                // 提取字符名称
                string character = obj.name.Substring(7); // 移除"Flying_"前缀
                
                // 获取目标位置信息
                Vector3 targetPosition = Vector3.zero;
                string targetObjectName = "";
                
                // 尝试通过PublicData获取目标位置
                if (PublicData.IsCharacterInTargetList(character))
                {
                    Transform targetTransform = PublicData.GetTargetPositionForCharacter(character);
                    if (targetTransform != null)
                    {
                        targetObjectName = targetTransform.name;
                        targetPosition = targetTransform.position;
                    }
                }
                
                // 如果无法获取目标信息，使用当前位置
                if (string.IsNullOrEmpty(targetObjectName))
                {
                    targetPosition = obj.transform.position;
                    targetObjectName = $"UnknownTarget_{character}";
                }
                
                var flyingData = new GameProgressData.FlyingCharacterData
                {
                    character = character,
                    targetObjectName = targetObjectName,
                    targetPosition = targetPosition,
                    delay = 0f // 默认无延迟
                };
                
                flyingCharacters.Add(flyingData);
                LogDebug($"收集到飞字物体: {character}, 目标: {targetObjectName}");
            }
        }
        
        LogDebug($"飞字动画收集完成 - 总计: {flyingCharacters.Count} 个");
        return flyingCharacters;
    }
    
    /// <summary>
    /// 获取已完成的目标列表
    /// </summary>
    private List<string> GetCompletedTargets()
    {
        return PublicData.GetCompletedTargets();
    }
    
    /// <summary>
    /// 获取当前目标列表（未完成的）
    /// </summary>
    private List<string> GetCurrentTargetList()
    {
        return PublicData.GetCurrentTargetList();
    }
    
    /// <summary>
    /// 获取BeachObject状态
    /// </summary>
    private GameProgressData.BeachObjectState GetBeachObjectState()
    {
        BeachObject beachObject = FindObjectOfType<BeachObject>();
        if (beachObject != null)
        {
            return new GameProgressData.BeachObjectState
            {
                hasYaBeenPlanted = beachObject.GetHasYaBeenPlanted()
            };
        }
        return new GameProgressData.BeachObjectState { hasYaBeenPlanted = false };
    }
    
    /// <summary>
    /// 收集倒计时状态
    /// </summary>
    private List<GameProgressData.CountdownTimerState> CollectCountdownTimerStates()
    {
        var countdownTimerStates = new List<GameProgressData.CountdownTimerState>();
        
        // 查找场景中所有的CountdownTimer组件
        CountdownTimer[] countdownTimers = FindObjectsOfType<CountdownTimer>();
        LogDebug($"开始收集倒计时状态 - 找到 {countdownTimers.Length} 个CountdownTimer组件");
        
        foreach (CountdownTimer countdownTimer in countdownTimers)
        {
            if (countdownTimer != null)
            {
                var stateData = countdownTimer.GetStateData();
                countdownTimerStates.Add(stateData);
                LogDebug($"收集倒计时状态: {stateData.objectName}, 剩余时间: {stateData.remainingTime}秒, 激活: {stateData.isActive}");
            }
        }
        
        LogDebug($"倒计时状态收集完成 - 总计: {countdownTimerStates.Count} 个");
        return countdownTimerStates;
    }
    
    /// <summary>
    /// 恢复广播历史
    /// </summary>
    private void RestoreBroadcastHistory(List<string> history) { }
    
    /// <summary>
    /// 恢复可用字符串
    /// </summary>
    private void RestoreAvailableStrings(List<string> availableStrings)
    {
        if (availableStrings == null || availableStrings.Count == 0) return;
        
        // 恢复StringSelector的可用字符串
        StringSelector stringSelector = FindObjectOfType<StringSelector>();
        if (stringSelector != null)
        {
            // 直接设置可用字符串列表，避免逐个添加导致重复重建按钮
            stringSelector.SetAvailableStrings(availableStrings);
        }
        
        LogDebug($"已恢复 {availableStrings.Count} 个可用字符串");
    }
    
    /// <summary>
    /// 恢复当前季节
    /// </summary>
    private void RestoreCurrentSeason(string season)
    {
        if (string.IsNullOrEmpty(season)) return;
        
        Level3Manager level3Manager = FindObjectOfType<Level3Manager>();
        if (level3Manager != null)
        {
            if (System.Enum.TryParse<SeasonType>(season, out SeasonType seasonType))
            {
                level3Manager.SetCurrentSeason(seasonType);
                LogDebug($"已恢复季节为: {season}");
            }
        }
    }
    
    /// <summary>
    /// 恢复收集的字符串
    /// </summary>
    private void RestoreCollectedStrings(List<string> collectedStrings)
    {
        if (collectedStrings == null || collectedStrings.Count == 0) return;
        
        Level3Manager level3Manager = FindObjectOfType<Level3Manager>();
        if (level3Manager != null)
        {
            foreach (string str in collectedStrings)
            {
                level3Manager.AddCollectedString(str);
            }
        }
        
        LogDebug($"已恢复 {collectedStrings.Count} 个收集的字符串");
    }
    
    /// <summary>
    /// 恢复飞字物体并依次播放动画
    /// </summary>
    private void RestoreFlyingCharacters(List<GameProgressData.FlyingCharacterData> flyingCharacters)
    {
        if (flyingCharacters == null || flyingCharacters.Count == 0) return;
        
        LogDebug($"开始恢复 {flyingCharacters.Count} 个飞字物体");
        
        // 启动协程依次播放飞字动画
        StartCoroutine(RestoreFlyingCharactersCoroutine(flyingCharacters));
    }
    
    /// <summary>
    /// 恢复飞字物体的协程
    /// </summary>
    private System.Collections.IEnumerator RestoreFlyingCharactersCoroutine(List<GameProgressData.FlyingCharacterData> flyingCharacters)
    {
        // 等待一帧确保所有系统初始化完成
        yield return null;
        
        ButtonController buttonController = FindObjectOfType<ButtonController>();
        if (buttonController == null)
        {
            LogDebug("未找到ButtonController，无法恢复飞字动画");
            yield break;
        }
        
        // 依次播放每个飞字动画
        for (int i = 0; i < flyingCharacters.Count; i++)
        {
            var flyingData = flyingCharacters[i];
            
            // 查找目标位置
            Transform targetTransform = null;
            if (!string.IsNullOrEmpty(flyingData.targetObjectName))
            {
                GameObject targetObj = GameObject.Find(flyingData.targetObjectName);
                if (targetObj != null)
                {
                    targetTransform = targetObj.transform;
                }
            }
            
            // 如果找不到目标对象，尝试使用保存的位置创建临时目标
            if (targetTransform == null)
            {
                // 创建一个临时的目标Transform
                GameObject tempTarget = new GameObject($"TempTarget_{flyingData.character}");
                tempTarget.transform.position = flyingData.targetPosition;
                targetTransform = tempTarget.transform;
            }
            
            // 播放飞字动画
            LogDebug($"播放飞字动画: {flyingData.character}");
            buttonController.Fly(flyingData.character, targetTransform);
            
            // 等待动画完成（大约1.5秒）再加上延迟时间
            yield return new WaitForSeconds(1.5f + flyingData.delay);
        }
        
        LogDebug("所有飞字动画恢复完成");
    }
    
    /// <summary>
    /// 恢复目标完成情况
    /// </summary>
    private void RestoreTargetCompletion(List<string> completedTargets, List<string> currentTargetList)
    {
        if (completedTargets != null)
        {
            PublicData.SetCompletedTargets(completedTargets);
            LogDebug($"已恢复 {completedTargets.Count} 个已完成的目标");
        }
        
        if (currentTargetList != null)
        {
            PublicData.SetCurrentTargetList(currentTargetList);
            LogDebug($"已恢复 {currentTargetList.Count} 个未完成的目标");
        }
        
        // 输出恢复后的状态信息
        int totalTargets = (completedTargets?.Count ?? 0) + (currentTargetList?.Count ?? 0);
        if (totalTargets > 0)
        {
            float progress = (float)(completedTargets?.Count ?? 0) / totalTargets * 100f;
            LogDebug($"目标完成进度: {completedTargets?.Count ?? 0}/{totalTargets} ({progress:F1}%)");
        }
    }
    
    /// <summary>
    /// 恢复BeachObject状态
    /// </summary>
    private void RestoreBeachObjectState(GameProgressData.BeachObjectState beachState)
    {
        if (beachState == null) return;
        
        BeachObject beachObject = FindObjectOfType<BeachObject>();
        if (beachObject != null)
        {
            beachObject.SetHasYaBeenPlanted(beachState.hasYaBeenPlanted);
            LogDebug($"已恢复BeachObject状态: hasYaBeenPlanted = {beachState.hasYaBeenPlanted}");
        }
        else
        {
            LogDebug("未找到BeachObject，跳过状态恢复");
        }
    }
    
    /// <summary>
    /// 恢复倒计时状态
    /// </summary>
    private void RestoreCountdownTimerStates(List<GameProgressData.CountdownTimerState> countdownTimerStates)
    {
        if (countdownTimerStates == null || countdownTimerStates.Count == 0) 
        {
            LogDebug("没有倒计时状态需要恢复");
            return;
        }
        
        LogDebug($"开始恢复 {countdownTimerStates.Count} 个倒计时状态");
        
        // 查找场景中所有的CountdownTimer组件
        CountdownTimer[] countdownTimers = FindObjectsOfType<CountdownTimer>();
        
        foreach (var stateData in countdownTimerStates)
        {
            // 通过UniqueID或对象名称查找对应的CountdownTimer
            CountdownTimer targetTimer = null;
            
            // 优先使用UniqueID查找
            if (!string.IsNullOrEmpty(stateData.uniqueId))
            {
                foreach (var timer in countdownTimers)
                {
                    if (timer != null && timer.GetComponent<UniqueID>()?.ID == stateData.uniqueId)
                    {
                        targetTimer = timer;
                        break;
                    }
                }
            }
            
            // 如果UniqueID查找失败，使用对象名称查找
            if (targetTimer == null)
            {
                foreach (var timer in countdownTimers)
                {
                    if (timer != null && timer.gameObject.name == stateData.objectName)
                    {
                        targetTimer = timer;
                        break;
                    }
                }
            }
            
            if (targetTimer != null)
            {
                // 恢复倒计时状态
                targetTimer.RestoreFromStateData(stateData);
                LogDebug($"已恢复倒计时状态: {stateData.objectName}, 剩余时间: {stateData.remainingTime}秒, 激活: {stateData.isActive}");
            }
            else
            {
                LogDebug($"未找到对应的CountdownTimer: {stateData.objectName} (UniqueID: {stateData.uniqueId})");
            }
        }
        
        LogDebug("倒计时状态恢复完成");
    }
    
    /// <summary>
    /// 退出游戏前保存状态
    /// </summary>
    public void ExitGameWithSave()
    {
        LogDebug("退出游戏，正在保存状态...");
        SaveGameState();
        
        // 延迟退出，确保保存完成
        StartCoroutine(ExitGameDelayed());
    }
    
    /// <summary>
    /// 延迟退出游戏
    /// </summary>
    private System.Collections.IEnumerator ExitGameDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    /// <summary>
    /// 可序列化的字符串列表
    /// </summary>
    [System.Serializable]
    private class SerializableList<T>
    {
        public List<T> items;
        
        public SerializableList(List<T> items)
        {
            this.items = items;
        }
    }
    
    /// <summary>
    /// 调试日志输出
    /// </summary>
    private void LogDebug(string message)
    {
        if (enableDebugLog)
        {
            GameLogger.LogDev($"[GameStateManager] {message}");
        }
    }
    
    /// <summary>
    /// 错误日志输出
    /// </summary>
    private void LogError(string message)
    {
        GameLogger.LogError($"[{GetType().Name}] {message}");
    }
    
    /// <summary>
    /// 在Inspector中显示当前状态信息
    /// </summary>
    [ContextMenu("显示当前状态")]
    public void ShowCurrentState()
    {
        LogDebug("=== 当前游戏状态信息 ===");
        LogDebug($"当前关卡: {currentLevelName}");
        LogDebug($"场景名称: {SceneManager.GetActiveScene().name}");
        LogDebug($"物体总数: {FindObjectsOfType<GameObject>().Length}");
        LogDebug($"Highlight组件数: {FindObjectsOfType<Highlight>().Length}");
        LogDebug("========================");
    }
    
    /// <summary>
    /// 手动保存状态
    /// </summary>
    [ContextMenu("手动保存状态")]
    public void ManualSaveState()
    {
        SaveGameState();
    }
    
    /// <summary>
    /// 手动恢复状态
    /// </summary>
    [ContextMenu("手动恢复状态")]
    public void ManualRestoreState()
    {
        RestoreGameState();
    }
    
    /// <summary>
    /// 清除保存状态
    /// </summary>
    [ContextMenu("清除保存状态")]
    public void ManualClearState()
    {
        ClearGameState();
    }

    /// <summary>
    /// 检查当前激活场景是否存在保存数据
    /// </summary>
    public bool HasSavedStateForActiveScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (IsStartupScene(sceneName) || IsLevel1Scene(sceneName) || IsEndLevelScene(sceneName)) return false;
        
        // **重构核心**: 从 LevelProgressManager 检查
        if (LevelProgressManager.Instance != null)
        {
            return LevelProgressManager.Instance.LoadLevelState(sceneName) != null;
        }
        
        return false;
    }

    /// <summary>
    /// 清空所有关卡的保存状态（遍历关卡序列）
    /// </summary>
    public static void ClearAllSavedStatesForAllLevels()
    {
        // **重构核心**: 调用 LevelProgressManager 来完成
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.ClearAllLevelStates();
        }
    }

    /// <summary>
    /// 是否存在任何可继续的关卡状态（遍历关卡序列，检测GameState_*是否存在）
    /// </summary>
    public static bool HasAnySavedLevelState()
    {
        // **重构核心**: 调用 LevelProgressManager 来完成
        if (LevelProgressManager.Instance != null)
        {
            return LevelProgressManager.Instance.HasAnyLevelStates();
        }
        return false;
    }

    /// <summary>
    /// 清除所有关卡的存档（基于 PublicData.LevelSequence）
    /// </summary>
    public static void ClearAllGameStates()
    {
        // **重构核心**: 这个逻辑现在应该由 LevelProgressManager.ClearAllProgress() 完全处理
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.ClearAllProgress();
        }
    }

    

    /// <summary>
    /// 是否为启动场景（不参与存档）。大小写不敏感，匹配"startup"。
    /// </summary>
    private static bool IsStartupScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;
        return sceneName.Trim().ToLowerInvariant() == "startup";
    }

    /// <summary>
    /// 是否为level1（不参与关卡内状态存档）
    /// </summary>
    private static bool IsLevel1Scene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;
        string lower = sceneName.Trim().ToLowerInvariant();
        if (lower == "level1") return true;
        // 同时兼容把首关名配置在 LevelSequence[0] 的情况
        if (PublicData.LevelSequence != null && PublicData.LevelSequence.Length > 0)
        {
            string first = (PublicData.LevelSequence[0] ?? "").Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(first) && first == lower) return true;
        }
        return false;
    }

    /// <summary>
    /// 是否为结算/谢幕场景 EndLevel（不参与存档）
    /// </summary>
    private static bool IsEndLevelScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;
        return sceneName.Trim().ToLowerInvariant() == "endlevel";
    }

    /// <summary>
    /// 是否存在任意关卡的关卡内存档（用于决定是否可“继续游戏”）
    /// </summary>
    public static bool HasAnySavedStatesForLevels()
    {
        // **重构核心**: 调用 LevelProgressManager
        if (LevelProgressManager.Instance != null)
        {
            return LevelProgressManager.Instance.HasAnyLevelStates();
        }
        return false;
    }
}
