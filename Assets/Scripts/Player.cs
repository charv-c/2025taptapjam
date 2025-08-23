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
            Debug.LogError("未找到主摄像机！");
            return;
        }

        // 计算屏幕边界
        CalculateScreenBounds();
        
        // 设置初始位置
        SetInitialPosition();
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
        else
        {
            // 使用外部设置的输入值
            Vector3 movement = new Vector3(currentHorizontalInput, currentVerticalInput, 0f);
            transform.Translate(movement * moveSpeed * Time.deltaTime);
        }
    }

    void ClampToScreen()
    {
        // 获取玩家在世界坐标中的位置
        Vector3 playerPosition = transform.position;

        // 将世界坐标转换为屏幕坐标
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(playerPosition);

        // 根据玩家类型限制移动范围
        if (isPlayer1)
        {
            // 玩家1只能在屏幕左半边移动
            float leftBoundary = playerWidth / 2 + screenBoundaryOffset;
            float rightBoundary = screenWidth / 2 - playerWidth / 2 - screenBoundaryOffset;
            screenPosition.x = Mathf.Clamp(screenPosition.x, leftBoundary, rightBoundary);
        }
        else
        {
            // 玩家2只能在屏幕右半边移动
            float leftBoundary = screenWidth / 2 + playerWidth / 2 + screenBoundaryOffset;
            float rightBoundary = screenWidth - playerWidth / 2 - screenBoundaryOffset;
            screenPosition.x = Mathf.Clamp(screenPosition.x, leftBoundary, rightBoundary);
        }

        // 将屏幕坐标转换回世界坐标
        Vector3 clampedWorldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        clampedWorldPosition.z = transform.position.z; // 保持原有的Z坐标
        
        // Y轴移动范围限制在-2.6到2.4
        clampedWorldPosition.y = Mathf.Clamp(clampedWorldPosition.y, -2.6f, 2.4f);

        // 应用限制后的位置
        transform.position = clampedWorldPosition;
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
    }
    
    void SetDefaultStartPosition()
    {
        // 根据玩家类型设置默认初始位置
        Vector3 defaultPosition = Vector3.zero;
        
        if (isPlayer1)
        {
            // Player1默认在左半边中央
            defaultPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenWidth * 0.25f, screenHeight * 0.5f, 0f));
        }
        else
        {
            // Player2默认在右半边中央
            defaultPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenWidth * 0.75f, screenHeight * 0.5f, 0f));
        }
        
        defaultPosition.z = transform.position.z; // 保持原有的Z坐标
        
        // 确保初始位置在Y轴限制范围内
        defaultPosition.y = Mathf.Clamp(defaultPosition.y, -2.6f, 2.4f);
        
        transform.position = defaultPosition;
    }

        // 可选：在编辑器中显示边界
    void OnDrawGizmosSelected()
    {
        if (mainCamera != null)
        {
            // 根据玩家类型设置不同的颜色
            Gizmos.color = isPlayer1 ? Color.blue : Color.red;

            // 计算移动边界的世界坐标
            float leftBoundary, rightBoundary;
            
            if (isPlayer1)
            {
                // 玩家1的边界（左半边）
                leftBoundary = playerWidth / 2 + screenBoundaryOffset;
                rightBoundary = screenWidth / 2 - playerWidth / 2 - screenBoundaryOffset;
            }
            else
            {
                // 玩家2的边界（右半边）
                leftBoundary = screenWidth / 2 + playerWidth / 2 + screenBoundaryOffset;
                rightBoundary = screenWidth - playerWidth / 2 - screenBoundaryOffset;
            }

            // 计算四个角的世界坐标
            Vector3 topLeft = mainCamera.ScreenToWorldPoint(new Vector3(leftBoundary,
                                                                       screenHeight - playerHeight / 2 - screenBoundaryOffset,
                                                                       transform.position.z));
            Vector3 topRight = mainCamera.ScreenToWorldPoint(new Vector3(rightBoundary,
                                                                         screenHeight - playerHeight / 2 - screenBoundaryOffset,
                                                                         transform.position.z));
            Vector3 bottomLeft = mainCamera.ScreenToWorldPoint(new Vector3(leftBoundary,
                                                                           playerHeight / 2 + screenBoundaryOffset,
                                                                           transform.position.z));
            Vector3 bottomRight = mainCamera.ScreenToWorldPoint(new Vector3(rightBoundary,
                                                                            playerHeight / 2 + screenBoundaryOffset,
                                                                            transform.position.z));

            // 绘制边界线
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }
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
        return (-2.6f, 2.4f);
    }
    
    // 公共方法：设置Y轴限制范围
    public void SetYAxisLimits(float minY, float maxY)
    {
        // 这里可以添加设置Y轴限制的逻辑
        Debug.Log($"Y轴限制范围设置为: {minY} 到 {maxY}");
    }
    
    // 公共方法：检查当前位置是否在Y轴限制范围内
    public bool IsWithinYAxisLimits()
    {
        float currentY = transform.position.y;
        return currentY >= -2.6f && currentY <= 2.4f;
    }
    
    // 公共方法：强制将位置限制在Y轴范围内
    public void ClampToYAxisLimits()
    {
        Vector3 position = transform.position;
        position.y = Mathf.Clamp(position.y, -2.6f, 2.4f);
        transform.position = position;
    }
    
    // R键按下时的处理
    private void OnRKeyPressed()
    {
        Debug.Log("R键被按下");
        
        // 如果player的carryletter不等于"人"，将其设置为人
        if (CarryCharacter != "人")
        {
            CarryCharacter = "人";
            Debug.Log($"已将CarryCharacter重置为: {CarryCharacter}");
        }
        
        // 恢复场景中所有物体的highlight脚本
        RestoreAllHighlightScripts();
    }
    
    // 恢复场景中所有物体的highlight脚本
    private void RestoreAllHighlightScripts()
    {
        // 查找场景中所有带有Highlight脚本的对象
        Highlight[] allHighlights = FindObjectsOfType<Highlight>();
        
        Debug.Log($"找到 {allHighlights.Length} 个Highlight脚本");
        
        foreach (Highlight highlight in allHighlights)
        {
            if (highlight != null && !highlight.enabled)
            {
                highlight.enabled = true;
                Debug.Log($"已恢复Highlight脚本: {highlight.gameObject.name}");
            }
        }
        
        Debug.Log("所有Highlight脚本恢复完成");
        
        // 将所有米字格设置为默认sprite
        ResetAllMiSquares();
    }
    
    // 将所有米字格设置为默认sprite
    private void ResetAllMiSquares()
    {
        // 查找场景中所有带有MiSquareController脚本的对象
        MiSquareController[] allMiSquares = FindObjectsOfType<MiSquareController>();
        
        Debug.Log($"找到 {allMiSquares.Length} 个米字格");
        
        foreach (MiSquareController miSquare in allMiSquares)
        {
            if (miSquare != null)
            {
                miSquare.ClearSprite(); // 清除sprite，恢复默认状态
                Debug.Log($"已重置米字格: {miSquare.gameObject.name}");
            }
        }
        
        Debug.Log("所有米字格重置完成");
    }
} 
