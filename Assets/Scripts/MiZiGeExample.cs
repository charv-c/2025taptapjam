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
        }
        
        if (rightMiZiGe != null)
        {
            rightMiZiGe.SetMiZiGeType(MiSquareController.MiZiGeType.Right);
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
        }
        
        if (rightMiZiGe != null)
        {
            rightMiZiGe.SetMiSquareSprite(testCharacter);
        }
    }
    
    // 测试不同字符的图片映射
    public void TestDifferentCharacters()
    {
        string[] testCharacters = { "人", "木", "火", "山", "日", "王" };
        
        foreach (string character in testCharacters)
        {
            // 检查左米字格是否有对应的图片
            if (PublicData.HasLeftMiZiGeSprite(character))
            {
                // 左米字格有字符的图片映射
            }
            else
            {
                // 左米字格没有字符的图片映射
            }
            
            // 检查右米字格是否有对应的图片
            if (PublicData.HasRightMiZiGeSprite(character))
            {
                // 右米字格有字符的图片映射
            }
            else
            {
                // 右米字格没有字符的图片映射
            }
        }
    }
    
    // 获取所有可用的米字格字符
    public void ListAllAvailableCharacters()
    {
        var leftChars = PublicData.GetAllLeftMiZiGeCharacters();
        var rightChars = PublicData.GetAllRightMiZiGeCharacters();
        // 字符列表功能已移除
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
