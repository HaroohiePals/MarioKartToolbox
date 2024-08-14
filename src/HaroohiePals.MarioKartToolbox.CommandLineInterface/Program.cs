using HaroohiePals.Nitro.Card;
using HaroohiePals.NitroKart.Rom;

string romFilePath = @"testfiles/rom.nds";
string outputPath = @"testfiles/project";

var rom = new NdsRom(File.ReadAllBytes(romFilePath));

var factory = new MkdsRomProjectFactory();

await factory.CreateAsync(rom, "MkdsTest", outputPath);