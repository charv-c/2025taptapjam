using UnityEngine;
using UnityEditor;

public class BuildSettingsHelper : EditorWindow
{
    [MenuItem("工具/窗口设置助手")]
    public static void ShowWindow()
    {
        GetWindow<BuildSettingsHelper>("窗口设置助手");
    }
    
    void OnGUI()
    {
        GUILayout.Label("打包窗口设置", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        // 全屏设置
        GUILayout.Label("全屏设置", EditorStyles.boldLabel);
        bool fullScreenMode = EditorGUILayout.Toggle("启动时全屏", PlayerSettings.fullScreenMode == FullScreenMode.FullScreenWindow);
        if (fullScreenMode)
        {
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else
        {
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        }
        
        EditorGUILayout.Space();
        
        // 分辨率设置
        GUILayout.Label("分辨率设置", EditorStyles.boldLabel);
        
        // 默认分辨率
        int defaultWidth = PlayerSettings.defaultScreenWidth;
        int defaultHeight = PlayerSettings.defaultScreenHeight;
        
        defaultWidth = EditorGUILayout.IntField("默认宽度", defaultWidth);
        defaultHeight = EditorGUILayout.IntField("默认高度", defaultHeight);
        
        PlayerSettings.defaultScreenWidth = defaultWidth;
        PlayerSettings.defaultScreenHeight = defaultHeight;
        
        EditorGUILayout.Space();
        
        // 支持的分辨率
        GUILayout.Label("支持的分辨率", EditorStyles.boldLabel);
        
        // 添加常用分辨率
        if (GUILayout.Button("添加 1920x1080"))
        {
            AddResolution(1920, 1080);
        }
        
        if (GUILayout.Button("添加 1366x768"))
        {
            AddResolution(1366, 768);
        }
        
        if (GUILayout.Button("添加 1280x720"))
        {
            AddResolution(1280, 720);
        }
        
        if (GUILayout.Button("添加 1024x768"))
        {
            AddResolution(1024, 768);
        }
        
        EditorGUILayout.Space();
        
        // 其他设置
        GUILayout.Label("其他设置", EditorStyles.boldLabel);
        
        // 允许全屏切换
        bool allowFullscreenSwitch = EditorGUILayout.Toggle("允许全屏切换", PlayerSettings.allowFullscreenSwitch);
        PlayerSettings.allowFullscreenSwitch = allowFullscreenSwitch;
        
        // 运行在后台
        bool runInBackground = EditorGUILayout.Toggle("后台运行", PlayerSettings.runInBackground);
        PlayerSettings.runInBackground = runInBackground;
        
        // 捕获鼠标
        bool captureSingleScreen = EditorGUILayout.Toggle("捕获鼠标", PlayerSettings.captureSingleScreen);
        PlayerSettings.captureSingleScreen = captureSingleScreen;
        
        EditorGUILayout.Space();
        
        // 应用设置按钮
        if (GUILayout.Button("应用所有设置"))
        {
            ApplyAllSettings();
        }
        
        EditorGUILayout.Space();
        
        // 显示当前设置
        GUILayout.Label("当前设置", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("全屏模式", PlayerSettings.fullScreenMode.ToString());
        EditorGUILayout.LabelField("默认分辨率", $"{PlayerSettings.defaultScreenWidth}x{PlayerSettings.defaultScreenHeight}");
        EditorGUILayout.LabelField("允许全屏切换", PlayerSettings.allowFullscreenSwitch.ToString());
        EditorGUILayout.LabelField("后台运行", PlayerSettings.runInBackground.ToString());
    }
    
    private void AddResolution(int width, int height)
    {
        // 检查系统支持的分辨率
        Resolution[] systemResolutions = Screen.resolutions;
        bool exists = false;
        
        foreach (Resolution res in systemResolutions)
        {
            if (res.width == width && res.height == height)
            {
                exists = true;
                break;
            }
        }
        
        if (!exists)
        {
            Debug.LogWarning($"系统不支持分辨率: {width}x{height}");
            Debug.Log($"建议在Player Settings中手动添加此分辨率");
        }
        else
        {
            Debug.Log($"系统支持分辨率: {width}x{height}");
            Debug.Log($"建议在Player Settings中添加此分辨率");
        }
        
        // 显示当前Player Settings中的分辨率设置
        Debug.Log($"当前Player Settings默认分辨率: {PlayerSettings.defaultScreenWidth}x{PlayerSettings.defaultScreenHeight}");
    }
    
    private void ApplyAllSettings()
    {
        // 保存设置
        AssetDatabase.SaveAssets();
        
        Debug.Log("所有窗口设置已应用");
        
        // 显示应用结果
        EditorUtility.DisplayDialog("设置完成", 
            $"窗口设置已应用：\n" +
            $"全屏模式: {PlayerSettings.fullScreenMode}\n" +
            $"默认分辨率: {PlayerSettings.defaultScreenWidth}x{PlayerSettings.defaultScreenHeight}\n" +
            $"允许全屏切换: {PlayerSettings.allowFullscreenSwitch}\n" +
            $"后台运行: {PlayerSettings.runInBackground}", 
            "确定");
    }
}
