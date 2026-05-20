# Delly.Modeling

一个 .NET 建模库，提供源代码生成功能，用于对象建模。通过 Roslyn 源生成器在编译时实现类似反射的属性访问和操作。

## 特性

- **编译时生成**：属性访问代码在编译时生成，消除了运行时反射开销
- **AOT 兼容**：支持提前编译（例如 Native AOT）
- **高性能**：比传统基于反射的属性访问快 10 倍以上
- **类型安全**：在保持字典式属性访问的同时，提供编译时类型检查
- **零运行时分配**：模型使用静态单例实例
- **框架支持**：.NET Standard 2.0 和 .NET 6.0+

## 安装

### NuGet 包

```bash
dotnet add package Delly.Modeling
dotnet add package Delly.Modeling.Generator
```

## 快速开始

使用 `[Modelable]` 特性标记您的类，并将其声明为 `partial`：

```csharp
using Delly.Modeling;

[Modelable]
public partial class User(string id)
{
    public string Id { get; } = id;
    public string Name { get; set; } = string.Empty;
    public string? Password { get; set; }
    public long Age { get; set; }
}
```

源生成器将自动生成启用属性访问的代码：

```csharp
// 获取 User 类的模型
var model = User.GetModel();

// 获取所有属性名称
var properties = model.GetProperties();
// 返回属性模型对象数组

// 获取特定属性
var ageProperty = model.GetProperty(nameof(User.Age));

// 获取属性值
var age = ageProperty?.GetValue(user);

// 设置属性值
ageProperty?.SetValue(user, 25L);
```

## 支持的类型

库包含常用类型的内置模型：

- `string` → `StringModel`
- `int` / `Int32` → `Int32Model`
- `long` / `Int64` → `Int64Model`
- `bool` / `Boolean` → `BooleanModel`
- `double` / `Double` → `DoubleModel`
- `decimal` / `Decimal` → `DecimalModel`
- `DateTime` → `DateTimeModel`
- `Guid` → `GuidModel`

## 性能

源生成器方法比反射提供了显著的性能优势：

| 方法 | 1000万次操作 | 开销 |
|------|--------------|------|
| 反射 | ~600ms | 高 |
| 源生成器 | ~60ms | 极低 |

*基准测试结果可能因硬件和运行时而异*

## 项目结构

- **[Delly.Modeling](./Delly.Modeling/)** - 核心建模库 (.NET Standard 2.0, .NET 6.0)
  - `ModelableAttribute` - 用于标记类以进行源生成的特性
  - `IModel` - 模型对象接口
  - `IModelProperty` - 模型属性接口
  - `Models` 命名空间中的内置类型模型

- **[Delly.Modeling.Generator](./Delly.Modeling.Generator/)** - Roslyn 源生成器 (.NET Standard 2.0)
  - `ModelableSourceGenerator.cs` - 主要源生成器实现
  - `ModelableSyntaxReceiver.cs` - 收集 [Modelable] 类的语法接收器

- **[Deme](./Deme/)** - 性能基准测试演示应用程序 (.NET 10.0，支持 AOT)

- **[NugetDemo](./NugetDemo/)** - NuGet 包使用演示

## 构建和运行

```bash
# 构建解决方案
dotnet build

# 运行演示应用程序（包含基准测试）
cd Deme
dotnet run
```

## 工作原理

1. 使用 `[Modelable]` 标记类并将其声明为 `partial`
2. 源生成器在编译时扫描具有此特性的类
3. 生成的代码包括：
   - 返回模型单例的静态 `GetModel()` 方法
   - 为每个属性实现 `IModelProperty` 的属性模型类
   - 带有属性访问方法的实现 `IModel` 的主模型类
4. 所有生成的代码都输出到 `obj/Generated` 并与您的项目一起编译

## 许可证

详见 LICENSE 文件。