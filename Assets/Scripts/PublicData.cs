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
    
    public static List<string> targetList = new List<string>()
    {
        "金", "相", "便", "间"
    };
    
    public static Dictionary<string, Transform> targetPositionDict = new Dictionary<string, Transform>();
    
    // 跟踪已合成的目标字符
    public static HashSet<string> completedTargets = new HashSet<string>();
    
    // 静态场景名称
    public static string sceneName;
    

    
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
    };

    // AutoHint 使用的字典：键与 stringKeyValuePairs 相同，值稍后由用户填写
    public static Dictionary<string, string> autoHintDict = new Dictionary<string, string>(){
        {"停", "意至「停」雨，云开日出"},
        {"休", "人倚木「休」，猎人歇息，猛虎现身"},
        {"伙", "人亦为「伙」，伙伴相随，孩童离去"},
        
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
        {"拼日", "日照入门中，方寸之「间」"},
        {"拼日一", "日下加一笔，成炯炯之「目」"},
        {"拼木目", "以「目」观「木」，是为「相」"},
        {"拼人更", "有「人」在侧，「更」易行事，此为「便」"},
        {"拼王人", "「王」得「人」心，则江山「全」"},
        {"拼全丷", "「全」字加盖「丷」，点缀成「金」"},

        // 选择数量限制提示
        {"select_limit", "最多只能选择两个字"},

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
                Debug.Log("PublicData: 已停止当前场景的BGM");
            }*/
            
            // 所有目标完成，切换到下一个场景
            if (!string.IsNullOrEmpty(sceneName))
            {
                Debug.Log($"所有目标完成，切换到场景: {sceneName}");
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning("场景名称未设置，无法切换场景");
            }
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
                Debug.Log($"禁用门的highlight: {highlight.gameObject.name}");
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
