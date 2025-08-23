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
        Debug.Log($"=== 开始执行AddLetterToAvailableList ===");
        Debug.Log($"当前对象: {gameObject.name}");
        Debug.Log($"Letter值: '{letter}'");
        Debug.Log($"Collectable值: {collectable}");
        
        // 检查是否可以被收集
        if (!collectable)
        {
            Debug.Log($"Highlight对象 '{gameObject.name}' 的collectable为false，无法添加到可用字符串列表");
            return;
        }
        
        Debug.Log($"Collectable检查通过，继续执行...");
        
        // 把自己的letter添加到可用字列表
        if (ButtonController.Instance != null)
        {
            Debug.Log($"ButtonController实例存在");
            StringSelector stringSelector = ButtonController.Instance.GetStringSelector();
            if (stringSelector != null)
            {
                Debug.Log($"StringSelector引用获取成功");
                Debug.Log($"准备添加letter '{letter}' 到可用字列表");
                
                stringSelector.AddAvailableString(letter);
                Debug.Log($"已将letter '{letter}' 添加到可用字列表");
                
                // 删除自己
                Destroy(gameObject);
                Debug.Log($"已删除当前对象: {gameObject.name}");
            }
            else
            {
                Debug.LogError("StringSelector引用为空，无法添加letter到可用字列表");
                Debug.LogError("请检查ButtonController中的StringSelector引用是否正确设置");
            }
        }
        else
        {
            Debug.LogError("未找到ButtonController实例，无法添加letter到可用字列表");
            Debug.LogError("请检查场景中是否存在ButtonController对象，并且单例模式正常工作");
        }
        
        Debug.Log($"=== AddLetterToAvailableList执行完成 ===");
    }
    // 函数A的实现
    private void FunctionA()
    {
        Debug.Log($"=== 开始执行FunctionA ===");
        Debug.Log($"当前对象: {gameObject.name}");
        Debug.Log($"Letter值: '{letter}'");
        Debug.Log($"Collectable值: {collectable}");
        
        // 检查player是否存在且有CarryCharacter属性
        if (player != null && !string.IsNullOrEmpty(player.CarryCharacter))
        {
            Debug.Log($"Player存在，CarryCharacter: '{player.CarryCharacter}'");
            
            // 优先级1：如果collectable为true，直接执行收集功能
            if (collectable)
            {
                Debug.Log($"Collectable为true，执行收集功能");
                AddLetterToAvailableList();
                return;
            }
            
            Debug.Log($"Collectable为false，继续检查其他条件");
            
            // 优先级2：检查player的CarryCharacter是否等于"人"
            if (player.CarryCharacter == "人")
            {
                Debug.Log($"Player携带'人'，检查letter是否在花列表中");
                // 检查自己的letter是否在listofhua中
                if (PublicData.listofhua.Contains(letter))
                {
                    Debug.Log($"Letter '{letter}' 在花列表中，执行ChangeMi");
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
                Debug.Log($"Player携带的不是'人'，而是 '{player.CarryCharacter}'");
                // 如果不为"人"，比较player的carryletter对应的字典值与当前letter
                string playerValue = PublicData.stringKeyValuePairs.ContainsKey(player.CarryCharacter) ? 
                                   PublicData.stringKeyValuePairs[player.CarryCharacter] : null;

                if (playerValue != null)
                {
                    Debug.Log($"Player携带字符 '{player.CarryCharacter}' 对应值: {playerValue}");
                    Debug.Log($"当前letter: {letter}");

                    // 比较playerValue与letter
                    if (playerValue == letter)
                    {
                        Debug.Log($"匹配成功，执行广播");
                        // 全屏广播carryletter的值
                        BroadcastCarryLetterValue(player.CarryCharacter);
                    }
                    else
                    {
                        Debug.Log($"匹配失败，playerValue: '{playerValue}', letter: '{letter}'");
                    }
                }
                else
                {
                    Debug.LogWarning($"Player携带字符 '{player.CarryCharacter}' 在字典中未找到对应值");
                }
            }
        }
        else
        {
            Debug.LogWarning("Player不存在或CarryCharacter为空");
            if (player == null)
            {
                Debug.LogWarning("Player引用为空");
            }
            else
            {
                Debug.LogWarning($"Player存在但CarryCharacter为空或空字符串: '{player.CarryCharacter}'");
            }
        }
        
        Debug.Log($"=== FunctionA执行完成 ===");
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
        // 在发送广播前，将player的CarryCharacter重置为"人"
        if (player != null)
        {
            Debug.Log($"广播前将player的CarryCharacter从 '{player.CarryCharacter}' 重置为 '人'");
            player.CarryCharacter = "人";
            
            // 更新对应的米字格
            UpdateMiSquareForPlayer();
        }
        else
        {
            Debug.LogWarning("Player引用为空，无法重置CarryCharacter");
        }
        
        // 使用BroadcastManager进行全屏广播
        if (BroadcastManager.Instance != null)
        {
            BroadcastManager.Instance.BroadcastToAll(carryLetter);
            Debug.Log($"已发送广播: {carryLetter}");
        }
        else
        {
            Debug.LogWarning("BroadcastManager实例不存在，无法进行广播");
        }
    }
    
    // 更新玩家对应的米字格
    private void UpdateMiSquareForPlayer()
    {
        // 获取玩家的类型（player1或player2）
        bool isPlayer1 = player.IsPlayer1();
        
        // 根据玩家类型查找对应的米字格
        string targetMiSquareName = isPlayer1 ? "MiSquare1" : "MiSquare2";
        
        // 查找指定名称的米字格
        GameObject targetMiSquare = GameObject.Find(targetMiSquareName);
        
        if (targetMiSquare != null)
        {
            MiSquareController miSquareController = targetMiSquare.GetComponent<MiSquareController>();
            if (miSquareController != null)
            {
                // 使用PublicData中的米字格字典，以"人"作为键获取对应的米字格图片
                Sprite miZiGeSprite = PublicData.GetMiZiGeSprite("人");
                if (miZiGeSprite != null)
                {
                    miSquareController.SetMiSquareSprite("人");
                    Debug.Log($"已更新{targetMiSquareName}: {targetMiSquare.name} 为'人'对应的米字格图片");
                }
                else
                {
                    Debug.LogWarning("PublicData中未找到'人'对应的米字格图片");
                }
            }
            else
            {
                Debug.LogWarning($"{targetMiSquareName}没有MiSquareController组件");
            }
        }
        else
        {
            Debug.LogWarning($"未找到{targetMiSquareName}");
        }
    }
    
    // 接收广播的方法
    public void ReceiveBroadcast(string broadcastedValue)
    {
        Debug.Log($"接收到广播: {broadcastedValue}, 当前对象: {gameObject.name}, letter: {letter}");
        
        // 根据对象名称和letter执行不同的逻辑
        HandleBroadcastByObject(broadcastedValue);
    }
    
    // 根据letter处理广播
    private void HandleBroadcastByObject(string broadcastedValue)
    {
        Debug.Log($"=== 开始处理广播 ===");
        Debug.Log($"广播值: '{broadcastedValue}'");
        Debug.Log($"当前对象: {gameObject.name}");
        Debug.Log($"当前letter: '{letter}'");
        
        // 检查自己的letter是否在字典中，且值等于广播的值
        if (PublicData.stringKeyValuePairs.ContainsKey(letter))
        {
            string myValue = PublicData.stringKeyValuePairs[letter];
            Debug.Log($"当前对象的letter '{letter}' 在字典中，对应值: '{myValue}'");
            if (myValue == broadcastedValue)
            {
                Debug.Log($"当前对象的letter '{letter}' 值与广播值 '{broadcastedValue}' 匹配，执行特殊逻辑");
                ExecuteSpecialLogic();
            }
        }
        else
        {
            Debug.Log($"当前对象的letter '{letter}' 不在字典中");
        }
        
        // 只有当广播值为"休"时才执行这些操作
        if (broadcastedValue == "休")
        {
            Debug.Log($"广播值为'休'，检查letter: '{letter}'");
            
            // 根据letter执行特定逻辑
            if (letter == "猎")
            {
                Debug.Log($"检测到letter为'猎'，准备隐藏对象");
                // 猎收到广播"休"以后隐藏
                HideObject();
            }
            else if (letter == "王")
            {
                Debug.Log($"检测到letter为'王'，准备显示对象");
                // 王收到广播"休"以后显示
                ShowObject();
            }
            else if (letter == "夹")
            {
                Debug.Log($"检测到letter为'夹'，准备显示对象");
                // 夹收到广播"休"以后显示
                ShowObject();
            }
            else
            {
                Debug.Log($"letter '{letter}' 不在处理列表中");
            }
        }
        // 只有当广播值为"伙"时才执行这些操作
        else if (broadcastedValue == "伙")
        {
            Debug.Log($"广播值为'伙'，检查letter: '{letter}'");
            
            // 根据letter执行特定逻辑
            if (letter == "孩")
            {
                Debug.Log($"检测到letter为'孩'，准备隐藏对象");
                // 孩收到广播"伙"以后隐藏
                HideObject();
            }
            else if (letter == "门")
            {
                Debug.Log($"检测到letter为'门'，准备显示对象");
                // 门收到广播"伙"以后显示
                ShowObject();
            }
            else
            {
                Debug.Log($"letter '{letter}' 不在处理列表中");
            }
        }
        else
        {
            Debug.Log($"广播值 '{broadcastedValue}' 不是'休'或'伙'，不执行特殊操作");
        }
        
        Debug.Log($"=== 广播处理完成 ===");
    }
    
    // 隐藏对象（不涉及SetActive，保持对象活跃以接收广播）
    private void HideObject()
    {
        Debug.Log($"=== 开始隐藏对象: {gameObject.name} ===");
        
        // 禁用SpriteRenderer来隐藏对象
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
            Debug.Log($"已禁用SpriteRenderer");
        }
        else
        {
            Debug.LogWarning("未找到SpriteRenderer组件");
        }
        
        // 禁用Collider2D来防止交互
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
            Debug.Log($"已禁用Collider2D");
        }
        else
        {
            Debug.LogWarning("未找到Collider2D组件");
        }
        
        // 禁用所有Light2D组件（包括子物体）- 使用SetActive
        Light2D[] allLights = GetComponentsInChildren<Light2D>(true); // true表示包括非激活的对象
        Debug.Log($"找到 {allLights.Length} 个Light2D组件");
        
        foreach (Light2D light in allLights)
        {
            if (light != null)
            {
                light.gameObject.SetActive(false);
                Debug.Log($"已SetActive(false) Light2D组件: {light.gameObject.name}");
            }
        }
        
        // 额外检查：禁用所有子物体的Renderer组件
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        Debug.Log($"找到 {allRenderers.Length} 个Renderer组件");
        
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer != null && renderer != spriteRenderer) // 避免重复处理主SpriteRenderer
            {
                renderer.enabled = false;
                Debug.Log($"已禁用Renderer组件: {renderer.gameObject.name}");
            }
        }
        
        Debug.Log($"=== 对象隐藏完成: {gameObject.name} (letter: {letter}) ===");
        Debug.Log($"隐藏的组件包括: SpriteRenderer, Collider2D, {allLights.Length}个Light2D(SetActive), {allRenderers.Length}个Renderer");
    }
    
    // 显示对象（不涉及SetActive，保持对象活跃以接收广播）
    private void ShowObject()
    {
        Debug.Log($"=== 开始显示对象: {gameObject.name} ===");
        
        // 启用SpriteRenderer来显示对象
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            Debug.Log($"已启用SpriteRenderer");
        }
        else
        {
            Debug.LogWarning("未找到SpriteRenderer组件");
        }
        
        // 启用Collider2D来恢复交互
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
            Debug.Log($"已启用Collider2D");
        }
        else
        {
            Debug.LogWarning("未找到Collider2D组件");
        }
        
        // 启用所有Light2D组件（包括子物体）- 使用SetActive
        Light2D[] allLights = GetComponentsInChildren<Light2D>(true); // true表示包括非激活的对象
        Debug.Log($"找到 {allLights.Length} 个Light2D组件");
        
        foreach (Light2D light in allLights)
        {
            if (light != null)
            {
                light.gameObject.SetActive(true);
                Debug.Log($"已SetActive(true) Light2D组件: {light.gameObject.name}");
            }
        }
        
        // 额外检查：启用所有子物体的Renderer组件
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);
        Debug.Log($"找到 {allRenderers.Length} 个Renderer组件");
        
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer != null && renderer != spriteRenderer) // 避免重复处理主SpriteRenderer
            {
                renderer.enabled = true;
                Debug.Log($"已启用Renderer组件: {renderer.gameObject.name}");
            }
        }
        
        Debug.Log($"=== 对象显示完成: {gameObject.name} (letter: {letter}) ===");
        Debug.Log($"显示的组件包括: SpriteRenderer, Collider2D, {allLights.Length}个Light2D(SetActive), {allRenderers.Length}个Renderer");
    }
    
    // 删除对象
    private void DeleteObject()
    {
        Debug.Log($"已删除对象: {gameObject.name} (letter: {letter})");
        Destroy(gameObject);
    }
    
    // 执行特殊逻辑的方法
    private void ExecuteSpecialLogic()
    {
        Debug.Log($"执行特殊逻辑，当前对象: {gameObject.name}, letter: {letter}");
        // 在这里添加具体的特殊逻辑
        // 比如改变颜色、播放音效、触发动画等
    }
    
    // 公共方法：检查对象是否可见
    public bool IsVisible()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        return spriteRenderer != null && spriteRenderer.enabled;
    }
    
    // 公共方法：检查对象是否可以交互
    public bool IsInteractable()
    {
        Collider2D collider = GetComponent<Collider2D>();
        return collider != null && collider.enabled;
    }
    
    // 公共方法：检查Light2D状态
    public bool HasActiveLights()
    {
        Light2D[] allLights = GetComponentsInChildren<Light2D>();
        foreach (Light2D light in allLights)
        {
            if (light.enabled)
            {
                return true;
            }
        }
        return false;
    }
    
    // 公共方法：获取对象的完整状态信息
    public string GetObjectStatus()
    {
        bool visible = IsVisible();
        bool interactable = IsInteractable();
        bool hasLights = HasActiveLights();
        bool scriptEnabled = enabled;
        
        return $"对象: {gameObject.name}, Letter: {letter}, 可见: {visible}, 可交互: {interactable}, 灯光: {hasLights}, 脚本启用: {scriptEnabled}";
    }
}
