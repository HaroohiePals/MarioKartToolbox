using HaroohiePals.NitroKart.MapObj.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings;

public class ItemboxSettings : MkdsMobjSettings
{
    public ItemboxSettings() { }

    public ItemboxSettings(MkdsMobjSettings settings)
        : base(settings) { }

    [DisplayName("Player Item Slot List ID")]
    [Description("Refers to ItemSlotList.dat")]
    public short PlayerItemSlotListId
    {
        get => (short)(Settings[1] - 1);
        set => Settings[1] = (short)(value + 1);
    }

    [DisplayName("Enemy Item Slot List ID")]
    [Description("Refers to ItemSlotList.dat")]
    public short EnemyItemSlotListId
    {
        get => (short)(Settings[2] - 1);
        set => Settings[2] = (short)(value + 1);
    }


    [DisplayName("Shadow Type")]
    [Description("Render a 2D, 3D or no shadow.")]
    public ItemboxShadowType ShadowType
    {
        get => (ItemboxShadowType)Settings[3];
        set => Settings[3] = (short)value;
    }


    [DisplayName("Disable Respawn")]
    public bool DisableRespawn
    {
        get => Settings[5] != 0;
        set => Settings[5] = (short)(value ? 1 : 0);
    }

    [Description("The use of this field is still unknown")]
    public bool DriverItemStatusFlag
    {
        get => Settings[6] != 0;
        set => Settings[6] = (short)(value ? 1 : 0);
    }
}
