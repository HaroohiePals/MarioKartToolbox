using HaroohiePals.Nitro.Card;
using HaroohiePals.NitroKart.Rom;
using System.CommandLine;

namespace HaroohiePals.MarioKartToolbox.CommandLineInterface.Commands;

sealed class CreateMkdsRomProjectCommand : Command
{
    public CreateMkdsRomProjectCommand() 
        : base("create", "Creates a Mario Kart DS ROM Project.")
    {
        var input = new Argument<string>("input", "Input ROM Path.");
        var projectName = new Argument<string>("name", "Project Name.");
        var output = new Option<string>("--output", "Output Project Path. Must be empty.");
        var unpackArc = new Option<bool>("--unpackarc", () => false, "Unpack archives (CARC).");

        output.AddAlias("-o");
        unpackArc.AddAlias("-u");

        Add(input);
        Add(projectName);
        Add(output);
        Add(unpackArc);

        this.SetHandler(HandleAsync, input, projectName, output, unpackArc);
    }

    private async Task HandleAsync(string inputPath, string projectName, string? outputPath, bool unpackArc)
    {
        if (!File.Exists(inputPath))
        {
            Console.WriteLine("The input file does not exist.");
            return;
        }

        if (outputPath is null)
            outputPath = projectName;

        if (Directory.Exists(outputPath) && Directory.EnumerateFiles(outputPath).Count() > 0)
        {
            Console.WriteLine("The output folder is not empty.");
            return;
        }

        var rom = new NdsRom(File.ReadAllBytes(inputPath));
        var projectFactory = new MkdsRomProjectFactory();
        await projectFactory.CreateAsync(rom, projectName, outputPath, unpackArc);
    }
}