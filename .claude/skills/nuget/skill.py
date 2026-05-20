#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
NuGet 包元数据生成器
分析 .csproj 文件并生成 Description 和 PackageTags 属性
"""

import os
import sys
import re
import json
from pathlib import Path
from typing import List, Dict, Any, Optional, Tuple
from xml.etree import ElementTree as ET


# 跳过的项目（测试、演示、示例项目）
SKIP_PATTERNS = [
    r'.*Test.*',
    r'.*\.Test$',
    r'Demo',
    r'Example',
    r'Deme',
]


def should_skip_project(project_name: str) -> bool:
    """检查项目是否应跳过。"""
    for pattern in SKIP_PATTERNS:
        if re.match(pattern, project_name, re.IGNORECASE):
            return True
    return False


def get_project_structure(root_path: Path) -> List[Dict[str, Any]]:
    """查找解决方案中的所有项目。"""
    projects = []
    for item in root_path.iterdir():
        if not item.is_dir() or item.name.startswith('.') or item.name in ['bin', 'obj', '.git']:
            continue
        csproj_files = list(item.glob('*.csproj'))
        if csproj_files:
            csproj = csproj_files[0]
            projects.append({
                'name': item.name,
                'path': item,
                'csproj': csproj,
                'files': list(item.glob('*.cs')),
            })
    return projects


def parse_csproj(csproj_path: Path) -> Dict[str, Any]:
    """解析 .csproj 文件获取元数据。"""
    content = csproj_path.read_text(encoding='utf-8')
    result = {
        'target_framework': '',
        'is_library': True,
        'is_aot_enabled': False,
        'is_generator': False,
        'package_references': [],
        'project_references': [],
        'xml_doc_enabled': False,
        'existing_description': '',
        'existing_tags': '',
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

    # XML 文档
    if '<GenerateDocumentationFile>True</GenerateDocumentationFile>' in content or \
       '<GenerateDocumentationFile>true</GenerateDocumentationFile>' in content:
        result['xml_doc_enabled'] = True

    # 检查是否为源代码生成器
    if 'Analyzer' in content or 'Roslyn' in content or 'SourceGenerator' in content:
        result['is_generator'] = True

    # 包引用
    pkg_matches = re.findall(r'<PackageReference Include="([^"]+)" Version="([^"]+)"', content)
    for pkg, ver in pkg_matches:
        result['package_references'].append({'package': pkg, 'version': ver})

    # 项目引用
    proj_matches = re.findall(r'<ProjectReference Include="([^"]+)"', content)
    result['project_references'] = proj_matches

    # 现有 Description
    desc_match = re.search(r'<Description>([^<]+)</Description>', content)
    if desc_match:
        result['existing_description'] = desc_match.group(1)

    # 现有 PackageTags
    tags_match = re.search(r'<PackageTags>([^<]+)</PackageTags>', content)
    if tags_match:
        result['existing_tags'] = tags_match.group(1)

    return result


def analyze_code_files(project: Dict[str, Any]) -> Dict[str, Any]:
    """分析 C# 源文件获取公共 API 和文档。"""
    classes = []
    interfaces = []
    attributes = []
    namespaces = set()

    for file in project['files']:
        try:
            content = file.read_text(encoding='utf-8')

            # 提取命名空间
            ns_match = re.search(r'namespace\s+([\w\.]+)', content)
            if ns_match:
                namespaces.add(ns_match.group(1))

            # 提取公共类
            class_matches = re.finditer(r'(?:public|internal)\s+(?:partial\s+)?class\s+(\w+)', content)
            for match in class_matches:
                class_name = match.group(1)
                # 跳过生成的类（g__ 前缀）
                if class_name.startswith('g__'):
                    continue
                # 获取类摘要
                summary = extract_xml_summary(content, class_name)
                classes.append({
                    'name': class_name,
                    'summary': summary,
                })

            # 提取接口
            interface_matches = re.finditer(r'public\s+interface\s+(\w+)(?:<[^>]+>)?', content)
            for match in interface_matches:
                interface_name = match.group(1)
                summary = extract_xml_summary(content, interface_name)
                interfaces.append({
                    'name': interface_name,
                    'summary': summary,
                })

            # 提取特性
            attr_matches = re.finditer(r'public\s+class\s+(\w+)\s*:\s*Attribute', content)
            for match in attr_matches:
                attr_name = match.group(1)
                summary = extract_xml_summary(content, attr_name)
                if 'Attribute' in attr_name:
                    attributes.append({
                        'name': attr_name,
                        'summary': summary,
                    })

        except Exception:
            continue

    return {
        'classes': classes,
        'interfaces': interfaces,
        'attributes': attributes,
        'namespaces': sorted(namespaces),
    }


def extract_xml_summary(content: str, class_name: str) -> str:
    """提取类的 XML 摘要。"""
    # 首先查找类声明
    class_pattern = rf'(?:public|internal)\s+(?:partial\s+)?class\s+{class_name}'
    class_match = re.search(class_pattern, content)
    if not class_match:
        return ""

    # 获取类之前的文本（最多 500 字符）
    start_pos = max(0, class_match.start() - 500)
    text_before = content[start_pos:class_match.start()]

    # 从 /// 注释中提取摘要
    summary_pattern = r'///\s*<summary>(.*?)</summary>'
    matches = re.findall(summary_pattern, text_before, re.DOTALL)
    for match in matches:
        summary = re.sub(r'\s+', ' ', match).strip()
        # 过滤掉通用/空摘要
        if summary and len(summary) > 5 and not summary.startswith('///'):
            return summary
    return ""


def generate_tags(project: Dict[str, Any], csproj_info: Dict[str, Any], code_analysis: Dict[str, Any]) -> List[str]:
    """基于项目分析生成标签。"""
    tags = []

    # 框架标签
    if 'netstandard' in csproj_info['target_framework']:
        tags.append('dotnet-standard')
        tags.append('netstandard')
    elif 'net8' in csproj_info['target_framework']:
        tags.append('dotnet-8')
        tags.append('csharp-8')
    elif 'net9' in csproj_info['target_framework']:
        tags.append('dotnet-9')
        tags.append('csharp-9')
    elif 'net10' in csproj_info['target_framework']:
        tags.append('dotnet-10')
        tags.append('csharp-10')

    # 特性标签
    if csproj_info['is_generator']:
        tags.append('roslyn')
        tags.append('source-generator')
        tags.append('code-generation')
        tags.append('compiler')
        tags.append('analyzer')

    if csproj_info['is_aot_enabled']:
        tags.append('aot')
        tags.append('native-aot')
        tags.append('trimming')

    # 依赖标签
    for pkg in csproj_info['package_references']:
        pkg_name = pkg['package'].lower()
        if 'analysis' in pkg_name:
            tags.append('roslyn-analyzer')

    # 代码模式标签
    project_name = project['name'].lower()
    if 'model' in project_name:
        tags.append('modeling')
        tags.append('reflection')
        tags.append('property-access')
        tags.append('dynamic')

    if code_analysis['attributes']:
        tags.append('attributes')

    if code_analysis['interfaces']:
        tags.append('interfaces')
        tags.append('generics')

    # 语言标签
    tags.append('csharp')
    tags.append('dotnet')

    return sorted(set(tags))


def generate_description(project: Dict[str, Any], csproj_info: Dict[str, Any], code_analysis: Dict[str, Any]) -> str:
    """生成包描述。"""
    name = project['name']
    target = csproj_info['target_framework']

    if csproj_info['is_generator']:
        desc = f"Roslyn Source Generator for compile-time code generation. "
        if code_analysis['attributes']:
            attr = code_analysis['attributes'][0]['name'] if code_analysis['attributes'] else ""
            if attr:
                desc += f"Uses {attr} to mark classes for automated code generation. "
        desc += "Eliminates runtime reflection overhead with compile-time generation. "
        desc += f"Supports {target} and is AOT-compatible."
        return desc
    else:
        desc = f"{name} is a .NET modeling library providing compile-time property access capabilities. "
        if code_analysis['interfaces']:
            for iface in code_analysis['interfaces']:
                if iface['summary']:
                    desc += f"{iface['summary']} "
        if code_analysis['attributes']:
            attr_name = code_analysis['attributes'][0]['name'] if code_analysis['attributes'] else ""
            if attr_name:
                desc += f"Uses {attr_name} to mark classes for source generation. "
        desc += f"Target Framework: {target}. "
        if csproj_info['is_aot_enabled']:
            desc += "AOT-compatible. "
        return desc.strip()


def update_csproj_with_metadata(csproj_path: Path, description: str, tags: List[str]) -> bool:
    """更新 .csproj 文件，添加 Description 和 PackageTags 属性。"""
    content = csproj_path.read_text(encoding='utf-8')

    # 检查 PropertyGroup 是否存在
    if '<PropertyGroup>' not in content:
        return False

    # 生成标签字符串
    tags_str = ' '.join(tags)

    # 检查 Description 是否已存在
    desc_pattern = r'<Description>[^<]*</Description>'
    if re.search(desc_pattern, content):
        content = re.sub(desc_pattern, f'<Description>{description}</Description>', content)
    else:
        # 在第一个 PropertyGroup 行后插入 Description
        content = content.replace('<PropertyGroup>', f'<PropertyGroup>\n    <Description>{description}</Description>', 1)

    # 检查 PackageTags 是否已存在
    tags_pattern = r'<PackageTags>[^<]*</PackageTags>'
    if re.search(tags_pattern, content):
        content = re.sub(tags_pattern, f'<PackageTags>{tags_str}</PackageTags>', content)
    else:
        # 在 Description 后或第一个 PropertyGroup 行后插入 PackageTags
        desc_section = f'<Description>{description}</Description>'
        if desc_section in content:
            content = content.replace(desc_section, f'{desc_section}\n    <PackageTags>{tags_str}</PackageTags>', 1)
        else:
            content = content.replace('<PropertyGroup>', f'<PropertyGroup>\n    <PackageTags>{tags_str}</PackageTags>', 1)

    # 写回文件
    csproj_path.write_text(content, encoding='utf-8')
    return True


def main():
    """主执行函数。"""
    script_dir = Path(__file__).parent
    project_root = script_dir.parent.parent.parent

    print('扫描项目结构以查找 NuGet 包...')
    projects = get_project_structure(project_root)

    # 过滤掉跳过的项目
    package_projects = [p for p in projects if not should_skip_project(p['name'])]

    if not package_projects:
        print('未找到包项目（所有项目均被过滤为测试/演示项目）。')
        return

    print(f'找到 {len(package_projects)} 个包项目:')
    for p in package_projects:
        print(f'  - {p["name"]}')

    results = []
    all_tags = set()

    print('\n生成 NuGet 元数据并更新 .csproj 文件...')
    for p in package_projects:
        print(f'  处理 {p["name"]}...')
        csproj_info = parse_csproj(p['csproj'])
        code_analysis = analyze_code_files(p)

        description = generate_description(p, csproj_info, code_analysis)
        tags = generate_tags(p, csproj_info, code_analysis)
        all_tags.update(tags)

        # 更新 csproj 文件
        success = update_csproj_with_metadata(p['csproj'], description, tags)

        results.append({
            'project': p['name'],
            'csproj': str(p['csproj']),
            'description': description,
            'tags': tags,
            'success': success,
        })

    # 输出结果
    print('\n' + '=' * 60)
    print('NuGet 包元数据更新结果')
    print('=' * 60 + '\n')

    for result in results:
        status = '成功' if result['success'] else '失败'
        print(f'[{status}] {result["project"]}')
        print(f'  文件: {result["csproj"]}')
        print(f'  描述: {result["description"]}')
        print(f'  标签: {", ".join(result["tags"])}')
        print()

    # 摘要
    print('=' * 60)
    print('摘要')
    print('=' * 60)
    print(f'已更新项目: {len([r for r in results if r["success"]])}/{len(results)}')
    print(f'唯一标签总数: {", ".join(sorted(all_tags))}')

    # 返回 JSON 结果
    result_json = {
        'projects': results,
        'tags': sorted(list(all_tags)),
        'totalProjects': len(package_projects),
        'successCount': len([r for r in results if r['success']]),
    }
    print('\n' + json.dumps(result_json, ensure_ascii=False, indent=2))


if __name__ == '__main__':
    main()