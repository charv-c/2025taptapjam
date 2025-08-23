using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    public RectTransform highlightBox;
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

    [Header("需要禁用的UI元素")]
    public Button[] uiButtonsToDisable; // 需要禁用的UI按钮数组
    #endregion

    #region Private State
    private enum TutorialStep { Welcome_Part1, Welcome_Part2, Welcome_Part3, MoveToDog, AfterTransform, MoveToGrass, AfterGetChong, SwitchPlayer_Part1, SwitchPlayer_Part2, MoveToDie, AfterGetDie, SelectAndSplit, AfterSplit, SelectAndCombine, AfterCombine, End_Part1, End_Part2, End_Part3 }
    private TutorialStep currentStep;
    private PlayerController playerController; // 引用PlayerController来控制玩家移动

    // 事件系统
    private Dictionary<TutorialStep, System.Action> stepHandlers;
    private Dictionary<TutorialStep, TutorialStep> stepTransitions;
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

        // 延迟一帧后禁止玩家移动，确保PlayerController已完全初始化
        StartCoroutine(DelayedDisablePlayerMovement());

        tutorialMask.SetActive(true); // 在教程开始时激活蒙层
        currentStep = TutorialStep.Welcome_Part1;
        ExecuteCurrentStep();
        continueButton.onClick.AddListener(GoToNextStep);

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
        stepHandlers[TutorialStep.End_Part1] = HandleEndPart1;
        stepHandlers[TutorialStep.End_Part2] = HandleEndPart2;
        stepHandlers[TutorialStep.End_Part3] = HandleEndPart3;
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
        stepTransitions[TutorialStep.AfterCombine] = TutorialStep.End_Part1;
        stepTransitions[TutorialStep.End_Part1] = TutorialStep.End_Part2;
        stepTransitions[TutorialStep.End_Part2] = TutorialStep.End_Part3;
    }

    private void ExecuteCurrentStep()
    {
        // 初始化UI状态
        InitializeUI();

        Debug.Log($"TutorialManager: 执行步骤 {currentStep}");

        if (stepHandlers.ContainsKey(currentStep))
        {
            try
            {
                stepHandlers[currentStep]?.Invoke();
                Debug.Log($"TutorialManager: 步骤 {currentStep} 执行成功");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"TutorialManager: 步骤 {currentStep} 执行失败: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"TutorialManager: 未找到步骤 {currentStep} 的处理函数");
        }
    }

    public void GoToNextStep()
    {
        Debug.Log($"TutorialManager: 尝试从步骤 {currentStep} 跳转到下一步");

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
        }
    }
    #endregion

    #region Step Handlers
    private void HandleWelcomePart1()
    {
        DisablePlayerMovement();
        DisablePlayerSwitching();
        DisableEnterKey();
        SetGuideExpression(exprNormal);
        hintText.text = "缘分的故事，始于同窗之谊...";
        continueButton.gameObject.SetActive(true);
    }

    private void HandleWelcomePart2()
    {
        DisablePlayerMovement();
        DisablePlayerSwitching();
        DisableEnterKey();
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
        EnableUIInteraction();
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
        
        // 启用回车键响应，允许玩家与小狗交互
        if (playerController != null && playerController.GetCurrentPlayer() != null)
        {
            Player currentPlayer = playerController.GetCurrentPlayer();
            currentPlayer.SetEnterKeyEnabled(true);
            Debug.Log("TutorialManager: 已启用回车键响应，允许与小狗交互");
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
        SetGuideExpression(exprHappy);
        hintText.text = "很好！你变成了\"伏\"字，获得了伏下身子的能力。";

        // 显示继续按钮，等待玩家点击
        continueButton.gameObject.SetActive(true);
        EnableUIInteraction(); // 启用UI交互以便继续按钮可用
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

        // 开始重复检查玩家是否获得"虫"字
        StartCoroutine(CheckForChongCharacter());
    }
    
    // 重复检查玩家是否获得"虫"字的协程
    private IEnumerator CheckForChongCharacter()
    {
        Debug.Log("TutorialManager: 开始重复检查玩家是否获得'虫'字");
        
        while (true)
        {
            // 检查玩家是否已经获得"虫"字
            if (playerController != null && playerController.GetCurrentPlayer() != null)
            {
                Player currentPlayer = playerController.GetCurrentPlayer();
                if (currentPlayer.CarryCharacter == "虫")
                {
                    Debug.Log("TutorialManager: 检测到玩家已获得'虫'字，自动进入下一步");
                    // 禁用回车键响应
                    currentPlayer.SetEnterKeyEnabled(false);
                    // 自动进入下一步
                    GoToNextStep();
                    yield break; // 退出协程
                }
                else
                {
                    Debug.Log($"TutorialManager: 玩家当前携带字符为 '{currentPlayer.CarryCharacter}'，继续等待获得'虫'字");
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

    private void HandleAfterGetChong()
    {
        DisablePlayerMovement();
        DisableEnterKey();
        SetGuideExpression(exprHappy);
        hintText.text = "你获得了【虫】字！这是化蝶的关键。";

        // 显示继续按钮，等待玩家点击
        continueButton.gameObject.SetActive(true);
        EnableUIInteraction(); // 启用UI交互以便继续按钮可用
    }

    private void HandleSwitchPlayerPart1()
    {
        DisablePlayerMovement();
        DisablePlayerSwitching();
        SetGuideExpression(exprSideEye);
        hintText.text = "与此同时，那位女扮男装的同窗【祝英台】，又在做什么呢？";
        continueButton.gameObject.SetActive(true);
        EnableUIInteraction();
    }

    private void HandleSwitchPlayerPart2()
    {
        SetGuideExpression(exprNormal);
        hintText.text = "按下【空格键】切换到她的视角看看吧。";
        continueButton.gameObject.SetActive(true);
        EnablePlayerMovement(1);
        EnablePlayerSwitching();
        EnableUIInteraction();
    }

    private void HandleMoveToDie()
    {
        SetGuideExpression(exprSideEye);
        hintText.text = "这是祝英台的书房。请操作她靠近书桌上的【文牒】，并按下【回车键】。";
        if (dieObject != null)
        {
            PointAtTarget(dieObject.transform);
        }
        EnablePlayerMovement(1);
        EnableEnterKey(); // 启用回车键响应，允许与文牒交互

        // 开始重复检查玩家是否获得"牒"字
        StartCoroutine(CheckForDieCharacter());
    }
    
    // 重复检查玩家是否获得"牒"字的协程
    private IEnumerator CheckForDieCharacter()
    {
        Debug.Log("TutorialManager: 开始重复检查玩家是否获得'牒'字");
        
        while (true)
        {
            // 检查玩家是否已经获得"牒"字
            if (playerController != null && playerController.GetCurrentPlayer() != null)
            {
                Player currentPlayer = playerController.GetCurrentPlayer();
                if (currentPlayer.CarryCharacter == "牒")
                {
                    Debug.Log("TutorialManager: 检测到玩家已获得'牒'字，自动进入下一步");
                    // 禁用回车键响应
                    currentPlayer.SetEnterKeyEnabled(false);
                    // 自动进入下一步
                    GoToNextStep();
                    yield break; // 退出协程
                }
                else
                {
                    Debug.Log($"TutorialManager: 玩家当前携带字符为 '{currentPlayer.CarryCharacter}'，继续等待获得'牒'字");
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

    private void HandleAfterGetDie()
    {
        DisablePlayerMovement();
        SetGuideExpression(exprHappy);
        hintText.text = "你获得了【牒】字！";

        // 显示继续按钮，等待玩家点击
        continueButton.gameObject.SetActive(true);
        EnableUIInteraction(); // 启用UI交互以便继续按钮可用
    }

    private void HandleSelectAndSplit()
    {
        SetGuideExpression(exprSideEye);
        hintText.text = "请在解字台中，选中【牒】字，然后点击【拆】按钮。";
        if (splitButton != null)
        {
            HighlightUITarget(splitButton.transform);
        }
        DisablePlayerMovement();
        EnableUIInteraction();
    }

    private void HandleAfterSplit()
    {
        SetGuideExpression(exprHappy);
        hintText.text = "成功拆分！你获得了【片】和【枼】。";
        EnablePlayerMovement(1);

        // 显示继续按钮，等待玩家点击
        continueButton.gameObject.SetActive(true);
        EnableUIInteraction(); // 启用UI交互以便继续按钮可用
    }

    private void HandleSelectAndCombine()
    {
        SetGuideExpression(exprSideEye);
        hintText.text = "化蝶的部件已齐。请选中【虫】和【枼】，然后点击【拼】按钮。";
        if (combineButton != null)
        {
            HighlightUITarget(combineButton.transform);
        }
        DisablePlayerMovement();

        // 延迟一秒后显示继续按钮
        StartCoroutine(DelayedShowContinueButton(1f));
    }

    private void HandleAfterCombine()
    {
        SetGuideExpression(exprHappy);
        hintText.text = "太棒了！诗句完成了！";
        EnablePlayerMovement(1);

        // 显示继续按钮，等待玩家点击
        continueButton.gameObject.SetActive(true);
        EnableUIInteraction(); // 启用UI交互以便继续按钮可用
    }

    private void HandleEndPart1()
    {
        DisablePlayerMovement();
        SetGuideExpression(exprHappy);
        hintText.text = "蝴蝶破茧而出，翩翩起舞。你成功让梁祝的缘分，在这此刻凝结。";
        continueButton.gameObject.SetActive(true);
    }

    private void HandleEndPart2()
    {
        DisablePlayerMovement();
        SetGuideExpression(exprNormal);
        hintText.text = "但他们的故事尚未结束，正如言界中还有许多破碎的篇章等待织补...";
        continueButton.gameObject.SetActive(true);
    }

    private void HandleEndPart3()
    {
        DisablePlayerMovement();
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
        highlightBox.gameObject.SetActive(false);
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
            Debug.Log("TutorialManager: 拆分'牒'字成功，但需要等待点击继续按钮");
            // 不自动跳转，等待玩家点击继续按钮
        }
    }

    public void OnCombineSuccess(string resultWord)
    {
        if (resultWord == "蝶" && currentStep == TutorialStep.SelectAndCombine)
        {
            Debug.Log("TutorialManager: 合成'蝶'字成功，但需要等待点击继续按钮");
            // 不自动跳转，等待玩家点击继续按钮
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

        // 隐藏蒙层
        if (tutorialMask != null)
        {
            tutorialMask.SetActive(false);
        }

        Debug.Log("TutorialManager: 启用UI交互");
    }
    #endregion

    #region Font Settings
    // 设置中文字体
    private void SetChineseFont()
    {
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
            Debug.LogWarning("TutorialManager: 未设置中文字体资源");
        }
    }
    #endregion

    #region Helper Methods
    // 设置引导人物表情
    private void SetGuideExpression(Sprite expression)
    {
        if (guideCharacterImage != null && expression != null)
        {
            guideCharacterImage.sprite = expression;
        }
    }

    // 指向场景中的世界物体 (已更新为箭头 sprite切换 和 翻转 逻辑)
    private void PointAtTarget(Transform target)
    {
        if (arrowImage == null || guideCharacterImage == null || target == null)
        {
            Debug.LogWarning("TutorialManager: PointAtTarget - 缺少必要的组件引用");
            return;
        }

        arrowImage.gameObject.SetActive(true);
        arrowImage.transform.position = guideCharacterImage.transform.position;

        Vector3 targetScreenPos = Camera.main.WorldToScreenPoint(target.position);
        Vector3 direction = (targetScreenPos - arrowImage.transform.position);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

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

    // 高亮UI元素
    private void HighlightUITarget(Transform target)
    {
        if (highlightBox == null || target == null)
        {
            Debug.LogWarning("TutorialManager: HighlightUITarget - 缺少必要的组件引用");
            return;
        }

        highlightBox.gameObject.SetActive(true);
        highlightBox.position = target.position;
        RectTransform targetRect = target.GetComponent<RectTransform>();
        if (targetRect != null)
        {
            highlightBox.sizeDelta = targetRect.sizeDelta + new Vector2(20, 20);
        }
    }
    
    // 检查是否在MoveToGrass步骤中
    public bool IsInMoveToGrassStep()
    {
        return currentStep == TutorialStep.MoveToGrass;
    }
    #endregion
}