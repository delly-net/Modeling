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