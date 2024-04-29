using HaroohiePals.Graphics;
using HaroohiePals.NitroKart.MapData.Binary;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections;

public sealed class MkdsStageInfo
{
    public ushort CourseId { get; set; }

    [Category("Track"), DisplayName("Laps/Battle Time")]
    public short NrLaps { get; set; }

    public byte PolePosition { get; set; }

    [Category("Fog"), DisplayName("Enabled")]
    public bool FogEnabled { get; set; }

    [Category("Fog"), DisplayName("Table Generation Mode"), Range(0, 2)]
    public byte FogTableGenMode { get; set; }

    [Category("Fog"), DisplayName("Shift"), Range(0, 10)]
    public byte FogShift { get; set; }

    [Category("Fog"), DisplayName("Offset"), Range(0, 0xFFFFFF)]
    public int FogOffset { get; set; }

    [Category("Fog"), DisplayName("Color")]
    public Rgb555 FogColor { get; set; }

    [Category("Fog"), DisplayName("Alpha")]
    public uint FogAlpha { get; set; }

    [Category("Track"), DisplayName("Light Colors"), Description("Colors to be assigned to a collision prism.")]
    public Rgb555[] KclLightColors { get; set; } = new[]
        { new Rgb555(Color.White), new Rgb555(Color.Gray), new Rgb555(Color.Gray), new Rgb555(Color.Gray) };

    [Category("Track"), DefaultValue(0.0)]
    public double MobjFarClip { get; set; }

    [Category("Track"), DefaultValue(0.0)]
    public double FrustumFar { get; set; }

    public MkdsStageInfo() { }

    public MkdsStageInfo(NkmdStag stag)
    {
        CourseId        = stag.CourseId;
        NrLaps          = stag.NrLaps;
        PolePosition    = stag.PolePosition;
        FogEnabled      = stag.FogEnabled;
        FogTableGenMode = stag.FogTableGenMode;
        FogShift        = stag.FogShift;
        FogOffset       = stag.FogOffset;
        FogColor        = stag.FogColor;
        FogAlpha        = stag.FogAlpha;
        KclLightColors = new[]
        {
            stag.KclLightColors[0],
            stag.KclLightColors[1],
            stag.KclLightColors[2],
            stag.KclLightColors[3]
        };
        MobjFarClip = stag.MobjFarClip;
        FrustumFar  = stag.FrustumFar;
    }

    public NkmdStag ToStag() => new()
    {
        CourseId        = CourseId,
        NrLaps          = NrLaps,
        PolePosition    = PolePosition,
        FogEnabled      = FogEnabled,
        FogTableGenMode = FogTableGenMode,
        FogShift        = FogShift,
        FogOffset       = FogOffset,
        FogColor        = FogColor,
        FogAlpha        = FogAlpha,
        KclLightColors  = new[] { KclLightColors[0], KclLightColors[1], KclLightColors[2], KclLightColors[3] },
        MobjFarClip     = MobjFarClip,
        FrustumFar      = FrustumFar
    };
}