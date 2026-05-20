using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Delly.Modeling.Generator;

/// <summary>
/// Modelable 特性源生成器
/// </summary>
[Generator]
public class ModelTableSourceGenerator : ISourceGenerator
{
    /// <summary>
    /// 初始化源生成器
    /// </summary>
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new ModelTableSyntaxReceiver());
    }

    /// <summary>
    /// 执行源生成
    /// </summary>
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not ModelTableSyntaxReceiver receiver || receiver.Classes.Count == 0)
            return;

        foreach (var classDeclaration in receiver.Classes)
        {
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
            var hintName = $"{className}.EntityModel.g.cs";
            context.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static string GenerateSourceCode(string namespaceName, string className, List<IPropertySymbol> properties)
    {
        var sb = new StringBuilder();

        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Data;");
        sb.AppendLine("using Delly.Modeling;");
        sb.AppendLine("using Delly.Modeling.Models;");
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
        sb.AppendLine($"    public static emc__{className} GetEntityModel() => emc__{className}.Instance;");
        sb.AppendLine("}");

        // 为每个属性生成 p__ClassNameModel_PropertyName 类
        foreach (var property in properties)
        {
            sb.AppendLine();
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// 自动模型");
            sb.AppendLine("/// </summary>");
            var propertyClassName = $"emp__{className}_{property.Name}";
            sb.AppendLine($"public class {propertyClassName} : IEntityModelProperty");
            sb.AppendLine("{");
            sb.AppendLine("    // 固定静态共享实例");
            sb.AppendLine($"    private static readonly {propertyClassName} _instance = new {propertyClassName}();");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 公共实例");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public static {propertyClassName} Instance => _instance;");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 属性名称");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public string Name => nameof({className}.{property.Name});");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 属性名称");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public string PropertyName => nameof({className}.{property.Name});");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 是否主键");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public bool IsPrimaryKey => false;");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 是否自增长");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public bool IsAutoIncrement => false;");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 属性建模");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public IBaseModel PropertyModel => {GetModelClassName(property.Type)}.Instance;");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 获取值");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    /// <param name=\"obj\"></param>");
            sb.AppendLine("    /// <returns></returns>");
            sb.AppendLine("    public object? GetValue(object? obj)");
            sb.AppendLine("    {");
            sb.AppendLine("        if (obj is null) { return null; }");
            sb.AppendLine($"        if (obj is {className} user) {{ return user.{property.Name}; }}");
            sb.AppendLine("        return null;");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 设置值");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    /// <param name=\"obj\"></param>");
            sb.AppendLine("    /// <param name=\"value\"></param>");
            if (property.IsReadOnly || property.SetMethod is null)
            {
                sb.AppendLine("    /// <exception cref=\"NotSupportedException\"></exception>");
                sb.AppendLine("    public void SetValue(object? obj, object? value)");
                sb.AppendLine("    {");
                sb.AppendLine($"        throw new NotSupportedException($\"Property '{property.Name}' is readonly.\");");
                sb.AppendLine("    }");
            }
            else
            {
                sb.AppendLine("    public void SetValue(object? obj, object? value)");
                sb.AppendLine("    {");
                var isNullable = property.Type.IsReferenceType && property.Type.NullableAnnotation == NullableAnnotation.Annotated;
                if (!isNullable)
                {
                    sb.AppendLine("        if (obj is null) { throw new NoNullAllowedException($\"Target object not allowed to be null.\"); }");
                }
                sb.AppendLine($"        var user = ({className})obj;");
                var conversion = GetConversionExpression(property.Type, "value");
                if (!isNullable)
                {
                    conversion = $"value is null ? throw new NoNullAllowedException($\"Value not allowed to be null.\") : {conversion}";
                }
                sb.AppendLine($"        user.{property.Name} = {conversion};");
                sb.AppendLine("    }");
            }
            sb.AppendLine("}");
        }

        // 生成主模型类 c__ClassNameModel
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// 自动模型");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public class emc__{className} : IEntityModel");
        sb.AppendLine("{");
        sb.AppendLine();
        sb.AppendLine("    // 固定静态共享实例");
        sb.AppendLine($"    private static readonly emc__{className} _instance = new emc__{className}();");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 公共实例");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public static emc__{className} Instance => _instance;");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 名称");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public string Name => \"{className}Model\";");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 名称");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public string ClassName => \"{className}Model\";");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 命名空间");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public string Namespace => \"{namespaceName}\";");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 获取属性集合");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <returns></returns>");
        sb.AppendLine("    public IEntityModelProperty[] GetProperties()");
        sb.AppendLine("    {");
        sb.Append("        return [");
        var propertyModelClasses = properties.Select(p => $"emp__{className}_{p.Name}.Instance");
        sb.Append(string.Join(", ", propertyModelClasses));
        sb.AppendLine("];");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 获取索引集合");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <returns></returns>");
        sb.AppendLine("    public EntityModelIndex[] GetIndexes()");
        sb.AppendLine("    {");
        sb.AppendLine("        return [];");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 获取属性");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <param name=\"name\"></param>");
        sb.AppendLine("    /// <returns></returns>");
        sb.AppendLine("    public IEntityModelProperty? GetProperty(string name)");
        sb.AppendLine("    {");
        sb.AppendLine("        switch (name)");
        sb.AppendLine("        {");
        foreach (var property in properties)
        {
            sb.AppendLine($"            case nameof({className}.{property.Name}):");
            sb.AppendLine($"                return emp__{className}_{property.Name}.Instance;");
        }
        sb.AppendLine("            default:");
        sb.AppendLine("                return null;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// 获取类型对应的模型类名
    /// </summary>
    private static string GetModelClassName(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType &&
            namedType.ConstructedFrom?.Name == "Nullable")
        {
            type = namedType.TypeArguments[0];
        }

        var fullTypeName = type.ToString();
        return fullTypeName switch
        {
            "string" => "StringModel",
            "System.String" => "StringModel",
            "int" or "System.Int32" => "Int32Model",
            "long" or "System.Int64" => "Int64Model",
            "bool" or "System.Boolean" => "BooleanModel",
            "double" or "System.Double" => "DoubleModel",
            "decimal" or "System.Decimal" => "DecimalModel",
            "System.DateTime" => "DateTimeModel",
            "System.Guid" => "GuidModel",
            _ => "StringModel"
        };
    }

    /// <summary>
    /// 获取类型转换表达式
    /// </summary>
    private static string GetConversionExpression(ITypeSymbol type, string valueExpr)
    {
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType &&
            namedType.ConstructedFrom?.Name == "Nullable")
        {
            type = namedType.TypeArguments[0];
        }

        var fullTypeName = type.ToString();

        return fullTypeName switch
        {
            "string" or "System.String" => $"{valueExpr}?.ToString()",
            "int" or "System.Int32" => $"Convert.ToInt32({valueExpr})",
            "long" or "System.Int64" => $"Convert.ToInt64({valueExpr})",
            "bool" or "System.Boolean" => $"Convert.ToBoolean({valueExpr})",
            "double" or "System.Double" => $"Convert.ToDouble({valueExpr})",
            "decimal" or "System.Decimal" => $"Convert.ToDecimal({valueExpr})",
            "System.DateTime" => $"Convert.ToDateTime({valueExpr})",
            "System.Guid" => $"{valueExpr} is Guid guid ? guid : Guid.Parse({valueExpr}?.ToString() ?? Guid.Empty.ToString())",
            _ => $"{valueExpr}?.ToString()"
        };
    }
}