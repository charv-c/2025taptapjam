using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Added for List

public class ButtonDoorTest : MonoBehaviour
{
    [Header("按钮门字测试")]
    [SerializeField] private Button testButton;
    [SerializeField] private TextMeshProUGUI testButtonText;
    [SerializeField] private TMP_FontAsset testFont;
    
    void Start()
    {
        TestButtonDoorCharacter();
    }
    
    void TestButtonDoorCharacter()
    {
        Debug.Log("=== 按钮门字显示测试 ===");
        
        // 测试字体支持
        if (testFont != null)
        {
            Debug.Log($"测试字体: {testFont.name}");
            bool hasDoor = testFont.HasCharacter('门');
            Debug.Log($"字体是否支持'门'字符: {hasDoor}");
            
            // 测试字符的详细信息
            if (testFont.characterLookupTable != null)
            {
                uint unicode = (uint)'门';
                if (testFont.characterLookupTable.ContainsKey(unicode))
                {
                    Debug.Log($"字体包含'门'字符的查找表项");
                }
                else
                {
                    Debug.LogWarning($"字体不包含'门'字符的查找表项");
                }
            }
        }
        
        // 测试按钮文本
        if (testButtonText != null)
        {
            Debug.Log($"按钮文本组件: {testButtonText.name}");
            Debug.Log($"当前文本: '{testButtonText.text}'");
            Debug.Log($"当前字体: {testButtonText.font?.name ?? "无"}");
            
            // 设置字体
            if (testFont != null)
            {
                testButtonText.font = testFont;
                Debug.Log($"已设置字体为: {testFont.name}");
            }
            
            // 测试设置"门"字
            testButtonText.text = "门";
            Debug.Log($"设置文本为: '{testButtonText.text}'");
            
            // 强制更新
            testButtonText.ForceMeshUpdate();
            
            // 检查文本是否为空或显示异常
            if (string.IsNullOrEmpty(testButtonText.text))
            {
                Debug.LogWarning("按钮文本为空");
            }
            else
            {
                Debug.Log($"按钮文本内容: '{testButtonText.text}'");
                Debug.Log($"按钮文本长度: {testButtonText.text.Length}");
                Debug.Log($"第一个字符: '{testButtonText.text[0]}' (Unicode: U+{(int)testButtonText.text[0]:X4})");
            }
        }
        
        // 测试StringSelector中的按钮创建
        TestStringSelectorButtons();
        
        Debug.Log("=== 按钮门字显示测试完成 ===");
    }
    
    void TestStringSelectorButtons()
    {
        Debug.Log("=== StringSelector按钮测试 ===");
        
        StringSelector stringSelector = FindObjectOfType<StringSelector>();
        if (stringSelector != null)
        {
            Debug.Log($"找到StringSelector: {stringSelector.name}");
            
            // 获取字体信息
            TMP_FontAsset chineseFont = stringSelector.GetChineseFont();
            if (chineseFont != null)
            {
                Debug.Log($"StringSelector字体: {chineseFont.name}");
                bool hasDoor = chineseFont.HasCharacter('门');
                Debug.Log($"StringSelector字体是否支持'门': {hasDoor}");
            }
            else
            {
                Debug.LogWarning("StringSelector字体为空");
            }
            
            // 检查"门"字是否在可用字符串列表中
            List<string> availableStrings = stringSelector.GetAvailableStrings();
            bool hasDoorInList = availableStrings.Contains("门");
            Debug.Log($"可用字符串列表是否包含'门': {hasDoorInList}");
            
            if (hasDoorInList)
            {
                Debug.Log("'门'字在可用字符串列表中");
            }
            else
            {
                Debug.LogWarning("'门'字不在可用字符串列表中");
                
                // 尝试添加"门"字
                Debug.Log("尝试添加'门'字到可用字符串列表");
                stringSelector.AddAvailableString("门");
                
                // 重新检查
                availableStrings = stringSelector.GetAvailableStrings();
                hasDoorInList = availableStrings.Contains("门");
                Debug.Log($"添加后，可用字符串列表是否包含'门': {hasDoorInList}");
            }
        }
        else
        {
            Debug.LogWarning("未找到StringSelector");
        }
    }
    
    // 手动创建测试按钮
    [ContextMenu("创建测试按钮")]
    public void CreateTestButton()
    {
        Debug.Log("=== 手动创建测试按钮 ===");
        
        // 创建按钮对象
        GameObject buttonObj = new GameObject("TestDoorButton");
        buttonObj.transform.SetParent(transform);
        
        // 添加RectTransform
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100, 50);
        rectTransform.anchoredPosition = Vector2.zero;
        
        // 添加Image组件
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = Color.white;
        
        // 添加Button组件
        Button button = buttonObj.AddComponent<Button>();
        
        // 创建文本对象
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        
        // 添加TextMeshProUGUI组件
        TextMeshProUGUI textMesh = textObj.AddComponent<TextMeshProUGUI>();
        textMesh.text = "门";
        textMesh.fontSize = 36;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = Color.black;
        
        // 设置字体
        if (testFont != null)
        {
            textMesh.font = testFont;
            Debug.Log($"测试按钮使用字体: {testFont.name}");
        }
        
        // 设置RectTransform
        RectTransform textRectTransform = textObj.GetComponent<RectTransform>();
        textRectTransform.anchorMin = Vector2.zero;
        textRectTransform.anchorMax = Vector2.one;
        textRectTransform.offsetMin = Vector2.zero;
        textRectTransform.offsetMax = Vector2.zero;
        
        // 强制更新
        textMesh.ForceMeshUpdate();
        
        Debug.Log($"测试按钮创建完成，文本: '{textMesh.text}'");
    }
}
