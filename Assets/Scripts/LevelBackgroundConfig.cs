using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 负责在游戏启动时，将Unity Inspector中配置的关卡背景图数据，
/// 加载到 PublicData.LevelEndBackgrounds 静态字典中。
/// 应将此脚本挂载于StartMenu场景的某个启动对象上（如[Managers]）。
/// </summary>
public class LevelBackgroundConfig : MonoBehaviour
{
    /// <summary>
    /// 用于在Inspector中方便地配置关卡名与背景图的对应关系。
    /// </summary>
    [System.Serializable]
    public struct LevelBackgroundMapping
    {
        [Tooltip("关卡的场景名，必须与Build Settings中的场景名完全一致")]
        public string levelSceneName;
        [Tooltip("该关卡通关后，在EndLevel场景中显示的背景图")]
        public Sprite backgroundSprite;
    }

    [Header("关卡背景图配置列表")]
    [Tooltip("在此处配置每个关卡通关后需要显示的背景图")]
    public List<LevelBackgroundMapping> backgroundMappings;

    void Awake()
    {
        // 确保字典是干净的，以防万一
        if (PublicData.LevelEndBackgrounds == null)
        {
            PublicData.LevelEndBackgrounds = new Dictionary<string, Sprite>();
        }
        PublicData.LevelEndBackgrounds.Clear();

        // 遍历在Inspector中配置好的所有映射关系
        foreach (var mapping in backgroundMappings)
        {
            // 进行有效性检查，防止配置错误
            if (!string.IsNullOrEmpty(mapping.levelSceneName) && mapping.backgroundSprite != null)
            {
                // 将配置添加到PublicData的静态字典中
                if (!PublicData.LevelEndBackgrounds.ContainsKey(mapping.levelSceneName))
                {
                    PublicData.LevelEndBackgrounds.Add(mapping.levelSceneName, mapping.backgroundSprite);
                }
                else
                {
                    // 如果存在重复配置，覆盖并打印警告
                    PublicData.LevelEndBackgrounds[mapping.levelSceneName] = mapping.backgroundSprite;
                    GameLogger.LogWarning($"LevelBackgroundConfig: 关卡 '{mapping.levelSceneName}' 的背景图配置重复，已使用新值覆盖。");
                }
            }
            else
            {
                GameLogger.LogWarning("LevelBackgroundConfig: 发现一条无效的背景图配置（场景名或图片为空），已跳过。");
            }
        }
        
        GameLogger.LogSystem($"关卡背景图配置加载完成，共加载了 {PublicData.LevelEndBackgrounds.Count} 条数据。");
    }
}
