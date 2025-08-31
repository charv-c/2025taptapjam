using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 新手引导管理器 V1.4 (表情、箭头、布局增强版)
/// </summary>
public class TutorialManager : MonoBehaviour
{
    #region Singleton
    public static TutorialManager Instance;
    #endregion

    #region Public References
    [Header("UI 元素")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI hintText;
    public Button continueButton;

    [Header("字体设置")]
    [SerializeField] private TMP_FontAsset chineseFont; // 中文字体资源

    [Header("引导人物与表情")]
    public Image guideCharacterImage;
    public Sprite exprNormal;     // 正常表情
    public Sprite exprHappy;      // 开心表情
    public Sprite exprSideEye;    // 斜眼表情 (替代了“惊讶”)

    [Header("指示器元素")]
    // public RectTransform highlightBox;
    public Image arrowImage;       // 箭头的Image组件
    public Sprite arrowLeft;       // 向左的箭头图片
    public Sprite arrowDownLeft;   // 向左下的箭头图片

    [Header("教学关卡中的关键对象")]
    public GameObject dogObject;
    public GameObject grassObject;
    public GameObject dieObject;

    [Header("解字台的关键UI按钮")]
    public Button splitButton;
    public Button combineButton;

    [Header("教学关卡蒙层")]
    public GameObject tutorialMask;

    [Header("遮罩控制器")]
    public MaskController maskController; // 遮罩控制器引用

    [Header("需要禁用的UI元素")]
    public Button[] uiButtonsToDisable; // 需要禁用的UI按钮数组
    #endregion

    #region Private State
    private enum TutorialStep { Welcome_Part1, Welcome_Part2, Welcome_Part3, MoveToDog, AfterTransform, MoveToGrass, AfterGetChong, SwitchPlayer_Part1, SwitchPlayer_Part2, MoveToDie, AfterGetDie, SelectAndSplit, AfterSplit, SelectAndCombine, AfterCombine, CharacterFly, End_Part1, End_Part2, End_Part3 }
    private TutorialStep currentStep;
    private PlayerController playerController; // 引用PlayerController来控制玩家移动

    // 事件系统
    private Dictionary<TutorialStep, System.Action> stepHandlers;
    private Dictionary<TutorialStep, TutorialStep> stepTransitions;
    
    // 状态标志
    private bool chongShown = false;
    private bool dieShown = false;
    #endregion

    #region Unity Lifecycle
    private void Awake() { if (Instance == null) { Instance = this; } else { Destroy(gameObject); } }

    private void Start()
    {
        // 获取PlayerController引用
        playerController = FindObjectOfType<PlayerController>();

        // 确保广播管理器存在
        EnsureBroadcastManagerExists();

        // 初始化事件系统
        InitializeEventSystem();

        // 设置中文字体
        SetChineseFont();

        // 检查组件引用
        CheckComponentReferences();

        // 设置Level1的BGM
        SetupLevel1BGM();

        // 延迟一帧后禁止玩家移动，确保PlayerController已完全初始化
        StartCoroutine(DelayedDisablePlayerMovement());

        // 初始化遮罩控制器
        InitializeMaskController();

        // 在教程开始时禁用字符选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.DisableCharacterSelection();
            Debug.Log("TutorialManager: 教程开始时已禁用字符选择");
        }
        
        // 在教程模式中禁用颜色管理
        if (playerController != null)
        {
            playerController.DisableColorManagement();
            Debug.Log("TutorialManager: 教程模式中已禁用颜色管理");
        }

        tutorialMask.SetActive(true); // 在教程开始时激活蒙层
        currentStep = TutorialStep.Welcome_Part1;
        ExecuteCurrentStep();
        continueButton.onClick.AddListener(GoToNextStep);

    }
    #endregion

    #region Audio Setup
    // 初始化遮罩控制器
    private void InitializeMaskController()
    {
        // 如果没有手动设置maskController，尝试在场景中查找
        if (maskController == null)
        {
            maskController = FindObjectOfType<MaskController>();
        }
        
        // 检查遮罩控制器是否已正确挂载
        if (maskController != null)
        {
            Debug.Log("TutorialManager: 遮罩控制器初始化完成");
        }
        else
        {
            Debug.LogWarning("TutorialManager: 遮罩控制器为空，请确保MaskController已挂载到遮罩对象上");
        }
    }

    // 设置Level1的BGM
    private void SetupLevel1BGM()
    {
        if (AudioManager.Instance != null)
        {
            // 播放教学关卡BGM
            if (AudioManager.Instance.bgmTutorial != null)
            {
                AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmTutorial);
                Debug.Log("TutorialManager: 已设置Level1 BGM为bgmTutorial");
            }
            else
            {
                Debug.LogWarning("TutorialManager: bgmTutorial音频片段未设置");
            }
            
            // 停止雨声环境音
            AudioManager.Instance.StopAmbient(1f);
            Debug.Log("TutorialManager: 已停止雨声环境音");
        }
        else
        {
            Debug.LogWarning("TutorialManager: 未找到AudioManager实例");
        }
    }

    // 播放UI交互音效
    private void PlayUIClickSound()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.sfxUIClick != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxUIClick);
            Debug.Log("TutorialManager: 播放UI交互音效");
        }
        else
        {
            Debug.LogWarning("TutorialManager: 无法播放UI交互音效，AudioManager或sfxUIClick为空");
        }
    }
    #endregion

    #region Event System
    private void InitializeEventSystem()
    {
        stepHandlers = new Dictionary<TutorialStep, System.Action>();
        stepTransitions = new Dictionary<TutorialStep, TutorialStep>();

        // 注册所有步骤的处理函数
        RegisterStepHandlers();

        // 注册步骤转换规则
        RegisterStepTransitions();
    }

    private void RegisterStepHandlers()
    {
        stepHandlers[TutorialStep.Welcome_Part1] = HandleWelcomePart1;
        stepHandlers[TutorialStep.Welcome_Part2] = HandleWelcomePart2;
        stepHandlers[TutorialStep.Welcome_Part3] = HandleWelcomePart3;
        stepHandlers[TutorialStep.MoveToDog] = HandleMoveToDog;
        stepHandlers[TutorialStep.AfterTransform] = HandleAfterTransform;
        stepHandlers[TutorialStep.MoveToGrass] = HandleMoveToGrass;
        stepHandlers[TutorialStep.AfterGetChong] = HandleAfterGetChong;
        stepHandlers[TutorialStep.SwitchPlayer_Part1] = HandleSwitchPlayerPart1;
        stepHandlers[TutorialStep.SwitchPlayer_Part2] = HandleSwitchPlayerPart2;
        stepHandlers[TutorialStep.MoveToDie] = HandleMoveToDie;
        stepHandlers[TutorialStep.AfterGetDie] = HandleAfterGetDie;
        stepHandlers[TutorialStep.SelectAndSplit] = HandleSelectAndSplit;
        stepHandlers[TutorialStep.AfterSplit] = HandleAfterSplit;
        stepHandlers[TutorialStep.SelectAndCombine] = HandleSelectAndCombine;
        stepHandlers[TutorialStep.AfterCombine] = HandleAfterCombine;
        stepHandlers[TutorialStep.CharacterFly] = HandleCharacterFly;
        stepHandlers[TutorialStep.End_Part1] = HandleEndPart1;
        stepHandlers[TutorialStep.End_Part2] = HandleEndPart2;
        stepHandlers[TutorialStep.End_Part3] = HandleEndPart3;
        
        Debug.Log($"TutorialManager: 已注册 {stepHandlers.Count} 个步骤处理函数");
        Debug.Log($"TutorialManager: SelectAndCombine步骤注册状态: {stepHandlers.ContainsKey(TutorialStep.SelectAndCombine)}");
        Debug.Log($"TutorialManager: SelectAndCombine步骤的处理函数: {stepHandlers[TutorialStep.SelectAndCombine]?.Method.Name ?? "null"}");
    }

    private void RegisterStepTransitions()
    {
        // 定义步骤转换规则
        stepTransitions[TutorialStep.Welcome_Part1] = TutorialStep.Welcome_Part2;
        stepTransitions[TutorialStep.Welcome_Part2] = TutorialStep.Welcome_Part3;
        stepTransitions[TutorialStep.Welcome_Part3] = TutorialStep.MoveToDog;
        stepTransitions[TutorialStep.MoveToDog] = TutorialStep.AfterTransform;
        stepTransitions[TutorialStep.AfterTransform] = TutorialStep.MoveToGrass;
        stepTransitions[TutorialStep.MoveToGrass] = TutorialStep.AfterGetChong;
        stepTransitions[TutorialStep.AfterGetChong] = TutorialStep.SwitchPlayer_Part1;
        stepTransitions[TutorialStep.SwitchPlayer_Part1] = TutorialStep.SwitchPlayer_Part2;
        stepTransitions[TutorialStep.SwitchPlayer_Part2] = TutorialStep.MoveToDie;
        stepTransitions[TutorialStep.MoveToDie] = TutorialStep.AfterGetDie;
        stepTransitions[TutorialStep.AfterGetDie] = TutorialStep.SelectAndSplit;
        stepTransitions[TutorialStep.SelectAndSplit] = TutorialStep.AfterSplit;
        stepTransitions[TutorialStep.AfterSplit] = TutorialStep.SelectAndCombine;
        stepTransitions[TutorialStep.SelectAndCombine] = TutorialStep.AfterCombine;
        stepTransitions[TutorialStep.AfterCombine] = TutorialStep.CharacterFly;
        stepTransitions[TutorialStep.CharacterFly] = TutorialStep.End_Part1;
        stepTransitions[TutorialStep.End_Part1] = TutorialStep.End_Part2;
        stepTransitions[TutorialStep.End_Part2] = TutorialStep.End_Part3;
    }

    private void ExecuteCurrentStep()
    {
        Debug.Log($"TutorialManager: ExecuteCurrentStep - 开始执行，当前步骤: {currentStep}");
        
        // 初始化UI状态
        InitializeUI();

        Debug.Log($"TutorialManager: 执行步骤 {currentStep}");
        Debug.Log($"TutorialManager: 当前步骤的整数值: {(int)currentStep}");

        if (stepHandlers.ContainsKey(currentStep))
        {
            Debug.Log($"TutorialManager: 找到步骤 {currentStep} 的处理函数，准备调用");
            try
            {
                var handler = stepHandlers[currentStep];
                Debug.Log($"TutorialManager: 处理函数类型: {handler?.Method.Name ?? "null"}");
                Debug.Log($"TutorialManager: ExecuteCurrentStep - 即将调用处理函数");
                handler?.Invoke();
                Debug.Log($"TutorialManager: 步骤 {currentStep} 执行成功");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"TutorialManager: 步骤 {currentStep} 执行失败: {e.Message}");
                Debug.LogError($"TutorialManager: 异常堆栈: {e.StackTrace}");
            }
        }
        else
        {
            Debug.LogError($"TutorialManager: 未找到步骤 {currentStep} 的处理函数");
            Debug.LogError($"TutorialManager: 已注册的步骤: {string.Join(", ", stepHandlers.Keys)}");
            
            // 检查SelectAndCombine步骤的注册情况
            Debug.LogError($"TutorialManager: SelectAndCombine步骤是否存在: {stepHandlers.ContainsKey(TutorialStep.SelectAndCombine)}");
            Debug.LogError($"TutorialManager: SelectAndCombine步骤的整数值: {(int)TutorialStep.SelectAndCombine}");
        }
    }

    public void GoToNextStep()
    {
        Debug.Log($"TutorialManager: 尝试从步骤 {currentStep} 跳转到下一步");
        Debug.Log($"TutorialManager: GoToNextStep - 调用堆栈: {System.Environment.StackTrace}");

        // 播放UI交互音效
        PlayUIClickSound();

        if (stepTransitions.ContainsKey(currentStep))
        {
            TutorialStep nextStep = stepTransitions[currentStep];
            Debug.Log($"TutorialManager: 从 {currentStep} 跳转到 {nextStep}");
            currentStep = nextStep;
            ExecuteCurrentStep();
        }
        else
        {
            Debug.Log($"TutorialManager: 步骤 {currentStep} 没有下一个步骤，教程结束");
            
            // 如果是最后一个步骤，加载下一个场景
            if (currentStep == TutorialStep.End_Part3)
            {
                Debug.Log("TutorialManager: 教学完成，加载下一个场景 Level2");
                LoadNextScene();
            }
        }
    }
    #endregion

    #region Step Handlers
    private void HandleWelcomePart1()
    {
        DisablePlayerMovement();
        DisablePlayerSwitching();
        DisableEnterKey();
        
        // 禁用字符选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.DisableCharacterSelection();
        }
        
        SetGuideExpression(exprNormal);
        hintText.text = "缘分的故事，始于同窗之谊...";
        continueButton.gameObject.SetActive(true);
    }

    private void HandleWelcomePart2()
    {
        DisablePlayerMovement();
        DisablePlayerSwitching();
        DisableEnterKey();
        
        // 禁用字符选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.DisableCharacterSelection();
        }
        
        SetGuideExpression(exprNormal);
        hintText.text = "这里是万松书院，梁山伯与祝英台初遇之地。而你，将化身为【梁山伯】...";
        continueButton.gameObject.SetActive(true);
    }

    private void HandleWelcomePart3()
    {
        SetGuideExpression(exprHappy);
        hintText.text = "请使用【WASD】键，在书院中自由走动，熟悉一下环境吧。";
        continueButton.gameObject.SetActive(true);
        EnablePlayerMovement();
        
        // 禁用字符选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.DisableCharacterSelection();
        }
        
        // EnableUIInteraction();
    }

    private void HandleMoveToDog()
    {
        SetGuideExpression(exprSideEye);
        hintText.text = "这只小狗似乎很亲近你。靠近它并按下【回车键】进行互动。";
        if (dogObject != null)
        {
            PointAtTarget(dogObject.transform);
        }
        EnablePlayerMovement(0);
        
        // 设置遮罩位置
        if (maskController != null)
        {
            maskController.SetMaskToRight();
            Debug.Log("TutorialManager: 已设置遮罩到右边位置");
        }
        
        // 启用回车键响应，允许玩家与小狗交互
        if (playerController != null && playerController.GetCurrentPlayer() != null)
        {
            Player currentPlayer = playerController.GetCurrentPlayer();
            currentPlayer.SetEnterKeyEnabled(true);
            Debug.Log("TutorialManager: 已启用回车键响应，允许与小狗交互");
        }

        // 禁用字符选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.DisableCharacterSelection();
        }

        // 开始重复检查玩家是否获得"伏"字
        StartCoroutine(CheckForFuCharacter());
    }
    
    // 重复检查玩家是否获得"伏"字的协程
    private IEnumerator CheckForFuCharacter()
    {
        Debug.Log("TutorialManager: 开始重复检查玩家是否获得'伏'字");
        
        while (true)
        {
            // 检查玩家是否已经获得"伏"字
            if (playerController != null && playerController.GetCurrentPlayer() != null)
            {
                Player currentPlayer = playerController.GetCurrentPlayer();
                if (currentPlayer.CarryCharacter == "伏")
                {
                    Debug.Log("TutorialManager: 检测到玩家已获得'伏'字，自动进入下一步");
                    // 禁用回车键响应
                    currentPlayer.SetEnterKeyEnabled(false);
                    // 自动进入下一步
                    GoToNextStep();
                    yield break; // 退出协程
                }
                else
                {
                    Debug.Log($"TutorialManager: 玩家当前携带字符为 '{currentPlayer.CarryCharacter}'，继续等待获得'伏'字");
                }
            }
            else
            {
                Debug.LogWarning("TutorialManager: 无法获取当前玩家信息");
            }
            
            // 等待0.5秒后再次检查
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void HandleAfterTransform()
    {
        DisablePlayerMovement();
        DisableEnterKey();
        
        // 禁用字符选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.DisableCharacterSelection();
        }
        
        SetGuideExpression(exprHappy);
        hintText.text = "很好！你变成了\"伏\"字，获得了伏下身子的能力。";

        // 显示继续按钮，等待玩家点击
        continueButton.gameObject.SetActive(true);
        //EnableUIInteraction(); // 启用UI交互以便继续按钮可用
    }

    private void HandleMoveToGrass()
    {
        SetGuideExpression(exprSideEye);
        hintText.text = "那片草丛里似乎藏着什么，靠近并按下【回车键】仔细看看吧。";
        if (grassObject != null)
        {
            PointAtTarget(grassObject.transform);
        }
        EnablePlayerMovement(0);
        
        EnableEnterKey(); // 启用回车键响应，允许与草丛交互

        // 禁用字符选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.DisableCharacterSelection();
        }

        // 重置虫显示标志
        chongShown = false;

        // 开始重复检查玩家是否获得"虫"字
        StartCoroutine(CheckForChongCharacter());
    }
    
    // 重复检查玩家是否获得"虫"字的协程
    private IEnumerator CheckForChongCharacter()
    {
        Debug.Log("TutorialManager: 开始重复检查玩家是否获得'虫'字");
        
        // 等待虫显示的通知
        while (!chongShown)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("TutorialManager: 收到虫显示通知，自动进入下一步");
        // 禁用回车键响应
        if (playerController != null && playerController.GetCurrentPlayer() != null)
        {
            Player currentPlayer = playerController.GetCurrentPlayer();
            currentPlayer.SetEnterKeyEnabled(false);
        }
        // 自动进入下一步
        GoToNextStep();
    }
    
    // 重复检查玩家是否按下空格键的协程
    private IEnumerator CheckForSpaceKeyPress()
    {
        Debug.Log("TutorialManager: 开始重复检查玩家是否按下空格键");
        
        // 记录初始的当前玩家索引
        int initialPlayerIndex = playerController != null ? playerController.GetCurrentPlayerIndex() : 0;
        
        while (true)
        {
            // 检查当前玩家索引是否发生变化（表示玩家按下了空格键）
            if (playerController != null)
            {
                int currentPlayerIndex = playerController.GetCurrentPlayerIndex();
                if (currentPlayerIndex != initialPlayerIndex)
                {
                    Debug.Log($"TutorialManager: 检测到玩家按下空格键，从玩家{initialPlayerIndex}切换到玩家{currentPlayerIndex}，自动进入下一步");
                    // 自动进入下一步
                    GoToNextStep();
                    yield break; // 退出协程
                }
            }
            else
            {
                Debug.LogWarning("TutorialManager: 无法获取PlayerController信息");
            }
            
            // 等待0.1秒后再次检查
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    // 重复检查玩家是否按下空格键的协程（Part2版本）
    private IEnumerator CheckForSpaceKeyPressInPart2()
    {
        Debug.Log("TutorialManager: 开始重复检查玩家是否按下空格键（Part2）");
        
        // 记录初始的当前玩家索引
        int initialPlayerIndex = playerController != null ? playerController.GetCurrentPlayerIndex() : 0;
        
        while (true)
        {
            // 检查当前玩家索引是否发生变化（表示玩家按下了空格键）
            if (playerController != null)
            {
                int currentPlayerIndex = playerController.GetCurrentPlayerIndex();
                if (currentPlayerIndex != initialPlayerIndex)
                {
                    Debug.Log($"TutorialManager: 检测到玩家按下空格键，从玩家{initialPlayerIndex}切换到玩家{currentPlayerIndex}，启用人物2移动并进入下一步");
                    
                    // 切换到人物2并启用移动
                    EnablePlayerMovement(1);
                    if (maskController != null)
                    {
                        maskController.SetMaskToLeft();
                        Debug.Log("TutorialManager: 已设置遮罩到左边位置");
                    }
                    
                    // 自动进入下一步
                    GoToNextStep();
                    yield break; // 退出协程
                }
            }
            else
            {
                Debug.LogWarning("TutorialManager: 无法获取PlayerController信息");
            }
            
            // 等待0.1秒后再次检查
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void HandleAfterGetChong()
    {
        DisablePlayerMovement();
        DisableEnterKey();
        
        // 禁用字符选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.DisableCharacterSelection();
        }
        
        SetGuideExpression(exprHappy);
        hintText.text = "你获得了【虫】字！这是化蝶的关键。";

        // 显示继续按钮，等待玩家点击
        continueButton.gameObject.SetActive(true);
        EnableUIInteraction(); // 启用UI交互以便继续按钮可用
    }

    private void HandleSwitchPlayerPart1()
    {
        DisableUIInteraction();
        DisablePlayerMovement();
        DisablePlayerSwitching();
        
        // 禁用字符选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.DisableCharacterSelection();
        }
        
        SetGuideExpression(exprSideEye);
        hintText.text = "与此同时，那位女扮男装的同窗【祝英台】，又在做什么呢？";
        continueButton.gameObject.SetActive(true);
        
    }

    private void HandleSwitchPlayerPart2()
    {
        SetGuideExpression(exprNormal);
        hintText.text = "按下【空格键】切换到她的视角看看吧。";
        continueButton.gameObject.SetActive(false); // 隐藏继续按钮，使用自动检测
        DisablePlayerMovement(); // 先禁用移动，等待空格键
        EnablePlayerSwitching(); // 启用切换功能
        
        // 禁用字符选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.DisableCharacterSelection();
        }
        
        //EnableUIInteraction();
        
        // 开始重复检查玩家是否按下空格键
        StartCoroutine(CheckForSpaceKeyPressInPart2());
    }

    private void HandleMoveToDie()
    {
        SetGuideExpression(exprSideEye);
        hintText.text = "这是祝英台的书房。请操作她靠近书桌上的【文牒】，并按下【回车键】。";
        if (dieObject != null)
        {
            PointAtTarget(dieObject.transform);
        }
        
        // 确保当前玩家是人物2（祝英台）
        if (playerController != null)
        {
            playerController.SetCurrentPlayerIndex(1);
        }
        
        EnablePlayerMovement(1);
        
        EnableEnterKey(); // 启用回车键响应，允许与文牒交互

        // 禁用字符选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.DisableCharacterSelection();
        }

        // 重置牒显示标志
        dieShown = false;

        // 开始重复检查玩家是否获得"牒"字
        StartCoroutine(CheckForDieCharacter());
    }
    
    // 重复检查玩家是否获得"牒"字的协程
    private IEnumerator CheckForDieCharacter()
    {
        Debug.Log("TutorialManager: 开始重复检查玩家是否获得'牒'字");
        
        // 等待牒显示的通知
        while (!dieShown)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.Log("TutorialManager: 收到牒显示通知，自动进入下一步");
        // 禁用回车键响应
        if (playerController != null && playerController.GetCurrentPlayer() != null)
        {
            Player currentPlayer = playerController.GetCurrentPlayer();
            currentPlayer.SetEnterKeyEnabled(false);
        }
        // 自动进入下一步
        GoToNextStep();
    }

    private void HandleAfterGetDie()
    {
        DisablePlayerMovement();
        
        // 禁用字符选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.DisableCharacterSelection();
        }
        
        SetGuideExpression(exprHappy);
        hintText.text = "你获得了【牒】字！";

        // 显示继续按钮，等待玩家点击
        continueButton.gameObject.SetActive(true);
        EnableUIInteraction(); // 启用UI交互以便继续按钮可用
    }

    private void HandleSelectAndSplit()
    {
        Debug.Log("TutorialManager: HandleSelectAndSplit - 开始执行");
        
        // 启用教程模式，防止ButtonController自动启用拆按钮
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.SetTutorialMode(true);
            Debug.Log("TutorialManager: HandleSelectAndSplit - 已启用教程模式");
        }
        
        // 先禁用拆按钮，确保在显示提示之前就禁用
        if (splitButton != null)
        {
            splitButton.interactable = false;
            Debug.Log("TutorialManager: HandleSelectAndSplit - 已禁用拆按钮");
        }
        else
        {
            Debug.LogError("TutorialManager: HandleSelectAndSplit - splitButton为空!");
        }
        
        // 设置表情和提示文本
        SetGuideExpression(exprSideEye);
        hintText.text = "请在解字台中，选中【牒】字，然后点击【拆】按钮。";
        
        // 高亮拆按钮（即使它是禁用的）
        if (splitButton != null)
        {
            Debug.Log($"TutorialManager: HandleSelectAndSplit - 调用HighlightUITarget，目标: {splitButton.name}");
            HighlightUITarget(splitButton.transform);
        }
        
        DisablePlayerMovement();
        EnableUIInteraction();
        
        // 在这个步骤中允许字符选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.EnableCharacterSelection();
            Debug.Log("TutorialManager: HandleSelectAndSplit - 已启用字符选择");
        }
        
        // 调整教学Panel位置，避免遮挡底部UI
        AdjustTutorialPanelPosition();
        Debug.Log("TutorialManager: HandleSelectAndSplit - 执行完成");
    }

    private void HandleAfterSplit()
    {
        Debug.Log("TutorialManager: HandleAfterSplit - 开始执行");
        
        DisablePlayerMovement();
        SetGuideExpression(exprHappy);
        hintText.text = "成功拆分！你获得了【片】和【枼】。";
        
        // 清理高亮UI元素
        ClearHighlightUI();

        // 恢复教学Panel的默认位置
        ResetTutorialPanelPosition();
        
        // 关闭教程模式，恢复ButtonController的正常行为
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.SetTutorialMode(false);
            Debug.Log("TutorialManager: HandleAfterSplit - 已关闭教程模式");
        }
        
        // 重新禁用字符选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.DisableCharacterSelection();
            Debug.Log("TutorialManager: HandleAfterSplit - 已重新禁用字符选择");
        }

        // 显示继续按钮，等待玩家点击
        continueButton.gameObject.SetActive(true);
        EnableUIInteraction(); // 启用UI交互以便继续按钮可用
        
        Debug.Log("TutorialManager: HandleAfterSplit - 执行完成");
    }

    private void HandleSelectAndCombine()
    {
        Debug.Log("TutorialManager: HandleSelectAndCombine - 开始执行");
        Debug.Log("TutorialManager: HandleSelectAndCombine - 第一行代码执行");
        Debug.Log("TutorialManager: HandleSelectAndCombine - 立即输出测试");
        
        // 启用教程模式，防止ButtonController自动启用拼按钮
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.SetTutorialMode(true);
            Debug.Log("TutorialManager: HandleSelectAndCombine - 已启用教程模式");
        }
        
        // 先禁用拼按钮，确保在显示提示之前就禁用
        if (combineButton != null)
        {
            combineButton.interactable = false;
            Debug.Log("TutorialManager: HandleSelectAndCombine - 已禁用拼按钮");
        }
        else
        {
            Debug.LogError("TutorialManager: HandleSelectAndCombine - combineButton为空!");
        }
        
        // 设置表情和提示文本
        SetGuideExpression(exprSideEye);
        hintText.text = "化蝶的部件已齐。请选中【虫】和【枼】，然后点击【拼】按钮。";
        
        // 高亮拼按钮（即使它是禁用的）
        if (combineButton != null)
        {
            Debug.Log($"TutorialManager: HandleSelectAndCombine - 调用HighlightUITarget，目标: {combineButton.name}");
            HighlightUITarget(combineButton.transform);
        }
        
        DisablePlayerMovement();
        EnableUIInteraction();

        // 在这个步骤中允许字符选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.EnableCharacterSelection();
            Debug.Log("TutorialManager: HandleSelectAndCombine - 已启用字符选择");
        }

        // 调整教学Panel位置，避免遮挡底部UI
        AdjustTutorialPanelPosition();
        Debug.Log("TutorialManager: HandleSelectAndCombine - 执行完成");
    }

    private void HandleAfterCombine()
    {
        SetGuideExpression(exprHappy);
        hintText.text = "太棒了！诗句完成了！";
        EnablePlayerMovement(1);

        // 清理高亮UI元素
        ClearHighlightUI();

        // 恢复教学Panel的默认位置
        ResetTutorialPanelPosition();

        // 关闭教程模式，恢复ButtonController的正常行为
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.SetTutorialMode(false);
            Debug.Log("TutorialManager: HandleAfterCombine - 已关闭教程模式");
        }
        
        // 重新禁用字符选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.DisableCharacterSelection();
            Debug.Log("TutorialManager: HandleAfterCombine - 已重新禁用字符选择");
        }

        // 显示继续按钮，等待玩家点击
        continueButton.gameObject.SetActive(true);
        EnableUIInteraction(); // 启用UI交互以便继续按钮可用
    }

    private void HandleCharacterFly()
    {
        SetGuideExpression(exprHappy);
        hintText.text = "蝴蝶破茧而出，翩翩起舞...";
        if (maskController != null)
        {
            maskController.DisableMask();
        }
        // 禁用玩家移动
        DisablePlayerMovement();
        EnableUIInteraction();
        
        // 禁用字符选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.DisableCharacterSelection();
        }
        
        // 隐藏继续按钮，使用自动触发
        continueButton.gameObject.SetActive(false);
        
        // 自动触发CharacterFlyButton的点击事件
        StartCoroutine(AutoTriggerCharacterFlyButton());
    }
    
    // 删除CharacterFlySequence协程，因为现在通过CharacterFlyButton点击事件来触发
    
    /// <summary>
    /// 自动触发CharacterFlyButton的点击事件
    /// </summary>
    private IEnumerator AutoTriggerCharacterFlyButton()
    {
        Debug.Log("TutorialManager: 开始自动触发CharacterFlyButton点击事件");
        
        // 等待一小段时间让玩家看到提示文字
        yield return new WaitForSeconds(1f);
        
        // 查找CharacterFlyButton组件
        CharacterFlyButton characterFlyButton = FindObjectOfType<CharacterFlyButton>();
        if (characterFlyButton != null)
        {
            Debug.Log("TutorialManager: 找到CharacterFlyButton，自动触发点击事件");
            
            // 通过反射调用私有方法OnFlyButtonClicked
            var method = typeof(CharacterFlyButton).GetMethod("OnFlyButtonClicked", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (method != null)
            {
                method.Invoke(characterFlyButton, null);
                Debug.Log("TutorialManager: 成功触发CharacterFlyButton的点击事件");
            }
            else
            {
                Debug.LogError("TutorialManager: 无法找到CharacterFlyButton的OnFlyButtonClicked方法");
                // 如果无法通过反射调用，则直接调用TutorialManager的方法
                OnCharacterFlyButtonClicked();
            }
        }
        else
        {
            Debug.LogWarning("TutorialManager: 未找到CharacterFlyButton组件，直接调用飞舞逻辑");
            // 如果没有找到CharacterFlyButton，则直接调用飞舞逻辑
            OnCharacterFlyButtonClicked();
        }
    }
    
    /// <summary>
    /// 处理CharacterFlyButton的点击事件
    /// 当玩家点击CharacterFlyButton时调用此方法
    /// </summary>
    public void OnCharacterFlyButtonClicked()
    {
        if (currentStep == TutorialStep.CharacterFly)
        {
            Debug.Log("TutorialManager: CharacterFlyButton被点击，开始文字飞舞动画");
            
            // 隐藏继续按钮，防止重复点击
            continueButton.gameObject.SetActive(false);
            
            // 开始文字飞舞动画
            StartCoroutine(CharacterFlyButtonSequence());
        }
        else
        {
            Debug.Log($"TutorialManager: CharacterFlyButton被点击，但当前步骤不是CharacterFly，当前步骤: {currentStep}");
        }
    }
    
    /// <summary>
    /// CharacterFlyButton点击后的文字飞舞序列
    /// </summary>
    private IEnumerator CharacterFlyButtonSequence()
    {
        Debug.Log("TutorialManager: 开始CharacterFlyButton文字飞舞序列");
        
        if (ButtonController.Instance != null)
        {
            // 查找CharacterFlyButton组件
            CharacterFlyButton characterFlyButton = FindObjectOfType<CharacterFlyButton>();
            if (characterFlyButton != null)
            {
                // 获取起始位置（按钮位置）和目标位置
                Vector2 startPosition = characterFlyButton.GetButtonPosition();
                Vector2 endPosition = characterFlyButton.GetTargetPosition();
                
                Debug.Log($"TutorialManager: 使用CharacterFlyButton位置 - 起始位置={startPosition}, 目标位置={endPosition}");
                
                // 从可用字符串列表中删除"蝶"字
                StringSelector stringSelector = ButtonController.Instance.GetStringSelector();
                if (stringSelector != null)
                {
                    stringSelector.RemoveAvailableString("蝶");
                    Debug.Log("TutorialManager: 从可用字符串列表中删除'蝶'字");
                }
                
                // 开始"蝶"字飞舞动画，起点为按钮位置
                ButtonController.Instance.StartLevel1CharacterFly("蝶", startPosition, endPosition);
                
                // 等待飞舞动画完成
                while (ButtonController.Instance.IsLevel1Flying())
                {
                    yield return new WaitForSeconds(0.1f);
                }
                
                Debug.Log("TutorialManager: CharacterFlyButton文字飞舞动画完成");
            }
            else
            {
                Debug.LogWarning("TutorialManager: 未找到CharacterFlyButton组件，使用默认逻辑");
                
                // 获取"蝶"字的目标位置
                Transform targetTransform = PublicData.GetTargetPositionForCharacter("蝶");
                Vector2 endPosition = Vector2.zero;
                
                if (targetTransform != null)
                {
                    // 直接获取UI坐标
                    RectTransform targetRectTransform = targetTransform as RectTransform;
                    if (targetRectTransform != null)
                    {
                        endPosition = targetRectTransform.anchoredPosition;
                    }
                    else
                    {
                        // 如果不是UI元素，尝试获取其子物体的RectTransform
                        RectTransform childRectTransform = targetTransform.GetComponentInChildren<RectTransform>();
                        if (childRectTransform != null)
                        {
                            endPosition = childRectTransform.anchoredPosition;
                        }
                    }
                    Debug.Log($"TutorialManager: 获取到'蝶'字目标位置: {endPosition}");
                }
                else
                {
                    Debug.LogWarning("TutorialManager: 未找到'蝶'字的目标位置，使用默认位置");
                }
                
                // 从可用字符串列表中删除"蝶"字
                StringSelector stringSelector = ButtonController.Instance.GetStringSelector();
                if (stringSelector != null)
                {
                    stringSelector.RemoveAvailableString("蝶");
                    Debug.Log("TutorialManager: 从可用字符串列表中删除'蝶'字");
                }
                
                // 开始"蝶"字飞舞动画，起点为屏幕正中间
                ButtonController.Instance.StartLevel1CharacterFly("蝶", Vector2.zero, endPosition);
                
                // 等待飞舞动画完成
                while (ButtonController.Instance.IsLevel1Flying())
                {
                    yield return new WaitForSeconds(0.1f);
                }
                
                Debug.Log("TutorialManager: 默认文字飞舞动画完成");
            }
        }
        else
        {
            Debug.LogWarning("TutorialManager: 未找到 ButtonController 实例，跳过飞舞动画");
        }
        
        // 动画完成后自动进入下一步
        yield return new WaitForSeconds(0.5f);
        GoToNextStep();
    }

    private void HandleEndPart1()
    {
        DisablePlayerMovement();
        
        // 禁用字符选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.DisableCharacterSelection();
        }
        
        SetGuideExpression(exprHappy);
        hintText.text = "你成功让梁祝的缘分，在此刻凝结。";
        continueButton.gameObject.SetActive(true);
    }

    private void HandleEndPart2()
    {
        DisablePlayerMovement();
        
        // 禁用字符选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.DisableCharacterSelection();
        }
        
        SetGuideExpression(exprNormal);
        hintText.text = "但他们的故事尚未结束，正如言界中还有许多破碎的篇章等待织补...";
        continueButton.gameObject.SetActive(true);
    }

    private void HandleEndPart3()
    {
        DisablePlayerMovement();
        
        // 禁用字符选择
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.DisableCharacterSelection();
        }
        
        SetGuideExpression(exprHappy);
        hintText.text = "下一章，我们将进入【牛郎】与【织女】的星河，感受那份跨越天际的守望。";
        continueButton.GetComponentInChildren<TextMeshProUGUI>().text = "完成教学";
        continueButton.gameObject.SetActive(true);
    }
    #endregion

    #region Core Logic
    private void InitializeUI()
    {
        tutorialPanel.SetActive(true);
        continueButton.gameObject.SetActive(false);
        // highlightBox.gameObject.SetActive(false);
        arrowImage.gameObject.SetActive(false);

        // 默认禁用UI交互，除非特定步骤需要
        DisableUIInteraction();
    }

    // 事件处理方法
    private IEnumerator AutoAdvance(float delay)
    {
        yield return new WaitForSeconds(delay);
        GoToNextStep();
    }

    // 延迟显示继续按钮的协程
    private IEnumerator DelayedShowContinueButton(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 显示继续按钮，等待玩家点击
        continueButton.gameObject.SetActive(true);
        EnableUIInteraction(); // 启用UI交互以便继续按钮可用

        Debug.Log("TutorialManager: 延迟显示继续按钮完成");
    }

    // 延迟禁用玩家移动的协程
    private IEnumerator DelayedDisablePlayerMovement()
    {
        // 等待一帧，确保PlayerController已完全初始化
        yield return null;
        
        // 禁止玩家移动，确保游戏开始时玩家无法移动
        DisablePlayerMovement();
        DisablePlayerSwitching(); // 禁用玩家切换
        DisableEnterKey(); // 禁用回车键响应
        
        Debug.Log("TutorialManager: 延迟禁用玩家移动和回车键响应完成");
    }

    public void OnPlayerTransformed()
    {
        if (currentStep == TutorialStep.MoveToDog)
        {
            // 检查当前玩家的carryLetter是否为"伏"
            if (playerController != null && playerController.GetCurrentPlayer() != null)
            {
                Player currentPlayer = playerController.GetCurrentPlayer();
                if (currentPlayer.CarryCharacter == "伏")
                {
                    Debug.Log("TutorialManager: 玩家获得'伏'字，显示继续按钮");
                    // 玩家已经获得"伏"字，显示继续按钮
                    continueButton.gameObject.SetActive(true);
                    EnableUIInteraction();
                }
                else
                {
                    Debug.Log($"TutorialManager: 玩家当前携带字符为 '{currentPlayer.CarryCharacter}'，需要获得'伏'字");
                }
            }
            else
            {
                Debug.LogWarning("TutorialManager: 无法获取当前玩家信息");
            }
        }
    }

    public void OnWordAcquired(string word)
    {
        if (word == "虫" && currentStep == TutorialStep.MoveToGrass)
        {
            Debug.Log("TutorialManager: 获得'虫'字，但需要等待点击继续按钮");
            // 不自动跳转，等待玩家点击继续按钮
        }
        if (word == "牒" && currentStep == TutorialStep.MoveToDie)
        {
            Debug.Log("TutorialManager: 获得'牒'字，但需要等待点击继续按钮");
            // 不自动跳转，等待玩家点击继续按钮
        }
    }

    public void OnSplitSuccess(string sourceWord)
    {
        if (sourceWord == "牒" && currentStep == TutorialStep.SelectAndSplit)
        {
            Debug.Log("TutorialManager: 拆分'牒'字成功，自动跳转到下一步");
            // 自动跳转到下一步
            GoToNextStep();
        }
    }

    public void OnCombineSuccess(string resultWord)
    {
        Debug.Log($"TutorialManager: OnCombineSuccess - 被调用，参数: {resultWord}，当前步骤: {currentStep}");
        if (resultWord == "蝶" && currentStep == TutorialStep.SelectAndCombine)
        {
            Debug.Log("TutorialManager: 合成'蝶'字成功，自动跳转到下一步");
            // 自动跳转到下一步
            GoToNextStep();
        }
        else
        {
            Debug.Log($"TutorialManager: OnCombineSuccess - 条件不满足，resultWord: {resultWord}, currentStep: {currentStep}");
        }
    }
    
    /// <summary>
    /// 处理字符选择变化，在教程步骤中控制按钮的启用状态
    /// </summary>
    public void OnCharacterSelectionChanged()
    {
        // 在教程模式中，只有在特定步骤才允许字符选择
        if (ButtonController.Instance != null && ButtonController.Instance.IsTutorialMode())
        {
            Debug.Log("TutorialManager: 当前处于教程模式，检查是否允许字符选择");
            
            // 只有在SelectAndSplit和SelectAndCombine步骤中才允许字符选择
            if (currentStep != TutorialStep.SelectAndSplit && currentStep != TutorialStep.SelectAndCombine)
            {
                Debug.Log("TutorialManager: 当前步骤不允许字符选择，禁用所有字符按钮");
                ButtonController.Instance.DisableCharacterSelection();
                return;
            }
        }
        
        if (currentStep == TutorialStep.SelectAndSplit)
        {
            // 检查是否选择了正确的字符（"牒"）
            if (ButtonController.Instance != null && ButtonController.Instance.GetStringSelector() != null)
            {
                var selectedStrings = ButtonController.Instance.GetStringSelector().SelectedStrings;
                bool hasDie = selectedStrings.Contains("牒");
                
                Debug.Log($"TutorialManager: 字符选择变化 - 选中字符: [{string.Join(", ", selectedStrings)}], 有牒: {hasDie}");
                
                // 只有当选中"牒"时才启用拆按钮
                if (hasDie && selectedStrings.Count == 1)
                {
                    if (splitButton != null)
                    {
                        splitButton.interactable = true;
                        Debug.Log("TutorialManager: 已启用拆按钮（选中了牒）");
                    }
                }
                else
                {
                    if (splitButton != null)
                    {
                        splitButton.interactable = false;
                        Debug.Log("TutorialManager: 已禁用拆按钮（未选中正确的字符）");
                    }
                }
            }
        }
        else if (currentStep == TutorialStep.SelectAndCombine)
        {
            // 检查是否选择了正确的字符（"虫"和"枼"）
            if (ButtonController.Instance != null && ButtonController.Instance.GetStringSelector() != null)
            {
                var selectedStrings = ButtonController.Instance.GetStringSelector().SelectedStrings;
                bool hasChong = selectedStrings.Contains("虫");
                bool hasYe = selectedStrings.Contains("枼");
                
                Debug.Log($"TutorialManager: 字符选择变化 - 选中字符: [{string.Join(", ", selectedStrings)}], 有虫: {hasChong}, 有枼: {hasYe}");
                
                // 只有当同时选中"虫"和"枼"时才启用拼按钮
                if (hasChong && hasYe && selectedStrings.Count == 2)
                {
                    if (combineButton != null)
                    {
                        combineButton.interactable = true;
                        Debug.Log("TutorialManager: 已启用拼按钮（选中了虫和枼）");
                    }
                }
                else
                {
                    if (combineButton != null)
                    {
                        combineButton.interactable = false;
                        Debug.Log("TutorialManager: 已禁用拼按钮（未选中正确的字符）");
                    }
                }
            }
        }
    }

    public void OnPlayerSwitched()
    {
        if (currentStep == TutorialStep.SwitchPlayer_Part2)
        {
            Debug.Log("TutorialManager: 玩家切换成功，但需要等待点击继续按钮");
            // 不自动跳转，等待玩家点击继续按钮
        }
    }

    #endregion

    #region Broadcast Handling
    // 确保广播管理器存在
    private void EnsureBroadcastManagerExists()
    {
        if (BroadcastManager.Instance == null)
        {
            // 创建空对象
            GameObject managerObject = new GameObject("BroadcastManager");

            // 添加BroadcastManager组件
            BroadcastManager manager = managerObject.AddComponent<BroadcastManager>();

            Debug.Log("TutorialManager: 已创建广播管理器");
        }
        else
        {
            Debug.Log("TutorialManager: 广播管理器已存在");
        }
    }
    
    // 加载下一个场景
    private void LoadNextScene()
    {
        Debug.Log("TutorialManager: 开始加载下一个场景");
        
        // 隐藏教学面板
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
        
        // 在切换到level2之前，启用所有操作
        EnableAllOperations();
        
        // 加载 Level2 场景
        SceneManager.LoadScene("level2");
    }
    
    // 启用所有操作（移动、切换、回车、空格）
    private void EnableAllOperations()
    {
        Debug.Log("TutorialManager: 准备进入level2，操作将由Level2Manager处理");
        
        // 在level2场景中，Level2Manager会自动启用所有操作
        // 这里只需要确保教学面板被隐藏
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
        
        // 重新启用字符选择功能
        if (ButtonController.Instance != null)
        {
            ButtonController.Instance.EnableCharacterSelection();
            Debug.Log("TutorialManager: 教程结束时已重新启用字符选择");
        }
        
        // 重新启用颜色管理功能
        if (playerController != null)
        {
            playerController.EnableColorManagement();
            Debug.Log("TutorialManager: 教程结束时已重新启用颜色管理");
        }
    }
    
    // 清理高亮UI元素
    private void ClearHighlightUI()
    {
        // if (highlightBox != null)
        // {
        //     highlightBox.gameObject.SetActive(false);
        // }
        if (arrowImage != null)
        {
            arrowImage.gameObject.SetActive(false);
        }
        Debug.Log("TutorialManager: 清理高亮UI元素");
    }

    // 接收广播的方法
    public void ReceiveBroadcast(string broadcastedValue)
    {
        Debug.Log($"TutorialManager: 接收到广播: {broadcastedValue}");

        // 当收到"伏"字的广播时，执行OnPlayerTransformed
        if (broadcastedValue == "伏")
        {
            Debug.Log("TutorialManager: 收到'伏'字广播，执行OnPlayerTransformed");
            OnPlayerTransformed();
        }
        // 当收到拆分成功的广播时，执行OnSplitSuccess
        else if (broadcastedValue == "split_success")
        {
            Debug.Log("TutorialManager: 收到拆分成功广播，执行OnSplitSuccess");
            OnSplitSuccess("牒");
        }
        // 当收到组合成功的广播时，执行OnCombineSuccess
        else if (broadcastedValue == "combine_success")
        {
            Debug.Log("TutorialManager: 收到组合成功广播，执行OnCombineSuccess");
            OnCombineSuccess("蝶");
        }
    }

    // 测试广播功能的方法（可在Inspector中调用）
    [ContextMenu("测试'伏'字广播")]
    public void TestFuBroadcast()
    {
        if (BroadcastManager.Instance != null)
        {
            Debug.Log("TutorialManager: 测试发送'伏'字广播");
            BroadcastManager.Instance.BroadcastToAll("伏");
        }
        else
        {
            Debug.LogError("TutorialManager: 广播管理器不存在，无法测试广播");
        }
    }

    // 测试玩家切换功能的方法（可在Inspector中调用）
    [ContextMenu("测试禁用玩家切换")]
    public void TestDisablePlayerSwitching()
    {
        DisablePlayerSwitching();
        Debug.Log("TutorialManager: 已禁用玩家切换");
    }

    [ContextMenu("测试启用玩家切换")]
    public void TestEnablePlayerSwitching()
    {
        EnablePlayerSwitching();
        Debug.Log("TutorialManager: 已启用玩家切换");
    }

    [ContextMenu("测试玩家切换事件")]
    public void TestPlayerSwitched()
    {
        Debug.Log("TutorialManager: 测试玩家切换事件");
        OnPlayerSwitched();
    }

    [ContextMenu("测试合成'蝶'字成功")]
    public void TestCombineDieSuccess()
    {
        Debug.Log("TutorialManager: 测试合成'蝶'字成功");
        OnCombineSuccess("蝶");
    }

    [ContextMenu("测试事件系统")]
    public void TestEventSystem()
    {
        Debug.Log($"TutorialManager: 当前步骤: {currentStep}");
        Debug.Log($"TutorialManager: 注册的处理函数数量: {stepHandlers.Count}");
        Debug.Log($"TutorialManager: 注册的转换规则数量: {stepTransitions.Count}");

        if (stepHandlers.ContainsKey(currentStep))
        {
            Debug.Log($"TutorialManager: 当前步骤的处理函数存在");
        }
        else
        {
            Debug.LogError($"TutorialManager: 当前步骤的处理函数不存在");
        }
    }

    [ContextMenu("测试延迟显示继续按钮")]
    public void TestDelayedShowContinueButton()
    {
        Debug.Log("TutorialManager: 测试延迟显示继续按钮");
        StartCoroutine(DelayedShowContinueButton(1f));
    }

    [ContextMenu("检查玩家携带字符")]
    public void CheckPlayerCarryCharacter()
    {
        if (playerController != null && playerController.GetCurrentPlayer() != null)
        {
            Player currentPlayer = playerController.GetCurrentPlayer();
            Debug.Log($"TutorialManager: 当前玩家携带字符为 '{currentPlayer.CarryCharacter}'");
        }
        else
        {
            Debug.LogWarning("TutorialManager: 无法获取当前玩家信息");
        }
    }
    #endregion

    #region Player Movement Control
    // 禁止玩家移动
    public void DisablePlayerMovement()
    {
        if (playerController != null)
        {
            playerController.DisableCurrentPlayerMovement();
            Debug.Log("TutorialManager: 已禁用玩家移动");
        }
        else
        {
            Debug.LogWarning("TutorialManager: PlayerController 为 null，无法禁用玩家移动");
        }
        

    }

    // 恢复玩家移动
    private void EnablePlayerMovement(int playerIndex = 0)
    {
        if (playerController != null)
        {
            // 设置指定玩家为当前操作角色
            playerController.SetCurrentPlayerIndex(playerIndex);
            playerController.EnableCurrentPlayerMovement();
        }
    }

    // 禁用玩家切换
    private void DisablePlayerSwitching()
    {
        if (playerController != null)
        {
            playerController.DisablePlayerSwitching();
        }
    }

    // 启用玩家切换
    private void EnablePlayerSwitching()
    {
        if (playerController != null)
        {
            playerController.EnablePlayerSwitching();
        }
    }
    
    // 禁用回车键响应
    private void DisableEnterKey()
    {
        if (playerController != null && playerController.GetCurrentPlayer() != null)
        {
            Player currentPlayer = playerController.GetCurrentPlayer();
            currentPlayer.SetEnterKeyEnabled(false);
            Debug.Log("TutorialManager: 已禁用回车键响应");
        }
    }
    
    // 启用回车键响应
    private void EnableEnterKey()
    {
        if (playerController != null && playerController.GetCurrentPlayer() != null)
        {
            Player currentPlayer = playerController.GetCurrentPlayer();
            currentPlayer.SetEnterKeyEnabled(true);
            Debug.Log("TutorialManager: 已启用回车键响应");
        }
    }
    #endregion

    #region UI Control
    // 禁用UI交互
    private void DisableUIInteraction()
    {
        // 禁用指定的UI按钮
        if (uiButtonsToDisable != null)
        {
            foreach (Button button in uiButtonsToDisable)
            {
                if (button != null)
                {
                    button.interactable = false;
                }
            }
        }

        // 激活蒙层来阻止UI点击
        if (tutorialMask != null)
        {
            tutorialMask.SetActive(true);
            
            // 获取Image组件并启用Raycast Target
            UnityEngine.UI.Image maskImage = tutorialMask.GetComponent<UnityEngine.UI.Image>();
            if (maskImage != null)
            {
                maskImage.raycastTarget = true;
                Debug.Log("TutorialManager: 已启用tutorialMask的Raycast Target");
            }
        }

        Debug.Log("TutorialManager: 禁用UI交互");
    }

    // 启用UI交互
    private void EnableUIInteraction()
    {
        // 启用指定的UI按钮
        if (uiButtonsToDisable != null)
        {
            foreach (Button button in uiButtonsToDisable)
            {
                if (button != null)
                {
                    button.interactable = true;
                }
            }
        }

        // 隐藏蒙层并禁用其Raycast Target
        if (tutorialMask != null)
        {
            tutorialMask.SetActive(false);
            
            // 获取Image组件并禁用Raycast Target
            UnityEngine.UI.Image maskImage = tutorialMask.GetComponent<UnityEngine.UI.Image>();
            if (maskImage != null)
            {
                maskImage.raycastTarget = false;
                Debug.Log("TutorialManager: 已禁用tutorialMask的Raycast Target");
            }
        }

        Debug.Log("TutorialManager: 启用UI交互");
    }
    #endregion

    #region Font Settings
    // 设置中文字体
    private void SetChineseFont()
    {
        // 如果没有设置中文字体，尝试从StringSelector获取
        if (chineseFont == null)
        {
            StringSelector stringSelector = FindObjectOfType<StringSelector>();
            if (stringSelector != null)
            {
                chineseFont = stringSelector.GetChineseFont();
                Debug.Log("TutorialManager: 从StringSelector获取中文字体");
            }
        }
        
        if (chineseFont != null)
        {
            // 设置提示文本的字体
            if (hintText != null)
            {
                hintText.font = chineseFont;
                hintText.ForceMeshUpdate();
            }

            // 设置继续按钮文本的字体
            TextMeshProUGUI continueButtonText = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (continueButtonText != null)
            {
                continueButtonText.font = chineseFont;
                continueButtonText.ForceMeshUpdate();
            }

            Debug.Log("TutorialManager: 中文字体设置完成");
        }
        else
        {
            Debug.LogWarning("TutorialManager: 未设置中文字体资源，尝试加载默认字体");
            // 尝试加载默认的中文字体
            TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts/SourceHanSerifCN-Heavy SDF 1");
            if (defaultFont != null)
            {
                chineseFont = defaultFont;
                if (hintText != null)
                {
                    hintText.font = chineseFont;
                    hintText.ForceMeshUpdate();
                }
                Debug.Log("TutorialManager: 使用默认中文字体");
            }
        }
    }
    #endregion

    #region Helper Methods
    // 调整教学Panel，禁用所有遮挡元素的Raycast Target避免拦截点击事件
    private void AdjustTutorialPanelPosition()
    {
        if (tutorialPanel != null)
        {
            // 禁用Panel的Raycast Target，让鼠标点击穿透到下面的UI
            UnityEngine.UI.Image panelImage = tutorialPanel.GetComponent<UnityEngine.UI.Image>();
            if (panelImage != null)
            {
                panelImage.raycastTarget = false;
                Debug.Log("TutorialManager: 已禁用教学Panel的Raycast Target");
            }
            
            // 同时禁用Panel内所有子对象的Raycast Target
            UnityEngine.UI.Image[] childImages = tutorialPanel.GetComponentsInChildren<UnityEngine.UI.Image>();
            foreach (UnityEngine.UI.Image childImage in childImages)
            {
                childImage.raycastTarget = false;
            }
            
            Debug.Log($"TutorialManager: 已禁用教学Panel及其{childImages.Length}个子对象的Raycast Target");
        }
        
        // 禁用所有可能遮挡的UI元素的Raycast Target
        DisableAllBlockingUIElements();
    }
    
    // 禁用所有可能遮挡的UI元素的Raycast Target
    private void DisableAllBlockingUIElements()
    {
        // 查找场景中所有的Canvas
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        
        foreach (Canvas canvas in allCanvases)
        {
            // 检查Canvas内的所有UI元素
            UnityEngine.UI.Image[] allImages = canvas.GetComponentsInChildren<UnityEngine.UI.Image>();
            UnityEngine.UI.Button[] allButtons = canvas.GetComponentsInChildren<UnityEngine.UI.Button>();
            UnityEngine.UI.Text[] allTexts = canvas.GetComponentsInChildren<UnityEngine.UI.Text>();
            TMPro.TextMeshProUGUI[] allTMPTexts = canvas.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            
            int disabledCount = 0;
            
            // 禁用所有Image的Raycast Target（除了按钮和重要的交互元素）
            foreach (UnityEngine.UI.Image image in allImages)
            {
                // 跳过按钮的Image组件
                if (image.GetComponent<UnityEngine.UI.Button>() != null)
                    continue;
                    
                // 跳过重要的交互元素（如"牒"字按钮、"拆"按钮等）
                if (image.name.Contains("牒") || image.name.Contains("拆") || image.name.Contains("拼"))
                    continue;
                    
                // 跳过底部UI区域的元素
                if (image.transform.position.y < Screen.height * 0.3f)
                    continue;
                
                image.raycastTarget = false;
                disabledCount++;
            }
            
            // 禁用所有Text的Raycast Target
            foreach (UnityEngine.UI.Text text in allTexts)
            {
                text.raycastTarget = false;
                disabledCount++;
            }
            
            // 禁用所有TMP Text的Raycast Target
            foreach (TMPro.TextMeshProUGUI tmpText in allTMPTexts)
            {
                tmpText.raycastTarget = false;
                disabledCount++;
            }
            
            Debug.Log($"TutorialManager: 在Canvas '{canvas.name}' 中禁用了 {disabledCount} 个UI元素的Raycast Target");
        }
        
        // 特别禁用Circle和Arrow的Raycast Target
        DisableSpecificUIElements();
    }
    
    // 特别禁用Circle和Arrow的Raycast Target
    private void DisableSpecificUIElements()
    {
        // 查找所有包含"Circle"或"Arrow"名称的UI元素
        UnityEngine.UI.Image[] allImages = FindObjectsOfType<UnityEngine.UI.Image>();
        
        foreach (UnityEngine.UI.Image image in allImages)
        {
            string imageName = image.name.ToLower();
            
            // 检查是否包含Circle或Arrow关键词
            if (imageName.Contains("circle") || imageName.Contains("arrow") || 
                imageName.Contains("圈") || imageName.Contains("箭头"))
            {
                image.raycastTarget = false;
                Debug.Log($"TutorialManager: 已禁用 {image.name} 的Raycast Target");
            }
        }
        
        // 特别处理highlightBox（高亮圈）
        // if (highlightBox != null)
        // {
        //     UnityEngine.UI.Image highlightImage = highlightBox.GetComponent<UnityEngine.UI.Image>();
        //     if (highlightImage != null)
        //     {
        //         highlightImage.raycastTarget = false;
        //         Debug.Log("TutorialManager: 已禁用highlightBox的Raycast Target");
        //     }
        // }
        
        // 特别处理arrowImage（箭头）
        if (arrowImage != null)
        {
            arrowImage.raycastTarget = false;
            Debug.Log("TutorialManager: 已禁用arrowImage的Raycast Target");
        }
    }
    
    // 恢复Circle和Arrow的Raycast Target
    private void RestoreSpecificUIElements()
    {
        // 查找所有包含"Circle"或"Arrow"名称的UI元素
        UnityEngine.UI.Image[] allImages = FindObjectsOfType<UnityEngine.UI.Image>();
        
        foreach (UnityEngine.UI.Image image in allImages)
        {
            string imageName = image.name.ToLower();
            
            // 检查是否包含Circle或Arrow关键词
            if (imageName.Contains("circle") || imageName.Contains("arrow") || 
                imageName.Contains("圈") || imageName.Contains("箭头"))
            {
                image.raycastTarget = true;
                Debug.Log($"TutorialManager: 已恢复 {image.name} 的Raycast Target");
            }
        }
        
        // 特别处理highlightBox（高亮圈）
        // if (highlightBox != null)
        // {
        //     UnityEngine.UI.Image highlightImage = highlightBox.GetComponent<UnityEngine.UI.Image>();
        //     if (highlightImage != null)
        //     {
        //         highlightImage.raycastTarget = true;
        //         Debug.Log("TutorialManager: 已恢复highlightBox的Raycast Target");
        //     }
        // }
        
        // 特别处理arrowImage（箭头）
        if (arrowImage != null)
        {
            arrowImage.raycastTarget = true;
            Debug.Log("TutorialManager: 已恢复arrowImage的Raycast Target");
        }
    }
    
    // 恢复所有UI元素的Raycast Target
    private void RestoreAllBlockingUIElements()
    {
        // 查找场景中所有的Canvas
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        
        foreach (Canvas canvas in allCanvases)
        {
            // 恢复所有UI元素的Raycast Target
            UnityEngine.UI.Image[] allImages = canvas.GetComponentsInChildren<UnityEngine.UI.Image>();
            UnityEngine.UI.Text[] allTexts = canvas.GetComponentsInChildren<UnityEngine.UI.Text>();
            TMPro.TextMeshProUGUI[] allTMPTexts = canvas.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            
            int restoredCount = 0;
            
            // 恢复所有Image的Raycast Target
            foreach (UnityEngine.UI.Image image in allImages)
            {
                image.raycastTarget = true;
                restoredCount++;
            }
            
            // 恢复所有Text的Raycast Target
            foreach (UnityEngine.UI.Text text in allTexts)
            {
                text.raycastTarget = true;
                restoredCount++;
            }
            
            // 恢复所有TMP Text的Raycast Target
            foreach (TMPro.TextMeshProUGUI tmpText in allTMPTexts)
            {
                tmpText.raycastTarget = true;
                restoredCount++;
            }
            
            Debug.Log($"TutorialManager: 在Canvas '{canvas.name}' 中恢复了 {restoredCount} 个UI元素的Raycast Target");
        }
        
        // 特别恢复Circle和Arrow的Raycast Target
        RestoreSpecificUIElements();
    }
    
    // 恢复教学Panel的Raycast Target
    private void ResetTutorialPanelPosition()
    {
        if (tutorialPanel != null)
        {
            // 恢复Panel的Raycast Target
            UnityEngine.UI.Image panelImage = tutorialPanel.GetComponent<UnityEngine.UI.Image>();
            if (panelImage != null)
            {
                panelImage.raycastTarget = true;
                Debug.Log("TutorialManager: 已恢复教学Panel的Raycast Target");
            }
            
            // 恢复Panel内所有子对象的Raycast Target
            UnityEngine.UI.Image[] childImages = tutorialPanel.GetComponentsInChildren<UnityEngine.UI.Image>();
            foreach (UnityEngine.UI.Image childImage in childImages)
            {
                childImage.raycastTarget = true;
            }
            
            Debug.Log($"TutorialManager: 已恢复教学Panel及其{childImages.Length}个子对象的Raycast Target");
        }
        
        // 恢复所有UI元素的Raycast Target
        RestoreAllBlockingUIElements();
    }
    // 检查组件引用
    private void CheckComponentReferences()
    {
        if (arrowImage == null)
            Debug.LogError("TutorialManager: arrowImage 未设置!");
            
        // if (highlightBox == null)
        //     Debug.LogError("TutorialManager: highlightBox 未设置!");
            
        if (guideCharacterImage == null)
            Debug.LogError("TutorialManager: guideCharacterImage 未设置!");
            
        if (arrowLeft == null)
            Debug.LogError("TutorialManager: arrowLeft 未设置!");
            
        if (arrowDownLeft == null)
            Debug.LogError("TutorialManager: arrowDownLeft 未设置!");
            
        if (dogObject == null)
            Debug.LogWarning("TutorialManager: dogObject 未设置!");
            
        if (grassObject == null)
            Debug.LogWarning("TutorialManager: grassObject 未设置!");
            
        if (dieObject == null)
            Debug.LogWarning("TutorialManager: dieObject 未设置!");
    }
    
    // 设置引导人物表情
    private void SetGuideExpression(Sprite expression)
    {
        if (guideCharacterImage != null && expression != null)
        {
            guideCharacterImage.sprite = expression;
        }
    }

    // 指向场景中的世界物体 (箭头紧挨高亮圈，尾部对着引导人物)
    private void PointAtTarget(Transform target)
    {
        if (arrowImage == null || guideCharacterImage == null || target == null)
        {
            Debug.LogWarning("TutorialManager: PointAtTarget - 缺少必要的组件引用");
            return;
        }

        // 首先显示高亮圈在目标物体上
        // HighlightUITarget(target);
        
        arrowImage.gameObject.SetActive(true);
        
        // 获取引导人物的屏幕坐标
        Vector3 guideScreenPos = Camera.main.WorldToScreenPoint(guideCharacterImage.transform.position);
        
        // 获取目标的屏幕坐标
        Vector3 targetScreenPos = Camera.main.WorldToScreenPoint(target.position);
        
        // 计算从引导人物到目标的方向向量
        Vector3 direction = (targetScreenPos - guideScreenPos).normalized;

        // 计算角度
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 将箭头放置在目标附近，紧挨着高亮圈
        // 根据方向计算箭头的偏移位置，指向高亮圈的边缘
        float arrowOffset = 80f*2; // 箭头距离目标的偏移距离，增大以指向高亮圈边缘
        Vector3 arrowPosition = targetScreenPos + direction * arrowOffset;
        arrowImage.transform.position = arrowPosition;

        // 决定是否水平翻转
        if (angle > 90 || angle < -90)
        {
            arrowImage.transform.localScale = new Vector3(-1, 1, 1); // 向左指，翻转
            angle += 180; // 修正角度
        }
        else
        {
            arrowImage.transform.localScale = new Vector3(1, 1, 1); // 向右指，不翻转
        }

        // 根据角度的绝对值决定使用哪个Sprite
        if (Mathf.Abs(angle) > 45) // 角度较大，更偏向下方
        {
            arrowImage.sprite = arrowDownLeft;
        }
        else // 角度较小，更偏向水平
        {
            arrowImage.sprite = arrowLeft;
        }

        arrowImage.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // 高亮UI元素或世界物体
    private void HighlightUITarget(Transform target)
    {
        // if (highlightBox == null || target == null)
        // {
        //     Debug.LogWarning("TutorialManager: HighlightUITarget - 缺少必要的组件引用");
        //     return;
        // }

        // highlightBox.gameObject.SetActive(true);
        arrowImage.gameObject.SetActive(true);
        
        // 检查目标是否是UI元素
        RectTransform targetRect = target.GetComponent<RectTransform>();
        if (targetRect != null)
        {
            // UI元素：直接使用RectTransform位置
            // highlightBox.position = target.position;
            // Vector2 size = targetRect.sizeDelta + new Vector2(20, 20);
            // highlightBox.sizeDelta = size;
            
            // 为UI元素设置箭头位置
            SetupArrowForUI(targetRect);
        }
        else
        {
            // 世界物体：转换为屏幕坐标
            Vector3 targetScreenPos = Camera.main.WorldToScreenPoint(target.position);
            // highlightBox.position = targetScreenPos;
            
            // 为世界物体设置默认大小
            // Vector2 defaultSize = new Vector2(150, 150); // 增大默认高亮框大小
            // highlightBox.sizeDelta = defaultSize;
            
            // 为世界物体设置箭头位置
            SetupArrowForWorldObject(targetScreenPos);
        }
    }
    
    // 为UI元素设置箭头位置
    private void SetupArrowForUI(RectTransform targetRect)
    {
        if (arrowImage == null || guideCharacterImage == null)
        {
            Debug.LogWarning("TutorialManager: SetupArrowForUI - 缺少必要的组件引用");
            return;
        }
        
        // 获取引导人物的屏幕坐标
        Vector3 guideScreenPos = Camera.main.WorldToScreenPoint(guideCharacterImage.transform.position);
        
        // 获取UI元素的屏幕坐标
        Vector3 targetScreenPos = targetRect.position;
        
        // 计算从引导人物到目标的方向向量
        Vector3 direction = (targetScreenPos - guideScreenPos).normalized;
        
        // 计算角度
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // 将箭头放置在目标附近，紧挨着高亮圈
        float arrowOffset = 80f; // 箭头距离目标的偏移距离
        Vector3 arrowPosition = targetScreenPos + direction * arrowOffset;
        arrowImage.transform.position = arrowPosition;
        
        // 决定是否水平翻转
        if (angle > 90 || angle < -90)
        {
            arrowImage.transform.localScale = new Vector3(-1, 1, 1); // 向左指，翻转
            angle += 180; // 修正角度
        }
        else
        {
            arrowImage.transform.localScale = new Vector3(1, 1, 1); // 向右指，不翻转
        }
        
        // 根据角度的绝对值决定使用哪个Sprite
        if (Mathf.Abs(angle) > 45) // 角度较大，更偏向下方
        {
            arrowImage.sprite = arrowDownLeft;
        }
        else // 角度较小，更偏向水平
        {
            arrowImage.sprite = arrowLeft;
        }
        
        arrowImage.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    // 为世界物体设置箭头位置
    private void SetupArrowForWorldObject(Vector3 targetScreenPos)
    {
        if (arrowImage == null || guideCharacterImage == null)
        {
            Debug.LogWarning("TutorialManager: SetupArrowForWorldObject - 缺少必要的组件引用");
            return;
        }
        
        // 获取引导人物的屏幕坐标
        Vector3 guideScreenPos = Camera.main.WorldToScreenPoint(guideCharacterImage.transform.position);
        
        // 计算从引导人物到目标的方向向量
        Vector3 direction = (targetScreenPos - guideScreenPos).normalized;
        
        // 计算角度
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // 将箭头放置在目标附近，紧挨着高亮圈
        float arrowOffset = 80f; // 箭头距离目标的偏移距离
        Vector3 arrowPosition = targetScreenPos + direction * arrowOffset;
        arrowImage.transform.position = arrowPosition;
        
        // 决定是否水平翻转
        if (angle > 90 || angle < -90)
        {
            arrowImage.transform.localScale = new Vector3(-1, 1, 1); // 向左指，翻转
            angle += 180; // 修正角度
        }
        else
        {
            arrowImage.transform.localScale = new Vector3(1, 1, 1); // 向右指，不翻转
        }
        
        // 根据角度的绝对值决定使用哪个Sprite
        if (Mathf.Abs(angle) > 45) // 角度较大，更偏向下方
        {
            arrowImage.sprite = arrowDownLeft;
        }
        else // 角度较小，更偏向水平
        {
            arrowImage.sprite = arrowLeft;
        }
        
        arrowImage.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    // 检查是否在MoveToGrass步骤中
    public bool IsInMoveToGrassStep()
    {
        return currentStep == TutorialStep.MoveToGrass;
    }
    
    // 检查是否在MoveToDie步骤中
    public bool IsInMoveToDieStep()
    {
        return currentStep == TutorialStep.MoveToDie;
    }
    
    // 虫显示通知方法
    public void OnChongShown()
    {
        Debug.Log("TutorialManager: 收到虫显示通知");
        chongShown = true;
    }
    
    // 牒显示通知方法
    public void OnDieShown()
    {
        Debug.Log("TutorialManager: 收到牒显示通知，设置dieShown为true");
        dieShown = true;
        Debug.Log($"TutorialManager: dieShown状态: {dieShown}");
    }
    #endregion
}