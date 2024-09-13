using HaroohiePals.MarioKartToolbox.CommandLineInterface.Commands;
using System.CommandLine;

namespace HaroohiePals.MarioKartToolbox.CommandLineInterface;

class Program
{
    private const string ROOT_COMMAND_DESCRIPTION = "Mario Kart Toolbox";

    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand(ROOT_COMMAND_DESCRIPTION);

        rootCommand.AddCommand(new CreateMkdsRomProjectCommand());
        rootCommand.AddCommand(new BuildMkdsRomCommand());

        return await rootCommand.InvokeAsync(args);
    }
}