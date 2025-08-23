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
    public string letter;
    
    [Header("米字格对象引用")]
    [SerializeField] private GameObject misquare;
    [SerializeField] private bool canControlMisquare = false;
    
    [Header("收集设置")]
    [SerializeField] private bool collectable = true;
    
    void Start()
    {
        light2d=GetComponentInChildren<Light2D>();
        light2d.enabled=false;
        
        // 检查初始状态
        if (letter == "夹")
        {
            Debug.Log($"夹对象初始化: {gameObject.name}");
            CheckObjectStatus();
            
            // 确保Highlight组件被激活
            if (!enabled)
            {
                Debug.Log($"激活夹对象的Highlight组件: {gameObject.name}");
                enabled = true;
            }
            
            // 确保GameObject被激活
            if (!gameObject.activeInHierarchy)
            {
                Debug.Log($"激活夹对象的GameObject: {gameObject.name}");
                gameObject.SetActive(true);
            }
        }
    }

    void Update()
    {
        if (!enabled) return;
        
        // 移除回车键检测，现在由Player统一处理
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
        Debug.Log($"ChangeMi: 开始处理字符合成，letter='{letter}'，canControlMisquare={canControlMisquare}");
        
        if (!canControlMisquare)
        {
            Debug.LogWarning($"ChangeMi: canControlMisquare为false，无法控制米字格");
            return;
        }
        
        string combinedCharacter = PublicData.FindOriginalString(letter, "人");
        Debug.Log($"ChangeMi: 查找合成字符，letter='{letter}' + '人' = '{combinedCharacter}'");
        
        if (combinedCharacter != null)
        {
            Debug.Log($"ChangeMi: 找到合成字符 '{combinedCharacter}'，开始更新米字格和玩家状态");
            
            if (misquare != null)
            {
                MiSquareController miSquareController = misquare.GetComponent<MiSquareController>();
                if (miSquareController != null)
                {
                    miSquareController.SetMiSquareSprite(combinedCharacter);
                    Debug.Log($"ChangeMi: 已更新米字格为字符 '{combinedCharacter}'");
                }
                else
                {
                    Debug.LogWarning($"ChangeMi: 米字格对象没有MiSquareController组件");
                }
            }
            else
            {
                Debug.LogWarning($"ChangeMi: misquare对象为空");
            }
            
            if (player != null)
            {
                // 使用新的SetCarryCharacter方法，会自动更新米字格图片
                player.SetCarryCharacter(combinedCharacter);
                Debug.Log($"ChangeMi: 已设置玩家携带字符为 '{combinedCharacter}'");
            }
            else
            {
                Debug.LogWarning($"ChangeMi: player对象为空");
            }
        }
        else
        {
            Debug.LogWarning($"ChangeMi: 未找到合成字符，letter='{letter}' + '人' 无法合成");
        }
        
        if (light2d != null)
        {
            light2d.enabled = false;
        }
        
        // 不禁用Highlight组件，保持其激活状态以接收广播
        Highlight highlightComponent = GetComponent<Highlight>();
        if (highlightComponent != null)
        {
            highlightComponent.enabled = false;
        }
        
        Debug.Log($"ChangeMi: 字符合成处理完成");
    }
    
    void AddLetterToAvailableList(){
        Debug.Log($"AddLetterToAvailableList: 开始添加字符 '{letter}'，collectable={collectable}");
        
        if (!collectable)
        {
            Debug.Log($"AddLetterToAvailableList: 字符 '{letter}' 不可收集，跳过");
            return;
        }
        
        if (ButtonController.Instance != null)
        {
            StringSelector stringSelector = ButtonController.Instance.GetStringSelector();
            if (stringSelector != null)
            {
                Debug.Log($"AddLetterToAvailableList: 正在添加字符 '{letter}' 到可用字符串列表");
                stringSelector.AddAvailableString(letter);
                Debug.Log($"AddLetterToAvailableList: 字符 '{letter}' 已添加到可用字符串列表，准备销毁对象");
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError($"AddLetterToAvailableList: StringSelector为空，无法添加字符 '{letter}'");
            }
        }
        else
        {
            Debug.LogError($"AddLetterToAvailableList: ButtonController.Instance为空，无法添加字符 '{letter}'");
        }
    }
    
    private void FunctionA()
    {
        Debug.Log($"FunctionA: 开始处理交互，对象letter='{letter}'，玩家携带字符='{player?.CarryCharacter}'，collectable={collectable}");
        
        if (player != null && !string.IsNullOrEmpty(player.CarryCharacter))
        {
            if (collectable)
            {
                Debug.Log($"FunctionA: 对象 '{letter}' 是可收集的，调用AddLetterToAvailableList");
                AddLetterToAvailableList();
                return;
            }
            
            if (player.CarryCharacter == "人")
            {
                Debug.Log($"FunctionA: 玩家携带'人'字符，检查 '{letter}' 是否在可化字列表中");
                if (PublicData.listofhua.Contains(letter))
                {
                    Debug.Log($"FunctionA: '{letter}' 在可化字列表中，调用ChangeMi");
                    ChangeMi();
                }
                else
                {
                    Debug.LogWarning($"FunctionA: '{letter}' 不在可化字列表中: {string.Join(", ", PublicData.listofhua)}");
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
                        Debug.Log($"FunctionA: 玩家携带字符 '{player.CarryCharacter}' 与对象 '{letter}' 匹配，调用BroadcastCarryLetterValue");
                        BroadcastCarryLetterValue(player.CarryCharacter);
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning($"FunctionA: 玩家为空或携带字符为空，玩家={player}, CarryCharacter='{player?.CarryCharacter}'");
        }
        
        // 特殊处理：草对象在教程步骤中的特殊逻辑
        HandleSpecialTutorialLogic();
    }
    
    // 处理教程中的特殊逻辑
    private void HandleSpecialTutorialLogic()
    {
        // 检查是否在MoveToGrass步骤中
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsInMoveToGrassStep())
        {
            if (letter == "草" && player != null)
            {
                Debug.Log($"FunctionA: 在MoveToGrass步骤中，玩家与草交互，执行特殊逻辑。玩家当前携带字符: '{player.CarryCharacter}'");
                
                // 显示草的子物体"虫"
                ShowChongChildObject();
                
                // 添加"虫"到可用字符串列表
                AddChongToAvailableList();
                
                // 通知TutorialManager虫已显示，可以进入下一步
                NotifyTutorialManagerChongShown();
            }
        }
    }
    
    // 通知TutorialManager虫已显示
    private void NotifyTutorialManagerChongShown()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnChongShown();
        }
    }
    
    // 显示草的子物体"虫"
    private void ShowChongChildObject()
    {
        // 查找当前草对象的子物体"虫"
        Transform chongChild = transform.Find("虫");
        if (chongChild != null)
        {
            chongChild.gameObject.SetActive(true);
            Debug.Log($"FunctionA: 已显示草的子物体'虫': {chongChild.gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"FunctionA: 未找到草对象的子物体'虫'");
        }
    }
    
    // 添加"虫"到可用字符串列表
    private void AddChongToAvailableList()
    {
        if (ButtonController.Instance != null)
        {
            StringSelector stringSelector = ButtonController.Instance.GetStringSelector();
            if (stringSelector != null)
            {
                Debug.Log("FunctionA: 正在添加字符 '虫' 到可用字符串列表");
                stringSelector.AddAvailableString("虫");
                Debug.Log("FunctionA: 字符 '虫' 已添加到可用字符串列表");
            }
            else
            {
                Debug.LogError("FunctionA: StringSelector为空，无法添加字符 '虫'");
            }
        }
        else
        {
            Debug.LogError("FunctionA: ButtonController.Instance为空，无法添加字符 '虫'");
        }
    }
    

    
    private void BroadcastCarryLetterValue(string carryLetter)
    {
        if (player != null)
        {
            // 使用新的SetCarryCharacter方法，会自动更新米字格图片
            player.SetCarryCharacter("人");
        }
        
        if (BroadcastManager.Instance != null)
        {
            BroadcastManager.Instance.BroadcastToAll(carryLetter);
        }
    }
    

    
    public void ReceiveBroadcast(string broadcastedValue)
    {
        Debug.Log($"收到广播: {broadcastedValue}, 当前对象: {gameObject.name}, letter: {letter}");
        Debug.Log($"对象状态 - GameObject active: {gameObject.activeInHierarchy}, Highlight enabled: {enabled}");
        
        // 如果Highlight组件被禁用，重新激活它
        if (!enabled)
        {
            Debug.Log($"重新激活Highlight组件: {gameObject.name}");
            enabled = true;
        }
        
        // 如果GameObject被禁用，重新激活它
        if (!gameObject.activeInHierarchy)
        {
            Debug.Log($"重新激活GameObject: {gameObject.name}");
            gameObject.SetActive(true);
        }
        
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
            Debug.Log($"收到'休'广播，当前对象letter={letter}");
            if (letter == "猎")
            {
                Debug.Log($"隐藏猎对象: {gameObject.name}");
                HideObject();
            }
            else if (letter == "王")
            {
                Debug.Log($"显示王对象: {gameObject.name}");
                ShowObject();
            }
            else if (letter == "夹")
            {
                Debug.Log($"显示夹对象: {gameObject.name}");
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
            if(letter == "雨"){
                HideObject();
            }
        }
        else if (broadcastedValue == "侠")
        {
            Debug.Log($"收到'侠'广播，当前对象letter={letter}");
            if (letter == "王")
            {
                Debug.Log($"处理'王'对象，开始隐藏对象并添加到可用列表");
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
    
    public void ShowObject()
    {
        // 确保Highlight组件被激活
        if (!enabled)
        {
            Debug.Log($"激活Highlight组件: {gameObject.name}");
            enabled = true;
        }
        
        // 确保GameObject被激活
        if (!gameObject.activeInHierarchy)
        {
            Debug.Log($"激活GameObject: {gameObject.name}");
            gameObject.SetActive(true);
        }
        
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
            Debug.Log($"激活碰撞箱: {gameObject.name}, 碰撞箱类型: {collider.GetType()}, 是否为Trigger: {collider.isTrigger}");
        }
        else
        {
            Debug.LogWarning($"未找到碰撞箱: {gameObject.name}");
        }
        
        Light2D[] allLights = GetComponentsInChildren<Light2D>(true);
        foreach (Light2D light in allLights)
        {
            if (light != null)
            {
                light.gameObject.SetActive(true);
                Debug.Log($"激活灯光: {light.gameObject.name}");
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
        
        Debug.Log($"显示对象完成: {gameObject.name}");
        
        // 检查对象当前状态
        CheckObjectStatus();
        
        // 如果是"夹"对象，检查广播系统是否能找到它
        if (letter == "夹")
        {
            StartCoroutine(CheckBroadcastReception());
        }
    }
    
    private System.Collections.IEnumerator CheckBroadcastReception()
    {
        yield return new WaitForEndOfFrame();
        
        Debug.Log($"检查'夹'对象的广播接收状态: {gameObject.name}");
        
        // 检查BroadcastManager是否能找到这个对象
        if (BroadcastManager.Instance != null)
        {
            MonoBehaviour[] allObjects = FindObjectsOfType<MonoBehaviour>();
            int highlightCount = 0;
            bool foundThisObject = false;
            
            foreach (MonoBehaviour obj in allObjects)
            {
                if (obj.GetType().GetMethod("ReceiveBroadcast") != null)
                {
                    if (obj is Highlight highlight)
                    {
                        highlightCount++;
                        if (highlight == this)
                        {
                            foundThisObject = true;
                            Debug.Log($"✓ 找到当前'夹'对象: {gameObject.name}");
                        }
                    }
                }
            }
            
            Debug.Log($"场景中共有 {highlightCount} 个Highlight组件，当前'夹'对象是否被找到: {foundThisObject}");
        }
    }
    
    private void CheckObjectStatus()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Collider2D collider = GetComponent<Collider2D>();
        
        Debug.Log($"对象状态检查 - {gameObject.name}:");
        Debug.Log($"  SpriteRenderer enabled: {spriteRenderer?.enabled}");
        Debug.Log($"  Collider2D enabled: {collider?.enabled}");
        Debug.Log($"  Collider2D isTrigger: {collider?.isTrigger}");
        Debug.Log($"  GameObject active: {gameObject.activeInHierarchy}");
        Debug.Log($"  Component enabled: {enabled}");
    }
    
    private void ExecuteSpecialLogic()
    {
        // 特殊逻辑实现
    }
    
    // 公共方法：触发交互（由Player调用）
    public void TriggerInteraction()
    {
        if (enabled && isHighlighted)
        {
            FunctionA();
        }
    }
}
