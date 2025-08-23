using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Highlight : MonoBehaviour
{
    Light2D light2d;
    private bool isHighlighted = false;
    Player player;
    
    [SerializeField]
    private string letter;
    
    [Header("米字格对象引用")]
    [SerializeField] private GameObject misquare;
    [SerializeField] private bool canControlMisquare = false;
    
    [Header("收集设置")]
    [SerializeField] private bool collectable = true;
    
    void Start()
    {
        light2d=GetComponentInChildren<Light2D>();
        light2d.enabled=false;
    }

    void Update()
    {
        if (!enabled) return;
        
        if (isHighlighted && Input.GetKeyDown(KeyCode.Return))
        {
            FunctionA();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled) return;
        
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();
            isHighlighted = true;
            if (light2d != null)
            {
                light2d.enabled = true;
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!enabled) return;
        
        if (other.CompareTag("Player"))
        {
            player = null;
            isHighlighted = false;
            if (light2d != null)
            {
                light2d.enabled = false;
            }
        }
    }
    
    void ChangeMi(){
        if (!canControlMisquare)
        {
            return;
        }
        
        string combinedCharacter = PublicData.FindOriginalString(letter, "人");
        if (combinedCharacter != null)
        {
            if (misquare != null)
            {
                MiSquareController miSquareController = misquare.GetComponent<MiSquareController>();
                if (miSquareController != null)
                {
                    miSquareController.SetMiSquareSprite(combinedCharacter);
                }
            }
            
            if (player != null)
            {
                player.CarryCharacter = combinedCharacter;
            }
        }
        
        if (light2d != null)
        {
            light2d.enabled = false;
        }
        
        Highlight highlightComponent = GetComponent<Highlight>();
        if (highlightComponent != null)
        {
            highlightComponent.enabled = false;
        }
    }
    
    void AddLetterToAvailableList(){
        if (!collectable)
        {
            return;
        }
        
        if (ButtonController.Instance != null)
        {
            StringSelector stringSelector = ButtonController.Instance.GetStringSelector();
            if (stringSelector != null)
            {
                stringSelector.AddAvailableString(letter);
                Destroy(gameObject);
            }
        }
    }
    
    private void FunctionA()
    {
        if (player != null && !string.IsNullOrEmpty(player.CarryCharacter))
        {
            if (collectable)
            {
                AddLetterToAvailableList();
                return;
            }
            
            if (player.CarryCharacter == "人")
            {
                if (PublicData.listofhua.Contains(letter))
                {
                    ChangeMi();
                }
            }
            else
            {
                string playerValue = PublicData.stringKeyValuePairs.ContainsKey(player.CarryCharacter) ? 
                                   PublicData.stringKeyValuePairs[player.CarryCharacter] : null;

                if (playerValue != null)
                {
                    if (playerValue == letter)
                    {
                        BroadcastCarryLetterValue(player.CarryCharacter);
                    }
                }
            }
        }
    }
    
    private void BroadcastCarryLetterValue(string carryLetter)
    {
        if (player != null)
        {
            player.CarryCharacter = "人";
            UpdateMiSquareForPlayer();
        }
        
        if (BroadcastManager.Instance != null)
        {
            BroadcastManager.Instance.BroadcastToAll(carryLetter);
        }
    }
    
    private void UpdateMiSquareForPlayer()
    {
        bool isPlayer1 = player.IsPlayer1();
        string targetMiSquareName = isPlayer1 ? "MiSquare1" : "MiSquare2";
        
        GameObject targetMiSquare = GameObject.Find(targetMiSquareName);
        
        if (targetMiSquare != null)
        {
            MiSquareController miSquareController = targetMiSquare.GetComponent<MiSquareController>();
            if (miSquareController != null)
            {
                Sprite miZiGeSprite = PublicData.GetMiZiGeSprite("人");
                if (miZiGeSprite != null)
                {
                    miSquareController.SetMiSquareSprite("人");
                }
            }
        }
    }
    
    public void ReceiveBroadcast(string broadcastedValue)
    {
        HandleBroadcastByObject(broadcastedValue);
    }
    
    private void HandleBroadcastByObject(string broadcastedValue)
    {
        if (PublicData.stringKeyValuePairs.ContainsKey(letter))
        {
            string myValue = PublicData.stringKeyValuePairs[letter];
            if (myValue == broadcastedValue)
            {
                ExecuteSpecialLogic();
            }
        }
        
        if (broadcastedValue == "休")
        {
            if (letter == "猎")
            {
                HideObject();
            }
            else if (letter == "王")
            {
                ShowObject();
            }
            else if (letter == "夹")
            {
                ShowObject();
            }
        }
        else if (broadcastedValue == "伙")
        {
            if (letter == "孩")
            {
                HideObject();
            }
            else if (letter == "门")
            {
                ShowObject();
            }
        }
        else if (broadcastedValue == "停")
        {
            if (letter == "日")
            {
                ShowObject();
            }
        }
        else if (broadcastedValue == "侠")
        {
            if (letter == "王")
            {
                HideObject();
                AddLetterToAvailableList();
            }
        }
    }
    
    private void HideObject()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        Light2D[] allLights = GetComponentsInChildren<Light2D>(true);
        foreach (Light2D light in allLights)
        {
            if (light != null)
            {
                light.gameObject.SetActive(false);
            }
        }
        
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer != null && renderer != spriteRenderer)
            {
                renderer.enabled = false;
            }
        }
    }
    
    private void ShowObject()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }
        
        Light2D[] allLights = GetComponentsInChildren<Light2D>(true);
        foreach (Light2D light in allLights)
        {
            if (light != null)
            {
                light.gameObject.SetActive(true);
            }
        }
        
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer != null && renderer != spriteRenderer)
            {
                renderer.enabled = true;
            }
        }
    }
    
    private void ExecuteSpecialLogic()
    {
        // 特殊逻辑实现
    }
}
