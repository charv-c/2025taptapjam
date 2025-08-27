# Level1 动态遮罩功能设置说明

## 功能概述

在level1场景中，当玩家在左边移动时，mask1遮罩会遮住右边屏幕；当玩家在右边移动时，mask1遮罩会遮住左边屏幕。这个功能通过MaskController脚本实现。

## 文件结构

- `MaskController.cs` - 主要的遮罩控制脚本
- `TutorialManager.cs` - 已集成MaskController功能

## 设置步骤

### 1. 场景设置

1. 在level1场景中，找到要作为遮罩的UI对象（通常是"TutorialMask"）
2. 该对象应该包含RectTransform和Image组件
3. 如果没有Image组件，MaskController会自动添加

### 2. 脚本设置

1. 将MaskController.cs脚本直接挂载到遮罩UI对象上
2. 在Inspector中设置以下参数：
   - `playerController`: 拖拽PlayerController对象（可选，会自动查找）
   - `leftMaskPosition`: 左边遮罩位置（默认：-960, 0）
   - `rightMaskPosition`: 右边遮罩位置（默认：960, 0）

## 功能特性

### 自动遮罩切换
- 当控制Player1时，遮罩移动到左边位置
- 当控制Player2时，遮罩移动到右边位置
- 遮罩在level1场景中一直保持显示和动态更新

### 场景限制
- 只在level1场景中工作
- 其他场景中不会执行遮罩逻辑

### 性能优化
- 只有当遮罩位置发生变化时才更新
- 避免不必要的UI更新

## 公共方法

### MaskController类

```csharp
// 手动设置遮罩位置
public void SetMaskPosition(Vector2 position)

// 设置遮罩为左边位置（遮住右边屏幕）
public void SetMaskToLeft()

// 设置遮罩为右边位置（遮住左边屏幕）
public void SetMaskToRight()

// 启用遮罩
public void EnableMask()

// 禁用遮罩
public void DisableMask()

// 设置遮罩透明度
public void SetMaskAlpha(float alpha)

// 获取当前遮罩位置
public Vector2 GetCurrentMaskPosition()

// 检查遮罩是否启用
public bool IsMaskEnabled()
```

## 集成说明

### TutorialManager集成
- TutorialManager会自动查找场景中的MaskController
- 遮罩在level1场景中一直保持显示和动态更新
- 在特定步骤中会设置遮罩的初始位置

### 遮罩位置设置
- `HandleMoveToDog()` - 移动到狗时设置遮罩到右边位置
- 玩家切换时 - 根据新玩家位置设置遮罩位置

## 调试信息

脚本会输出以下调试信息：
- 遮罩初始化完成
- 遮罩位置变化（当前控制玩家和遮罩移动方向）
- 遮罩位置设置确认

## 注意事项

1. 确保遮罩对象在Canvas中正确设置
2. 将MaskController脚本直接挂载到遮罩UI对象上
3. 遮罩位置坐标基于Canvas的anchoredPosition
4. 功能只在level1场景中生效
5. TutorialManager会自动查找场景中的MaskController，无需手动创建

## 故障排除

### 遮罩不显示
- 检查遮罩对象是否存在
- 确认Image组件已添加
- 验证遮罩的透明度设置
- 确认MaskController脚本已挂载到遮罩对象上

### 遮罩位置不正确
- 检查leftMaskPosition和rightMaskPosition设置
- 确认Canvas的缩放模式设置
- 验证遮罩的锚点设置

### 动态遮罩不工作
- 确认当前场景为level1
- 检查PlayerController是否正确设置
- 确认MaskController脚本已正确挂载
