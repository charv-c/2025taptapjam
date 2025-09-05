using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class BuildSettingsHelper : EditorWindow
{
    [MenuItem("工具/窗口设置助手")]
    public static void ShowWindow()
    {
        GetWindow<BuildSettingsHelper>("窗口设置助手");
    }
    
    // === 日志管理菜单项 ===
    [MenuItem("工具/日志管理/配置开发日志")]
    public static void EnableDevelopmentLogsMenu()
    {
        var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        
        if (!defines.Contains("ENABLE_DEVELOPMENT_LOGS"))
        {
            if (!string.IsNullOrEmpty(defines))
                defines += ";";
            defines += "ENABLE_DEVELOPMENT_LOGS";
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
            Debug.Log("已启用开发日志模式");
            EditorUtility.DisplayDialog("日志配置", "已启用开发日志模式\n开发环境日志将正常输出", "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("日志配置", "开发日志模式已经启用", "确定");
        }
    }

    [MenuItem("工具/日志管理/配置发布日志")]
    public static void DisableDevelopmentLogsMenu()
    {
        var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        
        // 添加禁用警告的符号
        if (!defines.Contains("DISABLE_WARNINGS"))
        {
            if (!string.IsNullOrEmpty(defines))
                defines += ";";
            defines += "DISABLE_WARNINGS";
        }
        
        // 移除开发日志符号
        defines = defines.Replace("ENABLE_DEVELOPMENT_LOGS;", "");
        defines = defines.Replace(";ENABLE_DEVELOPMENT_LOGS", "");
        defines = defines.Replace("ENABLE_DEVELOPMENT_LOGS", "");
        
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
        Debug.Log("已配置Release日志模式 - 开发日志已禁用");
        EditorUtility.DisplayDialog("日志配置", "已配置Release日志模式\n开发日志将被完全移除，仅保留错误和用户日志", "确定");
    }

    [MenuItem("工具/日志管理/显示当前配置")]
    public static void ShowCurrentLogConfigurationMenu()
    {
        string config = "";
        
        // 检查GameLogger是否存在
        var gameLoggerType = System.Type.GetType("GameLogger");
        if (gameLoggerType != null)
        {
            var method = gameLoggerType.GetMethod("GetLogConfig", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (method != null)
            {
                config = (string)method.Invoke(null, null);
            }
        }
        
        if (string.IsNullOrEmpty(config))
        {
            var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            
            bool isDevelopmentLogsEnabled = defines.Contains("ENABLE_DEVELOPMENT_LOGS");
            bool isWarningsDisabled = defines.Contains("DISABLE_WARNINGS");
            
            config = $"日志配置信息:\n";
            config += $"开发日志: {(isDevelopmentLogsEnabled ? "启用" : "禁用")}\n";
            config += $"警告日志: {(isWarningsDisabled ? "禁用" : "启用")}\n";
            config += $"编译符号: {defines}";
        }
        
        EditorUtility.DisplayDialog("当前日志配置", config, "确定");
        Debug.Log(config);
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
        
        EditorGUILayout.Space();
        
        // === 日志管理设置部分 ===
        GUILayout.Label("日志管理设置", EditorStyles.boldLabel);
        
        // 显示当前日志配置
        var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        
        bool isDevelopmentLogsEnabled = defines.Contains("ENABLE_DEVELOPMENT_LOGS");
        bool isWarningsDisabled = defines.Contains("DISABLE_WARNINGS");
        
        EditorGUILayout.LabelField("开发日志状态", isDevelopmentLogsEnabled ? "启用" : "禁用");
        EditorGUILayout.LabelField("警告日志状态", isWarningsDisabled ? "禁用" : "启用");
        EditorGUILayout.LabelField("编译符号", defines);
        
        EditorGUILayout.Space();
        
        // 日志配置按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("配置开发日志"))
        {
            ConfigureDevelopmentLogs();
        }
        
        if (GUILayout.Button("配置发布日志"))
        {
            ConfigureReleaseLogs();
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("显示日志配置信息"))
        {
            ShowLogConfiguration();
        }
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
    
    // === 日志管理方法 ===
    
    private void ConfigureDevelopmentLogs()
    {
        var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        
        if (!defines.Contains("ENABLE_DEVELOPMENT_LOGS"))
        {
            if (!string.IsNullOrEmpty(defines))
                defines += ";";
            defines += "ENABLE_DEVELOPMENT_LOGS";
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
            Debug.Log("已启用开发日志模式");
            EditorUtility.DisplayDialog("日志配置", "已启用开发日志模式\n开发环境日志将正常输出", "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("日志配置", "开发日志模式已经启用", "确定");
        }
    }

    private void ConfigureReleaseLogs()
    {
        var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        
        // 添加禁用警告的符号
        if (!defines.Contains("DISABLE_WARNINGS"))
        {
            if (!string.IsNullOrEmpty(defines))
                defines += ";";
            defines += "DISABLE_WARNINGS";
        }
        
        // 移除开发日志符号
        defines = defines.Replace("ENABLE_DEVELOPMENT_LOGS;", "");
        defines = defines.Replace(";ENABLE_DEVELOPMENT_LOGS", "");
        defines = defines.Replace("ENABLE_DEVELOPMENT_LOGS", "");
        
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
        Debug.Log("已配置Release日志模式 - 开发日志已禁用");
        EditorUtility.DisplayDialog("日志配置", "已配置Release日志模式\n开发日志将被完全移除，仅保留错误和用户日志", "确定");
    }

    private void ShowLogConfiguration()
    {
        string config = "";
        
        // 检查GameLogger是否存在
        var gameLoggerType = System.Type.GetType("GameLogger");
        if (gameLoggerType != null)
        {
            var method = gameLoggerType.GetMethod("GetLogConfig", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (method != null)
            {
                config = (string)method.Invoke(null, null);
            }
        }
        
        if (string.IsNullOrEmpty(config))
        {
            var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            
            bool isDevelopmentLogsEnabled = defines.Contains("ENABLE_DEVELOPMENT_LOGS");
            bool isWarningsDisabled = defines.Contains("DISABLE_WARNINGS");
            
            config = $"日志配置信息:\n";
            config += $"开发日志: {(isDevelopmentLogsEnabled ? "启用" : "禁用")}\n";
            config += $"警告日志: {(isWarningsDisabled ? "禁用" : "启用")}\n";
            config += $"编译符号: {defines}";
        }
        
        EditorUtility.DisplayDialog("当前日志配置", config, "确定");
        Debug.Log(config);
    }
}

// === 构建时自动日志配置 ===
public class BuildLogConfigProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        ConfigureLogSettings(report.summary.options);
    }

    private static void ConfigureLogSettings(BuildOptions options)
    {
        bool isDevelopmentBuild = (options & BuildOptions.Development) != 0;
        var targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

        if (isDevelopmentBuild)
        {
            // 开发版本 - 启用所有日志
            if (!defines.Contains("ENABLE_DEVELOPMENT_LOGS"))
            {
                if (!string.IsNullOrEmpty(defines))
                    defines += ";";
                defines += "ENABLE_DEVELOPMENT_LOGS";
            }
            
            // 移除DISABLE_WARNINGS
            defines = defines.Replace("DISABLE_WARNINGS;", "");
            defines = defines.Replace(";DISABLE_WARNINGS", "");
            defines = defines.Replace("DISABLE_WARNINGS", "");
        }
        else
        {
            // Release版本 - 禁用开发日志和警告
            if (!defines.Contains("DISABLE_WARNINGS"))
            {
                if (!string.IsNullOrEmpty(defines))
                    defines += ";";
                defines += "DISABLE_WARNINGS";
            }
            
            // 移除开发日志符号
            defines = defines.Replace("ENABLE_DEVELOPMENT_LOGS;", "");
            defines = defines.Replace(";ENABLE_DEVELOPMENT_LOGS", "");
            defines = defines.Replace("ENABLE_DEVELOPMENT_LOGS", "");
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
        
        Debug.Log($"构建日志配置: {(isDevelopmentBuild ? "Development" : "Release")}, 编译符号: {defines}");
    }
}
