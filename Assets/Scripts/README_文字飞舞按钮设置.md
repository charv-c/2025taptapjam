# 文字飞舞按钮设置说明

## 概述
`CharacterFlyButton` 脚本用于创建一个快捷按钮来触发文字飞舞动画。

## 在Unity中的设置步骤

### 1. 创建按钮UI
1. 在场景中右键选择 `UI > Button` 创建按钮
2. 将按钮重命名为 "CharacterFlyButton"
3. 调整按钮位置和大小到合适的位置

### 2. 添加脚本组件
1. 选择刚创建的按钮
2. 在 Inspector 面板中点击 "Add Component"
3. 搜索并添加 `CharacterFlyButton` 脚本

### 3. 配置脚本参数

#### 按钮设置 (Button Settings)
- **Fly Button**: 拖拽按钮的 Button 组件到此字段
- **Button Text**: 拖拽按钮上的 TextMeshProUGUI 组件到此字段

#### 飞舞设置 (Fly Settings)
- **Character To Fly**: 设置要飞舞的字符（默认："蝶"）
- **Target Position**: 拖拽目标位置的 Transform 到此字段
- **Target Canvas**: 拖拽目标 Canvas 到此字段

#### UI设置 (UI Settings)
- **Button Text Content**: 设置按钮显示的文字（默认："触发飞舞"）

### 4. 确保依赖组件存在
确保场景中有以下组件：
- `ButtonController` 实例（包含Level1飞舞功能）
- `AudioManager` 实例（用于播放音效）

## 功能说明

### 按钮功能
- 点击按钮会触发指定字符的飞舞动画
- 起点：屏幕正中间（Vector2.zero）
- 终点：设置的 Target Position
- 如果已有字符在飞行中，会忽略新的点击

### 音效
- 点击按钮时会播放 UI 点击音效
- 飞舞动画会播放目标飞入音效

### 安全检查
- 自动检查 `ButtonController` 实例是否存在
- 如果实例不存在，按钮会被禁用
- 防止重复触发飞舞动画

## 示例配置

### 基本配置
```
Button Settings:
- Fly Button: [Button组件]
- Button Text: [TextMeshProUGUI组件]

Fly Settings:
- Character To Fly: "蝶"
- Target Position: [目标Transform]
- Target Canvas: [Canvas]

UI Settings:
- Button Text Content: "触发飞舞"
```

### 高级用法
可以通过代码动态修改设置：
```csharp
CharacterFlyButton flyButton = FindObjectOfType<CharacterFlyButton>();
if (flyButton != null)
{
    flyButton.SetCharacterToFly("新字符");
    flyButton.SetTargetPosition(newTarget);
    flyButton.SetButtonText("新按钮文字");
}
```

## 注意事项
1. 确保场景中有 `ButtonController` 实例（包含Level1飞舞功能）
2. 目标位置应该是 UI 坐标（RectTransform）
3. 按钮应该在 Canvas 内部
4. 飞舞动画期间按钮不会响应新的点击
