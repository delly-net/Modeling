# Delly.Modeling

A .NET modeling library that provides source code generation capabilities for object modeling, enabling reflection-like property access and manipulation at compile time through Roslyn Source Generators.

## Features

- **Compile-time generation**: Property access code is generated at compile time, eliminating runtime reflection overhead
- **AOT-compatible**: Works with Ahead-of-Time compilation (e.g., Native AOT)
- **High performance**: Up to 10x faster than traditional reflection-based property access
- **Type safety**: Compile-time type checking while maintaining dictionary-like property access
- **Zero runtime allocation**: Uses static singleton instances for models
- **Framework support**: .NET Standard 2.0 and .NET 6.0+

## Installation

### NuGet Packages

```bash
dotnet add package Delly.Modeling
dotnet add package Delly.Modeling.Generator
```

## Quick Start

Mark your class with the `[Modelable]` attribute and declare it as `partial`:

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

The source generator will automatically generate code that enables property access:

```csharp
// Get the model for the User class
var model = User.GetModel();

// Get all property names
var properties = model.GetProperties();
// Returns array of property model objects

// Get a specific property
var ageProperty = model.GetProperty(nameof(User.Age));

// Get property value
var age = ageProperty?.GetValue(user);

// Set property value
ageProperty?.SetValue(user, 25L);
```

## Supported Types

The library includes built-in models for common types:

- `string` → `StringModel`
- `int` / `Int32` → `Int32Model`
- `long` / `Int64` → `Int64Model`
- `bool` / `Boolean` → `BooleanModel`
- `double` / `Double` → `DoubleModel`
- `decimal` / `Decimal` → `DecimalModel`
- `DateTime` → `DateTimeModel`
- `Guid` → `GuidModel`

## Performance

The source generator approach provides significant performance benefits over reflection:

| Method | 10M Operations | Overhead |
|--------|----------------|----------|
| Reflection | ~600ms | High |
| Source Generator | ~60ms | Minimal |

*Benchmark results may vary depending on hardware and runtime*

## Project Structure

- **[Delly.Modeling](./Delly.Modeling/)** - Core modeling library (.NET Standard 2.0, .NET 6.0)
  - `ModelableAttribute` - Attribute to mark classes for source generation
  - `IModel` - Interface for model objects
  - `IModelProperty` - Interface for model properties
  - Built-in type models in the `Models` namespace

- **[Delly.Modeling.Generator](./Delly.Modeling.Generator/)** - Roslyn Source Generator (.NET Standard 2.0)
  - `ModelableSourceGenerator.cs` - Main source generator implementation
  - `ModelableSyntaxReceiver.cs` - Syntax receiver for collecting [Modelable] classes

- **[Deme](./Deme/)** - Performance benchmark demonstration application (.NET 10.0 with AOT)

- **[NugetDemo](./NugetDemo/)** - NuGet package usage demonstration

## Building and Running

```bash
# Build the solution
dotnet build

# Run the demo application (with benchmark)
cd Deme
dotnet run
```

## How It Works

1. Mark a class with `[Modelable]` and declare it as `partial`
2. The source generator scans for classes with this attribute at compile time
3. Generated code includes:
   - A static `GetModel()` method that returns the model singleton
   - Property model classes implementing `IModelProperty` for each property
   - A main model class implementing `IModel` with property access methods
4. All generated code is emitted to `obj/Generated` and compiled with your project

## License

See LICENSE file for details.