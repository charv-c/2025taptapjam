# 米字格图片映射系统

## 概述

本系统为左右两个米字格提供了独立的图片映射字典，允许为不同的米字格设置不同的图片样式。

## 功能特性

- **两种米字格类型**：左米字格、右米字格
- **独立图片映射**：每种类型都有独立的图片映射字典
- **灵活配置**：可以在Inspector中直接配置图片映射
- **运行时支持**：支持运行时动态设置和查询图片映射

## 使用方法

### 1. 在PublicData中配置图片映射

在 `PublicData` 组件的Inspector中，您会看到两个图片映射列表：

- **左米字格图片映射**：左米字格的图片映射
- **右米字格图片映射**：右米字格的图片映射

每个列表都可以添加 `CharacterSpriteMapping` 条目，包含：
- `character`：字符名称（如"人"、"木"、"火"等）
- `sprite`：对应的Sprite图片

### 2. 设置MiSquareController的米字格类型

在 `MiSquareController` 组件中，设置 `MiZiGeType` 字段：

- `Left`：使用左米字格图片映射
- `Right`：使用右米字格图片映射

### 3. 代码中使用

```csharp
// 获取左米字格图片
Sprite leftSprite = PublicData.GetLeftMiZiGeSprite("人");

// 获取右米字格图片
Sprite rightSprite = PublicData.GetRightMiZiGeSprite("人");

// 检查是否有对应的图片
bool hasLeft = PublicData.HasLeftMiZiGeSprite("人");
bool hasRight = PublicData.HasRightMiZiGeSprite("人");

// 获取所有可用的字符
List<string> leftChars = PublicData.GetAllLeftMiZiGeCharacters();
List<string> rightChars = PublicData.GetAllRightMiZiGeCharacters();
```

### 4. 在MiSquareController中使用

```csharp
// 设置米字格类型
miSquareController.SetMiZiGeType(MiSquareController.MiZiGeType.Left);

// 设置图片（会根据类型自动选择对应的映射）
miSquareController.SetMiSquareSprite("人");

// 检查是否有对应的图片
bool hasSprite = miSquareController.HasMiZiGeSprite("人");
```

## 示例脚本

使用 `MiZiGeExample` 脚本来测试和演示功能：

1. 将脚本添加到场景中的GameObject上
2. 在Inspector中设置左右米字格控制器引用
3. 运行游戏或使用ContextMenu测试功能

### 可用的测试方法

- **测试米字格图片设置**：测试基本的图片设置功能
- **测试不同字符**：检查各种字符的图片映射情况
- **列出所有可用字符**：显示所有配置的字符

## 批量设置

如果需要运行时动态设置图片映射，可以使用批量设置方法：

```csharp
// 批量设置左米字格图片映射
Dictionary<string, Sprite> leftMappings = new Dictionary<string, Sprite>();
leftMappings["人"] = humanSprite;
leftMappings["木"] = woodSprite;
PublicData.SetLeftMiZiGeSpriteMappings(leftMappings);

// 批量设置右米字格图片映射
Dictionary<string, Sprite> rightMappings = new Dictionary<string, Sprite>();
rightMappings["人"] = humanSpriteAlt;
rightMappings["木"] = woodSpriteAlt;
PublicData.SetRightMiZiGeSpriteMappings(rightMappings);
```

## 注意事项

1. **字符名称一致性**：确保在代码中使用的字符名称与配置中的完全一致
2. **图片资源**：确保所有引用的Sprite资源都存在且正确
3. **初始化顺序**：PublicData的Awake方法会在游戏开始时自动初始化所有图片映射
4. **性能考虑**：图片映射使用字典存储，查询效率较高

## 扩展功能

如果需要添加更多米字格类型，可以：

1. 在 `MiZiGeType` 枚举中添加新类型
2. 在 `PublicData` 中添加对应的映射字典和方法
3. 在 `MiSquareController` 中添加对应的处理逻辑

## 故障排除

### 常见问题

1. **图片不显示**：检查字符名称是否正确，图片资源是否存在
2. **类型不匹配**：确保MiSquareController的MiZiGeType设置正确
3. **映射为空**：检查PublicData中的图片映射配置

### 调试方法

1. 使用 `MiZiGeExample` 脚本进行测试
2. 查看Console中的调试日志
3. 使用 `PublicData` 的查询方法检查映射状态
