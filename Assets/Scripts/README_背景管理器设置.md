# 背景管理器设置

## 概述

背景管理器用于在"停"和"雨"互动后，自动将左右两边背景的sprite改为没雨的sprite。

## 功能特性

- **自动背景切换**：当玩家携带"停"字符与"雨"对象互动时，自动切换到晴天背景
- **广播系统集成**：通过广播系统自动处理背景切换事件
- **灵活配置**：可以在Inspector中直接配置背景图片
- **调试支持**：提供完整的调试日志和测试功能

## 设置步骤

### 1. 创建背景管理器对象

1. 在场景中创建一个空的GameObject，命名为"BackgroundManager"
2. 添加`BackgroundManager`组件

### 2. 配置背景对象引用

在`BackgroundManager`组件的Inspector中设置：

- **Left Background**：拖入左背景的SpriteRenderer组件
- **Right Background**：拖入右背景的SpriteRenderer组件

### 3. 配置背景图片

在`BackgroundManager`组件的Inspector中设置背景图片：

- **Rainy Left Background**：拖入左雨天背景Sprite（如"左边背景-雨.png"）
- **Rainy Right Background**：拖入右雨天背景Sprite（如"右边背景-雨.png"）
- **Sunny Left Background**：拖入左晴天背景Sprite（如"底图 白.png"）
- **Sunny Right Background**：拖入右晴天背景Sprite（如"底图 白.png"）

### 4. 确保广播管理器存在

确保场景中有`BroadcastManager`对象，如果没有：

1. 创建一个空的GameObject
2. 添加`BroadcastManager`组件
3. 确保该对象在场景加载时就存在

## 工作原理

### 雨停流程

1. **玩家操作**：玩家携带"停"字符与"雨"对象交互
2. **广播发送**：`Highlight.BroadcastCarryLetterValue`发送"停"广播
3. **广播接收**：`BackgroundManager`接收"停"广播
4. **背景切换**：自动切换到晴天背景

### 背景切换逻辑

- 当接收到"停"广播时，`BackgroundManager`会自动调用`SwitchToSunnyBackground()`
- 该方法会将左右背景的sprite都切换为晴天背景
- 同时会播放相应的调试日志

## 代码示例

### 手动切换背景

```csharp
// 获取背景管理器
BackgroundManager backgroundManager = FindObjectOfType<BackgroundManager>();

// 切换到雨天背景
backgroundManager.SwitchToRainyBackground();

// 切换到晴天背景
backgroundManager.SwitchToSunnyBackground();
```

### 设置背景引用

```csharp
// 设置左背景
SpriteRenderer leftBg = GameObject.Find("LeftBackground").GetComponent<SpriteRenderer>();
backgroundManager.SetLeftBackground(leftBg);

// 设置右背景
SpriteRenderer rightBg = GameObject.Find("RightBackground").GetComponent<SpriteRenderer>();
backgroundManager.SetRightBackground(rightBg);
```

### 设置背景图片

```csharp
// 设置雨天背景图片
Sprite rainyLeft = Resources.Load<Sprite>("左边背景-雨");
Sprite rainyRight = Resources.Load<Sprite>("右边背景-雨");
backgroundManager.SetRainyBackgroundSprites(rainyLeft, rainyRight);

// 设置晴天背景图片
Sprite sunnyLeft = Resources.Load<Sprite>("底图 白");
Sprite sunnyRight = Resources.Load<Sprite>("底图 白");
backgroundManager.SetSunnyBackgroundSprites(sunnyLeft, sunnyRight);
```

## 测试方法

### 1. 使用ContextMenu测试

在`BackgroundManager`组件上右键，选择以下测试选项：

- **切换到雨天背景**：手动切换到雨天背景
- **切换到晴天背景**：手动切换到晴天背景
- **模拟停广播**：模拟发送"停"广播

### 2. 运行时测试

1. 运行游戏
2. 让玩家携带"停"字符
3. 与"雨"对象交互
4. 观察背景是否正确切换到晴天背景

### 3. 调试日志

启用`Enable Logging`选项，在Console中查看详细的调试信息：

- 背景管理器初始化信息
- 背景切换操作日志
- 广播接收日志

## 故障排除

### 常见问题

1. **背景不切换**
   - 检查`BackgroundManager`是否正确添加到场景中
   - 确认背景对象引用是否正确设置
   - 检查背景图片是否正确导入

2. **广播不工作**
   - 确认`BroadcastManager`在场景中存在
   - 检查`BackgroundManager`是否正确注册了`ReceiveBroadcast`方法

3. **背景图片显示错误**
   - 检查背景图片的导入设置
   - 确认Sprite的Pivot和Pixels Per Unit设置正确
   - 检查背景对象的Transform设置

### 调试方法

1. **启用日志**：在`BackgroundManager`中启用`Enable Logging`
2. **查看Console**：观察调试日志输出
3. **手动测试**：使用ContextMenu手动测试功能
4. **检查引用**：确认所有背景对象和图片引用都正确设置

## 扩展功能

如果需要添加更多背景类型或特殊处理逻辑，可以：

1. 在`BackgroundManager`中添加新的背景类型
2. 添加新的广播处理逻辑
3. 创建更复杂的背景切换动画

## 注意事项

1. **图片资源**：确保所有背景图片都正确导入和配置
2. **性能考虑**：背景切换使用简单的sprite替换，性能较好
3. **内存管理**：背景图片会一直加载在内存中，注意内存使用
4. **初始化顺序**：确保所有组件在游戏开始时正确初始化
