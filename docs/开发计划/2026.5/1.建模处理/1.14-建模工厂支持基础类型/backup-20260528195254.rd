# 1.14 建模工厂支持基础类型 - 原始备份

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
- 基础类型建模实现 `IBaseModel` 接口，而非 `IEntityModel` 接口
- 由于接口差异，基础类型建模不通过 `AddSet` 方法注册，而是直接添加到内部字典

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
- 提供 `Add(IEntityModel)` 方法添加单个实体模型
- 提供 `AddSet(IEntityModelSet)` 方法批量添加建模集合
- 查询方法按类全称或表名查找模型

### 接口差异

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

**IEntityModel** (实体模型接口)：
- 继承自 `IModel`
- 包含表名、类名、属性集合、索引集合等额外功能
- 用于数据库实体和查询对象的建模

### 新增需求

在 `DefaultEntityModelFactory` 构造函数中自动注册所有基础类型建模：
- 由于基础类型建模实现 `IBaseModel` 而非 `IEntityModel`，需要特殊处理
- TODO: 确认是添加独立的内部字典存储基础类型建模，还是复用现有 `_models` 字典
- TODO: 确认查询方法是否需要返回基础类型建模

## 实现方案

### 1. 存储方案选择

**方案 A：独立字典**
```csharp
private readonly Dictionary<string, IBaseModel> _baseModels;
```
- 优点：类型安全，职责清晰
- 缺点：需要额外的查询方法

**方案 B：统一字典**
```csharp
private readonly Dictionary<string, IBaseModel> _models;
```
- 优点：统一查询接口
- 缺点：失去类型安全性

TODO: 确认使用哪种方案（建议：方案 A）

### 2. 构造函数初始化

在构造函数中添加所有基础类型建模：

```csharp
public DefaultEntityModelFactory()
{
    _models = new Dictionary<string, IEntityModel>();
    _modelSets = new List<IEntityModelSet>();
    _registeredSets = new HashSet<IEntityModelSet>();
    _baseModels = new Dictionary<string, IBaseModel>();

    // 注册基础类型建模
    RegisterBaseTypeModels();
}

private void RegisterBaseTypeModels()
{
    // StringModel
    _baseModels[string.Empty] = StringModel.Instance;
    _baseModels["System.String"] = StringModel.Instance;

    // Int32Model
    _baseModels["System.Int32"] = Int32Model.Instance;

    // Int64Model
    _baseModels["System.Int64"] = Int64Model.Instance;

    // BooleanModel
    _baseModels["System.Boolean"] = BooleanModel.Instance;

    // DoubleModel
    _baseModels["System.Double"] = DoubleModel.Instance;

    // DecimalModel
    _baseModels["System.Decimal"] = DecimalModel.Instance;

    // DateTimeModel
    _baseModels["System.DateTime"] = DateTimeModel.Instance;

    // GuidModel
    _baseModels["System.Guid"] = GuidModel.Instance;
}
```

### 3. TODO: 待确认项

1. **存储方案**: 使用独立字典还是统一字典存储基础类型建模？
   - TODO: 确认（建议：使用独立字典 `_baseModels`）

2. **查询方法**: 是否需要新增 `GetBaseModel` 方法？
   - TODO: 确认是否需要区分查询实体模型和基础类型建模

3. **StringModel 类全称**: String 的类全称是使用 `string.Empty` 还是 `System.String`？
   - TODO: 确认（建议：同时支持两个键）

4. **泛型方法支持**: `GetModel<T>()` 方法是否需要支持基础类型？
   - TODO: 确认（建议：使用 typeof + switch case 模式匹配）

## 使用示例

```csharp
// 创建工厂实例（自动注册所有基础类型建模）
var factory = new DefaultEntityModelFactory();

// TODO: 根据确认方案调整查询方式
// 方案 A - 独立字典查询
var stringModel = factory.GetBaseModel("System.String");
var intModel = factory.GetBaseModel("System.Int32");

// 方案 B - 统一字典查询
var stringModel = factory.GetModel("System.String");

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

- [ ] 确认存储方案（独立字典 vs 统一字典）
- [ ] 确认查询方法设计
- [ ] 确认 StringModel 类全称键值
- [ ] 确认泛型方法是否支持基础类型
- [ ] 在 `DefaultEntityModelFactory` 中添加 `_baseModels` 字段（如采用方案 A）
- [ ] 实现 `RegisterBaseTypeModels` 私有方法
- [ ] 在构造函数中调用 `RegisterBaseTypeModels`
- [ ] 添加查询方法（如 `GetBaseModel`，如采用方案 A）
- [ ] 更新 XML 文档注释
- [ ] 在 Deme 项目中添加示例代码
- [ ] 更新单元测试
- [ ] 验证所有基础类型建模正确注册
- [ ] 验证查询方法返回正确结果

## 参考文档

- [DefaultEntityModelFactory.cs](../../../Delly.Modeling/DefaultEntityModelFactory.cs) - 需要修改的工厂类
- [IBaseModel.cs](../../../Delly.Modeling/IBaseModel.cs) - 基础模型接口
- [IEntityModel.cs](../../../Delly.Modeling/IEntityModel.cs) - 实体模型接口
- [StringModel.cs](../../../Delly.Modeling/Models/StringModel.cs) - String 建模参考
- [Int32Model.cs](../../../Delly.Modeling/Models/Int32Model.cs) - Int32 建模参考
- [代码规范.md](../../../docs/开发指南/代码规范.md) - 代码规范参考
- [注意事项.md](../../../docs/开发指南/注意事项.md) - 开发注意事项

## 验收标准

1. `DefaultEntityModelFactory` 构造函数自动注册所有 8 个基础类型建模
2. 基础类型建模使用正确的类全称作为键
3. 查询方法能正确返回已注册的基础类型建模
4. 生成的代码编译通过，无警告
5. XML 文档注释完整
6. Deme 项目中的示例能正常运行
7. 单元测试覆盖率 ≥ 80%
