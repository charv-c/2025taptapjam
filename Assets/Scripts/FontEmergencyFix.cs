using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class FontEmergencyFix : MonoBehaviour
{
    [Header("紧急字体修复")]
    [SerializeField] private bool autoFixOnStart = true;
    [SerializeField] private bool useSystemFont = true;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            EmergencyFix();
        }
    }
    
    [ContextMenu("紧急修复字体")]
    public void EmergencyFix()
    {
        // 尝试使用系统默认字体
        if (useSystemFont)
        {
            UseSystemDefaultFont();
        }
        
        // 强制重新创建所有按钮
        ForceRecreateAllButtons();
    }
    
    void UseSystemDefaultFont()
    {
        // 尝试加载TMP默认字体
        TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
        if (defaultFont != null)
        {
            ApplyFontToAll(defaultFont);
            return;
        }
        
        // 尝试加载LiberationSans
        TMP_FontAsset liberationFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (liberationFont != null)
        {
            ApplyFontToAll(liberationFont);
            return;
        }
        
        // 尝试从所有可用字体中选择
        TMP_FontAsset[] allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        foreach (TMP_FontAsset font in allFonts)
        {
            if (font != null && font.name.Contains("Liberation"))
            {
                ApplyFontToAll(font);
                return;
            }
        }
    }
    
    void ApplyFontToAll(TMP_FontAsset font)
    {
        // 应用到StringSelector
        StringSelector stringSelector = FindObjectOfType<StringSelector>();
        if (stringSelector != null)
        {
            stringSelector.SetChineseFont(font);
        }
        
        // 应用到所有TextMeshProUGUI组件
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();
        
        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text != null)
            {
                text.font = font;
                text.ForceMeshUpdate();
            }
        }
    }
    
    void ForceRecreateAllButtons()
    {
        StringSelector stringSelector = FindObjectOfType<StringSelector>();
        if (stringSelector != null)
        {
            // 清除所有现有按钮
            stringSelector.ClearSelection();
            
            // 重新创建所有按钮
            stringSelector.RecreateAllButtonsPublic();
        }
    }
    
    [ContextMenu("测试字符显示")]
    public void TestCharacterDisplay()
    {
        // 创建测试文本
        GameObject testObj = new GameObject("CharacterTest");
        testObj.transform.SetParent(transform);
        
        TextMeshProUGUI testText = testObj.AddComponent<TextMeshProUGUI>();
        testText.text = "门女子言尔也闪明林众好休信你他们侠粳米更丷亭丛从仙伙停全目大昌伏牒蝶片枼虫孩猎夹";
        testText.fontSize = 24;
        testText.color = Color.black;
        
        // 设置字体
        TMP_FontAsset currentFont = GetCurrentFont();
        if (currentFont != null)
        {
            testText.font = currentFont;
        }
        
        // 强制更新
        testText.ForceMeshUpdate();
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
        
        // 尝试加载TMP默认字体
        TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
        if (defaultFont != null)
        {
            return defaultFont;
        }
        
        return null;
    }
    
    [ContextMenu("创建测试按钮网格")]
    public void CreateTestButtonGrid()
    {
        Debug.Log("=== 创建测试按钮网格 ===");
        
        // 创建Canvas（如果不存在）
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("TestCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // 创建容器
        GameObject containerObj = new GameObject("TestButtonContainer");
        containerObj.transform.SetParent(canvas.transform, false);
        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(800, 600);
        
        // 测试字符列表
        string[] testChars = {
            "人", "王", "木", "火", "土", "金", "水", "日", "月", "山",
            "门", "女", "子", "言", "尔", "也", "闪", "明", "林", "众",
            "好", "休", "信", "你", "他", "们", "侠", "粳", "米", "更",
            "丷", "亭", "丛", "从", "仙", "伙", "停", "全", "目", "大",
            "昌", "伏", "牒", "蝶", "片", "枼", "虫", "孩", "猎", "夹"
        };
        
        // 创建按钮网格
        int columns = 10;
        float buttonWidth = 70f;
        float buttonHeight = 70f;
        float spacing = 5f;
        
        for (int i = 0; i < testChars.Length; i++)
        {
            int row = i / columns;
            int col = i % columns;
            
            Vector2 position = new Vector2(
                col * (buttonWidth + spacing) - 350f,
                -row * (buttonHeight + spacing) + 250f
            );
            
            CreateTestButton(testChars[i], position, containerRect, i);
        }
        
        Debug.Log($"创建了 {testChars.Length} 个测试按钮");
    }
    
    void CreateTestButton(string character, Vector2 position, Transform parent, int index)
    {
        // 创建按钮对象
        GameObject buttonObj = new GameObject($"TestButton_{index}_{character}");
        buttonObj.transform.SetParent(parent, false);
        
        // 添加RectTransform
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(70, 70);
        
        // 添加Image
        UnityEngine.UI.Image image = buttonObj.AddComponent<UnityEngine.UI.Image>();
        image.color = Color.white;
        
        // 添加Button
        UnityEngine.UI.Button button = buttonObj.AddComponent<UnityEngine.UI.Button>();
        
        // 创建文本
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI textMesh = textObj.AddComponent<TextMeshProUGUI>();
        textMesh.text = character;
        textMesh.fontSize = 32;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = Color.black;
        
        // 设置字体
        TMP_FontAsset font = GetCurrentFont();
        if (font != null)
        {
            textMesh.font = font;
        }
        
        // 设置文本RectTransform
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // 强制更新
        textMesh.ForceMeshUpdate();
        
        // 检查显示效果
        if (string.IsNullOrEmpty(textMesh.text))
        {
            image.color = Color.red;
            Debug.LogWarning($"按钮 {index} 显示为空: '{character}'");
        }
        else if (textMesh.text != character)
        {
            image.color = Color.yellow;
            Debug.LogWarning($"按钮 {index} 显示错误: 期望'{character}'，实际'{textMesh.text}'");
        }
        else
        {
            image.color = Color.green;
        }
    }
}
