---
name: code-review
description: Delly.Modeling 项目代码审查技能
---

# Delly.Modeling 代码审查

## 项目概述

Delly.Modeling 是一个 .NET 建模库，通过源代码生成器提供对象建模功能。

## 审查要点

### 代码风格
- 公共成员必须有 XML 文档注释 (`///`)
- 私有成员仅使用 `//` 注释
- XML `param` 名称必须与方法签名中的实际参数名称完全匹配
- 可空注解使用 `object?` 格式

### 架构规范
- 源代码生成优于反射
- 使用单例模式：`Model<T>` 每个类型使用单个静态实例
- 必须使用 `partial` 类来接收生成代码
- 源代码生成器输出到 `obj/Generated` 目录

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