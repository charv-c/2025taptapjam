using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PublicData : MonoBehaviour
{
    [Header("字符图片映射")]
    [SerializeField] private List<CharacterSpriteMapping> characterSpriteMappings = new List<CharacterSpriteMapping>();
    

    
    [Header("左米字格图片映射")]
    [SerializeField] private List<CharacterSpriteMapping> leftMiZiGeSpriteMappings = new List<CharacterSpriteMapping>();
    
    [Header("右米字格图片映射")]
    [SerializeField] private List<CharacterSpriteMapping> rightMiZiGeSpriteMappings = new List<CharacterSpriteMapping>();
    
    [Header("目标字符列表")]
    [SerializeField] private List<string> target = new List<string>()
    {
        "金", "相", "便", "间"
    };
    
    [Header("目标位置设置")]
    [SerializeField] private List<CharacterTransformMapping> targetPositionMappings = new List<CharacterTransformMapping>();
    
    [Header("场景设置")]
    [SerializeField] private string nextSceneName = "NextLevel";
    
    [Header("字符串拆分映射")]
    [SerializeField] private List<StringSplitMapping> stringSplitMappingsList = new List<StringSplitMapping>();
    
    [Header("化字列表")]
    [SerializeField] private List<string> listofhuaList = new List<string>();
    
    [Header("字符串键值对映射")]
    [SerializeField] private List<StringKeyValueMapping> stringKeyValuePairsList = new List<StringKeyValueMapping>();
    
    [Header("自动提示字典")]
    [SerializeField] private List<StringKeyValueMapping> autoHintDictList = new List<StringKeyValueMapping>();
    
    
    public static List<string> targetList = new List<string>()
    {
        "金", "相", "便", "间"
    };
    
    public static Dictionary<string, Transform> targetPositionDict = new Dictionary<string, Transform>();
    
    // 跟踪已合成的目标字符
    public static HashSet<string> completedTargets = new HashSet<string>();
    
    // 静态场景名称
    public static string sceneName;
    
    // 新增：数据驱动的关卡序列
    public static readonly string[] LevelSequence = { "level1", "level2", "level3" };
    
    // 新增：用于存储不同关卡通关后的背景图
    public static Dictionary<string, Sprite> LevelEndBackgrounds = new Dictionary<string, Sprite>();

    // 汉字拆分规则
    public static Dictionary<string, (string, string)> stringSplitMappings = new Dictionary<string, (string, string)>()
    {
        {"闪", ("门", "人")},
        {"休", ("人", "木")},
        {"停", ("亭", "人")},
        {"丛", ("从", "一")},
        {"仙", ("人", "山")},
        {"伙", ("人", "火")},
        {"粳", ("米", "更")},
        {"米", ("丷", "木")},
        {"从", ("人", "人")},
        {"全", ("王", "人")},
        {"目", ("日", "一")},
        {"大", ("人", "一")},
        {"昌", ("日", "日")},
        {"侠", ("人", "夹")},
        {"伏", ("人", "犬")},  // 添加伏字的映射
        {"牒", ("片", "枼")},  // 添加牒字的映射
        {"蝶", ("虫", "枼")},
        {"本", ("木", "一")},

        {"金", ("全", "丷")},
        {"相", ("木", "目")},
        {"便", ("人", "更")},
        {"间", ("门", "日")},
        
        // Level3彩蛋：王字可拆分为一+土
        {"王", ("一", "土")},
    };
    
    public static List<string> listofhua = new List<string>()
    {
        "亭", "山", "火", "木", "夹", "日", "犬",
    };
    
    public static Dictionary<string, string> stringKeyValuePairs = new Dictionary<string, string>()
    {
        {"停", "雨"},
        {"休", "猎"},
        {"侠", "王"},
        {"伙", "孩"},
        {"仙", "日"},
        {"蚜", "叶"},
        {"孟", "生"},
    };

    // AutoHint 使用的字典：键与 stringKeyValuePairs 相同，值稍后由用户填写
    public static Dictionary<string, string> autoHintDict = new Dictionary<string, string>(){
        // Level2 相关提示
        {"停", "意至「停」雨，云开日出"},
        {"休", "人倚木「休」，猎人歇息，猛虎现身"},
        {"伙", "人亦为「伙」，伙伴相随，孩童离去"},
        {"蚜", "「蚜」食绿「叶」，藤退散现山洞"},
        {"孟", "「孟」点「生」悟，人去日晷留"},
        
        {"人火伙", "人火相伴，结以为「伙」"},
        {"人亭停", "人入亭下，是为暂「停」"},
        {"人山仙", "人居山中，是为「仙」"},
        {"人夹侠", "行于山「夹」间者，谓之「侠」"},
        {"人木休", "人倚木而息，得以「休」"},

        {"门", "轻推柴扉，取得「门」字"},
        {"粳", "田中取禾，得「粳」米之粹"},
        {"丛", "探草木深处，觅得「丛」字"},
        {"日", "感「仙」人之力，摘得「日」轮"},
        {"王", "「侠」者伏猛虎，终成「王」"},

        {"拆粳", "「粳」字拆分，得「米」与「更」"},
        {"拆米", "剖析「米」粒，得「木」与「丷」"},
        {"拆丛", "「丛」林万象，归于「一」人相「从」"},
        {"拆从", "「从」字之本，在于「人」各有「人」随"},

        {"拼日门", "日照入门中，方寸之「间」"},
        {"拼一日", "日下加一笔，成炯炯之「目」"},
        {"拼木目", "以「目」观「木」，是为「相」"},
        {"拼人更", "有「人」在侧，「更」易行事，此为「便」"},
        {"拼人王", "「王」得「人」心，则江山「全」"},
        {"拼丷全", "「全」字加盖「丷」，点缀成「金」"},

        // Level3 相关提示
        {"琴默认提示", "一把「解语琴」，似乎可以解读文字背后的另一面"},
        {"琴季", "拨动「季」节之弦，春夏随心意而变"},
        
        {"子禾季", "「子」入「禾」下，是为四时之「季」"},
        {"子米籽", "「米」中之「子」，是为草木之「籽」"},
        {"子瓜孤", "「子」与「瓜」分，孑然一身是为「孤」"},
        {"子皿孟", "「子」立「皿」上，有先贤之风，是为「孟」"},
        {"牙艹芽", "「牙」遇「草」木，破土而出为「芽」"},
        {"牙隹雅", "「牙」侧有鸟「隹」，其鸣高「雅」"},
        {"牙虫蚜", "食叶之「虫」，附于草木为「蚜」"},
        {"牙穴穿", "以「牙」钻「穴」，破壁而「穿」"},

        {"琴雅", "「雅」音反转，琴弦拨动，得市井「俗」"},
        {"琴孤", "「孤」苦不再，琴音慰藉，得心中「欣」"},
        {"时", "日晷之意，寸寸光阴，可得「时」"},
        {"童", "孩童嬉戏，拾得无邪之「童」"},

        // Level3 拼字提示
        {"拼欠谷", "心有所「欠」，如空「谷」传响，是为「欲」"},
        {"拼人寸", "以「人」之手，度「寸」心，倾囊相「付」"},
        {"拼日立", "「日」光下「立」影，言语有声是为「音」"},
        {"拼口斤", "以「口」为耳，字字千「斤」，侧耳「听」"},

        // Level3 拆字提示
        {"拆俗", "市井「俗」事，不过「人」之五「谷」"},
        {"拆欣", "千「斤」担消，心中无「欠」，是为「欣」"},
        {"拆时", "「时」光流转，可见「日」升「寸」阴"},
        {"拆童", "「童」年时光，便是「立」于乡「里」"},
        {"拆里", "乡「里」人家，依「日」而息，靠「土」而生"},
        {"拆日", "「日」出东方，为天「口」添「一」横"},

        // 滩涂互动提示
        {"芽夏季", "「芽」逢盛夏，终得绽放成「花」"},
        {"芽春季", "「芽」喜盛夏，待季节更迭再试吧"},
        {"籽春季", "「籽」遇春风，悄然破土成「芽」"},
        {"籽夏季", "「籽」需春意，待季节更迭再试吧"},
        {"芽变瓜", "盛夏一至，「芽」已长成为「瓜」"},
        {"滩涂描述", "一片湿润滩涂，土质肥沃，适合生命成长"},
        
        // Level3关卡特殊彩蛋提示
        {"拼一土", "人得「一」「土」，可称为「王」"},

        // 选择数量限制提示
        {"select_limit", "最多只能选择两个字"},

        // 琴的默认提示
        

    };
    
    private static Dictionary<string, Sprite> characterSprites = new Dictionary<string, Sprite>();
    private static Dictionary<string, Sprite> leftMiZiGeSprites = new Dictionary<string, Sprite>();
    private static Dictionary<string, Sprite> rightMiZiGeSprites = new Dictionary<string, Sprite>();
    
    void Awake()
    {
        targetList.Clear();
        targetList.AddRange(target);
        
        // 初始化场景名称
        sceneName = nextSceneName;
        
        
        targetPositionDict.Clear();
        foreach (var mapping in targetPositionMappings)
        {
            if (!string.IsNullOrEmpty(mapping.character) && mapping.targetTransform != null)
            {
                targetPositionDict[mapping.character] = mapping.targetTransform;
            }
        }
        
        InitializeSpriteDictionary();
        InitializeStringMappings();
    }
    
    private void InitializeSpriteDictionary()
    {
        characterSprites.Clear();
        foreach (var mapping in characterSpriteMappings)
        {
            if (!string.IsNullOrEmpty(mapping.character) && mapping.sprite != null)
            {
                characterSprites[mapping.character] = mapping.sprite;
            }
        }
        

        // 初始化左米字格图片映射
        leftMiZiGeSprites.Clear();
        foreach (var mapping in leftMiZiGeSpriteMappings)
        {
            if (!string.IsNullOrEmpty(mapping.character) && mapping.sprite != null)
            {
                leftMiZiGeSprites[mapping.character] = mapping.sprite;
            }
        }
        
        // 初始化右米字格图片映射
        rightMiZiGeSprites.Clear();
        foreach (var mapping in rightMiZiGeSpriteMappings)
        {
            if (!string.IsNullOrEmpty(mapping.character) && mapping.sprite != null)
            {
                rightMiZiGeSprites[mapping.character] = mapping.sprite;
            }
        }
    }
    
    private void InitializeStringMappings()
    {
        // 初始化字符串拆分映射
        stringSplitMappings.Clear();
        foreach (var mapping in stringSplitMappingsList)
        {
            if (!string.IsNullOrEmpty(mapping.key) && !string.IsNullOrEmpty(mapping.value1) && !string.IsNullOrEmpty(mapping.value2))
            {
                stringSplitMappings[mapping.key] = (mapping.value1, mapping.value2);
            }
        }
        
        // 初始化化字列表
        listofhua.Clear();
        listofhua.AddRange(listofhuaList);
        
        // 初始化字符串键值对映射
        stringKeyValuePairs.Clear();
        foreach (var mapping in stringKeyValuePairsList)
        {
            if (!string.IsNullOrEmpty(mapping.key) && !string.IsNullOrEmpty(mapping.value))
            {
                stringKeyValuePairs[mapping.key] = mapping.value;
            }
        }
        
        // 初始化自动提示字典（不清空现有内容，只添加Inspector中配置的内容）
        foreach (var mapping in autoHintDictList)
        {
            if (!string.IsNullOrEmpty(mapping.key) && !string.IsNullOrEmpty(mapping.value))
            {
                autoHintDict[mapping.key] = mapping.value;
            }
        }
    }
    
    public static Sprite GetCharacterSprite(string character)
    {
        if (characterSprites.ContainsKey(character))
        {
            return characterSprites[character];
        }
        return null;
    }
    

    public static Sprite GetLeftMiZiGeSprite(string character)
    {
        if (leftMiZiGeSprites.ContainsKey(character))
        {
            return leftMiZiGeSprites[character];
        }
        return null;
    }
    
    public static Sprite GetRightMiZiGeSprite(string character)
    {
        if (rightMiZiGeSprites.ContainsKey(character))
        {
            return rightMiZiGeSprites[character];
        }
        return null;
    }
    

    public static bool HasLeftMiZiGeSprite(string character)
    {
        return leftMiZiGeSprites.ContainsKey(character);
    }
    
    public static bool HasRightMiZiGeSprite(string character)
    {
        return rightMiZiGeSprites.ContainsKey(character);
    }
    

    public static List<string> GetAllLeftMiZiGeCharacters()
    {
        return new List<string>(leftMiZiGeSprites.Keys);
    }
    
    public static List<string> GetAllRightMiZiGeCharacters()
    {
        return new List<string>(rightMiZiGeSprites.Keys);
    }
    
    public static string EnsureLegal(string character, string operation)
    {
        string result = FindOriginalString(character, operation);
        if (result != null)
        {
            return result;
        }
        return null;
    }
    
    public static Sprite GetResultSprite(string character, string operation)
    {
        string result = EnsureLegal(character, operation);
        if (result != null)
        {
            return GetCharacterSprite(result);
        }
        return null;
    }
    
    public static (string, string) GetStringSplit(string originalString)
    {
        if (stringSplitMappings.ContainsKey(originalString))
        {
            return stringSplitMappings[originalString];
        }
        return (originalString, "");
    }
    
    public static bool CanSplitString(string originalString)
    {
        return stringSplitMappings.ContainsKey(originalString);
    }
    
    public static List<string> GetAllSplittableStrings()
    {
        return new List<string>(stringSplitMappings.Keys);
    }
    
    public static string FindOriginalString(string part1, string part2)
    {
        foreach (var kvp in stringSplitMappings)
        {
            var storedPart1 = kvp.Value.Item1;
            var storedPart2 = kvp.Value.Item2;
            
            if ((storedPart1 == part1 && storedPart2 == part2) ||
                (storedPart1 == part2 && storedPart2 == part1))
            {
                return kvp.Key;
            }
        }
        return null;
    }
    
    public static bool IsCharacterInTargetList(string character)
    {
        return targetList.Contains(character);
    }
    
    public static Transform GetTargetPositionForCharacter(string character)
    {
        if (targetPositionDict.ContainsKey(character))
        {
            return targetPositionDict[character];
        }
        return null;
    }
    
    public List<string> GetTargetList()
    {
        return new List<string>(target);
    }
    
    // 标记目标字符为已完成
    public static void MarkTargetAsCompleted(string character)
    {
        if (targetList.Contains(character))
        {
            completedTargets.Add(character);
            // 从目标列表中移除已完成的字符
            targetList.Remove(character);
            CheckAllTargetsCompleted();
        }
    }
    
    // 检查是否所有目标都已完成
    public static bool AreAllTargetsCompleted()
    {
        // 计算总目标数量（已完成 + 未完成）
        int totalTargets = completedTargets.Count + targetList.Count;
        // 检查是否所有目标都已完成（已完成数量等于总目标数量）
        return completedTargets.Count == totalTargets && totalTargets > 0;
    }
    
    // 检查所有目标完成状态
    private static void CheckAllTargetsCompleted()
    {
        if (AreAllTargetsCompleted())
        {
            // 在切换场景前禁用门的highlight
            DisableDoorHighlights();
            
            // 停止当前场景的BGM，避免在下一场景中重复播放
            /*if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopBGM();
                GameLogger.LogDev("PublicData: 已停止当前场景的BGM");
            }*/
            
            GameLogger.LogDev($"PublicData: 所有目标完成，当前场景: {SceneManager.GetActiveScene().name}");
            
            // 保持原有逻辑：不直接切换场景，而是让LevelManager的Update()检测到完成状态
            // LevelManager会触发OnLevelCompleted事件，Level2Manager/Level3Manager会响应并显示closingMessages
            // 这样确保了完整的流程：目标完成 → closingMessages → GameFlowManager.CompleteLevel → EndLevel
            GameLogger.LogDev("PublicData: 关卡完成检测已触发，等待LevelManager处理后续流程");
        }
    }
    
    // 禁用所有门的highlight
    private static void DisableDoorHighlights()
    {
        // 查找场景中所有带有Highlight脚本的对象
        Highlight[] allHighlights = FindObjectsOfType<Highlight>();
        
        foreach (Highlight highlight in allHighlights)
        {
            if (highlight != null && highlight.letter == "门")
            {
                // 禁用门的Highlight组件
                highlight.enabled = false;
                GameLogger.LogDev($"禁用门的highlight: {highlight.gameObject.name}");
            }
        }
    }
    
    // 公共方法：场景切换前的通用处理
    public static void OnBeforeSceneTransition()
    {
        DisableDoorHighlights();
    }
    
    // 重置目标完成状态（用于重新开始关卡）
    public static void ResetTargetCompletion()
    {
        completedTargets.Clear();
    }
    
    // 获取完成进度
    public static float GetCompletionProgress()
    {
        int totalTargets = completedTargets.Count + targetList.Count;
        if (totalTargets == 0) return 0f;
        return (float)completedTargets.Count / totalTargets;
    }
    
    // 获取未完成的目标列表
    public static List<string> GetIncompleteTargets()
    {
        List<string> incomplete = new List<string>();
        foreach (string target in targetList)
        {
            if (!completedTargets.Contains(target))
            {
                incomplete.Add(target);
            }
        }
        return incomplete;
    }
    
    // 批量设置左米字格图片映射
    public static void SetLeftMiZiGeSpriteMappings(Dictionary<string, Sprite> mappings)
    {
        leftMiZiGeSprites.Clear();
        foreach (var kvp in mappings)
        {
            if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value != null)
            {
                leftMiZiGeSprites[kvp.Key] = kvp.Value;
            }
        }
    }
    
    // 批量设置右米字格图片映射
    public static void SetRightMiZiGeSpriteMappings(Dictionary<string, Sprite> mappings)
    {
        rightMiZiGeSprites.Clear();
        foreach (var kvp in mappings)
        {
            if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value != null)
            {
                rightMiZiGeSprites[kvp.Key] = kvp.Value;
            }
        }
    }
    
    // 获取所有米字格类型的字符列表
    public static List<string> GetAllMiZiGeCharactersByType(string type)
    {
        switch (type.ToLower())
        {
            case "left":
                return GetAllLeftMiZiGeCharacters();
            case "right":
                return GetAllRightMiZiGeCharacters();
            case "default":
            default:
                return GetAllLeftMiZiGeCharacters(); // 默认返回左米字格字符
        }
    }
    
    // 公共方法：添加字符串拆分映射
    public void AddStringSplitMapping(string key, string value1, string value2)
    {
        var mapping = new StringSplitMapping { key = key, value1 = value1, value2 = value2 };
        stringSplitMappingsList.Add(mapping);
        stringSplitMappings[key] = (value1, value2);
    }
    
    // 公共方法：添加字符串键值对映射
    public void AddStringKeyValueMapping(string key, string value)
    {
        var mapping = new StringKeyValueMapping { key = key, value = value };
        stringKeyValuePairsList.Add(mapping);
        stringKeyValuePairs[key] = value;
    }
    
    // 公共方法：添加自动提示映射
    public void AddAutoHintMapping(string key, string value)
    {
        var mapping = new StringKeyValueMapping { key = key, value = value };
        autoHintDictList.Add(mapping);
        autoHintDict[key] = value;
    }
    
    // 公共方法：添加化字字符
    public void AddHuaCharacter(string character)
    {
        if (!listofhuaList.Contains(character))
        {
            listofhuaList.Add(character);
            listofhua.Add(character);
        }
    }
    
    // 公共方法：清空所有映射
    public void ClearAllMappings()
    {
        stringSplitMappingsList.Clear();
        stringKeyValuePairsList.Clear();
        autoHintDictList.Clear();
        listofhuaList.Clear();
        
        stringSplitMappings.Clear();
        stringKeyValuePairs.Clear();
        autoHintDict.Clear();
        listofhua.Clear();
    }
    
}

[System.Serializable]
public class CharacterSpriteMapping
{
    [Tooltip("字符名称")]
    public string character;
    
    [Tooltip("对应的Sprite图片")]
    public Sprite sprite;
}

[System.Serializable]
public class CharacterTransformMapping
{
    [Tooltip("字符名称")]
    public string character;
    
    [Tooltip("对应的目标位置Transform")]
    public Transform targetTransform;
}

[System.Serializable]
public class StringSplitMapping
{
    [Tooltip("原始字符串")]
    public string key;
    
    [Tooltip("拆分后的第一部分")]
    public string value1;
    
    [Tooltip("拆分后的第二部分")]
    public string value2;
}

[System.Serializable]
public class StringKeyValueMapping
{
    [Tooltip("键")]
    public string key;
    
    [Tooltip("值")]
    public string value;
}
