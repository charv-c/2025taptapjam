using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class UISpriteAnchorHelper : EditorWindow
{
    [MenuItem("工具/RectTransform对齐Image")]
    public static void ShowWindow()
    {
        GetWindow<UISpriteAnchorHelper>("RectTransform对齐Image");
    }
    
    void OnGUI()
    {
        GUILayout.Label("RectTransform对齐Image工具", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("选中对象 - RectTransform对齐到Image"))
        {
            AlignSelectedObjectsToImage();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("场景中所有UI - RectTransform对齐到Image"))
        {
            AlignAllUIObjectsToImage();
        }
        
        EditorGUILayout.Space();
        
        // 显示帮助信息
        GUILayout.Label("使用说明", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "1. 选中要调整的UI对象\n" +
            "2. 点击对齐按钮\n" +
            "3. RectTransform会自动调整到Image的实际显示区域\n" +
            "4. 对于Sliced类型的Image，会考虑border的影响\n" +
            "5. 只调整尺寸，不改变锚点和位置", 
            MessageType.Info);
    }
    
    // 对齐选中对象到Image
    private void AlignSelectedObjectsToImage()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        int successCount = 0;
        
        foreach (GameObject obj in selectedObjects)
        {
            RectTransform rectTransform = obj.GetComponent<RectTransform>();
            Image image = obj.GetComponent<Image>();
            
            if (rectTransform != null && image != null && image.sprite != null)
            {
                AlignRectTransformToImage(rectTransform, image);
                successCount++;
                Debug.Log($"已对齐RectTransform到Image: {obj.name}");
            }
        }
        
        if (successCount > 0)
        {
            EditorUtility.DisplayDialog("完成", $"已对齐 {successCount} 个对象的RectTransform到Image", "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("提示", "请先选中包含Image组件的UI对象", "确定");
        }
    }
    
    // 对齐所有UI对象到Image
    private void AlignAllUIObjectsToImage()
    {
        Image[] allImages = FindObjectsOfType<Image>();
        int successCount = 0;
        
        foreach (Image image in allImages)
        {
            if (image.sprite != null)
            {
                RectTransform rectTransform = image.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    AlignRectTransformToImage(rectTransform, image);
                    successCount++;
                }
            }
        }
        
        EditorUtility.DisplayDialog("完成", $"已对齐场景中 {successCount} 个UI对象的RectTransform到Image", "确定");
    }
    
    // 对齐RectTransform到Image
    private void AlignRectTransformToImage(RectTransform rectTransform, Image image)
    {
        if (image.sprite == null) return;
        
        // 获取Image的实际显示区域
        Vector2 imageSize = rectTransform.sizeDelta;
        Vector2 displaySize = imageSize;
        
        // 根据Image的ImageType计算实际显示区域
        if (image.type == Image.Type.Sliced)
        {
            // 对于Sliced类型，考虑border
            Vector4 border = image.sprite.border;
            
            // 计算实际显示的内容区域
            float contentWidth = imageSize.x - (border.x + border.z);
            float contentHeight = imageSize.y - (border.y + border.w);
            
            displaySize = new Vector2(contentWidth, contentHeight);
        }
        else if (image.type == Image.Type.Simple)
        {
            // 对于Simple类型，使用Image的实际尺寸
            displaySize = imageSize;
        }
        else if (image.type == Image.Type.Tiled)
        {
            // 对于Tiled类型，使用Image的实际尺寸
            displaySize = imageSize;
        }
        
        // 只调整尺寸，不改变锚点和位置
        rectTransform.sizeDelta = displaySize;
    }
}
