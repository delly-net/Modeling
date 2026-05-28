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
        if (context.SyntaxReceiver is not ModelSetSyntaxReceiver receiver) { return; }

        if (receiver.MoSetClasses.Count == 0) { return; }

        // 获取同一程序集中所有标记了 MoTable 或 MoQuery 的类
        var modelClassInfos = new List<ModelClassInfo>();
        foreach (var classDeclaration in receiver.ModelClasses)
        {
            if (classDeclaration.SyntaxTree.FilePath.Contains("obj/")) { continue; }

            var model = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
            if (symbol is null) { continue; }

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
            if (moSetClass.SyntaxTree.FilePath.Contains("obj/")) { continue; }

            var model = context.Compilation.GetSemanticModel(moSetClass.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(moSetClass) as INamedTypeSymbol;
            if (symbol is null) { continue; }

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

        sb.AppendLine("#if !NETSTANDARD2_0");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("#endif");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using Delly.Modeling;");

        // 为有命名空间的模型类添加 using 指令
        var uniqueNamespaces = modelClasses
            .Where(m => !string.IsNullOrEmpty(m.Namespace))
            .Select(m => m.Namespace)
            .Distinct()
            .ToList();

        foreach (var ns in uniqueNamespaces)
        {
            sb.AppendLine($"using {ns};");
        }

        sb.AppendLine();

        if (!string.IsNullOrEmpty(namespaceName))
            sb.AppendLine($"namespace {namespaceName};");
        else
            sb.AppendLine();

        sb.AppendLine();
        sb.AppendLine($"public partial class {className} : IEntityModelSet");
        sb.AppendLine("{");
        sb.AppendLine("    private static readonly IEntityModel[] _models;");

        // 为每个模型类生成静态 Type 字段
        foreach (var modelInfo in modelClasses)
        {
            var fieldName = GetFieldName(modelInfo.ClassName);
            sb.AppendLine($"    private static readonly System.Type _{fieldName} = typeof({modelInfo.ClassName});");
        }

        sb.AppendLine();
        sb.AppendLine("    static " + className + "()");
        sb.AppendLine("    {");
        sb.Append("        _models = new IEntityModel[]");
        sb.AppendLine("        {");

        foreach (var modelInfo in modelClasses)
        {
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
        sb.AppendLine();

        // 生成 GetModel<T>() 方法
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 获取指定类型的模型");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <typeparam name=\"T\">类型</typeparam>");
        sb.AppendLine("    /// <returns>指定类型的模型</returns>");
        sb.AppendLine("    /// <exception cref=\"System.NotSupportedException\">当类型不匹配时抛出</exception>");
        sb.AppendLine("    public IEntityModel GetModel<T>()");
        sb.AppendLine("    {");
        sb.AppendLine("        var typeOfT = typeof(T);");
        foreach (var modelInfo in modelClasses)
        {
            var fieldName = GetFieldName(modelInfo.ClassName);
            sb.AppendLine($"        if (typeOfT == _{fieldName})");
            sb.AppendLine($"            return {modelInfo.ClassName}.GetEntityModel();");
        }
        sb.AppendLine("        throw new System.NotSupportedException($\"不支持的模型类型: {typeof(T).FullName}\");");
        sb.AppendLine("    }");
        sb.AppendLine();

        // 生成 TryGetModel<T>() 方法
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 尝试获取指定类型的模型");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <typeparam name=\"T\">类型</typeparam>");
        sb.AppendLine("    /// <returns>模型对象，未找到时返回 null</returns>");
        sb.AppendLine("#if !NETSTANDARD2_0");
        sb.AppendLine("    public IEntityModel? TryGetModel<T>()");
        sb.AppendLine("#else");
        sb.AppendLine("    public IEntityModel TryGetModel<T>()");
        sb.AppendLine("#endif");
        sb.AppendLine("    {");
        sb.AppendLine("        var typeOfT = typeof(T);");
        foreach (var modelInfo in modelClasses)
        {
            var fieldName = GetFieldName(modelInfo.ClassName);
            sb.AppendLine($"        if (typeOfT == _{fieldName})");
            sb.AppendLine($"            return {modelInfo.ClassName}.GetEntityModel();");
        }
        sb.AppendLine("        return null;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    // 获取类名对应的字段名（将类名转为小写开头）
    private static string GetFieldName(string className)
    {
        if (string.IsNullOrEmpty(className))
            return className;

        return char.ToLower(className[0]) + className.Substring(1);
    }

    // 模型类信息
    private sealed class ModelClassInfo
    {
        public string Namespace { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
    }
}