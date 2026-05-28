#nullable enable

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
/// MoTable 特性源生成器
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
        if (context.SyntaxReceiver is not ModelTableSyntaxReceiver receiver || receiver.Classes.Count == 0) { return; }

        foreach (var classDeclaration in receiver.Classes)
        {
            if (classDeclaration.SyntaxTree.FilePath.Contains("obj/")) { continue; }

            var model = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
            if (symbol is null) { continue; }

            var namespaceSymbol = symbol.ContainingNamespace;
            var namespaceName = namespaceSymbol.IsGlobalNamespace ? "" : namespaceSymbol.ToString();
            var className = symbol.Name;

            // 特性互斥检测
            if (!CheckAttributeMutualExclusion(symbol, context)) { continue; }

            var isQuery = IsMoQuery(symbol);

            // 读取 [MoTable] 特性的 Name，MoQuery 使用类名
            var tableName = isQuery ? className : (GetTableName(symbol) ?? className);

            // 读取属性元数据（MoTable 只包含有 [MoColumn] 的属性，MoQuery 包含所有属性）
            var propertyMetadatas = new List<PropertyMetadata>();

            foreach (var prop in symbol.GetMembers().OfType<IPropertySymbol>())
            {
                var meta = GetPropertyMetadata(prop, isQuery);
                if (meta == null) { continue; }

                propertyMetadatas.Add(meta);
            }

            // 收集并合并 [MoColumnIndex] 索引
            var indexMap = new Dictionary<string, IndexMergeEntry>();
            foreach (var meta in propertyMetadatas)
            {
                var columnName = meta.ColumnName ?? meta.Symbol.Name;
                foreach (var colIdx in meta.ColumnIndexes)
                {
                    if (!indexMap.TryGetValue(colIdx.Name, out var entry))
                    {
                        entry = new IndexMergeEntry();
                        indexMap[colIdx.Name] = entry;
                    }
                    entry.Columns.Add(columnName);
                    entry.IsUnique = entry.IsUnique || colIdx.IsUnique;
                }
            }

            // 转为有序列表（按 Name 排序保证确定性输出）
            var indexes = indexMap
                .OrderBy(kv => kv.Key)
                .Select(kv => new IndexMetadata(kv.Key, kv.Value.Columns.ToArray(), kv.Value.IsUnique))
                .ToList();

            // 收集所有构造函数信息
            var constructors = new List<ConstructorInfo>();
            foreach (var ctor in symbol.Constructors)
            {
                constructors.Add(new ConstructorInfo
                {
                    ParameterTypes = ctor.Parameters.Select(p => p.Type).ToList()
                });
            }

            var targetTypeName = string.IsNullOrEmpty(namespaceName) ? className : $"{namespaceName}.{className}";
            var source = GenerateSourceCode(context, namespaceName, className, tableName, propertyMetadatas, indexes, constructors, symbol);
            var hintName = $"{className}.EntityModel.g.cs";
            context.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static string GenerateSourceCode(GeneratorExecutionContext context, string namespaceName, string className, string tableName, List<PropertyMetadata> properties, List<IndexMetadata> indexes, List<ConstructorInfo> constructors, INamedTypeSymbol targetClass)
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
        sb.AppendLine($"    public static emc__{className} GetEntityModel() => emc__{className}.Instance;");
        sb.AppendLine("}");

        // 为每个属性生成 emp__ClassName_PropertyName 类
        foreach (var meta in properties)
        {
            var prop = meta.Symbol;
            var columnName = meta.ColumnName ?? prop.Name;

            sb.AppendLine();
            var propertyClassName = $"emp__{className}_{prop.Name}";
            sb.AppendLine("/// <summary>");
            sb.AppendLine($"/// {className} 的 {prop.Name} 实体属性模型");
            sb.AppendLine("/// </summary>");
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
            sb.AppendLine("    /// 列名称");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public string Name => \"{columnName}\";");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 属性名称");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public string PropertyName => nameof({className}.{prop.Name});");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 是否主键");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public bool IsPrimaryKey => {(meta.IsPrimaryKey ? "true" : "false")};");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 是否自增长");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public bool IsAutoIncrement => {(meta.IsAutoIncrement ? "true" : "false")};");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 类型");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public ColumnType Type => {GetColumnTypeExpression(meta.Type)};");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 长度");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public int Length => {meta.Length};");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 精度");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public int Precision => {meta.Precision};");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 注释");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public string Comment => \"{EscapeStringLiteral(meta.Comment)}\";");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 属性建模");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public IBaseModel PropertyModel => {GetModelClassName(prop.Type)}.Instance;");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 属性类型信息");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public Type PropertyType => typeof({GetTypeofName(prop.Type)});");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 获取属性是否可读取");
            sb.AppendLine("    /// </summary>");
            var canRead = prop.GetMethod != null;
            sb.AppendLine($"    public bool CanRead => {(canRead ? "true" : "false")};");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 获取属性是否可写入");
            sb.AppendLine("    /// </summary>");
            var canWrite = prop.SetMethod != null;
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
            sb.AppendLine($"        if (obj is {className} user) {{ return user.{prop.Name}; }}");
            sb.AppendLine("        return null;");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 设置值");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    /// <param name=\"obj\">目标对象</param>");
            sb.AppendLine("    /// <param name=\"value\">属性值</param>");
            if (prop.IsReadOnly || prop.SetMethod is null)
            {
                sb.AppendLine("    /// <exception cref=\"NotSupportedException\"></exception>");
                sb.AppendLine("    public void SetValue(object? obj, object? value)");
                sb.AppendLine("    {");
                sb.AppendLine($"        throw new NotSupportedException($\"Property '{prop.Name}' is readonly.\");");
                sb.AppendLine("    }");
            }
            else
            {
                sb.AppendLine("    public void SetValue(object? obj, object? value)");
                sb.AppendLine("    {");
                var isNullable = prop.Type.IsReferenceType && prop.Type.NullableAnnotation == NullableAnnotation.Annotated;
                if (!isNullable)
                {
                    sb.AppendLine("        if (obj is null) { throw new NoNullAllowedException($\"Target object not allowed to be null.\"); }");
                }
                sb.AppendLine($"        var user = obj as {className};");
                sb.AppendLine("        if (user == null) { return; }");
                var conversion = GetConversionExpression(prop.Type, "value");
                if (!isNullable)
                {
                    conversion = GetConversionExpressionNonNull(prop.Type, "value");
                }
                sb.AppendLine($"        user.{prop.Name} = {conversion};");
                sb.AppendLine("    }");
            }
            sb.AppendLine("}");
        }

        // 生成主模型类 emc__ClassName
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// {className} 的实体模型");
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
        sb.AppendLine("    /// 表名");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public string Name => \"{tableName}\";");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// 类名");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine($"    public string ClassName => \"{className}\";");
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
        sb.AppendLine("    /// 获取属性集合");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    /// <returns></returns>");
        sb.AppendLine("    public IEntityModelProperty[] GetProperties()");
        sb.AppendLine("    {");
        sb.Append("        return [");
        var propertyModelClasses = properties.Select(p => $"emp__{className}_{p.Symbol.Name}.Instance");
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
        if (indexes.Count == 0)
        {
            sb.AppendLine("        return [];");
        }
        else
        {
            sb.Append("        return [");
            for (int i = 0; i < indexes.Count; i++)
            {
                var idx = indexes[i];
                var columnsStr = string.Join(", ", idx.Columns.Select(c => $"\"{c}\""));
                sb.Append($"new EntityModelIndex(\"{idx.Name}\", [{columnsStr}], {(idx.IsUnique ? "true" : "false")})");
                if (i < indexes.Count - 1)
                    sb.Append(", ");
            }
            sb.AppendLine("];");
        }
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
        foreach (var meta in properties)
        {
            var prop = meta.Symbol;
            var columnName = meta.ColumnName ?? prop.Name;
            sb.AppendLine($"            case \"{columnName}\":");
            sb.AppendLine($"                return emp__{className}_{prop.Name}.Instance;");
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
                targetClass.Locations.FirstOrDefault()!,
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
    /// 读取 [MoTable] 特性的表名
    /// </summary>
    private static string? GetTableName(INamedTypeSymbol symbol)
    {
        var attr = symbol.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.Name == "MoTableAttribute");
        if (attr == null) { return null; }
        var nameArg = attr.ConstructorArguments.FirstOrDefault();
        return nameArg.Value?.ToString();
    }

    /// <summary>
    /// 检查类是否标记为查询对象
    /// </summary>
    private static bool IsMoQuery(INamedTypeSymbol symbol)
    {
        return symbol.GetAttributes().Any(a =>
            a.AttributeClass?.Name == "MoQueryAttribute");
    }

    /// <summary>
    /// 检查特性互斥
    /// </summary>
    private static bool CheckAttributeMutualExclusion(INamedTypeSymbol symbol, GeneratorExecutionContext context)
    {
        var hasMoTable = symbol.GetAttributes().Any(a => a.AttributeClass?.Name == "MoTableAttribute");
        var hasMoQuery = symbol.GetAttributes().Any(a => a.AttributeClass?.Name == "MoQueryAttribute");

        if (hasMoTable && hasMoQuery)
        {
            var diagnostic = Diagnostic.Create(
                new DiagnosticDescriptor(
                    "DM001",
                    "Attribute Mutually Exclusive",
                    "Class '{0}' cannot have both [MoTable] and [MoQuery] attributes. They are mutually exclusive.",
                    "Usage",
                    DiagnosticSeverity.Error,
                    true),
                symbol.Locations.FirstOrDefault(),
                symbol.Name);
            context.ReportDiagnostic(diagnostic);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 推断属性类型
    /// </summary>
    private static int InferColumnType(ITypeSymbol type)
    {
        var unwrappedType = UnwrapNullableType(type);
        var fullTypeName = unwrappedType.ToString();
        return fullTypeName switch
        {
            "string" or "System.String" => 0x0300,
            "int" or "System.Int32" => 0x0102,
            "long" or "System.Int64" => 0x0103,
            "bool" or "System.Boolean" => 0x0101,
            "double" or "System.Double" => 0x0100,
            "decimal" or "System.Decimal" => 0x0100,
            "System.DateTime" => 0x0200,
            "System.Guid" => 0,
            _ => 0x0300
        };
    }

    /// <summary>
    /// 获取类型的默认长度
    /// </summary>
    private static int GetDefaultLength(ITypeSymbol type)
    {
        var unwrappedType = UnwrapNullableType(type);
        var fullTypeName = unwrappedType.ToString();
        return fullTypeName switch
        {
            "string" or "System.String" => 255,
            _ => 0
        };
    }

    /// <summary>
    /// 解包可空类型
    /// </summary>
    private static ITypeSymbol UnwrapNullableType(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType && namedType.IsGenericType &&
            namedType.ConstructedFrom?.Name == "Nullable")
        {
            return namedType.TypeArguments[0];
        }
        return type;
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
    /// 读取属性元数据（检查 [MoColumn]、[MoColumnIndex] 特性）
    /// 返回 null 表示该属性不参与源生成（MoTable 无 [MoColumn]）
    /// </summary>
    private static PropertyMetadata? GetPropertyMetadata(IPropertySymbol property, bool isQuery)
    {
        if (isQuery)
        {
            // 查询对象：不读取 [MoColumn]，使用默认推断值
            return new PropertyMetadata
            {
                Symbol = property,
                ColumnName = property.Name,
                IsPrimaryKey = false,
                IsAutoIncrement = false,
                Type = InferColumnType(property.Type),
                Length = GetDefaultLength(property.Type),
                Precision = 0,
                Comment = string.Empty,
                ColumnIndexes = new List<ColumnIndexEntry>()
            };
        }

        // 实体表：读取 [MoColumn] 特性
        var moColumnAttr = property.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.Name == "MoColumnAttribute");

        // 没有 [MoColumn] 特性则不参与源生成
        if (moColumnAttr == null) { return null; }

        // 读取 [MoColumn] 的 Name 参数（构造函数参数）
        var columnName = moColumnAttr.ConstructorArguments
            .FirstOrDefault().Value?.ToString();

        // 读取 [MoColumn] 的命名属性
        var isPrimaryKey = false;
        var isAutoIncrement = false;
        var type = 0;
        var length = 0;
        var precision = 0;
        var comment = string.Empty;
        foreach (var namedArg in moColumnAttr.NamedArguments)
        {
            switch (namedArg.Key)
            {
                case "Key":
                    isPrimaryKey = (bool)(namedArg.Value.Value ?? false);
                    break;
                case "AutoIncrement":
                    isAutoIncrement = (bool)(namedArg.Value.Value ?? false);
                    break;
                case "Type":
                    type = (int)(namedArg.Value.Value ?? 0);
                    break;
                case "Length":
                    length = (int)(namedArg.Value.Value ?? 0);
                    break;
                case "Precision":
                    precision = (int)(namedArg.Value.Value ?? 0);
                    break;
                case "Comment":
                    comment = (string)(namedArg.Value.Value ?? string.Empty);
                    break;
            }
        }

        // 读取所有 [MoColumnIndex] 特性
        var columnIndexes = new List<ColumnIndexEntry>();
        foreach (var attr in property.GetAttributes())
        {
            if (attr.AttributeClass?.Name != "MoColumnIndexAttribute") { continue; }

            var indexName = string.Empty;
            var isUnique = false;

            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg.Key == "Name")
                    indexName = (string)(namedArg.Value.Value ?? string.Empty);
                else if (namedArg.Key == "IsUnique")
                    isUnique = (bool)(namedArg.Value.Value ?? false);
            }

            // Name 为空时使用属性名称
            if (string.IsNullOrEmpty(indexName))
                indexName = property.Name;

            columnIndexes.Add(new ColumnIndexEntry(indexName, isUnique));
        }

        return new PropertyMetadata
        {
            Symbol = property,
            ColumnName = columnName,
            IsPrimaryKey = isPrimaryKey,
            IsAutoIncrement = isAutoIncrement,
            Type = type,
            Length = length,
            Precision = precision,
            Comment = comment,
            ColumnIndexes = columnIndexes
        };
    }

    /// <summary>
    /// 将 ColumnType 枚举值转为代码表达式
    /// </summary>
    private static string GetColumnTypeExpression(int typeValue)
    {
        return typeValue switch
        {
            0x0100 => "ColumnType.DECIMAL",
            0x0101 => "ColumnType.BOOL",
            0x0102 => "ColumnType.INTEGER",
            0x0103 => "ColumnType.LONG",
            0x0200 => "ColumnType.TIME",
            0x0300 => "ColumnType.VARCHAR",
            0x0400 => "ColumnType.TEXT",
            _ => "ColumnType.UNSET"
        };
    }

    /// <summary>
    /// 转义字符串字面量中的特殊字符
    /// </summary>
    private static string EscapeStringLiteral(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    // 属性元数据
    private sealed class PropertyMetadata
    {
        public IPropertySymbol Symbol { get; set; } = null!;
        public string? ColumnName { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsAutoIncrement { get; set; }
        public int Type { get; set; }
        public int Length { get; set; }
        public int Precision { get; set; }
        public string Comment { get; set; } = string.Empty;
        public List<ColumnIndexEntry> ColumnIndexes { get; set; } = new();
    }

    // MoColumnIndex 条目
    private sealed class ColumnIndexEntry
    {
        public ColumnIndexEntry(string name, bool isUnique)
        {
            Name = name;
            IsUnique = isUnique;
        }

        public string Name { get; }
        public bool IsUnique { get; }
    }

    // 索引合并中间条目
    private sealed class IndexMergeEntry
    {
        public List<string> Columns { get; } = new();
        public bool IsUnique { get; set; }
    }

    // 最终索引元数据
    private sealed class IndexMetadata
    {
        public IndexMetadata(string name, string[] columns, bool isUnique)
        {
            Name = name;
            Columns = columns;
            IsUnique = isUnique;
        }

        public string Name { get; }
        public string[] Columns { get; }
        public bool IsUnique { get; }
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