using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CharacterDisplayTest : MonoBehaviour
{
    [Header("字符显示测试")]
    [SerializeField] private TMP_FontAsset testFont;
    [SerializeField] private Transform testContainer;
    
    void Start()
    {
        TestAllCharacters();
    }
    
    void TestAllCharacters()
    {
        Debug.Log("=== 全面字符显示测试 ===");
        
        // 获取所有需要测试的字符
        List<string> allCharacters = GetAllCharacters();
        
        // 测试字体支持
        TestFontSupport(allCharacters);
        
        // 创建测试按钮
        CreateTestButtons(allCharacters);
        
        Debug.Log("=== 字符显示测试完成 ===");
    }
    
    List<string> GetAllCharacters()
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
    
    void TestFontSupport(List<string> characters)
    {
        Debug.Log("=== 字体支持测试 ===");
        
        TMP_FontAsset font = GetBestFont();
        if (font == null)
        {
            Debug.LogError("未找到可用的字体");
            return;
        }
        
        Debug.Log($"使用字体: {font.name}");
        
        // 测试每个字符
        for (int i = 0; i < characters.Count; i++)
        {
            string character = characters[i];
            bool isSupported = font.HasCharacter(character[0]);
            
            string status = isSupported ? "✓" : "✗";
            Debug.Log($"{status} [{i:D2}] '{character}' (Unicode: U+{(int)character[0]:X4}): {(isSupported ? "支持" : "不支持")}");
            
            // 特别标记从"门"开始的字符
            if (character == "门")
            {
                Debug.LogWarning("=== 从'门'字开始的问题区域 ===");
            }
        }
        
        // 统计支持情况
        int supportedCount = 0;
        int totalCount = characters.Count;
        
        foreach (string character in characters)
        {
            if (font.HasCharacter(character[0]))
            {
                supportedCount++;
            }
        }
        
        Debug.Log($"字体支持统计: {supportedCount}/{totalCount} ({supportedCount * 100f / totalCount:F1}%)");
    }
    
    void CreateTestButtons(List<string> characters)
    {
        Debug.Log("=== 创建测试按钮 ===");
        
        // 创建容器
        if (testContainer == null)
        {
            GameObject containerObj = new GameObject("TestContainer");
            testContainer = containerObj.transform;
        }
        
        // 清除现有按钮
        foreach (Transform child in testContainer)
        {
            DestroyImmediate(child.gameObject);
        }
        
        TMP_FontAsset font = GetBestFont();
        if (font == null)
        {
            Debug.LogError("未找到可用的字体，无法创建测试按钮");
            return;
        }
        
        // 创建按钮网格
        int columns = 8;
        float buttonWidth = 80f;
        float buttonHeight = 80f;
        float spacing = 10f;
        
        for (int i = 0; i < characters.Count; i++)
        {
            int row = i / columns;
            int col = i % columns;
            
            Vector2 position = new Vector2(
                col * (buttonWidth + spacing),
                -row * (buttonHeight + spacing)
            );
            
            CreateTestButton(characters[i], position, font, i);
        }
        
        Debug.Log($"创建了 {characters.Count} 个测试按钮");
    }
    
    void CreateTestButton(string character, Vector2 position, TMP_FontAsset font, int index)
    {
        // 创建按钮对象
        GameObject buttonObj = new GameObject($"TestButton_{index}_{character}");
        buttonObj.transform.SetParent(testContainer, false);
        
        // 添加RectTransform
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(80, 80);
        
        // 添加Image
        Image image = buttonObj.AddComponent<Image>();
        image.color = Color.white;
        
        // 添加Button
        Button button = buttonObj.AddComponent<Button>();
        
        // 创建文本
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI textMesh = textObj.AddComponent<TextMeshProUGUI>();
        textMesh.text = character;
        textMesh.font = font;
        textMesh.fontSize = 36;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = Color.black;
        
        // 设置文本RectTransform
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // 强制更新
        textMesh.ForceMeshUpdate();
        
        // 检查文本是否为空
        if (string.IsNullOrEmpty(textMesh.text))
        {
            Debug.LogWarning($"按钮 {index} 的文本为空: '{character}'");
            image.color = Color.red; // 标记有问题的按钮
        }
        else if (textMesh.text != character)
        {
            Debug.LogWarning($"按钮 {index} 的文本不匹配: 期望'{character}'，实际'{textMesh.text}'");
            image.color = Color.yellow; // 标记有问题的按钮
        }
        else
        {
            image.color = Color.green; // 标记正常的按钮
        }
    }
    
    TMP_FontAsset GetBestFont()
    {
        // 优先使用测试字体
        if (testFont != null)
        {
            return testFont;
        }
        
        // 尝试加载SourceHanSerifCN-Heavy SDF 1 (动态字体)
        TMP_FontAsset sourceHanFont = Resources.Load<TMP_FontAsset>("Fonts/SourceHanSerifCN-Heavy SDF 1");
        if (sourceHanFont != null)
        {
            return sourceHanFont;
        }
        
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
        
        return null;
    }
    
    [ContextMenu("重新测试所有字符")]
    public void RetestAllCharacters()
    {
        TestAllCharacters();
    }
    
    [ContextMenu("测试StringSelector按钮")]
    public void TestStringSelectorButtons()
    {
        Debug.Log("=== 测试StringSelector按钮 ===");
        
        StringSelector stringSelector = FindObjectOfType<StringSelector>();
        if (stringSelector != null)
        {
            // 获取所有可用字符串
            List<string> availableStrings = stringSelector.GetAvailableStrings();
            Debug.Log($"StringSelector可用字符串数量: {availableStrings.Count}");
            
            // 检查每个字符串
            for (int i = 0; i < availableStrings.Count; i++)
            {
                string character = availableStrings[i];
                Debug.Log($"[{i:D2}] '{character}' (Unicode: U+{(int)character[0]:X4})");
            }
            
            // 重新创建按钮
            stringSelector.RecreateAllButtonsPublic();
            Debug.Log("已重新创建StringSelector按钮");
        }
        else
        {
            Debug.LogWarning("未找到StringSelector");
        }
    }
    
    [ContextMenu("修复字体问题")]
    public void FixFontIssues()
    {
        Debug.Log("=== 修复字体问题 ===");
        
        // 查找并修复所有TextMeshProUGUI组件
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();
        TMP_FontAsset bestFont = GetBestFont();
        
        if (bestFont == null)
        {
            Debug.LogError("未找到可用的字体");
            return;
        }
        
        int fixedCount = 0;
        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text != null && text.font != bestFont)
            {
                text.font = bestFont;
                text.ForceMeshUpdate();
                fixedCount++;
            }
        }
        
        Debug.Log($"修复了 {fixedCount} 个文本组件的字体");
        
        // 修复StringSelector
        StringSelector stringSelector = FindObjectOfType<StringSelector>();
        if (stringSelector != null)
        {
            stringSelector.SetChineseFont(bestFont);
            stringSelector.RecreateAllButtonsPublic();
            Debug.Log("已修复StringSelector字体");
        }
    }
}
