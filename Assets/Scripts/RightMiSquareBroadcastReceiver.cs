using UnityEngine;

/// <summary>
/// 右边米字格广播接收器
/// 专门处理字符合成时右边米字格的sprite更新
/// </summary>
public class RightMiSquareBroadcastReceiver : MonoBehaviour
{
    [Header("右边米字格设置")]
    [SerializeField] private MiSquareController rightMiSquareController;
    [SerializeField] private bool enableLogging = true;
    
    void Start()
    {
        // 如果没有手动设置，尝试自动获取
        if (rightMiSquareController == null)
        {
            rightMiSquareController = GetComponent<MiSquareController>();
        }
        
        // 确保米字格类型设置为右米字格
        if (rightMiSquareController != null)
        {
            rightMiSquareController.SetMiZiGeType(MiSquareController.MiZiGeType.Right);
            if (enableLogging)
            {
                Debug.Log($"RightMiSquareBroadcastReceiver: 已设置米字格类型为右米字格");
            }
        }
        else
        {
            Debug.LogError($"RightMiSquareBroadcastReceiver: 对象 '{gameObject.name}' 没有MiSquareController组件");
        }
    }
    
    /// <summary>
    /// 接收广播的方法（必须与广播调用方法名一致）
    /// </summary>
    /// <param name="broadcastedValue">广播的值</param>
    public void ReceiveBroadcast(string broadcastedValue)
    {
        if (enableLogging)
        {
            Debug.Log($"RightMiSquareBroadcastReceiver: 接收到广播: {broadcastedValue}");
        }
        
        // 处理combine_success广播
        if (broadcastedValue == "combine_success")
        {
            HandleCombineSuccess();
        }
    }
    
    /// <summary>
    /// 处理字符合成成功的广播
    /// </summary>
    private void HandleCombineSuccess()
    {
        if (enableLogging)
        {
            Debug.Log("RightMiSquareBroadcastReceiver: 处理字符合成成功广播");
        }
        
        if (rightMiSquareController == null)
        {
            Debug.LogError("RightMiSquareBroadcastReceiver: 右边米字格控制器为空");
            return;
        }
        
        // 获取当前玩家携带的字符
        string currentCharacter = GetCurrentPlayerCarryCharacter();
        if (enableLogging)
        {
            Debug.Log($"RightMiSquareBroadcastReceiver: 当前玩家携带字符: '{currentCharacter}'");
        }
        
        if (!string.IsNullOrEmpty(currentCharacter))
        {
            // 检查是否有对应类型的米字格sprite
            bool hasMiZiGeSprite = rightMiSquareController.HasMiZiGeSprite(currentCharacter);
            if (enableLogging)
            {
                Debug.Log($"RightMiSquareBroadcastReceiver: 字符 '{currentCharacter}' 是否有右米字格sprite: {hasMiZiGeSprite}");
            }
            
            if (hasMiZiGeSprite)
            {
                // 使用右米字格sprite
                rightMiSquareController.SetMiSquareSprite(currentCharacter);
                if (enableLogging)
                {
                    Debug.Log($"RightMiSquareBroadcastReceiver: 已设置右米字格为字符 '{currentCharacter}'，使用右米字格sprite");
                }
            }
            else
            {
                // 如果没有右米字格sprite，使用普通sprite
                rightMiSquareController.SetNormalSprite(currentCharacter);
                if (enableLogging)
                {
                    Debug.Log($"RightMiSquareBroadcastReceiver: 字符 '{currentCharacter}' 没有右米字格sprite，使用普通sprite");
                }
            }
        }
        else
        {
            if (enableLogging)
            {
                Debug.LogWarning("RightMiSquareBroadcastReceiver: 当前玩家没有携带字符");
            }
        }
    }
    
    /// <summary>
    /// 获取当前玩家携带的字符
    /// </summary>
    /// <returns>玩家携带的字符，如果没有则返回空字符串</returns>
    private string GetCurrentPlayerCarryCharacter()
    {
        // 查找场景中的PlayerController
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            Player currentPlayer = playerController.GetCurrentPlayer();
            if (currentPlayer != null)
            {
                return currentPlayer.CarryCharacter;
            }
        }
        
        // 如果找不到PlayerController，尝试直接查找Player
        Player[] players = FindObjectsOfType<Player>();
        foreach (Player player in players)
        {
            if (player != null && !string.IsNullOrEmpty(player.CarryCharacter))
            {
                return player.CarryCharacter;
            }
        }
        
        return "";
    }
    
    /// <summary>
    /// 设置右边米字格控制器引用
    /// </summary>
    /// <param name="controller">米字格控制器</param>
    public void SetRightMiSquareController(MiSquareController controller)
    {
        rightMiSquareController = controller;
        if (enableLogging)
        {
            Debug.Log($"RightMiSquareBroadcastReceiver: 已设置右边米字格控制器: {controller?.gameObject.name ?? "null"}");
        }
    }
    
    /// <summary>
    /// 获取右边米字格控制器引用
    /// </summary>
    /// <returns>米字格控制器</returns>
    public MiSquareController GetRightMiSquareController()
    {
        return rightMiSquareController;
    }
    
    /// <summary>
    /// 手动更新右边米字格（用于测试）
    /// </summary>
    /// <param name="character">要显示的字符</param>
    [ContextMenu("手动更新右边米字格")]
    public void ManualUpdateRightMiSquare(string character = "人")
    {
        if (rightMiSquareController != null)
        {
            bool hasMiZiGeSprite = rightMiSquareController.HasMiZiGeSprite(character);
            if (hasMiZiGeSprite)
            {
                rightMiSquareController.SetMiSquareSprite(character);
                Debug.Log($"RightMiSquareBroadcastReceiver: 手动设置右米字格为字符 '{character}'，使用右米字格sprite");
            }
            else
            {
                rightMiSquareController.SetNormalSprite(character);
                Debug.Log($"RightMiSquareBroadcastReceiver: 手动设置右米字格为字符 '{character}'，使用普通sprite");
            }
        }
        else
        {
            Debug.LogError("RightMiSquareBroadcastReceiver: 右边米字格控制器为空，无法手动更新");
        }
    }
}
