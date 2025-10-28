using UnityEngine;
using System.Collections;


/// <summary>
/// 商特殊逻辑组件
/// 处理商对象的特殊移动逻辑和sprite切换
/// </summary>
public class ShangSpecialLogic : MonoBehaviour
{
    [Header("商特殊设置")]
    [SerializeField] private bool enableLogging = true; // 是否启用日志
    [SerializeField] private Sprite moveSprite; // 移动时使用的sprite
    [SerializeField] private Sprite lightMoveSprite; // Light2D移动时使用的sprite
    
    [Header("移动设置")]
    [SerializeField] private string targetObjectName = "商-2"; // 目标对象名称
    [SerializeField] private string showObjectName = "汉字"; // 移动后要显示的对象名称
    [SerializeField] private float moveSpeed = 5f; // 移动速度
    [SerializeField] private float moveDuration = 1f; // 移动持续时间
    
    // 组件引用
    private SpriteRenderer spriteRenderer;
    private Sprite originalSprite; // 存储原始sprite
    private UnityEngine.Rendering.Universal.Light2D childLight2D; // 子物体的Light2D组件
    private Sprite originalLightSprite; // 存储Light2D的原始sprite
    private bool isMoving = false; // 是否正在移动
    
    void Start()
    {
        // 获取SpriteRenderer组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // 保存原始sprite
            originalSprite = spriteRenderer.sprite;
            
            if (enableLogging)
            {
                GameLogger.LogDev($"ShangSpecialLogic: 初始化完成，原始sprite已保存 - {gameObject.name}");
            }
        }
        else
        {
            GameLogger.LogWarning($"ShangSpecialLogic: 未找到SpriteRenderer组件 - {gameObject.name}");
        }
        
        // 查找子物体的Light2D组件
        childLight2D = GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>();
        if (childLight2D != null)
        {
            // 保存Light2D的原始sprite
            originalLightSprite = childLight2D.lightCookieSprite;
            
            if (enableLogging)
            {
                GameLogger.LogDev($"ShangSpecialLogic: 找到子物体Light2D组件，原始sprite已保存 - {gameObject.name}");
            }
        }
        else
        {
            if (enableLogging)
            {
                GameLogger.LogDev($"ShangSpecialLogic: 未找到子物体Light2D组件 - {gameObject.name}");
            }
        }
    }
    
    /// <summary>
    /// 当收到"帛"广播时调用
    /// </summary>
    public void OnBoBroadcast()
    {
        if (enableLogging)
        {
            GameLogger.LogDev($"ShangSpecialLogic: 收到'帛'广播，开始执行商特殊逻辑 - {gameObject.name}");
        }
        
        // 开始移动流程
        StartMoveSequence();
    }
    
    /// <summary>
    /// 开始移动序列
    /// </summary>
    private void StartMoveSequence()
    {
        if (isMoving)
        {
            if (enableLogging)
            {
                GameLogger.LogWarning($"ShangSpecialLogic: 已经在移动中，忽略重复调用 - {gameObject.name}");
            }
            return;
        }
        
        isMoving = true;
        
        // 切换到移动sprite
        SwitchToMoveSprite();
        
        // 执行移动
        StartCoroutine(MoveToTargetCoroutine());
        
        if (enableLogging)
        {
            GameLogger.LogDev($"ShangSpecialLogic: 商移动序列开始 - {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 切换到移动sprite
    /// </summary>
    private void SwitchToMoveSprite()
    {
        // 切换主物体的sprite
        if (spriteRenderer != null && moveSprite != null)
        {
            spriteRenderer.sprite = moveSprite;
            
            if (enableLogging)
            {
                GameLogger.LogDev($"ShangSpecialLogic: 已切换到移动sprite - {gameObject.name}");
            }
        }
        else if (moveSprite == null)
        {
            GameLogger.LogWarning($"ShangSpecialLogic: 移动sprite未设置 - {gameObject.name}");
        }
        
        // 切换子物体Light2D的sprite
        if (childLight2D != null && lightMoveSprite != null)
        {
            childLight2D.lightCookieSprite = lightMoveSprite;
            
            if (enableLogging)
            {
                GameLogger.LogDev($"ShangSpecialLogic: 已切换子物体Light2D的移动sprite - {gameObject.name}");
            }
        }
        else if (childLight2D != null && lightMoveSprite == null)
        {
            if (enableLogging)
            {
                GameLogger.LogDev($"ShangSpecialLogic: Light2D移动sprite未设置，保持原始sprite - {gameObject.name}");
            }
        }
    }
    
    /// <summary>
    /// 平滑移动到目标位置的协程
    /// </summary>
    private IEnumerator MoveToTargetCoroutine()
    {
        // 在场景中查找目标对象
        GameObject targetObject = FindObjectByName(targetObjectName);
        
        if (targetObject == null)
        {
            GameLogger.LogWarning($"ShangSpecialLogic: 未找到名为 '{targetObjectName}' 的目标对象 - {gameObject.name}");
            isMoving = false;
            yield break;
        }
        
        Vector3 targetPosition = targetObject.transform.position;
        Vector3 startPosition = transform.position;
        
        if (enableLogging)
        {
            GameLogger.LogDev($"ShangSpecialLogic: 商对象 '{gameObject.name}' 开始从位置 {startPosition} 平滑移动到 {targetPosition}");
        }
        
        float elapsedTime = 0f;
        
        // 平滑移动
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / moveDuration;
            
            // 使用Lerp进行平滑插值
            transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            
            yield return null;
        }
        
        // 确保最终位置准确
        transform.position = targetPosition;
        
        if (enableLogging)
        {
            GameLogger.LogDev($"ShangSpecialLogic: 商对象 '{gameObject.name}' 平滑移动完成，当前位置: {transform.position}");
        }
        
        // 移动完成后显示目标对象
        ShowTargetObject();
        
        // 移动完成，重置状态
        isMoving = false;
    }
    
    /// <summary>
    /// 显示目标对象
    /// </summary>
    private void ShowTargetObject()
    {
        // 在场景中查找目标对象（包括未激活的对象）
        GameObject targetObject = FindObjectByName(showObjectName);
        
        if (targetObject == null)
        {
            GameLogger.LogWarning($"ShangSpecialLogic: 未找到名为 '{showObjectName}' 的目标对象 - {gameObject.name}");
            return;
        }
        
        if (enableLogging)
        {
            GameLogger.LogDev($"ShangSpecialLogic: 找到目标对象 '{showObjectName}'，当前状态: activeInHierarchy={targetObject.activeInHierarchy}");
        }
        
        // 直接激活GameObject
        if (!targetObject.activeInHierarchy)
        {
            if (enableLogging)
            {
                GameLogger.LogDev($"ShangSpecialLogic: 激活目标对象 '{showObjectName}'");
            }
            targetObject.SetActive(true);
        }
        else
        {
            if (enableLogging)
            {
                GameLogger.LogDev($"ShangSpecialLogic: 目标对象 '{showObjectName}' 已经激活");
            }
        }
    }
    
    /// <summary>
    /// 根据名称查找对象（包括未激活的对象）
    /// </summary>
    /// <param name="objectName">对象名称</param>
    /// <returns>找到的对象，如果未找到返回null</returns>
    private GameObject FindObjectByName(string objectName)
    {
        // 首先尝试使用GameObject.Find（只能找到激活的对象）
        GameObject targetObject = GameObject.Find(objectName);
        if (targetObject != null)
        {
            return targetObject;
        }
        
        // 如果GameObject.Find找不到，遍历所有对象（包括未激活的）
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == objectName && obj.scene.isLoaded)
            {
                if (enableLogging)
                {
                    GameLogger.LogDev($"ShangSpecialLogic: 在未激活对象中找到 '{objectName}': {obj.name}");
                }
                return obj;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 恢复原始sprite
    /// </summary>
    public void RestoreOriginalSprite()
    {
        // 恢复主物体的原始sprite
        if (spriteRenderer != null && originalSprite != null)
        {
            spriteRenderer.sprite = originalSprite;
            
            if (enableLogging)
            {
                GameLogger.LogDev($"ShangSpecialLogic: 已恢复原始sprite - {gameObject.name}");
            }
        }
        
        // 恢复子物体Light2D的原始sprite
        if (childLight2D != null && originalLightSprite != null)
        {
            childLight2D.lightCookieSprite = originalLightSprite;
            
            if (enableLogging)
            {
                GameLogger.LogDev($"ShangSpecialLogic: 已恢复子物体Light2D的原始sprite - {gameObject.name}");
            }
        }
    }
    
    /// <summary>
    /// 设置移动sprite
    /// </summary>
    /// <param name="sprite">要设置的sprite</param>
    public void SetMoveSprite(Sprite sprite)
    {
        moveSprite = sprite;
        
        if (enableLogging)
        {
            GameLogger.LogDev($"ShangSpecialLogic: 已设置移动sprite - {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 设置目标对象名称
    /// </summary>
    /// <param name="targetName">目标对象名称</param>
    public void SetTargetObjectName(string targetName)
    {
        targetObjectName = targetName;
        
        if (enableLogging)
        {
            GameLogger.LogDev($"ShangSpecialLogic: 已设置目标对象名称为 '{targetName}' - {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 设置要显示的对象名称
    /// </summary>
    /// <param name="showName">要显示的对象名称</param>
    public void SetShowObjectName(string showName)
    {
        showObjectName = showName;
        
        if (enableLogging)
        {
            GameLogger.LogDev($"ShangSpecialLogic: 已设置显示对象名称为 '{showName}' - {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 获取是否正在移动
    /// </summary>
    /// <returns>是否正在移动</returns>
    public bool IsMoving()
    {
        return isMoving;
    }
    
    /// <summary>
    /// 重置移动状态
    /// </summary>
    public void ResetMoveState()
    {
        isMoving = false;
        
        if (enableLogging)
        {
            GameLogger.LogDev($"ShangSpecialLogic: 已重置移动状态 - {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 设置移动速度
    /// </summary>
    /// <param name="speed">移动速度</param>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
        
        if (enableLogging)
        {
            GameLogger.LogDev($"ShangSpecialLogic: 已设置移动速度为 {speed} - {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 设置移动持续时间
    /// </summary>
    /// <param name="duration">移动持续时间</param>
    public void SetMoveDuration(float duration)
    {
        moveDuration = duration;
        
        if (enableLogging)
        {
            GameLogger.LogDev($"ShangSpecialLogic: 已设置移动持续时间为 {duration} - {gameObject.name}");
        }
    }
    
    /// <summary>
    /// 设置Light2D移动sprite
    /// </summary>
    /// <param name="sprite">要设置的Light2D移动sprite</param>
    public void SetLightMoveSprite(Sprite sprite)
    {
        lightMoveSprite = sprite;
        
        if (enableLogging)
        {
            GameLogger.LogDev($"ShangSpecialLogic: 已设置Light2D移动sprite - {gameObject.name}");
        }
    }
}

