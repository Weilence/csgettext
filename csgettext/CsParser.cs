using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace csgettext;

public class CsParser
{
    public List<string> Parse(string code)
    {
        var msgIds = new List<string>();

        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root = syntaxTree.GetRoot();
        var classDeclarationSyntaxes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
        foreach (var classDeclarationSyntax in classDeclarationSyntaxes)
        {
            var argsNames = classDeclarationSyntax.ParameterList?.Parameters
                .Where(m => m.Type != null && IsIStringLocalizer(m.Type))
                .Select(m => m.Identifier.ValueText)
                .ToList() ?? [];

            var fieldNames = classDeclarationSyntax.DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .Where(m =>
                {
                    if (!m.Modifiers.Any(n => n.IsKind(SyntaxKind.PrivateKeyword)))
                    {
                        return false;
                    }

                    if (!m.Modifiers.Any(n => n.IsKind(SyntaxKind.ReadOnlyKeyword)))
                    {
                        return false;
                    }

                    return IsIStringLocalizer(m.Declaration.Type);
                })
                .SelectMany(m => m.Declaration.Variables.Select(n => n.Identifier.ValueText))
                .ToList();

            var variables = new HashSet<string>();
            foreach (var argsName in argsNames)
            {
                variables.Add(argsName);
            }

            foreach (var fieldName in fieldNames)
            {
                variables.Add(fieldName);
            }

            var strs = classDeclarationSyntax.DescendantNodes()
                .OfType<ElementAccessExpressionSyntax>()
                .Where(m =>
                {
                    if (m.Expression is not IdentifierNameSyntax identifier)
                    {
                        return false;
                    }

                    return variables.Contains(identifier.Identifier.ValueText) && m.ArgumentList.Arguments.Count > 0;
                })
                .Select(m => m.ArgumentList.Arguments.First().ToString())
                .ToList();

            foreach (var str in strs)
            {
                msgIds.Add(str.Trim('"'));
            }
        }

        return msgIds;
    }

    private bool IsIStringLocalizer(TypeSyntax type)
    {
        if (type is GenericNameSyntax genericName)
        {
            return genericName.Identifier.ValueText == "IStringLocalizer";
        }

        return type.ToString() == "IStringLocalizer";
    }

}