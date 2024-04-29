using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings;

public class MoveItemboxSettings : ItemboxSettings
{
    public MoveItemboxSettings() { }

    public MoveItemboxSettings(MkdsMobjSettings settings)
        : base(settings) { }

    [DisplayName("Base Path Speed")]
    public short BasePathSpeed
    {
        get => Settings[0];
        set => Settings[0] = value;
    }

    [DisplayName("Initial Path Point")]
    public short InitialPathPoint
    {
        get => Settings[4];
        set => Settings[4] = value;
    }
}
