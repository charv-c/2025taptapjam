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
    
    [Header("粒子纹理设置")]
    [SerializeField] private Sprite springParticleSprite1; // 春季粒子纹理1
    [SerializeField] private Sprite springParticleSprite2; // 春季粒子纹理2
    [SerializeField] private Sprite summerParticleSprite1; // 夏季粒子纹理1
    [SerializeField] private Sprite summerParticleSprite2; // 夏季粒子纹理2
    
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
        ConfigureSpringParticleSystem(springParticle, springParticleSprite1, springParticleSprite2);
        
        // 创建夏季粒子系统
        GameObject summerParticleObj = CreateParticleSystem("SummerParticleSystem", particleParent.transform);
        ParticleSystem summerParticle = summerParticleObj.GetComponent<ParticleSystem>();
        ConfigureSummerParticleSystem(summerParticle, summerParticleSprite1, summerParticleSprite2);
        
        // 设置粒子管理器引用
        SetParticleSystemReferences(springParticle, summerParticle);
        
        // 设置粒子纹理
        if (particleManager != null)
        {
            particleManager.SetSpringParticleSprites(springParticleSprite1, springParticleSprite2);
            particleManager.SetSummerParticleSprites(summerParticleSprite1, summerParticleSprite2);
        }
        
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
        var main = particle.main;
        main.playOnAwake = false;
        
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
    /// <param name="sprite1">粒子纹理1</param>
    /// <param name="sprite2">粒子纹理2</param>
    private void ConfigureSpringParticleSystem(ParticleSystem particleSystem, Sprite sprite1, Sprite sprite2)
    {
        var main = particleSystem.main;
        var emission = particleSystem.emission;
        var shape = particleSystem.shape;
        var velocityOverLifetime = particleSystem.velocityOverLifetime;
        var colorOverLifetime = particleSystem.colorOverLifetime;
        var sizeOverLifetime = particleSystem.sizeOverLifetime;
        var textureSheetAnimation = particleSystem.textureSheetAnimation;
        
        // 基本设置
        main.startLifetime = 2f;
        main.startSpeed = 2f;
        main.startSize = 0.5f;
        main.startColor = Color.white; // 固定白色
        main.maxParticles = 50;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        
        // 发射设置
        emission.enabled = true;
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
        
        // 颜色设置 - 白色粒子，透明度不变
        colorOverLifetime.enabled = false; // 禁用颜色生命周期变化
        
        // 大小渐变
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.5f, 1.2f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // 纹理设置 - 支持双纹理随机选择
        if (sprite1 != null || sprite2 != null)
        {
            textureSheetAnimation.enabled = true;
            textureSheetAnimation.mode = ParticleSystemAnimationMode.Sprites;
            
            // 添加所有可用的纹理
            if (sprite1 != null)
                textureSheetAnimation.AddSprite(sprite1);
            if (sprite2 != null)
                textureSheetAnimation.AddSprite(sprite2);
            
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
            
            // 设置渲染模式以正确显示Sprite
            main.startRotation3D = true;
            var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
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
    /// <param name="particleSystem">粒子系统</param>
    /// <param name="sprite1">粒子纹理1</param>
    /// <param name="sprite2">粒子纹理2</param>
    private void ConfigureSummerParticleSystem(ParticleSystem particleSystem, Sprite sprite1, Sprite sprite2)
    {
        var main = particleSystem.main;
        var emission = particleSystem.emission;
        var shape = particleSystem.shape;
        var velocityOverLifetime = particleSystem.velocityOverLifetime;
        var colorOverLifetime = particleSystem.colorOverLifetime;
        var sizeOverLifetime = particleSystem.sizeOverLifetime;
        var textureSheetAnimation = particleSystem.textureSheetAnimation;
        
        // 基本设置
        main.startLifetime = 2.5f;
        main.startSpeed = 2.5f;
        main.startSize = 0.6f;
        main.startColor = Color.white; // 固定白色
        main.maxParticles = 60;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        
        // 发射设置
        emission.enabled = true;
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
        
        // 颜色设置 - 白色粒子，透明度不变
        colorOverLifetime.enabled = false; // 禁用颜色生命周期变化
        
        // 大小渐变
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.5f, 1.3f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // 纹理设置 - 支持双纹理随机选择
        if (sprite1 != null || sprite2 != null)
        {
            textureSheetAnimation.enabled = true;
            textureSheetAnimation.mode = ParticleSystemAnimationMode.Sprites;
            
            // 添加所有可用的纹理
            if (sprite1 != null)
                textureSheetAnimation.AddSprite(sprite1);
            if (sprite2 != null)
                textureSheetAnimation.AddSprite(sprite2);
            
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
            
            // 设置渲染模式以正确显示Sprite
            main.startRotation3D = true;
            var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
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
        
        Debug.Log("SeasonParticleSetup: 创建了新的粒子材质");
        
        return particleMaterial;
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
