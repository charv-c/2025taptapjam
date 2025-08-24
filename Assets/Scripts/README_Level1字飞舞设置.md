# Level1 字飞舞功能设置说明

## 概述
`Level1CharacterFly` 脚本实现了类似 `ButtonController` 的字飞舞功能，可以在 level1 场景中让字符以螺旋轨迹飞向指定目标位置。

## 功能特点
- 螺旋轨迹飞行动画
- 缩放动画效果
- 音效播放
- 自动销毁飞舞的字符
- 防止重复飞行

## 设置步骤

### 1. 在 level1 场景中添加 Level1CharacterFly 组件
1. 打开 Unity 编辑器
2. 打开 `Assets/Scenes/level1.unity` 场景
3. 在 Hierarchy 窗口中右键点击空白区域
4. 选择 "Create Empty" 创建一个空的 GameObject
5. 将新创建的对象重命名为 "Level1CharacterFly"
6. 选中 Level1CharacterFly 对象
7. 在 Inspector 窗口中点击 "Add Component"
8. 搜索并添加 "Level1CharacterFly" 脚本

### 2. 配置组件参数
在 Inspector 中设置以下参数：

#### 飞舞设置
- **Target Position**: 设置目标位置（可以是任何 Transform）
- **Target Canvas**: 设置目标 Canvas（可选，如果不设置会使用场景中的第一个 Canvas）
- **Fly Duration**: 飞行时长（默认 1.5 秒）
- **Spiral Radius**: 螺旋半径（默认 50）
- **Spiral Turns**: 螺旋圈数（默认 3）

#### 字体设置
- **Chinese Font**: 设置中文字体（可选，如果不设置会尝试从 StringSelector 获取）

### 3. 调用飞舞功能
在代码中调用飞舞功能：

```csharp
// 获取 Level1CharacterFly 组件
Level1CharacterFly characterFly = FindObjectOfType<Level1CharacterFly>();

if (characterFly != null)
{
    // 开始字符飞舞
    // character: 要飞舞的字符
    // startPosition: 起始位置（屏幕坐标）
    characterFly.StartCharacterFly("蝶", new Vector2(100, 200));
}
```

## 使用示例

### 示例 1: 从屏幕中央飞向目标
```csharp
Level1CharacterFly characterFly = FindObjectOfType<Level1CharacterFly>();
if (characterFly != null)
{
    // 从屏幕中央开始飞舞
    Vector2 startPosition = Vector2.zero;
    characterFly.StartCharacterFly("蝶", startPosition);
}
```

### 示例 2: 从指定位置飞向目标
```csharp
Level1CharacterFly characterFly = FindObjectOfType<Level1CharacterFly>();
if (characterFly != null)
{
    // 从指定位置开始飞舞
    Vector2 startPosition = new Vector2(-200, 100);
    characterFly.StartCharacterFly("蝶", startPosition);
}
```

### 示例 3: 检查是否正在飞行
```csharp
Level1CharacterFly characterFly = FindObjectOfType<Level1CharacterFly>();
if (characterFly != null && !characterFly.IsFlying())
{
    // 只有在没有字符飞行时才开始新的飞行
    characterFly.StartCharacterFly("蝶", Vector2.zero);
}
```

## 注意事项
1. **目标位置设置**: 确保 Target Position 已正确设置，否则飞舞动画无法正常工作
2. **Canvas 设置**: 如果场景中有多个 Canvas，建议明确设置 Target Canvas
3. **字体设置**: 建议设置中文字体以确保字符正确显示
4. **重复飞行**: 脚本会自动防止多个字符同时飞行，如果需要连续飞行，请等待当前飞行完成
5. **自动销毁**: 飞舞的字符会在动画完成后延迟 2 秒自动销毁

## 动画效果
- **螺旋轨迹**: 字符会沿着螺旋轨迹飞向目标
- **缩放动画**: 飞行过程中字符会有缩放效果
- **音效**: 播放目标飞入音效
- **平滑过渡**: 使用 SmoothStep 实现平滑的动画过渡

## 调试信息
脚本会在控制台输出详细的调试信息，包括：
- 飞行开始和完成的信息
- 起始位置和目标位置
- 音效播放状态
- 错误和警告信息
