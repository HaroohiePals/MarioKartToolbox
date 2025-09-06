using HaroohiePals.NitroKart.MapObj.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings;

public class FallsWaterDestinationSettings : MkdsMobjSettings
{
    public FallsWaterDestinationSettings() { }

    public FallsWaterDestinationSettings(MkdsMobjSettings settings)
        : base(settings) { }

    [DisplayName("Index")]
    [Description("Must match the target FallsWater KCL variant ID")]
    public short PlayerItemSlotListId
    {
        get => (short)Settings[0];
        set => Settings[0] = (short)value;
    }

    [DisplayName("Max Pull Strength (50cc)")]
    public short MaxPullStrenght50cc
    {
        get => (short)Settings[1];
        set => Settings[1] = (short)value;
    }

    [DisplayName("Max Pull Strength (100cc)")]
    public short MaxPullStrenght100cc
    {
        get => (short)Settings[2];
        set => Settings[2] = (short)value;
    }

    [DisplayName("Max Pull Strength (150cc)")]
    public short MaxPullStrenght150cc
    {
        get => (short)Settings[3];
        set => Settings[3] = (short)value;
    }
}