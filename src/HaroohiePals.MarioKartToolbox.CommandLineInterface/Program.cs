using HaroohiePals.Nitro.Card;
using HaroohiePals.NitroKart.Rom;
using Newtonsoft.Json;

var projectFactory = new MkdsRomProjectFactory();
var romFactory = new MkdsRomFactory();

string romFilePath = @"testfiles/rom.nds";
string rebuiltRomFilePath = @"testfiles/rom_rebuilt.nds";
string projectName = "MkdsTest";
string outputPath = @"testfiles/project";
string projectPath = @$"testfiles/project/{projectName}.json";

bool createProject = true;
bool createRom = true;

if (createProject)
{
    if (Directory.Exists(outputPath))
        Directory.Delete(outputPath, true);

    byte[] sourceRomBytes = File.ReadAllBytes(romFilePath);
    var rom = new NdsRom(sourceRomBytes);

    await projectFactory.CreateAsync(rom, projectName, outputPath, true);
}

if (createRom)
{
    var project = JsonConvert.DeserializeObject<MkdsRomProject>(File.ReadAllText(projectPath));
    var rebuiltRom = await romFactory.CreateAsync(project, outputPath);

    byte[] rebuiltRomBytes = rebuiltRom.Write(true);
    File.WriteAllBytes(rebuiltRomFilePath, rebuiltRomBytes);
}