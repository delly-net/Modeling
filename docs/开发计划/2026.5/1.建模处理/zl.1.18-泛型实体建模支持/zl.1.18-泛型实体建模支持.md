# zl.1.18 泛型实体建模支持

## 任务信息

- **任务号**: zl.1.18
- **任务名称**: 泛型实体建模支持
- **责任人**: 郑琳
- **任务类型**: 功能开发
- **任务描述**: 在 `IEntityModel` 提供泛型操作函数，支持泛型实体模型的定义和构造，并添加常见泛型实体模型类

## 功能概述

在实体建模系统中添加泛型实体建模支持，包括：

1. **GetGenericModelDefinition 函数**：获取泛型实体模型的定义模型（定义在 `IEntityModel`）
2. **GetGenericModels 函数**：获取所有已构造的泛型实体模型列表（定义在 `IEntityModel`）
3. **MakeGenericModel 函数**：根据泛型参数创建已构造的泛型实体模型（定义在 `IEntityModel`）
4. **常见泛型实体模型类**：`ListEntityModel`、`DictionaryEntityModel` 等预定义泛型定义模型
5. **工厂泛型支持**：`DefaultEntityModelFactory.GetModel<T>()` 支持泛型类型获取

此功能是 zl.1.17 泛型建模支持在实体模型层面的扩展，专门针对 `[MoTable]` 和 `[MoQuery]` 标记的实体类提供泛型建模能力。

**重要约束**：
- 不允许使用反射方式调用
- 接口定义使用非空类型 `object`，通过条件编译支持不同 .NET 版本
- 泛型定义模型的 `ClassType` 应为开放泛型类型（如 `typeof(TableEntity<>)`）
- 已构造模型的 `ClassType` 应为具体类型（如 `typeof(TableEntity<int>)`）
- 泛型实体建模支持源生成，使用预定义实例模式
- 本任务中的函数定义在 `IEntityModel` 接口（而非 `IModel`）
- 与 zl.1.17 的区别：本任务专门针对实体模型（`IEntityModel`），返回类型也是 `IEntityModel`
- **不支持**泛型实体模型的泛型属性建模

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
4. 创建 `ListEntityModel`、`DictionaryEntityModel` 等常见泛型定义模型类
5. 创建 `ConstructedGenericEntityModel` 已构造泛型实体模型类
6. 源生成器为生成的实体模型类添加泛型相关方法支持
7. `DefaultEntityModelFactory.GetModel<T>()` 支持泛型类型获取

### 与 zl.1.17 的关系

| 特性 | zl.1.17 泛型建模支持 | zl.1.18 泛型实体建模支持 |
|:----|:-------------------|:----------------------|
| 接口 | `IModel` | `IEntityModel` |
| 返回类型 | `IModel` | `IEntityModel` |
| 适用范围 | 所有标记 `[Modelable]` 的类 | 标记 `[MoTable]` 或 `[MoQuery]` 的实体类 |
| 属性支持 | `IsGenericModel` 等属性在 `IBaseModel` | 继承自 `IBaseModel` |
| 源生成器 | `ModelableSourceGenerator` | `ModelTableSourceGenerator` |
| 预定义模型 | `ListModel`、`DictionaryModel` | `ListEntityModel`、`DictionaryEntityModel` |

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

// 场景5：使用预定义泛型实体模型
var listIntEntityModel = ListEntityModel.Instance.MakeGenericModel(Int32Model.Instance);
Console.WriteLine(listIntEntityModel.ClassType);  // List`1

// 场景6：通过工厂获取泛型实体模型
var listModel = DefaultEntityModelFactory.Instance.GetModel<List<int>>();
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

### 4. ListEntityModel 泛型定义模型实现

添加 `ListEntityModel` 类作为 `List<T>` 的泛型实体定义模型：

```csharp
#if !NETSTANDARD2_0
#nullable enable
#endif

using System;
using System.Collections.Generic;

namespace Delly.Modeling
{
    /// <summary>
    /// List&lt;&gt; 泛型实体定义模型
    /// </summary>
    public sealed class ListEntityModel : IEntityModel
    {
        private static readonly ListEntityModel _instance = new ListEntityModel();
        private static readonly Type _listType = typeof(List<>);
        private static readonly List<IEntityModel> _genericModels = new List<IEntityModel>();

        /// <summary>
        /// 获取 ListEntityModel 单例实例
        /// </summary>
        public static ListEntityModel Instance => _instance;

        private ListEntityModel()
        {
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => _listType.Name;

        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName => _listType.Name;

        /// <summary>
        /// 类型全名称
        /// </summary>
        public string Namespace => "System.Collections.Generic";

        /// <summary>
        /// 创建模型类型的新实例
        /// </summary>
        /// <param name="args">构造函数参数数组</param>
        /// <returns>模型类型的新实例</returns>
        public object CreateInstance(params object[] args)
        {
            throw new ArgumentException("ListEntityModel.CreateInstance 需要使用 MakeGenericModel 创建");
        }

        /// <summary>
        /// 模型类型信息
        /// </summary>
        public Type ClassType => _listType;

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
            throw new NotSupportedException("ListEntityModel 不支持泛型解析");
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
                throw new ArgumentException("List<T> 需要一个泛型参数");
            }

            var elementType = models[0].ClassType;
            var listType = _listType.MakeGenericType(elementType);
            var listModel = new ConstructedGenericEntityModel(listType, this, models);
            _genericModels.Add(listModel);
            return listModel;
        }
    }
}
```

### 5. DictionaryEntityModel 泛型定义模型实现

添加 `DictionaryEntityModel` 类作为 `Dictionary<TKey, TValue>` 的泛型实体定义模型：

```csharp
#if !NETSTANDARD2_0
#nullable enable
#endif

using System;
using System.Collections.Generic;

namespace Delly.Modeling
{
    /// <summary>
    /// Dictionary&lt;,&gt; 泛型实体定义模型
    /// </summary>
    public sealed class DictionaryEntityModel : IEntityModel
    {
        private static readonly DictionaryEntityModel _instance = new DictionaryEntityModel();
        private static readonly Type _dictionaryType = typeof(Dictionary<,>);
        private static readonly List<IEntityModel> _genericModels = new List<IEntityModel>();

        /// <summary>
        /// 获取 DictionaryEntityModel 单例实例
        /// </summary>
        public static DictionaryEntityModel Instance => _instance;

        private DictionaryEntityModel()
        {
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => _dictionaryType.Name;

        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName => _dictionaryType.Name;

        /// <summary>
        /// 类型全名称
        /// </summary>
        public string Namespace => "System.Collections.Generic";

        /// <summary>
        /// 创建模型类型的新实例
        /// </summary>
        /// <param name="args">构造函数参数数组</param>
        /// <returns>模型类型的新实例</returns>
        public object CreateInstance(params object[] args)
        {
            throw new ArgumentException("DictionaryEntityModel.CreateInstance 需要使用 MakeGenericModel 创建");
        }

        /// <summary>
        /// 模型类型信息
        /// </summary>
        public Type ClassType => _dictionaryType;

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
            throw new NotSupportedException("DictionaryEntityModel 不支持泛型解析");
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
        public int GenericDefinitionCount => 2;

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
            if (models.Length != 2)
            {
                throw new ArgumentException("Dictionary<TKey, TValue> 需要两个泛型参数");
            }

            var keyType = models[0].ClassType;
            var valueType = models[1].ClassType;
            var dictType = _dictionaryType.MakeGenericType(keyType, valueType);
            var dictModel = new ConstructedGenericEntityModel(dictType, this, models);
            _genericModels.Add(dictModel);
            return dictModel;
        }
    }
}
```

### 6. ConstructedGenericEntityModel 已构造泛型实体模型实现

添加 `ConstructedGenericEntityModel` 类用于表示已构造的泛型实体模型：

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

### 7. DefaultEntityModelFactory 泛型支持

修改 `DefaultEntityModelFactory` 类，在 `GetModel<T>()` 方法中添加泛型类型支持：

```csharp
/// <summary>
/// 获取指定类型的实体模型
/// </summary>
/// <typeparam name="T">类型参数</typeparam>
/// <returns>实体模型对象</returns>
public IEntityModel GetModel<T>() where T : class
{
    var type = typeof(T);

    // 判断是否为泛型类型
    if (type.IsGenericType)
    {
        // 获取开放泛型定义类型
        var genericDefinition = type.GetGenericTypeDefinition();

        // 根据开放泛型定义类型名称获取对应的泛型定义模型
        var definitionModel = genericDefinition.Name switch
        {
            "List`1" => ListEntityModel.Instance,
            "Dictionary`2" => DictionaryEntityModel.Instance,
            _ => throw new NotSupportedException($"不支持的泛型类型: {genericDefinition.Name}")
        };

        // 获取泛型参数类型对应的模型
        var genericArguments = type.GetGenericArguments();
        var argumentModels = new IEntityModel[genericArguments.Length];

        for (int i = 0; i < genericArguments.Length; i++)
        {
            argumentModels[i] = GetModelByType(genericArguments[i]);
        }

        // 通过泛型定义模型创建已构造的泛型实体模型
        return definitionModel.MakeGenericModel(argumentModels);
    }

    // 非泛型类型，执行现有常规逻辑
    // ... 现有代码 ...
}

/// <summary>
/// 根据类型获取实体模型
/// </summary>
/// <param name="type">类型对象</param>
/// <returns>实体模型对象</returns>
private IEntityModel GetModelByType(Type type)
{
    // 根据基础类型返回对应的实体模型
    return type.Name switch
    {
        nameof(String) => StringEntityModel.Instance,
        nameof(Int32) => Int32EntityModel.Instance,
        nameof(Int64) => Int64EntityModel.Instance,
        nameof(Boolean) => BooleanEntityModel.Instance,
        nameof(Double) => DoubleEntityModel.Instance,
        nameof(Decimal) => DecimalEntityModel.Instance,
        nameof(DateTime) => DateTimeEntityModel.Instance,
        nameof(Guid) => GuidEntityModel.Instance,
        _ => throw new NotSupportedException($"不支持的类型: {type.Name}")
    };
}
```

### 8. 源生成器实现

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

// 示例6：使用预定义泛型实体模型
var listIntEntityModel = ListEntityModel.Instance.MakeGenericModel(Int32Model.Instance);
Console.WriteLine($"List<int>: {listIntEntityModel.ClassType}");

// 示例7：使用 DictionaryEntityModel
var dictStringIntModel = DictionaryEntityModel.Instance.MakeGenericModel(StringModel.Instance, Int32Model.Instance);
Console.WriteLine($"Dictionary<string, int>: {dictStringIntModel.ClassType}");

// 示例8：通过工厂获取泛型实体模型
var factory = DefaultEntityModelFactory.Instance;
var listModel = factory.GetModel<List<int>>();
Console.WriteLine($"通过工厂获取 List<int>: {listModel.ClassType}");

// 示例9：MakeGenericModel 参数校验
try
{
    // ListEntityModel 只需要一个泛型参数
    ListEntityModel.Instance.MakeGenericModel(Int32Model.Instance, StringModel.Instance);
}
catch (ArgumentException ex)
{
    Console.WriteLine(ex.Message);  // List<T> 需要一个泛型参数
}
```

## 实现步骤

### 开发任务清单

- [ ] 在 `IEntityModel` 接口中添加 `GetGenericModelDefinition()` 函数定义
- [ ] 在 `IEntityModel` 接口中添加 `GetGenericModels()` 函数定义
- [ ] 在 `IEntityModel` 接口中添加 `MakeGenericModel()` 函数定义
- [ ] 创建 `ConstructedGenericEntityModel` 类
- [ ] 创建 `ListEntityModel` 类，使用 `static readonly Type _listType = typeof(List<>)` 方式
- [ ] 创建 `DictionaryEntityModel` 类，使用 `static readonly Type _dictionaryType = typeof(Dictionary<,>)` 方式
- [ ] 在 `DefaultEntityModelFactory.GetModel<T>()` 中添加泛型类型支持
- [ ] 在 `DefaultEntityModelFactory` 中添加 `GetModelByType()` 辅助方法
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
- [DefaultEntityModelFactory.cs](../../../Delly.Modeling/DefaultEntityModelFactory.cs) - 需要修改的工厂类
- [zl.1.17-泛型建模支持.md](./zl.1.17-泛型建模支持.md) - 泛型建模支持参考文档
- [代码规范.md](../../../docs/开发指南/代码规范.md) - 代码规范参考
- [注意事项.md](../../../docs/开发指南/注意事项.md) - 开发注意事项
- [需求补充.md](./zl.1.18-需求补充.md) - 需求补充文档

## 验收标准

### 接口定义
1. `IEntityModel` 接口包含 `GetGenericModelDefinition()` 函数定义
2. `IEntityModel` 接口包含 `GetGenericModels()` 函数定义
3. `IEntityModel` 接口包含 `MakeGenericModel()` 函数定义

### 非泛型实体模型
4. 非泛型实体模型的 `GetGenericModelDefinition()` 返回自身
5. 非泛型实体模型的 `GetGenericModels()` 返回空列表
6. 非泛型实体模型的 `MakeGenericModel()` 抛出 `NotSupportedException`

### 泛型定义模型
7. 泛型定义模型的 `IsGenericModel` 返回 `true`，`IsGenericDefinition` 返回 `true`
8. 泛型定义模型的 `GetGenericModelDefinition()` 返回自身
9. 泛型定义模型的 `GetGenericModels()` 返回所有已构造的泛型实体模型

### 已构造泛型模型
10. 已构造泛型的 `IsGenericModel` 返回 `true`，`IsGenericDefinition` 返回 `false`
11. 已构造泛型的 `GetGenericModelDefinition()` 返回泛型定义模型

### 预定义泛型实体模型
12. `ListEntityModel` 类已实现 `IEntityModel` 接口，`Name` 使用 `typeof(List<>).Name`
13. `DictionaryEntityModel` 类已实现 `IEntityModel` 接口，`Name` 使用 `typeof(Dictionary<,>).Name`
14. `ConstructedGenericEntityModel` 类已实现 `IEntityModel` 接口

### 工厂泛型支持
15. `DefaultEntityModelFactory.GetModel<T>()` 支持泛型类型获取
16. 工厂能正确处理 `List<T>` 类型
17. 工厂能正确处理 `Dictionary<TKey, TValue>` 类型

### 参数校验
18. `ListEntityModel.MakeGenericModel` 参数数量校验正确（需要1个参数）
19. `DictionaryEntityModel.MakeGenericModel` 参数数量校验正确（需要2个参数）

### 源生成器
20. 源生成器代码中不包含任何反射调用
21. 源生成器为生成的实体模型类添加泛型相关方法支持

### 文档和测试
22. XML 文档注释完整
23. 单元测试通过
24. Deme 项目中的示例能正常运行
25. AOT 编译通过
26. .NET Standard 2.0 兼容性验证通过
27. .NET 6.0 兼容性验证通过
