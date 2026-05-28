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
            var symbol = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
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

            var source = GenerateSourceCode(context, namespaceName, className, properties, constructors, symbol);
            var hintName = $"{className}.Model.g.cs";
            context.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static string GenerateSourceCode(GeneratorExecutionContext context, string namespaceName, string className, List<IPropertySymbol> properties, List<ConstructorInfo> constructors, INamedTypeSymbol targetClass)
    {
        var parserClassName = FindParserType(targetClass, context);

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
            sb.AppendLine($"/// <summary>");
            sb.AppendLine($"/// {className} 的 {property.Name} 属性模型");
            sb.AppendLine($"/// </summary>");
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
            sb.AppendLine("    /// 获取属性是否可读取");
            sb.AppendLine("    /// </summary>");
            var canRead = property.GetMethod != null;
            sb.AppendLine($"    public bool CanRead => {(canRead ? "true" : "false")};");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 获取属性是否可写入");
            sb.AppendLine("    /// </summary>");
            var canWrite = property.SetMethod != null;
            sb.AppendLine($"    public bool CanWrite => {(canWrite ? "true" : "false")};");
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
            sb.AppendLine("    /// <param name=\"obj\">目标对象</param>");
            sb.AppendLine("    /// <param name=\"value\">属性值</param>");
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
                sb.AppendLine($"        var user = obj as {className};");
                sb.AppendLine("        if (user == null) { return; }");
                var conversion = GetConversionExpression(property.Type, "value");
                if (!isNullable)
                {
                    conversion = GetConversionExpressionNonNull(property.Type, "value");
                }
                sb.AppendLine($"        user.{property.Name} = {conversion};");
                sb.AppendLine("    }");
            }
            sb.AppendLine("}");
        }

        // 生成主模型类 c__ClassNameModel
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// {className} 的模型");
        sb.AppendLine("/// </summary>");
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
        sb.AppendLine("    /// 是否为值类型对象");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public bool IsValue => false;");
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
        sb.AppendLine();

        // 始终生成 Parse 和 TryParse 方法
        if (!string.IsNullOrEmpty(parserClassName))
        {
            // 有 Parser 时声明 _parser 字段并使用它
            sb.AppendLine();
            sb.AppendLine("#if !NETSTANDARD2_0");
            sb.AppendLine("    // Parser 实例缓存");
            sb.AppendLine($"    private static Delly.Modeling.IParsable<{className}>? _parser;");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 尝试将输入对象解析为目标类型实例");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    /// <param name=\"obj\">输入对象</param>");
            sb.AppendLine("    /// <returns>目标类型实例，解析失败时返回 null</returns>");
            sb.AppendLine($"    public object? TryParse(object? obj)");
            sb.AppendLine("    {");
            sb.AppendLine("        if (obj == null)");
            sb.AppendLine("            return null;");
            sb.AppendLine();
            sb.AppendLine($"        if (obj is {className} target)");
            sb.AppendLine("            return target;");
            sb.AppendLine();
            sb.AppendLine("        if (_parser == null)");
            sb.AppendLine($"            _parser = new {parserClassName}();");
            sb.AppendLine();
            sb.AppendLine("        return _parser.TryParse(obj);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 将输入对象解析为目标类型实例");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    /// <param name=\"obj\">输入对象</param>");
            sb.AppendLine("    /// <returns>目标类型实例</returns>");
            sb.AppendLine("    /// <exception cref=\"ArgumentException\">当对象无法转换为目标类型时抛出</exception>");
            sb.AppendLine($"    public object Parse(object? obj)");
            sb.AppendLine("    {");
            sb.AppendLine("        var result = TryParse(obj);");
            sb.AppendLine("        if (result == null)");
            sb.AppendLine($"            throw new ArgumentException($\"Cannot convert {{obj?.GetType().Name ?? \"null\"}} to {className}\");");
            sb.AppendLine("        return result;");
            sb.AppendLine("    }");
            sb.AppendLine("#else");
            sb.AppendLine("    // Parser 实例缓存");
            sb.AppendLine($"    private static Delly.Modeling.IParsable<{className}> _parser;");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 尝试将输入对象解析为目标类型实例");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    /// <param name=\"obj\">输入对象</param>");
            sb.AppendLine("    /// <returns>目标类型实例，解析失败时返回 null</returns>");
            sb.AppendLine($"    public object TryParse(object obj)");
            sb.AppendLine("    {");
            sb.AppendLine("        if (obj == null)");
            sb.AppendLine("            return null;");
            sb.AppendLine();
            sb.AppendLine($"        if (obj is {className} target)");
            sb.AppendLine("            return target;");
            sb.AppendLine();
            sb.AppendLine("        if (_parser == null)");
            sb.AppendLine($"            _parser = new {parserClassName}();");
            sb.AppendLine();
            sb.AppendLine("        return _parser.TryParse(obj);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 将输入对象解析为目标类型实例");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    /// <param name=\"obj\">输入对象</param>");
            sb.AppendLine("    /// <returns>目标类型实例</returns>");
            sb.AppendLine("    /// <exception cref=\"ArgumentException\">当对象无法转换为目标类型时抛出</exception>");
            sb.AppendLine($"    public object Parse(object obj)");
            sb.AppendLine("    {");
            sb.AppendLine("        var result = TryParse(obj);");
            sb.AppendLine("        if (result == null)");
            sb.AppendLine($"            throw new ArgumentException($\"Cannot convert {{(obj != null ? obj.GetType().Name : \"null\")}} to {className}\");");
            sb.AppendLine("        return result;");
            sb.AppendLine("    }");
            sb.AppendLine("#endif");
        }
        else
        {
            // 没有 Parser 时不声明 _parser 字段，直接返回 null
            sb.AppendLine();
            sb.AppendLine("#if !NETSTANDARD2_0");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 尝试将输入对象解析为目标类型实例");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    /// <param name=\"obj\">输入对象</param>");
            sb.AppendLine("    /// <returns>目标类型实例，解析失败时返回 null</returns>");
            sb.AppendLine($"    public object? TryParse(object? obj)");
            sb.AppendLine("    {");
            sb.AppendLine("        if (obj == null)");
            sb.AppendLine("            return null;");
            sb.AppendLine();
            sb.AppendLine($"        if (obj is {className} target)");
            sb.AppendLine("            return target;");
            sb.AppendLine();
            sb.AppendLine("        return null;");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 将输入对象解析为目标类型实例");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    /// <param name=\"obj\">输入对象</param>");
            sb.AppendLine("    /// <returns>目标类型实例</returns>");
            sb.AppendLine("    /// <exception cref=\"ArgumentException\">当对象无法转换为目标类型时抛出</exception>");
            sb.AppendLine($"    public object Parse(object? obj)");
            sb.AppendLine("    {");
            sb.AppendLine("        var result = TryParse(obj);");
            sb.AppendLine("        if (result == null)");
            sb.AppendLine($"            throw new ArgumentException($\"Cannot convert {{obj?.GetType().Name ?? \"null\"}} to {className}\");");
            sb.AppendLine("        return result;");
            sb.AppendLine("    }");
            sb.AppendLine("#else");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 尝试将输入对象解析为目标类型实例");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    /// <param name=\"obj\">输入对象</param>");
            sb.AppendLine("    /// <returns>目标类型实例，解析失败时返回 null</returns>");
            sb.AppendLine($"    public object TryParse(object obj)");
            sb.AppendLine("    {");
            sb.AppendLine("        if (obj == null)");
            sb.AppendLine("            return null;");
            sb.AppendLine();
            sb.AppendLine($"        if (obj is {className} target)");
            sb.AppendLine("            return target;");
            sb.AppendLine();
            sb.AppendLine("        return null;");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 将输入对象解析为目标类型实例");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    /// <param name=\"obj\">输入对象</param>");
            sb.AppendLine("    /// <returns>目标类型实例</returns>");
            sb.AppendLine("    /// <exception cref=\"ArgumentException\">当对象无法转换为目标类型时抛出</exception>");
            sb.AppendLine($"    public object Parse(object obj)");
            sb.AppendLine("    {");
            sb.AppendLine("        var result = TryParse(obj);");
            sb.AppendLine("        if (result == null)");
            sb.AppendLine($"            throw new ArgumentException($\"Cannot convert {{(obj != null ? obj.GetType().Name : \"null\")}} to {className}\");");
            sb.AppendLine("        return result;");
            sb.AppendLine("    }");
            sb.AppendLine("#endif");
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

#if !NETSTANDARD2_0
    /// <summary>
    /// 查找目标类对应的 Parser 类型
    /// </summary>
    /// <param name="targetClass">目标类符号</param>
    /// <param name="context">生成器执行上下文</param>
    /// <returns>Parser 类型的完全限定名，如果未找到则返回 null</returns>
    private static string? FindParserType(INamedTypeSymbol targetClass, GeneratorExecutionContext context)
#else
    /// <summary>
    /// 查找目标类对应的 Parser 类型
    /// </summary>
    /// <param name="targetClass">目标类符号</param>
    /// <param name="context">生成器执行上下文</param>
    /// <returns>Parser 类型的完全限定名，如果未找到则返回 null</returns>
    private static string FindParserType(INamedTypeSymbol targetClass, GeneratorExecutionContext context)
#endif
    {
        var parsableAttrSymbol = context.Compilation.GetTypeByMetadataName("Delly.Modeling.ParsableAttribute");
        if (parsableAttrSymbol == null)
            return null;

        var iparsableInterface = context.Compilation.GetTypeByMetadataName("Delly.Modeling.IParsable`1");
        if (iparsableInterface == null)
            return null;

        // 查找目标类上的 [Parsable(typeof(ParserClass))] 特性
        var parsableAttr = targetClass.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Equals(parsableAttrSymbol, SymbolEqualityComparer.Default) == true);

        if (parsableAttr == null || parsableAttr.ConstructorArguments.Length == 0)
            return null;

        // 获取 Parser 类型
        var parserTypeSymbol = parsableAttr.ConstructorArguments[0].Value as INamedTypeSymbol;
        if (parserTypeSymbol == null)
            return null;

        // 验证 Parser 类型是否实现了 IParsable<T>
        bool foundMatchingInterface = false;
        foreach (var iface in parserTypeSymbol.AllInterfaces)
        {
            if (iface.Name == "IParsable" && iface is INamedTypeSymbol namedInterface &&
                namedInterface.IsGenericType && namedInterface.TypeArguments.Length == 1)
            {
                var targetTypeArgument = namedInterface.TypeArguments[0];
                // 验证泛型参数 T 是否为目标类类型
                if (SymbolEqualityComparer.Default.Equals(targetTypeArgument, targetClass))
                {
                    foundMatchingInterface = true;
                    break;
                }
            }
        }

        if (!foundMatchingInterface)
        {
            // 报告诊断错误：Parser 类型未实现 IParsable<目标类>
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(
                    "DM002",
                    "Invalid Parser Type",
                    "Parser type '{0}' must implement IParsable<{1}> interface",
                    "Usage",
                    DiagnosticSeverity.Error,
                    true),
                targetClass.Locations.FirstOrDefault(),
                parserTypeSymbol.Name,
                targetClass.Name));
            return null;
        }

        return parserTypeSymbol.ToString();
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

    /// <summary>
    /// 获取类型转换表达式（用于非空值）
    /// </summary>
    private static string GetConversionExpressionNonNull(ITypeSymbol type, string valueExpr)
    {
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType &&
            namedType.ConstructedFrom?.Name == "Nullable")
        {
            type = namedType.TypeArguments[0];
        }

        var fullTypeName = type.ToString();

        return fullTypeName switch
        {
            "string" or "System.String" => $"{valueExpr}.ToString() ?? string.Empty",
            "int" or "System.Int32" => $"Convert.ToInt32({valueExpr})",
            "long" or "System.Int64" => $"Convert.ToInt64({valueExpr})",
            "bool" or "System.Boolean" => $"Convert.ToBoolean({valueExpr})",
            "double" or "System.Double" => $"Convert.ToDouble({valueExpr})",
            "decimal" or "System.Decimal" => $"Convert.ToDecimal({valueExpr})",
            "System.DateTime" => $"Convert.ToDateTime({valueExpr})",
            "System.Guid" => $"{valueExpr} is Guid guid ? guid : Guid.Parse({valueExpr}.ToString() ?? Guid.Empty.ToString())",
            _ => $"{valueExpr}.ToString() ?? string.Empty"
        };
    }
}