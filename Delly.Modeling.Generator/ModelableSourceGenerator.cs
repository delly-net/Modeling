using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Delly.Modeling.Generator;

/// <summary>
/// Modelable 源代码生成器，为标记了 Modelable 特性的类生成模型访问代码
/// </summary>
[Generator]
public class ModelableSourceGenerator : ISourceGenerator
{
    /// <summary>
    /// 初始化生成器
    /// </summary>
    /// <param name="context">生成器初始化上下文</param>
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new ModelableSyntaxReceiver());
    }

    /// <summary>
    /// 执行源代码生成
    /// </summary>
    /// <param name="context">生成器执行上下文</param>
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not ModelableSyntaxReceiver receiver || receiver.Classes.Count == 0)
            return;

        foreach (var classDeclaration in receiver.Classes)
        {
            // 跳过生成的文件
            if (classDeclaration.SyntaxTree.FilePath.Contains("obj/"))
                continue;

            var model = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(classDeclaration);
            if (symbol is null)
                continue;

            var namespaceSymbol = symbol.ContainingNamespace;
            var namespaceName = namespaceSymbol.IsGlobalNamespace ? "" : namespaceSymbol.ToString();
            var className = symbol.Name;
            var properties = symbol.GetMembers().OfType<IPropertySymbol>().ToList();

            var source = GenerateSourceCode(namespaceName, className, properties);
            var hintName = $"{className}.Model.g.cs";
            context.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
        }
    }

    // 生成源代码字符串
    private static string GenerateSourceCode(string namespaceName, string className, List<IPropertySymbol> properties)
    {
        var sb = new StringBuilder();

        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Data;");
        sb.AppendLine("using Delly.Modeling;");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(namespaceName))
            sb.AppendLine($"namespace {namespaceName};");
        else
            sb.AppendLine();

        sb.AppendLine();
        sb.AppendLine($"public partial class {className}");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 获取模型");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public static g__{className}Model GetModel() => g__{className}Model.Instance;");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// 自动模型");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public class g__{className}Model : IModel<{className}>");
        sb.AppendLine("{");
        sb.AppendLine();
        sb.AppendLine("    // 固定静态共享实例");
        sb.AppendLine($"    private static readonly g__{className}Model _instance = new g__{className}Model();");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 公共实例");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public static g__{className}Model Instance => _instance;");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 获取集合");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <returns></returns>");
        sb.AppendLine("    public string[] GetProperties()");
        sb.AppendLine("    {");
        sb.Append("        return [");

        var propertyNames = properties.Select(p => $"nameof({className}.{p.Name})");
        sb.Append(string.Join(", ", propertyNames));

        sb.AppendLine("];");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 获取属性值");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <param name=\"model\"></param>");
        sb.AppendLine("    /// <param name=\"name\"></param>");
        sb.AppendLine($"    /// <param name=\"{char.ToLower(className[0])}{className.Substring(1)}\"></param>");
        sb.AppendLine("    /// <returns></returns>");
        sb.AppendLine($"    public object GetProperty({className} {char.ToLower(className[0])}{className.Substring(1)}, string name)");
        sb.AppendLine("    {");
        sb.AppendLine("        switch (name)");
        sb.AppendLine("        {");

        foreach (var property in properties)
        {
            sb.AppendLine($"            case nameof({className}.{property.Name}):");
            sb.AppendLine($"                return {char.ToLower(className[0])}{className.Substring(1)}.{property.Name};");
        }

        sb.AppendLine("            default: throw new MissingMemberException($\"Property '{name}' not found.\");");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 设置属性值");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <param name=\"model\"></param>");
        sb.AppendLine($"    /// <param name=\"{char.ToLower(className[0])}{className.Substring(1)}\"></param>");
        sb.AppendLine("    /// <param name=\"name\"></param>");
        sb.AppendLine("    /// <param name=\"value\"></param>");
        sb.AppendLine($"    public void SetProperty({className} {char.ToLower(className[0])}{className.Substring(1)}, string name, object value)");
        sb.AppendLine("    {");
        sb.AppendLine("        switch (name)");
        sb.AppendLine("        {");

        foreach (var property in properties)
        {
            sb.AppendLine($"            case nameof({className}.{property.Name}):");
            if (property.IsReadOnly || property.SetMethod is null)
            {
                sb.AppendLine($"                throw new NotSupportedException($\"Property '{{name}}' is readonly.\");");
            }
            else
            {
                var isNullable = property.Type.IsReferenceType && property.Type.NullableAnnotation == NullableAnnotation.Annotated;
                if (!isNullable && property.Type is INamedTypeSymbol namedType && !namedType.IsValueType)
                {
                    sb.AppendLine($"                {char.ToLower(className[0])}{className.Substring(1)}.{property.Name} = value?.ToString() ?? throw new NoNullAllowedException($\"Property '{property.Name}' not allowed to be null.\");");
                }
                else
                {
                    sb.AppendLine($"                {char.ToLower(className[0])}{className.Substring(1)}.{property.Name} = value?.ToString();");
                }
                sb.AppendLine("                break;");
            }
        }

        sb.AppendLine("            default: throw new MissingMemberException($\"Property '{name}' not found.\");");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}