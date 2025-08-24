using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class FontFixer : MonoBehaviour
{
    [Header("字体修复设置")]
    [SerializeField] private TMP_FontAsset targetFont;
    [SerializeField] private bool autoFixOnStart = true;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            FixFontIssues();
        }
    }
    
    [ContextMenu("修复字体问题")]
    public void FixFontIssues()
    {
        Debug.Log("=== 开始修复字体问题 ===");
        
        // 检查并修复StringSelector的字体
        FixStringSelectorFont();
        
        // 检查并修复所有TextMeshProUGUI组件的字体
        FixAllTextComponents();
        
        Debug.Log("=== 字体问题修复完成 ===");
    }
    
    void FixStringSelectorFont()
    {
        StringSelector stringSelector = FindObjectOfType<StringSelector>();
        if (stringSelector != null)
        {
            Debug.Log("修复StringSelector字体...");
            
            // 获取当前字体
            TMP_FontAsset currentFont = stringSelector.GetChineseFont();
            Debug.Log($"StringSelector当前字体: {currentFont?.name ?? "无"}");
            
            // 如果字体为空或有问题，设置新的字体
            if (currentFont == null || !IsFontValid(currentFont))
            {
                TMP_FontAsset newFont = GetBestAvailableFont();
                if (newFont != null)
                {
                    stringSelector.SetChineseFont(newFont);
                    Debug.Log($"StringSelector字体已设置为: {newFont.name}");
                }
                else
                {
                    Debug.LogWarning("未找到可用的中文字体");
                }
            }
            
            // 重新创建所有按钮
            stringSelector.RecreateAllButtonsPublic();
        }
        else
        {
            Debug.LogWarning("未找到StringSelector");
        }
    }
    
    void FixAllTextComponents()
    {
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();
        Debug.Log($"找到 {allTexts.Length} 个TextMeshProUGUI组件");
        
        TMP_FontAsset bestFont = GetBestAvailableFont();
        if (bestFont == null)
        {
            Debug.LogWarning("未找到可用的中文字体，跳过文本组件修复");
            return;
        }
        
        int fixedCount = 0;
        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text != null && (text.font == null || !IsFontValid(text.font)))
            {
                text.font = bestFont;
                text.ForceMeshUpdate();
                fixedCount++;
            }
        }
        
        Debug.Log($"修复了 {fixedCount} 个文本组件的字体");
    }
    
    bool IsFontValid(TMP_FontAsset font)
    {
        if (font == null) return false;
        
        // 检查字体是否支持基本的中文字符
        char[] testChars = { '门', '人', '王', '木', '火', '土', '金', '水', '日' };
        
        foreach (char c in testChars)
        {
            if (!font.HasCharacter(c))
            {
                Debug.LogWarning($"字体 {font.name} 不支持字符 '{c}' (Unicode: {(int)c:X4})");
                return false;
            }
        }
        
        return true;
    }
    
    TMP_FontAsset GetBestAvailableFont()
    {
        // 优先使用目标字体
        if (targetFont != null && IsFontValid(targetFont))
        {
            return targetFont;
        }
        
        // 尝试加载SourceHanSerifCN-Heavy SDF 1 (动态字体)
        TMP_FontAsset sourceHanFont = Resources.Load<TMP_FontAsset>("Fonts/SourceHanSerifCN-Heavy SDF 1");
        if (sourceHanFont != null && IsFontValid(sourceHanFont))
        {
            return sourceHanFont;
        }
        
        // 尝试从所有已加载的字体中查找
        TMP_FontAsset[] allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        foreach (TMP_FontAsset font in allFonts)
        {
            if (font != null && IsFontValid(font))
            {
                Debug.Log($"找到可用字体: {font.name}");
                return font;
            }
        }
        
        return null;
    }
    
    [ContextMenu("测试门字显示")]
    public void TestDoorCharacter()
    {
        Debug.Log("=== 测试门字显示 ===");
        
        TMP_FontAsset font = GetBestAvailableFont();
        if (font != null)
        {
            Debug.Log($"使用字体: {font.name}");
            bool hasDoor = font.HasCharacter('门');
            Debug.Log($"字体是否支持'门'字: {hasDoor}");
            
            if (hasDoor)
            {
                Debug.Log("✓ 门字可以正常显示");
            }
            else
            {
                Debug.LogWarning("✗ 门字无法显示");
            }
        }
        else
        {
            Debug.LogError("未找到可用的字体");
        }
    }
    
    [ContextMenu("创建门字测试按钮")]
    public void CreateDoorTestButton()
    {
        Debug.Log("=== 创建门字测试按钮 ===");
        
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
        
        // 创建按钮
        GameObject buttonObj = new GameObject("DoorTestButton");
        buttonObj.transform.SetParent(canvas.transform, false);
        
        // 添加RectTransform
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(200, 100);
        
        // 添加Image
        UnityEngine.UI.Image image = buttonObj.AddComponent<UnityEngine.UI.Image>();
        image.color = Color.white;
        
        // 添加Button
        UnityEngine.UI.Button button = buttonObj.AddComponent<UnityEngine.UI.Button>();
        
        // 创建文本
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI textMesh = textObj.AddComponent<TextMeshProUGUI>();
        textMesh.text = "门";
        textMesh.fontSize = 48;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = Color.black;
        
        // 设置字体
        TMP_FontAsset font = GetBestAvailableFont();
        if (font != null)
        {
            textMesh.font = font;
            Debug.Log($"测试按钮使用字体: {font.name}");
        }
        
        // 设置文本RectTransform
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // 强制更新
        textMesh.ForceMeshUpdate();
        
        Debug.Log("门字测试按钮创建完成");
    }
}
