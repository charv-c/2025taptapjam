using UnityEngine;
using System.Collections;

/// <summary>
/// 鼠标光标样式管理器 - 统一管理游戏中的鼠标样式
/// 支持默认样式和按下样式的自动切换
/// </summary>
public class MouseCursorManager : MonoBehaviour
{
    public static MouseCursorManager Instance { get; private set; }
    
    [Header("鼠标光标素材设置")]
    [Tooltip("默认鼠标光标纹理")]
    [SerializeField] private Texture2D defaultCursorTexture;
    [Tooltip("鼠标按下时的光标纹理")]
    [SerializeField] private Texture2D pressedCursorTexture;
    
    [Header("光标设置")]
    [Tooltip("光标热点位置 (相对于纹理左上角的像素偏移)")]
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero;
    
    [Header("调试设置")]
    [SerializeField] private bool enableDebugLog = true;
    
    // 当前光标状态
    private bool isPressed = false;
    private bool isInitialized = false;
    
    private void Awake()
    {
        // 实现单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (enableDebugLog)
            {
                GameLogger.LogSystem("MouseCursorManager: 初始化开始");
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        StartCoroutine(InitializeCursorSystem());
    }
    
    /// <summary>
    /// 初始化光标系统
    /// </summary>
    private IEnumerator InitializeCursorSystem()
    {
        // 等一帧确保场景完全加载
        yield return null;
        
        // 尝试从Resources加载光标纹理（如果Inspector中未设置）
        LoadCursorTexturesFromResources();
        
        // 验证光标纹理
        if (ValidateCursorTextures())
        {
            // 设置默认光标
            SetDefaultCursor();
            isInitialized = true;
            
            if (enableDebugLog)
            {
                GameLogger.LogSystem("MouseCursorManager: 光标系统初始化完成");
            }
        }
        else
        {
            GameLogger.LogError("MouseCursorManager: 光标纹理验证失败，系统无法正常工作");
        }
    }
    
    /// <summary>
    /// 从Resources文件夹加载光标纹理（备用方案）
    /// </summary>
    private void LoadCursorTexturesFromResources()
    {
        if (defaultCursorTexture == null)
        {
            defaultCursorTexture = Resources.Load<Texture2D>("Cursors/DefaultCursor");
            if (defaultCursorTexture != null && enableDebugLog)
            {
                GameLogger.LogSystem("MouseCursorManager: 从Resources加载默认光标纹理");
            }
        }
        
        if (pressedCursorTexture == null)
        {
            pressedCursorTexture = Resources.Load<Texture2D>("Cursors/PressedCursor");
            if (pressedCursorTexture != null && enableDebugLog)
            {
                GameLogger.LogSystem("MouseCursorManager: 从Resources加载按下光标纹理");
            }
        }
    }
    
    /// <summary>
    /// 验证光标纹理是否正确设置
    /// </summary>
    private bool ValidateCursorTextures()
    {
        bool isValid = true;
        
        if (defaultCursorTexture == null)
        {
            GameLogger.LogError("MouseCursorManager: 默认光标纹理未设置，请在Inspector中配置或将纹理放在Resources/Cursors/DefaultCursor路径");
            isValid = false;
        }
        
        if (pressedCursorTexture == null)
        {
            GameLogger.LogError("MouseCursorManager: 按下光标纹理未设置，请在Inspector中配置或将纹理放在Resources/Cursors/PressedCursor路径");
            isValid = false;
        }
        
        // 检查纹理类型是否正确
        if (defaultCursorTexture != null && !IsCursorTexture(defaultCursorTexture))
        {
            GameLogger.LogWarning("MouseCursorManager: 默认光标纹理类型不是Cursor，请在Inspector中将Texture Type设置为Cursor");
        }
        
        if (pressedCursorTexture != null && !IsCursorTexture(pressedCursorTexture))
        {
            GameLogger.LogWarning("MouseCursorManager: 按下光标纹理类型不是Cursor，请在Inspector中将Texture Type设置为Cursor");
        }
        
        return isValid;
    }
    
    /// <summary>
    /// 检查纹理是否为Cursor类型
    /// </summary>
    private bool IsCursorTexture(Texture2D texture)
    {
        // 这是一个简单的检查，实际上Unity会在内部处理Cursor类型的纹理
        return texture != null && texture.format != TextureFormat.DXT1; // Cursor纹理通常不使用DXT压缩
    }
    
    private void Update()
    {
        if (!isInitialized) return;
        
        // 检测鼠标按下和抬起
        bool currentPressed = Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2);
        
        if (currentPressed != isPressed)
        {
            isPressed = currentPressed;
            
            if (isPressed)
            {
                SetPressedCursor();
            }
            else
            {
                SetDefaultCursor();
            }
        }
    }
    
    /// <summary>
    /// 设置默认光标样式
    /// </summary>
    public void SetDefaultCursor()
    {
        if (defaultCursorTexture != null)
        {
            Cursor.SetCursor(defaultCursorTexture, cursorHotspot, CursorMode.Auto);
            
            if (enableDebugLog)
            {
                GameLogger.LogDev("MouseCursorManager: 设置默认光标样式");
            }
        }
    }
    
    /// <summary>
    /// 设置按下光标样式
    /// </summary>
    public void SetPressedCursor()
    {
        if (pressedCursorTexture != null)
        {
            Cursor.SetCursor(pressedCursorTexture, cursorHotspot, CursorMode.Auto);
            
            if (enableDebugLog)
            {
                GameLogger.LogDev("MouseCursorManager: 设置按下光标样式");
            }
        }
    }
    
    /// <summary>
    /// 重置为系统默认光标
    /// </summary>
    public void ResetToSystemCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        
        if (enableDebugLog)
        {
            GameLogger.LogSystem("MouseCursorManager: 重置为系统默认光标");
        }
    }
    
    /// <summary>
    /// 设置光标热点位置
    /// </summary>
    /// <param name="hotspot">新的热点位置</param>
    public void SetCursorHotspot(Vector2 hotspot)
    {
        cursorHotspot = hotspot;
        
        // 如果当前已初始化，重新应用当前光标
        if (isInitialized)
        {
            if (isPressed)
            {
                SetPressedCursor();
            }
            else
            {
                SetDefaultCursor();
            }
        }
        
        if (enableDebugLog)
        {
            GameLogger.LogSystem($"MouseCursorManager: 光标热点设置为 {hotspot}");
        }
    }
    
    /// <summary>
    /// 手动设置光标纹理
    /// </summary>
    /// <param name="defaultTexture">默认光标纹理</param>
    /// <param name="pressedTexture">按下光标纹理</param>
    public void SetCursorTextures(Texture2D defaultTexture, Texture2D pressedTexture)
    {
        defaultCursorTexture = defaultTexture;
        pressedCursorTexture = pressedTexture;
        
        if (ValidateCursorTextures())
        {
            // 重新应用当前光标
            if (isPressed)
            {
                SetPressedCursor();
            }
            else
            {
                SetDefaultCursor();
            }
            
            if (enableDebugLog)
            {
                GameLogger.LogSystem("MouseCursorManager: 光标纹理已更新");
            }
        }
    }
    
    /// <summary>
    /// 获取当前是否已初始化
    /// </summary>
    public bool IsInitialized()
    {
        return isInitialized;
    }
    
    /// <summary>
    /// 获取当前光标状态信息（调试用）
    /// </summary>
    public string GetCursorStatus()
    {
        string status = $"初始化: {isInitialized}, 按下状态: {isPressed}";
        status += $"\n默认纹理: {(defaultCursorTexture != null ? defaultCursorTexture.name : "未设置")}";
        status += $"\n按下纹理: {(pressedCursorTexture != null ? pressedCursorTexture.name : "未设置")}";
        status += $"\n热点位置: {cursorHotspot}";
        return status;
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && isInitialized)
        {
            // 应用重新获得焦点时，恢复当前应有的光标状态
            if (isPressed)
            {
                SetPressedCursor();
            }
            else
            {
                SetDefaultCursor();
            }
            
            if (enableDebugLog)
            {
                GameLogger.LogDev("MouseCursorManager: 应用重新获得焦点，恢复光标状态");
            }
        }
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            // 在销毁时重置为系统光标
            ResetToSystemCursor();
            Instance = null;
        }
    }
}
