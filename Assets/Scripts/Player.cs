using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float screenBoundaryOffset = 0.5f; // 距离屏幕边界的偏移量
    [SerializeField] private bool isPlayer1 = true; // 是否为玩家1（左半边）
    
    [Header("初始位置设置")]
    [SerializeField] private bool useCustomStartPosition = false; // 是否使用自定义初始位置
    [SerializeField] private Vector3 customStartPosition = Vector3.zero; // 自定义初始位置

    private Camera mainCamera;
    private float screenWidth;
    private float screenHeight;
    private float playerWidth;
    private float playerHeight;
    private bool inputEnabled = false; // 控制输入是否启用，默认禁用
    private float currentHorizontalInput = 0f;
    private float currentVerticalInput = 0f;
    public string CarryCharacter="人";
    
    void Start()
    {
        // 获取主摄像机
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        // 计算屏幕边界
        CalculateScreenBounds();
        
        // 设置初始位置
        SetInitialPosition();
        
        // 确保初始位置符合移动限制
        ClampToScreen();
    }

    void Update()
    {
        HandleMovement();
        ClampToScreen();
        
        // 检测R键按下
        if (Input.GetKeyDown(KeyCode.R))
        {
            OnRKeyPressed();
        }
    }

    

    void HandleMovement()
    {
        // 只有在输入启用时才处理移动
        if (inputEnabled)
        {
            // 获取输入
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            // 计算移动向量
            Vector3 movement = new Vector3(horizontalInput, verticalInput, 0f);

            // 应用移动
            transform.Translate(movement * moveSpeed * Time.deltaTime);
        }
        // 当输入被禁用时，不进行任何移动
    }

    void ClampToScreen()
    {
        // 获取玩家在世界坐标中的位置
        Vector3 playerPosition = transform.position;

        // 检查CarryCharacter是否为"仙"
        if (CarryCharacter == "仙")
        {
            // 仙状态：使用世界坐标限制，左上(-8.9, 4.6)，右下(8.9, -2.4)
            float clampedX = Mathf.Clamp(playerPosition.x, -8.9f, 8.9f);
            float clampedY = Mathf.Clamp(playerPosition.y, -2.4f, 4.6f);
            
            Vector3 clampedPosition = new Vector3(clampedX, clampedY, playerPosition.z);
            transform.position = clampedPosition;
        }
        else
        {
            // 正常状态：根据玩家类型限制移动范围
            float clampedX, clampedY;
            
            if (isPlayer1)
            {
                // 玩家1只能在左半边移动（X轴限制在-8.9到0）
                clampedX = Mathf.Clamp(playerPosition.x, -8.9f, 0.5f);
            }
            else
            {
                // 玩家2只能在右半边移动（X轴限制在0到8.9）
                clampedX = Mathf.Clamp(playerPosition.x, 0.5f, 8.9f);
            }

            // Y轴移动范围限制在-2.4到2.4（使用世界坐标）
            clampedY = Mathf.Clamp(playerPosition.y, -2.4f, 2.4f);
            
            Vector3 clampedPosition = new Vector3(clampedX, clampedY, playerPosition.z);
            transform.position = clampedPosition;
        }
    }

    void CalculateScreenBounds()
    {
        // 获取屏幕尺寸
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        // 获取玩家的尺寸（假设使用SpriteRenderer）
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            playerWidth = spriteRenderer.bounds.size.x;
            playerHeight = spriteRenderer.bounds.size.y;
        }
        else
        {
            // 如果没有SpriteRenderer，使用默认值
            playerWidth = 1f;
            playerHeight = 1f;
        }
    }
    
    void SetInitialPosition()
    {
        if (useCustomStartPosition)
        {
            // 使用自定义初始位置
            transform.position = customStartPosition;
        }
        else
        {
            // 使用默认的智能初始位置
            SetDefaultStartPosition();
        }
        
        // 验证初始位置
    }
    
    void SetDefaultStartPosition()
    {
        // 根据玩家类型设置默认初始位置
        Vector3 defaultPosition = Vector3.zero;
        
        if (isPlayer1)
        {
            // Player1默认在左半边中央（X轴-4.45，Y轴0）
            defaultPosition = new Vector3(-4.45f, 0f, transform.position.z);
        }
        else
        {
            // Player2默认在右半边中央（X轴4.45，Y轴0）
            defaultPosition = new Vector3(4.45f, 0f, transform.position.z);
        }
        
        transform.position = defaultPosition;
    }


    
        // 公共方法：设置输入是否启用
        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
            if (!enabled)
            {
                // 禁用输入时，重置输入值
                currentHorizontalInput = 0f;
                currentVerticalInput = 0f;
                Debug.Log($"Player: 已禁用输入，重置输入值");
            }
            else
            {
                Debug.Log($"Player: 已启用输入");
            }
        }

        // 公共方法：设置外部输入值
        public void SetInput(float horizontal, float vertical)
        {
            currentHorizontalInput = horizontal;
            currentVerticalInput = vertical;
        }

            // 公共方法：获取输入启用状态
    public bool IsInputEnabled()
    {
        return inputEnabled;
    }
    
    // 公共方法：设置玩家类型
    public void SetPlayerType(bool isPlayer1)
    {
        this.isPlayer1 = isPlayer1;
    }
    
    // 公共方法：获取玩家类型
    public bool IsPlayer1()
    {
        return isPlayer1;
    }
    
    // 公共方法：设置自定义初始位置
    public void SetCustomStartPosition(Vector3 position)
    {
        customStartPosition = position;
        useCustomStartPosition = true;
    }
    
    // 公共方法：启用/禁用自定义初始位置
    public void SetUseCustomStartPosition(bool useCustom)
    {
        useCustomStartPosition = useCustom;
    }
    
    // 公共方法：获取自定义初始位置
    public Vector3 GetCustomStartPosition()
    {
        return customStartPosition;
    }
    
    // 公共方法：重置为默认初始位置
    public void ResetToDefaultPosition()
    {
        useCustomStartPosition = false;
        if (mainCamera != null)
        {
            SetDefaultStartPosition();
        }
    }
    
    // 公共方法：重置到初始状态
    public void ResetToInitialState()
    {
        useCustomStartPosition = false;
        if (mainCamera != null)
        {
            SetDefaultStartPosition();
        }
    }
    
    // 公共方法：获取Y轴限制范围
    public (float min, float max) GetYAxisLimits()
    {
        return (-2.4f, 2.4f);
    }
    
    // 公共方法：设置Y轴限制范围
    public void SetYAxisLimits(float minY, float maxY)
    {
        // 这里可以添加设置Y轴限制的逻辑
    }
    
    // 公共方法：检查当前位置是否在Y轴限制范围内
    public bool IsWithinYAxisLimits()
    {
        float currentY = transform.position.y;
        return currentY >= -2.4f && currentY <= 2.4f;
    }
    
    // 公共方法：强制将位置限制在Y轴范围内
    public void ClampToYAxisLimits()
    {
        Vector3 position = transform.position;
        position.y = Mathf.Clamp(position.y, -2.4f, 2.4f);
        transform.position = position;
    }
    
    // R键按下时的处理
    private void OnRKeyPressed()
    {
        if (CarryCharacter != "人")
        {
            CarryCharacter = "人";
        }
        
        RestoreAllHighlightScripts();
        ShowRainObject();
    }
    
    // 显示雨对象
    private void ShowRainObject()
    {
        // 查找场景中所有带有Highlight脚本的对象
        Highlight[] allHighlights = FindObjectsOfType<Highlight>();
        
        foreach (Highlight highlight in allHighlights)
        {
            if (highlight != null && highlight.letter == "雨")
            {
                // 直接调用雨对象的ShowObject方法
                highlight.ShowObject();
                Debug.Log($"R键按下：显示雨对象 {highlight.gameObject.name}");
            }
        }
    }
    
    // 恢复场景中所有物体的highlight脚本
    private void RestoreAllHighlightScripts()
    {
        // 查找场景中所有带有Highlight脚本的对象
        Highlight[] allHighlights = FindObjectsOfType<Highlight>();
        
        foreach (Highlight highlight in allHighlights)
        {
            if (highlight != null && !highlight.enabled)
            {
                highlight.enabled = true;
            }
        }
        
        ResetAllMiSquares();
    }
    
    // 将所有米字格设置为"人"对应的图片
    private void ResetAllMiSquares()
    {
        // 查找场景中所有带有MiSquareController脚本的对象
        MiSquareController[] allMiSquares = FindObjectsOfType<MiSquareController>();
        
        foreach (MiSquareController miSquare in allMiSquares)
        {
            if (miSquare != null)
            {
                // 设置为"人"对应的米字格图片
                miSquare.SetMiSquareSprite("人");
            }
        }
    }
} 
