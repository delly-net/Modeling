# zl.1.17 泛型建模支持

## 任务信息

- **任务号**: zl.1.17
- **任务名称**: 泛型建模支持
- **责任人**: 郑琳
- **任务类型**: 功能开发
- **任务描述**: 在 `IBaseModel` 提供 `bool IsGenericModel`属性、`IBaseModel GetGenericModelDefinition()`函数 和 `IReadOnlyList<IBaseModel> GetGenericModels()` 函数

## 功能概述

在 `IBaseModel` 接口中添加泛型建模相关支持，包括：
1. **IsGenericModel 属性**：标识当前模型是否为泛型模型
2. **GetGenericModelDefinition 函数**：获取泛型模型的定义模型（如 `List<T>` 的定义模型）
3. **GetGenericModels 函数**：获取已构造的泛型模型列表（如 `List<int>`、`List<string>` 等）

此功能支持建模库在处理泛型类型时提供完整的类型信息，便于源生成器和运行时进行类型推断和处理。

**重要约束**：
- 不允许使用反射方式调用
- 接口定义使用非空类型 `object`，通过条件编译支持不同 .NET 版本
- 泛型定义模型的 `ClassType` 应为开放泛型类型（如 `List<>`）
- 已构造模型的 `ClassType` 应为具体类型（如 `List<int>`）

## 技术背景

### 现有实现

当前 `IBaseModel` 接口定义如下：

```csharp
#if !NETSTANDARD2_0
#nullable enable
#endif

using System;
using System.Collections.Generic;

namespace Delly.Modeling
{
    /// <summary>
    /// 模型对象接口，提供属性访问能力
    /// </summary>
    public interface IBaseModel
    {
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 类型全名称
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// 创建模型类型的新实例
        /// </summary>
        /// <param name="args">构造函数参数数组</param>
        /// <returns>模型类型的新实例</returns>
        object CreateInstance(params object[] args);

        /// <summary>
        /// 模型类型信息，源生成阶段使用 typeof(T) 赋值
        /// </summary>
        Type ClassType { get; }

        /// <summary>
        /// 将输入对象解析为目标类型实例
        /// </summary>
        /// <param name="obj">输入对象</param>
        /// <returns>目标类型实例</returns>
        /// <exception cref="ArgumentException">当对象无法转换为目标类型时抛出</exception>
#if NETSTANDARD2_0
        object Parse(object obj);
#else
        object Parse(object? obj);
#endif

        /// <summary>
        /// 尝试将输入对象解析为目标类型实例
        /// </summary>
        /// <param name="obj">输入对象</param>
        /// <returns>目标类型实例，解析失败时返回 null</returns>
#if NETSTANDARD2_0
        object TryParse(object obj);
#else
        object? TryParse(object? obj);
#endif

        /// <summary>
        /// 是否为值类型对象，基础值类型和 string 为 true，其他 class 类型为 false
        /// </summary>
        bool IsValue { get; }
    }
}
```

### 新增需求

1. **添加 IsGenericModel 属性**：标识当前模型是否为泛型模型
2. **添加 GetGenericModelDefinition 函数**：对于已构造的泛型模型，返回其泛型定义模型
3. **添加 GetGenericModels 函数**：对于泛型定义模型，返回所有已构造的泛型模型

### 使用场景示例

```csharp
// 场景1：判断是否为泛型模型
var listIntModel = Model.Get<List<int>>();
Console.WriteLine(listIntModel.IsGenericModel);  // true

var intModel = Int32Model.Instance;
Console.WriteLine(intModel.IsGenericModel);  // false

// 场景2：获取泛型定义模型
var definition = listIntModel.GetGenericModelDefinition();
Console.WriteLine(definition.ClassType);  // List<> (GenericType)

// 场景3：获取所有已构造的泛型模型
var genericModels = definition.GetGenericModels();
foreach (var model in genericModels)
{
    Console.WriteLine(model.ClassType);  // List<int>, List<string>, etc.
}
```

## 实现方案

### 1. 修改 IBaseModel 接口

添加以下成员：

```csharp
/// <summary>
/// 是否为泛型模型
/// </summary>
bool IsGenericModel { get; }

/// <summary>
/// 获取泛型模型的定义模型
/// </summary>
/// <returns>泛型定义模型，对于非泛型模型返回自身</returns>
IBaseModel GetGenericModelDefinition();

/// <summary>
/// 获取所有已构造的泛型模型
/// </summary>
/// <returns>已构造的泛型模型列表</returns>
IReadOnlyList<IBaseModel> GetGenericModels();
```

### 2. 基础 Model 对象实现

对于非泛型的基础 Model 对象：

```csharp
public sealed class Int32Model : IBaseModel
{
    // ... 现有代码 ...

    public bool IsGenericModel => false;

    public IBaseModel GetGenericModelDefinition()
    {
        return this;
    }

    public IReadOnlyList<IBaseModel> GetGenericModels()
    {
        return Array.Empty<IBaseModel>();
    }
}
```

### 3. 源生成器实现

需要在以下源生成器中添加相关代码：

| 源生成器 | 特性 | 生成的类 |
|:--------|:----|:--------|
| `ModelableSourceGenerator` | `ModelableAttribute` | `emc__{ClassName}` |
| `ModelTableSourceGenerator` | `MoTableAttribute` | `emc__{ClassName}` |
| `ModelTableSourceGenerator` | `MoQueryAttribute` | `emc__{ClassName}` |

**生成的泛型模型代码**：

```csharp
#if !NETSTANDARD2_0
#nullable enable
#endif

public partial class emc__ListInt : IBaseModel
{
    private static readonly Type _genericDefinitionType = typeof(List<>);
    private static IBaseModel? _genericDefinition;

    // ... 现有代码 ...

    /// <summary>
    /// 是否为泛型模型
    /// </summary>
    public bool IsGenericModel => true;

    /// <summary>
    /// 获取泛型模型的定义模型
    /// </summary>
    /// <returns>泛型定义模型</returns>
    public IBaseModel GetGenericModelDefinition()
    {
        if (_genericDefinition == null)
        {
            // 通过工厂获取泛型定义模型
            _genericDefinition = ModelFactory.GetModel(_genericDefinitionType);
        }
        return _genericDefinition;
    }

    /// <summary>
    /// 获取所有已构造的泛型模型
    /// </summary>
    /// <returns>已构造的泛型模型列表</returns>
    public IReadOnlyList<IBaseModel> GetGenericModels()
    {
        return Array.Empty<IBaseModel>();
    }
}
```

**生成的泛型定义模型代码**：

```csharp
#if !NETSTANDARD2_0
#nullable enable
#endif

public partial class emc__ListGenericDefinition : IBaseModel
{
    private static readonly IBaseModel[] _genericModels = new IBaseModel[]
    {
        ListInt.GetEntityModel(),
        ListString.GetEntityModel(),
        // ... 其他已构造的泛型模型 ...
    };

    // ... 现有代码 ...

    /// <summary>
    /// 是否为泛型模型
    /// </summary>
    public bool IsGenericModel => false;

    /// <summary>
    /// 获取泛型模型的定义模型
    /// </summary>
    /// <returns>泛型定义模型（自身）</returns>
    public IBaseModel GetGenericModelDefinition()
    {
        return this;
    }

    /// <summary>
    /// 获取所有已构造的泛型模型
    /// </summary>
    /// <returns>已构造的泛型模型列表</returns>
    public IReadOnlyList<IBaseModel> GetGenericModels()
    {
        return _genericModels;
    }
}
```

### 4. TODO: 待确认项

1. **泛型定义模型与已构造模型的关系管理方式**：
   - 当前方案：通过静态数组维护已构造模型列表
   - 是否需要支持动态注册？
     - 建议：使用静态数组，源生成时生成所有已构造模型

2. **泛型模型在工厂中的注册方式**：
   - 当前方案：源生成器自动注册所有泛型模型
   - 是否需要支持手动注册？
     - 建议：自动注册，减少用户手动操作

3. **开放泛型类型的 ClassType 表示方式**：
   - 当前方案：使用 `typeof(List<>)`
   - 验证：`typeof(List<>)` 是否在 AOT 场景下可用？
     - 建议：验证 AOT 兼容性

## 使用示例

```csharp
// 示例1：检查是否为泛型模型
var listIntModel = Model.Get<List<int>>();
if (listIntModel.IsGenericModel)
{
    Console.WriteLine("List<int> 是一个泛型模型");
}

// 示例2：获取泛型定义
var definition = listIntModel.GetGenericModelDefinition();
Console.WriteLine($"泛型定义: {definition.ClassType}");  // List`1

// 示例3：获取所有已构造的泛型模型
var genericModels = definition.GetGenericModels();
Console.WriteLine($"已构造的泛型模型数量: {genericModels.Count}");
foreach (var model in genericModels)
{
    Console.WriteLine($"  - {model.ClassType}");
}

// 示例4：非泛型模型的行为
var intModel = Int32Model.Instance;
Console.WriteLine(intModel.IsGenericModel);  // false
Console.WriteLine(intModel.GetGenericModelDefinition() == intModel);  // true
Console.WriteLine(intModel.GetGenericModels().Count);  // 0
```

## 实现步骤

### 开发任务清单

- [ ] 在 `IBaseModel` 接口中添加 `IsGenericModel` 属性定义
- [ ] 在 `IBaseModel` 接口中添加 `GetGenericModelDefinition()` 函数定义
- [ ] 在 `IBaseModel` 接口中添加 `GetGenericModels()` 函数定义
- [ ] 为所有基础 Model 对象添加泛型支持实现
  - [ ] StringModel
  - [ ] Int32Model
  - [ ] Int64Model
  - [ ] BooleanModel
  - [ ] DecimalModel
  - [ ] DoubleModel
  - [ ] DateTimeModel
  - [ ] GuidModel
  - [ ] EntityModel 类型（各实体模型）
- [ ] 在 `ModelableSourceGenerator` 中添加泛型检测逻辑
- [ ] 在 `ModelableSourceGenerator` 中生成泛型相关代码
- [ ] 在 `ModelTableSourceGenerator` 中添加泛型检测逻辑（MoTable）
- [ ] 在 `ModelTableSourceGenerator` 中生成泛型相关代码（MoTable）
- [ ] 在 `ModelTableSourceGenerator` 中添加泛型检测逻辑（MoQuery）
- [ ] 在 `ModelTableSourceGenerator` 中生成泛型相关代码（MoQuery）
- [ ] 添加 XML 文档注释
- [ ] 更新相关单元测试
- [ ] 在 Deme 项目中添加示例代码
- [ ] 验证 AOT 编译通过
- [ ] 验证 .NET Standard 2.0 兼容性
- [ ] 验证 .NET 6.0 兼容性

## 参考文档

- [IBaseModel.cs](../../../Delly.Modeling/IBaseModel.cs) - 需要修改的接口
- [代码规范.md](../../../docs/开发指南/代码规范.md) - 代码规范参考
- [注意事项.md](../../../docs/开发指南/注意事项.md) - 开发注意事项
- [需求补充.md](./zl.1.17-需求补充.md) - 需求补充文档

## 验收标准

1. `IBaseModel` 接口包含 `IsGenericModel` 属性定义
2. `IBaseModel` 接口包含 `GetGenericModelDefinition()` 函数定义
3. `IBaseModel` 接口包含 `GetGenericModels()` 函数定义
4. 所有基础 Model 对象都已实现泛型支持方法
5. 非泛型模型的 `IsGenericModel` 返回 `false`
6. 非泛型模型的 `GetGenericModelDefinition()` 返回自身
7. 非泛型模型的 `GetGenericModels()` 返回空列表
8. 泛型模型的 `IsGenericModel` 返回 `true`
9. 泛型模型的 `GetGenericModelDefinition()` 返回泛型定义模型
10. 泛型定义模型的 `GetGenericModels()` 返回所有已构造的泛型模型
11. 源生成器代码中不包含任何反射调用
12. XML 文档注释完整
13. 单元测试通过
14. Deme 项目中的示例能正常运行
15. AOT 编译通过
16. .NET Standard 2.0 兼容性验证通过
17. .NET 6.0 兼容性验证通过
