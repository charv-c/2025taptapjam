using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Highlight : MonoBehaviour
{
    Light2D light2d;
    private bool isHighlighted = false; // 跟踪是否处于高亮状态
    [SerializeField]
    string Characater;
    Player player;
    // 当前显示的字符
    private string currentCharacter;
    
    // Start is called before the first frame update
    void Start()
    {
        light2d=GetComponentInChildren<Light2D>();
        light2d.enabled=false;
        currentCharacter = Characater; // 初始化当前字符
    }

    // Update is called once per frame
    void Update()
    {
        // 当处于高亮状态时，检测Enter键按下
        if (isHighlighted && Input.GetKeyDown(KeyCode.Return))
        {
            FunctionA();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Trigger进入: {other.name}");
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();
            Debug.Log("玩家进入Trigger区域");
            isHighlighted = true; // 设置高亮状态为true
            if (light2d != null)
            {
                light2d.enabled = true; // 启用灯光
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"Trigger离开: {other.name}");
        if (other.CompareTag("Player"))
        {
            player = null;
            Debug.Log("玩家离开Trigger区域");
            isHighlighted = false; // 设置高亮状态为false
            if (light2d != null)
            {
                light2d.enabled = false; // 禁用灯光
            }
        }
    }
    
    // 函数A的实现
    private void FunctionA()
    {
        // 检查player是否存在且有CarryCharacter属性
        if (player != null && !string.IsNullOrEmpty(player.CarryCharacter))
        {
            string result = PublicData.EnsureLegal(Characater, player.CarryCharacter);
            if(result != null){
                currentCharacter = result; // 更新当前字符
                RefreshSprite();
            }
        }
        else
        {
            Debug.LogWarning("Player不存在或CarryCharacter为空");
        }
    }

    private void RefreshSprite()
    {
        // 获取当前字符对应的Sprite
        Sprite newSprite = PublicData.GetCharacterSprite(currentCharacter);
        if (newSprite != null)
        {
            // 更新SpriteRenderer的sprite
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = newSprite;
                Debug.Log($"更新Sprite为: {currentCharacter}");
            }
            else
            {
                Debug.LogWarning("未找到SpriteRenderer组件");
            }
        }
        else
        {
            Debug.LogWarning($"未找到字符 '{currentCharacter}' 对应的Sprite");
        }
    }
}
