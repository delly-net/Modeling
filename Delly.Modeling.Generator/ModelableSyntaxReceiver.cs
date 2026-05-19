using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Delly.Modeling.Generator;

public class ModelableSyntaxReceiver : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> Classes { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclaration &&
            HasModelableAttribute(classDeclaration))
        {
            Classes.Add(classDeclaration);
        }
    }

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