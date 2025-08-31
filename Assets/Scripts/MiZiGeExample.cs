using UnityEngine;

/// <summary>
/// 米字格图片映射示例脚本
/// 演示如何为左右两个米字格设置不同的图片映射
/// </summary>
public class MiZiGeExample : MonoBehaviour
{
    [Header("示例设置")]
    [SerializeField] private MiSquareController leftMiZiGe;
    [SerializeField] private MiSquareController rightMiZiGe;
    
    [Header("测试字符")]
    [SerializeField] private string testCharacter = "人";
    
    void Start()
    {
        // 确保左右米字格控制器已设置正确的类型
        if (leftMiZiGe != null)
        {
            leftMiZiGe.SetMiZiGeType(MiSquareController.MiZiGeType.Left);
            Debug.Log("左米字格类型已设置为 Left");
        }
        
        if (rightMiZiGe != null)
        {
            rightMiZiGe.SetMiZiGeType(MiSquareController.MiZiGeType.Right);
            Debug.Log("右米字格类型已设置为 Right");
        }
        
        // 测试设置图片
        TestMiZiGeSprites();
    }
    
    // 测试米字格图片设置
    public void TestMiZiGeSprites()
    {
        if (leftMiZiGe != null)
        {
            leftMiZiGe.SetMiSquareSprite(testCharacter);
            Debug.Log($"已为左米字格设置字符: {testCharacter}");
        }
        
        if (rightMiZiGe != null)
        {
            rightMiZiGe.SetMiSquareSprite(testCharacter);
            Debug.Log($"已为右米字格设置字符: {testCharacter}");
        }
    }
    
    // 测试不同字符的图片映射
    public void TestDifferentCharacters()
    {
        string[] testCharacters = { "人", "木", "火", "山", "日", "王" };
        
        foreach (string character in testCharacters)
        {
            Debug.Log($"测试字符: {character}");
            
            // 检查左米字格是否有对应的图片
            if (PublicData.HasLeftMiZiGeSprite(character))
            {
                Debug.Log($"左米字格有字符 '{character}' 的图片映射");
            }
            else
            {
                Debug.LogWarning($"左米字格没有字符 '{character}' 的图片映射");
            }
            
            // 检查右米字格是否有对应的图片
            if (PublicData.HasRightMiZiGeSprite(character))
            {
                Debug.Log($"右米字格有字符 '{character}' 的图片映射");
            }
            else
            {
                Debug.LogWarning($"右米字格没有字符 '{character}' 的图片映射");
            }
        }
    }
    
    // 获取所有可用的米字格字符
    public void ListAllAvailableCharacters()
    {
        Debug.Log("=== 左米字格字符 ===");
        var leftChars = PublicData.GetAllLeftMiZiGeCharacters();
        foreach (string character in leftChars)
        {
            Debug.Log($"左: {character}");
        }
        
        Debug.Log("=== 右米字格字符 ===");
        var rightChars = PublicData.GetAllRightMiZiGeCharacters();
        foreach (string character in rightChars)
        {
            Debug.Log($"右: {character}");
        }
    }
    
    // 在Inspector中调用的测试方法
    [ContextMenu("测试米字格图片设置")]
    public void TestMiZiGeSetup()
    {
        TestMiZiGeSprites();
    }
    
    [ContextMenu("测试不同字符")]
    public void TestCharacters()
    {
        TestDifferentCharacters();
    }
    
    [ContextMenu("列出所有可用字符")]
    public void ListCharacters()
    {
        ListAllAvailableCharacters();
    }
}
