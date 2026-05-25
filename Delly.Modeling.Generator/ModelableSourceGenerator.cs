using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Delly.Modeling.Generator;

/// <summary>
/// 构造函数信息
/// </summary>
internal sealed class ConstructorInfo
{
    public List<ITypeSymbol> ParameterTypes { get; set; } = new();
}

/// <summary>
/// Modelable 特性源生成器
/// </summary>
[Generator]
public class ModelableSourceGenerator : ISourceGenerator
{
    /// <summary>
    /// 初始化源生成器
    /// </summary>
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new ModelableSyntaxReceiver());
    }

    /// <summary>
    /// 执行源生成
    /// </summary>
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not ModelableSyntaxReceiver receiver || receiver.Classes.Count == 0)
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

            // 收集所有构造函数信息
            var constructors = new List<ConstructorInfo>();
            foreach (var ctor in symbol.Constructors)
            {
                constructors.Add(new ConstructorInfo
                {
                    ParameterTypes = ctor.Parameters.Select(p => p.Type).ToList()
                });
            }

            var source = GenerateSourceCode(namespaceName, className, properties, constructors);
            var hintName = $"{className}.Model.g.cs";
            context.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static string GenerateSourceCode(string namespaceName, string className, List<IPropertySymbol> properties, List<ConstructorInfo> constructors)
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
        sb.AppendLine($"    public static mc__{className} GetModel() => mc__{className}.Instance;");
        sb.AppendLine("}");

        // 为每个属性生成 p__ClassNameModel_PropertyName 类
        foreach (var property in properties)
        {
            sb.AppendLine();
            var propertyClassName = $"mp__{className}_{property.Name}";
            sb.AppendLine($"public class {propertyClassName} : IModelProperty");
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
            sb.AppendLine("    /// 属性建模");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public IBaseModel PropertyModel => {GetModelClassName(property.Type)}.Instance;");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 属性类型信息");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public Type PropertyType => typeof({GetTypeofName(property.Type)});");
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
        sb.AppendLine($"public class mc__{className} : IModel");
        sb.AppendLine("{");
        sb.AppendLine();
        sb.AppendLine("    // 固定静态共享实例");
        sb.AppendLine($"    private static readonly mc__{className} _instance = new mc__{className}();");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 公共实例");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public static mc__{className} Instance => _instance;");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 名称");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public string Name => \"{className}Model\";");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 命名空间");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public string Namespace => \"{namespaceName}\";");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 模型类型信息，源生成阶段使用 typeof(T) 赋值");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public Type ClassType => typeof({className});");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 获取集合");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <returns></returns>");
        sb.AppendLine("    public IModelProperty[] GetProperties()");
        sb.AppendLine("    {");
        sb.Append("        return [");
        var propertyModelClasses = properties.Select(p => $"mp__{className}_{p.Name}.Instance");
        sb.Append(string.Join(", ", propertyModelClasses));
        sb.AppendLine("];");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 获取属性");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <param name=\"name\"></param>");
        sb.AppendLine("    /// <returns></returns>");
        sb.AppendLine("    public IModelProperty? GetProperty(string name)");
        sb.AppendLine("    {");
        sb.AppendLine("        switch (name)");
        sb.AppendLine("        {");
        foreach (var property in properties)
        {
            sb.AppendLine($"            case nameof({className}.{property.Name}):");
            sb.AppendLine($"                return mp__{className}_{property.Name}.Instance;");
        }
        sb.AppendLine("            default:");
        sb.AppendLine("                return null;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 创建模型类型的新实例");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <param name=\"args\">构造函数参数数组</param>");
        sb.AppendLine("    /// <returns>模型类型的新实例</returns>");
        sb.AppendLine("    public object CreateInstance(params object[] args)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (args == null) args = new object[0];");

        // 检查是否有参数数量相同的构造函数
        var paramCounts = constructors.Select(c => c.ParameterTypes.Count).ToList();
        var hasDuplicate = paramCounts.GroupBy(x => x).Any(g => g.Count() > 1);

        if (hasDuplicate)
        {
            sb.AppendLine("        throw new NotSupportedException(\"Multiple constructors with the same parameter count are not supported.\");");
        }
        else
        {
            sb.AppendLine($"        var paramCount = args.Length;");
            sb.AppendLine("        switch (paramCount)");
            sb.AppendLine("        {");
            foreach (var ctor in constructors)
            {
                var paramTypes = ctor.ParameterTypes;
                var paramCount = paramTypes.Count;
                sb.AppendLine($"            case {paramCount}:");
                if (paramCount > 0)
                {
                    var typeCastArgs = new List<string>();
                    for (int i = 0; i < paramCount; i++)
                    {
                        var paramType = paramTypes[i];
                        var castExpr = GetTypeCastExpression(paramType, $"args[{i}]");
                        typeCastArgs.Add(castExpr);
                    }
                    sb.AppendLine($"                return new {className}({string.Join(", ", typeCastArgs)});");
                }
                else
                {
                    sb.AppendLine($"                return new {className}();");
                }
            }
            sb.AppendLine("            default:");
            sb.AppendLine($"                throw new ArgumentException($\"No constructor found with {{paramCount}} parameters.\");");
            sb.AppendLine("        }");
        }
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
    /// 获取可用于 typeof 的类型字符串（解包 nullable 类型）
    /// </summary>
    private static string GetTypeofName(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType &&
            namedType.ConstructedFrom?.Name == "Nullable")
        {
            type = namedType.TypeArguments[0];
        }

        var fullTypeName = type.ToString();
        return fullTypeName.TrimEnd('?');
    }

    /// <summary>
    /// 获取类型转换表达式
    /// </summary>
    private static string GetTypeCastExpression(ITypeSymbol type, string argExpr)
    {
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType &&
            namedType.ConstructedFrom?.Name == "Nullable")
        {
            type = namedType.TypeArguments[0];
        }

        var fullTypeName = type.ToString();

        return fullTypeName switch
        {
            "string" or "System.String" => $"({argExpr} as string) ?? string.Empty",
            "int" or "System.Int32" => $"Convert.ToInt32({argExpr})",
            "long" or "System.Int64" => $"Convert.ToInt64({argExpr})",
            "bool" or "System.Boolean" => $"Convert.ToBoolean({argExpr})",
            "double" or "System.Double" => $"Convert.ToDouble({argExpr})",
            "decimal" or "System.Decimal" => $"Convert.ToDecimal({argExpr})",
            "System.DateTime" => $"Convert.ToDateTime({argExpr})",
            "System.Guid" => $"{argExpr} is Guid guid ? guid : Guid.Parse({argExpr}?.ToString() ?? Guid.Empty.ToString())",
            _ => $"({argExpr} as {fullTypeName})"
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