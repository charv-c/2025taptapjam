using UnityEngine;

// 示例：其他类型的对象接收广播
public class BroadcastReceiver : MonoBehaviour
{
    [Header("广播接收设置")]
    [SerializeField] private string objectType = "通用对象";
    [SerializeField] private bool enableBroadcastLogging = true;
    
    // 接收广播的方法（必须与广播调用方法名一致）
    public void ReceiveBroadcast(string broadcastedValue)
    {
        if (enableBroadcastLogging)
        {
            Debug.Log($"BroadcastReceiver: 对象 '{gameObject.name}' (类型: {objectType}) 接收到广播: {broadcastedValue}");
        }
        
        // 根据广播的值执行不同的逻辑
        HandleBroadcast(broadcastedValue);
    }
    
    // 处理广播的方法
    private void HandleBroadcast(string broadcastedValue)
    {
        switch (broadcastedValue)
        {
            case "雨":
                HandleRainBroadcast();
                break;
            case "风":
                HandleWindBroadcast();
                break;
            case "火":
                HandleFireBroadcast();
                break;
            case "停":
                HandleStopBroadcast();
                break;
            default:
                HandleDefaultBroadcast(broadcastedValue);
                break;
        }
    }
    
    // 处理雨广播
    private void HandleRainBroadcast()
    {
        Debug.Log($"对象 {gameObject.name} 执行雨相关逻辑");
        // 可以在这里添加雨相关的效果
        // 比如改变颜色、播放音效、触发粒子效果等
    }
    
    // 处理风广播
    private void HandleWindBroadcast()
    {
        Debug.Log($"对象 {gameObject.name} 执行风相关逻辑");
        // 可以在这里添加风相关的效果
    }
    
    // 处理火广播
    private void HandleFireBroadcast()
    {
        Debug.Log($"对象 {gameObject.name} 执行火相关逻辑");
        // 可以在这里添加火相关的效果
    }
    
    // 处理停广播
    private void HandleStopBroadcast()
    {
        Debug.Log($"对象 {gameObject.name} 执行停相关逻辑");
        // 可以在这里添加停相关的效果
    }
    
    // 处理默认广播
    private void HandleDefaultBroadcast(string value)
    {
        Debug.Log($"对象 {gameObject.name} 执行默认广播逻辑: {value}");
        // 可以在这里添加默认的广播处理逻辑
    }
}
