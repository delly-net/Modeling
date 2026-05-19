# Delly.Modeling

A .NET modeling library that provides source code generation capabilities for object modeling, enabling reflection-like property access and manipulation at compile time through Source Generators.

## Project Structure

- **[Delly.Modeling](./Delly.Modeling/)** - Core modeling library (.NET Standard 2.0)
- **[Delly.Modeling.Generator](./Delly.Modeling.Generator/)** - Roslyn Source Generator
- **[Deme](./Deme/)** - Demonstration application

## Quick Start

```bash
# Build the solution
dotnet build

# Run the demo application
cd Deme
dotnet run
```

## Features

- Compile-time source generation for property access
- AOT-compatible (Ahead-of-Time compilation)
- No runtime reflection overhead
- Dictionary-like property access with type safety

## License
