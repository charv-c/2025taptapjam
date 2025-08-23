using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StringSelector : MonoBehaviour
{
    [Header("UI设置")]
    [SerializeField] private Transform buttonContainer; // 按钮容器
    [SerializeField] private GameObject buttonPrefab; // 按钮预制体
    [SerializeField] private TMP_FontAsset chineseFont; // 中文字体资源
    [SerializeField] private TMP_FontAsset fallbackFont; // 回退字体资源
    
    [Header("选择设置")]
    public int maxSelectionCount = 1; // 最大选择数量
    
    [Header("字符串列表")]
    [SerializeField] private List<string> allStrings = new List<string>()
    {
        "人", "王", "两点水", "木", "火", "土", "金", "水", "日", "月", "山", 
        "川", "口", "心", "手", "足", "目", "耳", "鼻", "舌"
    };
    
    [Header("游戏开始时可用字符串")]
    [SerializeField] private List<string> availableStrings = new List<string>();
    
    // 私有变量
    private List<int> selectedIndices = new List<int>(); // 已选择的按钮索引
    private List<Button> stringButtons = new List<Button>(); // 字符串按钮列表
    
    // 公共属性
    public List<string> SelectedStrings 
    { 
        get 
        {
            List<string> result = new List<string>();
            foreach (int index in selectedIndices)
            {
                if (index >= 0 && index < availableStrings.Count)
                {
                    result.Add(availableStrings[index]);
                }
            }
            return result;
        }
    }
    public string FirstSelectedString => selectedIndices.Count > 0 && selectedIndices[0] < availableStrings.Count ? availableStrings[selectedIndices[0]] : "";
    public string SecondSelectedString => selectedIndices.Count > 1 && selectedIndices[1] < availableStrings.Count ? availableStrings[selectedIndices[1]] : "";
    
    void Start()
    {
        InitializeUI();
    }
    
    // 初始化UI
    private void InitializeUI()
    {
        // 检查必要的组件
        if (buttonContainer == null)
        {
            Debug.LogError("StringSelector: 请在Inspector中设置Button Container!");
            return;
        }
        
        if (buttonPrefab == null)
        {
            Debug.LogError("StringSelector: 请在Inspector中设置Button Prefab!");
            return;
        }
        
        // 检查按钮预制体是否包含必要的组件
        Button prefabButton = buttonPrefab.GetComponent<Button>();
        if (prefabButton == null)
        {
            Debug.LogError("StringSelector: Button Prefab必须包含Button组件!");
            return;
        }
        
        TextMeshProUGUI prefabText = buttonPrefab.GetComponentInChildren<TextMeshProUGUI>();
        if (prefabText == null)
        {
            Debug.LogError("StringSelector: Button Prefab必须包含TextMeshProUGUI组件!");
            return;
        }
        
        // 清除现有按钮
        ClearButtons();
        
        // 为每个字符串创建按钮
        for (int i = 0; i < availableStrings.Count; i++)
        {
            CreateStringButton(availableStrings[i], i);
        }
    }
    
    // 创建字符串按钮
    private void CreateStringButton(string str, int index)
    {
        try
        {
            GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            
            // 确保按钮有正确的RectTransform设置
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // 设置合适的尺寸，避免按钮重叠
                rectTransform.sizeDelta = new Vector2(100, 50);
                
                // 确保按钮有正确的锚点设置，避免占据整个容器
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);
                rectTransform.anchoredPosition = Vector2.zero;
            }
            
            // 确保Image组件可以正确接收点击
            Image buttonImage = buttonObj.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.raycastTarget = true;
                // 确保Image有合适的透明度
                Color imageColor = buttonImage.color;
                imageColor.a = 1.0f;
                buttonImage.color = imageColor;
            }
            
            if (buttonText != null)
            {
                buttonText.text = str;
                
                // 设置字体
                if (chineseFont != null)
                {
                    buttonText.font = chineseFont;
                    
                    // 检查字体是否支持当前字符
                    if (!chineseFont.HasCharacter(str[0]))
                    {
                        Debug.LogWarning($"StringSelector: 字体 '{chineseFont.name}' 不支持字符 '{str}' (Unicode: {(int)str[0]:X4})");
                        
                        // 尝试使用回退字体
                        if (fallbackFont != null && fallbackFont.HasCharacter(str[0]))
                        {
                            buttonText.font = fallbackFont;
                            Debug.Log($"StringSelector: 使用回退字体 '{fallbackFont.name}' 显示字符 '{str}'");
                        }
                    }
                    else
                    {
                        Debug.Log($"StringSelector: 字体 '{chineseFont.name}' 支持字符 '{str}' (Unicode: {(int)str[0]:X4})");
                    }
                }
                else
                {
                    Debug.LogWarning("StringSelector: 未设置中文字体资源，可能导致中文字符显示异常");
                }
                
                // 确保文本居中显示
                buttonText.alignment = TextAlignmentOptions.Center;
                buttonText.fontSize = 72; // 设置字体大小为72
                
                // 强制更新文本显示
                buttonText.ForceMeshUpdate();
            }
            else
            {
                Debug.LogError($"StringSelector: 无法找到按钮的TextMeshProUGUI组件，字符串: {str}, 索引: {index}");
                return;
            }
            
            // 设置按钮点击事件，传递索引而不是字符串
            int buttonIndex = index; // 捕获当前索引
            button.onClick.AddListener(() => OnStringButtonClicked(buttonIndex, button));
            
            // 确保按钮可以交互
            button.interactable = true;
            
            stringButtons.Add(button);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"StringSelector: 创建按钮时发生错误，字符串: {str}, 索引: {index}, 错误: {e.Message}");
        }
    }
    
    // 字符串按钮点击事件
    private void OnStringButtonClicked(int index, Button button)
    {
        string str = index < availableStrings.Count ? availableStrings[index] : "未知";
        
        if (selectedIndices.Contains(index))
        {
            // 取消选择
            selectedIndices.Remove(index);
            UpdateButtonVisual(button, false);
        }
        else if (selectedIndices.Count < maxSelectionCount)
        {
            // 添加选择
            selectedIndices.Add(index);
            UpdateButtonVisual(button, true);
        }
        
        // 通知ButtonController更新确认按钮状态
        NotifySelectionChanged();
    }
    
    // 通知选择变化
    private void NotifySelectionChanged()
    {
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.RefreshConfirmButtonState();
        }
    }
    
    // 更新按钮视觉效果
    private void UpdateButtonVisual(Button button, bool isSelected)
    {
        ColorBlock colors = button.colors;
        if (isSelected)
        {
            colors.normalColor = Color.green;
            colors.selectedColor = Color.green;
        }
        else
        {
            colors.normalColor = Color.white;
            colors.selectedColor = Color.white;
        }
        button.colors = colors;
        
        // 更新按钮的交互性
        button.interactable = true;
        
        // 确保按钮有正确的Image组件用于点击检测
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.raycastTarget = true;
            // 确保Image有合适的透明度
            Color imageColor = buttonImage.color;
            imageColor.a = 1.0f;
            buttonImage.color = imageColor;
        }
        
        // 确保按钮的RectTransform有正确的尺寸
        RectTransform rectTransform = button.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 确保按钮有合适的尺寸
            if (rectTransform.sizeDelta.x < 50 || rectTransform.sizeDelta.y < 30)
            {
                rectTransform.sizeDelta = new Vector2(100, 50);
            }
        }
    }
    
    // 清除选择
    public void ClearSelection()
    {
        selectedIndices.Clear();
        UpdateAllButtonVisuals();
        
        // 通知ButtonController更新确认按钮状态
        NotifySelectionChanged();
    }
    
    // 更新所有按钮视觉效果
    private void UpdateAllButtonVisuals()
    {
        for (int i = 0; i < stringButtons.Count; i++)
        {
            if (i < availableStrings.Count)
            {
                UpdateButtonVisual(stringButtons[i], selectedIndices.Contains(i));
            }
        }
    }
    
    // 清除按钮
    private void ClearButtons()
    {
        foreach (Button button in stringButtons)
        {
            if (button != null)
            {
                DestroyImmediate(button.gameObject);
            }
        }
        stringButtons.Clear();
    }
    
    // 重新创建所有按钮
    private void RecreateAllButtons()
    {
        // 清除现有按钮
        ClearButtons();
        
        // 重新创建所有按钮
        for (int i = 0; i < availableStrings.Count; i++)
        {
            CreateStringButton(availableStrings[i], i);
        }
    }
    

    
    // 公共方法：添加可用字符串
    public void AddAvailableString(string str)
    {
        // 检查字符串是否在allStrings中存在
        if (!allStrings.Contains(str))
        {
            Debug.LogWarning($"字符串 '{str}' 不在所有字符串列表中，无法添加");
            return;
        }
        
        // 直接添加字符串，不检查是否已存在
        availableStrings.Add(str);
        int index = availableStrings.Count - 1; // 获取新添加字符串的索引
        CreateStringButton(str, index);
    }
    
    // 公共方法：移除可用字符串
    public void RemoveAvailableString(string str)
    {
        if (availableStrings.Contains(str))
        {
            int index = availableStrings.IndexOf(str);
            availableStrings.Remove(str);
            
            // 重新创建所有按钮以保持正确的索引
            RecreateAllButtons();
        }
    }
    
    // 公共方法：设置最大选择数量
    public void SetMaxSelectionCount(int count)
    {
        // 检查是否真的需要设置
        if (maxSelectionCount == count)
        {
            return;
        }
        
        maxSelectionCount = Mathf.Max(1, count);
        
        if (selectedIndices.Count > maxSelectionCount)
        {
            selectedIndices.RemoveRange(maxSelectionCount, selectedIndices.Count - maxSelectionCount);
            UpdateAllButtonVisuals();
        }
    }
    
    // 公共方法：检查字符串是否被选择
    public bool IsStringSelected(string str)
    {
        // 检查是否有任何选中的索引对应的字符串匹配
        foreach (int index in selectedIndices)
        {
            if (index >= 0 && index < availableStrings.Count && availableStrings[index] == str)
            {
                return true;
            }
        }
        return false;
    }
    
    // 公共方法：获取选择数量
    public int GetSelectionCount()
    {
        return selectedIndices.Count;
    }
    
    // 公共方法：修复按钮点击区域
    public void FixButtonClickAreas()
    {
        foreach (Button button in stringButtons)
        {
            if (button != null)
            {
                RectTransform rectTransform = button.GetComponent<RectTransform>();
                Image buttonImage = button.GetComponent<Image>();
                
                if (rectTransform != null)
                {
                    // 确保按钮有正确的尺寸
                    if (rectTransform.sizeDelta.x < 50 || rectTransform.sizeDelta.y < 30)
                    {
                        rectTransform.sizeDelta = new Vector2(100, 50);
                    }
                }
                
                if (buttonImage != null)
                {
                    // 确保Image组件可以接收点击
                    buttonImage.raycastTarget = true;
                    
                    // 设置合适的透明度
                    Color imageColor = buttonImage.color;
                    imageColor.a = 1.0f;
                    buttonImage.color = imageColor;
                }
                
                // 确保按钮可以交互
                button.interactable = true;
            }
        }
    }
    
    // 公共方法：设置按钮尺寸
    public void SetButtonSize(Vector2 size)
    {
        foreach (Button button in stringButtons)
        {
            if (button != null)
            {
                RectTransform rectTransform = button.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = size;
                }
            }
        }
        Debug.Log($"StringSelector: 所有按钮尺寸设置为: {size}");
    }
    

    
    // 公共方法：获取所有可用字符串
    public List<string> GetAvailableStrings()
    {
        return new List<string>(availableStrings);
    }
    

    
    // 公共方法：获取所有字符串列表
    public List<string> GetAllStrings()
    {
        return new List<string>(allStrings);
    }
    
    // 公共方法：检查字符串是否可用
    public bool IsStringAvailable(string str)
    {
        return availableStrings.Contains(str);
    }
    
    // 公共方法：获取可用字符串数量
    public int GetAvailableStringCount()
    {
        return availableStrings.Count;
    }
    
    // 公共方法：设置中文字体
    public void SetChineseFont(TMP_FontAsset font)
    {
        chineseFont = font;
        
        // 更新所有现有按钮的字体
        foreach (Button button in stringButtons)
        {
            if (button != null)
            {
                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null && chineseFont != null)
                {
                    buttonText.font = chineseFont;
                }
            }
        }
    }
    
    // 公共方法：检查字体是否支持字符
    public bool IsCharacterSupported(string character)
    {
        if (chineseFont == null)
        {
            Debug.LogWarning("StringSelector: 未设置中文字体");
            return false;
        }
        
        return chineseFont.HasCharacter(character[0]);
    }
    
    // 公共方法：获取当前字体名称
    public string GetCurrentFontName()
    {
        return chineseFont != null ? chineseFont.name : "未设置";
    }
}
