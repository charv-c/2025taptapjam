using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PublicData : MonoBehaviour
{
    [Header("字符图片映射")]
    [SerializeField] private List<CharacterSpriteMapping> characterSpriteMappings = new List<CharacterSpriteMapping>();
    

    
    // 字符串分割映射字典，存储分割前的字符串和分割后的两部分
    public static Dictionary<string, (string, string)> stringSplitMappings = new Dictionary<string, (string, string)>()
    {
        {"玩", ("王", "元")},
        {"明", ("日", "月")},
        {"林", ("木", "木")},
        {"众", ("人", "人")},
        {"好", ("女", "子")},
        {"休", ("人", "木")},
        {"信", ("人", "言")},
        {"你", ("人", "尔")},
        {"他", ("人", "也")},
        {"们", ("人", "门")}
    };
    
    // 运行时字典，用于快速查找字符对应的Sprite
    private static Dictionary<string, Sprite> characterSprites = new Dictionary<string, Sprite>();
    
    void Awake()
    {
        // 初始化sprite字典
        InitializeSpriteDictionary();
    }
    
    // 初始化sprite字典
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
