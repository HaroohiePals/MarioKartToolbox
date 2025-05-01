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
        var noUnpack = new Option<bool>("--no-unpack", () => false, "Disables archives unpacking (.carc).");

        output.AddAlias("-o");
        noUnpack.AddAlias("-n");

        Add(input);
        Add(projectName);
        Add(output);
        Add(noUnpack);

        this.SetHandler(HandleAsync, input, projectName, output, noUnpack);
    }

    private async Task HandleAsync(string inputPath, string projectName, string? outputPath, bool noUnpackArc)
    {
        if (!File.Exists(inputPath))
        {
            Console.WriteLine("The input file does not exist.");
            return;
        }

        if (outputPath is null)
            outputPath = projectName;

        if (Directory.Exists(outputPath) && Directory.EnumerateFiles(outputPath).Any())
        {
            Console.WriteLine("The output folder is not empty.");
            return;
        }

        var rom = new NdsRom(await File.ReadAllBytesAsync(inputPath));
        var projectFactory = new MkdsRomProjectFactory();
        await projectFactory.CreateAsync(rom, projectName, outputPath, !noUnpackArc);
    }
}