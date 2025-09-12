using UnityEngine;

/// <summary>
/// 季节粒子系统快速设置脚本 - 提供一键设置功能
/// </summary>
public class SeasonParticleSetup : MonoBehaviour
{
    [Header("快速设置")]
    [SerializeField] private bool autoSetupOnStart = false;
    [SerializeField] private Vector3 particleSystemPosition = Vector3.zero;
    
    [Header("粒子系统引用")]
    [SerializeField] private SeasonParticleManager particleManager;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupSeasonParticleSystem();
        }
    }
    
    /// <summary>
    /// 一键设置季节粒子系统
    /// </summary>
    [ContextMenu("一键设置季节粒子系统")]
    public void SetupSeasonParticleSystem()
    {
        // 检查是否已经存在粒子管理器
        if (particleManager == null)
        {
            particleManager = FindObjectOfType<SeasonParticleManager>();
        }
        
        if (particleManager != null)
        {
            Debug.Log("SeasonParticleSetup: 季节粒子系统已存在，跳过设置");
            return;
        }
        
        // 创建粒子系统父对象
        GameObject particleParent = new GameObject("SeasonParticleSystems");
        particleParent.transform.position = particleSystemPosition;
        
        // 添加粒子管理器
        particleManager = particleParent.AddComponent<SeasonParticleManager>();
        
        // 创建春季粒子系统
        GameObject springParticleObj = CreateParticleSystem("SpringParticleSystem", particleParent.transform);
        ParticleSystem springParticle = springParticleObj.GetComponent<ParticleSystem>();
        ConfigureSpringParticleSystem(springParticle);
        
        // 创建夏季粒子系统
        GameObject summerParticleObj = CreateParticleSystem("SummerParticleSystem", particleParent.transform);
        ParticleSystem summerParticle = summerParticleObj.GetComponent<ParticleSystem>();
        ConfigureSummerParticleSystem(summerParticle);
        
        // 设置粒子管理器引用
        SetParticleSystemReferences(springParticle, summerParticle);
        
        Debug.Log("SeasonParticleSetup: 季节粒子系统设置完成！");
    }
    
    /// <summary>
    /// 创建粒子系统对象
    /// </summary>
    /// <param name="name">对象名称</param>
    /// <param name="parent">父对象</param>
    /// <returns>创建的GameObject</returns>
    private GameObject CreateParticleSystem(string name, Transform parent)
    {
        GameObject particleObj = new GameObject(name);
        particleObj.transform.SetParent(parent);
        particleObj.transform.localPosition = Vector3.zero;
        
        ParticleSystem particle = particleObj.AddComponent<ParticleSystem>();
        particle.playOnAwake = false;
        
        return particleObj;
    }
    
    /// <summary>
    /// 设置粒子系统引用到管理器
    /// </summary>
    /// <param name="springParticle">春季粒子系统</param>
    /// <param name="summerParticle">夏季粒子系统</param>
    private void SetParticleSystemReferences(ParticleSystem springParticle, ParticleSystem summerParticle)
    {
        if (particleManager == null) return;
        
        // 使用反射设置私有字段
        var springField = typeof(SeasonParticleManager).GetField("springParticleSystem", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var summerField = typeof(SeasonParticleManager).GetField("summerParticleSystem", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (springField != null)
        {
            springField.SetValue(particleManager, springParticle);
        }
        
        if (summerField != null)
        {
            summerField.SetValue(particleManager, summerParticle);
        }
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
    
    /// <summary>
    /// 测试粒子系统
    /// </summary>
    [ContextMenu("测试粒子系统")]
    public void TestParticleSystem()
    {
        if (particleManager == null)
        {
            particleManager = FindObjectOfType<SeasonParticleManager>();
        }
        
        if (particleManager == null)
        {
            Debug.LogWarning("SeasonParticleSetup: 未找到SeasonParticleManager，请先设置粒子系统");
            return;
        }
        
        // 测试春季粒子
        particleManager.TestPlaySpringParticles();
        
        // 3秒后测试夏季粒子
        Invoke(nameof(TestSummerParticles), 3f);
    }
    
    private void TestSummerParticles()
    {
        if (particleManager != null)
        {
            particleManager.TestPlaySummerParticles();
        }
    }
    
    /// <summary>
    /// 清理粒子系统
    /// </summary>
    [ContextMenu("清理粒子系统")]
    public void CleanupParticleSystem()
    {
        if (particleManager != null)
        {
            particleManager.StopAllParticles();
        }
        
        GameObject particleParent = GameObject.Find("SeasonParticleSystems");
        if (particleParent != null)
        {
            DestroyImmediate(particleParent);
            Debug.Log("SeasonParticleSetup: 粒子系统已清理");
        }
    }
}
