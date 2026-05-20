#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
README 生成器技能
为项目和每个子项目生成中英文 README 文件
"""

import os
import sys
import re
import json
from pathlib import Path
from typing import List, Dict, Any, Tuple

def get_project_structure(root_path: Path) -> List[Dict[str, Any]]:
    """查找解决方案中的所有项目。"""
    projects = []
    for item in root_path.iterdir():
        if not item.is_dir() or item.name.startswith('.') or item.name in ['bin', 'obj']:
            continue
        csproj_files = list(item.glob('*.csproj'))
        if csproj_files:
            csproj = csproj_files[0]
            files = list(item.glob('*.cs'))
            projects.append({
                'name': item.name,
                'path': item,
                'csproj': csproj,
                'files': files
            })
    return projects


def parse_csproj(csproj_path: Path) -> Dict[str, Any]:
    """解析 .csproj 文件获取元数据。"""
    content = csproj_path.read_text(encoding='utf-8')
    result = {
        'target_framework': '',
        'is_library': True,
        'is_aot_enabled': False,
        'package_references': [],
        'project_references': []
    }

    # 目标框架
    tf_match = re.search(r'<TargetFramework>([^<]+)</TargetFramework>', content)
    if tf_match:
        result['target_framework'] = tf_match.group(1)

    # 输出类型
    if '<OutputType>Exe</OutputType>' in content:
        result['is_library'] = False

    # AOT
    if '<PublishAot>true</PublishAot>' in content:
        result['is_aot_enabled'] = True

    # 包引用
    pkg_matches = re.findall(r'<PackageReference Include="([^"]+)" Version="([^"]+)"', content)
    for pkg, ver in pkg_matches:
        result['package_references'].append({'package': pkg, 'version': ver})

    # 项目引用
    proj_matches = re.findall(r'<ProjectReference Include="([^"]+)"', content)
    result['project_references'] = proj_matches

    return result


def generate_root_readme(root_path: Path, projects: List[Dict[str, Any]]) -> Tuple[Path, Path]:
    """为根目录生成 README 文件。"""
    readme_en = root_path / 'README.md'
    readme_zh = root_path / 'README.zh-CN.md'

    en_content = """# Delly.Modeling

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
"""

    zh_content = """# Delly.Modeling

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
"""

    readme_en.write_text(en_content, encoding='utf-8')
    readme_zh.write_text(zh_content, encoding='utf-8')

    return readme_en, readme_zh


def generate_project_readme(project: Dict[str, Any], csproj_info: Dict[str, Any]) -> Tuple[Path, Path]:
    """为特定项目生成 README 文件。"""
    name = project['name']
    target = csproj_info['target_framework']
    ptype = 'Library' if csproj_info['is_library'] else 'Application'

    readme_en = project['path'] / 'README.md'
    readme_zh = project['path'] / 'README.zh-CN.md'

    # 文件描述
    descriptions_en = {
        'ModelableAttribute.cs': 'Attribute to mark classes for modeling',
        'IModel.cs': 'Model interface definition',
        'ModelableSourceGenerator.cs': 'Main source generator implementation',
        'ModelableSyntaxReceiver.cs': 'Syntax receiver for modelable classes',
        'Program.cs': 'Application entry point',
        'User.cs': 'Sample model class',
    }

    descriptions_zh = {
        'ModelableAttribute.cs': '用于标记类的建模属性',
        'IModel.cs': '模型接口定义',
        'ModelableSourceGenerator.cs': '主要源代码生成器实现',
        'ModelableSyntaxReceiver.cs': '可建模类的语法接收器',
        'Program.cs': '应用程序入口点',
        'User.cs': '示例模型类',
    }

    # 排序文件
    files = sorted([f.name for f in project['files']])

    # 构建英文 README
    en_lines = [
        f'# {name}',
        '',
        f'**Framework:** {target}',
        '',
        f'**Type:** {ptype}',
        '',
        '## Project Files',
        '',
        '| File | Description |',
        '|------|-------------|'
    ]

    for f in files:
        if f in descriptions_en:
            en_lines.append(f'| [{f}]({f}) | {descriptions_en[f]} |')

    if csproj_info['package_references']:
        en_lines.extend(['', '## Dependencies', '', '| Package | Version |', '|---------|---------|'])
        for pkg in csproj_info['package_references']:
            en_lines.append(f"| {pkg['package']} | {pkg['version']} |")

    if csproj_info['project_references']:
        en_lines.extend(['', '## Project References', ''])
        for ref in csproj_info['project_references']:
            en_lines.append(f'- {ref}')

    # 构建中文 README
    zh_lines = [
        f'# {name}',
        '',
        f'**框架:** {target}',
        '',
        f'**类型:** {ptype}',
        '',
        '## 项目文件',
        '',
        '| 文件 | 描述 |',
        '|------|------|'
    ]

    for f in files:
        if f in descriptions_zh:
            zh_lines.append(f'| [{f}]({f}) | {descriptions_zh[f]} |')

    if csproj_info['package_references']:
        zh_lines.extend(['', '## 依赖包', '', '| 包 | 版本 |', '|---------|---------|'])
        for pkg in csproj_info['package_references']:
            zh_lines.append(f"| {pkg['package']} | {pkg['version']} |")

    if csproj_info['project_references']:
        zh_lines.extend(['', '## 项目引用', ''])
        for ref in csproj_info['project_references']:
            zh_lines.append(f'- {ref}')

    readme_en.write_text('\n'.join(en_lines), encoding='utf-8')
    readme_zh.write_text('\n'.join(zh_lines), encoding='utf-8')

    return readme_en, readme_zh


def main():
    """主执行函数。"""
    script_dir = Path(__file__).parent
    project_root = script_dir.parent.parent.parent

    print('扫描项目结构...')
    projects = get_project_structure(project_root)

    print(f'找到 {len(projects)} 个项目:')
    for p in projects:
        print(f'  - {p["name"]}')

    generated_files = []

    print('\n生成根目录 README 文件...')
    root_files = generate_root_readme(project_root, projects)
    generated_files.extend(root_files)

    print('\n生成项目 README 文件...')
    for p in projects:
        print(f'  处理 {p["name"]}...')
        csproj_info = parse_csproj(p['csproj'])
        proj_files = generate_project_readme(p, csproj_info)
        generated_files.extend(proj_files)

    # 输出摘要
    print('\n=== 摘要 ===')
    print(f'已生成 {len(generated_files)} 个 README 文件:')
    for f in generated_files:
        relative = f.relative_to(project_root)
        print(f'  [OK] {relative}')

    # 返回 JSON 结果
    result = {
        'generatedFiles': [str(f.relative_to(project_root)) for f in generated_files],
        'totalFiles': len(generated_files)
    }
    print(json.dumps(result, ensure_ascii=False))


if __name__ == '__main__':
    main()