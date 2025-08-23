using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;

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
        EditorGUILayout.LabelField("Highlight对象信息", EditorStyles.boldLabel);
        
        // 显示基本信息
        EditorGUILayout.LabelField($"对象名称: {highlight.gameObject.name}");
        EditorGUILayout.LabelField($"脚本启用: {highlight.enabled}");
        
        // 显示组件状态
        SpriteRenderer spriteRenderer = highlight.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            EditorGUILayout.LabelField($"SpriteRenderer: {(spriteRenderer.enabled ? "启用" : "禁用")}");
        }
        
        Collider2D collider = highlight.GetComponent<Collider2D>();
        if (collider != null)
        {
            EditorGUILayout.LabelField($"Collider2D: {(collider.enabled ? "启用" : "禁用")}");
        }
        
        Light2D[] lights = highlight.GetComponentsInChildren<Light2D>();
        EditorGUILayout.LabelField($"Light2D组件数量: {lights.Length}");
        
        // 显示对象可见性
        bool isVisible = highlight.GetComponent<SpriteRenderer>()?.enabled ?? false;
        EditorGUILayout.LabelField($"对象可见性: {(isVisible ? "可见" : "隐藏")}");
        
        EditorGUILayout.Space();
        
        // 添加一些有用的按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("显示对象"))
        {
            highlight.SendMessage("ShowObject", SendMessageOptions.DontRequireReceiver);
        }
        if (GUILayout.Button("隐藏对象"))
        {
            highlight.SendMessage("HideObject", SendMessageOptions.DontRequireReceiver);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // 显示使用说明
        EditorGUILayout.HelpBox(
            "使用说明:\n" +
            "1. 设置Letter属性为对应的字符\n" +
            "2. 设置Collectable属性决定是否可收集\n" +
            "3. 确保对象有Collider2D组件用于交互\n" +
            "4. 确保对象有Light2D子物体用于高亮效果", 
            MessageType.Info);
    }
}
