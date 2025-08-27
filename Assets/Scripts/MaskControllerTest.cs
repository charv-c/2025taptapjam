using UnityEngine;

/// <summary>
/// MaskController功能测试脚本
/// 用于验证动态遮罩功能是否正常工作
/// </summary>
public class MaskControllerTest : MonoBehaviour
{
    [Header("测试设置")]
    [SerializeField] private MaskController maskController;
    [SerializeField] private PlayerController playerController;
    
    [Header("测试按键")]
    [SerializeField] private KeyCode testLeftKey = KeyCode.L;
    [SerializeField] private KeyCode testRightKey = KeyCode.R;
    [SerializeField] private KeyCode testAlphaKey = KeyCode.A;
    
    private void Start()
    {
        // 自动查找组件
        if (maskController == null)
        {
            maskController = FindObjectOfType<MaskController>();
        }
        
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }
        
        Debug.Log("MaskControllerTest: 测试脚本已初始化");
        LogCurrentStatus();
    }
    
    private void Update()
    {
        // 测试设置遮罩到左边
        if (Input.GetKeyDown(testLeftKey))
        {
            TestSetMaskToLeft();
        }
        
        // 测试设置遮罩到右边
        if (Input.GetKeyDown(testRightKey))
        {
            TestSetMaskToRight();
        }
        
        // 测试设置遮罩透明度
        if (Input.GetKeyDown(testAlphaKey))
        {
            TestSetMaskAlpha();
        }
    }
    

    
    /// <summary>
    /// 测试设置遮罩到左边
    /// </summary>
    private void TestSetMaskToLeft()
    {
        if (maskController != null)
        {
            maskController.SetMaskToLeft();
            Debug.Log("MaskControllerTest: 测试设置遮罩到左边");
            LogCurrentStatus();
        }
        else
        {
            Debug.LogWarning("MaskControllerTest: maskController为空，无法测试");
        }
    }
    
    /// <summary>
    /// 测试设置遮罩到右边
    /// </summary>
    private void TestSetMaskToRight()
    {
        if (maskController != null)
        {
            maskController.SetMaskToRight();
            Debug.Log("MaskControllerTest: 测试设置遮罩到右边");
            LogCurrentStatus();
        }
        else
        {
            Debug.LogWarning("MaskControllerTest: maskController为空，无法测试");
        }
    }
    
    /// <summary>
    /// 测试设置遮罩透明度
    /// </summary>
    private void TestSetMaskAlpha()
    {
        if (maskController != null)
        {
            float randomAlpha = Random.Range(0.1f, 0.9f);
            maskController.SetMaskAlpha(randomAlpha);
            Debug.Log($"MaskControllerTest: 测试设置遮罩透明度为 {randomAlpha:F2}");
            LogCurrentStatus();
        }
        else
        {
            Debug.LogWarning("MaskControllerTest: maskController为空，无法测试");
        }
    }
    
    /// <summary>
    /// 记录当前状态
    /// </summary>
    private void LogCurrentStatus()
    {
        if (maskController != null)
        {
            Debug.Log($"MaskControllerTest: 当前状态 - " +
                     $"遮罩启用: {(maskController.IsMaskEnabled() ? "是" : "否")}, " +
                     $"遮罩位置: {maskController.GetCurrentMaskPosition()}");
        }
        
        if (playerController != null)
        {
            Player currentPlayer = playerController.GetCurrentPlayer();
            if (currentPlayer != null)
            {
                Debug.Log($"MaskControllerTest: 玩家状态 - " +
                         $"位置: {currentPlayer.transform.position}, " +
                         $"输入启用: {(currentPlayer.IsInputEnabled() ? "是" : "否")}");
            }
        }
    }
    
    /// <summary>
    /// 在Inspector中显示测试信息
    /// </summary>
    private void OnGUI()
    {
        if (Application.isPlaying)
        {
                    GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("MaskController 测试控制");
        GUILayout.Label($"按 {testLeftKey} 设置遮罩到左边");
        GUILayout.Label($"按 {testRightKey} 设置遮罩到右边");
        GUILayout.Label($"按 {testAlphaKey} 随机设置透明度");
        
        if (maskController != null)
        {
            GUILayout.Label($"遮罩启用: {(maskController.IsMaskEnabled() ? "是" : "否")}");
            GUILayout.Label($"遮罩位置: {maskController.GetCurrentMaskPosition()}");
        }
            
            if (playerController != null)
            {
                Player currentPlayer = playerController.GetCurrentPlayer();
                if (currentPlayer != null)
                {
                    GUILayout.Label($"玩家位置: {currentPlayer.transform.position}");
                    GUILayout.Label($"输入启用: {(currentPlayer.IsInputEnabled() ? "是" : "否")}");
                }
            }
            
            GUILayout.EndArea();
        }
    }
}
