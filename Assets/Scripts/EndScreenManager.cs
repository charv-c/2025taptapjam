using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 游戏结束/关卡胜利页面管理器 V2.0 (架构优化版)
/// 负责根据GameFlowManager的状态显示UI，并处理进入下一关的交互。
/// </summary>
public class EndScreenManager : MonoBehaviour
{
    [Header("UI 控件引用")]
    [Tooltip("“下一关”按钮，应为包含扇子和文字的父对象Button")]
    [SerializeField] private Button nextLevelButton;
    [Tooltip("场景背景图片")]
    [SerializeField] private Image backgroundImage;

    [Header("最终通关文案")]
    [TextArea(3, 5)]
    [SerializeField] private string finalMessage = "知音已遇，高山流水之曲响彻心域。\n然缘分长河不止，前路尚有未谱之章，待执笔人他日再续。";

    private void Start()
    {
        // 确保GameFlowManager存在
        if (GameFlowManager.Instance == null)
        {
            GameLogger.LogError("EndScreenManager: GameFlowManager实例不存在！流程将无法继续。请确保从StartMenu场景启动游戏。");
            // 禁用按钮以防出错
            if (nextLevelButton != null) nextLevelButton.interactable = false;
            return;
        }

        // 播放一次性的胜利音效
        if (AudioManager.Instance != null && AudioManager.Instance.sfxWin != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxWin);
        }

        // 总是显示"下一关"按钮，让用户点击后再判断是否有下一关
        if (nextLevelButton != null) 
        {
            nextLevelButton.gameObject.SetActive(true);
            GameLogger.LogSystem("EndScreenManager: 显示下一关按钮，等待用户点击");
        }

        // 动态设置背景图
        SetEndBackground(); // 之前是注释的，现在我们正式启用它
    }

    private void Update()
    {
        // 检测是否按下空格键，并且"下一关"按钮是可见且可交互的
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (nextLevelButton != null && nextLevelButton.gameObject.activeInHierarchy && nextLevelButton.interactable)
            {
                GameLogger.LogDev("检测到空格键按下，触发进入下一关。");
                GoToNextLevel();
            }
        }
    }

    /// <summary>
    /// 进入下一个关卡（由按钮点击或空格键调用）
    /// </summary>
    public void GoToNextLevel()
    {
        if (GameFlowManager.Instance != null)
        {
            // 检查是否有下一关
            if (GameFlowManager.Instance.HasNextLevel())
            {
                // 有下一关，正常进入下一关
                GameLogger.LogSystem("EndScreenManager: 有下一关，正在加载下一个关卡");
                GameFlowManager.Instance.GoToNextLevel();
            }
            else
            {
                // 没有下一关，显示最终通关文案并隐藏按钮
                GameLogger.LogSystem("EndScreenManager: 已是最后一关，显示最终通关文案");
                
                // 隐藏下一关按钮
                if (nextLevelButton != null) 
                {
                    nextLevelButton.gameObject.SetActive(false);
                }
                
                // 使用弹窗系统显示最终通关文案
                if (InfoPopupManager.Instance != null)
                {
                    // 将最终文案拆分成数组以适应弹窗系统
                    string[] messages = finalMessage.Split('\n');
                    InfoPopupManager.Instance.ShowPopup(messages, ReturnToMainMenu, null, "返回主菜单");
                }
                else
                {
                    GameLogger.LogError("EndScreenManager: InfoPopupManager实例不存在，无法显示最终通关信息。");
                }
            }
        }
        else
        {
            GameLogger.LogError("EndScreenManager: GameFlowManager实例不存在，无法进入下一关。");
        }
    }
    
    /// <summary>
    /// 返回主菜单（游戏全部通关后调用）
    /// </summary>
    private void ReturnToMainMenu()
    {
        GameLogger.LogSystem("EndScreenManager: 返回主菜单");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Startup");
    }
    
    #region 音效代理方法
    /// <summary>
    /// 公开的音效代理方法，用于UI事件绑定，播放悬停音效。
    /// </summary>
    public void PlayHoverSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonHover();
        }
    }

    /// <summary>
    /// 公开的音效代理方法，用于UI事件绑定，播放点击音效。
    /// </summary>
    public void PlayClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
    #endregion

    // 动态设置通关背景
    private void SetEndBackground()
    {
        if (backgroundImage != null && GameFlowManager.Instance != null)
        {
            string lastLevel = GameFlowManager.LastCompletedLevelName;
            if (!string.IsNullOrEmpty(lastLevel) && PublicData.LevelEndBackgrounds.ContainsKey(lastLevel))
            {
                backgroundImage.sprite = PublicData.LevelEndBackgrounds[lastLevel];
                GameLogger.LogDev($"已为关卡'{lastLevel}'设置通关背景图。");
            }
            else
            {
                GameLogger.LogWarning($"未找到为关卡'{lastLevel}'配置的通关背景图。");
            }
        }
    }
}