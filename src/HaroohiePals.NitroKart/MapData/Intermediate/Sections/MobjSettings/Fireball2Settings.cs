using System;
using System.ComponentModel.DataAnnotations;


namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings;

public class Fireball2Settings : MkdsMobjSettings
{
    public Fireball2Settings() { }

    public Fireball2Settings(MkdsMobjSettings settings)
        : base(settings) { }

    [Range(1, 20)]
    public short NrArms
    {
        get => Settings[0];
        set => Settings[0] = value;
    }

    public short Radius
    {
        get => Settings[1];
        set => Settings[1] = value;
    }

    public short RotationSpeed
    {
        get => Settings[2];
        set => Settings[2] = value;
    }

    [Range(1, 20)]
    public int FireballsPerArm
    {
        get => Settings[3] + 1;
        set
        {
            Settings[3] = (short)(value - 1);
        }
    }
}