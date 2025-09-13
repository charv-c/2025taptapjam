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
    /// 用于在Inspector中方便地配置关卡名与UI资源的对应关系。
    /// </summary>
    [System.Serializable]
    public struct LevelBackgroundMapping
    {
        [Tooltip("关卡的场景名，必须与Build Settings中的场景名完全一致")]
        public string levelSceneName;
        [Tooltip("该关卡通关后，在EndLevel场景中显示的背景图")]
        public Sprite backgroundSprite;
        [Tooltip("该关卡通关后，扇子按钮的素材（可选，留空使用默认）")]
        public Sprite buttonSprite;
        [Tooltip("该关卡通关后，扇子按钮的文案（可选，留空使用默认）")]
        public string buttonText;
        [Tooltip("最终关卡的谢幕背景图（仅最后关卡需要设置）")]
        public Sprite creditsBackground;
    }

    [Header("关卡背景图配置列表")]
    [Tooltip("在此处配置每个关卡通关后需要显示的背景图")]
    public List<LevelBackgroundMapping> backgroundMappings;

    void Awake()
    {
        // 初始化所有字典
        InitializeDictionaries();

        // 遍历在Inspector中配置好的所有映射关系
        foreach (var mapping in backgroundMappings)
        {
            LoadMappingData(mapping);
        }
        
        GameLogger.LogSystem($"关卡UI配置加载完成，共加载了 {PublicData.LevelEndBackgrounds.Count} 条背景图数据。");
    }

    /// <summary>
    /// 初始化所有字典，确保它们是干净的状态
    /// </summary>
    private void InitializeDictionaries()
    {
        // 初始化背景图字典
        if (PublicData.LevelEndBackgrounds == null)
        {
            PublicData.LevelEndBackgrounds = new Dictionary<string, Sprite>();
        }
        PublicData.LevelEndBackgrounds.Clear();

        // 初始化按钮素材字典
        if (PublicData.LevelEndButtonSprites == null)
        {
            PublicData.LevelEndButtonSprites = new Dictionary<string, Sprite>();
        }
        PublicData.LevelEndButtonSprites.Clear();

        // 初始化按钮文案字典
        if (PublicData.LevelEndButtonTexts == null)
        {
            PublicData.LevelEndButtonTexts = new Dictionary<string, string>();
        }
        PublicData.LevelEndButtonTexts.Clear();

        // 初始化谢幕背景字典
        if (PublicData.CreditsBackgrounds == null)
        {
            PublicData.CreditsBackgrounds = new Dictionary<string, Sprite>();
        }
        PublicData.CreditsBackgrounds.Clear();
    }

    /// <summary>
    /// 加载单个映射配置到相应的字典中
    /// </summary>
    /// <param name="mapping">要加载的配置映射</param>
    private void LoadMappingData(LevelBackgroundMapping mapping)
    {
        // 进行有效性检查，防止配置错误
        if (string.IsNullOrEmpty(mapping.levelSceneName))
        {
            GameLogger.LogWarning("LevelBackgroundConfig: 发现一条无效的配置（场景名为空），已跳过。");
            return;
        }

        // 加载背景图配置
        if (mapping.backgroundSprite != null)
        {
            LoadSpriteToDict(PublicData.LevelEndBackgrounds, mapping.levelSceneName, mapping.backgroundSprite, "背景图");
        }

        // 加载按钮素材配置
        if (mapping.buttonSprite != null)
        {
            LoadSpriteToDict(PublicData.LevelEndButtonSprites, mapping.levelSceneName, mapping.buttonSprite, "按钮素材");
        }

        // 加载按钮文案配置
        if (!string.IsNullOrEmpty(mapping.buttonText))
        {
            LoadStringToDict(PublicData.LevelEndButtonTexts, mapping.levelSceneName, mapping.buttonText, "按钮文案");
        }

        // 加载谢幕背景配置
        if (mapping.creditsBackground != null)
        {
            LoadSpriteToDict(PublicData.CreditsBackgrounds, mapping.levelSceneName, mapping.creditsBackground, "谢幕背景");
        }
    }

    /// <summary>
    /// 将Sprite配置加载到指定字典
    /// </summary>
    private void LoadSpriteToDict(Dictionary<string, Sprite> dict, string key, Sprite value, string configType)
    {
        if (dict.ContainsKey(key))
        {
            dict[key] = value;
            GameLogger.LogWarning($"LevelBackgroundConfig: 关卡 '{key}' 的{configType}配置重复，已使用新值覆盖。");
        }
        else
        {
            dict.Add(key, value);
        }
    }

    /// <summary>
    /// 将String配置加载到指定字典
    /// </summary>
    private void LoadStringToDict(Dictionary<string, string> dict, string key, string value, string configType)
    {
        if (dict.ContainsKey(key))
        {
            dict[key] = value;
            GameLogger.LogWarning($"LevelBackgroundConfig: 关卡 '{key}' 的{configType}配置重复，已使用新值覆盖。");
        }
        else
        {
            dict.Add(key, value);
        }
    }
}
