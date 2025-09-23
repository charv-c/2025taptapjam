using UnityEngine;

/// <summary>
/// 唯一标识符组件 - 为GameObject提供稳定的GUID标识符
/// 用于替换脆弱的基于路径的对象引用，提升存档系统的健壮性
/// </summary>
public class UniqueID : MonoBehaviour
{
    [Header("唯一标识符")]
    [SerializeField] private string uniqueId = "";
    
    [Header("调试信息")]
    [SerializeField] private bool enableDebugLog = false;
    
    /// <summary>
    /// 获取唯一标识符
    /// </summary>
    public string ID => uniqueId;
    
    /// <summary>
    /// 检查ID是否有效
    /// </summary>
    public bool HasValidID => !string.IsNullOrEmpty(uniqueId);
    
    private void Awake()
    {
        // 确保ID存在
        EnsureUniqueID();
        
        // 注册到全局管理器
        UniqueIDManager.RegisterObject(this);
    }
    
    private void OnDestroy()
    {
        // 从全局管理器注销
        UniqueIDManager.UnregisterObject(this);
    }
    
    /// <summary>
    /// 确保对象有唯一ID
    /// </summary>
    private void EnsureUniqueID()
    {
        if (string.IsNullOrEmpty(uniqueId))
        {
            GenerateNewID();
        }
        else
        {
            // 检查ID格式是否正确
            if (!System.Guid.TryParse(uniqueId, out _))
            {
                LogWarning($"无效的GUID格式: {uniqueId}，重新生成");
                GenerateNewID();
            }
        }
    }
    
    /// <summary>
    /// 生成新的唯一ID
    /// </summary>
    [ContextMenu("生成新ID")]
    public void GenerateNewID()
    {
        string oldId = uniqueId;
        uniqueId = System.Guid.NewGuid().ToString();
        
        LogDebug($"为 {gameObject.name} 生成新ID: {uniqueId} (旧ID: {oldId})");
        
        // 标记为脏，确保在编辑器中保存
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.EditorUtility.SetDirty(gameObject);
        }
#endif
    }
    
    /// <summary>
    /// 重置ID（谨慎使用）
    /// </summary>
    [ContextMenu("重置ID")]
    public void ResetID()
    {
        if (Application.isPlaying)
        {
            LogWarning("运行时不允许重置ID");
            return;
        }
        
        GenerateNewID();
    }
    
    /// <summary>
    /// 验证ID的唯一性
    /// </summary>
    public bool ValidateUniqueness()
    {
        return UniqueIDManager.ValidateIDUniqueness(uniqueId, this);
    }
    
    /// <summary>
    /// 获取调试信息
    /// </summary>
    public string GetDebugInfo()
    {
        return $"GameObject: {gameObject.name}, ID: {uniqueId}, Path: {GetGameObjectPath()}";
    }
    
    /// <summary>
    /// 获取GameObject的完整路径（用于调试）
    /// </summary>
    private string GetGameObjectPath()
    {
        string path = gameObject.name;
        Transform parent = transform.parent;
        
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
    
    /// <summary>
    /// 调试日志
    /// </summary>
    private void LogDebug(string message)
    {
        if (enableDebugLog)
        {
            GameLogger.LogDev($"[UniqueID] {message}");
        }
    }
    
    /// <summary>
    /// 警告日志
    /// </summary>
    private void LogWarning(string message)
    {
        GameLogger.LogError($"[UniqueID] {message}");
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// 编辑器验证
    /// </summary>
    private void OnValidate()
    {
        // 只在编辑器中且非运行时执行
        if (!Application.isPlaying)
        {
            EnsureUniqueID();
        }
    }
    
    /// <summary>
    /// 在Inspector中显示ID信息
    /// </summary>
    [ContextMenu("显示ID信息")]
    public void ShowIDInfo()
    {
        Debug.Log($"[UniqueID] {GetDebugInfo()}");
    }
    
    /// <summary>
    /// 复制ID到剪贴板
    /// </summary>
    [ContextMenu("复制ID到剪贴板")]
    public void CopyIDToClipboard()
    {
        UnityEditor.EditorGUIUtility.systemCopyBuffer = uniqueId;
        Debug.Log($"[UniqueID] ID已复制到剪贴板: {uniqueId}");
    }
#endif
}
