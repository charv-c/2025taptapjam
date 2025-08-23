using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Highlight : MonoBehaviour
{
    Light2D light2d;
    private bool isHighlighted = false; // 跟踪是否处于高亮状态
    Player player;
    
    // 添加letter变量
    [SerializeField]
    private string letter; // 用于字典查找和广播系统的字符
    
    [Header("米字格对象引用")]
    [SerializeField] private GameObject misquare; // 拖入的misquare对象
    [SerializeField] private bool canControlMisquare = false; // 是否可以控制misquare
    
    [Header("收集设置")]
    [SerializeField] private bool collectable = true; // 决定物体能否被添加到可用字符串列表中
    
    // Start is called before the first frame update
    void Start()
    {
        light2d=GetComponentInChildren<Light2D>();
        light2d.enabled=false;
        
        // 检查misquare控制设置
        if (canControlMisquare)
        {
            if (misquare == null)
            {
                Debug.LogWarning($"Highlight对象 '{gameObject.name}' 启用了misquare控制但misquare引用未设置，请在Inspector中拖入misquare对象");
            }
            else
            {
                Debug.Log($"Highlight对象 '{gameObject.name}' 已启用misquare控制，关联对象: {misquare.name}");
            }
        }
        else
        {
            Debug.Log($"Highlight对象 '{gameObject.name}' 未启用misquare控制功能");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 只有在脚本启用时才执行
        if (!enabled) return;
        
        // 当处于高亮状态时，检测Enter键按下
        if (isHighlighted && Input.GetKeyDown(KeyCode.Return))
        {
            FunctionA();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 只有在脚本启用时才执行
        if (!enabled) return;
        
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
        // 只有在脚本启用时才执行
        if (!enabled) return;
        
        if (other.CompareTag("Player"))
        {
            player = null;
            isHighlighted = false; // 设置高亮状态为false
            if (light2d != null)
            {
                light2d.enabled = false; // 禁用灯光
            }
        }
    }
    void ChangeMi(){
        // 检查是否可以控制misquare
        if (!canControlMisquare)
        {
            Debug.Log($"Highlight对象 '{gameObject.name}' 未启用misquare控制功能");
            return;
        }
        
        // 获取自己的letter和"人"combine以后的字
        string combinedCharacter = PublicData.FindOriginalString(letter, "人");
        if (combinedCharacter != null)
        {
            Debug.Log($"Letter '{letter}' 和 '人' 合并后的字符: {combinedCharacter}");
            
            // 使用MiSquareController来设置misquare的sprite
            if (misquare != null)
            {
                MiSquareController miSquareController = misquare.GetComponent<MiSquareController>();
                if (miSquareController != null)
                {
                    miSquareController.SetMiSquareSprite(combinedCharacter);
                }
                else
                {
                    Debug.LogWarning("misquare对象没有MiSquareController组件，请添加该组件");
                }
            }
            else
            {
                Debug.LogWarning("misquare对象引用为空，无法更新Sprite");
            }
            
            // 新增功能：改变player的carryletter为本字的letter和人的combine结果
            if (player != null)
            {
                player.CarryCharacter = combinedCharacter;
                Debug.Log($"已将player的CarryCharacter设置为: {combinedCharacter}");
            }
            else
            {
                Debug.LogWarning("Player引用为空，无法设置CarryCharacter");
            }
        }
        else
        {
            Debug.LogWarning($"无法将 '{letter}' 和 '人' 合并");
        }
        
        // 关闭光并禁用Highlight脚本
        if (light2d != null)
        {
            light2d.enabled = false;
            Debug.Log($"已关闭Highlight对象 '{gameObject.name}' 的光");
        }
        
        // 暂时禁用Highlight脚本而不是删除
        Highlight highlightComponent = GetComponent<Highlight>();
        if (highlightComponent != null)
        {
            highlightComponent.enabled = false;
            Debug.Log($"已暂时禁用Highlight对象 '{gameObject.name}' 的Highlight脚本");
        }
    }
    void AddLetterToAvailableList(){
        // 检查是否可以被收集
        if (!collectable)
        {
            Debug.Log($"Highlight对象 '{gameObject.name}' 的collectable为false，无法添加到可用字符串列表");
            return;
        }
        
        // 把自己的letter添加到可用字列表
        if (ButtonController.Instance != null)
        {
            StringSelector stringSelector = ButtonController.Instance.GetStringSelector();
            if (stringSelector != null)
            {
                stringSelector.AddAvailableString(letter);
                Debug.Log($"已将letter '{letter}' 添加到可用字列表");
                
                // 删除自己
                Destroy(gameObject);
                Debug.Log($"已删除当前对象: {gameObject.name}");
            }
            else
            {
                Debug.LogWarning("StringSelector引用为空，无法添加letter到可用字列表");
            }
        }
        else
        {
            Debug.LogWarning("未找到ButtonController实例，无法添加letter到可用字列表");
        }
    }
    // 函数A的实现
    private void FunctionA()
    {
        // 检查player是否存在且有CarryCharacter属性
        if (player != null && !string.IsNullOrEmpty(player.CarryCharacter))
        {
            // 优先级1：如果collectable为true，直接执行收集功能
            if (collectable)
            {
                AddLetterToAvailableList();
                return;
            }
            
            // 优先级2：检查player的CarryCharacter是否等于"人"
            if (player.CarryCharacter == "人")
            {
                // 检查自己的letter是否在listofhua中
                if (PublicData.listofhua.Contains(letter))
                {
                    ChangeMi();
                }
                else
                {
                    // 如果不在花列表中且collectable为false，不执行任何操作
                    Debug.Log($"Letter '{letter}' 不在花列表中且collectable为false，不执行任何操作");
                }
            }
            else
            {
                // 如果不为"人"，比较自己的letter和player的carryletter的键对应的字典的值
                string playerValue = PublicData.stringKeyValuePairs.ContainsKey(player.CarryCharacter) ? 
                                   PublicData.stringKeyValuePairs[player.CarryCharacter] : null;
                string myValue = PublicData.stringKeyValuePairs.ContainsKey(letter) ? 
                               PublicData.stringKeyValuePairs[letter] : null;

                if (playerValue != null && myValue != null)
                {
                    Debug.Log($"Player携带字符 '{player.CarryCharacter}' 对应值: {playerValue}");
                    Debug.Log($"当前letter '{letter}' 对应值: {myValue}");

                    // 比较两个值
                    if (playerValue == myValue)
                    {
                        // 全屏广播carryletter的值
                        BroadcastCarryLetterValue(player.CarryCharacter);
                    }
                }
                else
                {
                    if (playerValue == null)
                    {
                        Debug.LogWarning($"Player携带字符 '{player.CarryCharacter}' 在字典中未找到对应值");
                    }
                    if (myValue == null)
                    {
                        Debug.LogWarning($"当前letter '{letter}' 在字典中未找到对应值");
                    }
                }
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
        Sprite newSprite = PublicData.GetCharacterSprite(letter);
        if (newSprite != null)
        {
            // 更新SpriteRenderer的sprite
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = newSprite;
                Debug.Log($"更新Sprite为: {letter}");
            }
            else
            {
                Debug.LogWarning("未找到SpriteRenderer组件");
            }
        }
        else
        {
            Debug.LogWarning($"未找到字符 '{letter}' 对应的Sprite");
        }
    }
    
    // 公共方法：检查letter是否在花列表中
    public bool IsLetterInHuaList()
    {
        return PublicData.listofhua.Contains(letter);
    }
    
    // 公共方法：设置Letter（用于字典查找和广播）
    public void SetLetter(string newLetter)
    {
        letter = newLetter;
        Debug.Log($"Letter已设置为: {newLetter}");
    }
    
    // 公共方法：获取Letter
    public string GetLetter()
    {
        return letter;
    }
    
    // 公共方法：获取字典中Letter对应的值
    public string GetLetterDictionaryValue()
    {
        if (PublicData.stringKeyValuePairs.ContainsKey(letter))
        {
            return PublicData.stringKeyValuePairs[letter];
        }
        return null;
    }
    
    // 公共方法：设置misquare对象引用
    public void SetMisquareObject(GameObject misquareObject)
    {
        misquare = misquareObject;
        Debug.Log($"Misquare对象已设置为: {misquareObject?.name ?? "null"}");
    }
    
    // 公共方法：获取misquare对象引用
    public GameObject GetMisquareObject()
    {
        return misquare;
    }
    
    // 公共方法：检查misquare对象是否已设置
    public bool HasMisquareObject()
    {
        return misquare != null;
    }
    
    // 公共方法：启用misquare控制
    public void EnableMisquareControl()
    {
        canControlMisquare = true;
        Debug.Log($"Highlight对象 '{gameObject.name}' 已启用misquare控制");
    }
    
    // 公共方法：禁用misquare控制
    public void DisableMisquareControl()
    {
        canControlMisquare = false;
        Debug.Log($"Highlight对象 '{gameObject.name}' 已禁用misquare控制");
    }
    
    // 公共方法：检查是否可以控制misquare
    public bool CanControlMisquare()
    {
        return canControlMisquare;
    }
    
    // 公共方法：设置misquare控制状态
    public void SetMisquareControl(bool enable)
    {
        canControlMisquare = enable;
        Debug.Log($"Highlight对象 '{gameObject.name}' misquare控制已设置为: {enable}");
    }
    
    // 公共方法：检查是否可以被收集
    public bool IsCollectable()
    {
        return collectable;
    }
    
    // 公共方法：设置是否可以被收集
    public void SetCollectable(bool canCollect)
    {
        collectable = canCollect;
        Debug.Log($"Highlight对象 '{gameObject.name}' collectable已设置为: {canCollect}");
    }
    
    // 公共方法：启用收集功能
    public void EnableCollectable()
    {
        collectable = true;
        Debug.Log($"Highlight对象 '{gameObject.name}' 已启用收集功能");
    }
    
    // 公共方法：禁用收集功能
    public void DisableCollectable()
    {
        collectable = false;
        Debug.Log($"Highlight对象 '{gameObject.name}' 已禁用收集功能");
    }
    
    // 全屏广播carryletter的值
    private void BroadcastCarryLetterValue(string carryLetter)
    {
        // 使用BroadcastManager进行全屏广播
        if (BroadcastManager.Instance != null)
        {
            BroadcastManager.Instance.BroadcastToAll(carryLetter);
        }
        else
        {
            Debug.LogWarning("BroadcastManager实例不存在，无法进行广播");
        }
    }
    
    // 接收广播的方法
    public void ReceiveBroadcast(string broadcastedValue)
    {
        Debug.Log($"接收到广播: {broadcastedValue}, 当前对象: {gameObject.name}, 类型: {GetType().Name}");
        
        // 检查自己的letter是否在字典中，且值等于广播的值
        if (PublicData.stringKeyValuePairs.ContainsKey(letter))
        {
            string myValue = PublicData.stringKeyValuePairs[letter];
            if (myValue == broadcastedValue)
            {
                Debug.Log($"当前对象的letter '{letter}' 值与广播值 '{broadcastedValue}' 匹配，执行特殊逻辑");
                // 这里可以添加匹配时的特殊逻辑
                ExecuteSpecialLogic();
            }
        }
    }
    
    // 执行特殊逻辑的方法
    private void ExecuteSpecialLogic()
    {
        Debug.Log($"执行特殊逻辑，当前对象: {gameObject.name}, letter: {letter}");
        // 在这里添加具体的特殊逻辑
        // 比如改变颜色、播放音效、触发动画等
    }
}
