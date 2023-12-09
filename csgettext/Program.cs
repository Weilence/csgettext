using CommandLine;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
return;

void Run(Options options)
{
    var texts = new HashSet<string>();
    foreach (var file in Directory.EnumerateFiles(options.Directory, "*.cs", SearchOption.AllDirectories))
    {
        var code = File.ReadAllText(file);
        foreach (var text in FindStringLocalizerUsage(code))
        {
            texts.Add(text);
        }
    }

    File.WriteAllText(options.Output, string.Join("\n", texts.Select(m => $"msgid {m}\nmsgstr \"\"\n")));
}

static HashSet<string> FindStringLocalizerUsage(string code)
{
    var texts = new HashSet<string>();

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
            texts.Add(str);
        }
    }

    return texts;
}

static bool IsIStringLocalizer(TypeSyntax type)
{
    if (type is GenericNameSyntax genericName)
    {
        return genericName.Identifier.ValueText == "IStringLocalizer";
    }

    return type.ToString() == "IStringLocalizer";
}

public class Options
{
    [Option('d', "dir", Default = "./")]
    public required string Directory { get; set; }

    [Option('o', "output", Default = "./text.pot")]
    public required string Output { get; set; }
}