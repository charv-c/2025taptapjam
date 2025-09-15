using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏流程管理器 V1.0
/// 负责管理关卡序列、场景切换等宏观游戏流程。
/// 这是一个跨场景持久化的单例。
/// </summary>
public class GameFlowManager : MonoBehaviour
{
    // 单例实例
    public static GameFlowManager Instance { get; private set; }

    /// <summary>
    /// 记录最后一个完成的关卡的场景名。
    /// </summary>
    public static string LastCompletedLevelName { get; private set; }

    private void Awake()
    {
        // 实现单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            GameLogger.LogSystem("GameFlowManager 初始化并设置为跨场景持久化。");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 完成一个关卡。
    /// 这个方法应该由各个关卡的管理器在关卡完成时调用。
    /// </summary>
    /// <param name="levelName">完成的关卡的场景名</param>
    public void CompleteLevel(string levelName)
    {
        LastCompletedLevelName = levelName;
        GameLogger.LogSystem($"GameFlowManager: 关卡 '{levelName}' 已完成。LastCompletedLevelName已设置为: '{LastCompletedLevelName}'");
        GameLogger.LogSystem($"GameFlowManager: 关卡序列: [{string.Join(", ", PublicData.LevelSequence)}]");
        
        // 更新关卡进度管理器
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.CompleteLevel(levelName);
            GameLogger.LogSystem($"GameFlowManager: 已更新关卡进度管理器，关卡 '{levelName}' 标记为完成");
        }
        
        GameLogger.LogSystem("GameFlowManager: 即将进入结算页面 EndLevel");
        SceneManager.LoadScene("EndLevel");
    }

    /// <summary>
    /// 进入下一个关卡。
    /// 这个方法由EndScreenManager调用。
    /// </summary>
    public void GoToNextLevel()
    {
        GameLogger.LogSystem($"GameFlowManager: GoToNextLevel() 被调用，LastCompletedLevelName: '{LastCompletedLevelName}'");
        
        bool hasNext = HasNextLevel();
        GameLogger.LogSystem($"GameFlowManager: HasNextLevel() 返回: {hasNext}");
        
        if (hasNext)
        {
            string nextScene = GetNextLevelSceneName();
            GameLogger.LogSystem($"GameFlowManager: GetNextLevelSceneName() 返回: '{nextScene}'");
            
            // 更新关卡进度管理器中的当前关卡
            if (LevelProgressManager.Instance != null)
            {
                LevelProgressManager.Instance.SetCurrentLevel(nextScene);
                GameLogger.LogSystem($"GameFlowManager: 已更新关卡进度管理器，当前关卡设置为: '{nextScene}'");
            }
            
            GameLogger.LogSystem($"GameFlowManager: 正在加载下一个关卡: {nextScene}");
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            GameLogger.LogSystem("GameFlowManager: 所有关卡已完成，没有下一个关卡。");
            // 此时EndScreenManager会处理UI显示，这里只记录日志
        }
    }

    /// <summary>
    /// 检查是否存在下一个关卡。
    /// </summary>
    /// <returns>如果有关卡序列中有下一关，则返回true。</returns>
    public bool HasNextLevel()
    {
        if (string.IsNullOrEmpty(LastCompletedLevelName))
        {
            // 如果还没有任何关卡被完成（例如，直接从菜单进入EndLevel），则认为有下一关（第一个）。
            GameLogger.LogSystem("GameFlowManager: HasNextLevel() - LastCompletedLevelName为空，返回true（第一个关卡）");
            return true;
        }

        int currentIndex = System.Array.IndexOf(PublicData.LevelSequence, LastCompletedLevelName);
        bool hasNext = currentIndex >= 0 && currentIndex < PublicData.LevelSequence.Length - 1;
        
        GameLogger.LogSystem($"GameFlowManager: HasNextLevel() - LastCompletedLevelName: '{LastCompletedLevelName}', currentIndex: {currentIndex}, 序列长度: {PublicData.LevelSequence.Length}, hasNext: {hasNext}");
        
        return hasNext;
    }

    /// <summary>
    /// 获取下一个关卡的场景名。
    /// </summary>
    /// <returns>下一个关卡的场景名。如果没有，则返回空字符串。</returns>
    public string GetNextLevelSceneName()
    {
        if (string.IsNullOrEmpty(LastCompletedLevelName))
        {
            // 如果还没有任何关卡被完成，则返回序列中的第一个关卡。
            if (PublicData.LevelSequence.Length > 0)
            {
                string firstLevel = PublicData.LevelSequence[0];
                GameLogger.LogSystem($"GameFlowManager: GetNextLevelSceneName() - LastCompletedLevelName为空，返回第一个关卡: '{firstLevel}'");
                return firstLevel;
            }
            GameLogger.LogSystem("GameFlowManager: GetNextLevelSceneName() - LastCompletedLevelName为空且序列为空，返回空字符串");
            return string.Empty;
        }

        int currentIndex = System.Array.IndexOf(PublicData.LevelSequence, LastCompletedLevelName);
        GameLogger.LogSystem($"GameFlowManager: GetNextLevelSceneName() - LastCompletedLevelName: '{LastCompletedLevelName}', currentIndex: {currentIndex}");
        
        if (currentIndex >= 0 && currentIndex < PublicData.LevelSequence.Length - 1)
        {
            string nextLevel = PublicData.LevelSequence[currentIndex + 1];
            GameLogger.LogSystem($"GameFlowManager: GetNextLevelSceneName() - 返回下一个关卡: '{nextLevel}' (索引: {currentIndex + 1})");
            return nextLevel;
        }

        GameLogger.LogSystem($"GameFlowManager: GetNextLevelSceneName() - 没有下一个关卡，返回空字符串 (currentIndex: {currentIndex}, 序列长度: {PublicData.LevelSequence.Length})");
        return string.Empty;
    }
}
