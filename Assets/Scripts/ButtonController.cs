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
        
        // 初始化split按钮状态
        UpdateButtonStates(0);
        
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
    }

    private void OnSplitButtonClicked()
    {
        // 直接执行分割操作
        splitletter();
    }
    
    private void OnCombineButtonClicked()
    {
        // 直接执行合并操作
        combineletter();
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
    
    // 公共方法：更新按钮状态
    public void UpdateButtonStates(int selectedCount)
    {
        if (splitButton != null)
        {
            // 当选中0个或2个字符串时，禁用split按钮
            splitButton.interactable = selectedCount == 1;
        }
        
        if (combineButton != null)
        {
            // 当选中2个字符串时，启用combine按钮（因为合并需要2个字符串）
            combineButton.interactable = selectedCount == 2;
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
        
        // 更新split按钮状态
        if (stringSelector != null)
        {
            UpdateButtonStates(stringSelector.GetSelectionCount());
        }
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
            // 检查选择数量
            int selectedCount = stringSelector.GetSelectionCount();
            if (selectedCount != 1)
            {
                Debug.LogWarning($"分割操作需要选择1个字符串，当前选择了{selectedCount}个");
                // 静默处理，清空选择
                stringSelector.ClearSelection();
                return;
            }
            
            string selectedString = stringSelector.FirstSelectedString;
            if (!string.IsNullOrEmpty(selectedString))
            {
                Debug.Log($"执行分割操作，选中的字符串: {selectedString}");
                Debug.Log($"选中的字符串Unicode: {(int)selectedString[0]:X4}");
                
                // 使用PublicData中的分割映射
                if (PublicData.CanSplitString(selectedString))
                {
                    var (part1, part2) = PublicData.GetStringSplit(selectedString);
                    Debug.Log($"分割结果: '{part1}' 和 '{part2}'");
                    Debug.Log($"分割结果Unicode: part1={(int)part1[0]:X4}, part2={(int)part2[0]:X4}");
                    
                    // 先清除当前选择状态
                    stringSelector.ClearSelection();
                    
                    // 先移除原本的字符串
                    stringSelector.RemoveAvailableString(selectedString);
                    
                    // 最后添加分割后的新字符串
                    stringSelector.AddAvailableString(part1);
                    stringSelector.AddAvailableString(part2);
                    
                    // 重新创建所有按钮以确保索引正确
                    stringSelector.RecreateAllButtonsPublic();
                    
                    // 确保最大选择数仍然是2
                    stringSelector.SetMaxSelectionCount(2);
                    
                    Debug.Log($"已移除原本的字符串: {selectedString}，添加新字符串: {part1} 和 {part2}，并重新创建按钮");
                }
                else
                {
                    Debug.LogWarning($"字符串 '{selectedString}' 没有预定义的分割映射");
                    // 静默处理，清空选择
                    stringSelector.ClearSelection();
                }
            }
            else
            {
                Debug.LogWarning("没有选中的字符串，无法执行分割操作");
                
            }
        }
        else
        {
            Debug.LogError("StringSelector引用为空，无法执行分割操作");
        }
    }
    
    // 合并字母函数
    private void combineletter()
    {
        if (stringSelector != null)
        {
            // 检查选择数量
            
            int selectedCount = stringSelector.GetSelectionCount();
            if (selectedCount != 2)
            {
                Debug.LogWarning($"合并操作需要选择2个字符串，当前选择了{selectedCount}个");
                // 静默处理，清空选择
                stringSelector.ClearSelection();
                return;
            }
            
            List<string> selectedStrings = stringSelector.SelectedStrings;
            string firstString = selectedStrings[0];
            string secondString = selectedStrings[1];
            Debug.Log($"执行合并操作，第一个字符串: {firstString}，第二个字符串: {secondString}");
            
            // 使用PublicData中的反向查找功能
            string originalString = PublicData.FindOriginalString(firstString, secondString);
            if (originalString != null)
            {
                Debug.Log($"合并结果: {originalString}");
                
                // 先清除当前选择状态
                stringSelector.ClearSelection();
                
                // 先移除原本的字符串
                stringSelector.RemoveAvailableString(firstString);
                stringSelector.RemoveAvailableString(secondString);
                
                // 最后添加合并后的新字符串
                stringSelector.AddAvailableString(originalString);
                
                // 重新创建所有按钮以确保索引正确
                stringSelector.RecreateAllButtonsPublic();
                
                // 确保最大选择数仍然是2
                stringSelector.SetMaxSelectionCount(2);
                
                // 清空选择
                stringSelector.ClearSelection();
                
                Debug.Log($"已移除原本的字符串: {firstString} 和 {secondString}，添加新字符串: {originalString}，并重新创建按钮");
            }
            else
            {
                Debug.LogWarning($"无法找到由 '{firstString}' 和 '{secondString}' 组成的有效合并结果");
                // 静默处理，清空选择
                stringSelector.ClearSelection();
            }
        }
        else
        {
            Debug.LogError("StringSelector引用为空，无法执行合并操作");
        }
    }
    
    // 公共方法：显示所有按钮
    public void ShowAllButtons()
    {
        if (splitButton != null) splitButton.gameObject.SetActive(true);
        if (combineButton != null) combineButton.gameObject.SetActive(true);
    }
    
    // 公共方法：设置所有按钮是否可交互
    public void SetAllButtonsInteractable(bool interactable)
    {
        if (splitButton != null) splitButton.interactable = interactable;
        if (combineButton != null) combineButton.interactable = interactable;
    }
    
    // 公共方法：获取所有按钮是否可交互
    public bool AreAllButtonsInteractable()
    {
        return (splitButton != null && splitButton.interactable) &&
               (combineButton != null && combineButton.interactable);
    }
    
    // 公共方法：手动触发Split按钮点击（供其他脚本调用）
    public void TriggerSplitButton()
    {
        OnSplitButtonClicked();
    }
    
    // 公共方法：设置StringSelector引用
    public void SetStringSelector(StringSelector selector)
    {
        stringSelector = selector;
    }
    
    // 公共方法：获取StringSelector引用
    public StringSelector GetStringSelector()
    {
        return stringSelector;
    }
}
