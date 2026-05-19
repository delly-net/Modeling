# Delly.Modeling

一个 .NET 建模库，提供源代码生成功能，用于对象建模。通过源生成器在编译时实现类似反射的属性访问和操作。

## 项目结构

- **[Delly.Modeling](./Delly.Modeling/)** - 核心建模库 (.NET Standard 2.0)
- **[Delly.Modeling.Generator](./Delly.Modeling.Generator/)** - Roslyn 源代码生成器
- **[Deme](./Deme/)** - 演示应用程序

## 快速开始

```bash
# 构建解决方案
dotnet build

# 运行演示应用程序
cd Deme
dotnet run
```

## 特性

- 编译时源代码生成实现属性访问
- 支持 AOT (提前编译)
- 无运行时反射开销
- 类似字典的属性访问，同时具备类型安全性

## 许可证
