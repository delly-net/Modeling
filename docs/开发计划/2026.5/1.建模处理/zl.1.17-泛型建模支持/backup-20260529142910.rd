# zl.1.17 泛型建模支持

## 任务信息

- **任务号**: zl.1.17
- **任务名称**: 泛型建模支持
- **责任人**: 郑琳
- **任务类型**: 功能开发
- **任务描述**: 在 `IBaseModel` 提供 `bool IsGenericModel` 属性，在 `IModel` 提供 `IModel GetGenericModelDefinition()`、`IReadOnlyList<IModel> GetGenericModels()` 和 `IModel MakeGenericModel(params IModel[] models)` 函数，以及常见泛型对象类

## 功能概述

在建模系统中添加泛型建模支持，包括：

1. **IsGenericModel 属性**：标识当前模型是否为已构造的泛型模型（定义在 `IBaseModel`）
2. **GetGenericModelDefinition 函数**：获取泛型模型的定义模型（定义在 `IModel`）
3. **GetGenericModels 函数**：获取已构造的泛型模型列表（定义在 `IModel`）
4. **MakeGenericModel 函数**：根据泛型参数创建已构造的泛型模型（定义在 `IModel`）
5. **常见泛型对象类**：`ListModel`、`DictionaryModel` 等预定义泛型定义模型

此功能支持建模库在处理泛型类型时提供完整的类型信息，便于源生成器和运行时进行类型推断和处理。

**重要约束**：
- 不允许使用反射方式调用
- 接口定义使用非空类型 `object`，通过条件编译支持不同 .NET 版本
- 泛型定义模型的 `ClassType` 应为开放泛型类型（如 `typeof(List<>)`）
- 已构造模型的 `ClassType` 应为具体类型（如 `typeof(List<int>)`）
- 泛型建模本身不支持源生成，使用预定义实例模式
- 泛型相关方法（除 `IsGenericModel` 外）定义在 `IModel` 接口，返回 `IModel` 类型
- **`IsGenericModel` 语义**：仅已构造的泛型（如 `List<int>`）返回 `true`，开放泛型定义（如 `List<>`）和非泛型类型返回 `false`

## 技术背景

### 现有实现

当前 `IBaseModel` 和 `IModel` 接口定义如下：

```csharp
#if !NETSTANDARD2_0
#nullable enable
#endif

using System;
using System.Collections.Generic;

namespace Delly.Modeling
{
    /// <summary>
    /// 模型对象基础接口
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

    /// <summary>
    /// 模型对象接口，提供属性访问能力
    /// </summary>
    public interface IModel : IBaseModel
    {
        /// <summary>
        /// 获取所有建模属性
        /// </summary>
        /// <returns>属性对象数组</returns>
        IModelProperty[] GetProperties();

        /// <summary>
        /// 获取建模属性
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <returns>属性对象</returns>
#if NETSTANDARD2_0
        IModelProperty GetProperty(string name);
#else
        IModelProperty? GetProperty(string name);
#endif
    }
}
```

### 新增需求

1. 在 `IBaseModel` 接口中添加 `IsGenericModel` 属性定义
2. 在 `IModel` 接口中添加 `GetGenericModelDefinition()` 函数定义
3. 在 `IModel` 接口中添加 `GetGenericModels()` 函数定义
4. 在 `IModel` 接口中添加 `MakeGenericModel()` 函数定义
5. 创建 `ListModel`、`DictionaryModel` 等常见泛型定义模型类
6. 创建 `ConstructedGenericModel` 已构造泛型模型类
7. 源生成器为生成的模型类添加 `IsGenericModel` 属性支持

### 使用场景示例

```csharp
// 场景1：判断是否为泛型模型
var listIntModel = ListModel.Instance.MakeGenericModel(Int32Model.Instance);
Console.WriteLine(listIntModel.IsGenericModel);  // true - 已构造的泛型

var listDefinition = ListModel.Instance;
Console.WriteLine(listDefinition.IsGenericModel);  // false - 开放泛型定义

var intModel = Int32Model.Instance;
Console.WriteLine(intModel.IsGenericModel);  // false - 非泛型类型

// 场景2：获取泛型定义模型
var definition = listIntModel.GetGenericModelDefinition();
Console.WriteLine(definition.ClassType);  // List<> (GenericType)
Console.WriteLine(definition.Name);  // List`1

// 场景3：获取所有已构造的泛型模型
var genericModels = definition.GetGenericModels();
foreach (var model in genericModels)
{
    Console.WriteLine(model.ClassType);  // List<int>, List<string>, etc.
}

// 场景4：使用 MakeGenericModel 创建泛型模型
var listInt32Model = ListModel.Instance.MakeGenericModel(Int32Model.Instance);
var dictStringIntModel = DictionaryModel.Instance.MakeGenericModel(StringModel.Instance, Int32Model.Instance);
```

## 实现方案

### 1. 修改 IBaseModel 接口

添加 `IsGenericModel` 属性定义：

```csharp
/// <summary>
/// 是否为已构造的泛型模型
/// </summary>
/// <remarks>
/// 对于已构造的泛型（如 List<int>）返回 true
/// 对于开放泛型定义（如 List<>）和非泛型类型返回 false
/// </remarks>
bool IsGenericModel { get; }
```

### 2. 修改 IModel 接口

添加以下成员：

```csharp
/// <summary>
/// 获取泛型模型的定义模型
/// </summary>
/// <returns>泛型定义模型，对于非泛型模型返回自身</returns>
IModel GetGenericModelDefinition();

/// <summary>
/// 获取所有已构造的泛型模型
/// </summary>
/// <returns>已构造的泛型模型列表</returns>
IReadOnlyList<IModel> GetGenericModels();

/// <summary>
/// 根据泛型参数创建已构造的泛型模型
/// </summary>
/// <param name="models">泛型参数对应的模型列表</param>
/// <returns>已构造的泛型模型</returns>
IModel MakeGenericModel(params IModel[] models);
```

### 3. 基础 Model 对象实现

对于非泛型的基础 Model 对象（如 `Int32Model`、`StringModel` 等）：

```csharp
public sealed class Int32Model : IModel
{
    // ... 现有代码 ...

    /// <summary>
    /// 是否为已构造的泛型模型
    /// </summary>
    public bool IsGenericModel => false;

    /// <summary>
    /// 获取泛型模型的定义模型
    /// </summary>
    /// <returns>自身</returns>
    public IModel GetGenericModelDefinition()
    {
        return this;
    }

    /// <summary>
    /// 获取所有已构造的泛型模型
    /// </summary>
    /// <returns>空列表</returns>
    public IReadOnlyList<IModel> GetGenericModels()
    {
        return Array.Empty<IModel>();
    }

    /// <summary>
    /// 根据泛型参数创建已构造的泛型模型
    /// </summary>
    /// <param name="models">泛型参数对应的模型列表</param>
    /// <returns>已构造的泛型模型</returns>
    /// <exception cref="NotSupportedException">Int32Model 不支持泛型建模创建</exception>
    public IModel MakeGenericModel(params IModel[] models)
    {
        throw new NotSupportedException("Int32Model 不支持泛型建模创建");
    }
}
```

### 4. 泛型定义模型实现

添加 `ListModel` 类作为 `List<T>` 的泛型定义模型：

```csharp
#if !NETSTANDARD2_0
#nullable enable
#endif

using System;
using System.Collections.Generic;

namespace Delly.Modeling
{
    /// <summary>
    /// List&lt;&gt; 泛型定义模型
    /// </summary>
    public sealed class ListModel : IModel
    {
        private static readonly ListModel _instance = new ListModel();
        private static readonly IModel[] _genericModels = Array.Empty<IModel>();

        /// <summary>
        /// 获取 ListModel 单例实例
        /// </summary>
        public static ListModel Instance => _instance;

        private ListModel()
        {
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => typeof(List<>).Name;

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
            if (args.Length == 0)
            {
                return Activator.CreateInstance(typeof(List<>).MakeGenericType(args[0].GetType()));
            }
            throw new ArgumentException("ListModel.CreateInstance 需要提供泛型类型参数");
        }

        /// <summary>
        /// 模型类型信息
        /// </summary>
        public Type ClassType => typeof(List<>);

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
            throw new NotSupportedException("ListModel 不支持泛型解析");
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
        /// 是否为已构造的泛型模型
        /// </summary>
        /// <remarks>开放泛型定义不是已构造的泛型，返回 false</remarks>
        public bool IsGenericModel => false;

        /// <summary>
        /// 获取所有建模属性
        /// </summary>
        /// <returns>属性对象数组</returns>
        public IModelProperty[] GetProperties()
        {
            return Array.Empty<IModelProperty>();
        }

        /// <summary>
        /// 获取建模属性
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <returns>属性对象</returns>
#if NETSTANDARD2_0
        public IModelProperty GetProperty(string name)
#else
        public IModelProperty? GetProperty(string name)
#endif
        {
            return null;
        }

        /// <summary>
        /// 获取泛型模型的定义模型
        /// </summary>
        /// <returns>自身</returns>
        public IModel GetGenericModelDefinition()
        {
            return this;
        }

        /// <summary>
        /// 获取所有已构造的泛型模型
        /// </summary>
        /// <returns>已构造的泛型模型列表</returns>
        public IReadOnlyList<IModel> GetGenericModels()
        {
            return _genericModels;
        }

        /// <summary>
        /// 根据泛型参数创建已构造的泛型模型
        /// </summary>
        /// <param name="models">泛型参数对应的模型列表</param>
        /// <returns>已构造的泛型模型</returns>
        public IModel MakeGenericModel(params IModel[] models)
        {
            if (models.Length != 1)
            {
                throw new ArgumentException("List<T> 需要一个泛型参数");
            }

            var elementType = models[0].ClassType;
            var listType = typeof(List<>).MakeGenericType(elementType);
            var listModel = new ConstructedGenericModel(listType, this, models);
            return listModel;
        }
    }
}
```

### 5. 已构造泛型模型实现

添加 `ConstructedGenericModel` 类用于表示已构造的泛型模型：

```csharp
#if !NETSTANDARD2_0
#nullable enable
#endif

using System;
using System.Collections.Generic;

namespace Delly.Modeling
{
    /// <summary>
    /// 已构造的泛型模型
    /// </summary>
    public sealed class ConstructedGenericModel : IModel
    {
        private readonly Type _constructedType;
        private readonly IModel _definition;
        private readonly IModel[] _genericArguments;

        /// <summary>
        /// 创建已构造的泛型模型
        /// </summary>
        /// <param name="constructedType">已构造的类型</param>
        /// <param name="definition">泛型定义模型</param>
        /// <param name="genericArguments">泛型参数模型列表</param>
        public ConstructedGenericModel(Type constructedType, IModel definition, IModel[] genericArguments)
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
            throw new NotSupportedException("ConstructedGenericModel 不支持泛型解析");
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
        /// 是否为已构造的泛型模型
        /// </summary>
        public bool IsGenericModel => true;

        /// <summary>
        /// 获取所有建模属性
        /// </summary>
        /// <returns>属性对象数组</returns>
        public IModelProperty[] GetProperties()
        {
            return Array.Empty<IModelProperty>();
        }

        /// <summary>
        /// 获取建模属性
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <returns>属性对象</returns>
#if NETSTANDARD2_0
        public IModelProperty GetProperty(string name)
#else
        public IModelProperty? GetProperty(string name)
#endif
        {
            return null;
        }

        /// <summary>
        /// 获取泛型模型的定义模型
        /// </summary>
        /// <returns>泛型定义模型</returns>
        public IModel GetGenericModelDefinition()
        {
            return _definition;
        }

        /// <summary>
        /// 获取所有已构造的泛型模型
        /// </summary>
        /// <returns>已构造的泛型模型列表</returns>
        public IReadOnlyList<IModel> GetGenericModels()
        {
            return Array.Empty<IModel>();
        }

        /// <summary>
        /// 根据泛型参数创建已构造的泛型模型
        /// </summary>
        /// <param name="models">泛型参数对应的模型列表</param>
        /// <returns>已构造的泛型模型</returns>
        /// <exception cref="NotSupportedException">已构造的泛型模型不支持再次构造</exception>
        public IModel MakeGenericModel(params IModel[] models)
        {
            throw new NotSupportedException("已构造的泛型模型不支持再次构造");
        }
    }
}
```

### 6. DictionaryModel 实现

添加 `DictionaryModel` 类作为 `Dictionary<TKey, TValue>` 的泛型定义模型：

```csharp
#if !NETSTANDARD2_0
#nullable enable
#endif

using System;
using System.Collections.Generic;

namespace Delly.Modeling
{
    /// <summary>
    /// Dictionary&lt;,&gt; 泛型定义模型
    /// </summary>
    public sealed class DictionaryModel : IModel
    {
        private static readonly DictionaryModel _instance = new DictionaryModel();
        private static readonly IModel[] _genericModels = Array.Empty<IModel>();

        /// <summary>
        /// 获取 DictionaryModel 单例实例
        /// </summary>
        public static DictionaryModel Instance => _instance;

        private DictionaryModel()
        {
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => typeof(Dictionary<,>).Name;

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
            throw new ArgumentException("DictionaryModel.CreateInstance 需要使用 MakeGenericModel 创建");
        }

        /// <summary>
        /// 模型类型信息
        /// </summary>
        public Type ClassType => typeof(Dictionary<,>);

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
            throw new NotSupportedException("DictionaryModel 不支持泛型解析");
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
        /// 是否为已构造的泛型模型
        /// </summary>
        /// <remarks>开放泛型定义不是已构造的泛型，返回 false</remarks>
        public bool IsGenericModel => false;

        /// <summary>
        /// 获取所有建模属性
        /// </summary>
        /// <returns>属性对象数组</returns>
        public IModelProperty[] GetProperties()
        {
            return Array.Empty<IModelProperty>();
        }

        /// <summary>
        /// 获取建模属性
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <returns>属性对象</returns>
#if NETSTANDARD2_0
        public IModelProperty GetProperty(string name)
#else
        public IModelProperty? GetProperty(string name)
#endif
        {
            return null;
        }

        /// <summary>
        /// 获取泛型模型的定义模型
        /// </summary>
        /// <returns>自身</returns>
        public IModel GetGenericModelDefinition()
        {
            return this;
        }

        /// <summary>
        /// 获取所有已构造的泛型模型
        /// </summary>
        /// <returns>已构造的泛型模型列表</returns>
        public IReadOnlyList<IModel> GetGenericModels()
        {
            return _genericModels;
        }

        /// <summary>
        /// 根据泛型参数创建已构造的泛型模型
        /// </summary>
        /// <param name="models">泛型参数对应的模型列表</param>
        /// <returns>已构造的泛型模型</returns>
        public IModel MakeGenericModel(params IModel[] models)
        {
            if (models.Length != 2)
            {
                throw new ArgumentException("Dictionary<TKey, TValue> 需要两个泛型参数");
            }

            var keyType = models[0].ClassType;
            var valueType = models[1].ClassType;
            var dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            var dictModel = new ConstructedGenericModel(dictType, this, models);
            return dictModel;
        }
    }
}
```

### 7. 源生成器实现

在以下源生成器中添加相关代码：

| 源生成器 | 特性 | 生成的类 | 添加内容 |
|:--------|:----|:--------|:--------|
| `ModelableSourceGenerator` | `ModelableAttribute` | `emc__{ClassName}` | `IsGenericModel` 属性支持 |
| `ModelTableSourceGenerator` | `MoTableAttribute` | `emc__{ClassName}` | `IsGenericModel` 属性支持 |
| `ModelTableSourceGenerator` | `MoQueryAttribute` | `emc__{ClassName}` | `IsGenericModel` 属性支持 |

**生成的泛型模型代码**：

```csharp
#if !NETSTANDARD2_0
#nullable enable
#endif

public partial class emc__ListInt : IModel
{
    private static readonly Type _genericDefinitionType = typeof(List<>);
    private static IModel? _genericDefinition;

    // ... 现有代码 ...

    /// <summary>
    /// 是否为已构造的泛型模型
    /// </summary>
    public bool IsGenericModel => true;

    /// <summary>
    /// 获取泛型模型的定义模型
    /// </summary>
    /// <returns>泛型定义模型</returns>
    public IModel GetGenericModelDefinition()
    {
        if (_genericDefinition == null)
        {
            // 通过静态实例获取泛型定义模型
            _genericDefinition = ListModel.Instance;
        }
        return _genericDefinition;
    }

    /// <summary>
    /// 获取所有已构造的泛型模型
    /// </summary>
    /// <returns>已构造的泛型模型列表</returns>
    public IReadOnlyList<IModel> GetGenericModels()
    {
        return Array.Empty<IModel>();
    }

    /// <summary>
    /// 根据泛型参数创建已构造的泛型模型
    /// </summary>
    /// <param name="models">泛型参数对应的模型列表</param>
    /// <returns>已构造的泛型模型</returns>
    /// <exception cref="NotSupportedException">已构造的泛型模型不支持再次构造</exception>
    public IModel MakeGenericModel(params IModel[] models)
    {
        throw new NotSupportedException("已构造的泛型模型不支持再次构造");
    }
}
```

## 使用示例

```csharp
// 示例1：检查是否为泛型模型
var listIntModel = ListModel.Instance.MakeGenericModel(Int32Model.Instance);
Console.WriteLine(listIntModel.IsGenericModel);  // true - 已构造的泛型

var listDefinition = ListModel.Instance;
Console.WriteLine(listDefinition.IsGenericModel);  // false - 开放泛型定义

var intModel = Int32Model.Instance;
Console.WriteLine(intModel.IsGenericModel);  // false - 非泛型类型

// 示例2：获取泛型定义
var definition = listIntModel.GetGenericModelDefinition();
Console.WriteLine($"泛型定义: {definition.ClassType}");  // List`1
Console.WriteLine($"泛型定义名称: {definition.Name}");  // List`1

// 示例3：获取所有已构造的泛型模型
var genericModels = definition.GetGenericModels();
Console.WriteLine($"已构造的泛型模型数量: {genericModels.Count}");

// 示例4：创建 Dictionary 泛型模型
var dictStringIntModel = DictionaryModel.Instance.MakeGenericModel(StringModel.Instance, Int32Model.Instance);
Console.WriteLine($"Dictionary<string, int>: {dictStringIntModel.ClassType}");
Console.WriteLine($"Dictionary 泛型定义: {dictStringIntModel.GetGenericModelDefinition().ClassType}");
```

## 实现步骤

### 开发任务清单

- [ ] 在 `IBaseModel` 接口中添加 `IsGenericModel` 属性定义
- [ ] 在 `IModel` 接口中添加 `GetGenericModelDefinition()` 函数定义
- [ ] 在 `IModel` 接口中添加 `GetGenericModels()` 函数定义
- [ ] 在 `IModel` 接口中添加 `MakeGenericModel()` 函数定义
- [ ] 创建 `ConstructedGenericModel` 类
- [ ] 创建 `ListModel` 类，使用 `typeof(List<>).Name` 作为 Name 属性
- [ ] 创建 `DictionaryModel` 类，使用 `typeof(Dictionary<,>).Name` 作为 Name 属性
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
- [ ] 在 `ModelableSourceGenerator` 中生成泛型相关代码和 `IsGenericModel` 属性
- [ ] 在 `ModelTableSourceGenerator` 中添加泛型检测逻辑（MoTable）
- [ ] 在 `ModelTableSourceGenerator` 中生成泛型相关代码和 `IsGenericModel` 属性（MoTable）
- [ ] 在 `ModelTableSourceGenerator` 中添加泛型检测逻辑（MoQuery）
- [ ] 在 `ModelTableSourceGenerator` 中生成泛型相关代码和 `IsGenericModel` 属性（MoQuery）
- [ ] 添加 XML 文档注释
- [ ] 更新相关单元测试
- [ ] 在 Deme 项目中添加示例代码
- [ ] 验证 AOT 编译通过
- [ ] 验证 .NET Standard 2.0 兼容性
- [ ] 验证 .NET 6.0 兼容性

## 参考文档

- [IBaseModel.cs](../../../Delly.Modeling/IBaseModel.cs) - 需要修改的基础接口
- [IModel.cs](../../../Delly.Modeling/IModel.cs) - 需要修改的模型接口
- [代码规范.md](../../../docs/开发指南/代码规范.md) - 代码规范参考
- [注意事项.md](../../../docs/开发指南/注意事项.md) - 开发注意事项
- [需求补充.md](./zl.1.17-需求补充.md) - 需求补充文档

## 验收标准

1. `IBaseModel` 接口包含 `IsGenericModel` 属性定义
2. `IModel` 接口包含 `GetGenericModelDefinition()` 函数定义
3. `IModel` 接口包含 `GetGenericModels()` 函数定义
4. `IModel` 接口包含 `MakeGenericModel()` 函数定义
5. `ListModel` 类已实现 `IModel` 接口，`Name` 使用 `typeof(List<>).Name`
6. `DictionaryModel` 类已实现 `IModel` 接口，`Name` 使用 `typeof(Dictionary<,>).Name`
7. `ConstructedGenericModel` 类已实现 `IModel` 接口
8. 所有基础 Model 对象都已实现泛型支持方法
9. 非泛型模型的 `IsGenericModel` 返回 `false`
10. 非泛型模型的 `GetGenericModelDefinition()` 返回自身
11. 非泛型模型的 `GetGenericModels()` 返回空列表
12. 开放泛型定义（如 `ListModel.Instance`）的 `IsGenericModel` 返回 `false`
13. 开放泛型定义的 `GetGenericModelDefinition()` 返回自身
14. 已构造泛型（如 `List<int>` 模型）的 `IsGenericModel` 返回 `true`
15. 已构造泛型的 `GetGenericModelDefinition()` 返回泛型定义模型
16. 泛型定义模型的 `GetGenericModels()` 返回所有已构造的泛型模型
17. `ListModel.Instance.MakeGenericModel(Int32Model.Instance)` 能正确创建 `List<int>` 模型
18. `DictionaryModel.Instance.MakeGenericModel(StringModel.Instance, Int32Model.Instance)` 能正确创建 `Dictionary<string, int>` 模型
19. 源生成器代码中不包含任何反射调用
20. 源生成器为生成的模型类添加 `IsGenericModel` 属性支持
21. XML 文档注释完整
22. 单元测试通过
23. Deme 项目中的示例能正常运行
24. AOT 编译通过
25. .NET Standard 2.0 兼容性验证通过
26. .NET 6.0 兼容性验证通过
