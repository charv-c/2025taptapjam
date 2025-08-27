using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MaskController : MonoBehaviour
{
    [Header("遮罩设置")]
    [SerializeField] private PlayerController playerController; // 玩家控制器引用
    
    [Header("遮罩位置设置")]
    [SerializeField] private Vector2 leftMaskPosition = new Vector2(-960, 0); // 左边遮罩位置
    [SerializeField] private Vector2 rightMaskPosition = new Vector2(960, 0); // 右边遮罩位置
    
    private RectTransform maskRectTransform;
    private bool isInitialized = false;
    
    void Start()
    {
        InitializeMask();
    }
    
    void Update()
    {
        if (isInitialized)
        {
            UpdateMaskPosition();
        }
    }
    
    /// <summary>
    /// 初始化遮罩
    /// </summary>
    private void InitializeMask()
    {
        // 获取当前GameObject的RectTransform组件
        maskRectTransform = GetComponent<RectTransform>();
        if (maskRectTransform == null)
        {
            Debug.LogError("MaskController: 当前GameObject没有RectTransform组件");
            return;
        }
        
        // 确保遮罩有Image组件
        Image maskImage = GetComponent<Image>();
        if (maskImage == null)
        {
            maskImage = gameObject.AddComponent<Image>();
            maskImage.color = new Color(0, 0, 0, 0.5f); // 半透明黑色
        }
        
        // 如果没有手动设置playerController，自动查找
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }
        
        isInitialized = true;
        Debug.Log("MaskController: 遮罩初始化完成");
        
        if (playerController == null)
        {
            Debug.LogError("MaskController: 未找到PlayerController");
        }
    }
    
    /// <summary>
    /// 根据当前控制的玩家编号更新遮罩位置
    /// </summary>
    private void UpdateMaskPosition()
    {
        // 只在level1场景中工作
        if (SceneManager.GetActiveScene().name != "level1")
        {
            return;
        }
        
        if (playerController == null || maskRectTransform == null)
        {
            return;
        }
        
        // 获取当前控制的玩家编号
        int currentPlayerIndex = playerController.GetCurrentPlayerIndex();
        
        // 根据当前控制的玩家编号决定遮罩位置
        Vector2 targetMaskPosition;
        
        if (currentPlayerIndex == 0)
        {
            // Player1对应遮罩在左边
            targetMaskPosition = leftMaskPosition;
        }
        else
        {
            // Player2对应遮罩在右边
            targetMaskPosition = rightMaskPosition;
        }
        
        // 只有当位置发生变化时才更新，避免不必要的更新
        if (maskRectTransform.anchoredPosition != targetMaskPosition)
        {
            maskRectTransform.anchoredPosition = targetMaskPosition;
            Debug.Log($"MaskController: 当前控制玩家 {currentPlayerIndex + 1}, 遮罩移动到 {(currentPlayerIndex == 0 ? "左边" : "右边")}");
        }
    }
    
    /// <summary>
    /// 手动设置遮罩位置
    /// </summary>
    /// <param name="position">目标位置</param>
    public void SetMaskPosition(Vector2 position)
    {
        if (maskRectTransform != null)
        {
            maskRectTransform.anchoredPosition = position;
        }
    }
    
    /// <summary>
    /// 设置遮罩为左边位置（遮住右边屏幕）
    /// </summary>
    public void SetMaskToLeft()
    {
        SetMaskPosition(rightMaskPosition);
    }
    
    /// <summary>
    /// 设置遮罩为右边位置（遮住左边屏幕）
    /// </summary>
    public void SetMaskToRight()
    {
        SetMaskPosition(leftMaskPosition);
    }
    
    /// <summary>
    /// 启用遮罩
    /// </summary>
    public void EnableMask()
    {
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// 禁用遮罩
    /// </summary>
    public void DisableMask()
    {
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 设置遮罩透明度
    /// </summary>
    /// <param name="alpha">透明度值（0-1）</param>
    public void SetMaskAlpha(float alpha)
    {
        Image maskImage = GetComponent<Image>();
        if (maskImage != null)
        {
            Color color = maskImage.color;
            color.a = Mathf.Clamp01(alpha);
            maskImage.color = color;
        }
    }
    
    /// <summary>
    /// 获取当前遮罩位置
    /// </summary>
    /// <returns>当前遮罩位置</returns>
    public Vector2 GetCurrentMaskPosition()
    {
        if (maskRectTransform != null)
        {
            return maskRectTransform.anchoredPosition;
        }
        return Vector2.zero;
    }
    
    /// <summary>
    /// 检查遮罩是否启用
    /// </summary>
    /// <returns>遮罩是否启用</returns>
    public bool IsMaskEnabled()
    {
        return gameObject.activeSelf;
    }
    

}
