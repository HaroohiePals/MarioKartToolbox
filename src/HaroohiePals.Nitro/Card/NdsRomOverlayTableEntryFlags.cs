using System;

namespace HaroohiePals.Nitro.Card;

[Flags]
public enum NdsRomOverlayTableEntryFlags : byte
{
    Compressed = 1,
    AuthenticationCode = 2
}