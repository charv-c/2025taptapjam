using UnityEngine;
using TMPro;

public class DoorCharacterTest : MonoBehaviour
{
    [Header("门字测试")]
    [SerializeField] private TextMeshProUGUI testText;
    [SerializeField] private TMP_FontAsset testFont;
    
    void Start()
    {
        TestDoorCharacter();
    }
    
    void TestDoorCharacter()
    {
        Debug.Log("=== 门字显示测试 ===");
        
        // 测试不同的"门"字变体
        string[] doorVariants = {
            "门",  // 标准简体中文 (U+95E8)
            "門",  // 繁体中文 (U+9580)
            "⻔",  // 简化变体 (U+2ED4)
        };
        
        Debug.Log("门字变体测试:");
        foreach (string variant in doorVariants)
        {
            int unicode = (int)variant[0];
            Debug.Log($"  '{variant}' - Unicode: U+{unicode:X4}");
        }
        
        // 测试字体支持
        if (testFont != null)
        {
            Debug.Log($"测试字体: {testFont.name}");
            
            foreach (string variant in doorVariants)
            {
                bool hasCharacter = testFont.HasCharacter(variant[0]);
                Debug.Log($"  字体是否支持 '{variant}': {hasCharacter}");
            }
        }
        else
        {
            Debug.LogWarning("未设置测试字体");
        }
        
        // 测试文本显示
        if (testText != null)
        {
            Debug.Log("=== 文本显示测试 ===");
            
            // 设置字体
            if (testFont != null)
            {
                testText.font = testFont;
            }
            
            // 测试标准简体中文"门"字
            testText.text = "门";
            Debug.Log($"设置文本: '{testText.text}'");
            
            // 强制更新文本
            testText.ForceMeshUpdate();
            
            // 检查文本是否为空
            if (string.IsNullOrEmpty(testText.text))
            {
                Debug.LogWarning("文本为空");
            }
            else
            {
                Debug.Log($"文本内容: '{testText.text}'");
                Debug.Log($"文本长度: {testText.text.Length}");
                Debug.Log($"第一个字符: '{testText.text[0]}' (Unicode: U+{(int)testText.text[0]:X4})");
            }
        }
        else
        {
            Debug.LogWarning("未设置测试文本组件");
        }
        
        // 测试从StringSelector获取字体
        StringSelector stringSelector = FindObjectOfType<StringSelector>();
        if (stringSelector != null)
        {
            TMP_FontAsset chineseFont = stringSelector.GetChineseFont();
            if (chineseFont != null)
            {
                Debug.Log($"StringSelector字体: {chineseFont.name}");
                bool hasDoor = chineseFont.HasCharacter('门');
                Debug.Log($"StringSelector字体是否支持'门': {hasDoor}");
            }
        }
        
        Debug.Log("=== 门字显示测试完成 ===");
    }
}
