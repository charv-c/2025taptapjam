using UnityEngine;
using TMPro;
using UnityEngine.TextCore.LowLevel;
using System.Collections.Generic;

public class FontRegenerator : MonoBehaviour
{
    [Header("字体重新生成设置")]
    [SerializeField] private Font sourceFont;
    [SerializeField] private string outputFontName = "RegeneratedChineseFont";
    [SerializeField] private int samplingPointSize = 64;
    [SerializeField] private int atlasPadding = 5;
    [SerializeField] private int atlasWidth = 1024;
    [SerializeField] private int atlasHeight = 1024;
    
    void Start()
    {
        // 自动检测并修复字体问题
        AutoDetectAndFixFontIssues();
    }
    
    void AutoDetectAndFixFontIssues()
    {
        Debug.Log("=== 自动检测并修复字体问题 ===");
        
        // 检查当前字体问题
        CheckCurrentFontIssues();
        
        // 尝试修复字体问题
        TryFixFontIssues();
    }
    
    void CheckCurrentFontIssues()
    {
        Debug.Log("=== 检查当前字体问题 ===");
        
        // 获取所有需要检查的字符
        List<string> testCharacters = GetTestCharacters();
        
        // 检查当前字体
        TMP_FontAsset currentFont = GetCurrentFont();
        if (currentFont != null)
        {
            Debug.Log($"当前字体: {currentFont.name}");
            CheckFontSupport(currentFont, testCharacters);
        }
        else
        {
            Debug.LogWarning("未找到当前字体");
        }
    }
    
    void CheckFontSupport(TMP_FontAsset font, List<string> characters)
    {
        int supportedCount = 0;
        int totalCount = characters.Count;
        
        Debug.Log("字符支持检查:");
        for (int i = 0; i < characters.Count; i++)
        {
            string character = characters[i];
            bool isSupported = font.HasCharacter(character[0]);
            
            if (isSupported)
            {
                supportedCount++;
            }
            
            string status = isSupported ? "✓" : "✗";
            Debug.Log($"{status} [{i:D2}] '{character}' (Unicode: U+{(int)character[0]:X4})");
            
            // 特别标记问题区域
            if (character == "门")
            {
                Debug.LogWarning("=== 从'门'字开始的问题区域 ===");
            }
        }
        
        float supportRate = (float)supportedCount / totalCount * 100f;
        Debug.Log($"字体支持率: {supportedCount}/{totalCount} ({supportRate:F1}%)");
        
        if (supportRate < 90f)
        {
            Debug.LogWarning($"字体支持率过低 ({supportRate:F1}%)，建议重新生成字体");
        }
    }
    
    void TryFixFontIssues()
    {
        Debug.Log("=== 尝试修复字体问题 ===");
        
        // 尝试重新生成字体
        if (sourceFont != null)
        {
            Debug.Log("尝试重新生成字体...");
            RegenerateFont();
        }
        else
        {
            Debug.LogWarning("未设置源字体，无法重新生成");
            
            // 尝试查找可用的源字体
            FindAvailableSourceFonts();
        }
    }
    
    void FindAvailableSourceFonts()
    {
        Debug.Log("=== 查找可用的源字体 ===");
        
        // 查找所有字体文件
        Font[] allFonts = Resources.FindObjectsOfTypeAll<Font>();
        Debug.Log($"找到 {allFonts.Length} 个字体文件");
        
        foreach (Font font in allFonts)
        {
            if (font != null)
            {
                Debug.Log($"字体: {font.name}");
                
                // 检查是否是中文字体
                if (IsChineseFont(font))
                {
                    Debug.Log($"✓ 找到中文字体: {font.name}");
                    sourceFont = font;
                    break;
                }
            }
        }
        
        if (sourceFont != null)
        {
            Debug.Log($"已选择源字体: {sourceFont.name}");
            RegenerateFont();
        }
        else
        {
            Debug.LogError("未找到可用的中文字体");
        }
    }
    
    bool IsChineseFont(Font font)
    {
        if (font == null) return false;
        
        string fontName = font.name.ToLower();
        return fontName.Contains("chinese") || 
               fontName.Contains("han") || 
               fontName.Contains("source") ||
               fontName.Contains("simsun") ||
               fontName.Contains("simhei") ||
               fontName.Contains("microsoft yahei");
    }
    
    [ContextMenu("重新生成字体")]
    public void RegenerateFont()
    {
        if (sourceFont == null)
        {
            Debug.LogError("请先设置源字体");
            return;
        }
        
        Debug.Log($"=== 重新生成字体: {sourceFont.name} ===");
        
        try
        {
            // 创建字体资源
            TMP_FontAsset newFontAsset = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                samplingPointSize,
                atlasPadding,
                GlyphRenderMode.SDFAA,
                atlasWidth,
                atlasHeight,
                AtlasPopulationMode.Dynamic
            );
            
            if (newFontAsset != null)
            {
                Debug.Log($"字体重新生成成功: {newFontAsset.name}");
                
                // 保存字体资源
                SaveFontAsset(newFontAsset);
                
                // 应用新字体
                ApplyNewFont(newFontAsset);
            }
            else
            {
                Debug.LogError("字体重新生成失败");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"字体重新生成时发生错误: {e.Message}");
        }
    }
    
    void SaveFontAsset(TMP_FontAsset fontAsset)
    {
        // 这里应该保存字体资源到文件
        // 由于在运行时无法直接保存资源，我们只记录信息
        Debug.Log($"字体资源已创建: {fontAsset.name}");
        Debug.Log($"字体资源GUID: {fontAsset.name}");
    }
    
    void ApplyNewFont(TMP_FontAsset newFont)
    {
        Debug.Log("=== 应用新字体 ===");
        
        // 应用到StringSelector
        StringSelector stringSelector = FindObjectOfType<StringSelector>();
        if (stringSelector != null)
        {
            stringSelector.SetChineseFont(newFont);
            stringSelector.RecreateAllButtonsPublic();
            Debug.Log("已应用到StringSelector");
        }
        
        // 应用到所有TextMeshProUGUI组件
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();
        int appliedCount = 0;
        
        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text != null)
            {
                text.font = newFont;
                text.ForceMeshUpdate();
                appliedCount++;
            }
        }
        
        Debug.Log($"已应用到 {appliedCount} 个文本组件");
    }
    
    List<string> GetTestCharacters()
    {
        return new List<string>
        {
            // 基础字符
            "人", "王", "两点水", "木", "火", "土", "金", "水", "日", "月", "山", 
            "川", "口", "心", "手", "足", "目", "耳", "鼻", "舌",
            // 拼字符串可能用到的额外字符
            "门", "女", "子", "言", "尔", "也",
            // 拼字符串后产生的新字符
            "闪", "明", "林", "众", "好", "休", "信", "你", "他", "们", "侠",
            // 额外需要的字符
            "粳", "米", "更", "丷", "亭", "丛", "从", "仙", "伙", "停", "全", "目", "大", "昌", "伏", "牒", "蝶", "片", "枼", "虫", "孩", "猎", "夹"
        };
    }
    
    TMP_FontAsset GetCurrentFont()
    {
        // 尝试从StringSelector获取
        StringSelector stringSelector = FindObjectOfType<StringSelector>();
        if (stringSelector != null)
        {
            TMP_FontAsset chineseFont = stringSelector.GetChineseFont();
            if (chineseFont != null)
            {
                return chineseFont;
            }
        }
        
        // 尝试加载SourceHanSerifCN-Heavy SDF 1 (动态字体)
        TMP_FontAsset sourceHanFont = Resources.Load<TMP_FontAsset>("Fonts/SourceHanSerifCN-Heavy SDF 1");
        if (sourceHanFont != null)
        {
            return sourceHanFont;
        }
        
        return null;
    }
    
    [ContextMenu("测试当前字体")]
    public void TestCurrentFont()
    {
        CheckCurrentFontIssues();
    }
    
    [ContextMenu("查找可用字体")]
    public void FindFonts()
    {
        FindAvailableSourceFonts();
    }
}
