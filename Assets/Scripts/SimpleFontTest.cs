using UnityEngine;
using TMPro;

public class SimpleFontTest : MonoBehaviour
{
    [Header("字体测试")]
    [SerializeField] private TMP_FontAsset testFont;
    [SerializeField] private TextMeshProUGUI testText;
    
    void Start()
    {
        TestFont();
    }
    
    void TestFont()
    {
        if (testFont == null)
        {
            Debug.LogError("SimpleFontTest: 请设置测试字体");
            return;
        }
        
        Debug.Log($"=== 字体测试: {testFont.name} ===");
        
        // 测试基本字体信息
        Debug.Log($"字体名称: {testFont.name}");
        Debug.Log($"材质: {testFont.material?.name ?? "无"}");
        Debug.Log($"纹理: {testFont.atlasTexture?.name ?? "无"}");
        
        // 测试字符支持
        string[] testChars = { "人", "门", "王", "木", "火", "土", "金", "水", "日", "月", "山", "川", "口", "心", "手", "足", "目", "耳", "鼻", "舌" };
        
        Debug.Log("字符支持情况:");
        foreach (string charStr in testChars)
        {
            bool hasCharacter = testFont.HasCharacter(charStr[0]);
            Debug.Log($"  '{charStr}' (Unicode: {(int)charStr[0]:X4}): {(hasCharacter ? "✓ 支持" : "✗ 不支持")}");
        }
        
        // 如果设置了测试文本，进行显示测试
        if (testText != null)
        {
            TestTextDisplay();
        }
    }
    
    void TestTextDisplay()
    {
        if (testFont == null || testText == null)
        {
            return;
        }
        
        Debug.Log("=== 文本显示测试 ===");
        
        // 设置字体
        testText.font = testFont;
        testText.fontSize = 72;
        
        // 测试单个字符
        testText.text = "门";
        Debug.Log($"设置文本: '{testText.text}'");
        
        // 等待一帧后检查
        StartCoroutine(CheckTextAfterFrame());
    }
    
    System.Collections.IEnumerator CheckTextAfterFrame()
    {
        yield return null; // 等待一帧
        
        if (testText.textInfo != null)
        {
            Debug.Log($"文本信息: 字符数={testText.textInfo.characterCount}");
            for (int i = 0; i < testText.textInfo.characterCount; i++)
            {
                var charInfo = testText.textInfo.characterInfo[i];
                Debug.Log($"  字符{i}: '{charInfo.character}' (Unicode: {(int)charInfo.character:X4}), 可见: {charInfo.isVisible}");
            }
        }
        else
        {
            Debug.LogWarning("无法获取文本信息");
        }
    }
    
    [ContextMenu("测试字体")]
    public void TestFontManually()
    {
        TestFont();
    }
}
