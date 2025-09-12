# 季节粒子系统双纹理支持更新

## 概述
已成功修改季节粒子系统以支持显示sprite，使用两个纹理并随机选择。现在每个季节（春季和夏季）都可以使用两个不同的纹理，粒子系统会随机选择其中一个纹理来显示。

**颜色设置更新：** 所有粒子颜色已设置为白色，让纹理的原始颜色得以完整显示，透明度保持完全不透明。

**纹理分配模式：** 根据Unity官方教程实现随机挑选精灵，每个粒子在生成时随机选择sprite，实现约50/50的分配比例。

## 修改的文件

### 1. SeasonParticleManager.cs
**主要修改：**
- 将单个纹理字段扩展为双纹理字段：
  - `springParticleSprite1` 和 `springParticleSprite2` (春季粒子纹理)
  - `summerParticleSprite1` 和 `summerParticleSprite2` (夏季粒子纹理)

- 更新纹理动画配置（按照Unity官方教程）：
  - 支持添加多个sprite到textureSheetAnimation
  - 设置 `startFrame` 为 Random Between Two Constants (0-2)
  - 删除 `frameOverTime` 动画，不想要任何动画
  - 保持原有的渲染设置和材质配置

- 新增/更新方法：
  - `SetSpringParticleSprites(sprite1, sprite2)` - 设置春季双纹理
  - `SetSummerParticleSprites(sprite1, sprite2)` - 设置夏季双纹理
  - `GetSpringParticleSprite(index)` - 获取指定索引的春季纹理
  - `GetSummerParticleSprite(index)` - 获取指定索引的夏季纹理
  - `GetSpringParticleSprites()` - 获取所有春季纹理
  - `GetSummerParticleSprites()` - 获取所有夏季纹理

- 颜色设置更新：
  - 所有粒子初始颜色设置为白色 (`Color.white`)
  - 禁用颜色生命周期变化，透明度保持完全不透明
  - 让sprite纹理的原始颜色得以完整显示

### 2. SeasonParticlePrefab.cs
**主要修改：**
- 更新纹理字段定义，支持双纹理设置
- 修改配置方法签名以接受两个纹理参数
- 更新纹理设置逻辑，支持随机选择

### 3. SeasonParticleSetup.cs
**主要修改：**
- 更新纹理字段定义，支持双纹理设置
- 修改配置方法调用和实现
- 更新粒子管理器纹理设置调用

## 使用方法

### 在Inspector中设置
1. 在SeasonParticleManager组件中找到"粒子纹理设置"部分
2. 为每个季节设置两个纹理：
   - Spring Particle Sprite 1 & 2 (春季粒子纹理)
   - Summer Particle Sprite 1 & 2 (夏季粒子纹理)
3. 粒子系统会自动随机选择其中一个纹理显示

### 通过代码设置
```csharp
// 获取粒子管理器
SeasonParticleManager manager = FindObjectOfType<SeasonParticleManager>();

// 设置春季双纹理
Sprite springSprite1 = Resources.Load<Sprite>("SpringParticle1");
Sprite springSprite2 = Resources.Load<Sprite>("SpringParticle2");
manager.SetSpringParticleSprites(springSprite1, springSprite2);

// 设置夏季双纹理
Sprite summerSprite1 = Resources.Load<Sprite>("SummerParticle1");
Sprite summerSprite2 = Resources.Load<Sprite>("SummerParticle2");
manager.SetSummerParticleSprites(summerSprite1, summerSprite2);
```

### 获取纹理
```csharp
// 获取特定纹理
Sprite springTexture1 = manager.GetSpringParticleSprite(1);
Sprite springTexture2 = manager.GetSpringParticleSprite(2);

// 获取所有纹理
Sprite[] allSpringTextures = manager.GetSpringParticleSprites();
Sprite[] allSummerTextures = manager.GetSummerParticleSprites();
```

## 技术细节

### Unity官方教程实现步骤
按照Unity官方教程"挑选随机精灵"的步骤实现：

1. **将 Mode 设置为 Sprites**：`textureSheetAnimation.mode = ParticleSystemAnimationMode.Sprites`
2. **添加精灵条目**：使用 `textureSheetAnimation.AddSprite()` 添加2个精灵
3. **设置 Start Frame**：选择 Random Between Two Constants，输入 0 和 2
   - 系统将选择一个介于 0 到 2（不包括 2）之间的随机数，即 0 或 1
   - 对粒子使用相应的精灵
4. **删除 Frame over time 动画**：不想要任何动画，设置为固定值
5. **设置sprite数量**：设置 `numTilesX = 1` 和 `numTilesY = 1`
6. **启用随机行**：设置 `useRandomRow = true` 确保随机选择

### 纹理动画配置（按照Unity官方教程）
- 使用 `ParticleSystemAnimationMode.Sprites` 模式
- 设置 `startFrame` 为 Random Between Two Constants (0-2)
  - 系统将选择一个介于0到2（不包括2）之间的随机数，即0或1
  - 对粒子使用相应的精灵
- 删除 `frameOverTime` 动画，不想要任何动画
- 设置sprite数量：`numTilesX = 1` 和 `numTilesY = 1`
- 启用随机行：`useRandomRow = true` 确保随机选择
- 保持原有的渲染模式设置（Billboard）
- 支持部分纹理设置（可以只设置一个纹理）

### 兼容性
- 完全向后兼容，如果不设置第二个纹理，系统会正常工作
- 原有的单纹理设置方法仍然可用
- 所有现有的粒子效果参数保持不变

### 性能考虑
- 双纹理不会显著影响性能
- Unity的粒子系统已经优化了多纹理处理
- 随机选择在GPU层面处理，效率很高

## 注意事项
1. 确保纹理资源正确导入并设置为Sprite模式
2. 建议纹理尺寸相近以获得最佳视觉效果
3. 如果只设置一个纹理，粒子将始终使用该纹理
4. 纹理的透明度和颜色设置会影响最终显示效果
5. **颜色设置**：粒子颜色已设置为白色，透明度保持完全不透明，让纹理原始颜色完整显示
6. 粒子在整个生命周期中保持完全不透明，不会出现淡入淡出效果
7. **纹理分配**：按照Unity官方教程实现随机挑选精灵，每个粒子在生成时随机选择sprite，实现约50/50的分配比例

## 测试建议
1. 在Scene中创建粒子系统并设置双纹理
2. 触发季节切换事件观察随机纹理分配效果
3. 使用调试菜单测试不同配置
4. 验证粒子数量、大小、颜色等参数正常工作

## 故障排除

### 如果只显示一种sprite
1. **检查sprite设置**：确保两个sprite都已正确设置且不为null
2. **使用调试功能**：右键点击SeasonParticleManager组件，选择"测试sprite选择"查看详细设置
3. **验证设置**：确保以下设置正确：
   - `textureSheetAnimation.enabled = true`
   - `textureSheetAnimation.mode = ParticleSystemAnimationMode.Sprites`
   - `textureSheetAnimation.spriteCount = 2`
   - `textureSheetAnimation.startFrame` 范围设置为 0-2
   - `textureSheetAnimation.useRandomRow = true`
4. **重新配置**：尝试重新配置粒子系统，确保所有设置都正确应用

---
*更新日期: 2024年*
*版本: 双纹理支持 v1.0*
