using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 游戏结束/关卡胜利页面管理器 V3.0 (状态机版)
/// 负责根据GameFlowManager的状态显示UI，支持最后关卡的特殊流程和谢幕界面。
/// 谢幕界面仅显示包含所有文本的背景图，支持点击屏幕/空格/回车返回主菜单。
/// </summary>
public class EndScreenManager : MonoBehaviour
{
    /// <summary>
    /// 结束界面的状态枚举
    /// </summary>
    private enum EndScreenState
    {
        NormalEnd,    // 普通通关界面
        FinalEnd,     // 最后一关通关界面  
        Credits       // 谢幕界面
    }

    [Header("UI 控件引用")]
    [Tooltip("“下一关”按钮，应为包含扇子和文字的父对象Button")]
    [SerializeField] private Button nextLevelButton;
    [Tooltip("场景背景图片")]
    [SerializeField] private Image backgroundImage;
    
    [Header("谢幕背景配置（本地序列化）")]
    [Tooltip("谢幕界面背景（感谢游玩）")] 
    [SerializeField] private Sprite creditsSprite;


    // 私有状态变量
    private EndScreenState currentState = EndScreenState.NormalEnd;

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

        // 判断是否最后关卡，设置初始状态
        bool isLastLevel = !GameFlowManager.Instance.HasNextLevel();
        // 最后一关：显示最终通关页面，但背景使用感谢游玩图，按钮保留
        currentState = isLastLevel ? EndScreenState.Credits : EndScreenState.NormalEnd;
        
        GameLogger.LogSystem($"EndScreenManager: 初始化状态 - {currentState} (最后关卡: {isLastLevel})");

        // 根据状态设置UI
        SetupUI();
    }

    private void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// 根据当前状态处理输入
    /// </summary>
    private void HandleInput()
    {
        switch (currentState)
        {
            case EndScreenState.NormalEnd:
            case EndScreenState.FinalEnd:
                // 在正常结束和最终结束状态下，检测空格键触发按钮点击
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (nextLevelButton != null && nextLevelButton.gameObject.activeInHierarchy && nextLevelButton.interactable)
                    {
                        GameLogger.LogDev("检测到空格键按下，触发按钮点击。");
                        OnNextLevelButtonClicked();
                    }
                }
                break;

            case EndScreenState.Credits:
                // 在谢幕状态下，检测各种输入返回主菜单
                // 谢幕界面只显示背景图（包含所有感谢文本），支持多种输入方式返回主菜单
                if (Input.GetKeyDown(KeyCode.Space) || 
                    Input.GetKeyDown(KeyCode.Return) || 
                    Input.GetMouseButtonDown(0))
                {
                    GameLogger.LogSystem("检测到用户输入，从谢幕界面返回主菜单。");
                    ReturnToMainMenu();
                }
                break;
        }
    }

    /// <summary>
    /// 按钮点击处理函数，根据当前状态执行不同逻辑
    /// </summary>
    public void OnNextLevelButtonClicked()
    {
        switch (currentState)
        {
            case EndScreenState.NormalEnd:
                // 普通关卡，进入下一关
                GoToNextLevel();
                break;
                
            case EndScreenState.FinalEnd:
                // 最后关卡，切换到谢幕界面
                TransitionToCredits();
                break;
                
            case EndScreenState.Credits:
                // 谢幕界面不应该有按钮，这里不处理
                break;
        }
    }
    
    /// <summary>
    /// 进入下一个关卡（普通关卡流程）
    /// </summary>
    private void GoToNextLevel()
    {
        if (GameFlowManager.Instance != null)
        {
            GameLogger.LogSystem("EndScreenManager: 正在加载下一个关卡");
            GameFlowManager.Instance.GoToNextLevel();
        }
        else
        {
            GameLogger.LogError("EndScreenManager: GameFlowManager实例不存在，无法进入下一关。");
        }
    }

    /// <summary>
    /// 切换到谢幕界面
    /// </summary>
    private void TransitionToCredits()
    {
        GameLogger.LogSystem("EndScreenManager: 切换到谢幕界面");
        
        currentState = EndScreenState.Credits;
        SetupUI();
    }
    
    /// <summary>
    /// 返回主菜单（游戏全部通关后调用）
    /// </summary>
    private void ReturnToMainMenu()
    {
        GameLogger.LogSystem("EndScreenManager: 返回主菜单");
        // 切换到Startup前，清空存档
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.ClearAllProgress();
        }
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

    /// <summary>
    /// 根据当前状态设置UI显示
    /// </summary>
    private void SetupUI()
    {
        switch (currentState)
        {
            case EndScreenState.NormalEnd:
                SetupNormalEndUI();
                break;
            case EndScreenState.FinalEnd:
                SetupFinalEndUI();
                break;
            case EndScreenState.Credits:
                SetupCreditsUI();
                break;
        }
    }

    /// <summary>
    /// 设置普通通关界面UI
    /// </summary>
    private void SetupNormalEndUI()
    {
        GameLogger.LogSystem("EndScreenManager: 设置普通通关界面UI");
        
        // 显示默认的下一关按钮
        if (nextLevelButton != null) 
        {
            nextLevelButton.gameObject.SetActive(true);
            SetupButtonAppearance(nextLevelButton, "下一关");

            // 确保按钮有悬停效果（这是默认行为，但显式设置更稳妥）
            nextLevelButton.transition = Selectable.Transition.SpriteSwap;
        }
        
        // 设置背景图
        SetBackgroundImage();
    }

    /// <summary>
    /// 设置最终关卡通关界面UI
    /// </summary>
    private void SetupFinalEndUI()
    {
        GameLogger.LogSystem("EndScreenManager: 设置最终关卡通关界面UI");
        
        // 显示特殊的按钮（可能有特殊素材和文案）
        if (nextLevelButton != null) 
        {
            nextLevelButton.gameObject.SetActive(true);
            SetupButtonAppearance(nextLevelButton, "完美落幕"); // 默认文案，可被配置覆盖

            // 禁用按钮的悬停效果
            nextLevelButton.transition = Selectable.Transition.None;
            GameLogger.LogSystem("EndScreenManager: 已禁用最终关卡按钮的悬停效果。");
        }
        
        // 最后一关改为使用"感谢游玩"背景图
        SetCreditsBackground();
    }

    /// <summary>
    /// 设置谢幕界面UI
    /// </summary>
    private void SetupCreditsUI()
    {
        GameLogger.LogSystem("EndScreenManager: 设置谢幕界面UI");
        
        // 隐藏按钮
        if (nextLevelButton != null) 
        {
            nextLevelButton.gameObject.SetActive(false);
        }
        
        // 设置谢幕背景图（背景图包含所有文本和视觉元素）
        SetCreditsBackground();
        
        GameLogger.LogSystem("EndScreenManager: 谢幕界面设置完成，等待用户输入返回主菜单");
    }

    /// <summary>
    /// 设置按钮外观（素材和文案）
    /// </summary>
    /// <param name="button">按钮对象</param>
    /// <param name="defaultText">默认文案</param>
    private void SetupButtonAppearance(Button button, string defaultText)
    {
        if (button == null || GameFlowManager.Instance == null) return;
        
        string lastLevel = GameFlowManager.LastCompletedLevelName;
        if (string.IsNullOrEmpty(lastLevel)) return;
        
        // 设置按钮素材
        if (PublicData.LevelEndButtonSprites.ContainsKey(lastLevel))
        {
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = PublicData.LevelEndButtonSprites[lastLevel];
                GameLogger.LogDev($"已为关卡'{lastLevel}'设置特殊按钮素材。");
            }
        }
        
        // 设置按钮文案
        string buttonText = defaultText;
        if (PublicData.LevelEndButtonTexts.ContainsKey(lastLevel))
        {
            buttonText = PublicData.LevelEndButtonTexts[lastLevel];
            GameLogger.LogDev($"已为关卡'{lastLevel}'设置特殊按钮文案: {buttonText}");
        }
        
        // 更新按钮文字
        TextMeshProUGUI textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = buttonText;
        }
    }

    /// <summary>
    /// 设置通关背景图
    /// </summary>
    private void SetBackgroundImage()
    {
        if (backgroundImage == null || GameFlowManager.Instance == null) return;
        
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
    
    /// <summary>
    /// 设置谢幕背景图
    /// </summary>
    private void SetCreditsBackground()
    {
        if (backgroundImage == null)
        {
            GameLogger.LogWarning("EndScreenManager: backgroundImage 未设置，无法显示谢幕背景");
            return;
        }
        
        if (creditsSprite != null)
        {
            backgroundImage.sprite = creditsSprite;
            GameLogger.LogSystem("EndScreenManager: 使用序列化的谢幕背景（感谢游玩）");
            return;
        }
        
        GameLogger.LogWarning("EndScreenManager: creditsSprite 未设置，保持当前背景");
    }
    
}