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
        // 播放UI点击音效
        if (AudioManager.Instance != null && AudioManager.Instance.sfxUIClick != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxUIClick);
        }
        // 直接执行分割操作
        splitletter();
    }
    
    private void OnCombineButtonClicked()
    {
        // 播放UI点击音效
        if (AudioManager.Instance != null && AudioManager.Instance.sfxUIClick != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxUIClick);
        }
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
                    
                    // 播放成功音效
                    if (AudioManager.Instance != null && AudioManager.Instance.sfxSplitSuccess != null)
                    {
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxSplitSuccess);
                    }

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
                    // 播放失败音效
                    if (AudioManager.Instance != null && AudioManager.Instance.sfxOperationFailure != null)
                    {
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxOperationFailure);
                    }
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
                
                // 播放成功音效
                if (AudioManager.Instance != null && AudioManager.Instance.sfxCombineSuccess != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxCombineSuccess);
                }

                // 先清除当前选择状态
                stringSelector.ClearSelection();
                
                // 先移除原本的字符串
                stringSelector.RemoveAvailableString(firstString);
                stringSelector.RemoveAvailableString(secondString);
                
                // 检查合并结果是否在target列表中
                if (PublicData.IsCharacterInTargetList(originalString))
                {
                    Debug.Log($"合并结果 '{originalString}' 在target列表中，准备飞向目标位置");
                    
                    // 获取目标位置
                    Transform targetPosition = PublicData.GetTargetPositionForCharacter(originalString);
                    Debug.Log($"获取到的目标位置: {(targetPosition != null ? targetPosition.name : "null")}");
                    
                    if (targetPosition != null)
                    {
                        Debug.Log($"目标位置有效，开始创建飞行动画");
                        // 创建飞向目标位置的字符
                        CreateFlyingCharacter(originalString, targetPosition);
                        Debug.Log($"已创建飞向目标位置的字符: {originalString}");
                    }
                    else
                    {
                        Debug.LogError($"目标位置为空！字符 '{originalString}' 没有配置对应的目标位置Transform");
                        Debug.LogError("请在PublicData的Inspector中为target字符配置对应的目标位置");
                        stringSelector.AddAvailableString(originalString);
                    }
                }
                else
                {
                    Debug.Log($"合并结果 '{originalString}' 不在target列表中，添加到可用字符串列表");
                    Debug.Log($"当前target列表: [{string.Join(", ", PublicData.targetList)}]");
                    // 最后添加合并后的新字符串
                    stringSelector.AddAvailableString(originalString);
                }
                
                // 重新创建所有按钮以确保索引正确
                stringSelector.RecreateAllButtonsPublic();
                
                // 确保最大选择数仍然是2
                stringSelector.SetMaxSelectionCount(2);
                
                // 清空选择
                stringSelector.ClearSelection();
                
                Debug.Log($"已移除原本的字符串: {firstString} 和 {secondString}，处理新字符串: {originalString}，并重新创建按钮");
            }
            else
            {
                Debug.LogWarning($"无法找到由 '{firstString}' 和 '{secondString}' 组成的有效合并结果");
                // 播放失败音效
                if (AudioManager.Instance != null && AudioManager.Instance.sfxOperationFailure != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxOperationFailure);
                }
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
    
    // 创建飞向目标位置的字符
    private void CreateFlyingCharacter(string character, Transform targetPosition)
    {
        Debug.Log($"=== 开始创建飞向目标位置的字符 ===");
        Debug.Log($"字符: '{character}'");
        Debug.Log($"目标位置: {targetPosition.name} at {targetPosition.position}");
        
        // 获取字符对应的Sprite
        Sprite characterSprite = PublicData.GetCharacterSprite(character);
        if (characterSprite == null)
        {
            Debug.LogError($"未找到字符 '{character}' 对应的Sprite");
            Debug.LogError("请检查PublicData中的characterSpriteMappings是否包含该字符");
            return;
        }
        Debug.Log($"成功获取字符Sprite: {characterSprite.name}");
        
        // 创建GameObject
        GameObject flyingCharacter = new GameObject($"Flying_{character}");
        Debug.Log($"创建了GameObject: {flyingCharacter.name}");
        
        // 添加SpriteRenderer组件
        SpriteRenderer spriteRenderer = flyingCharacter.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = characterSprite;
        spriteRenderer.sortingOrder = 100; // 确保在最前面显示
        Debug.Log($"添加了SpriteRenderer组件，sortingOrder: {spriteRenderer.sortingOrder}");
        
        // 设置初始位置（在屏幕中央或玩家位置）
        Vector3 startPosition = Vector3.zero;
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            startPosition = player.transform.position;
            Debug.Log($"使用玩家位置作为起始位置: {startPosition}");
        }
        else
        {
            Debug.LogWarning("未找到Player对象，使用原点作为起始位置");
        }
        flyingCharacter.transform.position = startPosition;
        
        // 开始飞行动画
        Debug.Log($"开始启动飞行动画协程");
        StartCoroutine(FlyToTarget(flyingCharacter, targetPosition.position, character));
        
        Debug.Log($"=== 飞向目标位置的字符创建完成 ===");
    }
    
    // 飞向目标的协程
    private IEnumerator FlyToTarget(GameObject flyingCharacter, Vector3 targetPosition, string character)
    {
        Debug.Log($"=== 开始飞行动画协程 ===");
        Debug.Log($"字符: '{character}'");
        Debug.Log($"起始位置: {flyingCharacter.transform.position}");
        Debug.Log($"目标位置: {targetPosition}");
        
        Vector3 startPosition = flyingCharacter.transform.position;
        float duration = 1.0f; // 飞行时间1秒
        float elapsedTime = 0f;
        
        Debug.Log($"动画持续时间: {duration}秒");
        
        // 播放飞行动画音效
        if (AudioManager.Instance != null && AudioManager.Instance.sfxGoalFlyIn != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxGoalFlyIn);
            Debug.Log("播放了飞行动画音效");
        }
        else
        {
            Debug.LogWarning("AudioManager或飞行动画音效为空");
        }
        
        Debug.Log("开始动画循环...");
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            // 使用缓动函数使动画更自然
            float easeProgress = Mathf.SmoothStep(0f, 1f, progress);
            
            // 更新位置
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, easeProgress);
            flyingCharacter.transform.position = newPosition;
            
            // 添加一些缩放效果
            float scale = 1f + Mathf.Sin(progress * Mathf.PI) * 0.2f;
            flyingCharacter.transform.localScale = Vector3.one * scale;
            
            // 每0.2秒输出一次进度
            if (Mathf.FloorToInt(progress * 5) != Mathf.FloorToInt((progress - Time.deltaTime / duration) * 5))
            {
                Debug.Log($"动画进度: {progress:P0}, 位置: {newPosition}, 缩放: {scale:F2}");
            }
            
            yield return null;
        }
        
        // 确保最终位置准确
        flyingCharacter.transform.position = targetPosition;
        flyingCharacter.transform.localScale = Vector3.one;
        
        Debug.Log($"飞行动画完成，最终位置: {flyingCharacter.transform.position}");
        
        // 延迟后销毁飞行的字符对象
        Debug.Log("等待0.5秒后销毁对象...");
        yield return new WaitForSeconds(0.5f);
        Destroy(flyingCharacter);
        
        Debug.Log($"=== 飞行动画协程结束，已销毁对象: {character} ===");
    }
}
