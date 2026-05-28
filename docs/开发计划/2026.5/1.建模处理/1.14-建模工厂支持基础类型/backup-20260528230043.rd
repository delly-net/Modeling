# 1.14 建模工厂支持基础类型

## 任务信息

- **任务号**: 1.14
- **任务名称**: 建模工厂支持基础类型
- **责任人**: 郑琳
- **任务类型**: 代码优化
- **任务描述**: `DefaultEntityModelFactory` 类中初始化时添加所有基础建模

## 功能概述

在 `DefaultEntityModelFactory` 的构造函数中初始化所有基础类型建模，使工厂能够直接提供基础类型（如 `string`、`int`、`long`、`bool`、`double`、`decimal`、`DateTime`、`Guid`）的建模支持，无需额外注册。

**重要约束**：
- 基础类型建模在工厂构造时自动注册，使用类全称 `{Namespace}.{ClassName}` 作为键
- 保持 `DefaultEntityModelFactory._models` 字段类型为 `Dictionary<string, IEntityModel>` 不变
- 现有 `IBaseModel` 实现类（StringModel、Int32Model 等）保持不变
- 为基础类型创建新的 `IEntityModel` 适配器类
- 泛型方法不使用 `where T : class` 约束，支持所有类型

## 技术背景

### 现有实现

**基础类型建模 Models** 目录下已存在以下模型类：
- `StringModel` - String 建模
- `Int32Model` - Int32 建模
- `Int64Model` - Int64 建模
- `BooleanModel` - Boolean 建模
- `DoubleModel` - Double 建模
- `DecimalModel` - Decimal 建模
- `DateTimeModel` - DateTime 建模
- `GuidModel` - Guid 建模

每个模型类都实现了 `IBaseModel` 接口，并提供静态 `Instance` 属性：
```csharp
public static StringModel Instance => _instance;
```

**DefaultEntityModelFactory** 当前实现：
- 构造函数仅初始化内部集合，未注册任何基础类型建模
- `_models` 字段类型为 `Dictionary<string, IEntityModel>`
- 提供 `Add(IEntityModel)` 方法添加单个实体模型
- 提供 `AddSet(IEntityModelSet)` 方法批量添加建模集合
- 查询方法按类全称或表名查找模型

### 接口定义

**IBaseModel** (基础类型建模接口)：
```csharp
/// <summary>
/// 基础模型接口
/// </summary>
public interface IBaseModel
{
    /// <summary>
    /// 名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 命名空间
    /// </summary>
    string Namespace { get; }

    /// <summary>
    /// 模型类型信息
    /// </summary>
    Type ClassType { get; }

    /// <summary>
    /// 创建模型类型的新实例
    /// </summary>
    object CreateInstance(params object[] args);

    /// <summary>
    /// 将输入对象解析为模型实例
    /// </summary>
    object Parse(object obj);

    /// <summary>
    /// 尝试将输入对象解析为模型实例
    /// </summary>
    object TryParse(object obj);

    /// <summary>
    /// 是否为值类型对象
    /// </summary>
    bool IsValue { get; }
}
```

**IEntityModel** (实体模型接口，继承 IBaseModel)：
```csharp
/// <summary>
/// 实体模型接口
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
    IEntityModelProperty[] GetProperties();

    /// <summary>
    /// 获取所有索引定义
    /// </summary>
    EntityModelIndex[] GetIndexes();

    /// <summary>
    /// 获取建模属性
    /// </summary>
    IEntityModelProperty GetProperty(string name);
}
```

## 实现方案

### 1. 创建适配器类

在 `Delly.Modeling/Models/EntityModels/` 目录下创建适配器类，实现 `IEntityModel` 接口：

```csharp
#if !NETSTANDARD2_0
#nullable enable
#endif

using System;

namespace Delly.Modeling
{
    /// <summary>
    /// String 实体建模适配器
    /// </summary>
    public sealed class StringEntityModel : IEntityModel
    {
        private static readonly StringEntityModel _instance = new StringEntityModel();
        private readonly IBaseModel _baseModel = StringModel.Instance;

        /// <summary>
        /// 获取 String 实体建模单例实例
        /// </summary>
        public static StringEntityModel Instance => _instance;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => _baseModel.Name;

        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace => _baseModel.Namespace;

        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName => "String";

        /// <summary>
        /// 模型类型信息
        /// </summary>
        public Type ClassType => _baseModel.ClassType;

        /// <summary>
        /// 是否为值类型对象
        /// </summary>
        public bool IsValue => _baseModel.IsValue;

        /// <summary>
        /// 创建模型类型的新实例
        /// </summary>
        /// <param name="args">构造函数参数数组</param>
        /// <returns>模型类型的新实例</returns>
        public object CreateInstance(params object[] args) => _baseModel.CreateInstance(args);

        /// <summary>
        /// 将输入对象解析为模型实例
        /// </summary>
        /// <param name="obj">输入对象</param>
        /// <returns>模型实例</returns>
#if NETSTANDARD2_0
        public object Parse(object obj) => _baseModel.Parse(obj);
#else
        public object Parse(object? obj) => _baseModel.Parse(obj);
#endif

        /// <summary>
        /// 尝试将输入对象解析为模型实例
        /// </summary>
        /// <param name="obj">输入对象</param>
        /// <returns>模型实例，解析失败时返回 null</returns>
#if NETSTANDARD2_0
        public object TryParse(object obj) => _baseModel.TryParse(obj);
#else
        public object? TryParse(object? obj) => _baseModel.TryParse(obj);
#endif

        /// <summary>
        /// 获取所有建模属性
        /// </summary>
        /// <returns>空数组</returns>
        public IEntityModelProperty[] GetProperties() => Array.Empty<IEntityModelProperty>();

        /// <summary>
        /// 获取所有索引定义
        /// </summary>
        /// <returns>空数组</returns>
        public EntityModelIndex[] GetIndexes() => Array.Empty<EntityModelIndex>();

        /// <summary>
        /// 获取建模属性
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <returns>null</returns>
#if NETSTANDARD2_0
        public IEntityModelProperty GetProperty(string name) => null;
#else
        public IEntityModelProperty? GetProperty(string name) => null;
#endif
    }
}
```

按照相同模式为其他 7 个基础类型创建适配器类：
- `Int32EntityModel`
- `Int64EntityModel`
- `BooleanEntityModel`
- `DoubleEntityModel`
- `DecimalEntityModel`
- `DateTimeEntityModel`
- `GuidEntityModel`

### 2. 添加静态 Type 字段

在 `DefaultEntityModelFactory` 中添加基础类型的静态 Type 字段：

```csharp
// 基础类型 Type 字段，用于泛型方法匹配
private static readonly Type _stringType = typeof(string);
private static readonly Type _int32Type = typeof(int);
private static readonly Type _int64Type = typeof(long);
private static readonly Type _booleanType = typeof(bool);
private static readonly Type _doubleType = typeof(double);
private static readonly Type _decimalType = typeof(decimal);
private static readonly Type _dateTimeType = typeof(DateTime);
private static readonly Type _guidType = typeof(Guid);
```

### 3. 实现注册方法

```csharp
// 注册基础类型建模
private void RegisterBaseTypeModels()
{
    // StringEntityModel - 同时支持两个键
    _models[string.Empty] = StringEntityModel.Instance;
    _models["System.String"] = StringEntityModel.Instance;

    // Int32EntityModel
    _models["System.Int32"] = Int32EntityModel.Instance;

    // Int64EntityModel
    _models["System.Int64"] = Int64EntityModel.Instance;

    // BooleanEntityModel
    _models["System.Boolean"] = BooleanEntityModel.Instance;

    // DoubleEntityModel
    _models["System.Double"] = DoubleEntityModel.Instance;

    // DecimalEntityModel
    _models["System.Decimal"] = DecimalEntityModel.Instance;

    // DateTimeEntityModel
    _models["System.DateTime"] = DateTimeEntityModel.Instance;

    // GuidEntityModel
    _models["System.Guid"] = GuidEntityModel.Instance;
}
```

### 4. 修改构造函数

在构造函数中调用注册方法：

```csharp
/// <summary>
/// 初始化默认实体建模工厂
/// </summary>
public DefaultEntityModelFactory()
{
    _models = new Dictionary<string, IEntityModel>();
    _modelSets = new List<IEntityModelSet>();
    _registeredSets = new HashSet<IEntityModelSet>();

    // 注册基础类型建模
    RegisterBaseTypeModels();
}
```

### 5. 扩展泛型方法（移除 class 约束）

使用 `typeof + switch case` 模式匹配支持基础类型，**不使用 `where T : class` 约束**：

```csharp
/// <summary>
/// 获取指定类型的实体模型
/// </summary>
/// <typeparam name="T">类型</typeparam>
/// <returns>指定类型的实体模型</returns>
/// <exception cref="System.NotSupportedException">当类型未找到时抛出</exception>
public IEntityModel GetModel<T>()
{
    return typeof(T) switch
    {
        var t when t == _stringType => GetModel("System.String") ?? throw new NotSupportedException($"未找到类型 {typeof(T).FullName} 的建模"),
        var t when t == _int32Type => GetModel("System.Int32") ?? throw new NotSupportedException($"未找到类型 {typeof(T).FullName} 的建模"),
        var t when t == _int64Type => GetModel("System.Int64") ?? throw new NotSupportedException($"未找到类型 {typeof(T).FullName} 的建模"),
        var t when t == _booleanType => GetModel("System.Boolean") ?? throw new NotSupportedException($"未找到类型 {typeof(T).FullName} 的建模"),
        var t when t == _doubleType => GetModel("System.Double") ?? throw new NotSupportedException($"未找到类型 {typeof(T).FullName} 的建模"),
        var t when t == _decimalType => GetModel("System.Decimal") ?? throw new NotSupportedException($"未找到类型 {typeof(T).FullName} 的建模"),
        var t when t == _dateTimeType => GetModel("System.DateTime") ?? throw new NotSupportedException($"未找到类型 {typeof(T).FullName} 的建模"),
        var t when t == _guidType => GetModel("System.Guid") ?? throw new NotSupportedException($"未找到类型 {typeof(T).FullName} 的建模"),
        _ => TryGetModelFromModelSets<T>() ?? throw new NotSupportedException($"类型 {typeof(T).FullName} 未在已注册的建模集合中找到")
    };
}

// 辅助方法：从建模集合中查找
#if NETSTANDARD2_0
private IEntityModel TryGetModelFromModelSets<T>()
#else
private IEntityModel? TryGetModelFromModelSets<T>()
#endif
{
    foreach (var set in _modelSets)
    {
        var model = set.TryGetModel<T>();
        if (model != null)
        {
            return model;
        }
    }
    return null;
}
```

## 使用示例

```csharp
// 创建工厂实例（自动注册所有基础类型建模）
var factory = new DefaultEntityModelFactory();

// 按类全称查询
var stringModel = factory.GetModel("System.String");
var intModel = factory.GetModel("System.Int32");

// 使用泛型方法查询（支持所有类型，包括值类型）
var stringModel2 = factory.GetModel<string>();
var intModel2 = factory.GetModel<int>();
var boolModel = factory.GetModel<bool>();
var doubleModel = factory.GetModel<double>();

// 使用基础类型建模
var str = stringModel.CreateInstance("hello");
var parsed = stringModel.Parse(123); // "123"

// 判断是否为值类型
if (stringModel.IsValue)
{
    Console.WriteLine("String 是值类型对象");
}

// 基础类型建模的属性和索引为空
var properties = stringModel.GetProperties(); // 返回空数组
var indexes = stringModel.GetIndexes(); // 返回空数组
var property = stringModel.GetProperty("AnyName"); // 返回 null
```

## 实现步骤

### 开发任务清单

- [ ] 创建 `Delly.Modeling/Models/EntityModels/` 目录
- [ ] 创建 `StringEntityModel` 适配器类
- [ ] 创建 `Int32EntityModel` 适配器类
- [ ] 创建 `Int64EntityModel` 适配器类
- [ ] 创建 `BooleanEntityModel` 适配器类
- [ ] 创建 `DoubleEntityModel` 适配器类
- [ ] 创建 `DecimalEntityModel` 适配器类
- [ ] 创建 `DateTimeEntityModel` 适配器类
- [ ] 创建 `GuidEntityModel` 适配器类
- [ ] 在 `DefaultEntityModelFactory` 中添加基础类型静态 Type 字段
- [ ] 实现 `RegisterBaseTypeModels()` 私有方法
- [ ] 在构造函数中调用 `RegisterBaseTypeModels()`
- [ ] 重构 `GetModel<T>()` 方法：移除 `where T : class` 约束，支持基础类型
- [ ] 更新 `TryGetModelFromModelSets<T>()` 辅助方法：移除 `where T : class` 约束
- [ ] 更新相关方法的 XML 文档注释
- [ ] 在 Deme 项目中添加示例代码（展示值类型泛型调用）
- [ ] 更新单元测试
- [ ] 验证所有基础类型建模正确注册
- [ ] 验证查询方法返回正确结果

## 规范要求

### XML 文档注释

所有公共成员必须包含完整的 XML 文档注释：
- 类、方法、属性使用 `///` 格式
- 参数名必须与实际参数名匹配
- 返回值描述清晰明确

### 代码风格

- 使用 `#if !NETSTANDARD2_0` 条件编译支持可空类型
- 私有成员使用 `//` 注释
- 公共成员使用 `///` 注释

### 命名规范

- 类名使用 PascalCase
- 方法名使用 PascalCase
- 私有字段使用 _camelCase
- 辅助方法使用清晰描述性名称

## 参考文档

- [DefaultEntityModelFactory.cs](../../../Delly.Modeling/DefaultEntityModelFactory.cs) - 需要修改的工厂类
- [IBaseModel.cs](../../../Delly.Modeling/IBaseModel.cs) - 基础模型接口
- [IEntityModel.cs](../../../Delly.Modeling/IEntityModel.cs) - 实体模型接口
- [StringModel.cs](../../../Delly.Modeling/Models/StringModel.cs) - String 建模参考
- [Int32Model.cs](../../../Delly.Modeling/Models/Int32Model.cs) - Int32 建模参考
- [代码规范.md](../../../docs/开发指南/代码规范.md) - 代码规范参考
- [注意事项.md](../../../docs/开发指南/注意事项.md) - 开发注意事项

## 验收标准

1. 创建 8 个基础类型的 `IEntityModel` 适配器类
2. `DefaultEntityModelFactory` 构造函数自动注册所有基础类型建模
3. 基础类型建模使用正确的类全称作为键
4. 查询方法能正确返回已注册的基础类型建模
5. `GetModel<T>()` 方法支持所有类型（包括值类型），不使用 `where T : class` 约束
6. 适配器类的 `GetProperties()` 和 `GetIndexes()` 返回空数组
7. 适配器类的 `GetProperty(name)` 返回 null
8. 生成的代码编译通过，无警告
9. XML 文档注释完整
10. Deme 项目中的示例能正常运行（包含值类型泛型调用示例）
11. 单元测试覆盖率 ≥ 80%
