using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
    #endregion

    #region Private State
    private enum TutorialStep { Welcome_Part1, Welcome_Part2, Welcome_Part3, MoveToDog, AfterTransform, MoveToGrass, AfterGetChong, SwitchPlayer_Part1, SwitchPlayer_Part2, MoveToDie, AfterGetDie, SelectAndSplit, AfterSplit, SelectAndCombine, AfterCombine, End_Part1, End_Part2, End_Part3 }
    private TutorialStep currentStep;
    #endregion

    #region Unity Lifecycle
    private void Awake() { if (Instance == null) { Instance = this; } else { Destroy(gameObject); } }

    private void Start()
    {
        tutorialMask.SetActive(true); // 在教程开始时激活蒙层
        currentStep = TutorialStep.Welcome_Part1;
        ShowStepInstructions(currentStep);
        continueButton.onClick.AddListener(GoToNextStep);
    }
    #endregion

    #region Core Logic
    private void ShowStepInstructions(TutorialStep step)
    {
        tutorialPanel.SetActive(true);
        continueButton.gameObject.SetActive(false);
        highlightBox.gameObject.SetActive(false);
        arrowImage.gameObject.SetActive(false);

        switch (step)
        {
            case TutorialStep.Welcome_Part1:
                SetGuideExpression(exprNormal);
                hintText.text = "缘分的故事，始于同窗之谊...";
                continueButton.gameObject.SetActive(true);
                break;
            case TutorialStep.Welcome_Part2:
                SetGuideExpression(exprNormal);
                hintText.text = "这里是万松书院，梁山伯与祝英台初遇之地。而你，将化身为【梁山伯】...";
                continueButton.gameObject.SetActive(true);
                break;
            case TutorialStep.Welcome_Part3:
                SetGuideExpression(exprHappy);
                hintText.text = "请使用【WASD】键，在书院中自由走动，熟悉一下环境吧。";
                continueButton.gameObject.SetActive(true);
                break;
            case TutorialStep.MoveToDog:
                SetGuideExpression(exprSideEye); // 斜眼表情，暗示有“好事”发生
                hintText.text = "这只小狗似乎很亲近你。靠近它并按下【回车键】进行互动。";
                PointAtTarget(dogObject.transform);
                break;
            case TutorialStep.AfterTransform:
                SetGuideExpression(exprHappy);
                hintText.text = "很好！你变成了“伏”字，获得了伏下身子的能力。";
                StartCoroutine(AutoAdvance(2.5f));
                break;
            case TutorialStep.MoveToGrass:
                SetGuideExpression(exprSideEye); // 斜眼表情，暗示草丛里有“秘密”
                hintText.text = "那片草丛里似乎藏着什么，靠近并按下【回车键】仔细看看吧。";
                PointAtTarget(grassObject.transform);
                break;
            case TutorialStep.AfterGetChong:
                SetGuideExpression(exprHappy);
                hintText.text = "你获得了【虫】字！这是化蝶的关键。";
                StartCoroutine(AutoAdvance(2f));
                break;
            case TutorialStep.SwitchPlayer_Part1:
                SetGuideExpression(exprSideEye); // 斜眼表情，引出另一位主角
                hintText.text = "与此同时，那位女扮男装的同窗【祝英台】，又在做什么呢？";
                continueButton.gameObject.SetActive(true);
                break;
            case TutorialStep.SwitchPlayer_Part2:
                SetGuideExpression(exprNormal);
                hintText.text = "按下【空格键】切换到她的视角看看吧。";
                continueButton.gameObject.SetActive(true);
                break;
            case TutorialStep.MoveToDie:
                SetGuideExpression(exprSideEye);
                hintText.text = "这是祝英台的书房。请操作她靠近书桌上的【文牒】，并按下【回车键】。";
                PointAtTarget(dieObject.transform);
                break;
            case TutorialStep.AfterGetDie:
                SetGuideExpression(exprHappy);
                hintText.text = "你获得了【牒】字！";
                StartCoroutine(AutoAdvance(2f));
                break;
            case TutorialStep.SelectAndSplit:
                SetGuideExpression(exprSideEye);
                hintText.text = "请在解字台中，选中【牒】字，然后点击【拆】按钮。";
                HighlightUITarget(splitButton.transform);
                break;
            case TutorialStep.AfterSplit:
                SetGuideExpression(exprHappy);
                hintText.text = "成功拆分！你获得了【片】和【枼】。";
                StartCoroutine(AutoAdvance(2f));
                break;
            case TutorialStep.SelectAndCombine:
                SetGuideExpression(exprSideEye);
                hintText.text = "化蝶的部件已齐。请选中【虫】和【枼】，然后点击【拼】按钮。";
                HighlightUITarget(combineButton.transform);
                break;
            case TutorialStep.AfterCombine:
                SetGuideExpression(exprHappy);
                hintText.text = "太棒了！诗句完成了！";
                StartCoroutine(AutoAdvance(2.5f));
                break;
            case TutorialStep.End_Part1:
                SetGuideExpression(exprHappy);
                hintText.text = "蝴蝶破茧而出，翩翩起舞。你成功让梁祝的缘分，在这此刻凝结。";
                continueButton.gameObject.SetActive(true);
                break;
            case TutorialStep.End_Part2:
                SetGuideExpression(exprNormal);
                hintText.text = "但他们的故事尚未结束，正如言界中还有许多破碎的篇章等待织补...";
                continueButton.gameObject.SetActive(true);
                break;
            case TutorialStep.End_Part3:
                SetGuideExpression(exprHappy);
                hintText.text = "下一章，我们将进入【牛郎】与【织女】的星河，感受那份跨越天际的守望。";
                continueButton.GetComponentInChildren<TextMeshProUGUI>().text = "完成教学";
                continueButton.gameObject.SetActive(true);
                break;
        }
    }
    
    // ... [其他GoToNextStep, Coroutine, Event Handlers等方法保持不变] ...
    public void GoToNextStep() { if ((int)currentStep >= System.Enum.GetValues(typeof(TutorialStep)).Length - 1) return; currentStep++; ShowStepInstructions(currentStep); }
    private IEnumerator AutoAdvance(float delay) { yield return new WaitForSeconds(delay); GoToNextStep(); }
    public void OnPlayerTransformed() { if (currentStep == TutorialStep.MoveToDog) GoToNextStep(); }
    public void OnWordAcquired(string word) { if (word == "虫" && currentStep == TutorialStep.MoveToGrass) GoToNextStep(); if (word == "牒" && currentStep == TutorialStep.MoveToDie) GoToNextStep(); }
    public void OnSplitSuccess(string sourceWord) { if (sourceWord == "牒" && currentStep == TutorialStep.SelectAndSplit) GoToNextStep(); }
    public void OnCombineSuccess(string resultWord) { if (resultWord == "蝶" && currentStep == TutorialStep.SelectAndCombine) GoToNextStep(); }
    public void OnPlayerSwitched() { if (currentStep == TutorialStep.SwitchPlayer_Part2) GoToNextStep(); }

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
        highlightBox.gameObject.SetActive(true);
        highlightBox.position = target.position;
        RectTransform targetRect = target.GetComponent<RectTransform>();
        if (targetRect != null)
        {
            highlightBox.sizeDelta = targetRect.sizeDelta + new Vector2(20, 20);
        }
    }
    #endregion
}