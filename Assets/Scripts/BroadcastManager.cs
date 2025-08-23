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
        // 设置单例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 保持跨场景存在
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // 全屏广播方法
    public void BroadcastToAll(string broadcastedValue)
    {
        if (enableBroadcastLogging)
        {
            Debug.Log($"BroadcastManager: 开始全屏广播值: {broadcastedValue}");
        }
        
        // 记录广播历史
        broadcastHistory.Add(broadcastedValue);
        
        // 查找场景中所有的MonoBehaviour对象
        MonoBehaviour[] allObjects = FindObjectsOfType<MonoBehaviour>();
        
        int receiverCount = 0;
        int highlightCount = 0;
        
        foreach (MonoBehaviour obj in allObjects)
        {
            // 检查对象是否有ReceiveBroadcast方法
            if (obj.GetType().GetMethod("ReceiveBroadcast") != null)
            {
                // 特别检查Highlight组件
                if (obj is Highlight highlight)
                {
                    highlightCount++;
                    Debug.Log($"找到Highlight组件: {obj.gameObject.name}, letter={highlight.letter}, enabled={obj.enabled}, activeInHierarchy={obj.gameObject.activeInHierarchy}");
                }
                
                // 调用对象的接收广播方法
                obj.SendMessage("ReceiveBroadcast", broadcastedValue, SendMessageOptions.DontRequireReceiver);
                receiverCount++;
            }
        }
        
        if (enableBroadcastLogging)
        {
            Debug.Log($"BroadcastManager: 广播完成，发送给 {receiverCount} 个对象，其中 {highlightCount} 个Highlight组件");
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
            Debug.Log($"BroadcastManager: 向 {objectsOfType.Length} 个 {typeof(T).Name} 对象广播: {broadcastedValue}");
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
                Debug.Log($"BroadcastManager: 向对象 '{objectName}' 广播: {broadcastedValue}");
            }
        }
        else
        {
            Debug.LogWarning($"BroadcastManager: 未找到名为 '{objectName}' 的对象");
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
    
    
    
}
