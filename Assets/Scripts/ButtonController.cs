using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class ButtonController : MonoBehaviour
{
    [Header("按钮设置")]
    [SerializeField] private Button splitButton, combineButton,confirmButton, cancelButton;
    [SerializeField] private float hideDelay = 0.1f; // 隐藏延迟时间
    
    [Header("UI提示设置")]
    [SerializeField] private TextMeshProUGUI messageText; // 消息提示文本组件
    [SerializeField] private float messageDisplayTime = 3f; // 消息显示时间（秒）
    
    [Header("选择器引用")]
    [SerializeField] private StringSelector stringSelector; // StringSelector引用
    
    // 私有变量
    private int currentMaxSelectCount = 1; // 当前最大选择数量
    
    // 单例模式，方便其他脚本访问
    public static ButtonController Instance { get; private set; }
    
    private void Awake()
    {
        // 设置单例
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // 初始化时隐藏confirm和cancel按钮
        if (confirmButton != null) confirmButton.gameObject.SetActive(false);
        if (cancelButton != null) cancelButton.gameObject.SetActive(false);
        
        // 初始化时隐藏消息文本
        if (messageText != null) messageText.gameObject.SetActive(false);
        
        // 添加按钮点击事件
        if (splitButton != null)
        {
            splitButton.onClick.AddListener(OnSplitButtonClicked);
        }
        else
        {
            Debug.LogError("未找到splitButton组件！");
        }
        
        if (combineButton != null)
        {
            combineButton.onClick.AddListener(OnCombineButtonClicked);
        }
        else
        {
            Debug.LogError("未找到combineButton组件！");
        }
        
        // 添加confirm和cancel按钮事件
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }
        
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
        }
    }

    private void OnSplitButtonClicked()
    {
        // 显示确认按钮并设置maxselectcount为1
        ShowConfirmButtonsWithMaxCount(1);
    }
    
    private void OnCombineButtonClicked()
    {
        // 显示确认按钮并设置maxselectcount为2
        ShowConfirmButtonsWithMaxCount(2);
    }
    
    // 私有方法：显示确认按钮并设置最大选择数量
    private void ShowConfirmButtonsWithMaxCount(int maxCount)
    {
        // 隐藏split和combine按钮
        if (splitButton != null) splitButton.gameObject.SetActive(false);
        if (combineButton != null) combineButton.gameObject.SetActive(false);
        
        // 显示confirm和cancel按钮，但确认按钮初始状态为禁用
        if (confirmButton != null) 
        {
            confirmButton.gameObject.SetActive(true);
            confirmButton.interactable = false; // 初始状态为禁用
        }
        if (cancelButton != null) cancelButton.gameObject.SetActive(true);
        
        // 清空当前选择
        ClearCurrentSelection();
        
        // 设置maxselectcount
        SetMaxSelectCount(maxCount);
        
        // 立即检查一次选择数量，使用传入的maxCount参数
        UpdateConfirmButtonStateWithMaxCount(maxCount);
    }
    
    // 私有方法：更新确认按钮状态
    private void UpdateConfirmButtonState()
    {
        if (confirmButton == null) return;
        
        // 使用存储的最大选择数量
        int currentMaxCount = currentMaxSelectCount;
        
        // 获取当前选择数量
        int currentSelectionCount = GetCurrentSelectionCount();
        
        // 如果选择数量达到最大值，启用确认按钮
        bool shouldEnable = currentSelectionCount >= currentMaxCount;
        confirmButton.interactable = shouldEnable;
    }
    
    // 私有方法：使用指定最大数量更新确认按钮状态
    private void UpdateConfirmButtonStateWithMaxCount(int maxCount)
    {
        if (confirmButton == null) return;
        
        // 获取当前选择数量
        int currentSelectionCount = GetCurrentSelectionCount();
        
        // 如果选择数量达到指定的最大值，启用确认按钮
        bool shouldEnable = currentSelectionCount >= maxCount;
        confirmButton.interactable = shouldEnable;
    }
    
    // 私有方法：获取当前最大选择数量
    private int GetCurrentMaxSelectCount()
    {
        // 从StringSelector获取当前最大选择数量
        if (stringSelector != null)
        {
            return stringSelector.maxSelectionCount;
        }
        
        // 如果没有找到StringSelector，返回默认值
        return 1;
    }
    
    // 私有方法：获取当前选择数量
    private int GetCurrentSelectionCount()
    {
        // 从StringSelector获取当前选择数量
        if (stringSelector != null)
        {
            return stringSelector.GetSelectionCount();
        }
        
        // 如果没有找到StringSelector，返回0
        return 0;
    }
    
    // 公共方法：手动更新确认按钮状态（供其他脚本调用）
    public void RefreshConfirmButtonState()
    {
        UpdateConfirmButtonState();
    }
    
    private void OnConfirmButtonClicked()
    {
        // 隐藏所有按钮
        HideAllButtons();
        
        // 根据最大可选数执行不同的函数
        if (currentMaxSelectCount == 1)
        {
            splitletter();
        }
        else if (currentMaxSelectCount == 2)
        {
            combineletter();
        }
        else
        {
            Debug.LogWarning($"未知的最大选择数量: {currentMaxSelectCount}");
        }
        
        // 执行完操作后，重新显示split和combine按钮
        ShowSplitAndCombineButtons();
        
        // 清空当前选择
        ClearCurrentSelection();
        
        // 恢复最大选择数量到默认值
        RestoreMaxSelectCount();
    }
    
    private void OnCancelButtonClicked()
    {
        // 隐藏confirm和cancel按钮
        if (confirmButton != null) confirmButton.gameObject.SetActive(false);
        if (cancelButton != null) cancelButton.gameObject.SetActive(false);
        
        // 重新显示split和combine按钮
        ShowSplitAndCombineButtons();
        
        // 恢复maxselectcount
        RestoreMaxSelectCount();
        
        // 清除选择
        if (stringSelector != null)
        {
            stringSelector.ClearSelection();
        }
    }
    
    private void HideAllButtons()
    {
        // 隐藏所有按钮
        if (splitButton != null) splitButton.gameObject.SetActive(false);
        if (combineButton != null) combineButton.gameObject.SetActive(false);
        if (confirmButton != null) confirmButton.gameObject.SetActive(false);
        if (cancelButton != null) cancelButton.gameObject.SetActive(false);
    }
    
    // 显示split和combine按钮
    private void ShowSplitAndCombineButtons()
    {
        if (splitButton != null) splitButton.gameObject.SetActive(true);
        if (combineButton != null) combineButton.gameObject.SetActive(true);
    }
    
    // 显示消息提示
    private void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(true);
            
            // 启动协程在指定时间后隐藏消息
            StartCoroutine(HideMessageAfterDelay());
        }
        else
        {
            Debug.LogWarning("MessageText组件未设置，无法显示消息提示");
        }
    }
    
    // 隐藏消息提示
    private void HideMessage()
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }
    
    // 延迟隐藏消息的协程
    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDisplayTime);
        HideMessage();
    }
    
    // 分割字母函数
    private void splitletter()
    {
        if (stringSelector != null)
        {
            string selectedString = stringSelector.FirstSelectedString;
            if (!string.IsNullOrEmpty(selectedString))
            {
                Debug.Log($"执行分割操作，选中的字符串: {selectedString}");
                
                // 使用PublicData中的分割映射
                if (PublicData.CanSplitString(selectedString))
                {
                    var (part1, part2) = PublicData.GetStringSplit(selectedString);
                    Debug.Log($"分割结果: '{part1}' 和 '{part2}'");
                    
                    // 这里可以添加具体的分割逻辑
                    // 例如：将分割后的字符添加到可用字符串列表中
                    if (stringSelector != null)
                    {
                        stringSelector.AddAvailableString(part1);
                        stringSelector.AddAvailableString(part2);
                    }
                }
                else
                {
                    Debug.LogWarning($"字符串 '{selectedString}' 没有预定义的分割映射");
                }
            }
            else
            {
                Debug.LogWarning("没有选中的字符串，无法执行分割操作");
                ShowMessage("请先选择一个字符串进行分割");
            }
        }
        else
        {
            Debug.LogError("StringSelector引用为空，无法执行分割操作");
            ShowMessage("系统错误：StringSelector组件未找到");
        }
    }
    
    // 合并字母函数
    private void combineletter()
    {
        if (stringSelector != null)
        {
            List<string> selectedStrings = stringSelector.SelectedStrings;
            if (selectedStrings.Count == 2)
            {
                string firstString = selectedStrings[0];
                string secondString = selectedStrings[1];
                Debug.Log($"执行合并操作，第一个字符串: {firstString}，第二个字符串: {secondString}");
                
                // 使用PublicData中的反向查找功能
                string originalString = PublicData.FindOriginalString(firstString, secondString);
                if (originalString != null)
                {
                    Debug.Log($"合并结果: {originalString}");
                    
                    // 这里可以添加具体的合并逻辑
                    // 例如：将合并后的字符串添加到可用字符串列表中
                    stringSelector.AddAvailableString(originalString);
                }
                else
                {
                    Debug.LogWarning($"无法找到由 '{firstString}' 和 '{secondString}' 组成的有效合并结果");
                }
            }
            else
            {
                Debug.LogWarning($"选中的字符串数量不正确，需要2个，实际有{selectedStrings.Count}个");
                ShowMessage($"请选择2个字符串进行合并，当前选择了{selectedStrings.Count}个");
            }
        }
        else
        {
            Debug.LogError("StringSelector引用为空，无法执行合并操作");
            ShowMessage("系统错误：StringSelector组件未找到");
        }
    }
    
    private void SetMaxSelectCount(int count)
    {
        // 检查是否真的需要设置
        if (stringSelector != null && stringSelector.maxSelectionCount == count)
        {
            currentMaxSelectCount = count;
            return;
        }
        
        // 存储当前最大选择数量
        currentMaxSelectCount = count;
        
        // 设置StringSelector的最大选择数量
        if (stringSelector != null)
        {
            stringSelector.SetMaxSelectionCount(count);
        }
        else
        {
            Debug.LogWarning("StringSelector引用为空，无法设置最大选择数量");
        }
    }
    
    // 私有方法：清空当前选择
    private void ClearCurrentSelection()
    {
        if (stringSelector != null)
        {
            stringSelector.ClearSelection();
        }
        else
        {
            Debug.LogWarning("StringSelector引用为空，无法清空选择");
        }
    }
    
    private void RestoreMaxSelectCount()
    {
        // 恢复存储的最大选择数量到默认值
        currentMaxSelectCount = 1;
        
        // 恢复StringSelector的最大选择数量到默认值
        if (stringSelector != null)
        {
            stringSelector.SetMaxSelectionCount(1); // 默认值为1
        }
        else
        {
            Debug.LogWarning("StringSelector引用为空，无法恢复最大选择数量");
        }
    }
    
    // 公共方法：显示所有按钮
    public void ShowAllButtons()
    {
        if (splitButton != null) splitButton.gameObject.SetActive(true);
        if (combineButton != null) combineButton.gameObject.SetActive(true);
    }
    
    // 公共方法：显示确认按钮
    public void ShowConfirmButtons()
    {
        if (confirmButton != null) confirmButton.gameObject.SetActive(true);
        if (cancelButton != null) cancelButton.gameObject.SetActive(true);
    }
    
    // 公共方法：切换确认按钮显示状态
    public void ToggleConfirmButtons()
    {
        bool currentState = confirmButton != null && confirmButton.gameObject.activeSelf;
        if (confirmButton != null) confirmButton.gameObject.SetActive(!currentState);
        if (cancelButton != null) cancelButton.gameObject.SetActive(!currentState);
    }
    
    // 公共方法：设置所有按钮是否可交互
    public void SetAllButtonsInteractable(bool interactable)
    {
        if (splitButton != null) splitButton.interactable = interactable;
        if (combineButton != null) combineButton.interactable = interactable;
        if (confirmButton != null) confirmButton.interactable = interactable;
        if (cancelButton != null) cancelButton.interactable = interactable;
    }
    
    // 公共方法：获取确认按钮是否可见
    public bool AreConfirmButtonsVisible()
    {
        return (confirmButton != null && confirmButton.gameObject.activeSelf) ||
               (cancelButton != null && cancelButton.gameObject.activeSelf);
    }
    
    // 公共方法：获取所有按钮是否可交互
    public bool AreAllButtonsInteractable()
    {
        return (splitButton != null && splitButton.interactable) &&
               (combineButton != null && combineButton.interactable) &&
               (confirmButton != null && confirmButton.interactable) &&
               (cancelButton != null && cancelButton.interactable);
    }
    
    // 公共方法：手动触发Split按钮点击（供其他脚本调用）
    public void TriggerSplitButton()
    {
        OnSplitButtonClicked();
    }
    
    // 公共方法：手动触发Confirm按钮点击（供其他脚本调用）
    public void TriggerConfirmButton()
    {
        OnConfirmButtonClicked();
    }
    
    // 公共方法：手动触发Cancel按钮点击（供其他脚本调用）
    public void TriggerCancelButton()
    {
        OnCancelButtonClicked();
    }
    
    // 公共方法：重置到初始状态
    public void ResetToInitialState()
    {
        // 显示split和combine按钮
        if (splitButton != null) splitButton.gameObject.SetActive(true);
        if (combineButton != null) combineButton.gameObject.SetActive(true);
        
        // 隐藏confirm和cancel按钮
        if (confirmButton != null) confirmButton.gameObject.SetActive(false);
        if (cancelButton != null) cancelButton.gameObject.SetActive(false);
        
        // 恢复maxselectcount
        RestoreMaxSelectCount();
    }
    
    // 公共方法：检查是否处于确认状态
    public bool IsInConfirmState()
    {
        return AreConfirmButtonsVisible();
    }
    
    // 公共方法：设置StringSelector引用
    public void SetStringSelector(StringSelector selector)
    {
        stringSelector = selector;
    }
}
