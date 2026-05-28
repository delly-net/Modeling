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
- 基础类型建模实现 `IBaseModel` 接口，存储到统一字典 `_models` 中

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

## 实现方案

### 1. 修改字段类型

将 `_models` 字段类型从 `IEntityModel` 改为 `IBaseModel`，统一存储实体模型和基础类型建模：

```csharp
// 修改前
private readonly Dictionary<string, IEntityModel> _models;

// 修改后
private readonly Dictionary<string, IBaseModel> _models;
```

### 2. 添加静态 Type 字段

在类中添加基础类型的静态 Type 字段，用于泛型方法匹配：

```csharp
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
    // StringModel - 同时支持两个键
    _models[string.Empty] = StringModel.Instance;
    _models["System.String"] = StringModel.Instance;

    // Int32Model
    _models["System.Int32"] = Int32Model.Instance;

    // Int64Model
    _models["System.Int64"] = Int64Model.Instance;

    // BooleanModel
    _models["System.Boolean"] = BooleanModel.Instance;

    // DoubleModel
    _models["System.Double"] = DoubleModel.Instance;

    // DecimalModel
    _models["System.Decimal"] = DecimalModel.Instance;

    // DateTimeModel
    _models["System.DateTime"] = DateTimeModel.Instance;

    // GuidModel
    _models["System.Guid"] = GuidModel.Instance;
}
```

### 4. 修改构造函数

在构造函数中调用注册方法：

```csharp
public DefaultEntityModelFactory()
{
    _models = new Dictionary<string, IBaseModel>();
    _modelSets = new List<IEntityModelSet>();
    _registeredSets = new HashSet<IEntityModelSet>();

    // 注册基础类型建模
    RegisterBaseTypeModels();
}
```

### 5. 扩展泛型方法

使用 `typeof + switch case` 模式匹配支持基础类型：

```csharp
public IBaseModel? GetModel<T>() where T : class
{
    return typeof(T) switch
    {
        var t when t == _stringType => _models.GetValueOrDefault("System.String"),
        var t when t == _int32Type => _models.GetValueOrDefault("System.Int32"),
        var t when t == _int64Type => _models.GetValueOrDefault("System.Int64"),
        var t when t == _booleanType => _models.GetValueOrDefault("System.Boolean"),
        var t when t == _doubleType => _models.GetValueOrDefault("System.Double"),
        var t when t == _decimalType => _models.GetValueOrDefault("System.Decimal"),
        var t when t == _dateTimeType => _models.GetValueOrDefault("System.DateTime"),
        var t when t == _guidType => _models.GetValueOrDefault("System.Guid"),
        _ => GetModel(typeof(T).FullName ?? string.Empty)
    };
}
```

### 6. 更新 XML 文档注释

确保所有公共成员都有完整的 XML 文档注释。

## 使用示例

```csharp
// 创建工厂实例（自动注册所有基础类型建模）
var factory = new DefaultEntityModelFactory();

// 按类全称查询
var stringModel = factory.GetModel("System.String");
var intModel = factory.GetModel("System.Int32");

// 使用泛型方法查询
var stringModel2 = factory.GetModel<string>();
var intModel2 = factory.GetModel<int>();

// 使用基础类型建模
var str = stringModel.CreateInstance("hello");
var parsed = stringModel.Parse(123); // "123"

// 判断是否为值类型
if (stringModel.IsValue)
{
    Console.WriteLine("String 是值类型对象");
}
```

## 实现步骤

### 开发任务清单

- [ ] 修改 `_models` 字段类型从 `Dictionary<string, IEntityModel>` 改为 `Dictionary<string, IBaseModel>`
- [ ] 添加基础类型静态 Type 字段
- [ ] 实现 `RegisterBaseTypeModels` 私有方法
- [ ] 在构造函数中调用 `RegisterBaseTypeModels`
- [ ] 扩展 `GetModel<T>()` 方法支持基础类型（使用 typeof + switch case）
- [ ] 更新相关方法的 XML 文档注释
- [ ] 在 Deme 项目中添加示例代码
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

- 方法名使用 PascalCase
- 私有字段使用 _camelCase
- 辅助方法使用清晰描述性名称

## 参考文档

- [DefaultEntityModelFactory.cs](../../../Delly.Modeling/DefaultEntityModelFactory.cs) - 需要修改的工厂类
- [IBaseModel.cs](../../../Delly.Modeling/IBaseModel.cs) - 基础模型接口
- [StringModel.cs](../../../Delly.Modeling/Models/StringModel.cs) - String 建模参考
- [Int32Model.cs](../../../Delly.Modeling/Models/Int32Model.cs) - Int32 建模参考
- [代码规范.md](../../../docs/开发指南/代码规范.md) - 代码规范参考
- [注意事项.md](../../../docs/开发指南/注意事项.md) - 开发注意事项

## 验收标准

1. `DefaultEntityModelFactory` 构造函数自动注册所有 8 个基础类型建模
2. 基础类型建模使用正确的类全称作为键
3. 查询方法能正确返回已注册的基础类型建模
4. `GetModel<T>()` 方法支持所有基础类型
5. 生成的代码编译通过，无警告
6. XML 文档注释完整
7. Deme 项目中的示例能正常运行
8. 单元测试覆盖率 ≥ 80%
