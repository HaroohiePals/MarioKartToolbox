using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Model;
using System.IO;
using System.Linq;
using Xunit;

namespace HaroohiePals.Nitro.NitroSystem.Test.G3d.Intermediate.Model
{
    public class ImdTest
    {
        //[Theory]
        //[InlineData("Note", @"d:\Projects\DS\CW\smgds\files\data\ObjectData\Note\Note.imd",
        //    @"d:\Projects\DS\CW\smgds\files\data\ObjectData\Note\Note.nsbmd")]
        //[InlineData("HeavenlyBeachPla",
        //    @"d:\Projects\DS\CW\smgds\files\data\ObjectData\HeavenlyBeachPlanet\HeavenlyBeachPlanet.imd",
        //    @"d:\Projects\DS\CW\smgds\files\data\ObjectData\HeavenlyBeachPlanet\HeavenlyBeachPlanet.nsbmd")]
        //[InlineData("PowerStar", @"d:\Projects\DS\CW\smgds\files\data\ObjectData\PowerStar\PowerStar.imd",
        //    @"d:\Projects\DS\CW\smgds\files\data\ObjectData\PowerStar\PowerStar.nsbmd")]
        //[InlineData("Kinopio", @"d:\Projects\DS\CW\smgds\files\data\ObjectData\Kinopio\Kinopio.imd",
        //    @"d:\Projects\DS\CW\smgds\files\data\ObjectData\Kinopio\Kinopio.nsbmd")]
        //[InlineData("LensFlare", @"d:\Projects\DS\CW\smgds\files\data\ObjectData\LensFlare\LensFlare.imd",
        //    @"d:\Projects\DS\CW\smgds\files\data\ObjectData\LensFlare\LensFlare.nsbmd")]
        //[InlineData("evp_wgt", @"d:\SDKs\NitroSystem\build\demos\g3d\samples\Envelope\data\src\evp_wgt.imd",
        //    @"d:\SDKs\NitroSystem\build\demos\g3d\samples\Envelope\data\evp_wgt.nsbmd")]
        //public void ImdToNsbmd(string modelName, string imdPath, string nsbmdPath)
        //{
        //    var imd              = new Imd(File.ReadAllBytes(imdPath));
        //    var g3dcvtrNsbmd     = File.ReadAllBytes(nsbmdPath);
        //    var g3dcvtrNsbmdData = new Nsbmd(g3dcvtrNsbmd);
        //    var nsbmd            = imd.ToNsbmd(modelName);
        //    var imdNsbmd         = nsbmd.Write();
        //    nsbmd = new Nsbmd(imdNsbmd);

        //    Assert.Equal(g3dcvtrNsbmdData.ModelSet.Models[0].Sbc, nsbmd.ModelSet.Models[0].Sbc);

        //    Assert.Equal(g3dcvtrNsbmd, imdNsbmd);
        //}

        [Theory]
        [InlineData("cube")]
        [InlineData("mat/cube_no1dot")]
        [InlineData("mat/cube_nodepthdecal")]
        [InlineData("mat/cube_nofarclip")]
        [InlineData("mat/cube_noxludepth")]
        [InlineData("mat/cube_shininess")]
        [InlineData("mat/cube_wire")]
        public void ImdToNsbmd2(string testPath)
        {
            string basePath         = Path.Join("Assets", "imd", testPath);
            var    imd              = new Imd(File.ReadAllBytes(basePath + ".imd"));
            var    g3dcvtrNsbmd     = File.ReadAllBytes(basePath + ".nsbmd");
            var    g3dcvtrNsbmdData = new Nsbmd(g3dcvtrNsbmd);

            string name = Path.GetFileName(testPath);
            if (name.Length > 16)
                name = name[..16];

            var nsbmd    = imd.ToNsbmd(name);
            var imdNsbmd = nsbmd.Write();
            nsbmd = new Nsbmd(imdNsbmd);

            Assert.Equal(g3dcvtrNsbmdData.ModelSet.Models[0].Sbc, nsbmd.ModelSet.Models[0].Sbc);

            Assert.Equal(g3dcvtrNsbmd, imdNsbmd);
        }

        [Theory]
        //[InlineData(true, @"D:\dev\projects\nitro\haruhi-ds\haruhiDS\HaruhiTest\files\data\Actor\StagePart\sosdan_room\model.nsbtx", @"D:\dev\projects\nitro\haruhi-ds\haruhiDS\HaruhiTest\files\data\Actor\StagePart\sosdan_room\model.imd")]
        //[InlineData(true, @"D:\dev\projects\nitro\haruhi-ds\haruhiDS\HaruhiTest\files\data\Actor\StagePart\test_stage\model.nsbtx", @"D:\dev\projects\nitro\haruhi-ds\haruhiDS\HaruhiTest\files\data\Actor\StagePart\test_stage\model.imd")]
        //[InlineData(true, @"D:\dev\projects\nitro\haruhi-ds\haruhiDS\HaruhiTest\files\data\Actor\StagePart\nishikita_park\model.nsbtx", @"D:\dev\projects\nitro\haruhi-ds\haruhiDS\HaruhiTest\files\data\Actor\StagePart\nishikita_park\model.imd")]

        //[InlineData(true, @"D:\dev\projects\nitro\haruhi-ds\haruhiDS\HaruhiTest\files\data\Actor\StagePart\model_combined.nsbtx",
        //    @"D:\dev\projects\nitro\haruhi-ds\haruhiDS\HaruhiTest\files\data\Actor\StagePart\sosdan_room\model.imd",
        //    @"D:\dev\projects\nitro\haruhi-ds\haruhiDS\HaruhiTest\files\data\Actor\StagePart\test_stage\model.imd",
        //    @"D:\dev\projects\nitro\haruhi-ds\haruhiDS\HaruhiTest\files\data\Actor\StagePart\nishikita_park\model.imd")]

        [InlineData(true, @"Assets/imd/tex/cube_nopow2.nsbtx", @"Assets/imd/tex/cube_nopow2.imd")]
        [InlineData(true, @"Assets/imd/tex/tex_combined.nsbtx", @"Assets/imd/tex/tex1.imd", @"Assets/imd/tex/tex2.imd", @"Assets/imd/tex/tex3.imd")]
        [InlineData(true, @"Assets/imd/tex/pal.nsbtx", @"Assets/imd/tex/pal.imd")]
        [InlineData(false, @"Assets/imd/tex/pal_nomerge.nsbtx", @"Assets/imd/tex/pal.imd")]
        public void ImdToNsbtx(bool mergeSameData, string nsbtxPath, params string[] imdPaths)
        {
            var inputImds = imdPaths.Select(x => new Imd(File.ReadAllBytes(x))).ToArray();
            var g3dcvtrNsbtx = File.ReadAllBytes(nsbtxPath);
            var g3dcvtrNsbtxData = new Nsbtx(g3dcvtrNsbtx);

            var nsbtx = Imd.ToNsbtx(inputImds, mergeSameData);
            var imdNsbtx = nsbtx.Write();

            //File.WriteAllBytes("Assets/test/original.nsbtx", g3dcvtrNsbtx);
            //File.WriteAllBytes("Assets/test/imd.nsbtx", imdNsbtx);

            Assert.Equal(g3dcvtrNsbtx, imdNsbtx);
        }
    }
}