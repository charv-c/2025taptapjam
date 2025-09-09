# 滩涂互动逻辑使用说明

## 概述

这个功能实现了伪代码中的滩涂互动逻辑：

```伪代码
if（player1.char=="芽"）
    if（season=="春"） 
         autohint.show（"「芽」喜盛夏，待季节更迭再试吧"）
    else 
         花.visible=true；
          autohint.show（「芽」逢盛夏，终得绽放成「花」"）；
         进程等待500ms
         隹.visible=true
else 
     autohint.show（"一片湿润滩涂，土质肥沃，适合生命成长"）
```

## 实现方式

### 滩涂物体脚本方式（推荐）

使用 `BeachObject.cs` 组件：

1. 将 `BeachObject` 脚本附加到letter为"滩"的物体上
2. 在Inspector中设置：
   - `flowerObject`: 花物体的引用
   - `zhuiObject`: 隹物体的引用
   - `delayBeforeShowZhui`: 显示隹物体前的延迟时间（默认0.5秒）
   - `enableDebugLog`: 是否启用调试日志

## 功能说明

### 触发条件

当玩家与letter为"滩"的Highlight对象互动时，会自动执行滩涂逻辑。

### 逻辑流程

1. **检查玩家携带字符**：
   - 获取player1（第一个玩家）的CarryCharacter

2. **如果携带字符是"芽"**：
   - **春季**：显示提示"「芽」喜盛夏，待季节更迭再试吧"
   - **非春季（夏季）**：
     - 显示花物体
     - 显示提示"「芽」逢盛夏，终得绽放成「花」"
     - 等待500ms后显示隹物体

3. **如果携带字符不是"芽"**：
   - 显示提示"一片湿润滩涂，土质肥沃，适合生命成长"

### 依赖组件

- `PlayerController`: 获取玩家信息
- `Level3Manager`: 检查当前季节
- `AutoHint`: 显示提示信息
- `Highlight`: 控制物体显示/隐藏

## 使用方法

### 在场景中设置

1. 确保场景中有以下对象：
   - letter为"滩"的Highlight对象（需要附加BeachObject脚本）
   - letter为"花"的Highlight对象（初始隐藏）
   - letter为"隹"的Highlight对象（初始隐藏）

2. 确保场景中有以下组件：
   - `PlayerController`
   - `Level3Manager`
   - `AutoHint`

3. 设置滩涂物体：
   - 将 `BeachObject` 脚本附加到letter为"滩"的物体上
   - 在Inspector中设置花和隹物体的引用

### 测试

在 `BeachObject` 组件的Inspector中，点击"测试滩涂互动"按钮可以测试功能。

## 技术细节

### 季节检查

使用 `Level3Manager.IsSpring()` 方法检查当前季节：
- `true`: 春季
- `false`: 夏季

### 物体显示

使用 `Highlight.ShowObject()` 方法显示物体，该方法会：
- 启用SpriteRenderer
- 启用Collider2D
- 启用Light2D（如果存在）

### 提示显示

使用 `AutoHint.ReceiveBroadcast()` 方法显示提示，该方法会：
- 淡入显示提示
- 停留指定时间
- 淡出隐藏

### 延迟执行

使用 `StartCoroutine()` 和 `WaitForSeconds()` 实现500ms延迟。

## 扩展性

这个设计允许：
1. 滩涂逻辑完全独立，易于维护和修改
2. 可以轻松添加更多季节相关的互动
3. 可以扩展更多字符的滩涂互动
4. 脚本直接附加到滩物体上，逻辑更清晰

## 注意事项

1. 确保场景中有必要的组件和对象
2. 花和隹物体应该初始设置为隐藏状态
3. 季节切换需要通过Level3Manager进行
4. 提示文本可以通过修改代码或配置进行调整
5. **重要**：必须将BeachObject脚本附加到letter为"滩"的物体上
6. 如果没有设置花和隹物体的引用，脚本会自动查找场景中的对应物体
