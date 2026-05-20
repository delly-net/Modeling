using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Delly.Modeling.Generator;

/// <summary>
/// Modelable 特性语法接收器，收集带有 Modelable 特性的类声明
/// </summary>
public class ModelableSyntaxReceiver : ISyntaxReceiver
{
    /// <summary>
    /// 收集的类声明列表
    /// </summary>
    public List<ClassDeclarationSyntax> Classes { get; } = new();

    /// <summary>
    /// 访问语法节点时的回调
    /// </summary>
    /// <param name="syntaxNode">当前访问的语法节点</param>
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclaration &&
            HasModelableAttribute(classDeclaration))
        {
            Classes.Add(classDeclaration);
        }
    }

    // 检查类是否具有 Modelable 特性
    private static bool HasModelableAttribute(ClassDeclarationSyntax classDeclaration)
    {
        foreach (var attributeList in classDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeName = attribute.Name.ToString();
                if (attributeName == "Modelable" ||
                    attributeName == "ModelableAttribute" ||
                    attributeName.EndsWith(".Modelable") ||
                    attributeName.EndsWith(".ModelableAttribute"))
                {
                    return true;
                }
            }
        }
        return false;
    }
}