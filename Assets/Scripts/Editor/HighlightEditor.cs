using UnityEngine;
using UnityEditor;

// Highlight对象的自定义编辑器
[CustomEditor(typeof(Highlight))]
public class HighlightEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Highlight highlight = (Highlight)target;
        
        // 绘制默认的Inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Misquare控制设置", EditorStyles.boldLabel);
        
        // 显示当前状态
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("当前状态:");
        string status = highlight.CanControlMisquare() ? "已启用" : "未启用";
        EditorGUILayout.LabelField(status, EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        
        // 显示misquare对象状态
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Misquare对象:");
        string misquareStatus = highlight.HasMisquareObject() ? "已设置" : "未设置";
        EditorGUILayout.LabelField(misquareStatus, EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // 控制按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("启用Misquare控制"))
        {
            highlight.EnableMisquareControl();
        }
        if (GUILayout.Button("禁用Misquare控制"))
        {
            highlight.DisableMisquareControl();
        }
        EditorGUILayout.EndHorizontal();
        
        // 检查设置是否合理
        if (highlight.CanControlMisquare() && !highlight.HasMisquareObject())
        {
            EditorGUILayout.HelpBox("警告：已启用Misquare控制但未设置Misquare对象！", MessageType.Warning);
        }
        
        if (!highlight.CanControlMisquare() && highlight.HasMisquareObject())
        {
            EditorGUILayout.HelpBox("提示：已设置Misquare对象但未启用控制功能。", MessageType.Info);
        }
        
        // 显示letter信息
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Letter信息", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"当前Letter: {highlight.GetLetter()}");
        
        // 检查letter是否在花列表中
        if (highlight.IsLetterInHuaList())
        {
            EditorGUILayout.LabelField("状态: 在化列表中", EditorStyles.boldLabel);
        }
        else
        {
            EditorGUILayout.LabelField("状态: 不在化列表中", EditorStyles.boldLabel);
        }
        
        // 显示字典值
        string dictValue = highlight.GetLetterDictionaryValue();
        if (!string.IsNullOrEmpty(dictValue))
        {
            EditorGUILayout.LabelField($"字典值: {dictValue}");
        }
        else
        {
            EditorGUILayout.LabelField("字典值: 未找到");
        }
    }
}
