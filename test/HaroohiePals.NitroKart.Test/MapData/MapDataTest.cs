using HaroohiePals.NitroKart.MapData.Binary;
using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapObj;
using HaroohiePals.NitroKart.Validation.MapData;
using HaroohiePals.Validation;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace HaroohiePals.NitroKart.Test.MapData
{
    public class MapDataTest
    {
        private static readonly string BasePath = Environment.GetEnvironmentVariable("NITROKART_TEST_COURSES_PATH");

        [Theory]
        //Final
        [InlineData(@"final\airship_course_arc\course_map.nkm")]
        //[InlineData(@"final\Award_arc\course_map.nkm")]
        [InlineData(@"final\bank_course_arc\course_map.nkm")]
        [InlineData(@"final\beach_courseD_arc\course_map.nkm")]
        [InlineData(@"final\beach_course_arc\course_map.nkm")]
        [InlineData(@"final\clock_course_arc\course_map.nkm")]
        [InlineData(@"final\cross_courseD_arc\course_map.nkm")]
        [InlineData(@"final\cross_course_arc\course_map.nkm")]
        [InlineData(@"final\desert_course_arc\course_map.nkm")]
        [InlineData(@"final\dokan_course_arc\course_map.nkm")]
        [InlineData(@"final\donkey_course_arc\course_map.nkm")]
        [InlineData(@"final\garden_course_arc\course_map.nkm")]
        [InlineData(@"final\koopa_course_arc\course_map.nkm")]
        [InlineData(@"final\luigi_course_arc\course_map.nkm")]
        [InlineData(@"final\mansion_courseD_arc\course_map.nkm")]
        [InlineData(@"final\mansion_course_arc\course_map.nkm")]
        [InlineData(@"final\mario_course_arc\course_map.nkm")]
        [InlineData(@"final\mini_block_64_arc\course_map.nkm")]
        [InlineData(@"final\mini_block_course_arc\course_map.nkm")]
        [InlineData(@"final\mini_dokan_gc_arc\course_map.nkm")]
        [InlineData(@"final\mini_stage1_arc\course_map.nkm")]
        [InlineData(@"final\mini_stage2_arc\course_map.nkm")]
        [InlineData(@"final\mini_stage3_arc\course_map.nkm")]
        [InlineData(@"final\mini_stage4_arc\course_map.nkm")]
        //Those courses don't have proper respawn points set despite being referenced
        //by ID, the reference resolver would generate an exception
        //[InlineData(@"final\MR_stage1_arc\course_map.nkm")]
        //[InlineData(@"final\MR_stage2_arc\course_map.nkm")]
        //[InlineData(@"final\MR_stage3_arc\course_map.nkm")]
        [InlineData(@"final\MR_stage4_arc\course_map.nkm")]
        [InlineData(@"final\nokonoko_course_arc\course_map.nkm")]
        [InlineData(@"final\old_baby_gc_arc\course_map.nkm")]
        [InlineData(@"final\old_choco_64_arc\course_map.nkm")]
        [InlineData(@"final\old_choco_sfc_arc\course_map.nkm")]
        [InlineData(@"final\old_donut_sfc_arc\course_map.nkm")]
        [InlineData(@"final\old_frappe_64_arc\course_map.nkm")]
        [InlineData(@"final\old_hyudoro_64_arc\course_map.nkm")]
        [InlineData(@"final\old_kinoko_gc_arc\course_map.nkm")]
        [InlineData(@"final\old_koopa_agb_arc\course_map.nkm")]
        [InlineData(@"final\old_luigi_agb_arc\course_map.nkm")]
        [InlineData(@"final\old_luigi_gcD_arc\course_map.nkm")]
        [InlineData(@"final\old_luigi_gc_arc\course_map.nkm")]
        [InlineData(@"final\old_mario_gc_arc\course_map.nkm")]
        [InlineData(@"final\old_mario_sfc_arc\course_map.nkm")]
        [InlineData(@"final\old_momo_64D_arc\course_map.nkm")]
        [InlineData(@"final\old_momo_64_arc\course_map.nkm")]
        [InlineData(@"final\old_noko_sfc_arc\course_map.nkm")]
        [InlineData(@"final\old_peach_agb_arc\course_map.nkm")]
        [InlineData(@"final\old_sky_agb_arc\course_map.nkm")]
        [InlineData(@"final\old_yoshi_gc_arc\course_map.nkm")]
        [InlineData(@"final\pinball_course_arc\course_map.nkm")]
        [InlineData(@"final\rainbow_course_arc\course_map.nkm")]
        [InlineData(@"final\ridge_course_arc\course_map.nkm")]
        //DK Pass has respawn points with non-ordered indices, this would probably need a separate test
        //[InlineData(@"final\snow_course_arc\course_map.nkm")]
        [InlineData(@"final\stadium_course_arc\course_map.nkm")]
        [InlineData(@"final\StaffRollTrue_arc\course_map.nkm")]
        [InlineData(@"final\StaffRoll_arc\course_map.nkm")]
        [InlineData(@"final\test1_course_arc\course_map.nkm")]
        [InlineData(@"final\town_course_arc\course_map.nkm")]
        [InlineData(@"final\wario_course_arc\course_map.nkm")]

        //Skip testing mission courses because their nkm have unproper item points or
        //enemy points references, this would probably need a separate test
        //[InlineData(@"final\airship_course_arc\MissionRun\mr08_tool.nkm")]
        //[InlineData(@"final\airship_course_arc\MissionRun\mr69_tool.nkm")]
        //[InlineData(@"final\bank_course_arc\MissionRun\mr12_tool.nkm")]
        //[InlineData(@"final\bank_course_arc\MissionRun\mr85_tool.nkm")]
        //[InlineData(@"final\beach_course_arc\MissionRun\mr11_tool.nkm")]
        //[InlineData(@"final\beach_course_arc\MissionRun\mr82_tool.nkm")]
        //[InlineData(@"final\clock_course_arc\MissionRun\mr07_tool.nkm")]
        //[InlineData(@"final\clock_course_arc\MissionRun\mr33_tool.nkm")]
        //[InlineData(@"final\cross_course_arc\MissionRun\mr13_tool.nkm")]
        //[InlineData(@"final\cross_course_arc\MissionRun\mr54_tool.nkm")]
        //[InlineData(@"final\desert_course_arc\MissionRun\mr41_tool.nkm")]
        //[InlineData(@"final\desert_course_arc\MissionRun\mr61_tool.nkm")]
        //[InlineData(@"final\garden_course_arc\MissionRun\mr04_tool.nkm")]
        //[InlineData(@"final\garden_course_arc\MissionRun\mr90_tool.nkm")]
        //[InlineData(@"final\koopa_course_arc\MissionRun\mr64_tool.nkm")]
        //[InlineData(@"final\koopa_course_arc\MissionRun\mr89_tool.nkm")]
        //[InlineData(@"final\mansion_course_arc\MissionRun\mr14_tool.nkm")]
        //[InlineData(@"final\mansion_course_arc\MissionRun\mr20_tool.nkm")]
        //[InlineData(@"final\mario_course_arc\MissionRun\mr72_tool.nkm")]
        //[InlineData(@"final\mario_course_arc\MissionRun\mr91_tool.nkm")]
        //[InlineData(@"final\mini_block_64_arc\MissionRun\mr79_tool.nkm")]
        //[InlineData(@"final\mini_block_course_arc\MissionRun\mr10_tool.nkm")]
        //[InlineData(@"final\mini_dokan_gc_arc\MissionRun\mr76_tool.nkm")]
        //[InlineData(@"final\mini_stage1_arc\MissionRun\mr77_tool.nkm")]
        //[InlineData(@"final\mini_stage1_arc\MissionRun\mr81_tool.nkm")]
        //[InlineData(@"final\mini_stage2_arc\MissionRun\mr80_tool.nkm")]
        //[InlineData(@"final\mini_stage3_arc\MissionRun\mr39_tool.nkm")]
        //[InlineData(@"final\mini_stage4_arc\MissionRun\mr78_tool.nkm")]
        //[InlineData(@"final\MR_stage1_arc\MissionRun\mr09_tool.nkm")]
        //[InlineData(@"final\MR_stage1_arc\MissionRun\mr10_tool.nkm")]
        //[InlineData(@"final\MR_stage2_arc\MissionRun\mr17_tool.nkm")]
        //[InlineData(@"final\MR_stage2_arc\MissionRun\mr50_tool.nkm")]
        //[InlineData(@"final\MR_stage3_arc\MissionRun\mr51_tool.nkm")]
        //[InlineData(@"final\MR_stage4_arc\MissionRun\mr49_tool.nkm")]
        //[InlineData(@"final\old_baby_gc_arc\MissionRun\mr21_tool.nkm")]
        //[InlineData(@"final\old_baby_gc_arc\MissionRun\mr38_tool.nkm")]
        //[InlineData(@"final\old_choco_64_arc\MissionRun\mr83_tool.nkm")]
        //[InlineData(@"final\old_choco_sfc_arc\MissionRun\mr15_tool.nkm")]
        //[InlineData(@"final\old_donut_sfc_arc\MissionRun\mr43_tool.nkm")]
        //[InlineData(@"final\old_frappe_64_arc\MissionRun\mr28_tool.nkm")]
        //[InlineData(@"final\old_hyudoro_64_arc\MissionRun\mr86_tool.nkm")]
        //[InlineData(@"final\old_kinoko_gc_arc\MissionRun\mr49_tool.nkm")]
        //[InlineData(@"final\old_kinoko_gc_arc\MissionRun\mr53_tool.nkm")]
        //[InlineData(@"final\old_koopa_agb_arc\MissionRun\mr27_tool.nkm")]
        //[InlineData(@"final\old_koopa_agb_arc\MissionRun\mr37_tool.nkm")]
        //[InlineData(@"final\old_luigi_agb_arc\MissionRun\mr66_tool.nkm")]
        //[InlineData(@"final\old_luigi_gc_arc\MissionRun\mr25_tool.nkm")]
        //[InlineData(@"final\old_luigi_gc_arc\MissionRun\mr34_tool.nkm")]
        //[InlineData(@"final\old_mario_sfc_arc\MissionRun\mr29_tool.nkm")]
        //[InlineData(@"final\old_momo_64_arc\MissionRun\mr02_tool.nkm")]
        //[InlineData(@"final\old_momo_64_arc\MissionRun\mr06_tool.nkm")]
        //[InlineData(@"final\old_noko_sfc_arc\MissionRun\mr48_tool.nkm")]
        //[InlineData(@"final\old_peach_agb_arc\MissionRun\mr55_tool.nkm")]
        //[InlineData(@"final\old_sky_agb_arc\MissionRun\mr44_tool.nkm")]
        //[InlineData(@"final\old_yoshi_gc_arc\MissionRun\mr40_tool.nkm")]
        //[InlineData(@"final\pinball_course_arc\MissionRun\mr70_tool.nkm")]
        //[InlineData(@"final\pinball_course_arc\MissionRun\mr75_tool.nkm")]
        //[InlineData(@"final\rainbow_course_arc\MissionRun\mr60_tool.nkm")]
        //[InlineData(@"final\ridge_course_arc\MissionRun\mr03_tool.nkm")]
        //[InlineData(@"final\ridge_course_arc\MissionRun\mr62_tool.nkm")]
        //[InlineData(@"final\snow_course_arc\MissionRun\mr35_tool.nkm")]
        //[InlineData(@"final\snow_course_arc\MissionRun\mr88_tool.nkm")]
        //[InlineData(@"final\stadium_course_arc\MissionRun\mr84_tool.nkm")]
        //[InlineData(@"final\town_course_arc\MissionRun\mr26_tool.nkm")]
        //[InlineData(@"final\town_course_arc\MissionRun\mr47_tool.nkm")]
        public void ReadWriteTest(string path)
        {
            var originalNkmData = File.ReadAllBytes(System.IO.Path.Combine(BasePath, path));

            var nkm = new Nkmd(originalNkmData);

            var inputNkmData = nkm.Write(); //Write and recalculate values in order to have a "clean" nkm

            var mapData = MkdsMapDataFactory.CreateFromNkm(new Nkmd(inputNkmData));

            var outputNkmData = NkmdFactory.FromMapData(mapData).Write();

            //File.WriteAllBytes("original.nkm", inputNkmData);
            //File.WriteAllBytes("rewritten.nkm", outputNkmData);

            Assert.Equal(inputNkmData, outputNkmData);
        }


        [Theory]
        //Final
        [InlineData(@"final\airship_course_arc\course_map.nkm")]
        [InlineData(@"final\Award_arc\course_map.nkm")]
        [InlineData(@"final\bank_course_arc\course_map.nkm")]
        [InlineData(@"final\beach_courseD_arc\course_map.nkm")]
        [InlineData(@"final\beach_course_arc\course_map.nkm")]
        [InlineData(@"final\clock_course_arc\course_map.nkm")]
        [InlineData(@"final\cross_courseD_arc\course_map.nkm")]
        [InlineData(@"final\cross_course_arc\course_map.nkm")]
        [InlineData(@"final\desert_course_arc\course_map.nkm")]
        [InlineData(@"final\dokan_course_arc\course_map.nkm")]
        [InlineData(@"final\donkey_course_arc\course_map.nkm")]
        [InlineData(@"final\garden_course_arc\course_map.nkm")]
        [InlineData(@"final\koopa_course_arc\course_map.nkm")]
        [InlineData(@"final\luigi_course_arc\course_map.nkm")]
        [InlineData(@"final\mansion_courseD_arc\course_map.nkm")]
        [InlineData(@"final\mansion_course_arc\course_map.nkm")]
        [InlineData(@"final\mario_course_arc\course_map.nkm")]
        [InlineData(@"final\mini_block_64_arc\course_map.nkm")]
        [InlineData(@"final\mini_block_course_arc\course_map.nkm")]
        [InlineData(@"final\mini_dokan_gc_arc\course_map.nkm")]
        [InlineData(@"final\mini_stage1_arc\course_map.nkm")]
        [InlineData(@"final\mini_stage2_arc\course_map.nkm")]
        [InlineData(@"final\mini_stage3_arc\course_map.nkm")]
        [InlineData(@"final\mini_stage4_arc\course_map.nkm")]
        [InlineData(@"final\MR_stage1_arc\course_map.nkm")]
        [InlineData(@"final\MR_stage2_arc\course_map.nkm")]
        [InlineData(@"final\MR_stage3_arc\course_map.nkm")]
        [InlineData(@"final\MR_stage4_arc\course_map.nkm")]
        [InlineData(@"final\nokonoko_course_arc\course_map.nkm")]
        [InlineData(@"final\old_baby_gc_arc\course_map.nkm")]
        [InlineData(@"final\old_choco_64_arc\course_map.nkm")]
        [InlineData(@"final\old_choco_sfc_arc\course_map.nkm")]
        [InlineData(@"final\old_donut_sfc_arc\course_map.nkm")]
        [InlineData(@"final\old_frappe_64_arc\course_map.nkm")]
        [InlineData(@"final\old_hyudoro_64_arc\course_map.nkm")]
        [InlineData(@"final\old_kinoko_gc_arc\course_map.nkm")]
        [InlineData(@"final\old_koopa_agb_arc\course_map.nkm")]
        [InlineData(@"final\old_luigi_agb_arc\course_map.nkm")]
        [InlineData(@"final\old_luigi_gcD_arc\course_map.nkm")]
        [InlineData(@"final\old_luigi_gc_arc\course_map.nkm")]
        [InlineData(@"final\old_mario_gc_arc\course_map.nkm")]
        [InlineData(@"final\old_mario_sfc_arc\course_map.nkm")]
        [InlineData(@"final\old_momo_64D_arc\course_map.nkm")]
        [InlineData(@"final\old_momo_64_arc\course_map.nkm")]
        [InlineData(@"final\old_noko_sfc_arc\course_map.nkm")]
        [InlineData(@"final\old_peach_agb_arc\course_map.nkm")]
        [InlineData(@"final\old_sky_agb_arc\course_map.nkm")]
        [InlineData(@"final\old_yoshi_gc_arc\course_map.nkm")]
        [InlineData(@"final\pinball_course_arc\course_map.nkm")]
        [InlineData(@"final\rainbow_course_arc\course_map.nkm")]
        [InlineData(@"final\ridge_course_arc\course_map.nkm")]
        [InlineData(@"final\snow_course_arc\course_map.nkm")]
        [InlineData(@"final\stadium_course_arc\course_map.nkm")]
        [InlineData(@"final\StaffRollTrue_arc\course_map.nkm")]
        [InlineData(@"final\StaffRoll_arc\course_map.nkm")]
        [InlineData(@"final\test1_course_arc\course_map.nkm")]
        [InlineData(@"final\test_circle_arc\course_map.nkm")]
        [InlineData(@"final\town_course_arc\course_map.nkm")]
        [InlineData(@"final\wario_course_arc\course_map.nkm")]
        [InlineData(@"final\airship_course_arc\MissionRun\mr08_tool.nkm")]
        [InlineData(@"final\airship_course_arc\MissionRun\mr69_tool.nkm")]
        [InlineData(@"final\bank_course_arc\MissionRun\mr12_tool.nkm")]
        [InlineData(@"final\bank_course_arc\MissionRun\mr85_tool.nkm")]
        [InlineData(@"final\beach_course_arc\MissionRun\mr11_tool.nkm")]
        [InlineData(@"final\beach_course_arc\MissionRun\mr82_tool.nkm")]
        [InlineData(@"final\clock_course_arc\MissionRun\mr07_tool.nkm")]
        [InlineData(@"final\clock_course_arc\MissionRun\mr33_tool.nkm")]
        [InlineData(@"final\cross_course_arc\MissionRun\mr13_tool.nkm")]
        [InlineData(@"final\cross_course_arc\MissionRun\mr54_tool.nkm")]
        [InlineData(@"final\desert_course_arc\MissionRun\mr41_tool.nkm")]
        [InlineData(@"final\desert_course_arc\MissionRun\mr61_tool.nkm")]
        [InlineData(@"final\garden_course_arc\MissionRun\mr04_tool.nkm")]
        [InlineData(@"final\garden_course_arc\MissionRun\mr90_tool.nkm")]
        [InlineData(@"final\koopa_course_arc\MissionRun\mr64_tool.nkm")]
        [InlineData(@"final\koopa_course_arc\MissionRun\mr89_tool.nkm")]
        [InlineData(@"final\mansion_course_arc\MissionRun\mr14_tool.nkm")]
        [InlineData(@"final\mansion_course_arc\MissionRun\mr20_tool.nkm")]
        [InlineData(@"final\mario_course_arc\MissionRun\mr72_tool.nkm")]
        [InlineData(@"final\mario_course_arc\MissionRun\mr91_tool.nkm")]
        [InlineData(@"final\mini_block_64_arc\MissionRun\mr79_tool.nkm")]
        [InlineData(@"final\mini_block_course_arc\MissionRun\mr10_tool.nkm")]
        [InlineData(@"final\mini_dokan_gc_arc\MissionRun\mr76_tool.nkm")]
        [InlineData(@"final\mini_stage1_arc\MissionRun\mr77_tool.nkm")]
        [InlineData(@"final\mini_stage1_arc\MissionRun\mr81_tool.nkm")]
        [InlineData(@"final\mini_stage2_arc\MissionRun\mr80_tool.nkm")]
        [InlineData(@"final\mini_stage3_arc\MissionRun\mr39_tool.nkm")]
        [InlineData(@"final\mini_stage4_arc\MissionRun\mr78_tool.nkm")]
        [InlineData(@"final\MR_stage1_arc\MissionRun\mr09_tool.nkm")]
        [InlineData(@"final\MR_stage1_arc\MissionRun\mr10_tool.nkm")]
        [InlineData(@"final\MR_stage2_arc\MissionRun\mr17_tool.nkm")]
        [InlineData(@"final\MR_stage2_arc\MissionRun\mr50_tool.nkm")]
        [InlineData(@"final\MR_stage3_arc\MissionRun\mr51_tool.nkm")]
        [InlineData(@"final\MR_stage4_arc\MissionRun\mr49_tool.nkm")]
        [InlineData(@"final\old_baby_gc_arc\MissionRun\mr21_tool.nkm")]
        [InlineData(@"final\old_baby_gc_arc\MissionRun\mr38_tool.nkm")]
        [InlineData(@"final\old_choco_64_arc\MissionRun\mr83_tool.nkm")]
        [InlineData(@"final\old_choco_sfc_arc\MissionRun\mr15_tool.nkm")]
        [InlineData(@"final\old_donut_sfc_arc\MissionRun\mr43_tool.nkm")]
        [InlineData(@"final\old_frappe_64_arc\MissionRun\mr28_tool.nkm")]
        [InlineData(@"final\old_hyudoro_64_arc\MissionRun\mr86_tool.nkm")]
        [InlineData(@"final\old_kinoko_gc_arc\MissionRun\mr49_tool.nkm")]
        [InlineData(@"final\old_kinoko_gc_arc\MissionRun\mr53_tool.nkm")]
        [InlineData(@"final\old_koopa_agb_arc\MissionRun\mr27_tool.nkm")]
        [InlineData(@"final\old_koopa_agb_arc\MissionRun\mr37_tool.nkm")]
        [InlineData(@"final\old_luigi_agb_arc\MissionRun\mr66_tool.nkm")]
        [InlineData(@"final\old_luigi_gc_arc\MissionRun\mr25_tool.nkm")]
        [InlineData(@"final\old_luigi_gc_arc\MissionRun\mr34_tool.nkm")]
        [InlineData(@"final\old_mario_sfc_arc\MissionRun\mr29_tool.nkm")]
        [InlineData(@"final\old_momo_64_arc\MissionRun\mr02_tool.nkm")]
        [InlineData(@"final\old_momo_64_arc\MissionRun\mr06_tool.nkm")]
        [InlineData(@"final\old_noko_sfc_arc\MissionRun\mr48_tool.nkm")]
        [InlineData(@"final\old_peach_agb_arc\MissionRun\mr55_tool.nkm")]
        [InlineData(@"final\old_sky_agb_arc\MissionRun\mr44_tool.nkm")]
        [InlineData(@"final\old_yoshi_gc_arc\MissionRun\mr40_tool.nkm")]
        [InlineData(@"final\pinball_course_arc\MissionRun\mr70_tool.nkm")]
        [InlineData(@"final\pinball_course_arc\MissionRun\mr75_tool.nkm")]
        [InlineData(@"final\rainbow_course_arc\MissionRun\mr60_tool.nkm")]
        [InlineData(@"final\ridge_course_arc\MissionRun\mr03_tool.nkm")]
        [InlineData(@"final\ridge_course_arc\MissionRun\mr62_tool.nkm")]
        [InlineData(@"final\snow_course_arc\MissionRun\mr35_tool.nkm")]
        [InlineData(@"final\snow_course_arc\MissionRun\mr88_tool.nkm")]
        [InlineData(@"final\stadium_course_arc\MissionRun\mr84_tool.nkm")]
        [InlineData(@"final\town_course_arc\MissionRun\mr26_tool.nkm")]
        [InlineData(@"final\town_course_arc\MissionRun\mr47_tool.nkm")]
        //USA Kiosk Demo
        [InlineData(@"kiosk\airship_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\bank_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\beach_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\clock_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\cross_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\desert_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\dokan_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\donkey_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\garden_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\koopa_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\luigi_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\mansion_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\mario_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\mini_block_64_arc\course_map.nkm")]
        [InlineData(@"kiosk\mini_block_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\mini_dokan_gc_arc\course_map.nkm")]
        [InlineData(@"kiosk\mini_stage1_arc\course_map.nkm")]
        [InlineData(@"kiosk\mini_stage2_arc\course_map.nkm")]
        [InlineData(@"kiosk\mini_stage3_arc\course_map.nkm")]
        [InlineData(@"kiosk\mini_stage4_arc\course_map.nkm")]
        [InlineData(@"kiosk\nokonoko_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\old_baby_gc_arc\course_map.nkm")]
        [InlineData(@"kiosk\old_choco_64_arc\course_map.nkm")]
        [InlineData(@"kiosk\old_choco_sfc_arc\course_map.nkm")]
        [InlineData(@"kiosk\old_donut_sfc_arc\course_map.nkm")]
        [InlineData(@"kiosk\old_frappe_64_arc\course_map.nkm")]
        [InlineData(@"kiosk\old_hyudoro_64_arc\course_map.nkm")]
        [InlineData(@"kiosk\old_kinoko_gc_arc\course_map.nkm")]
        [InlineData(@"kiosk\old_koopa_agb_arc\course_map.nkm")]
        [InlineData(@"kiosk\old_luigi_agb_arc\course_map.nkm")]
        [InlineData(@"kiosk\old_luigi_gc_arc\course_map.nkm")]
        [InlineData(@"kiosk\old_mario_gc_arc\course_map.nkm")]
        [InlineData(@"kiosk\old_mario_sfc_arc\course_map.nkm")]
        [InlineData(@"kiosk\old_momo_64_arc\course_map.nkm")]
        [InlineData(@"kiosk\old_noko_sfc_arc\course_map.nkm")]
        [InlineData(@"kiosk\old_peach_agb_arc\course_map.nkm")]
        [InlineData(@"kiosk\old_sky_agb_arc\course_map.nkm")]
        [InlineData(@"kiosk\old_yoshi_gc_arc\course_map.nkm")]
        [InlineData(@"kiosk\pinball_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\rainbow_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\ridge_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\snow_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\stadium_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\test1_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\town_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\wario_course_arc\course_map.nkm")]
        [InlineData(@"kiosk\mini_block_course_arc\MissionRun\mr10_tool.nkm")]
        [InlineData(@"kiosk\MR_stage1_arc\MissionRun\mr09_tool.nkm")]

        // All these NKM are version 32. However, they don't seem to be written
        // in the same way as other NKMs with the same version
        [InlineData(@"kiosk\test_circle_arc\course_map.nkm")]
        [InlineData(@"kiosk\airship_course_arc\MissionRun\mr08_tool.nkm")]
        [InlineData(@"kiosk\clock_course_arc\MissionRun\mr07_tool.nkm")]
        [InlineData(@"kiosk\garden_course_arc\MissionRun\mr04_tool.nkm")]
        [InlineData(@"kiosk\old_mario_sfc_arc\MissionRun\mr05_tool.nkm")]
        [InlineData(@"kiosk\old_momo_64_arc\MissionRun\mr02_tool.nkm")]
        [InlineData(@"kiosk\old_momo_64_arc\MissionRun\mr06_tool.nkm")]
        [InlineData(@"kiosk\old_yoshi_gc_arc\MissionRun\mr01_tool.nkm")]
        [InlineData(@"kiosk\ridge_course_arc\MissionRun\mr03_tool.nkm")]
        public void ReadWriteXmlTest(string path)
        {
            var mapData = MkdsMapDataFactory.CreateFromNkm(
                new Nkmd(File.ReadAllBytes(Path.Combine(BasePath, path))));

            // Try fixing errors
            var validatorFactory = new MkdsMapDataValidatorFactory(new MkdsMapObjDatabase());
            var errors = validatorFactory.CreateMkdsMapDataValidator().Validate(mapData);
            foreach (var error in errors.Where(x => x.Level == ErrorLevel.Fatal))
            {
                if (error.TryFix(out var action))
                    action.Do();
            }

            var inputNkmData = NkmdFactory.FromMapData(mapData).Write();
            var outputXml    = mapData.WriteXml();

            //File.WriteAllBytes("output.xml", outputXml);
            MkdsMapData rereadMapData;

            try
            {
                rereadMapData = MkdsMapData.FromXml(outputXml);
            }
            catch
            {
                File.WriteAllBytes("output.xml", outputXml);
                throw;
            }

            //File.WriteAllBytes(path + ".xml", outputXml);

            Assert.Equal(mapData.MapObjects.Count, rereadMapData.MapObjects.Count);

            for (int i = 0; i < mapData.MapObjects.Count; i++)
                Assert.Equal(mapData.MapObjects[i].Settings.GetType(),
                    rereadMapData.MapObjects[i].Settings.GetType());

            var outputNkmData = NkmdFactory.FromMapData(rereadMapData).Write();

            Assert.Equal(inputNkmData, outputNkmData);

            //if (!result)
            //{
            //    File.WriteAllBytes("output.xml", outputXml);
            //    File.WriteAllBytes("original.nkm", inputNkmData);
            //    File.WriteAllBytes("rewritten.nkm", outputNkmData);
            //}

            //return result;
        }
    }
}