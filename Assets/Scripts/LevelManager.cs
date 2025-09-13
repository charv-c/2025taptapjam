using UnityEngine;

public class LevelManager : MonoBehaviour
{
    // 当所有目标完成时触发的事件
    public event System.Action OnLevelCompleted;
    private bool isCompleted = false;
    
    private void Start()
    {
        isCompleted = false; // 确保每次加载场景时重置
        // 重置目标完成状态
        PublicData.ResetTargetCompletion();
    }
    
    private void Update()
    {
        // 如果尚未完成，则检查是否所有目标都已完成
        if (!isCompleted && PublicData.AreAllTargetsCompleted())
        {
            isCompleted = true;
            GameLogger.LogSystem($"LevelManager: 关卡 {gameObject.scene.name} 的所有目标已完成。");
            
            // 触发关卡完成事件
            OnLevelCompleted?.Invoke();
        }
    }
}
