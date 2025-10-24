using UnityEngine;

/// <summary>
/// 季节粒子系统管理器 - 管理春夏季节变换时的粒子效果
/// </summary>
public class SeasonParticleManager : MonoBehaviour
{
    [Header("粒子系统设置")]
    [SerializeField] private ParticleSystem springParticleSystem; // 春季粉色粒子系统
    [SerializeField] private ParticleSystem summerParticleSystem; // 夏季绿色粒子系统
    
    [Header("粒子纹理设置")]
    [SerializeField] private Sprite springParticleSprite1; // 春季粒子纹理1
    [SerializeField] private Sprite springParticleSprite2; // 春季粒子纹理2
    [SerializeField] private Sprite summerParticleSprite1; // 夏季粒子纹理1
    [SerializeField] private Sprite summerParticleSprite2; // 夏季粒子纹理2
    
    [Header("粒子效果设置")]
    [SerializeField] private float particleDuration = 3f; // 粒子效果持续时间
    [SerializeField] private bool enableParticleEffects = true; // 是否启用粒子效果
    
    [Header("调试设置")]
    [SerializeField] private bool showDebugInfo = true;
    
    // 粒子颜色固定为白色，不需要设置
    
    [Header("粒子大小设置")]
    [SerializeField] private float springParticleSize = 0.5f;
    [SerializeField] private float summerParticleSize = 0.6f;
    
    [Header("粒子数量设置")]
    [SerializeField] private int springParticleCount = 50;
    [SerializeField] private int summerParticleCount = 60;
    
    private Level3Manager level3Manager;
    private bool isPlayingParticles = false;
    
    private void Start()
    {
        // 获取Level3Manager引用
        level3Manager = FindObjectOfType<Level3Manager>();
        if (level3Manager == null)
        {
            GameLogger.LogWarning("SeasonParticleManager: 未找到Level3Manager，无法监听季节切换事件");
            return;
        }
        
        // 订阅季节切换事件
        level3Manager.OnSeasonChanged += OnSeasonChanged;
        
        // 不重设已存在的粒子系统参数
        
        if (showDebugInfo)
        {
            GameLogger.LogDev("SeasonParticleManager: 初始化完成，已订阅季节切换事件");
        }
    }
    
    private void OnDestroy()
    {
        // 取消订阅事件
        if (level3Manager != null)
        {
            level3Manager.OnSeasonChanged -= OnSeasonChanged;
        }
    }
    
    // 移除参数初始化，避免改动已有粒子系统设定
    
    /// <summary>
    /// 配置春季粒子系统
    /// </summary>
    private void ConfigureSpringParticleSystem()
    {
        var main = springParticleSystem.main;
        var emission = springParticleSystem.emission;
        var shape = springParticleSystem.shape;
        var velocityOverLifetime = springParticleSystem.velocityOverLifetime;
        var colorOverLifetime = springParticleSystem.colorOverLifetime;
        var textureSheetAnimation = springParticleSystem.textureSheetAnimation;
        
        // 基本设置
        main.startLifetime = 2f;
        main.startSpeed = 2f;
        main.startSize = springParticleSize;
        main.startColor = Color.white; // 固定白色
        main.maxParticles = springParticleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // 发射设置
        emission.enabled = true; // 启用发射
        emission.rateOverTime = 0f; // 不使用持续发射
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, springParticleCount)
        });
        
        // 确保发射器设置正确
        emission.burstCount = 1;
        
        // 形状设置 - 圆形发射
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 1f;
        
        // 速度设置
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(1f);
        
        // 颜色设置 - 白色粒子，透明度不变
        colorOverLifetime.enabled = false; // 禁用颜色生命周期变化
        
        // 纹理设置 - 根据Unity官方教程实现随机挑选精灵
        if (springParticleSprite1 != null || springParticleSprite2 != null)
        {
            textureSheetAnimation.enabled = true;
            textureSheetAnimation.mode = ParticleSystemAnimationMode.Sprites;
            
            // 添加所有可用的纹理
            if (springParticleSprite1 != null)
                textureSheetAnimation.AddSprite(springParticleSprite1);
            if (springParticleSprite2 != null)
                textureSheetAnimation.AddSprite(springParticleSprite2);
            
            // 设置sprite数量
            textureSheetAnimation.numTilesX = 1;
            textureSheetAnimation.numTilesY = 1;
            
            // 设置Start Frame为Random Between Two Constants (0-2)
            // 系统将选择一个介于0到2（不包括2）之间的随机数，即0或1
            textureSheetAnimation.startFrame = new ParticleSystem.MinMaxCurve(0f, 2f);
            
            // 删除Frame over time动画 - 不想要任何动画
            textureSheetAnimation.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
            
            // 确保使用随机行
            textureSheetAnimation.rowMode = ParticleSystemAnimationRowMode.Random;
            
            // 设置渲染模式为Stretched Billboard以正确显示Sprite
            main.startRotation3D = true;
            var renderer = springParticleSystem.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                renderer.lengthScale = 1f;
                renderer.velocityScale = 0.1f;
                renderer.material = GetParticleMaterial();
            }
        }
        else
        {
            textureSheetAnimation.enabled = false;
        }
    }
    
    /// <summary>
    /// 配置夏季粒子系统
    /// </summary>
    private void ConfigureSummerParticleSystem()
    {
        var main = summerParticleSystem.main;
        var emission = summerParticleSystem.emission;
        var shape = summerParticleSystem.shape;
        var velocityOverLifetime = summerParticleSystem.velocityOverLifetime;
        var colorOverLifetime = summerParticleSystem.colorOverLifetime;
        var textureSheetAnimation = summerParticleSystem.textureSheetAnimation;
        
        // 基本设置
        main.startLifetime = 2.5f;
        main.startSpeed = 2.5f;
        main.startSize = summerParticleSize;
        main.startColor = Color.white; // 固定白色
        main.maxParticles = summerParticleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // 发射设置
        emission.enabled = true; // 启用发射
        emission.rateOverTime = 0f; // 不使用持续发射
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, summerParticleCount)
        });
        
        // 确保发射器设置正确
        emission.burstCount = 1;
        
        // 形状设置 - 圆形发射
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 1.2f;
        
        // 速度设置
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(1.2f);
        
        // 颜色设置 - 白色粒子，透明度不变
        colorOverLifetime.enabled = false; // 禁用颜色生命周期变化
        
        // 纹理设置 - 根据Unity官方教程实现随机挑选精灵
        if (summerParticleSprite1 != null || summerParticleSprite2 != null)
        {
            textureSheetAnimation.enabled = true;
            textureSheetAnimation.mode = ParticleSystemAnimationMode.Sprites;
            
            // 添加所有可用的纹理
            if (summerParticleSprite1 != null)
                textureSheetAnimation.AddSprite(summerParticleSprite1);
            if (summerParticleSprite2 != null)
                textureSheetAnimation.AddSprite(summerParticleSprite2);
            
            // 设置sprite数量
            textureSheetAnimation.numTilesX = 1;
            textureSheetAnimation.numTilesY = 1;
            
            // 设置Start Frame为Random Between Two Constants (0-2)
            // 系统将选择一个介于0到2（不包括2）之间的随机数，即0或1
            textureSheetAnimation.startFrame = new ParticleSystem.MinMaxCurve(0f, 2f);
            
            // 删除Frame over time动画 - 不想要任何动画
            textureSheetAnimation.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
            
            // 确保使用随机行
            textureSheetAnimation.rowMode = ParticleSystemAnimationRowMode.Random;
            
            // 设置渲染模式为Stretched Billboard以正确显示Sprite
            main.startRotation3D = true;
            var renderer = summerParticleSystem.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                renderer.lengthScale = 1f;
                renderer.velocityScale = 0.1f;
                renderer.material = GetParticleMaterial();
            }
        }
        else
        {
            textureSheetAnimation.enabled = false;
        }
    }
    
    /// <summary>
    /// 季节切换事件处理
    /// </summary>
    /// <param name="newSeason">新季节</param>
    private void OnSeasonChanged(SeasonType newSeason)
    {
        if (!enableParticleEffects)
        {
            if (showDebugInfo)
            {
                GameLogger.LogDev("SeasonParticleManager: 粒子效果已禁用，跳过播放");
            }
            return;
        }
        
        if (isPlayingParticles)
        {
            // 若仍在播放，先停止当前效果，再按新季节播放
            if (showDebugInfo)
            {
                GameLogger.LogDev("SeasonParticleManager: 前一效果未结束，先停止再切换播放");
            }
            StopAllParticles();
            isPlayingParticles = false;
        }
        
        if (showDebugInfo)
        {
            GameLogger.LogDev($"SeasonParticleManager: 季节切换到 {newSeason}，开始播放粒子效果");
        }
        
        // 根据季节播放对应的粒子效果
        switch (newSeason)
        {
            case SeasonType.Spring:
                PlaySpringParticles();
                break;
            case SeasonType.Summer:
                PlaySummerParticles();
                break;
        }
    }
    
    /// <summary>
    /// 播放春季粒子效果
    /// </summary>
    private void PlaySpringParticles()
    {
        if (springParticleSystem == null)
        {
            GameLogger.LogWarning("SeasonParticleManager: 春季粒子系统未设置，无法播放效果");
            return;
        }
        
        if (showDebugInfo)
        {
            GameLogger.LogDev("SeasonParticleManager: 播放春季粉色粒子效果");
        }
        
        // 停止其他粒子系统
        if (summerParticleSystem != null && summerParticleSystem.isPlaying)
        {
            summerParticleSystem.Stop();
        }

        // 直接播放，不修改任何已有设定
        springParticleSystem.Play();
        isPlayingParticles = true;
        
        // 等待一帧后检查粒子数量
        StartCoroutine(CheckParticleCountAfterFrame(springParticleSystem, "春季"));
        
        // 设置定时停止
        StartCoroutine(StopParticlesAfterDelay(springParticleSystem));
    }
    
    /// <summary>
    /// 播放夏季粒子效果
    /// </summary>
    private void PlaySummerParticles()
    {
        if (summerParticleSystem == null)
        {
            GameLogger.LogWarning("SeasonParticleManager: 夏季粒子系统未设置，无法播放效果");
            return;
        }
        
        if (showDebugInfo)
        {
            GameLogger.LogDev("SeasonParticleManager: 播放夏季绿色粒子效果");
        }
        
        // 停止其他粒子系统
        if (springParticleSystem != null && springParticleSystem.isPlaying)
        {
            springParticleSystem.Stop();
        }

        // 确保夏季粒子系统具备一次性爆发的配置与可见渲染
        EnsureSummerConfigured();

        // 清空旧粒子，避免历史残留影响观感
        summerParticleSystem.Clear();

        // 播放
        summerParticleSystem.Play();
        isPlayingParticles = true;
        
        // 等待一帧后检查粒子数量
        StartCoroutine(CheckParticleCountAfterFrame(summerParticleSystem, "夏季"));
        
        // 设置定时停止
        StartCoroutine(StopParticlesAfterDelay(summerParticleSystem));
    }
    
    /// <summary>
    /// 延迟停止粒子效果
    /// </summary>
    /// <param name="particleSystem">要停止的粒子系统</param>
    private System.Collections.IEnumerator StopParticlesAfterDelay(ParticleSystem particleSystem)
    {
        // 使用真实时间，避免 Time.timeScale==0 时无法计时导致永不停止
        yield return new WaitForSecondsRealtime(particleDuration);
        
        if (particleSystem != null && particleSystem.isPlaying)
        {
            particleSystem.Stop();
            if (showDebugInfo)
            {
                GameLogger.LogDev("SeasonParticleManager: 粒子效果播放完成，已停止");
            }
        }
        
        isPlayingParticles = false;
    }
    
    /// <summary>
    /// 等待一帧后检查粒子数量
    /// </summary>
    /// <param name="particleSystem">粒子系统</param>
    /// <param name="seasonName">季节名称</param>
    private System.Collections.IEnumerator CheckParticleCountAfterFrame(ParticleSystem particleSystem, string seasonName)
    {
        yield return new WaitForEndOfFrame();
        
        if (particleSystem != null)
        {
            int particleCount = particleSystem.particleCount;
            if (showDebugInfo)
            {
                GameLogger.LogDev($"SeasonParticleManager: {seasonName}粒子系统播放后粒子数量: {particleCount}");
            }
            
            if (particleCount == 0)
            {
                GameLogger.LogWarning($"SeasonParticleManager: {seasonName}粒子系统未产生粒子（保留原设定，未做自动修复）");
            }
        }
    }
    
    /// <summary>
    /// 延迟停止持续发射
    /// </summary>
    /// <param name="particleSystem">粒子系统</param>
    /// <param name="delay">延迟时间</param>
    private System.Collections.IEnumerator StopContinuousEmissionAfterDelay(ParticleSystem particleSystem, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        
        if (particleSystem != null)
        {
            var emission = particleSystem.emission;
            emission.rateOverTime = 0f; // 停止持续发射
            particleSystem.Stop();
            
            if (showDebugInfo)
            {
                GameLogger.LogDev("SeasonParticleManager: 持续发射测试已停止");
            }
        }
    }
    
    /// <summary>
    /// 手动播放春季粒子效果（用于测试）
    /// </summary>
    [ContextMenu("测试播放春季粒子")]
    public void TestPlaySpringParticles()
    {
        if (enableParticleEffects)
        {
            PlaySpringParticles();
        }
        else
        {
            GameLogger.LogDev("SeasonParticleManager: 粒子效果已禁用，无法测试");
        }
    }
    
    /// <summary>
    /// 测试sprite选择
    /// </summary>
    [ContextMenu("测试sprite选择")]
    public void TestSpriteSelection()
    {
        GameLogger.LogDev("=== 测试sprite选择 ===");
        
        // 检查春季粒子系统
        if (springParticleSystem != null)
        {
            var textureSheetAnimation = springParticleSystem.textureSheetAnimation;
            GameLogger.LogDev($"春季粒子系统sprite设置:");
            GameLogger.LogDev($"  - 启用状态: {textureSheetAnimation.enabled}");
            GameLogger.LogDev($"  - 模式: {textureSheetAnimation.mode}");
            GameLogger.LogDev($"  - Sprite数量: {textureSheetAnimation.spriteCount}");
            GameLogger.LogDev($"  - Start Frame范围: {textureSheetAnimation.startFrame.constantMin} - {textureSheetAnimation.startFrame.constantMax}");
            GameLogger.LogDev($"  - 行模式: {textureSheetAnimation.rowMode}");
            GameLogger.LogDev($"  - Sprite1: {(springParticleSprite1 != null ? springParticleSprite1.name : "null")}");
            GameLogger.LogDev($"  - Sprite2: {(springParticleSprite2 != null ? springParticleSprite2.name : "null")}");
        }
        
        // 检查夏季粒子系统
        if (summerParticleSystem != null)
        {
            var textureSheetAnimation = summerParticleSystem.textureSheetAnimation;
            GameLogger.LogDev($"夏季粒子系统sprite设置:");
            GameLogger.LogDev($"  - 启用状态: {textureSheetAnimation.enabled}");
            GameLogger.LogDev($"  - 模式: {textureSheetAnimation.mode}");
            GameLogger.LogDev($"  - Sprite数量: {textureSheetAnimation.spriteCount}");
            GameLogger.LogDev($"  - Start Frame范围: {textureSheetAnimation.startFrame.constantMin} - {textureSheetAnimation.startFrame.constantMax}");
            GameLogger.LogDev($"  - 行模式: {textureSheetAnimation.rowMode}");
            GameLogger.LogDev($"  - Sprite1: {(summerParticleSprite1 != null ? summerParticleSprite1.name : "null")}");
            GameLogger.LogDev($"  - Sprite2: {(summerParticleSprite2 != null ? summerParticleSprite2.name : "null")}");
        }
        
        GameLogger.LogDev("=== sprite选择测试完成 ===");
    }
    
    /// <summary>
    /// 调试粒子系统状态
    /// </summary>
    [ContextMenu("调试粒子系统状态")]
    public void DebugParticleSystemStatus()
    {
        GameLogger.LogDev("=== 粒子系统调试信息 ===");
        
        // 检查春季粒子系统
        if (springParticleSystem != null)
        {
            var main = springParticleSystem.main;
            var emission = springParticleSystem.emission;
            var renderer = springParticleSystem.GetComponent<ParticleSystemRenderer>();
            GameLogger.LogDev($"春季粒子系统状态:");
            GameLogger.LogDev($"  - 是否播放中: {springParticleSystem.isPlaying}");
            GameLogger.LogDev($"  - 是否暂停: {springParticleSystem.isPaused}");
            GameLogger.LogDev($"  - 是否停止: {springParticleSystem.isStopped}");
            GameLogger.LogDev($"  - 最大粒子数: {main.maxParticles}");
            GameLogger.LogDev($"  - 发射启用: {emission.enabled}");
            GameLogger.LogDev($"  - 粒子数量: {springParticleSystem.particleCount}");
            GameLogger.LogDev($"  - 位置: {springParticleSystem.transform.position}");
            GameLogger.LogDev($"  - 渲染器启用: {renderer.enabled}");
            GameLogger.LogDev($"  - 渲染器材质: {(renderer.material != null ? renderer.material.name : "null")}");
            GameLogger.LogDev($"  - 渲染器排序层级: {renderer.sortingOrder}");
            GameLogger.LogDev($"  - 渲染器排序层: {renderer.sortingLayerName}");
        }
        else
        {
            GameLogger.LogWarning("春季粒子系统未设置！");
        }
        
        // 检查夏季粒子系统
        if (summerParticleSystem != null)
        {
            var main = summerParticleSystem.main;
            var emission = summerParticleSystem.emission;
            var renderer = summerParticleSystem.GetComponent<ParticleSystemRenderer>();
            GameLogger.LogDev($"夏季粒子系统状态:");
            GameLogger.LogDev($"  - 是否播放中: {summerParticleSystem.isPlaying}");
            GameLogger.LogDev($"  - 是否暂停: {summerParticleSystem.isPaused}");
            GameLogger.LogDev($"  - 是否停止: {summerParticleSystem.isStopped}");
            GameLogger.LogDev($"  - 最大粒子数: {main.maxParticles}");
            GameLogger.LogDev($"  - 发射启用: {emission.enabled}");
            GameLogger.LogDev($"  - 粒子数量: {summerParticleSystem.particleCount}");
            GameLogger.LogDev($"  - 位置: {summerParticleSystem.transform.position}");
            GameLogger.LogDev($"  - 渲染器启用: {renderer.enabled}");
            GameLogger.LogDev($"  - 渲染器材质: {(renderer.material != null ? renderer.material.name : "null")}");
            GameLogger.LogDev($"  - 渲染器排序层级: {renderer.sortingOrder}");
            GameLogger.LogDev($"  - 渲染器排序层: {renderer.sortingLayerName}");
        }
        else
        {
            GameLogger.LogWarning("夏季粒子系统未设置！");
        }
        
        // 检查摄像机信息
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            GameLogger.LogDev($"主摄像机信息:");
            GameLogger.LogDev($"  - 位置: {mainCamera.transform.position}");
            GameLogger.LogDev($"  - 旋转: {mainCamera.transform.rotation.eulerAngles}");
            GameLogger.LogDev($"  - 视野大小: {mainCamera.orthographicSize}");
            GameLogger.LogDev($"  - 是否正交: {mainCamera.orthographic}");
        }
        else
        {
            GameLogger.LogWarning("未找到主摄像机！");
        }
        
        GameLogger.LogDev($"粒子效果启用状态: {enableParticleEffects}");
        GameLogger.LogDev($"当前播放状态: {isPlayingParticles}");
    }
    
    /// <summary>
    /// 手动播放夏季粒子效果（用于测试）
    /// </summary>
    [ContextMenu("测试播放夏季粒子")]
    public void TestPlaySummerParticles()
    {
        if (enableParticleEffects)
        {
            PlaySummerParticles();
        }
        else
        {
            GameLogger.LogDev("SeasonParticleManager: 粒子效果已禁用，无法测试");
        }
    }
    
    /// <summary>
    /// 停止所有粒子效果
    /// </summary>
    [ContextMenu("停止所有粒子效果")]
    public void StopAllParticles()
    {
        if (springParticleSystem != null && springParticleSystem.isPlaying)
        {
            springParticleSystem.Stop();
        }
        
        if (summerParticleSystem != null && summerParticleSystem.isPlaying)
        {
            summerParticleSystem.Stop();
        }
        
        isPlayingParticles = false;
        
        if (showDebugInfo)
        {
            GameLogger.LogDev("SeasonParticleManager: 已停止所有粒子效果");
        }
    }
    
    /// <summary>
    /// 强制播放春季粒子（用于调试）
    /// </summary>
    [ContextMenu("强制播放春季粒子")]
    public void ForcePlaySpringParticles()
    {
        if (springParticleSystem == null)
        {
            GameLogger.LogWarning("春季粒子系统未设置！");
            return;
        }
        
        // 强制启用播放，但不重设任何参数
        enableParticleEffects = true;
        springParticleSystem.Play();
        
        GameLogger.LogDev("强制播放春季粒子效果");
    }
    
    /// <summary>
    /// 使用持续发射测试春季粒子
    /// </summary>
    [ContextMenu("持续发射测试春季粒子")]
    public void TestSpringParticlesWithContinuousEmission()
    {
        if (springParticleSystem == null)
        {
            GameLogger.LogWarning("春季粒子系统未设置！");
            return;
        }
        
        var main = springParticleSystem.main;
        var emission = springParticleSystem.emission;
        
        // 设置持续发射
        emission.enabled = true;
        emission.rateOverTime = 20f; // 每秒发射20个粒子
        emission.SetBursts(new ParticleSystem.Burst[0]); // 清除Burst设置
        
        // 基本设置
        main.startLifetime = 2f;
        main.startSpeed = 2f;
        main.startSize = springParticleSize;
        main.startColor = Color.white; // 固定白色
        main.maxParticles = springParticleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // 清除并播放
        springParticleSystem.Clear();
        springParticleSystem.Play();
        
        GameLogger.LogDev("使用持续发射测试春季粒子效果");
        
        // 3秒后停止
        StartCoroutine(StopContinuousEmissionAfterDelay(springParticleSystem, 3f));
    }
    
    /// <summary>
    /// 修复粒子渲染问题
    /// </summary>
    [ContextMenu("修复粒子渲染问题")]
    public void FixParticleRenderingIssues()
    {
        GameLogger.LogDev("=== 开始修复粒子渲染问题 ===");
        
        // 修复春季粒子系统
        if (springParticleSystem != null)
        {
            FixParticleSystemRendering(springParticleSystem, "春季");
        }
        
        // 修复夏季粒子系统
        if (summerParticleSystem != null)
        {
            FixParticleSystemRendering(summerParticleSystem, "夏季");
        }
        
        GameLogger.LogDev("=== 粒子渲染问题修复完成 ===");
    }
    
    /// <summary>
    /// 修复单个粒子系统的渲染问题
    /// </summary>
    /// <param name="particleSystem">粒子系统</param>
    /// <param name="name">系统名称</param>
    private void FixParticleSystemRendering(ParticleSystem particleSystem, string name)
    {
        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        if (renderer == null)
        {
            GameLogger.LogWarning($"{name}粒子系统缺少渲染器组件！");
            return;
        }
        
        // 确保渲染器启用
        renderer.enabled = true;
        
        // 设置合适的排序层级
        renderer.sortingOrder = 10; // 确保在其他元素之上
        
        // 检查并设置材质
        if (renderer.material == null)
        {
            // 使用默认粒子材质
            Material defaultMaterial = new Material(Shader.Find("Sprites/Default"));
            renderer.material = defaultMaterial;
            GameLogger.LogDev($"{name}粒子系统已设置默认材质");
        }
        
        // 设置渲染模式
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        
        // 确保粒子系统在摄像机前方
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 cameraPos = mainCamera.transform.position;
            particleSystem.transform.position = new Vector3(cameraPos.x, cameraPos.y, cameraPos.z - 1f);
            GameLogger.LogDev($"{name}粒子系统位置已调整到摄像机前方: {particleSystem.transform.position}");
        }
        
        // 确保粒子系统GameObject激活
        particleSystem.gameObject.SetActive(true);
        
        GameLogger.LogDev($"{name}粒子系统渲染问题修复完成");
    }
    
    /// <summary>
    /// 创建简单的可见粒子测试
    /// </summary>
    [ContextMenu("创建简单粒子测试")]
    public void CreateSimpleParticleTest()
    {
        GameLogger.LogDev("=== 创建简单粒子测试 ===");
        
        // 创建测试粒子系统
        GameObject testParticleObj = new GameObject("TestParticleSystem");
        testParticleObj.transform.position = Vector3.zero;
        
        ParticleSystem testParticle = testParticleObj.AddComponent<ParticleSystem>();
        var renderer = testParticleObj.GetComponent<ParticleSystemRenderer>();
        
        // 基本设置
        var main = testParticle.main;
        main.startLifetime = 3f;
        main.startSpeed = 2f;
        main.startSize = 1f;
        main.startColor = Color.red;
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        
        // 发射设置
        var emission = testParticle.emission;
        emission.enabled = true;
        emission.rateOverTime = 50f;
        
        // 形状设置
        var shape = testParticle.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 1f;
        
        // 渲染器设置
        renderer.enabled = true;
        renderer.sortingOrder = 100;
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        
        // 确保在摄像机前方
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 cameraPos = mainCamera.transform.position;
            testParticleObj.transform.position = new Vector3(cameraPos.x, cameraPos.y, cameraPos.z - 1f);
        }
        
        GameLogger.LogDev("简单粒子测试已创建，应该能看到红色粒子");
        
        // 5秒后销毁测试对象
        StartCoroutine(DestroyTestParticleAfterDelay(testParticleObj, 5f));
    }
    
    /// <summary>
    /// 延迟销毁测试粒子
    /// </summary>
    /// <param name="testObj">测试对象</param>
    /// <param name="delay">延迟时间</param>
    private System.Collections.IEnumerator DestroyTestParticleAfterDelay(GameObject testObj, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        
        if (testObj != null)
        {
            DestroyImmediate(testObj);
            GameLogger.LogDev("测试粒子已销毁");
        }
    }
    
    /// <summary>
    /// 强制播放夏季粒子（用于调试）
    /// </summary>
    [ContextMenu("强制播放夏季粒子")]
    public void ForcePlaySummerParticles()
    {
        if (summerParticleSystem == null)
        {
            GameLogger.LogWarning("夏季粒子系统未设置！");
            return;
        }
        
        // 强制启用播放，但不重设任何参数
        enableParticleEffects = true;

        // 强化配置，确保能看见粒子
        EnsureSummerConfigured();
        summerParticleSystem.Clear();
        summerParticleSystem.Play();
        
        GameLogger.LogDev("强制播放夏季粒子效果");
    }

    /// <summary>
    /// 确保夏季粒子系统具备基础的可见配置（一次性爆发、材质、渲染器启用）
    /// </summary>
    private void EnsureSummerConfigured()
    {
        try
        {
            var renderer = summerParticleSystem.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.enabled = true;
                if (renderer.material == null)
                {
                    renderer.material = GetParticleMaterial();
                }
            }
            // 确保对象激活
            if (!summerParticleSystem.gameObject.activeInHierarchy)
            {
                summerParticleSystem.gameObject.SetActive(true);
            }
        }
        catch { }
    }
    
    /// <summary>
    /// 设置粒子效果启用状态
    /// </summary>
    /// <param name="enabled">是否启用</param>
    public void SetParticleEffectsEnabled(bool enabled)
    {
        enableParticleEffects = enabled;
        if (showDebugInfo)
        {
            GameLogger.LogDev($"SeasonParticleManager: 粒子效果已{(enabled ? "启用" : "禁用")}");
        }
    }
    
    /// <summary>
    /// 获取粒子效果启用状态
    /// </summary>
    /// <returns>是否启用粒子效果</returns>
    public bool IsParticleEffectsEnabled()
    {
        return enableParticleEffects;
    }
    
    /// <summary>
    /// 设置粒子效果持续时间
    /// </summary>
    /// <param name="duration">持续时间（秒）</param>
    public void SetParticleDuration(float duration)
    {
        particleDuration = Mathf.Max(0.1f, duration);
        if (showDebugInfo)
        {
            GameLogger.LogDev($"SeasonParticleManager: 粒子效果持续时间设置为 {particleDuration} 秒");
        }
    }
    
    
    
    /// <summary>
    /// 设置春季粒子纹理
    /// </summary>
    /// <param name="sprite1">粒子纹理1</param>
    /// <param name="sprite2">粒子纹理2</param>
    public void SetSpringParticleSprites(Sprite sprite1, Sprite sprite2 = null)
    {
        springParticleSprite1 = sprite1;
        springParticleSprite2 = sprite2;
        if (springParticleSystem != null)
        {
            ConfigureSpringParticleSystem();
        }
        if (showDebugInfo)
        {
            GameLogger.LogDev($"SeasonParticleManager: 春季粒子纹理已设置为 {(sprite1 != null ? sprite1.name : "null")} 和 {(sprite2 != null ? sprite2.name : "null")}");
        }
    }
    
    /// <summary>
    /// 设置夏季粒子纹理
    /// </summary>
    /// <param name="sprite1">粒子纹理1</param>
    /// <param name="sprite2">粒子纹理2</param>
    public void SetSummerParticleSprites(Sprite sprite1, Sprite sprite2 = null)
    {
        summerParticleSprite1 = sprite1;
        summerParticleSprite2 = sprite2;
        if (summerParticleSystem != null)
        {
            ConfigureSummerParticleSystem();
        }
        if (showDebugInfo)
        {
            GameLogger.LogDev($"SeasonParticleManager: 夏季粒子纹理已设置为 {(sprite1 != null ? sprite1.name : "null")} 和 {(sprite2 != null ? sprite2.name : "null")}");
        }
    }
    
    /// <summary>
    /// 获取春季粒子纹理
    /// </summary>
    /// <param name="index">纹理索引 (1 或 2)</param>
    /// <returns>春季粒子纹理</returns>
    public Sprite GetSpringParticleSprite(int index = 1)
    {
        return index == 1 ? springParticleSprite1 : springParticleSprite2;
    }
    
    /// <summary>
    /// 获取夏季粒子纹理
    /// </summary>
    /// <param name="index">纹理索引 (1 或 2)</param>
    /// <returns>夏季粒子纹理</returns>
    public Sprite GetSummerParticleSprite(int index = 1)
    {
        return index == 1 ? summerParticleSprite1 : summerParticleSprite2;
    }
    
    /// <summary>
    /// 获取所有春季粒子纹理
    /// </summary>
    /// <returns>春季粒子纹理数组</returns>
    public Sprite[] GetSpringParticleSprites()
    {
        return new Sprite[] { springParticleSprite1, springParticleSprite2 };
    }
    
    /// <summary>
    /// 获取所有夏季粒子纹理
    /// </summary>
    /// <returns>夏季粒子纹理数组</returns>
    public Sprite[] GetSummerParticleSprites()
    {
        return new Sprite[] { summerParticleSprite1, summerParticleSprite2 };
    }
    
    /// <summary>
    /// 获取粒子材质
    /// </summary>
    /// <returns>粒子材质</returns>
    private Material GetParticleMaterial()
    {
        // 尝试获取默认的粒子材质
        Material defaultMaterial = Resources.Load<Material>("Default-Particle");
        if (defaultMaterial != null)
        {
            return defaultMaterial;
        }
        
        // 如果没有找到默认材质，创建一个新的
        Material particleMaterial = new Material(Shader.Find("Sprites/Default"));
        particleMaterial.name = "ParticleMaterial";
        
        if (showDebugInfo)
        {
            GameLogger.LogDev("SeasonParticleManager: 创建了新的粒子材质");
        }
        
        return particleMaterial;
    }
    
    /// <summary>
    /// 设置粒子材质
    /// </summary>
    /// <param name="material">材质</param>
    public void SetParticleMaterial(Material material)
    {
        if (springParticleSystem != null)
        {
            var renderer = springParticleSystem.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.material = material;
            }
        }
        
        if (summerParticleSystem != null)
        {
            var renderer = summerParticleSystem.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.material = material;
            }
        }
        
        if (showDebugInfo)
        {
            GameLogger.LogDev($"SeasonParticleManager: 粒子材质已设置为 {(material != null ? material.name : "null")}");
        }
    }
}
