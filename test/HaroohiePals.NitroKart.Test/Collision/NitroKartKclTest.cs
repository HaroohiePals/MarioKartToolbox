using HaroohiePals.IO;
using HaroohiePals.KCollision;
using HaroohiePals.KCollision.Formats;
using System;
using System.IO;
using Xunit;

namespace HaroohiePals.NitroKart.Test.Collision
{
    public class NitroKartKclTest
    {
        private static readonly string BasePath = Environment.GetEnvironmentVariable("NITROKART_TEST_COURSES_PATH");

        [Theory]
        //Final
        [InlineData(@"final\airship_course_arc\course_collision.kcl")]
        [InlineData(@"final\Award_arc\course_collision.kcl")]
        [InlineData(@"final\bank_course_arc\course_collision.kcl")]
        [InlineData(@"final\beach_courseD_arc\course_collision.kcl")]
        [InlineData(@"final\beach_course_arc\course_collision.kcl")]
        [InlineData(@"final\clock_course_arc\course_collision.kcl")]
        [InlineData(@"final\cross_courseD_arc\course_collision.kcl")]
        [InlineData(@"final\cross_course_arc\course_collision.kcl")]
        [InlineData(@"final\desert_course_arc\course_collision.kcl")]
        [InlineData(@"final\dokan_course_arc\course_collision.kcl")]
        [InlineData(@"final\donkey_course_arc\course_collision.kcl")]
        [InlineData(@"final\garden_course_arc\course_collision.kcl")]
        [InlineData(@"final\koopa_course_arc\course_collision.kcl")]
        [InlineData(@"final\luigi_course_arc\course_collision.kcl")]
        [InlineData(@"final\mansion_courseD_arc\course_collision.kcl")]
        [InlineData(@"final\mansion_course_arc\course_collision.kcl")]
        [InlineData(@"final\mario_course_arc\course_collision.kcl")]
        [InlineData(@"final\mini_block_64_arc\course_collision.kcl")]
        [InlineData(@"final\mini_block_course_arc\course_collision.kcl")]
        [InlineData(@"final\mini_dokan_gc_arc\course_collision.kcl")]
        [InlineData(@"final\mini_stage1_arc\course_collision.kcl")]
        [InlineData(@"final\mini_stage2_arc\course_collision.kcl")]
        [InlineData(@"final\mini_stage3_arc\course_collision.kcl")]
        [InlineData(@"final\mini_stage4_arc\course_collision.kcl")]
        [InlineData(@"final\MR_stage1_arc\course_collision.kcl")]
        [InlineData(@"final\MR_stage2_arc\course_collision.kcl")]
        [InlineData(@"final\MR_stage3_arc\course_collision.kcl")]
        [InlineData(@"final\MR_stage4_arc\course_collision.kcl")]
        [InlineData(@"final\nokonoko_course_arc\course_collision.kcl")]
        [InlineData(@"final\old_baby_gc_arc\course_collision.kcl")]
        [InlineData(@"final\old_choco_64_arc\course_collision.kcl")]
        [InlineData(@"final\old_choco_sfc_arc\course_collision.kcl")]
        [InlineData(@"final\old_donut_sfc_arc\course_collision.kcl")]
        [InlineData(@"final\old_frappe_64_arc\course_collision.kcl")]
        [InlineData(@"final\old_hyudoro_64_arc\course_collision.kcl")]
        [InlineData(@"final\old_kinoko_gc_arc\course_collision.kcl")]
        [InlineData(@"final\old_koopa_agb_arc\course_collision.kcl")]
        [InlineData(@"final\old_luigi_agb_arc\course_collision.kcl")]
        [InlineData(@"final\old_luigi_gcD_arc\course_collision.kcl")]
        [InlineData(@"final\old_luigi_gc_arc\course_collision.kcl")]
        [InlineData(@"final\old_mario_gc_arc\course_collision.kcl")]
        [InlineData(@"final\old_mario_sfc_arc\course_collision.kcl")]
        [InlineData(@"final\old_momo_64D_arc\course_collision.kcl")]
        [InlineData(@"final\old_momo_64_arc\course_collision.kcl")]
        [InlineData(@"final\old_noko_sfc_arc\course_collision.kcl")]
        [InlineData(@"final\old_peach_agb_arc\course_collision.kcl")]
        [InlineData(@"final\old_sky_agb_arc\course_collision.kcl")]
        [InlineData(@"final\old_yoshi_gc_arc\course_collision.kcl")]
        [InlineData(@"final\pinball_course_arc\course_collision.kcl")]
        [InlineData(@"final\rainbow_course_arc\course_collision.kcl")]
        [InlineData(@"final\ridge_course_arc\course_collision.kcl")]
        [InlineData(@"final\snow_course_arc\course_collision.kcl")]
        [InlineData(@"final\stadium_course_arc\course_collision.kcl")]
        [InlineData(@"final\StaffRollTrue_arc\course_collision.kcl")]
        [InlineData(@"final\StaffRoll_arc\course_collision.kcl")]
        [InlineData(@"final\test1_course_arc\course_collision.kcl")]
        [InlineData(@"final\town_course_arc\course_collision.kcl")]
        [InlineData(@"final\wario_course_arc\course_collision.kcl")]
        public void ReadWriteTest(string path)
        {
            var originalKclData = File.ReadAllBytes(Path.Combine(BasePath, path));

            var kcl = new MkdsKcl(originalKclData);

            var outputKclData = kcl.Write();

            Assert.Equal(originalKclData, outputKclData);
        }

        [Theory]
        //Final
        [InlineData(@"final\airship_course_arc\course_collision.kcl")]
        [InlineData(@"final\Award_arc\course_collision.kcl")]
        [InlineData(@"final\bank_course_arc\course_collision.kcl")]
        [InlineData(@"final\beach_courseD_arc\course_collision.kcl")]
        [InlineData(@"final\beach_course_arc\course_collision.kcl")]
        [InlineData(@"final\clock_course_arc\course_collision.kcl")]
        [InlineData(@"final\cross_courseD_arc\course_collision.kcl")]
        [InlineData(@"final\cross_course_arc\course_collision.kcl")]
        [InlineData(@"final\desert_course_arc\course_collision.kcl")]
        [InlineData(@"final\dokan_course_arc\course_collision.kcl")]
        [InlineData(@"final\donkey_course_arc\course_collision.kcl")]
        [InlineData(@"final\garden_course_arc\course_collision.kcl")]
        [InlineData(@"final\koopa_course_arc\course_collision.kcl")]
        [InlineData(@"final\luigi_course_arc\course_collision.kcl")]
        [InlineData(@"final\mansion_courseD_arc\course_collision.kcl")]
        [InlineData(@"final\mansion_course_arc\course_collision.kcl")]
        [InlineData(@"final\mario_course_arc\course_collision.kcl")]
        [InlineData(@"final\mini_block_64_arc\course_collision.kcl")]
        [InlineData(@"final\mini_block_course_arc\course_collision.kcl")]
        [InlineData(@"final\mini_dokan_gc_arc\course_collision.kcl")]
        [InlineData(@"final\mini_stage1_arc\course_collision.kcl")]
        [InlineData(@"final\mini_stage2_arc\course_collision.kcl")]
        [InlineData(@"final\mini_stage3_arc\course_collision.kcl")]
        [InlineData(@"final\mini_stage4_arc\course_collision.kcl")]
        [InlineData(@"final\MR_stage1_arc\course_collision.kcl")]
        [InlineData(@"final\MR_stage2_arc\course_collision.kcl")]
        [InlineData(@"final\MR_stage3_arc\course_collision.kcl")]
        [InlineData(@"final\MR_stage4_arc\course_collision.kcl")]
        [InlineData(@"final\nokonoko_course_arc\course_collision.kcl")]
        [InlineData(@"final\old_baby_gc_arc\course_collision.kcl")]
        [InlineData(@"final\old_choco_64_arc\course_collision.kcl")]
        [InlineData(@"final\old_choco_sfc_arc\course_collision.kcl")]
        [InlineData(@"final\old_donut_sfc_arc\course_collision.kcl")]
        [InlineData(@"final\old_frappe_64_arc\course_collision.kcl")]
        [InlineData(@"final\old_hyudoro_64_arc\course_collision.kcl")]
        [InlineData(@"final\old_kinoko_gc_arc\course_collision.kcl")]
        [InlineData(@"final\old_koopa_agb_arc\course_collision.kcl")]
        [InlineData(@"final\old_luigi_agb_arc\course_collision.kcl")]
        [InlineData(@"final\old_luigi_gcD_arc\course_collision.kcl")]
        [InlineData(@"final\old_luigi_gc_arc\course_collision.kcl")]
        [InlineData(@"final\old_mario_gc_arc\course_collision.kcl")]
        [InlineData(@"final\old_mario_sfc_arc\course_collision.kcl")]
        [InlineData(@"final\old_momo_64D_arc\course_collision.kcl")]
        [InlineData(@"final\old_momo_64_arc\course_collision.kcl")]
        [InlineData(@"final\old_noko_sfc_arc\course_collision.kcl")]
        [InlineData(@"final\old_peach_agb_arc\course_collision.kcl")]
        [InlineData(@"final\old_sky_agb_arc\course_collision.kcl")]
        [InlineData(@"final\old_yoshi_gc_arc\course_collision.kcl")]
        [InlineData(@"final\pinball_course_arc\course_collision.kcl")]
        [InlineData(@"final\rainbow_course_arc\course_collision.kcl")]
        [InlineData(@"final\ridge_course_arc\course_collision.kcl")]
        [InlineData(@"final\snow_course_arc\course_collision.kcl")]
        [InlineData(@"final\stadium_course_arc\course_collision.kcl")]
        [InlineData(@"final\StaffRollTrue_arc\course_collision.kcl")]
        [InlineData(@"final\StaffRoll_arc\course_collision.kcl")]
        [InlineData(@"final\test1_course_arc\course_collision.kcl")]
        [InlineData(@"final\town_course_arc\course_collision.kcl")]
        [InlineData(@"final\wario_course_arc\course_collision.kcl")]
        public void OctreeRewriteTest(string path)
        {
            var originalKclData = File.ReadAllBytes(Path.Combine(BasePath, path));

            var kcl = new MkdsKcl(originalKclData);

            kcl.Octree = kcl.GetOctree().Write(Endianness.LittleEndian, KclOctree.CompressionMethod.Equal);

            var outputKclData = kcl.Write();

            Assert.Equal(originalKclData, outputKclData);
        }
    }
}