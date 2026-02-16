using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaroohiePals.NitroKart.Rom;

public class MkdsRomProject
{
    public string Name { get; set; }
    public NdsRomInfo RomInfo { get; set; } = new();
    public int Version { get; set; } = 0;
}
