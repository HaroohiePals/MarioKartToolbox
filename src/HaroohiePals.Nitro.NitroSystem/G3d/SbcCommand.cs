namespace HaroohiePals.Nitro.NitroSystem.G3d;

public enum SbcCommand : byte
{
    Nop = 0x00,
    Return = 0x01,
    Node = 0x02,
    Matrix = 0x03,
    Material = 0x04,
    Shape = 0x05,
    NodeDescription = 0x06,
    Billboard = 0x07,
    BillboardY = 0x08,
    NodeMix = 0x09,
    CallDisplayList = 0x0A,
    PosScale = 0x0B,
    EnvironmentMap = 0x0C,
    ProjectionMap = 0x0D
}
