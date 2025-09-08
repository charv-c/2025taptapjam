using UnityEngine;
using UnityEditor;

/// <summary>
/// Level3Manager的自定义编辑器，显示收集列表和季节管理
/// </summary>
[CustomEditor(typeof(Level3Manager))]
public class Level3ManagerEditor : Editor
{
    private SerializedProperty currentSeasonProp;
    private SerializedProperty seasonTransitionDurationProp;
    private SerializedProperty enableSeasonTransitionProp;
    private SerializedProperty showDebugInfoProp;
    private SerializedProperty collectedStringsProp;

    private void OnEnable()
    {
        // 获取所有序列化属性
        currentSeasonProp = serializedObject.FindProperty("currentSeason");
        seasonTransitionDurationProp = serializedObject.FindProperty("seasonTransitionDuration");
        enableSeasonTransitionProp = serializedObject.FindProperty("enableSeasonTransition");
        showDebugInfoProp = serializedObject.FindProperty("showDebugInfo");
        collectedStringsProp = serializedObject.FindProperty("collectedStrings");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 绘制默认的Inspector，但排除我们要自定义的属性
        DrawPropertiesExcluding(serializedObject, 
            "currentSeason", "seasonTransitionDuration", "enableSeasonTransition", 
            "showDebugInfo", "collectedStrings");

        EditorGUILayout.Space();
        
        // 季节设置
        EditorGUILayout.LabelField("季节管理", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(currentSeasonProp, new GUIContent("当前季节"));
        EditorGUILayout.PropertyField(seasonTransitionDurationProp, new GUIContent("季节切换持续时间"));
        EditorGUILayout.PropertyField(enableSeasonTransitionProp, new GUIContent("启用季节切换动画"));
        
        EditorGUILayout.Space();
        
        // 获取Level3Manager引用
        Level3Manager manager = target as Level3Manager;
        
        // 季节切换按钮
        EditorGUILayout.LabelField("季节切换", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("切换到春季"))
        {
            if (manager != null)
            {
                manager.SwitchToSpring();
            }
        }
        
        if (GUILayout.Button("切换到夏季"))
        {
            if (manager != null)
            {
                manager.SwitchToSummer();
            }
        }
        
        if (GUILayout.Button("切换季节"))
        {
            if (manager != null)
            {
                manager.ToggleSeason();
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // 收集设置
        EditorGUILayout.LabelField("收集管理", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(collectedStringsProp, new GUIContent("已收集字符串"), true);
        
        // 显示收集统计信息
        if (manager != null)
        {
            int collectedCount = manager.GetCollectedCount();
            EditorGUILayout.LabelField($"已收集数量: {collectedCount}");
            
            if (collectedCount > 0)
            {
                var collectedStrings = manager.GetCollectedStrings();
                string collectedList = string.Join(", ", collectedStrings);
                EditorGUILayout.LabelField($"已收集内容: {collectedList}");
            }
        }
        
        EditorGUILayout.Space();
        
        // 收集管理按钮
        EditorGUILayout.LabelField("收集操作", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("清空收集列表"))
        {
            if (manager != null)
            {
                manager.ClearCollectedStrings();
            }
        }
        
        if (GUILayout.Button("显示收集列表"))
        {
            if (manager != null)
            {
                manager.DebugShowCollectedStrings();
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // 调试设置
        EditorGUILayout.LabelField("调试设置", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(showDebugInfoProp, new GUIContent("显示调试信息"));

        serializedObject.ApplyModifiedProperties();
    }
}
