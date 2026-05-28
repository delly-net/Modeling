# 1.16 nullable规范定义

## 任务信息

- **任务号**: 1.16
- **任务名称**: nullable规范定义
- **责任人**: 郑琳
- **任务类型**: 代码优化
- **任务描述**: 在 `*.csproj` 文件中定义 `nullable`，而非在代码中使用`#nullable enable`来定义

## 功能概述

将项目中分散在各代码文件中的 `#nullable enable` 指令统一移除，改为在项目文件 `*.csproj` 中通过 `<Nullable>enable</Nullable>` 属性进行统一配置。此任务旨在简化代码维护，统一可空引用类型配置管理。

**重要约束**：
- 修改范围为 `Delly.Modeling` 和 `Delly.Modeling.Generator` 项目的所有代码文件
- 移除所有代码文件中的 `#nullable enable` 和 `#nullable disable` 指令
- 移除代码文件中相关的条件编译 `#if !NETSTANDARD2_0` 包裹的 nullable 指令
- 保持条件编译的其他功能部分不变（如方法签名）
- 确保修改后代码编译通过且功能正常
- 源生成器生成的代码模板仍需保留条件编译的 nullable 指令

## 技术背景

### 现有实现

#### 1. csproj 文件现状

**Delly.Modeling/Delly.Modeling.csproj** - 当前无 `<Nullable>` 设置：
```xml
<PropertyGroup>
  <TargetFrameworks>netstandard2.0;net6.0</TargetFrameworks>
  <GenerateDocumentationFile>True</GenerateDocumentationFile>
  <!-- ... -->
</PropertyGroup>
```

**Delly.Modeling.Generator/Delly.Modeling.Generator.csproj** - 当前无 `<Nullable>` 设置：
```xml
<PropertyGroup>
  <TargetFramework>netstandard2.0</TargetFramework>
  <LangVersion>latest</LangVersion>
  <ImplicitUsings>enable</ImplicitUsings>
  <!-- ... -->
</PropertyGroup>
```

#### 2. 代码文件中的 nullable 指令

项目中有多处使用 `#nullable enable` 指令，主要分布在：

| 文件类型 | 数量 | 示例位置 |
|:--------|:----|:--------|
| Models/*.cs | 约16个 | `Delly.Modeling/Models/*.cs` |
| Interfaces/*.cs | 约8个 | `Delly.Modeling/Interfaces/*.cs` |
| Generator/*.cs | 约6个 | `Delly.Modeling.Generator/*.cs` |
| 其他文件 | 约3个 | `DefaultEntityModelFactory.cs`, `ModelUtils.cs` 等 |

**代码示例（现有实现）**：
```csharp
#if !NETSTANDARD2_0
#nullable enable
#endif

public sealed class Int32Model : IBaseModel
{
#if !NETSTANDARD2_0
    /// <summary>
    /// 尝试将输入对象解析为 Int32 实例
    /// </summary>
    /// <param name="obj">输入对象</param>
    /// <returns>Int32 实例，解析失败时返回 null</returns>
    public object? TryParse(object? obj)
#else
    /// <summary>
    /// 尝试将输入对象解析为 Int32 实例
    /// </summary>
    /// <param name="obj">输入对象</param>
    /// <returns>Int32 实例，解析失败时返回 null</returns>
    public object TryParse(object obj)
#endif
    {
        // 实现
    }
}
```

### 新增需求

#### 1. 统一在 csproj 中配置

**Delly.Modeling/Delly.Modeling.csproj** 修改：
```xml
<PropertyGroup>
  <TargetFrameworks>netstandard2.0;net6.0</TargetFrameworks>
  <Nullable>enable</Nullable>
  <!-- ... -->
</PropertyGroup>
```

**Delly.Modeling.Generator/Delly.Modeling.Generator.csproj** 修改：
```xml
<PropertyGroup>
  <TargetFramework>netstandard2.0</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <!-- ... -->
</PropertyGroup>
```

#### 2. 代码文件修改

移除所有代码文件中的 nullable 指令，包括：
- 移除 `#nullable enable`
- 移除 `#nullable disable`
- 移除包裹 nullable 指令的条件编译 `#if !NETSTANDARD2_0`

**修改后代码示例**：
```csharp
public sealed class Int32Model : IBaseModel
{
#if !NETSTANDARD2_0
    /// <summary>
    /// 尝试将输入对象解析为 Int32 实例
    /// </summary>
    /// <param name="obj">输入对象</param>
    /// <returns>Int32 实例，解析失败时返回 null</returns>
    public object? TryParse(object? obj)
#else
    /// <summary>
    /// 尝试将输入对象解析为 Int32 实例
    /// </summary>
    /// <param name="obj">输入对象</param>
    /// <returns>Int32 实例，解析失败时返回 null</returns>
    public object TryParse(object obj)
#endif
    {
        // 实现
    }
}
```

#### 3. 源生成器代码模板处理

**源生成器生成的代码**需要保留 nullable 指令，因为：
1. 生成代码的目标项目可能没有启用 nullable
2. 生成代码应具有自包含的 nullable 配置
3. 确保生成代码在各种环境下都能正确工作

**源生成器代码模板保持不变**：
```csharp
// 源生成器中生成的代码模板
public class ModelableSourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        // 生成代码时保留 #nullable enable
        var code = $$"""
            #if !NETSTANDARD2_0
            #nullable enable
            #endif

            public sealed class emc__User : IBaseModel
            {
                // ...
            }
            """;
    }
}
```

### .NET 版本兼容性说明

**关键问题**：.NET Standard 2.0 编译器不支持 nullable 引用类型。

**解决方案**：
1. `<Nullable>enable</Nullable>` 仅对 `net6.0` 目标框架生效
2. `netstandard2.0` 目标框架编译时忽略 nullable 配置
3. 条件编译 `#if !NETSTANDARD2_0` 仍用于区分方法签名

**编译行为**：
```
netstandard2.0 目标: 编译器忽略 nullable，使用非空类型签名
net6.0 目标:       编译器启用 nullable，使用可空类型签名
```

## 实现方案

### 1. 具体步骤

#### 步骤1：修改 csproj 文件

**1.1 修改 Delly.Modeling/Delly.Modeling.csproj**
- 在 `<PropertyGroup>` 中添加 `<Nullable>enable</Nullable>`
- 位置：在 `<TargetFrameworks>` 之后

**1.2 修改 Delly.Modeling.Generator/Delly.Modeling.Generator.csproj**
- 在 `<PropertyGroup>` 中添加 `<Nullable>enable</Nullable>`
- 位置：在 `<TargetFramework>` 之后

#### 步骤2：移除代码文件中的 nullable 指令

**2.1 Delly.Modeling/Models/** - 移除 nullable 指令
- 处理约16个 Model 类文件
- 移除文件顶部 `#if !NETSTANDARD2_0` 包裹的 `#nullable enable`
- 保留方法签名相关的条件编译

**2.2 Delly.Modeling/Interfaces/** - 移除 nullable 指令
- 处理约8个接口文件
- 移除文件顶部的 nullable 指令

**2.3 Delly.Modeling.Generator/** - 移除 nullable 指令
- 处理约6个源生成器文件
- 移除文件顶部的 nullable 指令
- **保留**生成代码模板中的 nullable 指令

**2.4 Delly.Modeling/ 根目录文件** - 移除 nullable 指令
- 处理约3个文件
- `DefaultEntityModelFactory.cs`
- `ModelUtils.cs`
- `ParsableAttribute.cs`

#### 步骤3：验证和测试

**3.1 编译验证**
- 运行 `dotnet build` 验证所有目标框架编译通过
- 检查是否有新的 nullable 警告产生

**3.2 功能验证**
- 运行单元测试确认功能正常
- 运行 Deme 项目验证功能

### 2. TODO: 待确认项

- [x] **确认 .NET Standard 2.0 目标框架的行为**（已确认：编译器忽略 nullable 配置）
- [x] **确认是否需要在 csproj 中使用条件编译区分不同目标框架的 nullable 配置**（已确认：不需要，使用 `<Nullable>enable</Nullable>` 全局配置即可）
- [ ] **确认源生成器生成的代码是否需要保留 nullable 指令**（建议：保留，确保生成代码的自包含性）
- [ ] **确认是否需要为不同目标框架设置不同的 nullable 配置**（建议：不需要，统一使用 enable）

## 使用示例

### 修改前后对比

#### csproj 文件对比

**修改前**：
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>netstandard2.0;net6.0</TargetFrameworks>
    <GenerateDocumentationFile>True</GenerateDocumentationFile>
  </PropertyGroup>
</Project>
```

**修改后**：
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>netstandard2.0;net6.0</TargetFrameworks>
    <Nullable>enable</Nullable>
    <GenerateDocumentationFile>True</GenerateDocumentationFile>
  </PropertyGroup>
</Project>
```

#### 代码文件对比

**修改前**：
```csharp
#if !NETSTANDARD2_0
#nullable enable
#endif

public sealed class Int32Model : IBaseModel
{
#if !NETSTANDARD2_0
    public object? TryParse(object? obj)
#else
    public object TryParse(object obj)
#endif
    {
        // 实现
    }
}
```

**修改后**：
```csharp
public sealed class Int32Model : IBaseModel
{
#if !NETSTANDARD2_0
    public object? TryParse(object? obj)
#else
    public object TryParse(object obj)
#endif
    {
        // 实现
    }
}
```

## 实现步骤

### 开发任务清单

- [ ] **csproj 文件修改**
  - [ ] Delly.Modeling/Delly.Modeling.csproj - 添加 `<Nullable>enable</Nullable>`
  - [ ] Delly.Modeling.Generator/Delly.Modeling.Generator.csproj - 添加 `<Nullable>enable</Nullable>`

- [ ] **Delly.Modeling/Models/** - 移除 nullable 指令（约16个文件）
  - [ ] StringModel.cs
  - [ ] Int32Model.cs
  - [ ] Int64Model.cs
  - [ ] BooleanModel.cs
  - [ ] DoubleModel.cs
  - [ ] DecimalModel.cs
  - [ ] DateTimeModel.cs
  - [ ] GuidModel.cs
  - [ ] EntityModels/StringEntityModel.cs
  - [ ] EntityModels/Int32EntityModel.cs
  - [ ] EntityModels/Int64EntityModel.cs
  - [ ] EntityModels/BooleanEntityModel.cs
  - [ ] EntityModels/DoubleEntityModel.cs
  - [ ] EntityModels/DecimalEntityModel.cs
  - [ ] EntityModels/DateTimeEntityModel.cs
  - [ ] EntityModels/GuidEntityModel.cs

- [ ] **Delly.Modeling/Interfaces/** - 移除 nullable 指令（约8个文件）
  - [ ] IBaseModel.cs
  - [ ] IModel.cs
  - [ ] IModelProperty.cs
  - [ ] IEntityModel.cs
  - [ ] IEntityModelProperty.cs
  - [ ] IEntityModelSet.cs
  - [ ] IEntityModelFactory.cs
  - [ ] IParsable.cs

- [ ] **Delly.Modeling.Generator/** - 移除 nullable 指令（约6个文件）
  - [ ] ModelableSourceGenerator.cs - **保留**生成代码模板中的 nullable 指令
  - [ ] ModelableSyntaxReceiver.cs
  - [ ] ModelTableSourceGenerator.cs - **保留**生成代码模板中的 nullable 指令
  - [ ] ModelTableSyntaxReceiver.cs
  - [ ] ModelSetSourceGenerator.cs - **保留**生成代码模板中的 nullable 指令
  - [ ] ModelSetSyntaxReceiver.cs

- [ ] **Delly.Modeling 根目录** - 移除 nullable 指令（约3个文件）
  - [ ] DefaultEntityModelFactory.cs
  - [ ] ModelUtils.cs
  - [ ] ParsableAttribute.cs

- [ ] **验证**
  - [ ] 运行 `dotnet build` 确认所有目标框架编译通过
  - [ ] 运行单元测试确认功能正常
  - [ ] 运行 Deme 项目验证功能

## 参考文档

- [代码规范.md](../../开发指南/代码规范.md) - 代码规范参考
- [注意事项.md](../../开发指南/注意事项.md) - 开发注意事项，包含 .NET 版本兼容性规范
- [功能清单.md](../功能清单.md) - 任务清单
- [1.15-代码规范整理](./1.15-代码规范整理.md) - 代码规范整理参考
- [1.10-编译警告处理](./1.10-编译警告处理.md) - 编译警告处理参考

## 验收标准

1. `Delly.Modeling.csproj` 包含 `<Nullable>enable</Nullable>` 配置
2. `Delly.Modeling.Generator.csproj` 包含 `<Nullable>enable</Nullable>` 配置
3. 所有代码文件中的 `#nullable enable` 指令已移除
4. 所有代码文件中的 `#nullable disable` 指令已移除
5. 条件编译 `#if !NETSTANDARD2_0` 包裹的 nullable 指令已移除
6. 方法签名相关的条件编译保持不变
7. 源生成器代码模板中的 nullable 指令保持不变
8. `dotnet build` 所有目标框架编译通过
9. 单元测试全部通过
10. Deme 项目正常运行
