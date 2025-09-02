using UnityEngine;
using UnityEngine.UI;

// 米字格控制器：管理misquare对象的sprite变化
public class MiSquareController : MonoBehaviour
{
    [Header("米字格设置")]
    [SerializeField] private Image imageComponent;
    [SerializeField] private bool enableLogging = true;
    
    [Header("米字格类型")]
    [SerializeField] private MiZiGeType miZiGeType = MiZiGeType.Left;
    
    // 当前显示的字符
    private string currentCharacter = "";
    
    // 米字格类型枚举
    public enum MiZiGeType
    {
        Left,       // 左米字格
        Right       // 右米字格
    }
    
    void Start()
    {
        // 如果没有手动设置Image，自动获取
        if (imageComponent == null)
        {
            imageComponent = GetComponent<Image>();
        }
        
        if (imageComponent == null)
        {
            Debug.LogError($"MiSquareController: 对象 '{gameObject.name}' 没有Image组件");
        }
        else
        {
            Debug.Log($"MiSquareController: 对象 '{gameObject.name}' 初始化完成");
        }
    }
    
    // 设置米字格sprite
    public void SetMiSquareSprite(string character)
    {
        if (imageComponent == null)
        {
            Debug.LogError("Image组件为空，无法设置sprite");
            return;
        }
        
        // 根据米字格类型获取对应的Sprite
        Sprite miZiGeSprite = null;
        switch (miZiGeType)
        {
            case MiZiGeType.Left:
                miZiGeSprite = PublicData.GetLeftMiZiGeSprite(character);
                break;
            case MiZiGeType.Right:
                miZiGeSprite = PublicData.GetRightMiZiGeSprite(character);
                break;
            default:
                // 默认使用左米字格
                miZiGeSprite = PublicData.GetLeftMiZiGeSprite(character);
                break;
        }
        
        if (miZiGeSprite != null)
        {
            imageComponent.sprite = miZiGeSprite;
            currentCharacter = character;
            
            if (enableLogging)
            {
                Debug.Log($"MiSquareController: 已设置{miZiGeType}米字格sprite为字符 '{character}'");
            }
        }
        else
        {
            Debug.LogWarning($"MiSquareController: 未找到字符 '{character}' 对应的{miZiGeType}米字格Sprite");
        }
    }
    
    // 设置普通sprite
    public void SetNormalSprite(string character)
    {
        if (imageComponent == null)
        {
            Debug.LogError("Image组件为空，无法设置sprite");
            return;
        }
        
        // 获取字符对应的普通Sprite
        Sprite normalSprite = PublicData.GetCharacterSprite(character);
        if (normalSprite != null)
        {
            imageComponent.sprite = normalSprite;
            currentCharacter = character;
            
            if (enableLogging)
            {
                Debug.Log($"MiSquareController: 已设置普通sprite为字符 '{character}'");
            }
        }
        else
        {
            Debug.LogWarning($"MiSquareController: 未找到字符 '{character}' 对应的普通Sprite");
        }
    }
    
    // 获取当前显示的字符
    public string GetCurrentCharacter()
    {
        return currentCharacter;
    }
    
    // 清除sprite
    public void ClearSprite()
    {
        if (imageComponent != null)
        {
            imageComponent.sprite = null;
            currentCharacter = "";
            
            if (enableLogging)
            {
                Debug.Log("MiSquareController: 已清除sprite");
            }
        }
    }
    
    // 检查是否有sprite
    public bool HasSprite()
    {
        return imageComponent != null && imageComponent.sprite != null;
    }
    
    // 设置Image引用
    public void SetImage(Image image)
    {
        imageComponent = image;
        Debug.Log($"MiSquareController: Image已设置为: {image?.gameObject.name ?? "null"}");
    }
    
    // 获取Image引用
    public Image GetImage()
    {
        return imageComponent;
    }
    
    // 设置米字格类型
    public void SetMiZiGeType(MiZiGeType type)
    {
        miZiGeType = type;
        if (enableLogging)
        {
            Debug.Log($"MiSquareController: 米字格类型已设置为 {type}");
        }
    }
    
    // 获取米字格类型
    public MiZiGeType GetMiZiGeType()
    {
        return miZiGeType;
    }
    
    // 检查是否有对应类型的米字格Sprite
    public bool HasMiZiGeSprite(string character)
    {
        switch (miZiGeType)
        {
            case MiZiGeType.Left:
                return PublicData.HasLeftMiZiGeSprite(character);
            case MiZiGeType.Right:
                return PublicData.HasRightMiZiGeSprite(character);
            default:
                // 默认检查左米字格
                return PublicData.HasLeftMiZiGeSprite(character);
        }
    }
}
