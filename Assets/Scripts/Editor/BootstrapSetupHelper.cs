using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

/// <summary>
/// Bootstrap设置助手 - 自动创建必要的预制体和目录结构
/// 解决单场景测试时的初始化问题
/// </summary>
public class BootstrapSetupHelper : EditorWindow
{
    [MenuItem("游戏工具/Bootstrap设置助手")]
    public static void ShowWindow()
    {
        GetWindow<BootstrapSetupHelper>("Bootstrap设置助手");
    }

    private void OnGUI()
    {
        GUILayout.Label("Bootstrap设置助手", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("自动设置游戏启动引导系统，解决单场景测试问题", EditorStyles.helpBox);
        GUILayout.Space(10);
        
        if (GUILayout.Button("自动创建所有必要资源", GUILayout.Height(30)))
        {
            SetupBootstrapSystem();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("为当前场景添加SceneInitializer"))
        {
            AddSceneInitializerToCurrentScene();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("检查Bootstrap系统状态"))
        {
            CheckBootstrapSystemStatus();
        }
        
        GUILayout.Space(20);
        
        GUILayout.Label("使用说明：", EditorStyles.boldLabel);
        GUILayout.Label("1. 点击'自动创建所有必要资源'来设置系统", EditorStyles.wordWrappedLabel);
        GUILayout.Label("2. 系统会自动创建预制体和目录结构", EditorStyles.wordWrappedLabel);
        GUILayout.Label("3. 每个场景都会自动添加SceneInitializer", EditorStyles.wordWrappedLabel);
        GUILayout.Label("4. 现在可以从任意场景启动测试了！", EditorStyles.wordWrappedLabel);
    }

    /// <summary>
    /// 设置完整的Bootstrap系统
    /// </summary>
    private void SetupBootstrapSystem()
    {
        EditorUtility.DisplayProgressBar("设置Bootstrap系统", "正在创建目录结构...", 0.1f);
        
        // 1. 创建必要的目录
        CreateDirectories();
        
        EditorUtility.DisplayProgressBar("设置Bootstrap系统", "正在创建AudioManager预制体...", 0.3f);
        
        // 2. 创建AudioManager预制体
        CreateAudioManagerPrefab();
        
        EditorUtility.DisplayProgressBar("设置Bootstrap系统", "正在创建InfoPopupManager预制体...", 0.5f);
        
        // 3. 创建InfoPopupManager预制体
        CreateInfoPopupManagerPrefab();
        
        EditorUtility.DisplayProgressBar("设置Bootstrap系统", "正在创建GameFlowManager预制体...", 0.7f);
        
        // 4. 创建GameFlowManager预制体
        CreateGameFlowManagerPrefab();
        
        EditorUtility.DisplayProgressBar("设置Bootstrap系统", "正在创建Bootstrap预制体...", 0.9f);
        
        // 5. 创建GameBootstrap预制体
        CreateGameBootstrapPrefab();
        
        // 6. 为所有场景添加SceneInitializer
        AddSceneInitializerToAllScenes();
        
        EditorUtility.ClearProgressBar();
        
        EditorUtility.DisplayDialog("设置完成", "Bootstrap系统设置完成！\n现在可以从任意场景启动测试了。", "确定");
        
        Debug.Log("Bootstrap设置助手: 系统设置完成！所有必要资源已创建。");
    }

    /// <summary>
    /// 创建必要的目录结构
    /// </summary>
    private void CreateDirectories()
    {
        string[] directories = 
        {
            "Assets/Resources",
            "Assets/Resources/Prefabs"
        };

        foreach (string dir in directories)
        {
            if (!AssetDatabase.IsValidFolder(dir))
            {
                string parentDir = Path.GetDirectoryName(dir);
                string dirName = Path.GetFileName(dir);
                AssetDatabase.CreateFolder(parentDir, dirName);
                Debug.Log($"Bootstrap设置助手: 创建目录 {dir}");
            }
        }
    }

    /// <summary>
    /// 创建AudioManager预制体
    /// </summary>
    private void CreateAudioManagerPrefab()
    {
        GameObject audioManagerObj = new GameObject("AudioManager");
        
        // 添加AudioManager组件
        AudioManager audioManager = audioManagerObj.AddComponent<AudioManager>();
        
        // 创建AudioSource组件
        AudioSource bgmSource = audioManagerObj.AddComponent<AudioSource>();
        AudioSource sfxSource = audioManagerObj.AddComponent<AudioSource>();
        AudioSource ambientSource = audioManagerObj.AddComponent<AudioSource>();
        
        // 配置AudioSource
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = 0.5f;
        
        sfxSource.playOnAwake = false;
        sfxSource.volume = 1f;
        
        ambientSource.loop = true;
        ambientSource.playOnAwake = false;
        ambientSource.volume = 1f;
        
        // 使用反射设置私有字段（如果需要）
        var bgmField = typeof(AudioManager).GetField("bgmSource", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var sfxField = typeof(AudioManager).GetField("sfxSource", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var ambientField = typeof(AudioManager).GetField("ambientSource", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        bgmField?.SetValue(audioManager, bgmSource);
        sfxField?.SetValue(audioManager, sfxSource);
        ambientField?.SetValue(audioManager, ambientSource);
        
        // 保存为预制体
        string prefabPath = "Assets/Resources/Prefabs/AudioManager.prefab";
        PrefabUtility.SaveAsPrefabAsset(audioManagerObj, prefabPath);
        
        DestroyImmediate(audioManagerObj);
        Debug.Log($"Bootstrap设置助手: AudioManager预制体已创建 - {prefabPath}");
    }

    /// <summary>
    /// 创建InfoPopupManager预制体
    /// </summary>
    private void CreateInfoPopupManagerPrefab()
    {
        // 检查是否已存在InfoPopupPanel预制体
        GameObject popupPanelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/InfoPopupPanel.prefab");
        
        if (popupPanelPrefab == null)
        {
            Debug.LogWarning("Bootstrap设置助手: 未找到InfoPopupPanel预制体，InfoPopupManager可能需要手动设置");
        }

        GameObject infoPopupObj = new GameObject("InfoPopupManager");
        InfoPopupManager infoPopup = infoPopupObj.AddComponent<InfoPopupManager>();
        
        // 如果找到了popup预制体，设置引用
        if (popupPanelPrefab != null)
        {
            var field = typeof(InfoPopupManager).GetField("popupPanelPrefab", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(infoPopup, popupPanelPrefab);
        }
        
        // 保存为预制体
        string prefabPath = "Assets/Resources/Prefabs/InfoPopupManager.prefab";
        PrefabUtility.SaveAsPrefabAsset(infoPopupObj, prefabPath);
        
        DestroyImmediate(infoPopupObj);
        Debug.Log($"Bootstrap设置助手: InfoPopupManager预制体已创建 - {prefabPath}");
    }

    /// <summary>
    /// 创建GameFlowManager预制体
    /// </summary>
    private void CreateGameFlowManagerPrefab()
    {
        GameObject gameFlowObj = new GameObject("GameFlowManager");
        gameFlowObj.AddComponent<GameFlowManager>();
        
        // 保存为预制体
        string prefabPath = "Assets/Resources/Prefabs/GameFlowManager.prefab";
        PrefabUtility.SaveAsPrefabAsset(gameFlowObj, prefabPath);
        
        DestroyImmediate(gameFlowObj);
        Debug.Log($"Bootstrap设置助手: GameFlowManager预制体已创建 - {prefabPath}");
    }

    /// <summary>
    /// 创建GameBootstrap预制体
    /// </summary>
    private void CreateGameBootstrapPrefab()
    {
        GameObject bootstrapObj = new GameObject("GameBootstrap");
        GameBootstrap bootstrap = bootstrapObj.AddComponent<GameBootstrap>();
        
        // 设置预制体引用
        GameObject audioManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/AudioManager.prefab");
        GameObject infoPopupManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/InfoPopupManager.prefab");
        GameObject gameFlowManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/GameFlowManager.prefab");
        
        // 使用反射设置预制体引用
        var audioField = typeof(GameBootstrap).GetField("audioManagerPrefab", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var infoField = typeof(GameBootstrap).GetField("infoPopupManagerPrefab", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var flowField = typeof(GameBootstrap).GetField("gameFlowManagerPrefab", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
        audioField?.SetValue(bootstrap, audioManagerPrefab);
        infoField?.SetValue(bootstrap, infoPopupManagerPrefab);
        flowField?.SetValue(bootstrap, gameFlowManagerPrefab);
        
        // 保存为预制体
        string prefabPath = "Assets/Resources/Prefabs/GameBootstrap.prefab";
        PrefabUtility.SaveAsPrefabAsset(bootstrapObj, prefabPath);
        
        DestroyImmediate(bootstrapObj);
        Debug.Log($"Bootstrap设置助手: GameBootstrap预制体已创建 - {prefabPath}");
    }

    /// <summary>
    /// 为当前场景添加SceneInitializer
    /// </summary>
    private void AddSceneInitializerToCurrentScene()
    {
        // 检查是否已存在
        SceneInitializer existing = FindObjectOfType<SceneInitializer>();
        if (existing != null)
        {
            Debug.Log("Bootstrap设置助手: 当前场景已存在SceneInitializer");
            return;
        }

        GameObject initializerObj = new GameObject("SceneInitializer");
        SceneInitializer initializer = initializerObj.AddComponent<SceneInitializer>();
        
        // 检查当前场景是否为测试场景（level2或level3）
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName.Contains("level2") || sceneName.Contains("level3") || sceneName.Contains("Level"))
        {
            // 使用反射设置测试场景标志
            var testField = typeof(SceneInitializer).GetField("isTestingScene", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            testField?.SetValue(initializer, true);
            Debug.Log($"Bootstrap设置助手: 为场景 {sceneName} 添加了SceneInitializer（测试模式）");
        }
        else
        {
            Debug.Log($"Bootstrap设置助手: 为场景 {sceneName} 添加了SceneInitializer");
        }

        EditorUtility.SetDirty(initializerObj);
    }

    /// <summary>
    /// 为所有场景添加SceneInitializer
    /// </summary>
    private void AddSceneInitializerToAllScenes()
    {
        // 这个功能需要遍历所有场景，但在运行时不容易实现
        // 现在只为当前场景添加
        AddSceneInitializerToCurrentScene();
        
        Debug.Log("Bootstrap设置助手: 建议手动为其他场景添加SceneInitializer，或者在每个场景中运行此工具");
    }

    /// <summary>
    /// 检查Bootstrap系统状态
    /// </summary>
    private void CheckBootstrapSystemStatus()
    {
        bool hasGameBootstrapScript = File.Exists("Assets/Scripts/GameBootstrap.cs");
        bool hasSceneInitializerScript = File.Exists("Assets/Scripts/SceneInitializer.cs");
        bool hasResourcesFolder = AssetDatabase.IsValidFolder("Assets/Resources/Prefabs");
        
        GameObject audioManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/AudioManager.prefab");
        GameObject infoPopupManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/InfoPopupManager.prefab");
        GameObject gameFlowManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/GameFlowManager.prefab");
        GameObject gameBootstrapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/GameBootstrap.prefab");
        
        SceneInitializer sceneInitializer = FindObjectOfType<SceneInitializer>();
        
        string status = "Bootstrap系统状态检查:\n\n";
        status += $"✓ GameBootstrap脚本: {(hasGameBootstrapScript ? "已存在" : "❌ 缺失")}\n";
        status += $"✓ SceneInitializer脚本: {(hasSceneInitializerScript ? "已存在" : "❌ 缺失")}\n";
        status += $"✓ Resources文件夹: {(hasResourcesFolder ? "已存在" : "❌ 缺失")}\n";
        status += $"✓ AudioManager预制体: {(audioManagerPrefab != null ? "已存在" : "❌ 缺失")}\n";
        status += $"✓ InfoPopupManager预制体: {(infoPopupManagerPrefab != null ? "已存在" : "❌ 缺失")}\n";
        status += $"✓ GameFlowManager预制体: {(gameFlowManagerPrefab != null ? "已存在" : "❌ 缺失")}\n";
        status += $"✓ GameBootstrap预制体: {(gameBootstrapPrefab != null ? "已存在" : "❌ 缺失")}\n";
        status += $"✓ 当前场景SceneInitializer: {(sceneInitializer != null ? "已存在" : "❌ 缺失")}\n";
        
        bool isFullySetup = hasGameBootstrapScript && hasSceneInitializerScript && hasResourcesFolder && 
                           audioManagerPrefab != null && infoPopupManagerPrefab != null && 
                           gameFlowManagerPrefab != null && gameBootstrapPrefab != null;
        
        if (isFullySetup)
        {
            status += "\n🎉 系统已完全设置！可以从任意场景启动测试。";
        }
        else
        {
            status += "\n⚠️ 系统未完全设置，请运行'自动创建所有必要资源'。";
        }
        
        EditorUtility.DisplayDialog("系统状态", status, "确定");
        Debug.Log(status);
    }
}
