using UnityEngine;
using System.Collections.Generic;

// 广播管理器：集中管理全屏广播功能
public class BroadcastManager : MonoBehaviour
{
    [Header("广播设置")]
    [SerializeField] private bool enableBroadcastLogging = true;
    [SerializeField] private bool enableDebugMode = true;
    
    // 单例模式
    public static BroadcastManager Instance { get; private set; }
    
    // 广播历史记录
    private List<string> broadcastHistory = new List<string>();
    
    private void Awake()
    {
        // 核心单例逻辑：确保全局唯一性
        if (Instance == null)
        {
            // 如果这是第一个实例，则将其设为单例并标记为跨场景保留
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (enableDebugMode)
            {
                GameLogger.LogDev("BroadcastManager: 单例已初始化并标记为DontDestroyOnLoad。");
            }
        }
        else if (Instance != this)
        {
            // 如果单例已存在，并且不是当前这个实例，则说明当前实例是重复的
            if (enableDebugMode)
            {
                GameLogger.LogWarning($"BroadcastManager: 检测到重复的BroadcastManager实例（在对象'{gameObject.name}'上），此实例将被销毁。");
            }
            // 立即销毁当前重复的实例，以防其他脚本在Awake中错误地引用它
            Destroy(gameObject);
        }
    }
    
    // 全屏广播方法
    public void BroadcastToAll(string broadcastedValue)
    {
        if (enableBroadcastLogging)
        {
            GameLogger.LogDev($"BroadcastManager: 开始全屏广播值: {broadcastedValue}");
        }
        
        // 记录广播历史
        broadcastHistory.Add(broadcastedValue);
        
        // 查找场景中所有的MonoBehaviour对象
        MonoBehaviour[] allObjects = FindObjectsOfType<MonoBehaviour>();
        
        int receiverCount = 0;
        int highlightCount = 0;
        int autoHintCount = 0;
        
        foreach (MonoBehaviour obj in allObjects)
        {
            // 检查对象是否有ReceiveBroadcast方法
            if (obj.GetType().GetMethod("ReceiveBroadcast") != null)
            {
                // 特别检查Highlight组件
                if (obj is Highlight highlight)
                {
                    highlightCount++;
                    GameLogger.LogDev($"找到Highlight组件: {obj.gameObject.name}, letter={highlight.letter}, enabled={obj.enabled}, activeInHierarchy={obj.gameObject.activeInHierarchy}");
                }
                // 特别检查AutoHint组件
                else if (obj is AutoHint autoHint)
                {
                    autoHintCount++;
                    GameLogger.LogDev($"找到AutoHint组件: {obj.gameObject.name}, enabled={obj.enabled}, activeInHierarchy={obj.gameObject.activeInHierarchy}");
                }
                
                // 调用对象的接收广播方法
                obj.SendMessage("ReceiveBroadcast", broadcastedValue, SendMessageOptions.DontRequireReceiver);
                receiverCount++;
            }
        }
        
        if (enableBroadcastLogging)
        {
            GameLogger.LogDev($"BroadcastManager: 广播完成，发送给 {receiverCount} 个对象，其中 {highlightCount} 个Highlight组件，{autoHintCount} 个AutoHint组件");
        }
    }
    
    // 广播给特定类型的对象
    public void BroadcastToType<T>(string broadcastedValue) where T : MonoBehaviour
    {
        T[] objectsOfType = FindObjectsOfType<T>();
        
        foreach (T obj in objectsOfType)
        {
            if (obj.GetType().GetMethod("ReceiveBroadcast") != null)
            {
                obj.SendMessage("ReceiveBroadcast", broadcastedValue, SendMessageOptions.DontRequireReceiver);
            }
        }
        
        if (enableBroadcastLogging)
        {
            GameLogger.LogDev($"BroadcastManager: 向 {objectsOfType.Length} 个 {typeof(T).Name} 对象广播: {broadcastedValue}");
        }
    }
    
    // 广播给指定名称的对象
    public void BroadcastToObject(string objectName, string broadcastedValue)
    {
        GameObject targetObject = GameObject.Find(objectName);
        if (targetObject != null)
        {
            MonoBehaviour[] components = targetObject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in components)
            {
                if (component.GetType().GetMethod("ReceiveBroadcast") != null)
                {
                    component.SendMessage("ReceiveBroadcast", broadcastedValue, SendMessageOptions.DontRequireReceiver);
                }
            }
            
            if (enableBroadcastLogging)
            {
                GameLogger.LogDev($"BroadcastManager: 向对象 '{objectName}' 广播: {broadcastedValue}");
            }
        }
        else
        {
            GameLogger.LogWarning($"BroadcastManager: 未找到名为 '{objectName}' 的对象");
        }
    }
    
    
    
    // 获取最近的广播
    public string GetLastBroadcast()
    {
        if (broadcastHistory.Count > 0)
        {
            return broadcastHistory[broadcastHistory.Count - 1];
        }
        return null;
    }
    
    /// <summary>
    /// 检查广播历史中是否存在指定的广播消息
    /// </summary>
    /// <param name="broadcastMessage">要检查的广播消息</param>
    /// <returns>是否存在</returns>
    public bool HasBroadcastHistory(string broadcastMessage)
    {
        if (string.IsNullOrEmpty(broadcastMessage)) return false;
        return broadcastHistory.Contains(broadcastMessage);
    }
    
    /// <summary>
    /// 获取广播历史记录（只读）
    /// </summary>
    /// <returns>广播历史记录</returns>
    public IReadOnlyList<string> GetBroadcastHistory()
    {
        return broadcastHistory.AsReadOnly();
    }
    
    /// <summary>
    /// 获取广播历史记录（可修改副本）
    /// </summary>
    /// <returns>广播历史记录副本</returns>
    public List<string> GetBroadcastHistoryCopy()
    {
        return new List<string>(broadcastHistory);
    }
    
    /// <summary>
    /// 替换广播历史（用于存档恢复，不触发实际广播）
    /// </summary>
    /// <param name="history">新的历史列表</param>
    public void ReplaceHistory(List<string> history)
    {
        broadcastHistory.Clear();
        if (history != null)
        {
            broadcastHistory.AddRange(history);
        }
    }
    
    /// <summary>
    /// 清空广播历史记录
    /// </summary>
    public void ClearBroadcastHistory()
    {
        broadcastHistory.Clear();
        if (enableDebugMode)
        {
            GameLogger.LogDev("BroadcastManager: 广播历史记录已清空");
        }
    }
    
}
