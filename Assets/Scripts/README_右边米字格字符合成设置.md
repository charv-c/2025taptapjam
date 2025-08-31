# 右边米字格字符合成设置

## 概述

本系统实现了当右边的人和combine变成其他字时，右边的米字格使用右边米字格字典的sprite的功能。

## 功能特性

- **自动类型检测**：系统会自动检测米字格的类型（左米字格或右米字格）
- **智能sprite选择**：根据米字格类型自动选择对应的sprite字典
- **广播系统集成**：通过广播系统自动处理字符合成事件
- **回退机制**：如果没有对应类型的米字格sprite，会自动使用普通sprite

## 设置步骤

### 1. 配置右边米字格字典

在 `PublicData` 组件的Inspector中，找到"右米字格图片映射"部分：

1. 点击"+"按钮添加新的映射条目
2. 在 `character` 字段中输入字符名称（如"人"、"木"、"火"等）
3. 在 `sprite` 字段中拖入对应的右边米字格Sprite图片
4. 重复上述步骤，为所有需要的字符添加映射

### 2. 设置右边米字格对象

对于场景中的右边米字格对象：

1. 确保该对象有 `MiSquareController` 组件
2. 在 `MiSquareController` 组件中，将 `MiZiGeType` 设置为 `Right`
3. 添加 `RightMiSquareBroadcastReceiver` 组件
4. 在 `RightMiSquareBroadcastReceiver` 组件中，将 `Right Mi Square Controller` 字段设置为对应的 `MiSquareController`

### 3. 确保广播管理器存在

确保场景中有 `BroadcastManager` 对象，如果没有：

1. 创建一个空的GameObject
2. 添加 `BroadcastManager` 组件
3. 确保该对象在场景加载时就存在

## 工作原理

### 字符合成流程

1. **玩家操作**：玩家选择两个字符并点击"拼"按钮
2. **字符合成**：`ButtonController` 执行字符合成逻辑
3. **广播发送**：合成成功后发送 `combine_success` 广播
4. **广播接收**：`RightMiSquareBroadcastReceiver` 接收广播
5. **sprite更新**：根据当前玩家携带的字符更新右边米字格的sprite

### 化字流程（ChangeMi）

1. **玩家交互**：玩家携带"人"字符与可化字对象交互
2. **字符合成**：`Highlight.ChangeMi()` 方法执行字符合成
3. **类型检测**：自动检测米字格类型
4. **sprite选择**：根据类型选择对应的sprite字典
5. **显示更新**：更新米字格显示

## 代码示例

### 手动设置右边米字格

```csharp
// 获取右边米字格控制器
MiSquareController rightMiSquare = GetComponent<MiSquareController>();

// 设置米字格类型为右米字格
rightMiSquare.SetMiZiGeType(MiSquareController.MiZiGeType.Right);

// 检查是否有右米字格sprite
bool hasRightSprite = rightMiSquare.HasMiZiGeSprite("人");

if (hasRightSprite)
{
    // 使用右米字格sprite
    rightMiSquare.SetMiSquareSprite("人");
}
else
{
    // 使用普通sprite
    rightMiSquare.SetNormalSprite("人");
}
```

### 获取右米字格sprite

```csharp
// 直接从PublicData获取右米字格sprite
Sprite rightSprite = PublicData.GetRightMiZiGeSprite("人");

// 检查是否有右米字格sprite
bool hasRightSprite = PublicData.HasRightMiZiGeSprite("人");

// 获取所有右米字格字符
List<string> rightCharacters = PublicData.GetAllRightMiZiGeCharacters();
```

## 测试方法

### 1. 使用ContextMenu测试

在 `RightMiSquareBroadcastReceiver` 组件上右键，选择"手动更新右边米字格"来测试功能。

### 2. 运行时测试

1. 运行游戏
2. 让玩家携带"人"字符
3. 与可化字对象交互（如"亭"、"山"、"火"等）
4. 观察右边米字格是否正确显示对应的右米字格sprite

### 3. 字符合成测试

1. 选择两个可以合成的字符
2. 点击"拼"按钮
3. 观察右边米字格是否正确更新

## 故障排除

### 常见问题

1. **右边米字格不更新**
   - 检查 `RightMiSquareBroadcastReceiver` 是否正确添加到右边米字格对象
   - 确认 `MiSquareController` 的 `MiZiGeType` 设置为 `Right`
   - 检查 `BroadcastManager` 是否存在

2. **显示普通sprite而不是右米字格sprite**
   - 检查 `PublicData` 中的"右米字格图片映射"是否配置正确
   - 确认字符名称是否完全一致
   - 检查Sprite资源是否正确导入

3. **广播不工作**
   - 确认 `BroadcastManager` 在场景中存在
   - 检查 `RightMiSquareBroadcastReceiver` 是否正确注册了 `ReceiveBroadcast` 方法

### 调试方法

1. **启用日志**：在 `RightMiSquareBroadcastReceiver` 中启用 `Enable Logging`
2. **查看Console**：观察调试日志输出
3. **手动测试**：使用ContextMenu手动测试功能

## 扩展功能

如果需要添加更多米字格类型或特殊处理逻辑，可以：

1. 在 `MiSquareController.MiZiGeType` 枚举中添加新类型
2. 在 `PublicData` 中添加对应的映射字典
3. 创建新的广播接收器处理特定逻辑

## 注意事项

1. **字符名称一致性**：确保所有地方使用的字符名称完全一致
2. **资源管理**：确保所有Sprite资源都正确导入和配置
3. **性能考虑**：广播系统使用反射，避免过于频繁的广播调用
4. **初始化顺序**：确保所有组件在游戏开始时正确初始化
