# 窗口全屏和摄像机比例缩放设置指南


## 概述

本指南将帮助您设置Unity项目在打包时实现窗口全屏和摄像机比例缩放功能。

## 文件说明

### 1. WindowManager.cs
- **功能**：运行时管理窗口全屏和摄像机比例缩放
- **位置**：`Assets/Scripts/WindowManager.cs`

### 2. BuildSettingsHelper.cs
- **功能**：编辑器工具，帮助设置Player Settings
- **位置**：`Assets/Scripts/Editor/BuildSettingsHelper.cs`

## 设置步骤

### 步骤1：添加WindowManager到场景

1. 在场景中创建一个空对象，命名为"WindowManager"
2. 将 `WindowManager.cs` 脚本添加到该对象上
3. 在Inspector中配置以下设置：
   - **Set Fullscreen On Start**: 勾选（启动时全屏）
   - **Maintain Aspect Ratio**: 勾选（保持宽高比）
   - **Target Aspect Ratio**: 设置为 16:9 或您需要的比例
   - **Main Camera**: 拖入主摄像机
   - **Adjust Camera On Start**: 勾选（启动时调整摄像机）

### 步骤2：使用编辑器工具设置Player Settings

1. 在Unity菜单栏选择 **工具 > 窗口设置助手**
2. 在打开的窗口中配置以下设置：

#### 全屏设置
- **启动时全屏**: 勾选

#### 分辨率设置
- **默认宽度**: 1920
- **默认高度**: 1080
- 点击"添加 1920x1080"等按钮添加支持的分辨率

#### 其他设置
- **允许全屏切换**: 勾选
- **后台运行**: 勾选
- **捕获鼠标**: 根据需要勾选

3. 点击"应用所有设置"按钮

### 步骤3：手动设置Player Settings（推荐）

1. 打开 **Edit > Project Settings > Player**
2. 在 **Resolution and Presentation** 部分设置：

#### Full Screen Mode
- 选择 **FullScreenWindow**

#### Default Screen Width/Height
- 设置为 1920x1080 或您需要的分辨率

#### Supported Aspect Ratios
- 添加 16:9, 4:3 等需要的宽高比

#### Other Settings
- **Allow Fullscreen Switch**: 勾选
- **Run In Background**: 勾选
- **Capture Single Screen**: 根据需要勾选

## 运行时功能

### 键盘快捷键
- **F11**: 切换全屏/窗口模式
- **ESC**: 退出全屏模式

### 自动功能
- 游戏启动时自动设置为全屏
- 自动调整摄像机视口以保持宽高比
- 在不同分辨率下自动缩放

## 摄像机比例缩放原理

### 视口矩形计算
```csharp
// 计算当前宽高比
float currentAspectRatio = Screen.width / Screen.height;
float targetAspect = targetAspectRatio.x / targetAspectRatio.y;

// 计算视口矩形
Rect viewportRect = CalculateViewportRect(currentAspectRatio, targetAspect);

// 设置摄像机视口
mainCamera.rect = viewportRect;
```

### 宽高比适配
- 如果屏幕太宽：缩放宽度，在两侧添加黑边
- 如果屏幕太高：缩放高度，在上下添加黑边
- 始终保持目标宽高比，不会变形

## 常见问题

### Q: 游戏启动时不是全屏？
A: 检查Player Settings中的Full Screen Mode是否设置为FullScreenWindow

### Q: 画面变形了？
A: 确保WindowManager中的Maintain Aspect Ratio已勾选

### Q: 无法切换全屏？
A: 检查Player Settings中的Allow Fullscreen Switch是否勾选

### Q: 在不同分辨率下显示异常？
A: 确保在Supported Aspect Ratios中添加了相应的宽高比

## 高级配置

### 自定义宽高比
```csharp
// 在WindowManager中设置
targetAspectRatio = new Vector2(4, 3); // 4:3比例
targetAspectRatio = new Vector2(21, 9); // 21:9比例
```

### 动态调整
```csharp
// 运行时动态调整
windowManager.SetTargetAspectRatio(16, 10); // 16:10比例
windowManager.AdjustCamera();
```

## 打包注意事项

1. **测试不同分辨率**：在打包前测试不同分辨率的显示效果
2. **宽高比兼容性**：确保支持常见的宽高比（16:9, 4:3, 16:10等）
3. **性能考虑**：全屏模式可能影响性能，注意优化
4. **用户偏好**：提供全屏/窗口切换选项，尊重用户选择

## 技术支持

如果遇到问题，请检查：
1. Console中的错误信息
2. WindowManager的调试日志
3. Player Settings配置
4. 摄像机设置
