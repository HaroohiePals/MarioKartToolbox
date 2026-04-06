using HaroohiePals.NitroKart.Rom;
using Newtonsoft.Json;
using System.CommandLine;

namespace HaroohiePals.MarioKartToolbox.CommandLineInterface.Commands;

sealed class BuildMkdsRomCommand : Command
{
    public BuildMkdsRomCommand() 
        : base("build", "Build a Mario Kart DS ROM.")
    {
        var input = new Argument<string>("input", "Input Project JSON.");
        var output = new Option<string>("--output", "Output file name.");

        output.AddAlias("-o");

        Add(input);
        Add(output);

        this.SetHandler(HandleAsync, input, output);
    }

    private async Task HandleAsync(string inputPath, string? outputFileName)
    {
        if (!File.Exists(inputPath))
        {
            Console.WriteLine("The input file does not exist.");
            return;
        }

        var fileInfo = new FileInfo(inputPath);
        var romFactory = new MkdsRomFactory();
        var project = JsonConvert.DeserializeObject<MkdsRomProject>(await File.ReadAllTextAsync(inputPath));

        if (project is null)
        {
            Console.WriteLine("Invalid project file.");
            return;
        }

        outputFileName ??= $"{project.Name}.nds";

        var rom = await romFactory.CreateAsync(project, fileInfo.DirectoryName);

        await File.WriteAllBytesAsync(outputFileName, rom.Write(true));
    }
}