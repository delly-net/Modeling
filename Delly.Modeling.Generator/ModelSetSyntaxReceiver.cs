#if !NETSTANDARD2_0
#nullable enable
#endif

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Delly.Modeling.Generator;

/// <summary>
/// ModelSet 特性语法接收器，收集带有 MoSet 特性的类声明
/// </summary>
public class ModelSetSyntaxReceiver : ISyntaxReceiver
{
    /// <summary>
    /// 收集的 MoSet 类声明列表
    /// </summary>
    public List<ClassDeclarationSyntax> MoSetClasses { get; } = new();

    /// <summary>
    /// 收集的 MoTable 和 MoQuery 类声明列表
    /// </summary>
    public List<ClassDeclarationSyntax> ModelClasses { get; } = new();

    /// <summary>
    /// 访问语法节点时的回调
    /// </summary>
    /// <param name="syntaxNode">当前访问的语法节点</param>
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclaration)
        {
            if (HasMoSetAttribute(classDeclaration))
            {
                MoSetClasses.Add(classDeclaration);
            }
            else if (HasModelAttribute(classDeclaration))
            {
                ModelClasses.Add(classDeclaration);
            }
        }
    }

    // 检查类是否具有 MoSet 特性
    private static bool HasMoSetAttribute(ClassDeclarationSyntax classDeclaration)
    {
        foreach (var attributeList in classDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeName = attribute.Name.ToString();
                if (attributeName == "MoSet" ||
                    attributeName == "MoSetAttribute" ||
                    attributeName.EndsWith(".MoSet") ||
                    attributeName.EndsWith(".MoSetAttribute"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    // 检查类是否具有 MoTable 或 MoQuery 特性
    private static bool HasModelAttribute(ClassDeclarationSyntax classDeclaration)
    {
        foreach (var attributeList in classDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeName = attribute.Name.ToString();
                if (attributeName == "MoTable" ||
                    attributeName == "MoTableAttribute" ||
                    attributeName.EndsWith(".MoTable") ||
                    attributeName.EndsWith(".MoTableAttribute") ||
                    attributeName == "MoQuery" ||
                    attributeName == "MoQueryAttribute" ||
                    attributeName.EndsWith(".MoQuery") ||
                    attributeName.EndsWith(".MoQueryAttribute"))
                {
                    return true;
                }
            }
        }
        return false;
    }
}