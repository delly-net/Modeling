---
name: nuget
description: 分析项目代码并生成 NuGet 包描述和标签到 csproj 文件中
---

# NuGet 包元数据生成器

此技能分析解决方案中的所有 `.csproj` 文件，提取项目元数据、依赖项和代码结构，生成完整的 NuGet 包元数据，然后直接更新 csproj 文件中的 `Description` 和 `PackageTags` 属性。

## 使用方法

```
/nuget
```

## 功能

1. 发现解决方案中的所有 `.csproj` 文件
2. 对每个项目分析：
   - 目标框架
   - 包元数据（版本、图标、仓库 URL 等）
   - 依赖项（PackageReference）
   - 代码结构（类、接口、公共 API）
   - XML 文档摘要
3. 生成：
   - **Description**：基于代码分析的综合包描述
   - **PackageTags**：用于 NuGet 发现的相关标签
4. **直接更新 csproj 文件**，添加或更新 `<Description>` 和 `<PackageTags>` 属性

## 描述生成逻辑

对于**源代码生成器**：
- "Roslyn Source Generator for compile-time code generation. Uses [Attribute] to mark classes for automated code generation. Eliminates runtime reflection overhead with compile-time generation. Supports [TargetFramework] and is AOT-compatible."

对于**库**：
- "[ProjectName] is a .NET modeling library providing compile-time property access capabilities. [Interface summary] Uses [Attribute] to mark classes for source generation. Target Framework: [TargetFramework]. AOT-compatible."

## 标签生成逻辑

标签基于以下内容生成：
- **框架**：`dotnet-standard`, `netstandard`, `dotnet-8`, `dotnet-9`, `dotnet-10`
- **特性**：`roslyn`, `source-generator`, `code-generation`, `compiler`, `analyzer`, `aot`, `native-aot`, `trimming`
- **代码模式**：`modeling`, `reflection`, `property-access`, `dynamic`, `attributes`, `interfaces`, `generics`
- **语言**：`csharp`, `dotnet`

## 跳过的项目

- 匹配以下模式的项目：`*Test*`, `*.Test`, `Demo`, `Example`, `Deme`

## 输出

对每个处理的项目，显示：
- 项目名称和 csproj 路径
- 生成的描述
- 生成的标签
- 成功/失败状态

输出示例：
```
[SUCCESS] Delly.Modeling
  File: Delly.Modeling/Delly.Modeling.csproj
  Description: Delly.Modeling is a .NET modeling library providing compile-time property access capabilities...
  Tags: aot, attributes, csharp, dotnet, dotnet-standard, generics, interfaces, modeling, native-aot, netstandard, property-access, reflection, trimming
```