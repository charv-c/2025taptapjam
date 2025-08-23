using UnityEngine;

// 米字格控制器：管理misquare对象的sprite变化
public class MiSquareController : MonoBehaviour
{
    [Header("米字格设置")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool enableLogging = true;
    
    // 当前显示的字符
    private string currentCharacter = "";
    
    void Start()
    {
        // 如果没有手动设置SpriteRenderer，自动获取
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (spriteRenderer == null)
        {
            Debug.LogError($"MiSquareController: 对象 '{gameObject.name}' 没有SpriteRenderer组件");
        }
        else
        {
            Debug.Log($"MiSquareController: 对象 '{gameObject.name}' 初始化完成");
        }
    }
    
    // 设置米字格sprite
    public void SetMiSquareSprite(string character)
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer组件为空，无法设置sprite");
            return;
        }
        
        // 获取字符对应的米字格Sprite
        Sprite miZiGeSprite = PublicData.GetMiZiGeSprite(character);
        if (miZiGeSprite != null)
        {
            spriteRenderer.sprite = miZiGeSprite;
            currentCharacter = character;
            
            if (enableLogging)
            {
                Debug.Log($"MiSquareController: 已设置米字格sprite为字符 '{character}'");
            }
        }
        else
        {
            Debug.LogWarning($"MiSquareController: 未找到字符 '{character}' 对应的米字格Sprite");
        }
    }
    
    // 设置普通sprite
    public void SetNormalSprite(string character)
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer组件为空，无法设置sprite");
            return;
        }
        
        // 获取字符对应的普通Sprite
        Sprite normalSprite = PublicData.GetCharacterSprite(character);
        if (normalSprite != null)
        {
            spriteRenderer.sprite = normalSprite;
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
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = null;
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
        return spriteRenderer != null && spriteRenderer.sprite != null;
    }
    
    // 设置SpriteRenderer引用
    public void SetSpriteRenderer(SpriteRenderer renderer)
    {
        spriteRenderer = renderer;
        Debug.Log($"MiSquareController: SpriteRenderer已设置为: {renderer?.gameObject.name ?? "null"}");
    }
    
    // 获取SpriteRenderer引用
    public SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }
}
