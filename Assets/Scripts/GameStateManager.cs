using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

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
    
    // PlayerPrefs键名常量
    private const string GAME_STATE_KEY = "GameState_";
    private const string BROADCAST_HISTORY_KEY = "BroadcastHistory_";
    private const string AVAILABLE_STRINGS_KEY = "AvailableStrings_";
    private const string CURRENT_SEASON_KEY = "CurrentSeason_";
    private const string COLLECTED_STRINGS_KEY = "CollectedStrings_";
    
    // 当前关卡名称
    private string currentLevelName;
    
    // 游戏状态数据类
    [System.Serializable]
    public class GameObjectState
    {
        public string objectName;
        public string objectPath;
        public bool isActive;
        public bool hasHighlight;
        public bool highlightEnabled;
        public string highlightLetter;
        public Vector3 position;
        public bool hasSpriteRenderer;
        public bool spriteRendererEnabled;
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
        public float saveTime;
    }
    
    private void Awake()
    {
        // 实现单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentLevelName = SceneManager.GetActiveScene().name;
            LogDebug("GameStateManager 初始化并设置为跨场景持久化");
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
        GameStateData stateData = new GameStateData
        {
            levelName = currentLevelName,
            objectStates = CollectObjectStates(),
            broadcastHistory = GetBroadcastHistory(),
            availableStrings = GetAvailableStrings(),
            currentSeason = GetCurrentSeason(),
            collectedStrings = GetCollectedStrings(),
            saveTime = Time.time
        };
        
        string jsonData = JsonUtility.ToJson(stateData, true);
        PlayerPrefs.SetString(GAME_STATE_KEY + currentLevelName, jsonData);
        PlayerPrefs.Save();
        
        LogDebug($"游戏状态已保存 - 关卡: {currentLevelName}, 物体数量: {stateData.objectStates.Count}");
    }
    
    /// <summary>
    /// 恢复游戏状态
    /// </summary>
    public void RestoreGameState()
    {
        currentLevelName = SceneManager.GetActiveScene().name;
        string jsonData = PlayerPrefs.GetString(GAME_STATE_KEY + currentLevelName, "");
        
        if (string.IsNullOrEmpty(jsonData))
        {
            LogDebug($"没有找到关卡 {currentLevelName} 的保存数据");
            return;
        }
        
        try
        {
            GameStateData stateData = JsonUtility.FromJson<GameStateData>(jsonData);
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
        PlayerPrefs.DeleteKey(GAME_STATE_KEY + currentLevelName);
        PlayerPrefs.DeleteKey(BROADCAST_HISTORY_KEY + currentLevelName);
        PlayerPrefs.DeleteKey(AVAILABLE_STRINGS_KEY + currentLevelName);
        PlayerPrefs.DeleteKey(CURRENT_SEASON_KEY + currentLevelName);
        PlayerPrefs.DeleteKey(COLLECTED_STRINGS_KEY + currentLevelName);
        PlayerPrefs.Save();
        
        LogDebug($"关卡 {currentLevelName} 的保存数据已清除");
    }
    
    /// <summary>
    /// 检查是否应该恢复游戏状态
    /// </summary>
    private bool ShouldRestoreGameState()
    {
        // 检查是否有保存数据
        currentLevelName = SceneManager.GetActiveScene().name;
        string jsonData = PlayerPrefs.GetString(GAME_STATE_KEY + currentLevelName, "");
        return !string.IsNullOrEmpty(jsonData);
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
    private List<GameObjectState> CollectObjectStates()
    {
        List<GameObjectState> objectStates = new List<GameObjectState>();
        
        // 收集所有GameObject
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj == null || obj.scene != gameObject.scene) continue;
            
            GameObjectState state = new GameObjectState
            {
                objectName = obj.name,
                objectPath = GetGameObjectPath(obj),
                isActive = obj.activeInHierarchy,
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
            
            objectStates.Add(state);
        }
        
        return objectStates;
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
        
        // 从根对象开始查找
        GameObject root = GameObject.Find(pathParts[0]);
        if (root == null) return null;
        
        // 递归查找子对象
        Transform current = root.transform;
        for (int i = 1; i < pathParts.Length; i++)
        {
            Transform child = current.Find(pathParts[i]);
            if (child == null) return null;
            current = child;
        }
        
        return current.gameObject;
    }
    
    /// <summary>
    /// 应用游戏状态
    /// </summary>
    private void ApplyGameState(GameStateData stateData)
    {
        // 恢复物体状态
        foreach (GameObjectState objState in stateData.objectStates)
        {
            GameObject obj = FindGameObjectByPath(objState.objectPath);
            if (obj == null) continue;
            
            // 恢复GameObject激活状态
            obj.SetActive(objState.isActive);
            
            // 恢复位置
            obj.transform.position = objState.position;
            
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
        }
        
        // 恢复广播历史
        RestoreBroadcastHistory(stateData.broadcastHistory);
        
        // 恢复可用字符串
        RestoreAvailableStrings(stateData.availableStrings);
        
        // 恢复当前季节
        RestoreCurrentSeason(stateData.currentSeason);
        
        // 恢复收集的字符串
        RestoreCollectedStrings(stateData.collectedStrings);
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
    /// 恢复广播历史
    /// </summary>
    private void RestoreBroadcastHistory(List<string> history)
    {
        if (history == null || history.Count == 0) return;
        
        // 重新发送所有广播
        foreach (string broadcast in history)
        {
            if (BroadcastManager.Instance != null)
            {
                BroadcastManager.Instance.BroadcastToAll(broadcast);
            }
        }
        
        LogDebug($"已恢复 {history.Count} 个广播历史");
    }
    
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
            foreach (string str in availableStrings)
            {
                stringSelector.AddAvailableString(str);
            }
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
            Debug.Log($"[GameStateManager] {message}");
        }
    }
    
    /// <summary>
    /// 错误日志输出
    /// </summary>
    private void LogError(string message)
    {
        Debug.LogError($"[GameStateManager] {message}");
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
}
