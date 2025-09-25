using UnityEngine;

/// <summary>
/// 首次运行当前构建版本时，自动重置存档为全新状态。
/// 通过比较 PlayerPrefs 中记录的版本与 Application.version 判断是否为新构建。
/// </summary>
public static class SaveResetInitializer
{
    private const string SavedBuildVersionKey = "SavedBuildVersion";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetSavesOnNewBuild()
    {
        string currentVersion = Application.version ?? string.Empty;
        string savedVersion = PlayerPrefs.GetString(SavedBuildVersionKey, string.Empty);

        // 如果检测到是新构建版本（版本号不同），则删除现有存档并记录当前版本
        if (!string.Equals(currentVersion, savedVersion, System.StringComparison.Ordinal))
        {
            Debug.Log($"[SaveResetInitializer] 首次运行当前构建版本（{currentVersion}），清理旧存档。");

            // 删除 JSON 存档（主文件与备份）
            JsonStorageManager.DeleteGameProgress();

            // 记录当前版本，避免后续重复清理
            PlayerPrefs.SetString(SavedBuildVersionKey, currentVersion);
            PlayerPrefs.Save();
        }
    }
}


