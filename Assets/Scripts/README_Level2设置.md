# Level2 场景设置说明

## 概述
为了确保在切换到 level2 时允许所有移动、切换、回车、空格操作，需要在 level2 场景中添加 `Level2Manager` 脚本。

## 设置步骤

### 1. 在 level2 场景中添加 Level2Manager
1. 打开 Unity 编辑器
2. 打开 `Assets/Scenes/level2.unity` 场景
3. 在 Hierarchy 窗口中右键点击空白区域
4. 选择 "Create Empty" 创建一个空的 GameObject
5. 将新创建的对象重命名为 "Level2Manager"
6. 选中 Level2Manager 对象
7. 在 Inspector 窗口中点击 "Add Component"
8. 搜索并添加 "Level2Manager" 脚本

### 2. 验证设置
设置完成后，当从教学关卡切换到 level2 时：
- 所有玩家的移动功能将被启用
- 玩家切换功能（空格键）将被启用
- 所有玩家的回车键交互将被启用
- 第一个玩家将被设置为当前控制角色

## 脚本功能
`Level2Manager` 脚本会在场景加载时自动：
1. 查找场景中的 `PlayerController`
2. 启用所有玩家的输入
3. 启用所有玩家的回车键响应
4. 启用玩家切换功能
5. 设置第一个玩家为当前控制角色

## 注意事项
- 脚本会在场景加载后延迟一帧执行，确保 `PlayerController` 已完全初始化
- 如果场景中没有找到 `PlayerController`，会在控制台输出错误信息
- 所有操作启用后会在控制台输出确认信息
