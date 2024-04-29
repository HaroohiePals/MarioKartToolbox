using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using System.Reflection;

namespace HaroohiePals.NitroKart.MapObj.Water
{
    [MapObj(MkdsMapObjectId.MiniStage3Water, null, null, nameof(Water.LoadMiniStage3Water))]
    [MapObj(MkdsMapObjectId.BeachWater, null, null, nameof(Water.LoadBeachWater))]
    [MapObj(MkdsMapObjectId.TownWater, null, null, nameof(Water.LoadTownWater))]
    [MapObj(MkdsMapObjectId.YoshiWater, null, null, nameof(Water.LoadYoshiWater))]
    [MapObj(MkdsMapObjectId.HyudoroWater, null, null, nameof(Water.LoadHyudoroWater))]
    public class Water : MObjInstance
    {
        public Water(MkdsContext context, RenderPart[] renderParts, LogicPart logicPart)
            : base(context, renderParts, logicPart) { }

        public override void Init(MkdsMapObject obji, object arg)
        {
            _context.MObjState.WaterObject =  this;
            Flags                          &= ~InstanceFlags.DisableVisibilityUpdates;
            Flags                          &= ~InstanceFlags.Bit3;
            Alpha                          =  31;
            typeof(Water).GetMethod((string)arg, BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] { _context });
        }

        private static void LoadMiniStage3Water(MkdsContext context)
        {
            context.MObjState.WaterState = new WaterState(context, 232, 197, 80,
                50, 60, 0x7FFF, 0x3FFF, 500, 300, 300,
                true, 5, 10, 10, true);
            context.MObjState.WaterState.WaterANsbmd =
                MObjUtil.GetMapObjFile<Nsbmd>(context, "mini_stage3_waterA.nsbmd");
            context.MObjState.WaterState.WaterCNsbmd =
                MObjUtil.GetMapObjFile<Nsbmd>(context, "mini_stage3_waterC.nsbmd");
        }

        private static void LoadBeachWater(MkdsContext context)
        {
            context.MObjState.WaterState = new WaterState(context, 120, 98, 80,
                67, 60, 0x7FFF, 0x3FFF, 500, 150, 150,
                true, 6, 10, 10, true);
            context.MObjState.WaterState.WaterANsbmd =
                MObjUtil.GetMapObjFile<Nsbmd>(context, "beach_waterA.nsbmd");
            context.MObjState.WaterState.WaterCNsbmd =
                MObjUtil.GetMapObjFile<Nsbmd>(context, "beach_waterC.nsbmd");
        }

        private static void LoadTownWater(MkdsContext context)
        {
            context.MObjState.WaterState = new WaterState(context, 0, 0, 41,
                30, 11, 0x4E20, 0x3FFF, 300, 120, 30,
                true, 13, 25, 16, false);
            context.MObjState.WaterState.WaterANsbmd =
                MObjUtil.GetMapObjFile<Nsbmd>(context, "town_waterA.nsbmd");
            context.MObjState.WaterState.WaterCNsbmd =
                MObjUtil.GetMapObjFile<Nsbmd>(context, "town_waterC.nsbmd");
        }

        private static void LoadYoshiWater(MkdsContext context)
        {
            context.MObjState.WaterState = new WaterState(context, 0, 0, 200,
                85, 60, 0x7FFF, 0x3FFF, 450, 120, 30,
                true, 10, 23, 16, false);
            context.MObjState.WaterState.WaterANsbmd = null;
            context.MObjState.WaterState.WaterCNsbmd =
                MObjUtil.GetMapObjFile<Nsbmd>(context, "yoshi_waterC.nsbmd");
        }

        private static void LoadHyudoroWater(MkdsContext context)
        {
            context.MObjState.WaterState = new WaterState(context, 0, 0, 99,
                77, 30, 0x7FFF, 0x3FFF, 450, 120, 30,
                true, 15, 25, 16, false);
            context.MObjState.WaterState.WaterANsbmd =
                MObjUtil.GetMapObjFile<Nsbmd>(context, "hyudoro_waterA.nsbmd");
            context.MObjState.WaterState.WaterCNsbmd =
                MObjUtil.GetMapObjFile<Nsbmd>(context, "hyudoro_waterC.nsbmd");
        }
    }
}