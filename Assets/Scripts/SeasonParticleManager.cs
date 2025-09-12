using UnityEngine;

/// <summary>
/// 季节粒子系统管理器 - 管理春夏季节变换时的粒子效果
/// </summary>
public class SeasonParticleManager : MonoBehaviour
{
    [Header("粒子系统设置")]
    [SerializeField] private ParticleSystem springParticleSystem; // 春季粉色粒子系统
    [SerializeField] private ParticleSystem summerParticleSystem; // 夏季绿色粒子系统
    
    [Header("粒子效果设置")]
    [SerializeField] private float particleDuration = 3f; // 粒子效果持续时间
    [SerializeField] private bool enableParticleEffects = true; // 是否启用粒子效果
    
    [Header("调试设置")]
    [SerializeField] private bool showDebugInfo = true;
    
    [Header("粒子颜色设置")]
    [SerializeField] private Color springParticleColor = new Color(1f, 0.7f, 0.8f, 1f); // 粉色
    [SerializeField] private Color summerParticleColor = new Color(0.4f, 0.8f, 0.4f, 1f); // 绿色
    
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
        
        // 初始化粒子系统
        InitializeParticleSystems();
        
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
    
    /// <summary>
    /// 初始化粒子系统
    /// </summary>
    private void InitializeParticleSystems()
    {
        // 初始化春季粒子系统
        if (springParticleSystem != null)
        {
            ConfigureSpringParticleSystem();
        }
        else
        {
            GameLogger.LogWarning("SeasonParticleManager: 春季粒子系统未设置");
        }
        
        // 初始化夏季粒子系统
        if (summerParticleSystem != null)
        {
            ConfigureSummerParticleSystem();
        }
        else
        {
            GameLogger.LogWarning("SeasonParticleManager: 夏季粒子系统未设置");
        }
    }
    
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
        
        // 基本设置
        main.startLifetime = 2f;
        main.startSpeed = 2f;
        main.startSize = springParticleSize;
        main.startColor = springParticleColor;
        main.maxParticles = springParticleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // 发射设置
        emission.enabled = false; // 默认不发射
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, springParticleCount)
        });
        
        // 形状设置 - 圆形发射
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 1f;
        
        // 速度设置
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(1f);
        
        // 颜色渐变
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(springParticleColor, 0f),
                new GradientColorKey(springParticleColor, 0.7f),
                new GradientColorKey(new Color(springParticleColor.r, springParticleColor.g, springParticleColor.b, 0f), 1f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;
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
        
        // 基本设置
        main.startLifetime = 2.5f;
        main.startSpeed = 2.5f;
        main.startSize = summerParticleSize;
        main.startColor = summerParticleColor;
        main.maxParticles = summerParticleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // 发射设置
        emission.enabled = false; // 默认不发射
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, summerParticleCount)
        });
        
        // 形状设置 - 圆形发射
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 1.2f;
        
        // 速度设置
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(1.2f);
        
        // 颜色渐变
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(summerParticleColor, 0f),
                new GradientColorKey(summerParticleColor, 0.7f),
                new GradientColorKey(new Color(summerParticleColor.r, summerParticleColor.g, summerParticleColor.b, 0f), 1f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;
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
            if (showDebugInfo)
            {
                GameLogger.LogDev("SeasonParticleManager: 粒子效果正在播放中，跳过新的播放请求");
            }
            return;
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
        
        // 播放春季粒子效果
        springParticleSystem.Play();
        isPlayingParticles = true;
        
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
        
        // 播放夏季粒子效果
        summerParticleSystem.Play();
        isPlayingParticles = true;
        
        // 设置定时停止
        StartCoroutine(StopParticlesAfterDelay(summerParticleSystem));
    }
    
    /// <summary>
    /// 延迟停止粒子效果
    /// </summary>
    /// <param name="particleSystem">要停止的粒子系统</param>
    private System.Collections.IEnumerator StopParticlesAfterDelay(ParticleSystem particleSystem)
    {
        yield return new WaitForSeconds(particleDuration);
        
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
    /// 更新春季粒子颜色
    /// </summary>
    /// <param name="color">新颜色</param>
    public void UpdateSpringParticleColor(Color color)
    {
        springParticleColor = color;
        if (springParticleSystem != null)
        {
            ConfigureSpringParticleSystem();
        }
        if (showDebugInfo)
        {
            GameLogger.LogDev($"SeasonParticleManager: 春季粒子颜色已更新为 {color}");
        }
    }
    
    /// <summary>
    /// 更新夏季粒子颜色
    /// </summary>
    /// <param name="color">新颜色</param>
    public void UpdateSummerParticleColor(Color color)
    {
        summerParticleColor = color;
        if (summerParticleSystem != null)
        {
            ConfigureSummerParticleSystem();
        }
        if (showDebugInfo)
        {
            GameLogger.LogDev($"SeasonParticleManager: 夏季粒子颜色已更新为 {color}");
        }
    }
}
