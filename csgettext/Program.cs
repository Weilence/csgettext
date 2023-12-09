using CommandLine;
using csgettext;

Parser.Default.ParseArguments<Options>(args).WithParsed(Run);
return;

void Run(Options options)
{
    var parser = new CsParser();
    var potFile = new PotFile();

    foreach (var file in Directory.EnumerateFiles(options.Directory, "*.cs", SearchOption.AllDirectories))
    {
        var code = File.ReadAllText(file);
        potFile.Add(parser.Parse(code));
    }

    var outputPath = Directory.GetParent(options.Output);
    if (outputPath == null)
    {
        Console.WriteLine("Output path error.");
        return;
    }

    File.WriteAllText(options.Output, potFile.ToContent());
    foreach (var language in options.Languages)
    {
        try
        {
            var poContent = File.ReadAllText(Path.Combine(outputPath.FullName, language + ".po"));
            var poFile = new PoFile(poContent);
            File.WriteAllText(Path.Combine(outputPath.FullName, language + ".po"), poFile.ToContent(potFile));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Merge {language}.po file error: {e.Message}");
        }
    }
}

public class Options
{
    [Option('d', "dir", Default = "./")]
    public required string Directory { get; set; }

    [Option('o', "output", Default = "./template.pot")]
    public required string Output { get; set; }

    [Option('l', "languages")]
    public required IEnumerable<string> Languages { get; set; } = [];
}