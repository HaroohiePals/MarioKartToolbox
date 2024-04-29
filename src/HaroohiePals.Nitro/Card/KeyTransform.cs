using HaroohiePals.IO;
using System;

namespace HaroohiePals.Nitro.Card;

public static class KeyTransform
{
    public static byte[] TransformTable(uint idCode, int level, int modulo, ReadOnlySpan<byte> keyTable)
    {
        var newTable = keyTable[..Blowfish.KEY_TABLE_LENGTH].ToArray();

        var keyCode = new byte[12];
        IOUtil.WriteU32Le(keyCode, 0, idCode);
        IOUtil.WriteU32Le(keyCode, 4, idCode >> 1);
        IOUtil.WriteU32Le(keyCode, 8, idCode << 1);
        if (level >= 1)
            ApplyKeyCode(keyCode, modulo, newTable);
        if (level >= 2)
            ApplyKeyCode(keyCode, modulo, newTable);
        IOUtil.WriteU32Le(keyCode, 4, IOUtil.ReadU32Le(keyCode, 4) << 1);
        IOUtil.WriteU32Le(keyCode, 8, IOUtil.ReadU32Le(keyCode, 8) >> 1);
        if (level >= 3)
            ApplyKeyCode(keyCode, modulo, newTable);
        return newTable;
    }

    private static void ApplyKeyCode(byte[] keyCode, int modulo, byte[] keyTable)
    {
        var bf = new Blowfish(keyTable);
        bf.Encrypt(keyCode, 4, 8);
        bf.Encrypt(keyCode, 0, 8);
        for (int i = 0; i < Blowfish.P_TABLE_ENTRY_COUNT; i++)
        {
            IOUtil.WriteU32Le(keyTable, i * 4,
                IOUtil.ReadU32Le(keyTable, i * 4) ^ IOUtil.ReadU32Be(keyCode, (i * 4) % modulo));
        }

        var scratch = new byte[8];
        for (int i = 0; i < Blowfish.KEY_TABLE_LENGTH; i += 8)
        {
            //update table
            bf = new Blowfish(keyTable);
            bf.Encrypt(scratch);
            Array.Copy(scratch, 4, keyTable, i, 4);
            Array.Copy(scratch, 0, keyTable, i + 4, 4);
        }
    }
}