using HaroohiePals.Graphics;
using HaroohiePals.IO;
using System.ComponentModel;
using System.Xml.Serialization;

namespace HaroohiePals.Nitro.JNLib.Spl.Emitter
{
    /// <summary>
    /// This set of params allows for one out of two effects
    /// - When IsRandom = true, particles use a random color from
    ///   the set { Color, InitialColor, EndingColor }
    /// - When IsRandom = false, particles will have an animated color,
    ///   optionally with interpolation.
    ///   - When interpolating, the animation works as follows:
    ///     0            - InitialColor
    ///     InEndTime    - InitialColor -> Color
    ///     PeakTime     - Color -> EndingColor
    ///     OutStartTime - EndingColor
    ///   - When not interpolating, the animation works as follows:
    ///     0            - InitialColor
    ///     InEndTime    - Color
    ///     PeakTime     - EndingColor
    ///     OutStartTime - EndingColor
    /// Note that Color is the color which is set in the emitter
    /// </summary>
    public class ColorAnimParams
    {
        public ColorAnimParams()
        {
        }

        public ColorAnimParams(EndianBinaryReader er)
        {
            InitialColor = er.Read<ushort>();
            EndingColor = er.Read<ushort>();
            InEndTime = er.Read<byte>() / 256f;
            PeakTime = er.Read<byte>() / 256f;
            OutStartTime = er.Read<byte>() / 256f;
            er.Read<byte>();
            ushort tmp = er.Read<ushort>();
            IsRandom = (tmp & 1) == 1;
            Loop = (tmp >> 1 & 1) == 1;
            Interpolate = (tmp >> 2 & 1) == 1;
            er.Read<ushort>();
        }

        public void Write(EndianBinaryWriterEx ew)
        {
            ew.Write(InitialColor);
            ew.Write(EndingColor);
            ew.Write(FloatHelper.ToByte(InEndTime, nameof(InEndTime)));
            ew.Write(FloatHelper.ToByte(PeakTime, nameof(PeakTime)));
            ew.Write(FloatHelper.ToByte(OutStartTime, nameof(OutStartTime)));
            ew.Write((byte)0);
            ew.Write((ushort)(
                (IsRandom ? 1u : 0) |
                (Loop ? 1u : 0) << 1 |
                (Interpolate ? 1u : 0) << 2));
            ew.Write((ushort)0);
        }

        public Rgb555 InitialColor { get; set; }
        public Rgb555 EndingColor { get; set; }

        /// <summary>
        /// When interpolating, InitialColor will interpolate to Color from this moment until PeakTime
        /// When not interpolating, the color will be Color from this moment until PeakTime
        /// </summary>
        public float InEndTime { get; set; }

        /// <summary>
        /// When interpolating, the moment InitialColor has been fully interpolated to Color
        /// When not interpolating, the moment EndingColor starts being used
        /// </summary>
        public float PeakTime { get; set; }

        /// <summary>
        /// The moment at which EndingColor is fully used
        /// </summary>
        public float OutStartTime { get; set; }

        /// <summary>
        /// When true particles pick a random color from the set { Color, InitialColor, EndingColor }
        /// instead of performing a color animation
        /// </summary>
        [XmlAttribute]
        [DefaultValue(false)]
        public bool IsRandom { get; set; }

        [XmlAttribute]
        [DefaultValue(false)]
        public bool Loop { get; set; }

        /// <summary>
        /// When true colors are linearly interpolated during the animation
        /// </summary>
        [XmlAttribute]
        [DefaultValue(false)]
        public bool Interpolate { get; set; }
    }
}