using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class ButtonController : MonoBehaviour
{
    [Header("按钮设置")]
    [SerializeField] private Button splitButton, combineButton,confirmButton, cancelButton;
    [SerializeField] private float hideDelay = 0.1f;
    
    [Header("UI提示设置")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float messageDisplayTime = 3f;
    
    [Header("选择器引用")]
    [SerializeField] private StringSelector stringSelector;
    
    public static ButtonController Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        if (confirmButton != null) confirmButton.gameObject.SetActive(false);
        if (cancelButton != null) cancelButton.gameObject.SetActive(false);
        
        if (messageText != null) messageText.gameObject.SetActive(false);
        
        UpdateButtonStates(0);
        
        if (splitButton != null)
        {
            splitButton.onClick.AddListener(OnSplitButtonClicked);
        }
        
        if (combineButton != null)
        {
            combineButton.onClick.AddListener(OnCombineButtonClicked);
        }
    }

    private void OnSplitButtonClicked()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.sfxUIClick != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxUIClick);
        }
        splitletter();
    }
    
    private void OnCombineButtonClicked()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.sfxUIClick != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxUIClick);
        }
        combineletter();
    }
    
    private int GetCurrentSelectionCount()
    {
        if (stringSelector != null)
        {
            return stringSelector.GetSelectionCount();
        }
        return 0;
    }
    
    public void UpdateButtonStates(int selectedCount)
    {
        if (splitButton != null)
        {
            splitButton.interactable = selectedCount == 1;
        }
        
        if (combineButton != null)
        {
            combineButton.interactable = selectedCount == 2;
        }
    }
    
    private void HideAllButtons()
    {
        if (splitButton != null) splitButton.gameObject.SetActive(false);
        if (combineButton != null) combineButton.gameObject.SetActive(false);
        if (confirmButton != null) confirmButton.gameObject.SetActive(false);
        if (cancelButton != null) cancelButton.gameObject.SetActive(false);
    }
    
    private void ShowSplitAndCombineButtons()
    {
        if (splitButton != null) splitButton.gameObject.SetActive(true);
        if (combineButton != null) combineButton.gameObject.SetActive(true);
        
        if (stringSelector != null)
        {
            UpdateButtonStates(stringSelector.GetSelectionCount());
        }
    }
    
    private void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(true);
            StartCoroutine(HideMessageAfterDelay());
        }
    }
    
    private void HideMessage()
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }
    
    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDisplayTime);
        HideMessage();
    }
    
    private void splitletter()
    {
        if (stringSelector != null)
        {
            int selectedCount = stringSelector.GetSelectionCount();
            if (selectedCount != 1)
            {
                stringSelector.ClearSelection();
                return;
            }
            
            string selectedString = stringSelector.FirstSelectedString;
            if (!string.IsNullOrEmpty(selectedString))
            {
                if (PublicData.CanSplitString(selectedString))
                {
                    var (part1, part2) = PublicData.GetStringSplit(selectedString);
                    
                    if (AudioManager.Instance != null && AudioManager.Instance.sfxSplitSuccess != null)
                    {
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxSplitSuccess);
                    }

                    stringSelector.ClearSelection();
                    stringSelector.RemoveAvailableString(selectedString);
                    stringSelector.AddAvailableString(part1);
                    stringSelector.AddAvailableString(part2);
                    stringSelector.RecreateAllButtonsPublic();
                    stringSelector.SetMaxSelectionCount(2);
                }
                else
                {
                    if (AudioManager.Instance != null && AudioManager.Instance.sfxOperationFailure != null)
                    {
                        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxOperationFailure);
                    }
                    stringSelector.ClearSelection();
                }
            }
        }
    }
    
    private void combineletter()
    {
        if (stringSelector != null)
        {
            int selectedCount = stringSelector.GetSelectionCount();
            if (selectedCount != 2)
            {
                stringSelector.ClearSelection();
                return;
            }
            
            List<string> selectedStrings = stringSelector.SelectedStrings;
            string firstString = selectedStrings[0];
            string secondString = selectedStrings[1];
            
            string originalString = PublicData.FindOriginalString(firstString, secondString);
            if (originalString != null)
            {
                if (AudioManager.Instance != null && AudioManager.Instance.sfxCombineSuccess != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxCombineSuccess);
                }

                stringSelector.ClearSelection();
                stringSelector.RemoveAvailableString(firstString);
                stringSelector.RemoveAvailableString(secondString);
                
                if (PublicData.IsCharacterInTargetList(originalString))
                {
                    Transform targetPosition = PublicData.GetTargetPositionForCharacter(originalString);
                    if (targetPosition != null)
                    {
                        CreateFlyingCharacter(originalString, targetPosition);
                    }
                    else
                    {
                        stringSelector.AddAvailableString(originalString);
                    }
                }
                else
                {
                    stringSelector.AddAvailableString(originalString);
                }
                
                stringSelector.RecreateAllButtonsPublic();
                stringSelector.SetMaxSelectionCount(2);
                stringSelector.ClearSelection();
            }
            else
            {
                if (AudioManager.Instance != null && AudioManager.Instance.sfxOperationFailure != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxOperationFailure);
                }
                stringSelector.ClearSelection();
            }
        }
    }
    
    public void ShowAllButtons()
    {
        if (splitButton != null) splitButton.gameObject.SetActive(true);
        if (combineButton != null) combineButton.gameObject.SetActive(true);
    }
    
    public void SetAllButtonsInteractable(bool interactable)
    {
        if (splitButton != null) splitButton.interactable = interactable;
        if (combineButton != null) combineButton.interactable = interactable;
    }
    
    public bool AreAllButtonsInteractable()
    {
        return (splitButton != null && splitButton.interactable) &&
               (combineButton != null && combineButton.interactable);
    }
    
    public void TriggerSplitButton()
    {
        OnSplitButtonClicked();
    }
    
    public void SetStringSelector(StringSelector selector)
    {
        stringSelector = selector;
    }
    
    public StringSelector GetStringSelector()
    {
        return stringSelector;
    }
    
    private void CreateFlyingCharacter(string character, Transform targetPosition)
    {
        Sprite characterSprite = PublicData.GetCharacterSprite(character);
        if (characterSprite == null)
        {
            return;
        }
        
        GameObject flyingCharacter = new GameObject($"Flying_{character}");
        
        SpriteRenderer spriteRenderer = flyingCharacter.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = characterSprite;
        spriteRenderer.sortingOrder = 100;
        
        Vector3 startPosition = Vector3.zero;
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            startPosition = player.transform.position;
        }
        flyingCharacter.transform.position = startPosition;
        
        StartCoroutine(FlyToTarget(flyingCharacter, targetPosition.position, character));
    }
    
    private IEnumerator FlyToTarget(GameObject flyingCharacter, Vector3 targetPosition, string character)
    {
        Vector3 startPosition = flyingCharacter.transform.position;
        float duration = 1.0f;
        float elapsedTime = 0f;
        
        if (AudioManager.Instance != null && AudioManager.Instance.sfxGoalFlyIn != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxGoalFlyIn);
        }
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            float easeProgress = Mathf.SmoothStep(0f, 1f, progress);
            
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, easeProgress);
            flyingCharacter.transform.position = newPosition;
            
            float scale = 1f + Mathf.Sin(progress * Mathf.PI) * 0.2f;
            flyingCharacter.transform.localScale = Vector3.one * scale;
            
            yield return null;
        }
        
        flyingCharacter.transform.position = targetPosition;
        flyingCharacter.transform.localScale = Vector3.one;
        
        // 标记目标字符为已完成
        PublicData.MarkTargetAsCompleted(character);
        
        yield return new WaitForSeconds(0.5f);
        Destroy(flyingCharacter);
    }
}
