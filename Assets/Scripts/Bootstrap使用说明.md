# Bootstrap系统使用说明

## 概述

为了解决从非Startup场景启动测试时系统初始化问题，我们设计了一套完整的Bootstrap启动引导系统。这个系统确保无论从哪个场景启动，都能正常加载音频管理、提示框管理、游戏流程管理等核心功能。

## 核心组件

### 1. GameBootstrap (核心启动管理器)
- **作用**: 负责在任意场景启动时检查并创建必要的单例管理器
- **位置**: `Assets/Scripts/GameBootstrap.cs`
- **功能**: 
  - 自动检测AudioManager、InfoPopupManager、GameFlowManager是否存在
  - 如果不存在，自动创建并配置这些管理器
  - 支持预制体加载和运行时创建两种模式

### 2. SceneInitializer (场景初始化器)
- **作用**: 每个场景的初始化协调器，确保Bootstrap在场景启动时完成
- **位置**: `Assets/Scripts/SceneInitializer.cs`
- **功能**:
  - 自动检测并确保GameBootstrap存在
  - 等待Bootstrap初始化完成
  - 为测试场景提供额外的调试信息

### 3. IBootstrapAware接口
- **作用**: 允许其他组件监听Bootstrap完成事件
- **实现类**: Level2Manager、Level3Manager
- **功能**: 在Bootstrap完成后收到通知，确保依赖的系统已就绪

## 使用方法

### 自动设置（推荐）

1. **打开Bootstrap设置助手**
   - Unity编辑器菜单：`游戏工具 -> Bootstrap设置助手`

2. **一键自动设置**
   - 点击"自动创建所有必要资源"按钮
   - 系统会自动创建所有预制体和目录结构
   - 为当前场景添加SceneInitializer

3. **验证设置**
   - 点击"检查Bootstrap系统状态"验证设置是否完整

### 手动设置

如果需要手动设置，请按以下步骤：

1. **创建必要目录**
   ```
   Assets/Resources/
   Assets/Resources/Prefabs/
   ```

2. **为每个场景添加SceneInitializer**
   - 在场景中创建空GameObject
   - 添加SceneInitializer组件
   - 如果是测试场景，勾选`isTestingScene`

3. **确保预制体存在**
   - AudioManager.prefab
   - InfoPopupManager.prefab  
   - GameFlowManager.prefab
   - GameBootstrap.prefab

## 工作原理

### 启动流程

```
场景启动
    ↓
SceneInitializer.Awake()
    ↓
GameBootstrap.EnsureInitialized()
    ↓
检查单例是否存在
    ↓
如果不存在，创建管理器
    ↓
标记Bootstrap完成
    ↓
通知IBootstrapAware组件
    ↓
Level2Manager/Level3Manager开始显示开场白
    ↓
游戏正常运行
```

### 关键设计特点

1. **零配置启动**: 从任意场景启动都能自动初始化
2. **容错性强**: 即使预制体缺失也能通过代码创建基础配置
3. **测试友好**: 提供详细的调试日志和状态检查
4. **向后兼容**: 不影响正常的Startup场景启动流程

## 场景管理器改进

### Level2Manager和Level3Manager的变化

- **新增Bootstrap支持**: 实现了IBootstrapAware接口
- **协程初始化**: 使用协程等待Bootstrap完成
- **容错处理**: 即使管理器缺失也能graceful降级
- **状态管理**: 通过标志位确保初始化只进行一次

### 主要改进点

```csharp
// 旧版本
void Start()
{
    if (InfoPopupManager.Instance != null)
    {
        // 直接使用，可能为null
    }
}

// 新版本  
void Start()
{
    GameBootstrap.EnsureInitialized();
    StartCoroutine(InitializeLevelCoroutine());
}

private IEnumerator InitializeLevelCoroutine()
{
    while (!GameBootstrap.IsInitialized)
        yield return new WaitForSeconds(0.1f);
    
    // 现在可以安全使用所有管理器
}
```

## 测试与调试

### 日志系统
系统使用GameLogger输出详细的初始化日志：
- `GameBootstrap: 初始化开始`
- `Level2Manager: Bootstrap完成，开始场景初始化`  
- `SceneInitializer: 场景初始化完成，所有核心系统正常`

### 调试模式
SceneInitializer在测试场景中会输出额外的系统状态信息：
- AudioManager状态
- InfoPopupManager状态
- GameFlowManager状态
- PlayerController状态
- LevelManager状态

### 状态检查
使用`GameBootstrap.ValidateCoreSystems()`检查所有核心系统是否正常工作。

## 常见问题

**Q: 从Level2启动时角色无法移动？**
A: 确保场景中有SceneInitializer组件，它会等待Bootstrap完成后启用角色操作。

**Q: 音频无法播放？**  
A: 检查AudioManager预制体是否正确配置了AudioSource组件。

**Q: 开场白不显示？**
A: 检查InfoPopupPanel预制体是否存在于Assets/Prefabs/目录下。

**Q: 如何验证系统是否正常工作？**
A: 使用Bootstrap设置助手的"检查Bootstrap系统状态"功能。

## 扩展指南

### 添加新的管理器

1. 在GameBootstrap中添加检查和创建逻辑：
```csharp
private void EnsureNewManager()
{
    if (NewManager.Instance == null)
    {
        // 创建管理器逻辑
    }
}
```

2. 在InitializeGameSystems协程中调用

3. 在ValidateCoreSystems中添加验证

### 实现IBootstrapAware

任何需要在Bootstrap完成后执行逻辑的组件都可以实现此接口：

```csharp
public class MyComponent : MonoBehaviour, IBootstrapAware
{
    public void OnBootstrapComplete()
    {
        // Bootstrap完成后的初始化逻辑
    }
}
```

## 性能考虑

- Bootstrap初始化通常在1-2帧内完成
- 使用协程避免阻塞主线程
- 单例检查开销极小
- 预制体加载比运行时创建更高效

---

**注意**: 这个系统是为了解决开发测试阶段的便利性问题，正式发布时建议仍然从Startup场景启动以确保完整的初始化流程。
