using UnityEngine;

/// <summary>
/// 季节粒子系统预制体创建器 - 用于在编辑器中创建粒子系统预制体
/// </summary>
public class SeasonParticlePrefab : MonoBehaviour
{
    [Header("预制体设置")]
    [SerializeField] private bool createSpringParticleSystem = true;
    [SerializeField] private bool createSummerParticleSystem = true;
    
    [Header("粒子系统父对象")]
    [SerializeField] private Transform particleParent;
    
    private void Start()
    {
        // 这个脚本主要用于在编辑器中创建预制体，运行时不需要执行
    }
    
    /// <summary>
    /// 创建春季粒子系统
    /// </summary>
    [ContextMenu("创建春季粒子系统")]
    public void CreateSpringParticleSystem()
    {
        GameObject springParticleObj = new GameObject("SpringParticleSystem");
        
        if (particleParent != null)
        {
            springParticleObj.transform.SetParent(particleParent);
        }
        
        springParticleObj.transform.localPosition = Vector3.zero;
        
        ParticleSystem springParticle = springParticleObj.AddComponent<ParticleSystem>();
        
        // 配置春季粒子系统
        ConfigureSpringParticleSystem(springParticle);
        
        Debug.Log("春季粒子系统已创建: " + springParticleObj.name);
    }
    
    /// <summary>
    /// 创建夏季粒子系统
    /// </summary>
    [ContextMenu("创建夏季粒子系统")]
    public void CreateSummerParticleSystem()
    {
        GameObject summerParticleObj = new GameObject("SummerParticleSystem");
        
        if (particleParent != null)
        {
            summerParticleObj.transform.SetParent(particleParent);
        }
        
        summerParticleObj.transform.localPosition = Vector3.zero;
        
        ParticleSystem summerParticle = summerParticleObj.AddComponent<ParticleSystem>();
        
        // 配置夏季粒子系统
        ConfigureSummerParticleSystem(summerParticle);
        
        Debug.Log("夏季粒子系统已创建: " + summerParticleObj.name);
    }
    
    /// <summary>
    /// 创建完整的季节粒子系统
    /// </summary>
    [ContextMenu("创建完整季节粒子系统")]
    public void CreateCompleteSeasonParticleSystem()
    {
        // 创建父对象
        GameObject seasonParticleParent = new GameObject("SeasonParticleSystems");
        particleParent = seasonParticleParent.transform;
        
        // 创建春季粒子系统
        if (createSpringParticleSystem)
        {
            CreateSpringParticleSystem();
        }
        
        // 创建夏季粒子系统
        if (createSummerParticleSystem)
        {
            CreateSummerParticleSystem();
        }
        
        // 添加SeasonParticleManager组件
        SeasonParticleManager manager = seasonParticleParent.AddComponent<SeasonParticleManager>();
        
        // 自动分配粒子系统引用
        ParticleSystem[] particleSystems = seasonParticleParent.GetComponentsInChildren<ParticleSystem>();
        if (particleSystems.Length >= 2)
        {
            // 使用反射设置私有字段（仅用于编辑器）
            var springField = typeof(SeasonParticleManager).GetField("springParticleSystem", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var summerField = typeof(SeasonParticleManager).GetField("summerParticleSystem", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (springField != null && summerField != null)
            {
                springField.SetValue(manager, particleSystems[0]);
                summerField.SetValue(manager, particleSystems[1]);
            }
        }
        
        Debug.Log("完整季节粒子系统已创建: " + seasonParticleParent.name);
    }
    
    /// <summary>
    /// 配置春季粒子系统
    /// </summary>
    /// <param name="particleSystem">粒子系统</param>
    private void ConfigureSpringParticleSystem(ParticleSystem particleSystem)
    {
        var main = particleSystem.main;
        var emission = particleSystem.emission;
        var shape = particleSystem.shape;
        var velocityOverLifetime = particleSystem.velocityOverLifetime;
        var colorOverLifetime = particleSystem.colorOverLifetime;
        var sizeOverLifetime = particleSystem.sizeOverLifetime;
        
        // 基本设置
        main.startLifetime = 2f;
        main.startSpeed = 2f;
        main.startSize = 0.5f;
        main.startColor = new Color(1f, 0.7f, 0.8f, 1f); // 粉色
        main.maxParticles = 50;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        
        // 发射设置
        emission.enabled = false;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 50)
        });
        
        // 形状设置
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
                new GradientColorKey(new Color(1f, 0.7f, 0.8f, 1f), 0f),
                new GradientColorKey(new Color(1f, 0.7f, 0.8f, 1f), 0.7f),
                new GradientColorKey(new Color(1f, 0.7f, 0.8f, 0f), 1f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;
        
        // 大小渐变
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.5f, 1.2f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
    }
    
    /// <summary>
    /// 配置夏季粒子系统
    /// </summary>
    /// <param name="particleSystem">粒子系统</param>
    private void ConfigureSummerParticleSystem(ParticleSystem particleSystem)
    {
        var main = particleSystem.main;
        var emission = particleSystem.emission;
        var shape = particleSystem.shape;
        var velocityOverLifetime = particleSystem.velocityOverLifetime;
        var colorOverLifetime = particleSystem.colorOverLifetime;
        var sizeOverLifetime = particleSystem.sizeOverLifetime;
        
        // 基本设置
        main.startLifetime = 2.5f;
        main.startSpeed = 2.5f;
        main.startSize = 0.6f;
        main.startColor = new Color(0.4f, 0.8f, 0.4f, 1f); // 绿色
        main.maxParticles = 60;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        
        // 发射设置
        emission.enabled = false;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 60)
        });
        
        // 形状设置
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
                new GradientColorKey(new Color(0.4f, 0.8f, 0.4f, 1f), 0f),
                new GradientColorKey(new Color(0.4f, 0.8f, 0.4f, 1f), 0.7f),
                new GradientColorKey(new Color(0.4f, 0.8f, 0.4f, 0f), 1f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;
        
        // 大小渐变
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.5f, 1.3f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
    }
}
