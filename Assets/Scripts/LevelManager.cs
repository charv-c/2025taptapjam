using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("UI设置")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TextMeshProUGUI levelCompleteText;
    
    [Header("关卡设置")]
    [SerializeField] private string nextSceneName = "NextLevel";
    [SerializeField] private float sceneTransitionDelay = 2f;
    [SerializeField] private string levelCompleteMessage = "关卡完成！";
    
    private void Start()
    {
        // 重置目标完成状态
        PublicData.ResetTargetCompletion();
        
        // 隐藏完成面板
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
        
        // 更新进度显示
        UpdateProgressDisplay();
    }
    
    private void Update()
    {
        // 检查是否所有目标都已完成
        if (PublicData.AreAllTargetsCompleted())
        {
            ShowLevelComplete();
        }
        else
        {
            UpdateProgressDisplay();
        }
    }
    
    private void UpdateProgressDisplay()
    {
        float progress = PublicData.GetCompletionProgress();
        
        // 更新进度条
        if (progressBar != null)
        {
            progressBar.value = progress;
        }
        
        // 更新进度文本
        if (progressText != null)
        {
            int completedCount = PublicData.completedTargets.Count;
            int totalCount = PublicData.completedTargets.Count + PublicData.targetList.Count;
            progressText.text = $"进度: {completedCount}/{totalCount}";
        }
    }
    
    private void ShowLevelComplete()
    {
        // 显示完成面板
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }
        
        // 更新完成文本
        if (levelCompleteText != null)
        {
            levelCompleteText.text = levelCompleteMessage;
        }
        
        // 场景切换现在由PublicData处理，这里不需要额外处理
    }
    

    
    // 公共方法：手动触发关卡完成（用于测试）
    public void ForceLevelComplete()
    {
        // 标记所有目标为已完成
        foreach (string target in PublicData.targetList.ToArray())
        {
            PublicData.MarkTargetAsCompleted(target);
        }
    }
    
    // 公共方法：重新开始当前关卡
    public void RestartLevel()
    {
        PublicData.ResetTargetCompletion();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    // 公共方法：获取当前完成进度
    public float GetCurrentProgress()
    {
        return PublicData.GetCompletionProgress();
    }
    
    // 公共方法：获取未完成的目标列表
    public System.Collections.Generic.List<string> GetIncompleteTargets()
    {
        return PublicData.GetIncompleteTargets();
    }
}
