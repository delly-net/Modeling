# zl.1.18 泛型实体建模支持 - 备份

**备份时间**: 2026-05-29 15:25:05
**原始文件**: [zl.1.18-泛型实体建模支持.md](./zl.1.18-泛型实体建模支持.md)

---

# zl.1.18 泛型实体建模支持

## 任务信息

- **任务号**: zl.1.18
- **任务名称**: 泛型实体建模支持
- **责任人**: 郑琳
- **任务类型**: 功能开发
- **任务描述**: 在 `IEntityModel` 提供泛型操作函数，支持泛型实体模型的定义和构造

## 功能概述

在实体建模系统中添加泛型实体建模支持，包括：

1. **GetGenericModelDefinition 函数**：获取泛型实体模型的定义模型（定义在 `IEntityModel`）
2. **GetGenericModels 函数**：获取所有已构造的泛型实体模型列表（定义在 `IEntityModel`）
3. **MakeGenericModel 函数**：根据泛型参数创建已构造的泛型实体模型（定义在 `IEntityModel`）

此功能是 zl.1.17 泛型建模支持在实体模型层面的扩展，专门针对 `[MoTable]` 和 `[MoQuery]` 标记的实体类提供泛型建模能力。

**重要约束**：
- 不允许使用反射方式调用
- 接口定义使用非空类型 `object`，通过条件编译支持不同 .NET 版本
- 泛型定义模型的 `ClassType` 应为开放泛型类型（如 `typeof(TableEntity<>)`）
- 已构造模型的 `ClassType` 应为具体类型（如 `typeof(TableEntity<int>)`）
- 泛型实体建模支持源生成，使用预定义实例模式
- 本任务中的函数定义在 `IEntityModel` 接口（而非 `IModel`）
- 与 zl.1.17 的区别：本任务专门针对实体模型（`IEntityModel`），返回类型也是 `IEntityModel`

## 技术背景

### 现有实现

当前 `IEntityModel` 接口定义如下：

```csharp
#if !NETSTANDARD2_0
#nullable enable
#endif

using System;
using System.Collections.Generic;

namespace Delly.Modeling
{
    /// <summary>
    /// 实体模型
    /// </summary>
    public interface IEntityModel : IBaseModel
    {
        /// <summary>
        /// 类名
        /// </summary>
        string ClassName { get; }

        /// <summary>
        /// 获取所有建模属性
        /// </summary>
        /// <returns>属性对象数组</returns>
        IEntityModelProperty[] GetProperties();

        /// <summary>
        /// 获取所有索引定义
        /// </summary>
        /// <returns>属性对象数组</returns>
        EntityModelIndex[] GetIndexes();

        /// <summary>
        /// 获取建模属性
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <returns>属性对象</returns>
#if NETSTANDARD2_0
        IEntityModelProperty GetProperty(string name);
#else
        IEntityModelProperty? GetProperty(string name);
#endif
    }
}
```

### 新增需求

1. 在 `IEntityModel` 接口中添加 `GetGenericModelDefinition()` 函数定义
2. 在 `IEntityModel` 接口中添加 `GetGenericModels()` 函数定义
3. 在 `IEntityModel` 接口中添加 `MakeGenericModel()` 函数定义
4. 源生成器为生成的实体模型类添加泛型相关方法支持

### 与 zl.1.17 的关系

| 特性 | zl.1.17 泛型建模支持 | zl.1.18 泛型实体建模支持 |
|:----|:-------------------|:----------------------|
| 接口 | `IModel` | `IEntityModel` |
| 返回类型 | `IModel` | `IEntityModel` |
| 适用范围 | 所有标记 `[Modelable]` 的类 | 标记 `[MoTable]` 或 `[MoQuery]` 的实体类 |
| 属性支持 | `IsGenericModel` 等属性在 `IBaseModel` | 继承自 `IBaseModel` |
| 源生成器 | `ModelableSourceGenerator` | `ModelTableSourceGenerator` |

### 使用场景示例

```csharp
// 场景1：定义泛型实体基类
[MoTable]
public partial class TableEntity<T>
{
    [MoColumn]
    public int Id { get; set; }
}

// 场景2：获取泛型定义模型
var tableIntModel = TableEntity<int>.GetEntityModel();
var definition = tableIntModel.GetGenericModelDefinition();
Console.WriteLine(definition.ClassType);  // TableEntity`1 (GenericType)

// 场景3：获取所有已构造的泛型实体模型
var genericModels = definition.GetGenericModels();
foreach (var model in genericModels)
{
    Console.WriteLine(model.ClassType);  // TableEntity<int>, TableEntity<string>, etc.
}

// 场景4：使用 MakeGenericModel 创建泛型实体模型
var tableStringModel = definition.MakeGenericModel(StringModel.Instance);
```

## 实现方案

### 1. 修改 IEntityModel 接口

添加以下成员：

```csharp
/// <summary>
/// 获取泛型实体模型的定义模型
/// </summary>
/// <returns>泛型实体定义模型，对于非泛型实体模型返回自身</returns>
IEntityModel GetGenericModelDefinition();

/// <summary>
/// 获取所有已构造的泛型实体模型
/// </summary>
/// <returns>已构造的泛型实体模型列表</returns>
IReadOnlyList<IEntityModel> GetGenericModels();

/// <summary>
/// 根据泛型参数创建已构造的泛型实体模型
/// </summary>
/// <param name="models">泛型参数对应的模型列表</param>
/// <returns>已构造的泛型实体模型</returns>
IEntityModel MakeGenericModel(params IEntityModel[] models);
```

### 2. 非泛型实体模型实现

对于非泛型的实体模型（如 `UserEntity`、`UserQuery` 等）：

```csharp
public partial class emc__UserEntity : IEntityModel
{
    // ... 现有代码 ...

    /// <summary>
    /// 获取泛型实体模型的定义模型
    /// </summary>
    /// <returns>自身</returns>
    public IEntityModel GetGenericModelDefinition()
    {
        return this;
    }

    /// <summary>
    /// 获取所有已构造的泛型实体模型
    /// </summary>
    /// <returns>空列表</returns>
    public IReadOnlyList<IEntityModel> GetGenericModels()
    {
        return Array.Empty<IEntityModel>();
    }

    /// <summary>
    /// 根据泛型参数创建已构造的泛型实体模型
    /// </summary>
    /// <param name="models">泛型参数对应的模型列表</param>
    /// <returns>已构造的泛型实体模型</returns>
    /// <exception cref="NotSupportedException">UserEntity 不支持泛型实体建模创建</exception>
    public IEntityModel MakeGenericModel(params IEntityModel[] models)
    {
        throw new NotSupportedException("UserEntity 不支持泛型实体建模创建");
    }
}
```

### 3. 泛型实体模型实现

对于泛型实体模型（如 `TableEntity<T>`），源生成器生成的代码：

```csharp
public partial class emc__TableEntity<T> : IEntityModel
{
    // ... 现有代码 ...

    /// <summary>
    /// 获取泛型实体模型的定义模型
    /// </summary>
    /// <returns>泛型实体定义模型</returns>
    public IEntityModel GetGenericModelDefinition()
    {
        // 泛型定义模型由源生成器自动创建
        return TableEntityGenericModel.Instance;
    }

    /// <summary>
    /// 获取所有已构造的泛型实体模型
    /// </summary>
    /// <returns>空列表</returns>
    public IReadOnlyList<IEntityModel> GetGenericModels()
    {
        return Array.Empty<IEntityModel>();
    }

    /// <summary>
    /// 根据泛型参数创建已构造的泛型实体模型
    /// </summary>
    /// <param name="models">泛型参数对应的模型列表</param>
    /// <returns>已构造的泛型实体模型</returns>
    /// <exception cref="NotSupportedException">已构造的泛型实体模型不支持再次构造</exception>
    public IEntityModel MakeGenericModel(params IEntityModel[] models)
    {
        throw new NotSupportedException("已构造的泛型实体模型不支持再次构造");
    }
}
```

### 4. 泛型实体定义模型实现

源生成器自动创建的泛型定义模型类：

```csharp
#if !NETSTANDARD2_0
#nullable enable
#endif

using System;
using System.Collections.Generic;

namespace Delly.Modeling
{
    /// <summary>
    /// TableEntity&lt;T&gt; 泛型实体定义模型
    /// </summary>
    public sealed class TableEntityGenericModel : IEntityModel
    {
        private static readonly TableEntityGenericModel _instance = new TableEntityGenericModel();
        private static readonly List<IEntityModel> _genericModels = new List<IEntityModel>();

        /// <summary>
        /// 获取 TableEntityGenericModel 单例实例
        /// </summary>
        public static TableEntityGenericModel Instance => _instance;

        private TableEntityGenericModel()
        {
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => typeof(TableEntity<>).Name;

        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName => typeof(TableEntity<>).Name;

        /// <summary>
        /// 类型全名称
        /// </summary>
        public string Namespace => "MyApp.Models";

        /// <summary>
        /// 创建模型类型的新实例
        /// </summary>
        /// <param name="args">构造函数参数数组</param>
        /// <returns>模型类型的新实例</returns>
        public object CreateInstance(params object[] args)
        {
            throw new ArgumentException("TableEntityGenericModel.CreateInstance 需要使用 MakeGenericModel 创建");
        }

        /// <summary>
        /// 模型类型信息
        /// </summary>
        public Type ClassType => typeof(TableEntity<>);

        /// <summary>
        /// 将输入对象解析为目标类型实例
        /// </summary>
        /// <param name="obj">输入对象</param>
        /// <returns>目标类型实例</returns>
#if NETSTANDARD2_0
        public object Parse(object obj)
#else
        public object Parse(object? obj)
#endif
        {
            throw new NotSupportedException("TableEntityGenericModel 不支持泛型解析");
        }

        /// <summary>
        /// 尝试将输入对象解析为目标类型实例
        /// </summary>
        /// <param name="obj">输入对象</param>
        /// <returns>目标类型实例，解析失败时返回 null</returns>
#if NETSTANDARD2_0
        public object TryParse(object obj)
#else
        public object? TryParse(object? obj)
#endif
        {
            return null;
        }

        /// <summary>
        /// 是否为值类型对象
        /// </summary>
        public bool IsValue => false;

        /// <summary>
        /// 是否为泛型模型
        /// </summary>
        public bool IsGenericModel => true;

        /// <summary>
        /// 是否为开放泛型定义
        /// </summary>
        public bool IsGenericDefinition => true;

        /// <summary>
        /// 泛型定义数量
        /// </summary>
        public int GenericDefinitionCount => 1;

        /// <summary>
        /// 获取所有建模属性
        /// </summary>
        /// <returns>属性对象数组</returns>
        public IEntityModelProperty[] GetProperties()
        {
            return Array.Empty<IEntityModelProperty>();
        }

        /// <summary>
        /// 获取所有索引定义
        /// </summary>
        /// <returns>索引定义数组</returns>
        public EntityModelIndex[] GetIndexes()
        {
            return Array.Empty<EntityModelIndex>();
        }

        /// <summary>
        /// 获取建模属性
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <returns>属性对象</returns>
#if NETSTANDARD2_0
        public IEntityModelProperty GetProperty(string name)
#else
        public IEntityModelProperty? GetProperty(string name)
#endif
        {
            return null;
        }

        /// <summary>
        /// 获取泛型实体模型的定义模型
        /// </summary>
        /// <returns>自身</returns>
        public IEntityModel GetGenericModelDefinition()
        {
            return this;
        }

        /// <summary>
        /// 获取所有已构造的泛型实体模型
        /// </summary>
        /// <returns>已构造的泛型实体模型列表</returns>
        public IReadOnlyList<IEntityModel> GetGenericModels()
        {
            return _genericModels;
        }

        /// <summary>
        /// 根据泛型参数创建已构造的泛型实体模型
        /// </summary>
        /// <param name="models">泛型参数对应的模型列表</param>
        /// <returns>已构造的泛型实体模型</returns>
        public IEntityModel MakeGenericModel(params IEntityModel[] models)
        {
            if (models.Length != 1)
            {
                throw new ArgumentException("TableEntity<T> 需要一个泛型参数");
            }

            var elementType = models[0].ClassType;
            var entityType = typeof(TableEntity<>).MakeGenericType(elementType);
            var entityModel = new ConstructedGenericEntityModel(entityType, this, models);
            _genericModels.Add(entityModel);
            return entityModel;
        }
    }
}
```

### 5. 已构造泛型实体模型实现

源生成器自动创建的已构造泛型实体模型类：

```csharp
#if !NETSTANDARD2_0
#nullable enable
#endif

using System;
using System.Collections.Generic;

namespace Delly.Modeling
{
    /// <summary>
    /// 已构造的泛型实体模型
    /// </summary>
    public sealed class ConstructedGenericEntityModel : IEntityModel
    {
        private readonly Type _constructedType;
        private readonly IEntityModel _definition;
        private readonly IEntityModel[] _genericArguments;

        /// <summary>
        /// 创建已构造的泛型实体模型
        /// </summary>
        /// <param name="constructedType">已构造的类型</param>
        /// <param name="definition">泛型定义模型</param>
        /// <param name="genericArguments">泛型参数模型列表</param>
        public ConstructedGenericEntityModel(Type constructedType, IEntityModel definition, IEntityModel[] genericArguments)
        {
            _constructedType = constructedType;
            _definition = definition;
            _genericArguments = genericArguments;
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => _constructedType.Name;

        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName => _constructedType.Name;

        /// <summary>
        /// 类型全名称
        /// </summary>
        public string Namespace => _constructedType.Namespace ?? string.Empty;

        /// <summary>
        /// 创建模型类型的新实例
        /// </summary>
        /// <param name="args">构造函数参数数组</param>
        /// <returns>模型类型的新实例</returns>
        public object CreateInstance(params object[] args)
        {
            return Activator.CreateInstance(_constructedType, args);
        }

        /// <summary>
        /// 模型类型信息
        /// </summary>
        public Type ClassType => _constructedType;

        /// <summary>
        /// 将输入对象解析为目标类型实例
        /// </summary>
        /// <param name="obj">输入对象</param>
        /// <returns>目标类型实例</returns>
#if NETSTANDARD2_0
        public object Parse(object obj)
#else
        public object Parse(object? obj)
#endif
        {
            throw new NotSupportedException("ConstructedGenericEntityModel 不支持泛型解析");
        }

        /// <summary>
        /// 尝试将输入对象解析为目标类型实例
        /// </summary>
        /// <param name="obj">输入对象</param>
        /// <returns>目标类型实例，解析失败时返回 null</returns>
#if NETSTANDARD2_0
        public object TryParse(object obj)
#else
        public object? TryParse(object? obj)
#endif
        {
            return null;
        }

        /// <summary>
        /// 是否为值类型对象
        /// </summary>
        public bool IsValue => false;

        /// <summary>
        /// 是否为泛型模型
        /// </summary>
        public bool IsGenericModel => true;

        /// <summary>
        /// 是否为开放泛型定义
        /// </summary>
        public bool IsGenericDefinition => false;

        /// <summary>
        /// 泛型定义数量
        /// </summary>
        public int GenericDefinitionCount => _definition.GenericDefinitionCount;

        /// <summary>
        /// 获取所有建模属性
        /// </summary>
        /// <returns>属性对象数组</returns>
        public IEntityModelProperty[] GetProperties()
        {
            return Array.Empty<IEntityModelProperty>();
        }

        /// <summary>
        /// 获取所有索引定义
        /// </summary>
        /// <returns>索引定义数组</returns>
        public EntityModelIndex[] GetIndexes()
        {
            return Array.Empty<EntityModelIndex>();
        }

        /// <summary>
        /// 获取建模属性
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <returns>属性对象</returns>
#if NETSTANDARD2_0
        public IEntityModelProperty GetProperty(string name)
#else
        public IEntityModelProperty? GetProperty(string name)
#endif
        {
            return null;
        }

        /// <summary>
        /// 获取泛型实体模型的定义模型
        /// </summary>
        /// <returns>泛型实体定义模型</returns>
        public IEntityModel GetGenericModelDefinition()
        {
            return _definition;
        }

        /// <summary>
        /// 获取所有已构造的泛型实体模型
        /// </summary>
        /// <returns>已构造的泛型实体模型列表</returns>
        public IReadOnlyList<IEntityModel> GetGenericModels()
        {
            return Array.Empty<IEntityModel>();
        }

        /// <summary>
        /// 根据泛型参数创建已构造的泛型实体模型
        /// </summary>
        /// <param name="models">泛型参数对应的模型列表</param>
        /// <returns>已构造的泛型实体模型</returns>
        /// <exception cref="NotSupportedException">已构造的泛型实体模型不支持再次构造</exception>
        public IEntityModel MakeGenericModel(params IEntityModel[] models)
        {
            throw new NotSupportedException("已构造的泛型实体模型不支持再次构造");
        }
    }
}
```

### 6. 源生成器实现

在 `ModelTableSourceGenerator` 中添加泛型实体检测和生成逻辑：

**检测逻辑**：

```csharp
// 检查类是否为泛型
bool isGeneric = classSymbol.IsGenericType;

// 检查是否为泛型定义
bool isGenericDefinition = classSymbol.IsGenericType && classSymbol.IsUnboundGenericType;

// 获取泛型参数数量
int genericParameterCount = isGeneric ? classSymbol.TypeParameters.Length : 0;
```

**生成的代码**（泛型定义）：

当检测到泛型定义（如 `TableEntity<T>`）时，源生成器应生成：

1. 泛型定义模型类（如 `TableEntityGenericModel`）
2. 已构造泛型实体模型类（如 `ConstructedGenericEntityModel`）
3. 为原始泛型类生成的 `emc__TableEntity<T>` 类添加泛型支持方法

**生成的代码**（非泛型类）：

对于非泛型实体类，生成的 `emc__{ClassName}` 类添加：

```csharp
/// <summary>
/// 获取泛型实体模型的定义模型
/// </summary>
/// <returns>自身</returns>
public IEntityModel GetGenericModelDefinition()
{
    return this;
}

/// <summary>
/// 获取所有已构造的泛型实体模型
/// </summary>
/// <returns>空列表</returns>
public IReadOnlyList<IEntityModel> GetGenericModels()
{
    return Array.Empty<IEntityModel>();
}

/// <summary>
/// 根据泛型参数创建已构造的泛型实体模型
/// </summary>
/// <param name="models">泛型参数对应的模型列表</param>
/// <returns>已构造的泛型实体模型</returns>
/// <exception cref="NotSupportedException">{ClassName} 不支持泛型实体建模创建</exception>
public IEntityModel MakeGenericModel(params IEntityModel[] models)
{
    throw new NotSupportedException("{ClassName} 不支持泛型实体建模创建");
}
```

## 使用示例

```csharp
// 示例1：定义泛型实体类
[MoTable]
public partial class TableEntity<T>
{
    [MoColumn]
    public int Id { get; set; }

    [MoColumn]
    public T? Value { get; set; }
}

// 示例2：使用已构造的泛型实体模型
var tableIntModel = TableEntity<int>.GetEntityModel();
Console.WriteLine($"是否为泛型模型: {tableIntModel.IsGenericModel}");          // true
Console.WriteLine($"是否为开放泛型定义: {tableIntModel.IsGenericDefinition}");    // false
Console.WriteLine($"泛型定义数量: {tableIntModel.GenericDefinitionCount}");       // 1

// 示例3：获取泛型定义模型
var definition = tableIntModel.GetGenericModelDefinition();
Console.WriteLine($"泛型定义: {definition.ClassType}");  // TableEntity`1
Console.WriteLine($"是否为开放泛型定义: {definition.IsGenericDefinition}");  // true

// 示例4：获取所有已构造的泛型实体模型
var genericModels = definition.GetGenericModels();
Console.WriteLine($"已构造的泛型实体模型数量: {genericModels.Count}");

// 示例5：使用 MakeGenericModel 创建泛型实体模型
var tableStringModel = definition.MakeGenericModel(StringModel.Instance);
Console.WriteLine($"TableEntity<string>: {tableStringModel.ClassType}");

// 示例6：MakeGenericModel 参数校验
try
{
    // TableEntity<T> 只需要一个泛型参数
    definition.MakeGenericModel(Int32Model.Instance, StringModel.Instance);
}
catch (ArgumentException ex)
{
    Console.WriteLine(ex.Message);  // TableEntity<T> 需要一个泛型参数
}
```

## 实现步骤

### 开发任务清单

- [ ] 在 `IEntityModel` 接口中添加 `GetGenericModelDefinition()` 函数定义
- [ ] 在 `IEntityModel` 接口中添加 `GetGenericModels()` 函数定义
- [ ] 在 `IEntityModel` 接口中添加 `MakeGenericModel()` 函数定义
- [ ] 创建 `ConstructedGenericEntityModel` 类
- [ ] 在 `ModelTableSourceGenerator` 中添加泛型检测逻辑（MoTable）
- [ ] 在 `ModelTableSourceGenerator` 中生成泛型定义模型代码（MoTable）
- [ ] 在 `ModelTableSourceGenerator` 中生成泛型相关方法代码（MoTable）
- [ ] 在 `ModelTableSourceGenerator` 中添加泛型检测逻辑（MoQuery）
- [ ] 在 `ModelTableSourceGenerator` 中生成泛型定义模型代码（MoQuery）
- [ ] 在 `ModelTableSourceGenerator` 中生成泛型相关方法代码（MoQuery）
- [ ] 为所有非泛型实体模型添加泛型支持实现
- [ ] 添加 XML 文档注释
- [ ] 更新相关单元测试
- [ ] 在 Deme 项目中添加示例代码
- [ ] 验证 AOT 编译通过
- [ ] 验证 .NET Standard 2.0 兼容性
- [ ] 验证 .NET 6.0 兼容性

## 参考文档

- [IEntityModel.cs](../../../Delly.Modeling/IEntityModel.cs) - 需要修改的实体接口
- [IBaseModel.cs](../../../Delly.Modeling/IBaseModel.cs) - 基础模型接口
- [zl.1.17-泛型建模支持.md](./zl.1.17-泛型建模支持.md) - 泛型建模支持参考文档
- [代码规范.md](../../../docs/开发指南/代码规范.md) - 代码规范参考
- [注意事项.md](../../../docs/开发指南/注意事项.md) - 开发注意事项
- [需求补充.md](./zl.1.18-需求补充.md) - 需求补充文档

## 验收标准

1. `IEntityModel` 接口包含 `GetGenericModelDefinition()` 函数定义
2. `IEntityModel` 接口包含 `GetGenericModels()` 函数定义
3. `IEntityModel` 接口包含 `MakeGenericModel()` 函数定义
4. 非泛型实体模型的 `GetGenericModelDefinition()` 返回自身
5. 非泛型实体模型的 `GetGenericModels()` 返回空列表
6. 非泛型实体模型的 `MakeGenericModel()` 抛出 `NotSupportedException`
7. 泛型定义模型的 `IsGenericModel` 返回 `true`，`IsGenericDefinition` 返回 `true`
8. 泛型定义模型的 `GetGenericModelDefinition()` 返回自身
9. 泛型定义模型的 `GetGenericModels()` 返回所有已构造的泛型实体模型
10. 已构造泛型的 `IsGenericModel` 返回 `true`，`IsGenericDefinition` 返回 `false`
11. 已构造泛型的 `GetGenericModelDefinition()` 返回泛型定义模型
12. `MakeGenericModel` 参数数量校验正确
13. 源生成器代码中不包含任何反射调用
14. 源生成器为生成的实体模型类添加泛型相关方法支持
15. XML 文档注释完整
16. 单元测试通过
17. Deme 项目中的示例能正常运行
18. AOT 编译通过
19. .NET Standard 2.0 兼容性验证通过
20. .NET 6.0 兼容性验证通过
