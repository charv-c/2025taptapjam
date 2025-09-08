using UnityEngine;
using UnityEditor;

/// <summary>
/// HintManager的自定义编辑器，实现条件显示功能
/// </summary>
[CustomEditor(typeof(HintManager))]
public class HintManagerEditor : Editor
{
    private SerializedProperty currentSceneProp;
    private SerializedProperty rainObjectProp;
    private SerializedProperty childObjectProp;
    private SerializedProperty hunterObjectProp;
    private SerializedProperty kingObjectProp;
    private SerializedProperty sunObjectsProp;
    private SerializedProperty leafObjectProp;
    private SerializedProperty oldObjectProp;
    private SerializedProperty lifeObjectProp;
    private SerializedProperty level3CollectedStringsProp;

    private void OnEnable()
    {
        // 获取所有序列化属性
        currentSceneProp = serializedObject.FindProperty("currentScene");
        rainObjectProp = serializedObject.FindProperty("rainObject");
        childObjectProp = serializedObject.FindProperty("childObject");
        hunterObjectProp = serializedObject.FindProperty("hunterObject");
        kingObjectProp = serializedObject.FindProperty("kingObject");
        sunObjectsProp = serializedObject.FindProperty("sunObjects");
        leafObjectProp = serializedObject.FindProperty("leafObject");
        oldObjectProp = serializedObject.FindProperty("oldObject");
        lifeObjectProp = serializedObject.FindProperty("lifeObject");
        level3CollectedStringsProp = serializedObject.FindProperty("level3CollectedStrings");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 绘制默认的Inspector，但排除我们要自定义的属性
        DrawPropertiesExcluding(serializedObject, 
            "currentScene", "rainObject", "childObject", "hunterObject", "kingObject", "sunObjects",
            "leafObject", "oldObject", "lifeObject", "level3CollectedStrings");

        EditorGUILayout.Space();
        
        // 场景设置
        EditorGUILayout.LabelField("场景设置", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(currentSceneProp, new GUIContent("当前场景"));
        
        EditorGUILayout.Space();
        
        // 根据场景类型显示不同的目标引用框
        SceneType currentScene = (SceneType)currentSceneProp.enumValueIndex;
        
        if (currentScene == SceneType.Level2)
        {
            EditorGUILayout.LabelField("Level2场景目标引用", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(rainObjectProp, new GUIContent("雨 (Rain)"));
            EditorGUILayout.PropertyField(childObjectProp, new GUIContent("孩 (Child)"));
            EditorGUILayout.PropertyField(hunterObjectProp, new GUIContent("猎 (Hunter)"));
            EditorGUILayout.PropertyField(kingObjectProp, new GUIContent("王 (King)"));
            EditorGUILayout.PropertyField(sunObjectsProp, new GUIContent("日列表 (Sun Objects)"), true);
        }
        else if (currentScene == SceneType.Level3)
        {
            EditorGUILayout.LabelField("Level3场景目标引用", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(leafObjectProp, new GUIContent("叶 (Leaf)"));
            EditorGUILayout.PropertyField(oldObjectProp, new GUIContent("老 (Old)"));
            EditorGUILayout.PropertyField(lifeObjectProp, new GUIContent("生 (Life)"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Level3收集（获得过的字符串）", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(level3CollectedStringsProp, new GUIContent("已收集字符串"), true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
