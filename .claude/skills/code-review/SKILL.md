---
name: code-review
description: Delly.Modeling 项目代码审查技能
---

# Delly.Modeling 代码审查

## 项目概述

Delly.Modeling 是一个 .NET 建模库，通过源代码生成器提供对象建模功能。

## 审查要点

### 代码风格

#### 文档注释
- **公共成员**: 必须使用 `///` 语法的 XML 文档注释
  - 必须包含 `<summary>` 和 `<returns>` 标签
  - `param` 名称必须与方法签名中的实际参数名称完全匹配
  ```csharp
  /// <summary>
  /// 公共方法描述
  /// </summary>
  /// <param name="paramName">参数描述，必须匹配实际参数名</param>
  /// <returns>返回值描述</returns>
  public void MyMethod(string paramName)
  ```

- **私有成员**: 仅使用 `//` 注释，在 `//` 后留一个空格
  ```csharp
  // 缓存属性信息
  private PropertyInfo[]? _properties;
  ```

#### if 语句规范
- 必须使用大括号，即使是单行语句
- 单行规则：当整个语句小于 120 字符时，将条件和执行合并为一行
  ```csharp
  // ✅ 正确 - 单行
  if (a) { b(); }

  // ✅ 正确 - 多行（超过 120 字符）
  if (veryLongConditionName && anotherVeryLongCondition)
  {
      ExecuteMethod();
  }

  // ❌ 错误 - 无大括号
  if (obj == null)
      return null;
  ```

#### 可空注解
- 在接口和公共 API 中，对可空对象参数使用 `object?`
- 在项目级别配置可空引用类型，不在源代码文件中使用 `#nullable enable` 指令
- 对于多目标框架项目，使用 MSBuild Condition 仅对支持的框架启用可空
- 对于 `Models/*.cs` 文件，使用条件编译同时支持 .NET Standard 2.0 和现代 .NET

### 架构规范
- 源代码生成优于反射
- 使用单例模式：`Model<T>` 每个类型使用单个静态实例
- 必须使用 `partial` 类来接收生成代码
- 源代码生成器输出到 `obj/Generated` 目录

### 命名规范
- 类：PascalCase
- 方法：PascalCase
- 属性：PascalCase
- 私有字段：_camelCase

### 关键模式
- 使用 `[Modelable]` 属性标记需要建模的类
- 提供基于字符串的属性访问，具有编译时生成功能
- 支持生成的方法：`GetProperties()`、`GetProperty()`、`SetProperty()`

### 项目结构
- **Delly.Modeling**: .NET Standard 2.0，核心建模库
- **Delly.Modeling.Generator**: .NET Standard 2.0，Roslyn 源代码生成器
- **Deme**: .NET 10.0，演示应用（启用 AOT 发布）

### 构建配置
- 所有项目启用可空引用类型
- 启用隐式 using
- 启用 XML 文档生成