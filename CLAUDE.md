# CLAUDE.md

## Project Overview

Delly.Modeling is a .NET modeling library that provides source code generation capabilities for object modeling. It enables reflection-like property access and manipulation at compile time through Source Generators.

## Architecture

The solution consists of three projects:

### Delly.Modeling
- **Framework**: .NET Standard 2.0
- **Purpose**: Core modeling library
- **Key Components**:
  - `ModelableAttribute`: Marks classes that should be modeled
  - `Model<T>`: Model object singleton for type T
  - `ModelUtils`: Static utilities for model access

### Delly.Modeling.Generator
- **Framework**: .NET Standard 2.0
- **Purpose**: Roslyn Source Generator
- **Dependencies**:
  - Microsoft.CodeAnalysis.CSharp 4.12.0
  - Microsoft.CodeAnalysis.Analyzers 3.3.4
- **Capabilities**: Generates property accessors and extension methods for `[Modelable]` classes

### Deme
- **Framework**: .NET 10.0
- **Purpose**: Demonstration application
- **Configuration**:
  - AOT publishing enabled (`PublishAot`)
  - Compiler generated files output to `obj/Generated`
  - Trimmer enabled for minimal deployment size

## Usage Pattern

Classes marked with `[Modelable]` attribute receive generated extension methods:

```csharp
[Modelable]
public partial class User(string id)
{
    public string Id { get; } = id;
    public string Name { get; set; } = string.Empty;
    public string? Password { get; set; }
}
```

Generated methods include:
- `GetProperties()`: Returns array of property names
- `GetProperty(model, instance, name)`: Gets property value by name
- `SetProperty(model, instance, name, value)`: Sets property value by name

## Key Design Decisions

1. **Source Generation over Reflection**: Compile-time generation enables AOT compatibility and better performance
2. **Singleton Pattern**: `Model<T>` uses a single static instance per type
3. **Partial Classes**: Enables separation of user code and generated code
4. **String-based Property Access**: Provides dictionary-like property access with compile-time generation

## Development Guidelines

- Classes intended for modeling must be marked with `[Modelable]`
- Classes must be declared as `partial` to receive generated code
- Source generator output is emitted to `obj/Generated` (check this directory for generated code)
- The generator uses Roslyn's CSharp syntax APIs
- Generated code must use `partial` keyword and provide complete XML documentation comments

## Code Style & Comment Standards

### Documentation Comments

- **Public members**: All public classes, methods, properties, and interfaces MUST have XML documentation comments using `///` syntax
  - Example:
    ```csharp
    /// <summary>
    /// Public method description
    /// </summary>
    /// <param name="paramName">Parameter description matching actual parameter name</param>
    /// <returns>Return value description</returns>
    public void MyMethod(string paramName)
    ```
- **Private members**: Only use `//` comments for private members to reduce code volume
  - Leave one space after `//` symbol before the comment content
  - Example:
    ```csharp
    // Cache property information
    private PropertyInfo[]? _properties;
    ```
- **Parameter matching**: XML `param` names must exactly match actual parameter names in the method signature

### Code Style

- **if statements**: Must use braces, even for single-line statements
  - Single-line rule: When the entire statement is under 120 characters, merge condition and execution into one line
  ```csharp
  // ✅ Correct - Single line
  if (a) { b(); }

  // ✅ Correct - Multi line (over 120 chars)
  if (veryLongConditionName && anotherVeryLongCondition)
  {
      ExecuteMethod();
  }

  // ❌ Incorrect - No braces
  if (obj == null)
      return null;
  ```

### Nullable Annotations

- Use `object?` for nullable object parameters in interfaces and public APIs
- Configure nullable reference types at project level in `*.csproj` using `<Nullable>enable</Nullable>`
- For multi-target framework projects, use MSBuild Condition to enable nullable only for supported frameworks:
  ```xml
  <!-- Only enable Nullable for supported frameworks -->
  <PropertyGroup Condition="'$(TargetFramework)'!='netstandard2.0'">
    <Nullable>enable</Nullable>
  </PropertyGroup>
  ```
- For `Models/*.cs` files, use conditional compilation to support both .NET Standard 2.0 and modern .NET:
  ```csharp
  #if !NETSTANDARD2_0
  public object? TryParse(object? obj)
  #else
  public object TryParse(object obj)
  #endif
  {
      // Implementation
  }
  ```
- Source generator code templates should retain `#nullable enable` directives for generated code
- **Note**: Do not use `#nullable enable` directives in source code files; configure at project level instead
- **Note**: Pure .NET Standard 2.0 projects (like Delly.Modeling.Generator) don't need `<Nullable>enable</Nullable>` configuration

### Naming Conventions

- Classes: PascalCase
- Methods: PascalCase
- Properties: PascalCase
- Private fields: _camelCase

## Build & Run

```bash
# Build solution
dotnet build

# Run demo
cd Deme
dotnet run
```

## Notes

- The generator project is referenced as an Analyzer in the demo project
- All projects use nullable reference types enabled
- Implicit usings are enabled across projects
- Documentation generation is enabled for XML documentation