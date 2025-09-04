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
    
    // 因互动占用而被禁用的标记，用于交互后统一恢复
    [HideInInspector]
    public bool disabledByInteraction = false;
    
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

    // 对外提供：是否为可收集元素且当前处于启用显示状态
    public bool IsCollectableActive()
    {
        if (!collectable) return false;

        // 认为可交互显示的条件：
        // - 组件启用
        // - GameObject 处于激活
        // - 自身 SpriteRenderer 启用（若有）
        // - 碰撞箱启用（若有）
        if (!enabled) return false;
        if (!gameObject.activeInHierarchy) return false;

        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && !spriteRenderer.enabled) return false;

        var collider = GetComponent<Collider2D>();
        if (collider != null && !collider.enabled) return false;

        return true;
    }

    void Update()
    {
        if (!enabled) return;
        
        // 移除回车键检测，现在由Player统一处理
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled) return;
        
        // 如果是门对象，检查孩子是否隐藏
        if (letter == "门" && !IsChildHidden())
        {
            Debug.Log("门对象：孩子未隐藏，不允许激活高亮");
            return;
        }
        
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
    
    // 检查孩子是否隐藏
    private bool IsChildHidden()
    {
        // 查找场景中所有带有Highlight脚本的对象
        Highlight[] allHighlights = FindObjectsOfType<Highlight>();
        
        foreach (Highlight highlight in allHighlights)
        {
            if (highlight != null && highlight.letter == "孩")
            {
                // 检查孩子的SpriteRenderer是否被禁用
                SpriteRenderer childSpriteRenderer = highlight.GetComponent<SpriteRenderer>();
                if (childSpriteRenderer != null && !childSpriteRenderer.enabled)
                {
                    Debug.Log("门对象：检测到孩子已隐藏，允许激活高亮");
                    return true;
                }
                else
                {
                    Debug.Log("门对象：检测到孩子未隐藏，不允许激活高亮");
                    return false;
                }
            }
        }
        
        // 如果没有找到孩子对象，默认不允许激活
        Debug.Log("门对象：未找到孩子对象，不允许激活高亮");
        return false;
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
            
            // 播放化字音效
            if (AudioManager.Instance != null && AudioManager.Instance.sfxTransform != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxTransform);
                Debug.Log("Highlight: 播放化字音效");
            }
            
            if (misquare != null)
            {
                MiSquareController miSquareController = misquare.GetComponent<MiSquareController>();
                if (miSquareController != null)
                {
                    // 根据米字格类型设置对应的sprite
                    MiSquareController.MiZiGeType miZiGeType = miSquareController.GetMiZiGeType();
                    Debug.Log($"ChangeMi: 米字格类型为 {miZiGeType}");
                    
                    // 检查是否有对应类型的米字格sprite
                    bool hasMiZiGeSprite = miSquareController.HasMiZiGeSprite(combinedCharacter);
                    Debug.Log($"ChangeMi: 字符 '{combinedCharacter}' 是否有对应类型的米字格sprite: {hasMiZiGeSprite}");
                    
                    if (hasMiZiGeSprite)
                    {
                        // 使用对应类型的米字格sprite
                        miSquareController.SetMiSquareSprite(combinedCharacter);
                        Debug.Log($"ChangeMi: 已更新米字格为字符 '{combinedCharacter}'，使用{miZiGeType}米字格sprite");
                    }
                    else
                    {
                        // 如果没有对应类型的米字格sprite，使用普通sprite
                        miSquareController.SetNormalSprite(combinedCharacter);
                        Debug.Log($"ChangeMi: 字符 '{combinedCharacter}' 没有{miZiGeType}米字格sprite，使用普通sprite");
                    }
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

                // 合字成功后发送广播，例如 人 + 亭 -> 停 ，广播 "人亭停"
                if (BroadcastManager.Instance != null)
                {
                    string combineBroadcast = $"人{letter}{combinedCharacter}";
                    BroadcastManager.Instance.BroadcastToAll(combineBroadcast);
                    Debug.Log($"ChangeMi: 已广播合字提示 '{combineBroadcast}'");
                }
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
        
        // 播放取字音效
        if (AudioManager.Instance != null && AudioManager.Instance.sfxAcquire != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxAcquire);
            Debug.Log("Highlight: 播放取字音效");
        }
        
        if (ButtonController.Instance != null)
        {
            StringSelector stringSelector = ButtonController.Instance.GetStringSelector();
            if (stringSelector != null)
            {
                Debug.Log($"AddLetterToAvailableList: 正在添加字符 '{letter}' 到可用字符串列表");
                stringSelector.AddAvailableString(letter);
                Debug.Log($"AddLetterToAvailableList: 字符 '{letter}' 已添加到可用字符串列表");
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
        
        // 特殊处理：草对象和牒对象在教程步骤中的特殊逻辑
        bool handledByTutorial = HandleSpecialTutorialLogic();
        
        // 如果已经被教程逻辑处理，直接返回
        if (handledByTutorial)
        {
            return;
        }
        
        // collectable优先于carryletter逻辑，但"王"对象有特殊处理
        if (collectable)
        {
            // 特殊处理："王"对象只能被携带"侠"字符的玩家收集
            if (letter == "王")
            {
                if (player != null && player.CarryCharacter == "侠")
                {
                    Debug.Log($"FunctionA: 玩家携带'侠'字符，可以收集'王'对象");
                    AddLetterToAvailableList();
                    
                    // 收集"王"成功后发送广播
                    if (BroadcastManager.Instance != null)
                    {
                        BroadcastManager.Instance.BroadcastToAll("王");
                        Debug.Log($"FunctionA: 已广播收集提示 '王'");
                    }
                    
                    // 重要：重置玩家状态为"人"字
                    player.SetCarryCharacter("人");
                    Debug.Log($"FunctionA: 已将玩家状态重置为'人'字");
                    
                    Destroy(gameObject);
                    return;
                }
                else
                {
                    Debug.Log($"FunctionA: 玩家携带字符'{player?.CarryCharacter}'，不能收集'王'对象，无反应");
                    return; // 无反应，直接返回
                }
            }
            
            // 其他可收集对象的正常处理
            Debug.Log($"FunctionA: 对象 '{letter}' 是可收集的，优先添加到可用字符串列表");
            AddLetterToAvailableList();
            
            // 收集成功后发送广播
            if (BroadcastManager.Instance != null)
            {
                BroadcastManager.Instance.BroadcastToAll(letter);
                Debug.Log($"FunctionA: 已广播收集提示 '{letter}'");
            }
            
            // 销毁可收集的对象
            Debug.Log($"FunctionA: 销毁可收集对象 '{letter}'");
            Destroy(gameObject);
            return; // 直接返回，不执行后续的carryletter逻辑
        }
        
        // 只有在对象不可收集时才执行 carryletter / 其他交互逻辑
        if (player != null && !string.IsNullOrEmpty(player.CarryCharacter))
        {
            // 放宽条件：无论玩家当前携带什么字符，只要该对象在可化字列表中，均触发化字
            if (PublicData.listofhua.Contains(letter))
            {
                Debug.Log($"FunctionA: '{letter}' 在可化字列表中，触发化字（忽略玩家当前携带字符）");
                ChangeMi();
                // 仅恢复与当前携带字符对应的被禁用高亮，并禁用当前对象高亮
                TransferDisabledHighlightToCurrent();
                return;
            }
            else if (player.CarryCharacter == "侠")
            {
                // 侠字符的特殊处理
                string playerValue = PublicData.stringKeyValuePairs.ContainsKey(player.CarryCharacter) ? 
                                   PublicData.stringKeyValuePairs[player.CarryCharacter] : null;

                if (playerValue != null && playerValue == letter)
                {
                    Debug.Log($"FunctionA: 玩家携带'侠'字符与对象 '{letter}' 匹配，调用BroadcastCarryLetterValue");
                    BroadcastCarryLetterValue(player.CarryCharacter);
                }
                else
                {
                    Debug.Log($"FunctionA: 玩家携带'侠'字符，但对象 '{letter}' 不匹配，无反应");
                }
            }
            else
            {
                // 其他字符的处理
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
    }

    // 恢复所有此前因互动占用而禁用的对象，然后禁用当前对象并打标
    private void TransferDisabledHighlightToCurrent()
    {
        Highlight[] allHighlights = FindObjectsOfType<Highlight>();
        int restoredCount = 0;
        foreach (Highlight h in allHighlights)
        {
            if (h != null && h != this && !h.enabled)
            {
                if (h.disabledByInteraction)
                {
                    h.enabled = true;
                    h.disabledByInteraction = false;
                    restoredCount++;
                }
            }
        }

        // 兼容历史：若没有任何对象打过标记，则恢复可化字列表中目前被禁用的对象
        if (restoredCount == 0)
        {
            foreach (Highlight h in allHighlights)
            {
                if (h != null && h != this && !h.enabled && PublicData.listofhua.Contains(h.letter))
                {
                    h.enabled = true;
                    restoredCount++;
                }
            }
        }

        // 禁用当前对象，并标记为因互动占用而禁用
        enabled = false;
        disabledByInteraction = true;
        Debug.Log($"Highlight: 已恢复 {restoredCount} 个此前被占用禁用的对象，并将当前对象 '{gameObject.name}' 置为禁用");
    }
    
    // 处理教程中的特殊逻辑
    private bool HandleSpecialTutorialLogic()
    {
        Debug.Log($"FunctionA: HandleSpecialTutorialLogic - letter='{letter}', TutorialManager.Instance={TutorialManager.Instance != null}");
        
        // 检查是否在MoveToGrass步骤中
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsInMoveToGrassStep())
        {
            Debug.Log("FunctionA: 当前在MoveToGrass步骤中");
            if (letter == "草" && player != null)
            {
                Debug.Log($"FunctionA: 在MoveToGrass步骤中，玩家与草交互，执行特殊逻辑。玩家当前携带字符: '{player.CarryCharacter}'");
                
                // 显示草的子物体"虫"
                ShowChongChildObject();
                
                // 添加"虫"到可用字符串列表
                AddChongToAvailableList();
                
                // 通知TutorialManager虫已显示，可以进入下一步
                NotifyTutorialManagerChongShown();
                
                return true; // 已处理
            }
        }
        
        // 检查是否在MoveToDie步骤中
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsInMoveToDieStep())
        {
            Debug.Log("FunctionA: 当前在MoveToDie步骤中");
            if (letter == "牒" && player != null)
            {
                Debug.Log($"FunctionA: 在MoveToDie步骤中，玩家与牒交互，执行特殊逻辑。玩家当前携带字符: '{player.CarryCharacter}'");
                
                // 设置玩家携带"牒"字
                player.SetCarryCharacter("牒");
                
                // 添加"牒"到可用字符串列表
                AddDieToAvailableList();
                
                // 隐藏"牒"对象
                HideObject();
                
                // 通知TutorialManager牒已显示，可以进入下一步
                NotifyTutorialManagerDieShown();
                
                return true; // 已处理
            }
            else
            {
                Debug.Log($"FunctionA: 在MoveToDie步骤中，但条件不匹配 - letter='{letter}', player={player != null}");
            }
        }
        else
        {
            Debug.Log("FunctionA: 不在MoveToDie步骤中");
        }
        
        return false; // 未处理
    }
    
    // 通知TutorialManager虫已显示
    private void NotifyTutorialManagerChongShown()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnChongShown();
        }
    }
    
    // 通知TutorialManager牒已显示
    private void NotifyTutorialManagerDieShown()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnDieShown();
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
        // 播放取字音效
        if (AudioManager.Instance != null && AudioManager.Instance.sfxAcquire != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxAcquire);
            Debug.Log("Highlight: 播放取字音效（虫）");
        }
        
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
    
    // 添加"牒"到可用字符串列表
    private void AddDieToAvailableList()
    {
        // 播放取字音效
        if (AudioManager.Instance != null && AudioManager.Instance.sfxAcquire != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxAcquire);
            Debug.Log("Highlight: 播放取字音效（牒）");
        }
        
        if (ButtonController.Instance != null)
        {
            StringSelector stringSelector = ButtonController.Instance.GetStringSelector();
            if (stringSelector != null)
            {
                Debug.Log("FunctionA: 正在添加字符 '牒' 到可用字符串列表");
                stringSelector.AddAvailableString("牒");
                Debug.Log("FunctionA: 字符 '牒' 已添加到可用字符串列表");
            }
            else
            {
                Debug.LogError("FunctionA: StringSelector为空，无法添加字符 '牒'");
            }
        }
        else
        {
            Debug.LogError("FunctionA: ButtonController.Instance为空，无法添加字符 '牒'");
        }
    }
    
    // 添加"门"到可用字符串列表
    private void AddDoorToAvailableList()
    {
        // 播放取字音效
        if (AudioManager.Instance != null && AudioManager.Instance.sfxAcquire != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxAcquire);
            Debug.Log("Highlight: 播放取字音效（门）");
        }
        
        if (ButtonController.Instance != null)
        {
            StringSelector stringSelector = ButtonController.Instance.GetStringSelector();
            if (stringSelector != null)
            {
                Debug.Log("Highlight: 正在添加字符 '门' 到可用字符串列表");
                stringSelector.AddAvailableString("门");
                Debug.Log("Highlight: 字符 '门' 已添加到可用字符串列表");
            }
            else
            {
                Debug.LogError("Highlight: StringSelector为空，无法添加字符 '门'");
            }
        }
        else
        {
            Debug.LogError("Highlight: ButtonController.Instance为空，无法添加字符 '门'");
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
                // 雨停后切换BGM为bgmSunny并停止雨声环境音
                SwitchToSunnyBGM();
            }
        }
        else if (broadcastedValue == "侠")
        {
            Debug.Log($"收到'侠'广播，当前对象letter={letter}");
            if (letter == "王")
            {
                Debug.Log($"处理'王'对象，开始隐藏对象并添加到可用列表");
                HideObject();
                // 如果对象是可收集的，添加到可用字符串列表
                if (collectable)
                {
                    AddLetterToAvailableList();
                    
                    // 收集"王"成功后发送广播
                    if (BroadcastManager.Instance != null)
                    {
                        BroadcastManager.Instance.BroadcastToAll("王");
                        Debug.Log($"收到'侠'广播: 已广播收集提示 '王'");
                    }
                }
            }
        }
        
    }
    
    // 切换到晴天BGM并停止雨声
    private void SwitchToSunnyBGM()
    {
        if (AudioManager.Instance != null)
        {
            // 切换到晴天BGM
            if (AudioManager.Instance.bgmSunny != null)
            {
                AudioManager.Instance.CrossfadeToBGM(AudioManager.Instance.bgmSunny, 2f);
                Debug.Log("Highlight: 雨停后切换到bgmSunny");
            }
            else
            {
                Debug.LogWarning("Highlight: bgmSunny音频片段未设置");
            }
            
            // 停止雨声环境音
            AudioManager.Instance.StopAmbient(2f);
            Debug.Log("Highlight: 雨停后停止雨声环境音");
        }
        else
        {
            Debug.LogWarning("Highlight: 未找到AudioManager实例");
        }
        
        // 切换到晴天背景
        SwitchToSunnyBackground();
    }
    
    /// <summary>
    /// 切换到晴天背景
    /// </summary>
    private void SwitchToSunnyBackground()
    {
        // 查找场景中的BackgroundManager
        BackgroundManager backgroundManager = FindObjectOfType<BackgroundManager>();
        if (backgroundManager != null)
        {
            backgroundManager.SwitchToSunnyBackground();
            Debug.Log("Highlight: 雨停后切换到晴天背景");
        }
        else
        {
            Debug.LogWarning("Highlight: 未找到BackgroundManager实例");
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
        
        // 如果是门对象被隐藏，添加"门"到可用字符串列表
        if (letter == "门")
        {
            Debug.Log("门对象被收集，添加'门'到可用字符串列表");
            AddDoorToAvailableList();
            
            // 门对象收集成功后发送广播
            if (BroadcastManager.Instance != null)
            {
                BroadcastManager.Instance.BroadcastToAll("门");
                Debug.Log("HideObject: 已广播收集提示 '门'");
            }
            
            // 门对象现在和其他收集元素保持一致：交互后正常隐藏
            Debug.Log("门对象：交互后正常隐藏，与其他收集元素保持一致");
        }
    }
    
    public void ShowObject()
    {
        // 如果是门对象，检查孩子是否隐藏
        if (letter == "门" && !IsChildHidden())
        {
            Debug.Log($"门对象：孩子未隐藏，不允许显示和激活高亮: {gameObject.name}");
            return;
        }
        
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
        Debug.Log($"ExecuteSpecialLogic: 执行特殊逻辑，对象letter='{letter}'，collectable={collectable}");
        
        // 如果对象是可收集的，添加到可用字符串列表
        if (collectable)
        {
            Debug.Log($"ExecuteSpecialLogic: 对象 '{letter}' 是可收集的，添加到可用字符串列表");
            AddLetterToAvailableList();
        }
    }
    
    // 公共方法：触发交互（由Player调用）
    public void TriggerInteraction()
    {
        if (enabled && isHighlighted)
        {
            // 如果是门对象，检查孩子是否隐藏
            if (letter == "门" && !IsChildHidden())
            {
                Debug.Log("门对象：孩子未隐藏，不允许交互");
                return;
            }
            
            FunctionA();
        }
    }
}
