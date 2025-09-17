using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 测试工具：一键清空所有本地存储（PlayerPrefs）与项目内相关存档键
/// 用法：
/// 1) 将本脚本挂在任意场景中的一个 GameObject 上
/// 2) 在该组件的右键上下文菜单中点击“清空所有本地存储（测试）”
/// </summary>
public class ClearAllLocalDataTester : MonoBehaviour
{
    [Header("自动清理设置（默认关闭，需手动勾选）")]
    [SerializeField] private bool enableAutoClear = false;
    [SerializeField] private bool clearOnStartupScene = false;
    [SerializeField] private bool clearOnLevel1Scene = false;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!enableAutoClear) return;
        string name = (scene.name ?? "").Trim().ToLowerInvariant();
        if ((clearOnStartupScene && name == "startup") || (clearOnLevel1Scene && name == "level1"))
        {
            ClearAllLocalData();
        }
    }
    [ContextMenu("清空所有本地存储（测试）")]
    public void ClearAllLocalData()
    {
        // 先清理关卡内状态（按关卡遍历）
        GameStateManager.ClearAllGameStates();

        // 再清理关卡进度
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.ClearAllProgress();
        }

        // 最后兜底：彻底清空 PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("[ClearAllLocalDataTester] 已清空所有本地存储（含关卡状态与进度）。");
    }
}


