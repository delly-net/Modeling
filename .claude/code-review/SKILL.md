# Delly.Modeling 项目代码审查

此技能用于审查 Delly.Modeling 项目的代码变更。

## 使用方法

```
/code-review
```

## 审查范围

审查以下项目的代码变更：
- Delly.Modeling - 核心建模库
- Delly.Modeling.Generator - Roslyn 源代码生成器
- Deme - 演示应用程序
- NugetDemo - NuGet 包演示

## 审查重点

### 1. 文档注释规范

**公共成员**：
- 所有公共类、方法、属性和接口必须有 XML 文档注释 (`///`)
- XML `param` 名称必须与方法签名中的参数名称完全匹配
- 必须包含 `<returns>` 标签说明返回值

```csharp
/// <summary>
/// 获取指定属性的值
/// </summary>
/// <param name="model">模型实例</param>
/// <param name="instance">对象实例</param>
/// <param name="name">属性名称</param>
/// <returns>属性值</returns>
public static object? GetProperty(Model model, object instance, string name)
```

**私有成员**：
- 仅使用 `//` 注释（注意 `//` 后必须有一个空格）
- 不需要文档注释

### 2. 代码风格规范

**if 语句**：
- 必须使用大括号包裹，即使只有一行代码

```csharp
// ✅ 正确
if (obj == null)
{
    return null;
}

// ❌ 错误
if (obj == null)
    return null;
```

**命名约定**：
- 类名、方法名、属性名：PascalCase
- 私有字段：_camelCase

### 3. 可空注解规范

- 在接口和公共 API 中使用 `object?` 表示可空对象参数
- 对于 .NET Standard 2.0 兼容性，使用条件编译：
  ```csharp
  #if NETSTANDARD2_0
  object GetProperty(string name);
  #else
  object? GetProperty(string name);
  #endif
  ```

### 4. Models/*.cs 特殊规范

**适用范围**：`Delly.Modeling/Models/` 目录下的 Model 类文件

**实现方式**：使用条件编译区分不同 .NET 版本

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

### 5. 源代码生成规范

- 生成代码输出到 `obj/Generated` 目录
- 生成的类使用 `partial` 关键字
- 为生成的扩展方法提供完整的 XML 文档注释

### 6. 建模相关规范

- 类必须标记 `[Modelable]`、`[MoTable]`、`[MoQuery]` 或 `[MoSet]` 属性
- 类必须声明为 `partial`
- 支持主构造函数语法

## 输出

返回代码审查结果，包括发现的问题和建议。
