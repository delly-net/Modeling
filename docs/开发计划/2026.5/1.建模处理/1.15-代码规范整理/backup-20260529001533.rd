# 1.15 代码规范整理

## 任务信息

- **任务号**: 1.15
- **任务名称**: 代码规范整理
- **责任人**: 郑琳
- **任务类型**: 代码优化
- **任务描述**: 根据 `/docs/开发指南/代码规范.md` 中的规范进行全部代码文件优化

## 功能概述

对项目中所有代码文件进行规范整理，确保代码符合项目定义的编码标准。此任务旨在提高代码质量、可读性和可维护性，为后续开发提供统一的代码风格基础。

**重要约束**：
- 修改范围包括所有 `.cs` 文件（源生成器、Models、接口、演示项目等）
- 不得修改代码逻辑，仅调整代码风格和注释
- 确保所有公共成员都有完整的 XML 文档注释
- 代码修改后必须编译通过且功能正常
- Models/*.cs 文件需要保持条件编译结构

## 技术背景

### 代码规范内容

根据 `/docs/开发指南/代码规范.md`，项目代码需要遵守以下规范：

#### 1. 文档注释规范

**公共成员**：
- 所有公共类、方法、属性和接口必须使用 XML 文档注释
- 使用 `///` 语法
- `param` 名称必须与实际参数名称完全匹配

```csharp
/// <summary>
/// 获取指定属性的值
/// </summary>
/// <param name="model">模型实例</param>
/// <param name="instance">对象实例</param>
/// <param name="name">属性名称</param>
/// <returns>属性值</returns>
public static object? GetProperty(Model model, object instance, string name)
```

**私有成员**：
- 仅使用 `//` 注释（注意空格）

```csharp
// 缓存属性信息
private PropertyInfo[]? _properties;
```

#### 2. 代码风格规范

**if 语句规范**：
- `if` 语句必须使用大括号包裹
- 单行 if 语句：整体少于 120 字符时可合并为一行

```csharp
// 正确 - 单行
if (a) { b(); }

// 正确 - 多行（超过120字符）
if (veryLongConditionName && anotherVeryLongCondition)
{
    ExecuteMethod();
}

// 错误 - 无大括号
if (obj == null)
    return null;
```

#### 3. 可空注解规范

- 接口和公共 API 使用 `object` 格式（兼容 .NET Standard 2.0）
- Models/*.cs 使用条件编译支持不同 .NET 版本

#### 4. 命名约定

- 类名：PascalCase
- 方法名：PascalCase
- 属性名：PascalCase
- 私有字段：_camelCase

#### 5. 源代码生成规范

- 生成代码输出到 `obj/Generated` 目录
- 生成的类使用 `partial` 关键字
- 为生成的扩展方法提供完整的 XML 文档注释
- 源生成器生成的代码使用条件编译支持可空类型

### 现有实现

项目代码文件分布：

```
Delly.Modeling/
├─ Attributes/
├─ Interfaces/
├─ Models/
└─ *.cs (根目录文件)

Delly.Modeling.Generator/
├─ *SourceGenerator.cs
└─ *SyntaxReceiver.cs

Deme/
└─ *.cs (演示类)
```

### 新增需求

对所有代码文件进行规范检查和修复：

1. **公共成员 XML 注释检查**
   - 确保所有公共类有 `/// <summary>` 注释
   - 确保所有公共方法有完整的 `/// <summary>`、`/// <param>`、`/// <returns>` 注释
   - 确保所有公共属性有 `/// <summary>` 注释
   - 验证 `param` 名称与实际参数名称匹配

2. **if 语句规范检查**
   - 所有 if 语句使用大括号
   - 单行 if 语句符合 120 字符限制

3. **私有成员注释检查**
   - 私有成员使用 `//` 注释而非 `///`
   - `//` 后保留一个空格

4. **命名约定检查**
   - 类名、方法名、属性名使用 PascalCase
   - 私有字段使用 _camelCase

5. **Models/*.cs 文件检查**
   - 确认条件编译结构正确
   - 确认可空类型使用正确

## 实现方案

### 1. 按目录顺序检查和修复

按以下顺序处理代码文件：

1. **Delly.Modeling/Interfaces/** - 接口定义
2. **Delly.Modeling/Attributes/** - 特性类
3. **Delly.Modeling/Models/** - 模型类
4. **Delly.Modeling/*.cs** - 根目录文件
5. **Delly.Modeling.Generator/** - 源生成器
6. **Deme/*.cs** - 演示项目

### 2. 检查清单

对于每个文件，检查以下项目：

| 检查项 | 内容 | 修复方式 |
|:------|:-----|:--------|
| 公共类注释 | 缺少 `/// <summary>` | 添加 XML 注释 |
| 公共方法注释 | 缺少完整的 param/returns | 添加 XML 注释 |
| 公共属性注释 | 缺少 `/// <summary>` | 添加 XML 注释 |
| param 匹配 | param 名称与参数名称不匹配 | 修正 param 名称 |
| if 大括号 | if 语句无大括号 | 添加大括号 |
| 单行 if | 单行 if 超过 120 字符 | 拆分为多行 |
| 私有注释 | 私有成员使用 `///` | 改为 `//` |
| 注释空格 | `//` 后无空格 | 添加空格 |
| 命名规范 | 不符合 PascalCase/_camelCase | 修正命名 |

### 3. 示例修复

**修复前**：
```csharp
public interface IModel
{
    object GetProperty(string name);
}

private readonly Dictionary<string, IModel> _models;

if (obj == null)
    return null;

if (veryLongConditionName && anotherVeryLongCondition && moreCondition)
    ExecuteMethod();
```

**修复后**：
```csharp
/// <summary>
/// 模型接口，定义模型的基础功能
/// </summary>
public interface IModel
{
    /// <summary>
    /// 获取指定名称的属性
    /// </summary>
    /// <param name="name">属性名称</param>
    /// <returns>属性实例，未找到时返回 null</returns>
    object GetProperty(string name);
}

// 模型缓存字典
private readonly Dictionary<string, IModel> _models;

if (obj == null)
{
    return null;
}

if (veryLongConditionName && anotherVeryLongCondition && moreCondition)
{
    ExecuteMethod();
}
```

## 使用示例

规范整理后的代码示例：

```csharp
/// <summary>
/// 表示属性的模型接口
/// </summary>
public interface IModelProperty
{
    /// <summary>
    /// 获取属性名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 获取属性值
    /// </summary>
    /// <param name="model">模型实例</param>
    /// <param name="instance">对象实例</param>
    /// <returns>属性值</returns>
    object GetValue(IModel model, object instance);
}

/// <summary>
/// 默认属性模型实现
/// </summary>
public sealed class DefaultModelProperty : IModelProperty
{
    // 属性名称
    private readonly string _name;

    /// <summary>
    /// 初始化属性模型实例
    /// </summary>
    /// <param name="name">属性名称</param>
    public DefaultModelProperty(string name)
    {
        _name = name;
    }

    /// <summary>
    /// 获取属性名称
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// 获取属性值
    /// </summary>
    /// <param name="model">模型实例</param>
    /// <param name="instance">对象实例</param>
    /// <returns>属性值</returns>
    public object GetValue(IModel model, object instance)
    {
        if (instance == null)
        {
            return null;
        }
        return model.GetProperty(_name);
    }
}
```

## 实现步骤

### 开发任务清单

- [ ] Delly.Modeling/Interfaces/
  - [ ] IBaseModel.cs - 检查和修复
  - [ ] IModel.cs - 检查和修复
  - [ ] IModelProperty.cs - 检查和修复
  - [ ] IEntityModel.cs - 检查和修复
  - [ ] IEntityModelProperty.cs - 检查和修复
  - [ ] IEntityModelSet.cs - 检查和修复
  - [ ] IEntityModelFactory.cs - 检查和修复
- [ ] Delly.Modeling/Attributes/
  - [ ] ModelableAttribute.cs - 检查和修复
  - [ ] MoTableAttribute.cs - 检查和修复
  - [ ] MoQueryAttribute.cs - 检查和修复
  - [ ] MoSetAttribute.cs - 检查和修复
  - [ ] MoColumnAttribute.cs - 检查和修复
  - [ ] MoColumnIndexAttribute.cs - 检查和修复
  - [ ] ParsableAttribute.cs - 检查和修复
- [ ] Delly.Modeling/Models/
  - [ ] 检查所有 Model 类的条件编译结构
  - [ ] 确认可空类型使用正确
- [ ] Delly.Modeling 根目录
  - [ ] ColumnType.cs - 检查和修复
  - [ ] EntityModelIndex.cs - 检查和修复
  - [ ] IParsable.cs - 检查和修复
  - [ ] DefaultEntityModelFactory.cs - 检查和修复
  - [ ] ModelUtils.cs - 检查和修复
- [ ] Delly.Modeling.Generator
  - [ ] ModelableSourceGenerator.cs - 检查和修复
  - [ ] ModelableSyntaxReceiver.cs - 检查和修复
  - [ ] ModelTableSourceGenerator.cs - 检查和修复
  - [ ] ModelTableSyntaxReceiver.cs - 检查和修复
  - [ ] ModelSetSourceGenerator.cs - 检查和修复
  - [ ] ModelSetSyntaxReceiver.cs - 检查和修复
- [ ] Deme 演示项目
  - [ ] Program.cs - 检查和修复
  - [ ] 所有演示类 - 检查和修复
- [ ] 验证
  - [ ] 运行 `dotnet build` 确认编译通过
  - [ ] 运行单元测试确认功能正常
  - [ ] 运行 Deme 项目验证功能

## 参考文档

- [代码规范.md](../../开发指南/代码规范.md) - 代码规范参考
- [注意事项.md](../../开发指南/注意事项.md) - 开发注意事项
- [功能清单.md](../功能清单.md) - 任务清单
- [1.15-需求补充.md](./1.15-需求补充.md) - 需求补充文档
- [1.10-编译警告处理](./1.10-编译警告处理.md) - 编译警告处理参考

## 验收标准

1. 所有公共成员都有完整的 XML 文档注释
2. 所有 `param` 名称与实际参数名称匹配
3. 所有 if 语句都使用大括号
4. 单行 if 语句符合 120 字符限制
5. 私有成员使用 `//` 注释且保留空格
6. 类名、方法名、属性名使用 PascalCase
7. 私有字段使用 _camelCase
8. Models/*.cs 保持正确的条件编译结构
9. `dotnet build` 编译通过且无警告
10. 单元测试全部通过
11. Deme 项目正常运行
