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
    private bool enterKeyEnabled = true; // 控制回车键是否启用，默认启用
    public string CarryCharacter="人";
    
    // 颜色控制相关
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isGrayedOut = false;
    
    [Header("颜色设置")]
    [SerializeField] private Color grayedOutColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    
    void Start()
    {
        // 获取SpriteRenderer组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
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
        
        // 检测回车键按下（只有在启用时才响应）
        if (enterKeyEnabled && Input.GetKeyDown(KeyCode.Return))
        {
            OnEnterKeyPressed();
        }
    }

    

    void HandleMovement()
    {
        // 只有在输入启用时才处理移动
        if (inputEnabled)
        {
            // 使用SetInput设置的值，而不是直接获取输入
            // 这样PlayerController可以控制输入
            float horizontalInput = currentHorizontalInput;
            float verticalInput = currentVerticalInput;

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
                clampedX = Mathf.Clamp(playerPosition.x, -8.9f, -0.8f);
            }
            else
            {
                // 玩家2只能在右半边移动（X轴限制在0到8.9）
                clampedX = Mathf.Clamp(playerPosition.x, 0.8f, 8.9f);
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
            SetCarryCharacter("人");
        }
        
        RestoreAllHighlightScripts();
    }
    
    // 回车键按下时的处理
    private void OnEnterKeyPressed()
    {
        // 检查当前玩家是否为当前控制角色
        if (!IsCurrentControlledPlayer())
        {
            Debug.Log($"Player: 当前玩家不是控制角色，忽略回车键输入");
            return;
        }
        
        Debug.Log($"Player: 当前玩家是控制角色，执行回车键交互逻辑");
        // 查找附近的Highlight对象并触发交互
        TriggerNearbyHighlightInteraction();
    }
    
    // 检查当前玩家是否为当前控制角色
    private bool IsCurrentControlledPlayer()
    {
        // 获取PlayerController实例
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning("Player: 未找到PlayerController，默认允许交互");
            return true;
        }
        
        // 获取当前控制角色
        Player currentControlledPlayer = playerController.GetCurrentPlayer();
        if (currentControlledPlayer == null)
        {
            Debug.LogWarning("Player: 当前控制角色为空，默认允许交互");
            return true;
        }
        
        // 检查当前玩家是否为当前控制角色
        bool isCurrentControlled = (currentControlledPlayer == this);
        
        if (isCurrentControlled)
        {
            Debug.Log($"Player: 当前玩家是控制角色，允许执行交互");
        }
        else
        {
            Debug.Log($"Player: 当前玩家不是控制角色，禁止执行交互");
        }
        
        return isCurrentControlled;
    }
    
    // 触发附近的Highlight对象交互
    private void TriggerNearbyHighlightInteraction()
    {
        // 查找场景中所有Highlight对象
        Highlight[] allHighlights = FindObjectsOfType<Highlight>();
        Highlight targetHighlight = null;
        
        // 优先级排序：小孩 > 其他对象 > 门对象（当小孩未隐藏时）
        foreach (Highlight highlight in allHighlights)
        {
            if (highlight != null && highlight.enabled)
            {
                // 检查是否与当前玩家有碰撞（通过碰撞箱判断）
                if (IsPlayerCollidingWithHighlight(highlight))
                {
                    // 特殊处理：如果检测到小孩对象，优先选择小孩
                    if (highlight.letter == "孩")
                    {
                        targetHighlight = highlight;
                        Debug.Log($"Player: 优先选择小孩对象进行交互: '{highlight.gameObject.name}'");
                        break; // 小孩优先级最高，直接跳出
                    }
                    // 如果检测到门对象，需要检查孩子是否隐藏
                    else if (highlight.letter == "门")
                    {
                        // 只有在没有找到其他可交互对象且孩子已隐藏时才选择门
                        if (targetHighlight == null && IsChildHidden())
                        {
                            targetHighlight = highlight;
                            Debug.Log($"Player: 小孩已隐藏，可选择门对象: '{highlight.gameObject.name}'");
                        }
                        else
                        {
                            Debug.Log($"Player: 门对象被跳过，小孩可能未隐藏或已有其他交互对象");
                        }
                    }
                    // 其他对象正常处理
                    else if (targetHighlight == null)
                    {
                        targetHighlight = highlight;
                        Debug.Log($"Player: 选择对象进行交互: '{highlight.gameObject.name}'");
                    }
                }
            }
        }
        
        // 执行交互
        if (targetHighlight != null)
        {
            targetHighlight.TriggerInteraction();
            Debug.Log($"Player: 最终触发与Highlight对象 '{targetHighlight.gameObject.name}' 的交互");
        }
    }
    
    // 检查孩子是否隐藏（复用Highlight中的逻辑）
    private bool IsChildHidden()
    {
        // 查找场景中所有带有Highlight脚本的对象
        Highlight[] allHighlights = FindObjectsOfType<Highlight>();
        
        foreach (Highlight highlight in allHighlights)
        {
            if (highlight != null && highlight.letter == "孩")
            {
                // 检查孩子的SpriteRenderer是否被禁用
                SpriteRenderer childSpriteRenderer = highlight.GetComponent<SpriteRenderer>();
                if (childSpriteRenderer != null && !childSpriteRenderer.enabled)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        
        // 如果没有找到孩子对象，默认认为已隐藏
        return true;
    }
    
    // 检查玩家是否与Highlight对象碰撞
    private bool IsPlayerCollidingWithHighlight(Highlight highlight)
    {
        // 获取玩家的碰撞箱
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null)
        {
            Debug.LogWarning("Player: 玩家没有Collider2D组件");
            return false;
        }
        
        // 获取Highlight对象的碰撞箱
        Collider2D highlightCollider = highlight.GetComponent<Collider2D>();
        if (highlightCollider == null)
        {
            Debug.LogWarning($"Player: Highlight对象 '{highlight.gameObject.name}' 没有Collider2D组件");
            return false;
        }
        
        // 检查两个碰撞箱是否重叠
        bool isColliding = playerCollider.IsTouching(highlightCollider);
        
        if (isColliding)
        {
            Debug.Log($"Player: 检测到与Highlight对象 '{highlight.gameObject.name}' 的碰撞");
        }
        
        return isColliding;
    }
    
    // 设置携带字符并更新米字格图片
    public void SetCarryCharacter(string newCharacter)
    {
        string oldCharacter = CarryCharacter;
        Debug.Log($"Player.SetCarryCharacter: 开始设置携带字符，从 '{oldCharacter}' 更改为 '{newCharacter}'");
        
        CarryCharacter = newCharacter;
        
        // 更新对应的米字格图片
        UpdateMiSquareForCarryCharacter(newCharacter);
        
        Debug.Log($"Player.SetCarryCharacter: 携带字符设置完成，当前携带字符为 '{CarryCharacter}'");
    }
    
    // 更新米字格图片
    private void UpdateMiSquareForCarryCharacter(string character)
    {
        Debug.Log($"Player.UpdateMiSquareForCarryCharacter: 开始更新米字格图片，字符='{character}'，isPlayer1={isPlayer1}");
        
        // 根据玩家类型确定对应的米字格
        string targetMiSquareName = isPlayer1 ? "MiSquare1" : "MiSquare2";
        Debug.Log($"Player.UpdateMiSquareForCarryCharacter: 查找米字格对象 '{targetMiSquareName}'");
        
        GameObject targetMiSquare = GameObject.Find(targetMiSquareName);
        
        if (targetMiSquare != null)
        {
            Debug.Log($"Player.UpdateMiSquareForCarryCharacter: 找到米字格对象 '{targetMiSquareName}'");
            MiSquareController miSquareController = targetMiSquare.GetComponent<MiSquareController>();
            if (miSquareController != null)
            {
                miSquareController.SetMiSquareSprite(character);
                Debug.Log($"Player.UpdateMiSquareForCarryCharacter: 已更新米字格 '{targetMiSquareName}' 为字符 '{character}'");
            }
            else
            {
                Debug.LogWarning($"Player.UpdateMiSquareForCarryCharacter: 米字格 '{targetMiSquareName}' 没有MiSquareController组件");
            }
        }
        else
        {
            Debug.LogWarning($"Player.UpdateMiSquareForCarryCharacter: 未找到米字格对象 '{targetMiSquareName}'");
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
        
        Debug.Log("Player: 已重置所有米字格为'人'字符");
    }
    
    // 获取当前携带的字符
    public string GetCarryCharacter()
    {
        return CarryCharacter;
    }
    
    // 检查当前携带的字符是否为指定字符
    public bool IsCarryingCharacter(string character)
    {
        return CarryCharacter == character;
    }
    
    // 启用/禁用回车键响应
    public void SetEnterKeyEnabled(bool enabled)
    {
        enterKeyEnabled = enabled;
        Debug.Log($"Player: 回车键响应已{(enabled ? "启用" : "禁用")}");
    }
    
    // 获取回车键响应状态
    public bool IsEnterKeyEnabled()
    {
        return enterKeyEnabled;
    }
    
    /// <summary>
    /// 将玩家设置为灰色（未操控状态）
    /// </summary>
    public void SetGrayedOut()
    {
        if (spriteRenderer != null && !isGrayedOut)
        {
            // 保存原始颜色（如果还没有保存）
            if (originalColor == Color.clear)
            {
                originalColor = spriteRenderer.color;
            }
            
            // 设置为灰色（可在 Inspector 中配置）
            spriteRenderer.color = grayedOutColor;
            isGrayedOut = true;
            
            Debug.Log($"Player: 已将玩家设置为灰色状态");
        }
    }
    
    /// <summary>
    /// 恢复正常颜色（操控状态）
    /// </summary>
    public void RestoreNormalColor()
    {
        if (spriteRenderer != null && isGrayedOut)
        {
            // 恢复原始颜色
            spriteRenderer.color = originalColor;
            isGrayedOut = false;
            
            Debug.Log($"Player: 已恢复玩家正常颜色");
        }
    }
    
    /// <summary>
    /// 检查玩家是否处于灰色状态
    /// </summary>
    /// <returns>是否为灰色状态</returns>
    public bool IsGrayedOut()
    {
        return isGrayedOut;
    }
    
    /// <summary>
    /// 获取原始颜色
    /// </summary>
    /// <returns>原始颜色</returns>
    public Color GetOriginalColor()
    {
        return originalColor;
    }
    
    /// <summary>
    /// 设置自定义颜色
    /// </summary>
    /// <param name="color">要设置的颜色</param>
    public void SetCustomColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
            Debug.Log($"Player: 已设置自定义颜色: {color}");
        }
    }
} 
