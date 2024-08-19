using HaroohiePals.Nitro.Card;
using HaroohiePals.NitroKart.Rom;
using Newtonsoft.Json;

var projectFactory = new MkdsRomProjectFactory();
var romFactory = new MkdsRomFactory();

string romFilePath = @"testfiles/rom.nds";
string rebuiltRomFilePath = @"testfiles/rom_rebuilt.nds";
string outputPath = @"testfiles/project";
string projectPath = @"testfiles/project/MkdsTest.json";

Directory.Delete(outputPath, true);

byte[] sourceRomBytes = File.ReadAllBytes(romFilePath);
var rom = new NdsRom(sourceRomBytes);

await projectFactory.CreateAsync(rom, "MkdsTest", outputPath);

var project = JsonConvert.DeserializeObject<MkdsRomProject>(File.ReadAllText(projectPath));
var rebuiltRom = await romFactory.CreateAsync(project, outputPath);

byte[] rebuiltRomBytes = rebuiltRom.Write(false);
File.WriteAllBytes(rebuiltRomFilePath, rebuiltRomBytes);