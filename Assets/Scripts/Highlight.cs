using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    
    [Header("Level4收集类型设置")]
    [SerializeField] private CollectType collectType = CollectType.Unconditional;
    
    /// <summary>
    /// 收集类型枚举
    /// </summary>
    public enum CollectType
    {
        [Tooltip("无条件收集：无论carryletter是什么都可以收集")]
        Unconditional,
        [Tooltip("条件收集：只有carryletter为'蛇'时才能收集")]
        SnakeOnly
    }

    [Header("显示/隐藏设置")]
    [Tooltip("是否在开始时隐藏（仅禁用渲染/碰撞/光照，不会禁用GameObject或组件）")]
    [SerializeField] private bool startHidden = false;
    [Tooltip("是否连同所有子节点的SpriteRenderer一并切换显示状态")]
    [SerializeField] private bool affectChildSpriteRenderers = true;

    private SpriteRenderer cachedSpriteRenderer;
    private Collider2D cachedCollider2D;
    
    void Awake()
    {
        // 缓存常用组件（即使物体或子物体初始为隐藏也能获取）
        cachedSpriteRenderer = GetComponent<SpriteRenderer>();
        cachedCollider2D = GetComponent<Collider2D>();
        light2d = GetComponentInChildren<Light2D>(true);
        // 防御：确保缓存的Light2D属于当前对象层级，避免场景重载后出现错挂到其他同字对象上的情况
        if (light2d != null && !light2d.transform.IsChildOf(transform))
        {
            light2d = null;
        }
        // 若未找到或被判定为外部对象，则在本对象下创建一个专属的灯光子物体，避免共享/错挂
        // 但是某些对象不需要Light2D（如酒对象）
        if (light2d == null && !ShouldSkipLight2D())
        {
            Transform lightChild = transform.Find("Light2D_Local");
            if (lightChild == null)
            {
                GameObject g = new GameObject("Light2D_Local");
                g.transform.SetParent(transform, false);
                light2d = g.AddComponent<Light2D>();
                // 基础参数：点光、较小半径，默认关闭，由后续高亮逻辑控制
                light2d.lightType = Light2D.LightType.Point;
                light2d.pointLightInnerRadius = 0.1f;
                light2d.pointLightOuterRadius = 1.5f;
                light2d.intensity = 1.0f;
                light2d.enabled = false;
                GameLogger.LogDev($"Highlight: 为 '{gameObject.name}' 创建专属Light2D子物体");
            }
            else
            {
                light2d = lightChild.GetComponent<Light2D>();
                if (light2d == null)
                {
                    light2d = lightChild.gameObject.AddComponent<Light2D>();
                }
                light2d.enabled = false;
            }
        }
        
        // 如果没有找到Light2D，记录调试信息
        if (light2d == null)
        {
            GameLogger.LogDev($"Highlight: 对象 '{gameObject.name}' 没有Light2D组件，将跳过光照相关操作");
        }
    }
    
    /// <summary>
    /// 判断是否应该跳过Light2D的创建
    /// </summary>
    /// <returns>如果应该跳过Light2D创建则返回true</returns>
    private bool ShouldSkipLight2D()
    {
        // 酒对象不需要Light2D
        if (letter == "酒")
        {
            GameLogger.LogDev($"Highlight: 酒对象 '{gameObject.name}' 跳过Light2D创建");
            return true;
        }
        
        // 可以在这里添加其他不需要Light2D的对象类型
        // if (letter == "其他对象类型")
        // {
        //     return true;
        // }
        
        return false;
    }
    
    /// <summary>
    /// 根据收集类型判断是否可以收集（仅限Level4）
    /// </summary>
    /// <returns>如果可以收集则返回true</returns>
    private bool CanCollectBasedOnType()
    {
        // 检查是否在Level4场景
        if (!IsInLevel4())
        {
            // 不在Level4场景，使用默认的无条件收集逻辑
            return true;
        }
        
        // 在Level4场景中，根据收集类型判断
        switch (collectType)
        {
            case CollectType.Unconditional:
                GameLogger.LogDev($"Highlight: 对象 '{letter}' 为无条件收集类型，可以收集");
                return true;
                
            case CollectType.SnakeOnly:
                if (player != null && player.CarryCharacter == "蛇")
                {
                    GameLogger.LogDev($"Highlight: 对象 '{letter}' 为蛇条件收集类型，玩家携带'蛇'，可以收集");
                    return true;
                }
                else
                {
                    GameLogger.LogDev($"Highlight: 对象 '{letter}' 为蛇条件收集类型，玩家携带'{player?.CarryCharacter}'，无法收集");
                    return false;
                }
                
            default:
                GameLogger.LogWarning($"Highlight: 未知的收集类型 {collectType}，默认允许收集");
                return true;
        }
    }
    
    /// <summary>
    /// 检查当前是否在Level4场景
    /// </summary>
    /// <returns>如果在Level4场景则返回true</returns>
    private bool IsInLevel4()
    {
        // 通过场景名称判断是否为Level4
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isLevel4 = sceneName.Contains("Level4") || sceneName.Contains("level4");
        
        if (isLevel4)
        {
            GameLogger.LogDev($"Highlight: 当前场景为Level4: {sceneName}");
        }
        
        return isLevel4;
    }
    
    /// <summary>
    /// 移动到指定名称的对象位置
    /// </summary>
    /// <param name="targetObjectName">目标对象的名称</param>
    private void MoveToTargetObject(string targetObjectName)
    {
        // 在场景中查找目标对象
        GameObject targetObject = GameObject.Find(targetObjectName);
        
        if (targetObject == null)
        {
            GameLogger.LogWarning($"Highlight: 未找到名为 '{targetObjectName}' 的目标对象");
            return;
        }
        
        Vector3 targetPosition = targetObject.transform.position;
        Vector3 currentPosition = transform.position;
        
        GameLogger.LogDev($"Highlight: 商对象 '{gameObject.name}' 从位置 {currentPosition} 移动到 {targetPosition}");
        
        // 直接设置位置（如果需要平滑移动，可以使用协程或DOTween）
        transform.position = targetPosition;
        
        GameLogger.LogDev($"Highlight: 商对象 '{gameObject.name}' 移动完成，当前位置: {transform.position}");
    }
    
    /// <summary>
    /// 显示指定名称的对象
    /// </summary>
    /// <param name="targetObjectName">目标对象的名称</param>
    private void ShowTargetObject(string targetObjectName)
    {
        // 在场景中查找目标对象（包括未激活的对象）
        GameObject targetObject = FindObjectByName(targetObjectName);
        
        if (targetObject == null)
        {
            GameLogger.LogWarning($"Highlight: 未找到名为 '{targetObjectName}' 的目标对象");
            return;
        }
        
        GameLogger.LogDev($"Highlight: 找到目标对象 '{targetObjectName}'，当前状态: activeInHierarchy={targetObject.activeInHierarchy}");
        
        // 直接激活GameObject
        if (!targetObject.activeInHierarchy)
        {
            GameLogger.LogDev($"Highlight: 激活目标对象 '{targetObjectName}'");
            targetObject.SetActive(true);
        }
        else
        {
            GameLogger.LogDev($"Highlight: 目标对象 '{targetObjectName}' 已经激活");
        }
    }
    
    /// <summary>
    /// 根据名称查找对象（包括未激活的对象）
    /// </summary>
    /// <param name="objectName">对象名称</param>
    /// <returns>找到的对象，如果未找到返回null</returns>
    private GameObject FindObjectByName(string objectName)
    {
        // 首先尝试使用GameObject.Find（只能找到激活的对象）
        GameObject targetObject = GameObject.Find(objectName);
        if (targetObject != null)
        {
            return targetObject;
        }
        
        // 如果GameObject.Find找不到，遍历所有对象（包括未激活的）
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == objectName && obj.scene.isLoaded)
            {
                GameLogger.LogDev($"Highlight: 在未激活对象中找到 '{objectName}': {obj.name}");
                return obj;
            }
        }
        
        return null;
    }

    void Start()
    {
        // 确保灯光默认关闭（若存在）
        if (light2d != null)
        {
            light2d.enabled = false;
        }
        
        // 检查初始状态
        if (letter == "夹")
        {
            GameLogger.LogDev($"夹对象初始化: {gameObject.name}");
            CheckObjectStatus();
            
            // 确保Highlight组件被激活
            if (!enabled)
            {
                GameLogger.LogDev($"激活夹对象的Highlight组件: {gameObject.name}");
                enabled = true;
            }
            
            // 确保GameObject被激活
            if (!gameObject.activeInHierarchy)
            {
                GameLogger.LogDev($"激活夹对象的GameObject: {gameObject.name}");
                gameObject.SetActive(true);
            }
        }

        // 应用初始显示/隐藏状态（不禁用GameObject与组件本身）
        ApplyHiddenState(startHidden);
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
    
    /// <summary>
    /// 获取当前是否处于高亮状态
    /// </summary>
    /// <returns>是否高亮</returns>
    public bool IsHighlighted()
    {
        return isHighlighted;
    }
    
    /// <summary>
    /// 设置高亮状态
    /// </summary>
    /// <param name="highlighted">是否高亮</param>
    public void SetHighlight(bool highlighted)
    {
        isHighlighted = highlighted;
        if (light2d != null)
        {
            light2d.enabled = highlighted;
        }
        GameLogger.LogDev($"Highlight: 设置 '{gameObject.name}' 高亮状态为 {highlighted}");
    }

    private void ApplyHiddenState(bool hidden)
    {
        // 自身SpriteRenderer
        if (cachedSpriteRenderer != null)
        {
            cachedSpriteRenderer.enabled = !hidden;
        }

        // 子级SpriteRenderer
        if (affectChildSpriteRenderers)
        {
            var childSprites = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in childSprites)
            {
                sr.enabled = !hidden;
            }
        }

        // 碰撞体
        if (cachedCollider2D != null)
        {
            cachedCollider2D.enabled = !hidden;
        }

        // 灯光 - 修复：每个对象的light2d状态应该独立控制
        if (light2d != null)
        {
            // 如果对象被隐藏，直接关闭灯光
            if (hidden)
            {
                light2d.enabled = false;
            }
            else
            {
                // 如果对象显示，根据高亮状态决定灯光
                light2d.enabled = isHighlighted;
            }
        }
    }

    [ContextMenu("显示(仅禁用渲染级隐藏)")]
    public void Show()
    {
        startHidden = false;
        ApplyHiddenState(false);
        GameLogger.LogDev($"Highlight: 已显示 {gameObject.name}");
    }

    [ContextMenu("隐藏(仅禁用渲染级隐藏)")]
    public void Hide()
    {
        startHidden = true;
        ApplyHiddenState(true);
        GameLogger.LogDev($"Highlight: 已隐藏 {gameObject.name}");
    }

    // 在编辑器中改值时即刻生效
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            // 预览用的安全获取
            if (cachedSpriteRenderer == null) cachedSpriteRenderer = GetComponent<SpriteRenderer>();
            if (cachedCollider2D == null) cachedCollider2D = GetComponent<Collider2D>();
            if (light2d == null) light2d = GetComponentInChildren<Light2D>(true);
            ApplyHiddenState(startHidden);
        }
    }

    void Update()
    {
        if (!enabled) return;
        
        // 移除F键检测，现在由Player统一处理
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled) return;
        
        // 如果是门对象，检查孩子是否隐藏
        if (letter == "门" && !IsChildHidden())
        {
            GameLogger.LogDev("门对象：孩子未隐藏，不允许激活高亮");
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
            
            // 琴对象的特殊逻辑：通知QinSpecialLogic脚本
            if (letter == "琴")
            {
                QinSpecialLogic qinLogic = GetComponent<QinSpecialLogic>();
                if (qinLogic != null)
                {
                    qinLogic.OnPlayerEnter();
                }
            }
            
            // 酒对象的特殊逻辑：通知WineSpecialLogic脚本
            if (letter == "酒")
            {
                WineSpecialLogic wineLogic = GetComponent<WineSpecialLogic>();
                if (wineLogic != null)
                {
                    wineLogic.OnPlayerEnter();
                }
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
            
            // 琴对象的特殊逻辑：通知QinSpecialLogic脚本
            if (letter == "琴")
            {
                QinSpecialLogic qinLogic = GetComponent<QinSpecialLogic>();
                if (qinLogic != null)
                {
                    qinLogic.OnPlayerExit();
                }
            }
            
            // 酒对象的特殊逻辑：通知WineSpecialLogic脚本
            if (letter == "酒")
            {
                WineSpecialLogic wineLogic = GetComponent<WineSpecialLogic>();
                if (wineLogic != null)
                {
                    wineLogic.OnPlayerExit();
                }
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
                    GameLogger.LogDev("门对象：检测到孩子已隐藏，允许激活高亮");
                    return true;
                }
                else
                {
                    GameLogger.LogDev("门对象：检测到孩子未隐藏，不允许激活高亮");
                    return false;
                }
            }
        }
        
        // 如果没有找到孩子对象，默认不允许激活
        GameLogger.LogDev("门对象：未找到孩子对象，不允许激活高亮");
        return false;
    }
    
    void ChangeMi(){
        GameLogger.LogDev($"ChangeMi: 开始处理字符合成，letter='{letter}'，canControlMisquare={canControlMisquare}");
        
        if (!canControlMisquare)
        {
            GameLogger.LogWarning($"ChangeMi: canControlMisquare为false，无法控制米字格");
            return;
        }
        
        string carrier = "人";
        if (player != null)
        {
            string initial = player.GetInitialCarryCharacter();
            if (!string.IsNullOrEmpty(initial))
            {
                carrier = initial;
            }
        }
        string combinedCharacter = PublicData.FindOriginalString(letter, carrier);
        GameLogger.LogDev($"ChangeMi: 查找合成字符，letter='{letter}' + 初始字符'{carrier}' = '{combinedCharacter}'");
        
        if (combinedCharacter != null)
        {
            GameLogger.LogDev($"ChangeMi: 找到合成字符 '{combinedCharacter}'，开始更新米字格和玩家状态");
            
            // 播放化字音效
            if (AudioManager.Instance != null && AudioManager.Instance.sfxTransform != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxTransform);
                GameLogger.LogDev("Highlight: 播放化字音效");
            }
            
            if (misquare != null)
            {
                MiSquareController miSquareController = misquare.GetComponent<MiSquareController>();
                if (miSquareController != null)
                {
                    // 根据米字格类型设置对应的sprite
                    MiSquareController.MiZiGeType miZiGeType = miSquareController.GetMiZiGeType();
                    GameLogger.LogDev($"ChangeMi: 米字格类型为 {miZiGeType}");
                    
                    // 检查是否有对应类型的米字格sprite
                    bool hasMiZiGeSprite = miSquareController.HasMiZiGeSprite(combinedCharacter);
                    GameLogger.LogDev($"ChangeMi: 字符 '{combinedCharacter}' 是否有对应类型的米字格sprite: {hasMiZiGeSprite}");
                    
                    if (hasMiZiGeSprite)
                    {
                        // 使用对应类型的米字格sprite
                        miSquareController.SetMiSquareSprite(combinedCharacter);
                        GameLogger.LogDev($"ChangeMi: 已更新米字格为字符 '{combinedCharacter}'，使用{miZiGeType}米字格sprite");
                    }
                    else
                    {
                        // 如果没有对应类型的米字格sprite，使用普通sprite
                        miSquareController.SetNormalSprite(combinedCharacter);
                        GameLogger.LogDev($"ChangeMi: 字符 '{combinedCharacter}' 没有{miZiGeType}米字格sprite，使用普通sprite");
                    }
                }
                else
                {
                    GameLogger.LogWarning($"ChangeMi: 米字格对象没有MiSquareController组件");
                }
            }
            else
            {
                GameLogger.LogWarning($"ChangeMi: misquare对象为空");
            }
            
            if (player != null)
            {
                // 使用新的SetCarryCharacter方法，会自动更新米字格图片
                player.SetCarryCharacter(combinedCharacter);
                GameLogger.LogDev($"ChangeMi: 已设置玩家携带字符为 '{combinedCharacter}'");

                // 合字成功后发送广播，使用玩家的初始字符，例如 牙 + 虫 -> 蚜 ，广播 "牙虫蚜"
                if (BroadcastManager.Instance != null)
                {
                    string initialChar = player.GetInitialCarryCharacter();
                    string combineBroadcast = $"{initialChar}{letter}{combinedCharacter}";
                    BroadcastManager.Instance.BroadcastToAll(combineBroadcast);
                    GameLogger.LogDev($"ChangeMi: 已广播合字提示 '{combineBroadcast}' (初始字符='{initialChar}')");
                }
            }
            else
            {
                GameLogger.LogWarning($"ChangeMi: player对象为空");
            }
            
            // 特殊处理：如果是瓜对象完成化字，通知BeachObject恢复滩涂交互状态
            if (letter == "瓜")
            {
                NotifyBeachObjectOnGuaInteraction();
            }
        }
        else
        {
            GameLogger.LogWarning($"ChangeMi: 未找到合成字符，letter='{letter}' + '人' 无法合成");
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
        
        GameLogger.LogDev($"ChangeMi: 字符合成处理完成");
    }
    
    /// <summary>
    /// 当瓜对象完成化字互动后，通知BeachObject恢复滩涂交互状态
    /// </summary>
    private void NotifyBeachObjectOnGuaInteraction()
    {
        GameLogger.LogDev("Highlight: 瓜对象完成化字，通知BeachObject恢复滩涂交互状态");
        
        // 查找场景中的BeachObject组件
        BeachObject beachObject = FindObjectOfType<BeachObject>();
        if (beachObject != null)
        {
            // 延迟调用，确保化字逻辑完全完成后再恢复滩涂状态
            StartCoroutine(DelayedNotifyBeachObject(beachObject));
        }
        else
        {
            GameLogger.LogWarning("Highlight: 未找到BeachObject组件，无法恢复滩涂交互状态");
        }
    }
    
    /// <summary>
    /// 延迟通知BeachObject，确保瓜对象的化字逻辑完全完成
    /// </summary>
    /// <param name="beachObject">BeachObject组件引用</param>
    /// <returns>协程</returns>
    private System.Collections.IEnumerator DelayedNotifyBeachObject(BeachObject beachObject)
    {
        // 等待一帧，确保所有化字逻辑完成
        yield return null;
        
        // 调用BeachObject的方法来恢复滩涂交互状态
        beachObject.OnGuaInteractionCompleted();
        GameLogger.LogDev("Highlight: 已通知BeachObject恢复滩涂交互状态");
    }
    
    void AddLetterToAvailableList(){
        GameLogger.LogDev($"AddLetterToAvailableList: 开始添加字符 '{letter}'，collectable={collectable}");
        
        if (!collectable)
        {
            GameLogger.LogDev($"AddLetterToAvailableList: 字符 '{letter}' 不可收集，跳过");
            return;
        }
        
        // 播放取字音效
        if (AudioManager.Instance != null && AudioManager.Instance.sfxAcquire != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxAcquire);
            GameLogger.LogDev("Highlight: 播放取字音效");
        }
        
        if (ButtonController.Instance != null)
        {
            StringSelector stringSelector = ButtonController.Instance.GetStringSelector();
            if (stringSelector != null)
            {
                GameLogger.LogDev($"AddLetterToAvailableList: 正在添加字符 '{letter}' 到可用字符串列表");
                stringSelector.AddAvailableString(letter);
                GameLogger.LogDev($"AddLetterToAvailableList: 字符 '{letter}' 已添加到可用字符串列表");

                // 若当前为Level3场景，则同步记录到Level3Manager的收集列表
                var level3Manager = FindObjectOfType<Level3Manager>();
                if (level3Manager != null)
                {
                    level3Manager.AddCollectedString(letter);
                    GameLogger.LogDev($"AddLetterToAvailableList: Level3 收集记录 + '{letter}'");
                }
            }
            else
            {
                GameLogger.LogError($"AddLetterToAvailableList: StringSelector为空，无法添加字符 '{letter}'");
            }
        }
        else
        {
            GameLogger.LogError($"AddLetterToAvailableList: ButtonController.Instance为空，无法添加字符 '{letter}'");
        }
    }
    
    private void FunctionA()
    {
        GameLogger.LogDev($"FunctionA: 开始处理交互，对象letter='{letter}'，玩家携带字符='{player?.CarryCharacter}'，collectable={collectable}");
        
        // 先行处理：琴对象的交互应优先于其它分支，避免被教程/collectable等逻辑短路
        if (letter == "琴")
        {
            GameLogger.LogDev($"FunctionA: 琴对象优先处理分支，玩家携带字符: '{player?.CarryCharacter}'");
            if (player != null && !string.IsNullOrEmpty(player.CarryCharacter))
            {
                QinSpecialLogic qinLogic = GetComponent<QinSpecialLogic>();
                if (qinLogic != null)
                {
                    GameLogger.LogDev($"FunctionA: 调用QinSpecialLogic.OnPlayerInteract('{player.CarryCharacter}')");
                    qinLogic.OnPlayerInteract(player.CarryCharacter);
                    GameLogger.LogDev("FunctionA: 琴对象处理完成，返回");
                }
                else
                {
                    GameLogger.LogWarning($"FunctionA: 琴对象 '{gameObject.name}' 缺少QinSpecialLogic组件");
                }
            }
            else
            {
                GameLogger.LogDev("FunctionA: 琴对象处理时玩家携带字符为空");
            }
            return;
        }
        
        // 先行处理：酒对象的交互应优先于其它分支，避免被教程/collectable等逻辑短路
        if (letter == "酒")
        {
            GameLogger.LogDev($"FunctionA: 酒对象优先处理分支，玩家携带字符: '{player?.CarryCharacter}'");
            WineSpecialLogic wineLogic = GetComponent<WineSpecialLogic>();
            if (wineLogic != null)
            {
                string carryCharacter = player != null ? player.CarryCharacter : "";
                GameLogger.LogDev($"FunctionA: 调用WineSpecialLogic.OnPlayerInteract('{carryCharacter}', {player?.gameObject.name})");
                wineLogic.OnPlayerInteract(carryCharacter, player);
                GameLogger.LogDev("FunctionA: 酒对象处理完成，返回");
            }
            else
            {
                GameLogger.LogWarning($"FunctionA: 酒对象 '{gameObject.name}' 缺少WineSpecialLogic组件");
            }
            return;
        }
        
        // 特殊处理：草对象和牒对象在教程步骤中的特殊逻辑
        bool handledByTutorial = HandleSpecialTutorialLogic();
        
        // 如果已经被教程逻辑处理，直接返回
        if (handledByTutorial)
        {
            return;
        }
        
        // 特殊处理："滩"对象调用BeachObject脚本（无论是否可收集）
        if (letter == "滩")
        {
            Debug.Log($"Highlight: 检测到滩对象互动，玩家携带字符: '{player?.CarryCharacter}'");
            Debug.Log($"Highlight: 当前对象名称: {gameObject.name}");
            Debug.Log($"Highlight: 当前对象上的组件: {string.Join(", ", GetComponents<MonoBehaviour>().Select(c => c.GetType().Name))}");
            
            if (player != null && !string.IsNullOrEmpty(player.CarryCharacter))
            {
                // 调用BeachObject脚本的滩涂互动逻辑
                BeachObject beachObject = GetComponent<BeachObject>();
                Debug.Log($"Highlight: BeachObject组件状态: {(beachObject != null ? "存在" : "不存在")}");
                
                if (beachObject != null)
                {
                    Debug.Log($"Highlight: 调用BeachObject.ExecuteBeachInteraction('{player.CarryCharacter}')");
                    beachObject.ExecuteBeachInteraction(player.CarryCharacter);
                    Debug.Log($"Highlight: BeachObject.ExecuteBeachInteraction调用完成");
                }
                else
                {
                    Debug.LogError("Highlight: 滩对象没有BeachObject脚本组件！请在滩对象上添加BeachObject脚本。");
                    GameLogger.LogWarning("FunctionA: 滩对象没有BeachObject脚本组件");
                }
                
                // 滩涂互动不再发送广播，避免全屏提示
                
                // 滩对象不销毁，保持可重复交互
                return;
            }
            else
            {
                GameLogger.LogDev($"FunctionA: 玩家携带字符为空，滩对象无反应");
                return;
            }
        }
        
        // collectable优先于carryletter逻辑，但"王"对象有特殊处理
        if (collectable)
        {
            // 特殊处理："王"对象只能被携带"侠"字符的玩家收集
            if (letter == "王")
            {
                if (player != null && player.CarryCharacter == "侠")
                {
                    GameLogger.LogDev($"FunctionA: 玩家携带'侠'字符，可以收集'王'对象");
                    AddLetterToAvailableList();
                    
                    // 收集"王"成功后发送广播
                    if (BroadcastManager.Instance != null)
                    {
                        BroadcastManager.Instance.BroadcastToAll("王");
                        GameLogger.LogDev($"FunctionA: 已广播收集提示 '王'");
                    }
                    
                    // 重要：重置玩家状态为初始字符
                    string initialChar = player.GetInitialCarryCharacter();
                    player.SetCarryCharacter(initialChar);
                    GameLogger.LogDev($"FunctionA: 已将玩家状态重置为'{initialChar}'字符");
                    
                    Destroy(gameObject);
                    return;
                }
                else
                {
                    GameLogger.LogDev($"FunctionA: 玩家携带字符'{player?.CarryCharacter}'，不能收集'王'对象，无反应");
                    return; // 无反应，直接返回
                }
            }
            
            // 特殊处理："琴"对象委托给QinSpecialLogic脚本处理
            if (letter == "琴")
            {
                GameLogger.LogDev($"FunctionA: 琴对象互动开始，玩家携带字符: '{player?.CarryCharacter}'");
                
                if (player != null && !string.IsNullOrEmpty(player.CarryCharacter))
                {
                    // 委托给QinSpecialLogic脚本处理
                    QinSpecialLogic qinLogic = GetComponent<QinSpecialLogic>();
                    if (qinLogic != null)
                    {
                        GameLogger.LogDev($"FunctionA: 调用QinSpecialLogic.OnPlayerInteract('{player.CarryCharacter}')");
                        qinLogic.OnPlayerInteract(player.CarryCharacter);
                        GameLogger.LogDev($"FunctionA: QinSpecialLogic.OnPlayerInteract调用完成");
                    }
                    else
                    {
                        GameLogger.LogWarning($"FunctionA: 琴对象 '{gameObject.name}' 没有QinSpecialLogic脚本组件");
                    }
                    
                    // 琴对象不销毁，保持可重复交互
                    return;
                }
                else
                {
                    GameLogger.LogDev($"FunctionA: 玩家携带字符为空，琴对象无反应");
                    return;
                }
            }
            
            // 其他可收集对象的正常处理
            // 检查收集类型条件（仅限Level4）
            if (!CanCollectBasedOnType())
            {
                GameLogger.LogDev($"FunctionA: 对象 '{letter}' 收集条件不满足，无法收集");
                return;
            }
            
            GameLogger.LogDev($"FunctionA: 对象 '{letter}' 是可收集的，优先添加到可用字符串列表");
            AddLetterToAvailableList();
            
            // 收集成功后发送广播
            if (BroadcastManager.Instance != null)
            {
                BroadcastManager.Instance.BroadcastToAll(letter);
                GameLogger.LogDev($"FunctionA: 已广播收集提示 '{letter}'");
            }
            
            // 销毁可收集的对象
            GameLogger.LogDev($"FunctionA: 销毁可收集对象 '{letter}'");
            Destroy(gameObject);
            return; // 直接返回，不执行后续的carryletter逻辑
        }
        
        // 只有在对象不可收集时才执行 carryletter / 其他交互逻辑
        if (player != null && !string.IsNullOrEmpty(player.CarryCharacter))
        {
            // 放宽条件：无论玩家当前携带什么字符，只要该对象在可化字列表中，均触发化字
            if (PublicData.listofhua.Contains(letter))
            {
                GameLogger.LogDev($"FunctionA: '{letter}' 在可化字列表中，触发化字（忽略玩家当前携带字符）");
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
                    GameLogger.LogDev($"FunctionA: 玩家携带'侠'字符与对象 '{letter}' 匹配，调用BroadcastCarryLetterValue");
                    BroadcastCarryLetterValue(player.CarryCharacter);
                }
                else
                {
                    GameLogger.LogDev($"FunctionA: 玩家携带'侠'字符，但对象 '{letter}' 不匹配，无反应");
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
                        GameLogger.LogDev($"FunctionA: 玩家携带字符 '{player.CarryCharacter}' 与对象 '{letter}' 匹配，调用BroadcastCarryLetterValue");
                        BroadcastCarryLetterValue(player.CarryCharacter);
                    }
                }
            }
        }
        else
        {
            GameLogger.LogWarning($"FunctionA: 玩家为空或携带字符为空，玩家={player}, CarryCharacter='{player?.CarryCharacter}'");
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
        GameLogger.LogDev($"Highlight: 已恢复 {restoredCount} 个此前被占用禁用的对象，并将当前对象 '{gameObject.name}' 置为禁用");
    }
    
    // 处理教程中的特殊逻辑
    private bool HandleSpecialTutorialLogic()
    {
        GameLogger.LogDev($"FunctionA: HandleSpecialTutorialLogic - letter='{letter}', TutorialManager.Instance={TutorialManager.Instance != null}");
        
        // 检查是否在MoveToGrass步骤中
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsInMoveToGrassStep())
        {
            GameLogger.LogDev("FunctionA: 当前在MoveToGrass步骤中");
            if (letter == "草" && player != null)
            {
                GameLogger.LogDev($"FunctionA: 在MoveToGrass步骤中，玩家与草交互，执行特殊逻辑。玩家当前携带字符: '{player.CarryCharacter}'");
                
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
            GameLogger.LogDev("FunctionA: 当前在MoveToDie步骤中");
            if (letter == "牒" && player != null)
            {
                GameLogger.LogDev($"FunctionA: 在MoveToDie步骤中，玩家与牒交互，执行特殊逻辑。玩家当前携带字符: '{player.CarryCharacter}'");
                
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
                GameLogger.LogDev($"FunctionA: 在MoveToDie步骤中，但条件不匹配 - letter='{letter}', player={player != null}");
            }
        }
        else
        {
            GameLogger.LogDev("FunctionA: 不在MoveToDie步骤中");
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
            GameLogger.LogDev($"FunctionA: 已显示草的子物体'虫': {chongChild.gameObject.name}");
        }
        else
        {
            GameLogger.LogWarning($"FunctionA: 未找到草对象的子物体'虫'");
        }
    }
    
    // 添加"虫"到可用字符串列表
    private void AddChongToAvailableList()
    {
        // 播放取字音效
        if (AudioManager.Instance != null && AudioManager.Instance.sfxAcquire != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxAcquire);
            GameLogger.LogDev("Highlight: 播放取字音效（虫）");
        }
        
        if (ButtonController.Instance != null)
        {
            StringSelector stringSelector = ButtonController.Instance.GetStringSelector();
            if (stringSelector != null)
            {
                GameLogger.LogDev("FunctionA: 正在添加字符 '虫' 到可用字符串列表");
                stringSelector.AddAvailableString("虫");
                GameLogger.LogDev("FunctionA: 字符 '虫' 已添加到可用字符串列表");
            }
            else
            {
                GameLogger.LogError("FunctionA: StringSelector为空，无法添加字符 '虫'");
            }
        }
        else
        {
            GameLogger.LogError("FunctionA: ButtonController.Instance为空，无法添加字符 '虫'");
        }
    }
    
    // 添加"牒"到可用字符串列表
    private void AddDieToAvailableList()
    {
        // 播放取字音效
        if (AudioManager.Instance != null && AudioManager.Instance.sfxAcquire != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxAcquire);
            GameLogger.LogDev("Highlight: 播放取字音效（牒）");
        }
        
        if (ButtonController.Instance != null)
        {
            StringSelector stringSelector = ButtonController.Instance.GetStringSelector();
            if (stringSelector != null)
            {
                GameLogger.LogDev("FunctionA: 正在添加字符 '牒' 到可用字符串列表");
                stringSelector.AddAvailableString("牒");
                GameLogger.LogDev("FunctionA: 字符 '牒' 已添加到可用字符串列表");
            }
            else
            {
                GameLogger.LogError("FunctionA: StringSelector为空，无法添加字符 '牒'");
            }
        }
        else
        {
            GameLogger.LogError("FunctionA: ButtonController.Instance为空，无法添加字符 '牒'");
        }
    }
    
    // 添加"门"到可用字符串列表
    private void AddDoorToAvailableList()
    {
        // 播放取字音效
        if (AudioManager.Instance != null && AudioManager.Instance.sfxAcquire != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxAcquire);
            GameLogger.LogDev("Highlight: 播放取字音效（门）");
        }
        
        if (ButtonController.Instance != null)
        {
            StringSelector stringSelector = ButtonController.Instance.GetStringSelector();
            if (stringSelector != null)
            {
                GameLogger.LogDev("Highlight: 正在添加字符 '门' 到可用字符串列表");
                stringSelector.AddAvailableString("门");
                GameLogger.LogDev("Highlight: 字符 '门' 已添加到可用字符串列表");
            }
            else
            {
                GameLogger.LogError("Highlight: StringSelector为空，无法添加字符 '门'");
            }
        }
        else
        {
            GameLogger.LogError("Highlight: ButtonController.Instance为空，无法添加字符 '门'");
        }
    }
    
    // 通用：添加任意值到可用字符串列表
    private void AddValueToAvailableList(string value)
    {
        if (string.IsNullOrEmpty(value)) return;
        // 播放取字音效（与其他添加函数保持一致）
        if (AudioManager.Instance != null && AudioManager.Instance.sfxAcquire != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxAcquire);
            GameLogger.LogDev($"Highlight: 播放取字音效（{value}）");
        }
        
        if (ButtonController.Instance != null)
        {
            StringSelector stringSelector = ButtonController.Instance.GetStringSelector();
            if (stringSelector != null)
            {
                GameLogger.LogDev($"Highlight: 正在添加字符 '{value}' 到可用字符串列表");
                stringSelector.AddAvailableString(value);
                GameLogger.LogDev($"Highlight: 字符 '{value}' 已添加到可用字符串列表");
            }
            else
            {
                GameLogger.LogError($"Highlight: StringSelector为空，无法添加字符 '{value}'");
            }
        }
        else
        {
            GameLogger.LogError($"Highlight: ButtonController.Instance为空，无法添加字符 '{value}'");
        }
    }
    

    
    private void BroadcastCarryLetterValue(string carryLetter)
    {
        if (player != null)
        {
            // 使用新的SetCarryCharacter方法，会自动更新米字格图片
            string initialChar = player.GetInitialCarryCharacter();
            player.SetCarryCharacter(initialChar);
        }
        
        if (BroadcastManager.Instance != null)
        {
            BroadcastManager.Instance.BroadcastToAll(carryLetter);
        }
    }
    

    
    public void ReceiveBroadcast(string broadcastedValue)
    {
        GameLogger.LogDev($"收到广播: {broadcastedValue}, 当前对象: {gameObject.name}, letter: {letter}");
        GameLogger.LogDev($"对象状态 - GameObject active: {gameObject.activeInHierarchy}, Highlight enabled: {enabled}");
        
        // 如果Highlight组件被禁用，重新激活它
        if (!enabled)
        {
            GameLogger.LogDev($"重新激活Highlight组件: {gameObject.name}");
            enabled = true;
        }
        
        // 如果GameObject被禁用，重新激活它
        if (!gameObject.activeInHierarchy)
        {
            GameLogger.LogDev($"重新激活GameObject: {gameObject.name}");
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
            GameLogger.LogDev($"收到'休'广播，当前对象letter={letter}");
            if (letter == "猎")
            {
                GameLogger.LogDev($"隐藏猎对象: {gameObject.name}");
                // 播放猎人离去音效
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayHunterLeave();
                }
                HideObject();
            }
            else if (letter == "王")
            {
                GameLogger.LogDev($"显示王对象: {gameObject.name}");
                ShowObject();
            }
            else if (letter == "夹")
            {
                GameLogger.LogDev($"显示夹对象: {gameObject.name}");
                ShowObject();
            }
        }
        else if (broadcastedValue == "伙")
        {
            if (letter == "孩")
            {
                // 播放孩童笑声音效 (Level2)
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayChildLaugh();
                }
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
            GameLogger.LogDev($"收到'侠'广播，当前对象letter={letter}");
            if (letter == "王")
            {
                GameLogger.LogDev($"处理'王'对象，开始隐藏对象并添加到可用列表");
                HideObject();
                // 如果对象是可收集的，添加到可用字符串列表
                if (collectable)
                {
                    AddLetterToAvailableList();
                    
                    // 收集"王"成功后发送广播
                    if (BroadcastManager.Instance != null)
                    {
                        BroadcastManager.Instance.BroadcastToAll("王");
                        GameLogger.LogDev($"收到'侠'广播: 已广播收集提示 '王'");
                    }
                }
            }
        }
        else if (broadcastedValue == "蚜")
        {
            GameLogger.LogDev($"收到'蚜'广播，当前对象letter={letter}");
            if (letter == "叶")
            {
                // 播放虫子吃叶子音效 (Level3)
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayBugEatLeaf();
                }
                HideObject();
            }
            else if (letter == "穴")
            {
                ShowObject();
            }
        }
        else if (broadcastedValue == "穿")
        {
            GameLogger.LogDev($"收到'穿'广播，当前对象letter={letter}");
            if (letter == "老")
            {
                // 播放孩童笑声音效 (Level3 - 老人变为孩童)
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayChildLaugh();
                }
                HideObject();
            }
            else if (letter == "童")
            {
                ShowObject();
            }
        }
        else if (broadcastedValue == "皇")
        {
            GameLogger.LogDev($"收到'皇'广播，当前对象letter={letter}");
            if (letter == "民")
            {
                GameLogger.LogDev($"隐藏民对象: {gameObject.name}");
                HideObject();
            }
            if (letter == "乔")
            {
                GameLogger.LogDev($"显示乔对象: {gameObject.name}");
                ShowObject();
            }
        }
        else if (broadcastedValue == "帛")
        {
            GameLogger.LogDev($"收到'帛'广播，当前对象letter={letter}");
            if (letter == "商")
            {
                // 调用商特殊逻辑组件
                ShangSpecialLogic shangLogic = GetComponent<ShangSpecialLogic>();
                if (shangLogic != null)
                {
                    shangLogic.OnBoBroadcast();
                }
                else
                {
                    GameLogger.LogWarning($"Highlight: 商对象没有ShangSpecialLogic组件 - {gameObject.name}");
                }
            }
            if (letter == "椟")
            {
                GameLogger.LogDev($"显示椟对象: {gameObject.name}");
                ShowObject();
            }
        }
        else if (broadcastedValue == "柏")
        {
            GameLogger.LogDev($"收到'柏'广播，当前对象letter={letter}");
            if (letter == "鼠")
            {
                GameLogger.LogDev($"隐藏鼠对象: {gameObject.name}");
                HideObject();
            }
            if (letter == "维")
            {
                GameLogger.LogDev($"显示维对象: {gameObject.name}");
                ShowObject();
                // 设置维为可收集状态
                collectable = true;
                GameLogger.LogDev($"维对象已设置为可收集状态: {gameObject.name}");
            }
        }
        else if (broadcastedValue == "清")
        {
            GameLogger.LogDev($"收到'清'广播，当前对象letter={letter}");
            if (letter == "枯花")
            {
                GameLogger.LogDev($"隐藏枯对象: {gameObject.name}");
                ShowTargetObject("鲜花");
                HideObject();
            }
            else if (letter == "胡")
            {
                GameLogger.LogDev($"显示胡对象: {gameObject.name}");
                ShowObject();
            }
            
            // 显示场景中名为"鲜花"的物体（所有对象都会执行这个逻辑）
            
        }
        else if (broadcastedValue == "睛")
        {
            GameLogger.LogDev($"收到'睛'广播，当前对象letter={letter}");
            if (letter == "汉")
            {
                GameLogger.LogDev($"显示汉对象: {gameObject.name}");
                ShowObject();
            }
        }
        else if (broadcastedValue == "孟")
        {
            GameLogger.LogDev($"收到'孟'广播，当前对象letter={letter}");
            if (letter == "生")
            {
                // 播放书生恍然大悟音效 (Level3)
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayScholarEnlighten();
                }
                HideObject();
            }
            else if (letter == "时")
            {
                ShowObject();
            }
        }
        // 移除"琴雅"和"琴孤"的处理逻辑，改由Level3Manager统一处理
        // 这样避免了每个Highlight对象都重复执行，导致重复添加字符串
        else if (broadcastedValue == "季夏")
        {
            GameLogger.LogDev($"收到'季夏'广播，当前对象letter={letter}");
            if (letter == "芽")
            {
                HideObject();
            }
            // 移除直接显示瓜物体的逻辑，让BeachObject.TransformYaToGuaOnSeasonChange()统一控制
            // 只有芽物体显示时才会显示瓜，确保逻辑一致性
        }
        else if (broadcastedValue == "芽")
        {
            GameLogger.LogDev($"收到'芽'广播，当前对象letter={letter}");
            // 1) 显示场景中没有Highlight脚本的"花"Sprite物体
            ShowFlowerSpriteWithoutHighlight();
            // 2) 启用 letter == "鸟" 的高亮对象
            EnableHighlightByLetter("隹");
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
                GameLogger.LogDev("Highlight: 雨停后切换到bgmSunny");
            }
            else
            {
                GameLogger.LogWarning("Highlight: bgmSunny音频片段未设置");
            }
            
            // 停止雨声环境音
            AudioManager.Instance.StopAmbient(2f);
            GameLogger.LogDev("Highlight: 雨停后停止雨声环境音");
        }
        else
        {
            GameLogger.LogWarning("Highlight: 未找到AudioManager实例");
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
            backgroundManager.SwitchToSwappedState();
            GameLogger.LogDev("Highlight: 雨停后切换到晴天背景");
        }
        else
        {
            GameLogger.LogWarning("Highlight: 未找到BackgroundManager实例");
        }
    }

    public void HideObject()
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
            GameLogger.LogDev("门对象被收集，添加'门'到可用字符串列表");
            AddDoorToAvailableList();
            
            // 门对象收集成功后发送广播
            if (BroadcastManager.Instance != null)
            {
                BroadcastManager.Instance.BroadcastToAll("门");
                GameLogger.LogDev("HideObject: 已广播收集提示 '门'");
            }
            
            // 门对象现在和其他收集元素保持一致：交互后正常隐藏
            GameLogger.LogDev("门对象：交互后正常隐藏，与其他收集元素保持一致");
        }
    }
    
    public void ShowObject()
    {
        // 如果是门对象，检查孩子是否隐藏
        if (letter == "门" && !IsChildHidden())
        {
            GameLogger.LogDev($"门对象：孩子未隐藏，不允许显示和激活高亮: {gameObject.name}");
            return;
        }
        
        // 确保Highlight组件被激活
        if (!enabled)
        {
            GameLogger.LogDev($"激活Highlight组件: {gameObject.name}");
            enabled = true;
        }
        
        // 确保GameObject被激活
        if (!gameObject.activeInHierarchy)
        {
            GameLogger.LogDev($"激活GameObject: {gameObject.name}");
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
            GameLogger.LogDev($"激活碰撞箱: {gameObject.name}, 碰撞箱类型: {collider.GetType()}, 是否为Trigger: {collider.isTrigger}");
        }
        else
        {
            GameLogger.LogWarning($"未找到碰撞箱: {gameObject.name}");
        }
        
        Light2D[] allLights = GetComponentsInChildren<Light2D>(true);
        foreach (Light2D light in allLights)
        {
            if (light != null)
            {
                light.gameObject.SetActive(true);
                GameLogger.LogDev($"激活灯光: {light.gameObject.name}");
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
        
        GameLogger.LogDev($"显示对象完成: {gameObject.name}");
        
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
        
        GameLogger.LogDev($"检查'夹'对象的广播接收状态: {gameObject.name}");
        
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
                            GameLogger.LogDev($"✓ 找到当前'夹'对象: {gameObject.name}");
                        }
                    }
                }
            }
            
            GameLogger.LogDev($"场景中共有 {highlightCount} 个Highlight组件，当前'夹'对象是否被找到: {foundThisObject}");
        }
    }
    
    private void CheckObjectStatus()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Collider2D collider = GetComponent<Collider2D>();
        
        GameLogger.LogDev($"对象状态检查 - {gameObject.name}:");
        GameLogger.LogDev($"  SpriteRenderer enabled: {spriteRenderer?.enabled}");
        GameLogger.LogDev($"  Collider2D enabled: {collider?.enabled}");
        GameLogger.LogDev($"  Collider2D isTrigger: {collider?.isTrigger}");
        GameLogger.LogDev($"  GameObject active: {gameObject.activeInHierarchy}");
        GameLogger.LogDev($"  Component enabled: {enabled}");
    }
    
    private void ExecuteSpecialLogic()
    {
        // 特殊逻辑实现
        GameLogger.LogDev($"ExecuteSpecialLogic: 执行特殊逻辑，对象letter='{letter}'，collectable={collectable}");
        
        // 如果对象是可收集的，添加到可用字符串列表
        if (collectable)
        {
            GameLogger.LogDev($"ExecuteSpecialLogic: 对象 '{letter}' 是可收集的，添加到可用字符串列表");
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
                GameLogger.LogDev("门对象：孩子未隐藏，不允许交互");
                return;
            }
            
            FunctionA();
        }
    }
    
    // 显示场景中没有Highlight脚本的"花"Sprite物体
    private void ShowFlowerSpriteWithoutHighlight()
    {
        // 查找场景中所有GameObject
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            // 检查对象名称是否包含"花"
            if (obj.name.Contains("花"))
            {
                // 检查该对象是否有Highlight脚本
                Highlight highlightComponent = obj.GetComponent<Highlight>();
                if (highlightComponent == null)
                {
                    // 没有Highlight脚本，显示该对象
                    SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.enabled = true;
                        GameLogger.LogDev($"显示没有Highlight脚本的'花'对象: {obj.name}");
                    }
                    
                    // 确保GameObject是激活的
                    if (!obj.activeInHierarchy)
                    {
                        obj.SetActive(true);
                        GameLogger.LogDev($"激活没有Highlight脚本的'花'对象: {obj.name}");
                    }
                }
            }
        }
    }
    
    // 启用指定letter的Highlight对象
    private void EnableHighlightByLetter(string targetLetter)
    {
        Highlight[] allHighlights = FindObjectsOfType<Highlight>(true);
        foreach (Highlight highlight in allHighlights)
        {
            if (highlight != null && highlight.letter == targetLetter)
            {
                highlight.ShowObject();
                GameLogger.LogDev($"启用letter为'{targetLetter}'的Highlight对象: {highlight.gameObject.name}");
            }
        }
    }
    
    
}
