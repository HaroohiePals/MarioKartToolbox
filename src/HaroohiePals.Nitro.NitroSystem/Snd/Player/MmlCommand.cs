namespace HaroohiePals.Nitro.NitroSystem.Snd.Player
{
    public enum MmlCommand : byte
    {
        Wait = 0x80,
        Prg  = 0x81,

        OpenTrack = 0x93,
        Jump      = 0x94,
        Call      = 0x95,

        Random   = 0xa0,
        Variable = 0xa1,
        If       = 0xa2,

        Setvar   = 0xb0,
        Addvar   = 0xb1,
        Subvar   = 0xb2,
        Mulvar   = 0xb3,
        Divvar   = 0xb4,
        Shiftvar = 0xb5,
        Randvar  = 0xb6,

        CmpEq = 0xb8,
        CmpGe = 0xb9,
        CmpGt = 0xba,
        CmpLe = 0xbb,
        CmpLt = 0xbc,
        CmpNe = 0xbd,

        Pan        = 0xc0,
        Volume     = 0xc1,
        MainVolume = 0xc2,
        Transpose  = 0xc3,
        PitchBend  = 0xc4,
        BendRange  = 0xc5,
        Prio       = 0xc6,
        NoteWait   = 0xc7,
        Tie        = 0xc8,
        Porta      = 0xc9,
        ModDepth   = 0xca,
        ModSpeed   = 0xcb,
        ModType    = 0xcc,
        ModRange   = 0xcd,
        PortaSw    = 0xce,
        PortaTime  = 0xcf,
        Attack     = 0xd0,
        Decay      = 0xd1,
        Sustain    = 0xd2,
        Release    = 0xd3,
        LoopStart  = 0xd4,
        Volume2    = 0xd5,
        Printvar   = 0xd6,
        Mute       = 0xd7,

        ModDelay   = 0xe0,
        Tempo      = 0xe1,
        SweepPitch = 0xe3,

        LoopEnd    = 0xfc,
        Ret        = 0xfd,
        AllocTrack = 0xfe,
        Fin        = 0xff
    }
}