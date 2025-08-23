using UnityEngine;

// 自动创建广播管理器的脚本
public class BroadcastManagerAutoCreator : MonoBehaviour
{
    [Header("自动创建设置")]
    [SerializeField] private bool autoCreateOnStart = true;
    [SerializeField] private string managerObjectName = "BroadcastManager";
    
    void Start()
    {
        if (autoCreateOnStart)
        {
            CreateBroadcastManagerIfNeeded();
        }
    }
    
    // 创建广播管理器（如果不存在）
    public void CreateBroadcastManagerIfNeeded()
    {
        if (BroadcastManager.Instance == null)
        {
            // 创建空对象
            GameObject managerObject = new GameObject(managerObjectName);
            
            // 添加BroadcastManager组件
            BroadcastManager manager = managerObject.AddComponent<BroadcastManager>();
            
            Debug.Log($"BroadcastManagerAutoCreator: 已创建广播管理器对象 '{managerObjectName}'");
        }
        else
        {
            Debug.Log("BroadcastManagerAutoCreator: 广播管理器已存在，无需创建");
        }
    }
    
    // 手动创建广播管理器
    [ContextMenu("手动创建广播管理器")]
    public void CreateBroadcastManager()
    {
        CreateBroadcastManagerIfNeeded();
    }
    
    // 检查广播管理器是否存在
    [ContextMenu("检查广播管理器状态")]
    public void CheckBroadcastManagerStatus()
    {
        if (BroadcastManager.Instance != null)
        {
            Debug.Log($"BroadcastManagerAutoCreator: 广播管理器存在，对象名: {BroadcastManager.Instance.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("BroadcastManagerAutoCreator: 广播管理器不存在");
        }
    }
}
