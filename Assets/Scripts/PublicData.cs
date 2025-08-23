using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PublicData : MonoBehaviour
{
    [Header("字符图片映射")]
    [SerializeField] private List<CharacterSpriteMapping> characterSpriteMappings = new List<CharacterSpriteMapping>();
    
    [Header("米字格图片映射")]
    [SerializeField] private List<CharacterSpriteMapping> miZiGeSpriteMappings = new List<CharacterSpriteMapping>();
    
    [Header("目标字符列表")]
    [SerializeField] private List<string> target = new List<string>()
    {
        "金", "相", "便", "间"
    };
    
    // 静态target列表，供其他脚本直接访问
    public static List<string> targetList = new List<string>()
    {
        "金", "相", "便", "间"
    };
    
    [Header("目标位置设置")]
    // 目标位置字典，键是字符，值是transform
    [SerializeField] private List<CharacterTransformMapping> targetPositionMappings = new List<CharacterTransformMapping>();
    
    // 静态目标位置字典，供其他脚本直接访问
    public static Dictionary<string, Transform> targetPositionDict = new Dictionary<string, Transform>();
    

    
    // 字符串分割映射字典，存储分割前的字符串和分割后的两部分
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

        {"金", ("全", "丷")},
        {"相", ("木", "目")},
        {"便", ("人", "更")},
        {"间", ("门", "日")},
    };
    
    // 花相关的字符列表
    public static List<string> listofhua = new List<string>()
    {
        "亭", "山", "火", "木", "夹",
    };
    
    // 字符串键值对字典
    public static Dictionary<string, string> stringKeyValuePairs = new Dictionary<string, string>()
    {
        {"停", "雨"},
        {"休", "猎"},
        {"侠", "王"},
        {"伙", "孩"},
        {"仙", "日"},
    };
    
    // 运行时字典，用于快速查找字符对应的Sprite
    private static Dictionary<string, Sprite> characterSprites = new Dictionary<string, Sprite>();
    
    // 运行时字典，用于快速查找字符对应的米字格Sprite
    private static Dictionary<string, Sprite> miZiGeSprites = new Dictionary<string, Sprite>();
    
    void Awake()
    {
        // 同步Inspector中的target列表到静态列表
        targetList.Clear();
        targetList.AddRange(target);
        
        // 初始化目标位置字典
        targetPositionDict.Clear();
        foreach (var mapping in targetPositionMappings)
        {
            if (!string.IsNullOrEmpty(mapping.character) && mapping.targetTransform != null)
            {
                targetPositionDict[mapping.character] = mapping.targetTransform;
                Debug.Log($"已添加目标位置映射: '{mapping.character}' -> {mapping.targetTransform.name}");
            }
        }
        
        // 初始化sprite字典
        InitializeSpriteDictionary();
    }
    
    // 初始化sprite字典
    private void InitializeSpriteDictionary()
    {
        // 初始化普通字符字典
        characterSprites.Clear();
        foreach (var mapping in characterSpriteMappings)
        {
            if (!string.IsNullOrEmpty(mapping.character) && mapping.sprite != null)
            {
                characterSprites[mapping.character] = mapping.sprite;
            }
        }
        
        // 初始化米字格字符字典
        miZiGeSprites.Clear();
        foreach (var mapping in miZiGeSpriteMappings)
        {
            if (!string.IsNullOrEmpty(mapping.character) && mapping.sprite != null)
            {
                miZiGeSprites[mapping.character] = mapping.sprite;
            }
        }
    }
    
    // 公共方法：根据字符获取对应的Sprite
    public static Sprite GetCharacterSprite(string character)
    {
        if (characterSprites.ContainsKey(character))
        {
            return characterSprites[character];
        }
        else
        {
            Debug.LogWarning($"未找到字符 '{character}' 对应的Sprite");
            return null;
        }
    }
    
    // 公共方法：根据字符获取对应的米字格Sprite
    public static Sprite GetMiZiGeSprite(string character)
    {
        if (miZiGeSprites.ContainsKey(character))
        {
            return miZiGeSprites[character];
        }
        else
        {
            Debug.LogWarning($"未找到字符 '{character}' 对应的米字格Sprite");
            return null;
        }
    }
    
    // 公共方法：检查字符是否有对应的米字格图片
    public static bool HasMiZiGeSprite(string character)
    {
        return miZiGeSprites.ContainsKey(character);
    }
    
    // 公共方法：获取所有有米字格图片的字符列表
    public static List<string> GetAllMiZiGeCharacters()
    {
        return new List<string>(miZiGeSprites.Keys);
    }
    
    // 公共方法：检查操作是否合法，并返回对应的结果（使用stringSplitMappings）
    public static string EnsureLegal(string character, string operation)
    {
        // 使用stringSplitMappings进行反向查找
        string result = FindOriginalString(character, operation);
        if (result != null)
        {
            Debug.Log($"字符 '{character}' 和 '{operation}' 合成的结果是：'{result}'");
            return result;
        }
        
        // 如果都不存在，返回null
        Debug.LogWarning($"字典中未找到字符 '{character}' 和 '{operation}' 的任何组合定义");
        return null; // 返回null表示操作不合法
    }
    
    // 公共方法：获取操作结果对应的Sprite
    public static Sprite GetResultSprite(string character, string operation)
    {
        string result = EnsureLegal(character, operation);
        if (result != null)
        {
            return GetCharacterSprite(result);
        }
        return null;
    }
    
    // 公共方法：根据字符串获取分割后的两部分
    public static (string, string) GetStringSplit(string originalString)
    {
        if (stringSplitMappings.ContainsKey(originalString))
        {
            return stringSplitMappings[originalString];
        }
        else
        {
            Debug.LogWarning($"未找到字符串 '{originalString}' 的分割映射");
            return (originalString, ""); // 返回原字符串和空字符串
        }
    }
    
    // 公共方法：检查字符串是否可以分割
    public static bool CanSplitString(string originalString)
    {
        return stringSplitMappings.ContainsKey(originalString);
    }
    
    // 公共方法：获取所有可分割的字符串列表
    public static List<string> GetAllSplittableStrings()
    {
        return new List<string>(stringSplitMappings.Keys);
    }
    
    // 公共方法：根据两个部分查找原始字符串（反向查找，不需要顺序一致）
    public static string FindOriginalString(string part1, string part2)
    {
        // 只遍历一次字典，同时检查原始顺序和交换顺序
        foreach (var kvp in stringSplitMappings)
        {
            var storedPart1 = kvp.Value.Item1;
            var storedPart2 = kvp.Value.Item2;
            
            // 检查是否匹配（支持顺序交换）
            if ((storedPart1 == part1 && storedPart2 == part2) ||
                (storedPart1 == part2 && storedPart2 == part1))
            {
                return kvp.Key;
            }
        }
        
        Debug.LogWarning($"未找到由 '{part1}' 和 '{part2}' 组成的原始字符串（包括交换顺序）");
        return null;
    }
    
    // 公共方法：检查字符是否在target列表中
    public static bool IsCharacterInTargetList(string character)
    {
        return targetList.Contains(character);
    }
    
    // 静态方法：获取字符对应的目标位置
    public static Transform GetTargetPositionForCharacter(string character)
    {
        if (targetPositionDict.ContainsKey(character))
        {
            return targetPositionDict[character];
        }
        return null;
    }
    

    
    // 公共方法：获取target列表
    public List<string> GetTargetList()
    {
        return new List<string>(target);
    }
}

// 用于在Inspector中可视化的字符-Sprite映射结构
[System.Serializable]
public class CharacterSpriteMapping
{
    [Tooltip("字符名称")]
    public string character;
    
    [Tooltip("对应的Sprite图片")]
    public Sprite sprite;
}

// 用于在Inspector中可视化的字符-Transform映射结构
[System.Serializable]
public class CharacterTransformMapping
{
    [Tooltip("字符名称")]
    public string character;
    
    [Tooltip("对应的目标位置Transform")]
    public Transform targetTransform;
}
