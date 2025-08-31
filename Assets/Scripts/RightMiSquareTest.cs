using UnityEngine;

/// <summary>
/// 右边米字格字符合成功能测试脚本
/// 用于验证右边米字格在字符合成时是否正确使用右边米字格字典的sprite
/// </summary>
public class RightMiSquareTest : MonoBehaviour
{
    [Header("测试设置")]
    [SerializeField] private MiSquareController rightMiSquareController;
    [SerializeField] private bool enableLogging = true;
    
    [Header("测试字符")]
    [SerializeField] private string testCharacter = "人";
    
    void Start()
    {
        // 如果没有手动设置，尝试自动获取
        if (rightMiSquareController == null)
        {
            rightMiSquareController = GetComponent<MiSquareController>();
        }
        
        // 确保米字格类型设置为右米字格
        if (rightMiSquareController != null)
        {
            rightMiSquareController.SetMiZiGeType(MiSquareController.MiZiGeType.Right);
            if (enableLogging)
            {
                Debug.Log($"RightMiSquareTest: 已设置米字格类型为右米字格");
            }
        }
        else
        {
            Debug.LogError($"RightMiSquareTest: 对象 '{gameObject.name}' 没有MiSquareController组件");
        }
    }
    
    /// <summary>
    /// 测试右边米字格sprite设置
    /// </summary>
    [ContextMenu("测试右边米字格sprite设置")]
    public void TestRightMiSquareSprite()
    {
        if (rightMiSquareController == null)
        {
            Debug.LogError("RightMiSquareTest: 右边米字格控制器为空");
            return;
        }
        
        if (enableLogging)
        {
            Debug.Log($"RightMiSquareTest: 开始测试字符 '{testCharacter}' 的右边米字格sprite");
        }
        
        // 检查是否有右米字格sprite
        bool hasRightSprite = rightMiSquareController.HasMiZiGeSprite(testCharacter);
        if (enableLogging)
        {
            Debug.Log($"RightMiSquareTest: 字符 '{testCharacter}' 是否有右米字格sprite: {hasRightSprite}");
        }
        
        if (hasRightSprite)
        {
            // 使用右米字格sprite
            rightMiSquareController.SetMiSquareSprite(testCharacter);
            if (enableLogging)
            {
                Debug.Log($"RightMiSquareTest: 已设置右米字格为字符 '{testCharacter}'，使用右米字格sprite");
            }
        }
        else
        {
            // 使用普通sprite
            rightMiSquareController.SetNormalSprite(testCharacter);
            if (enableLogging)
            {
                Debug.Log($"RightMiSquareTest: 字符 '{testCharacter}' 没有右米字格sprite，使用普通sprite");
            }
        }
    }
    
    /// <summary>
    /// 测试多个字符的右边米字格sprite
    /// </summary>
    [ContextMenu("测试多个字符的右边米字格sprite")]
    public void TestMultipleCharacters()
    {
        string[] testCharacters = { "人", "木", "火", "山", "日", "王", "亭", "仙", "伙", "停" };
        
        foreach (string character in testCharacters)
        {
            if (enableLogging)
            {
                Debug.Log($"RightMiSquareTest: 测试字符 '{character}'");
            }
            
            // 检查是否有右米字格sprite
            bool hasRightSprite = PublicData.HasRightMiZiGeSprite(character);
            if (enableLogging)
            {
                Debug.Log($"RightMiSquareTest: 字符 '{character}' 是否有右米字格sprite: {hasRightSprite}");
            }
            
            if (hasRightSprite)
            {
                // 获取右米字格sprite
                Sprite rightSprite = PublicData.GetRightMiZiGeSprite(character);
                if (enableLogging)
                {
                    Debug.Log($"RightMiSquareTest: 字符 '{character}' 的右米字格sprite: {rightSprite?.name ?? "null"}");
                }
            }
        }
    }
    
    /// <summary>
    /// 列出所有右米字格字符
    /// </summary>
    [ContextMenu("列出所有右米字格字符")]
    public void ListAllRightMiZiGeCharacters()
    {
        var rightCharacters = PublicData.GetAllRightMiZiGeCharacters();
        if (enableLogging)
        {
            Debug.Log($"RightMiSquareTest: 所有右米字格字符 ({rightCharacters.Count} 个):");
            foreach (string character in rightCharacters)
            {
                Debug.Log($"RightMiSquareTest: - {character}");
            }
        }
    }
    
    /// <summary>
    /// 测试字符合成后的右边米字格更新
    /// </summary>
    [ContextMenu("测试字符合成后的右边米字格更新")]
    public void TestCombineSuccessUpdate()
    {
        if (enableLogging)
        {
            Debug.Log("RightMiSquareTest: 模拟字符合成成功事件");
        }
        
        // 模拟发送combine_success广播
        if (BroadcastManager.Instance != null)
        {
            BroadcastManager.Instance.BroadcastToAll("combine_success");
            if (enableLogging)
            {
                Debug.Log("RightMiSquareTest: 已发送combine_success广播");
            }
        }
        else
        {
            Debug.LogError("RightMiSquareTest: BroadcastManager不存在");
        }
    }
    
    /// <summary>
    /// 测试化字功能
    /// </summary>
    [ContextMenu("测试化字功能")]
    public void TestChangeMi()
    {
        if (enableLogging)
        {
            Debug.Log("RightMiSquareTest: 测试化字功能");
        }
        
        // 模拟玩家携带"人"字符与"亭"对象交互
        string testObject = "亭";
        string combinedCharacter = PublicData.FindOriginalString(testObject, "人");
        
        if (enableLogging)
        {
            Debug.Log($"RightMiSquareTest: 模拟 '{testObject}' + '人' = '{combinedCharacter}'");
        }
        
        if (combinedCharacter != null && rightMiSquareController != null)
        {
            // 检查是否有右米字格sprite
            bool hasRightSprite = rightMiSquareController.HasMiZiGeSprite(combinedCharacter);
            if (enableLogging)
            {
                Debug.Log($"RightMiSquareTest: 字符 '{combinedCharacter}' 是否有右米字格sprite: {hasRightSprite}");
            }
            
            if (hasRightSprite)
            {
                // 使用右米字格sprite
                rightMiSquareController.SetMiSquareSprite(combinedCharacter);
                if (enableLogging)
                {
                    Debug.Log($"RightMiSquareTest: 已设置右米字格为字符 '{combinedCharacter}'，使用右米字格sprite");
                }
            }
            else
            {
                // 使用普通sprite
                rightMiSquareController.SetNormalSprite(combinedCharacter);
                if (enableLogging)
                {
                    Debug.Log($"RightMiSquareTest: 字符 '{combinedCharacter}' 没有右米字格sprite，使用普通sprite");
                }
            }
        }
        else
        {
            if (enableLogging)
            {
                Debug.LogWarning($"RightMiSquareTest: 无法合成字符或右边米字格控制器为空");
            }
        }
    }
    
    /// <summary>
    /// 设置测试字符
    /// </summary>
    /// <param name="character">要测试的字符</param>
    public void SetTestCharacter(string character)
    {
        testCharacter = character;
        if (enableLogging)
        {
            Debug.Log($"RightMiSquareTest: 测试字符已设置为 '{character}'");
        }
    }
    
    /// <summary>
    /// 设置右边米字格控制器
    /// </summary>
    /// <param name="controller">米字格控制器</param>
    public void SetRightMiSquareController(MiSquareController controller)
    {
        rightMiSquareController = controller;
        if (enableLogging)
        {
            Debug.Log($"RightMiSquareTest: 已设置右边米字格控制器: {controller?.gameObject.name ?? "null"}");
        }
    }
}
