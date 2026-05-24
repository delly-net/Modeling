#if !NETSTANDARD2_0
#nullable enable
#endif

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Delly.Modeling.Generator;

/// <summary>
/// MoSet 特性源生成器
/// </summary>
[Generator]
public class ModelSetSourceGenerator : ISourceGenerator
{
    /// <summary>
    /// 初始化源生成器
    /// </summary>
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new ModelSetSyntaxReceiver());
    }

    /// <summary>
    /// 执行源生成
    /// </summary>
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not ModelSetSyntaxReceiver receiver)
            return;

        if (receiver.MoSetClasses.Count == 0)
            return;

        // 获取同一程序集中所有标记了 MoTable 或 MoQuery 的类
        var modelClassInfos = new List<ModelClassInfo>();
        foreach (var classDeclaration in receiver.ModelClasses)
        {
            if (classDeclaration.SyntaxTree.FilePath.Contains("obj/"))
                continue;

            var model = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
            if (symbol is null)
                continue;

            var namespaceSymbol = symbol.ContainingNamespace;
            var namespaceName = namespaceSymbol.IsGlobalNamespace ? "" : namespaceSymbol.ToString();
            var className = symbol.Name;

            modelClassInfos.Add(new ModelClassInfo
            {
                Namespace = namespaceName,
                ClassName = className
            });
        }

        foreach (var moSetClass in receiver.MoSetClasses)
        {
            if (moSetClass.SyntaxTree.FilePath.Contains("obj/"))
                continue;

            var model = context.Compilation.GetSemanticModel(moSetClass.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(moSetClass) as INamedTypeSymbol;
            if (symbol is null)
                continue;

            var namespaceSymbol = symbol.ContainingNamespace;
            var namespaceName = namespaceSymbol.IsGlobalNamespace ? "" : namespaceSymbol.ToString();
            var className = symbol.Name;

            var source = GenerateSourceCode(namespaceName, className, modelClassInfos);
            var hintName = $"{className}.ModelSet.g.cs";
            context.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static string GenerateSourceCode(string namespaceName, string className, List<ModelClassInfo> modelClasses)
    {
        var sb = new StringBuilder();

        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using Delly.Modeling;");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(namespaceName))
            sb.AppendLine($"namespace {namespaceName};");
        else
            sb.AppendLine();

        sb.AppendLine();
        sb.AppendLine($"public partial class {className} : IEntityModelSet");
        sb.AppendLine("{");
        sb.AppendLine("    private static readonly IEntityModel[] _models;");
        sb.AppendLine();
        sb.AppendLine("    static " + className + "()");
        sb.AppendLine("    {");
        sb.Append("        _models = new IEntityModel[]");
        sb.AppendLine("        {");

        foreach (var modelInfo in modelClasses)
        {
            if (!string.IsNullOrEmpty(modelInfo.Namespace))
                sb.AppendLine($"            {modelInfo.Namespace}.{modelInfo.ClassName}.GetEntityModel(),");
            else
                sb.AppendLine($"            {modelInfo.ClassName}.GetEntityModel(),");
        }

        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 获取集合中的所有实体模型");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <returns>所有实体模型</returns>");
        sb.AppendLine("    public System.Collections.Generic.IReadOnlyList<IEntityModel> GetModels()");
        sb.AppendLine("    {");
        sb.AppendLine("        return _models;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 获取集合中的模型数量");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public int Count => _models.Length;");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// 模型类信息
    /// </summary>
    private sealed class ModelClassInfo
    {
        public string Namespace { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
    }
}