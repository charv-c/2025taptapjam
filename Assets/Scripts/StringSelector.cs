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
    [SerializeField] private int maxSelectionCount = 2; // 最大选择数量，始终保持为2
    
    [Header("字符串列表")]
    [SerializeField] private List<string> allStrings = new List<string>()
    {
        // 基础字符
        "人", "王", "两点水", "木", "火", "土", "金", "水", "日", "月", "山", 
        "川", "口", "心", "手", "足", "目", "耳", "鼻", "舌",
        // 拼字符串可能用到的额外字符
        "门", "女", "子", "言", "尔", "也",
        // 拼字符串后产生的新字符
        "闪", "明", "林", "众", "好", "休", "信", "你", "他", "们", "侠"
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
        // 确保最大选择数始终为2
        maxSelectionCount = 2;
        
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
                Debug.Log($"StringSelector: 正在创建按钮，字符串: '{str}'，字符代码: {(int)str[0]}，索引: {index}");
                
                // 添加按钮标识，用于调试
                buttonObj.name = $"Button_{index}_{str}";

                // 设置字体
                if (chineseFont != null)
                {
                    buttonText.font = chineseFont;

                    // 检查字体是否支持当前字符
                    if (!chineseFont.HasCharacter(str[0]))
                    {
                        Debug.LogWarning($"StringSelector: 中文字体不支持字符 '{str}'，字符代码: {(int)str[0]}");
                        if (fallbackFont != null && fallbackFont.HasCharacter(str[0]))
                        {
                            buttonText.font = fallbackFont;
                            Debug.Log($"StringSelector: 使用回退字体显示字符 '{str}'");
                        }
                        else
                        {
                            Debug.LogWarning($"StringSelector: 回退字体也不支持字符 '{str}'，将使用默认字体");
                        }
                    }
                    else
                    {
                        Debug.Log($"StringSelector: 中文字体支持字符 '{str}'");
                    }

                    // 确保文本居中显示
                    buttonText.alignment = TextAlignmentOptions.Center;
                    buttonText.fontSize = 72; // 设置字体大小为72

                    // 强制更新文本显示
                    buttonText.ForceMeshUpdate();

                }
                else
                {
                    Debug.LogWarning($"StringSelector: 未设置中文字体，使用默认字体创建按钮 '{str}'");
                    // 继续创建按钮，使用默认字体
                }

                // 设置按钮点击事件，传递索引而不是字符串
                int buttonIndex = index; // 捕获当前索引
                Debug.Log($"StringSelector: 为按钮设置点击事件，字符串: '{str}'，索引: {buttonIndex}");
                button.onClick.RemoveAllListeners(); // 先清除所有监听器
                button.onClick.AddListener(() => OnStringButtonClicked(buttonIndex, button));

                // 确保按钮可以交互
                button.interactable = true;

                stringButtons.Add(button);
                Debug.Log($"StringSelector: 按钮已添加到stringButtons列表，当前按钮数量: {stringButtons.Count}");

                // 延迟更新按钮显示，确保文字正确显示
                StartCoroutine(DelayedButtonUpdate(buttonText, str));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"StringSelector: 创建按钮时发生错误，字符串: {str}, 索引: {index}, 错误: {e.Message}");
        }
    }
    
    // 延迟更新按钮显示的协程
    private IEnumerator DelayedButtonUpdate(TextMeshProUGUI buttonText, string str)
    {
        // 等待一帧
        yield return null;
        
        // 再次强制更新文本显示
        if (buttonText != null)
        {
            buttonText.ForceMeshUpdate();
            Debug.Log($"StringSelector: 延迟更新完成，字符串 '{str}' 的文本网格已重新更新");
        }
    }
    
    // 字符串按钮点击事件
    private void OnStringButtonClicked(int index, Button button)
    {
        // 飞行动画期间禁止选择
        if (ButtonController.Instance != null && ButtonController.Instance.IsFlyingAnimationActive())
        {
            return;
        }
        
        string str = index < availableStrings.Count ? availableStrings[index] : "未知";
        
        // Debug输出被点击的字符串
        Debug.Log($"StringSelector: 字符串按钮被点击，字符串: '{str}'，索引: {index}");
        Debug.Log($"StringSelector: 按钮名称: '{button.gameObject.name}'");
        Debug.Log($"StringSelector: 当前availableStrings列表: [{string.Join(", ", availableStrings)}]");
        Debug.Log($"StringSelector: 当前stringButtons数量: {stringButtons.Count}");
        
        // 添加更详细的调试信息
        Debug.Log($"StringSelector: 按钮文本显示: '{button.GetComponentInChildren<TextMeshProUGUI>()?.text ?? "无文本"}'");
        Debug.Log($"StringSelector: 按钮是否可交互: {button.interactable}");
        Debug.Log($"StringSelector: 按钮是否激活: {button.gameObject.activeInHierarchy}");
        
        // 验证索引是否正确
        if (index >= 0 && index < availableStrings.Count)
        {
            Debug.Log($"StringSelector: 索引验证 - 索引{index}对应字符串: '{availableStrings[index]}'");
        }
        else
        {
            Debug.LogError($"StringSelector: 索引错误 - 索引{index}超出范围[0, {availableStrings.Count})");
        }
        
        if (selectedIndices.Contains(index))
        {
            // 取消选择
            selectedIndices.Remove(index);
            UpdateButtonVisual(button, false);
            Debug.Log($"StringSelector: 取消选择字符串 '{str}'");
        }
        else if (selectedIndices.Count < maxSelectionCount)
        {
            // 添加选择
            selectedIndices.Add(index);
            Debug.Log($"StringSelector: 添加选择，索引: {index}，字符串: '{str}'，当前选择数量: {selectedIndices.Count}");
            UpdateButtonVisual(button, true);
            Debug.Log($"StringSelector: 选择字符串 '{str}'，当前选择数量: {selectedIndices.Count}");
        }
        
        // 通知ButtonController更新确认按钮状态
        NotifySelectionChanged();
    }
    
    // 通知选择变化
    private void NotifySelectionChanged()
    {
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.UpdateButtonStates(selectedIndices.Count);
        }
    }
    
    // 更新按钮视觉效果
    private void UpdateButtonVisual(Button button, bool isSelected)
    {
        Debug.Log($"StringSelector: 更新按钮视觉效果，isSelected: {isSelected}");
        
        ColorBlock colors = button.colors;
        if (isSelected)
        {
            colors.normalColor = Color.green;
            colors.selectedColor = Color.green;
            Debug.Log($"StringSelector: 设置按钮为绿色（选中状态）");
        }
        else
        {
            colors.normalColor = Color.white;
            colors.selectedColor = Color.white;
            Debug.Log($"StringSelector: 设置按钮为白色（未选中状态）");
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
            
            // 直接设置Image的颜色
            if (isSelected)
            {
                buttonImage.color = Color.green;
                Debug.Log($"StringSelector: 直接设置Image颜色为绿色");
            }
            else
            {
                buttonImage.color = Color.white;
                Debug.Log($"StringSelector: 直接设置Image颜色为白色");
            }
        }
        else
        {
            Debug.LogWarning($"StringSelector: 按钮没有Image组件");
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
        
        // 强制更新按钮状态
        button.enabled = false;
        button.enabled = true;
        
        Debug.Log($"StringSelector: 按钮视觉效果更新完成");
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
    
    // 公共方法：重新创建所有按钮
    public void RecreateAllButtonsPublic()
    {
        Debug.Log($"StringSelector: 重新创建所有按钮，当前可用字符串: [{string.Join(", ", availableStrings)}]");
        RecreateAllButtons();
        Debug.Log($"StringSelector: 按钮重新创建完成，按钮数量: {stringButtons.Count}");
    }
    

    
    // 公共方法：添加可用字符串
    public void AddAvailableString(string str)
    {
        Debug.Log($"StringSelector: 开始添加字符串 '{str}'");
        
        // 如果字符串不在allStrings中，先添加到allStrings
        if (!allStrings.Contains(str))
        {
            Debug.Log($"StringSelector: 字符 '{str}' 不在allStrings列表中，正在添加...");
            AddToAllStrings(str);
        }
        
        Debug.Log($"StringSelector: 正在添加字符串 '{str}' 到可用字符串列表");
        
        // 清空当前选择
        ClearSelection();
        
        // 检查字符串是否已经存在
        if (availableStrings.Contains(str))
        {
            Debug.Log($"StringSelector: 字符串 '{str}' 已存在于可用字符串列表中，跳过添加");
            return;
        }
        
        // 直接添加字符串
        availableStrings.Add(str);
        int index = availableStrings.Count - 1; // 获取新添加字符串的索引
        
        Debug.Log($"StringSelector: 字符串 '{str}' 已添加到索引 {index}，当前可用字符串数量: {availableStrings.Count}");
        
        CreateStringButton(str, index);
        
        Debug.Log($"StringSelector: 字符串 '{str}' 的按钮已创建完成");
        
        // 验证按钮是否真的被创建了
        if (index < stringButtons.Count && stringButtons[index] != null)
        {
            Debug.Log($"StringSelector: 验证成功 - 按钮 '{str}' 已正确创建");
        }
        else
        {
            Debug.LogError($"StringSelector: 验证失败 - 按钮 '{str}' 创建失败");
        }
        
        // 重新创建所有按钮以确保UI正确更新
        RecreateAllButtonsPublic();
    }
    
    // 公共方法：移除可用字符串
    public void RemoveAvailableString(string str)
    {
        if (availableStrings.Contains(str))
        {
            int index = availableStrings.IndexOf(str);
            availableStrings.Remove(str);
            
            // 移除对应的按钮
            if (index >= 0 && index < stringButtons.Count)
            {
                Button buttonToRemove = stringButtons[index];
                if (buttonToRemove != null)
                {
                    DestroyImmediate(buttonToRemove.gameObject);
                }
                stringButtons.RemoveAt(index);
            }
            
            // 更新后续按钮的索引
            UpdateButtonIndicesAfterRemoval(index);
        }
    }
    
    // 更新移除按钮后的索引
    private void UpdateButtonIndicesAfterRemoval(int removedIndex)
    {
        // 更新selectedIndices中大于removedIndex的索引
        for (int i = 0; i < selectedIndices.Count; i++)
        {
            if (selectedIndices[i] > removedIndex)
            {
                selectedIndices[i]--;
            }
            else if (selectedIndices[i] == removedIndex)
            {
                // 移除已删除的索引
                selectedIndices.RemoveAt(i);
                i--; // 调整循环索引
            }
        }
        
        // 更新按钮的点击事件，重新绑定正确的索引
        for (int i = 0; i < stringButtons.Count; i++)
        {
            if (stringButtons[i] != null)
            {
                // 移除旧的点击事件
                stringButtons[i].onClick.RemoveAllListeners();
                
                // 添加新的点击事件，使用正确的索引
                int currentIndex = i;
                stringButtons[i].onClick.AddListener(() => OnStringButtonClicked(currentIndex, stringButtons[i]));
            }
        }
    }
    
    // 公共属性：获取最大选择数量（始终为2）
    public int MaxSelectionCount => 2;
    
    // 公共方法：设置最大选择数量（此方法被禁用，始终返回2）
    public void SetMaxSelectionCount(int count)
    {
        // 此方法被禁用，最大选择数始终为2
        Debug.LogWarning("SetMaxSelectionCount方法已被禁用，最大选择数始终为2");
        
        // 确保maxSelectionCount始终为2
        maxSelectionCount = 2;
        
        // 如果选择数量超过2，移除多余的选择
        if (selectedIndices.Count > 2)
        {
            selectedIndices.RemoveRange(2, selectedIndices.Count - 2);
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
    
    // 公共方法：获取最大选择数量（始终为2）
    public int GetMaxSelectionCount()
    {
        return 2;
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
    
    // 公共方法：获取按钮容器
    public Transform GetButtonContainer()
    {
        return buttonContainer;
    }
    
    // 公共方法：获取中文字体
    public TMP_FontAsset GetChineseFont()
    {
        return chineseFont;
    }
    
    // 公共方法：获取当前字体名称
    public string GetCurrentFontName()
    {
        return chineseFont != null ? chineseFont.name : "未设置";
    }
    
    // 公共方法：动态添加字符到allStrings列表
    public void AddToAllStrings(string str)
    {
        if (!allStrings.Contains(str))
        {
            allStrings.Add(str);
            Debug.Log($"StringSelector: 已添加字符 '{str}' 到allStrings列表");
        }
    }
    
    // 公共方法：检查字符是否在allStrings列表中
    public bool IsInAllStrings(string str)
    {
        return allStrings.Contains(str);
    }
}
