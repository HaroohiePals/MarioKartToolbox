using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaroohiePals.NitroKart.Rom;

public class NdsRomInfo
{
    public string FsRootPath { get; set; }
    public string HeaderPath { get; set; }
    public string BannerPath { get; set; }
    public string RsaSignaturePath { get; set; }
    public string Arm9Path { get; set; }
    public string Arm9OvtPath { get; set; }
    public string[] Arm9OverlaysPaths { get; set; }
    public string Arm7Path { get; set; }
    public string Arm7OvtPath { get; set; }
    public string[] Arm7OverlaysPaths { get; set; }
}
