---
name: readme
description: 扫描整个项目目录并生成中英文 README 文件
---

# README 生成器

此技能自动扫描整个项目目录，为项目根目录和指定的子项目生成 README 文件。

## 使用方法

```
/readme
```

## 功能

1. 扫描项目结构以识别所有 `.csproj` 项目
2. 分析所有.cs代码源文件、项目配置和依赖项
3. 为根目录生成 `README.md`（英文）和 `README.zh-CN.md`（中文）
4. 为以下项目子目录创建单独的 README 文件：
   - `Delly.Modeling.Generator/README.md`

## 输出

返回所有生成的 README 文件摘要及其路径。