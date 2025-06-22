using HaroohiePals.Graphics;
using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using HaroohiePals.Nitro.Gx;
using System;
using System.Text;
using System.Xml.Serialization;

namespace HaroohiePals.Nitro.Card;

[Serializable]
public sealed class NdsBannerDataV1
{
    public NdsBannerDataV1() { }

    public NdsBannerDataV1(EndianBinaryReader er)
    {
        Image = er.Read<byte>(32 * 32 / 2);
        Pltt = er.Read<byte>(16 * 2);
        GameName = new string[6];
        GameName[0] = er.ReadString(Encoding.Unicode, 128).Replace("\0", "");
        GameName[1] = er.ReadString(Encoding.Unicode, 128).Replace("\0", "");
        GameName[2] = er.ReadString(Encoding.Unicode, 128).Replace("\0", "");
        GameName[3] = er.ReadString(Encoding.Unicode, 128).Replace("\0", "");
        GameName[4] = er.ReadString(Encoding.Unicode, 128).Replace("\0", "");
        GameName[5] = er.ReadString(Encoding.Unicode, 128).Replace("\0", "");
    }

    public void Write(EndianBinaryWriter er)
    {
        er.Write(Image, 0, 32 * 32 / 2);
        er.Write(Pltt, 0, 16 * 2);
        foreach (string s in GameName) er.Write(GameName[0].PadRight(128, '\0'), Encoding.Unicode, false);
    }

    [ArraySize(32 * 32 / 2)]
    public byte[] Image;

    [ArraySize(16 * 2)]
    public byte[] Pltt;

    [XmlIgnore]
    public string[] GameName; //6, 128 chars (UTF16-LE)

    [XmlElement("GameName")]
    public string[] Base64GameName
    {
        get
        {
            string[] b = new string[6];
            for (int i = 0; i < 6; i++)
            {
                b[i] = Convert.ToBase64String(Encoding.Unicode.GetBytes(GameName[i]));
            }

            return b;
        }
        set
        {
            GameName = new string[6];
            for (int i = 0; i < 6; i++)
            {
                GameName[i] = Encoding.Unicode.GetString(Convert.FromBase64String(value[i]));
            }
        }
    }

    public ushort GetCrc()
    {
        byte[] data = new byte[2080];
        Array.Copy(Image, data, 512);
        Array.Copy(Pltt, 0, data, 512, 32);
        Array.Copy(Encoding.Unicode.GetBytes(GameName[0].PadRight(128, '\0')), 0, data, 544, 256);
        Array.Copy(Encoding.Unicode.GetBytes(GameName[1].PadRight(128, '\0')), 0, data, 544 + 256, 256);
        Array.Copy(Encoding.Unicode.GetBytes(GameName[2].PadRight(128, '\0')), 0, data, 544 + 256 * 2, 256);
        Array.Copy(Encoding.Unicode.GetBytes(GameName[3].PadRight(128, '\0')), 0, data, 544 + 256 * 3, 256);
        Array.Copy(Encoding.Unicode.GetBytes(GameName[4].PadRight(128, '\0')), 0, data, 544 + 256 * 4, 256);
        Array.Copy(Encoding.Unicode.GetBytes(GameName[5].PadRight(128, '\0')), 0, data, 544 + 256 * 5, 256);
        return Crc16.GetCrc16(data);
    }

    public Rgba8Bitmap GetIcon() => GxUtil.DecodeChar(Image, Pltt, ImageFormat.Pltt16, 32, 32, true);
}