# README Generator

This skill automatically scans the entire project directory and generates comprehensive README files in both English and Chinese.

## Usage

```
/readme
```

## What it does

1. Scans the project structure to identify all `.csproj` projects
2. Analyzes source files, project configurations, and dependencies
3. Generates `README.md` (English) and `README.zh-CN.md` (Chinese) for the root directory
4. Creates individual README files for each project subdirectory:
   - `Delly.Modeling/README.md` and `Delly.Modeling/README.zh-CN.md`
   - `Delly.Modeling.Generator/README.md` and `Delly.Modeling.Generator/README.zh-CN.md`
   - `Deme/README.md` and `Deme/README.zh-CN.md`

## Output

Returns a summary of all generated README files with their paths.