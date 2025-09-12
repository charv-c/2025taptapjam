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
    
    [Header("粒子纹理设置")]
    [SerializeField] private Sprite springParticleSprite1; // 春季粒子纹理1
    [SerializeField] private Sprite springParticleSprite2; // 春季粒子纹理2
    [SerializeField] private Sprite summerParticleSprite1; // 夏季粒子纹理1
    [SerializeField] private Sprite summerParticleSprite2; // 夏季粒子纹理2
    
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
        ConfigureSpringParticleSystem(springParticle, springParticleSprite1, springParticleSprite2);
        
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
        ConfigureSummerParticleSystem(summerParticle, summerParticleSprite1, summerParticleSprite2);
        
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
            textureSheetAnimation.useRandomRow = true;
            
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
            textureSheetAnimation.useRandomRow = true;
            
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
        
        Debug.Log("SeasonParticlePrefab: 创建了新的粒子材质");
        
        return particleMaterial;
    }
}
