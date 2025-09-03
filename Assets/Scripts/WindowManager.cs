using UnityEngine;

public class WindowManager : MonoBehaviour
{
    [Header("窗口设置")]
    [SerializeField] private bool setFullscreenOnStart = true;
    [SerializeField] private bool maintainAspectRatio = true;
    [SerializeField] private Vector2 targetAspectRatio = new Vector2(16, 9); // 目标宽高比
    
    [Header("摄像机设置")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private bool adjustCameraOnStart = true;
    
    void Start()
    {
        // 设置全屏
        if (setFullscreenOnStart)
        {
            SetFullscreen();
        }
        
        // 调整摄像机
        if (adjustCameraOnStart)
        {
            AdjustCamera();
        }
    }
    
    void Update()
    {
        // 检测F11键切换全屏
        if (Input.GetKeyDown(KeyCode.F11))
        {
            ToggleFullscreen();
        }
        
        // 检测ESC键退出全屏
        if (Input.GetKeyDown(KeyCode.Escape) && Screen.fullScreen)
        {
            SetWindowed();
        }
    }
    
    // 设置全屏
    public void SetFullscreen()
    {
        Screen.fullScreen = true;
    }
    
    // 设置窗口模式
    public void SetWindowed()
    {
        Screen.fullScreen = false;
    }
    
    // 切换全屏/窗口模式
    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        
        // 重新调整摄像机
        AdjustCamera();
    }
    
    // 调整摄像机以适应屏幕比例
    public void AdjustCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (mainCamera == null)
        {
            return;
        }
        
        // 获取当前屏幕分辨率
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        
        // 计算当前宽高比
        float currentAspectRatio = screenWidth / screenHeight;
        float targetAspect = targetAspectRatio.x / targetAspectRatio.y;
        

        
        if (maintainAspectRatio)
        {
            // 计算视口矩形
            Rect viewportRect = CalculateViewportRect(currentAspectRatio, targetAspect);
            
            // 设置摄像机视口
            mainCamera.rect = viewportRect;
            

        }
        else
        {
            // 重置摄像机视口
            mainCamera.rect = new Rect(0, 0, 1, 1);
        }
    }
    
    // 计算视口矩形以保持宽高比
    private Rect CalculateViewportRect(float currentAspect, float targetAspect)
    {
        float scaleWidth = currentAspect / targetAspect;
        float scaleHeight = targetAspect / currentAspect;
        
        if (scaleWidth > scaleHeight)
        {
            // 屏幕太宽，需要缩放宽度
            float width = scaleHeight;
            float x = (1.0f - width) * 0.5f;
            return new Rect(x, 0, width, 1.0f);
        }
        else
        {
            // 屏幕太高，需要缩放高度
            float height = scaleWidth;
            float y = (1.0f - height) * 0.5f;
            return new Rect(0, y, 1.0f, height);
        }
    }
    
    // 设置目标宽高比
    public void SetTargetAspectRatio(float width, float height)
    {
        targetAspectRatio = new Vector2(width, height);
        AdjustCamera();
    }
    
    // 获取当前屏幕信息
    public void LogScreenInfo()
    {
        // 屏幕信息记录功能已移除
    }
    
    // 公共方法：重新调整所有设置
    [ContextMenu("重新调整设置")]
    public void ReapplySettings()
    {
        if (setFullscreenOnStart)
        {
            SetFullscreen();
        }
        AdjustCamera();
        LogScreenInfo();
    }
}
