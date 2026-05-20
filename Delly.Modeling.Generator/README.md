# Delly.Modeling.Generator

Roslyn Source Generator for Delly.Modeling - provides compile-time code generation for object modeling.

## Overview

This source generator automatically generates code for classes marked with the `[Modelable]` attribute, enabling reflection-like property access with zero runtime overhead.

## Framework

- **Target Framework:** .NET Standard 2.0
- **Language Version:** Latest C#

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.CodeAnalysis.CSharp | 4.12.0 | C# compilation and syntax analysis |
| Microsoft.CodeAnalysis.Analyzers | 3.3.4 | Roslyn analyzer support |

## How It Works

The generator consists of two main components:

### 1. ModelableSyntaxReceiver

Collects class declarations that have the `[Modelable]` attribute during syntax tree traversal.

**Location:** [ModelableSyntaxReceiver.cs](./ModelableSyntaxReceiver.cs)

### 2. ModelableSourceGenerator

The main generator that:
- Scans for classes with `[Modelable]` attribute
- Analyzes properties and their types
- Generates extension methods for property access
- Outputs generated code to `obj/Generated`

**Location:** [ModelableSourceGenerator.cs](./ModelableSourceGenerator.cs)

## Generated Code Structure

For a class named `User` with properties, the generator creates:

### 1. GetModel() Extension Method

```csharp
public partial class User
{
    public static c__UserModel GetModel() => c__UserModel.Instance;
}
```

### 2. Property Model Classes

For each property, a class implementing `IModelProperty`:

```csharp
public class p__UserModel_Age : IModelProperty
{
    public string Name => nameof(User.Age);
    public IModel PropertyModel => Int64Model.Instance;
    public object? GetValue(object? obj) { ... }
    public void SetValue(object? obj, object? value) { ... }
}
```

### 3. Main Model Class

```csharp
public class c__UserModel : IModel
{
    public static c__UserModel Instance => _instance;
    public string Name => "UserModel";
    public string Namespace => "...";
    public IModelProperty[] GetProperties() { ... }
    public IModelProperty? GetProperty(string name) { ... }
}
```

## Type Support

The generator handles the following types with appropriate conversion:

| Type | Model Class | Conversion Expression |
|------|-------------|----------------------|
| `string` | StringModel | `value?.ToString()` |
| `int` / `Int32` | Int32Model | `Convert.ToInt32(value)` |
| `long` / `Int64` | Int64Model | `Convert.ToInt64(value)` |
| `bool` / `Boolean` | BooleanModel | `Convert.ToBoolean(value)` |
| `double` / `Double` | DoubleModel | `Convert.ToDouble(value)` |
| `decimal` / `Decimal` | DecimalModel | `Convert.ToDecimal(value)` |
| `DateTime` | DateTimeModel | `Convert.ToDateTime(value)` |
| `Guid` | GuidModel | `Guid.Parse(value?.ToString())` |

## Special Handling

- **Read-only properties**: `SetValue()` throws `NotSupportedException`
- **Nullable reference types**: Proper null checking with `NoNullAllowedException` for non-nullable values
- **Nullable value types**: Automatically unwrapped to their underlying types

## Integration

The generator is packaged as a Roslyn analyzer and automatically integrates with projects that reference the `Delly.Modeling.Generator` NuGet package.

## License

See LICENSE file in the root repository.