using System;
using System.Text;

namespace HaroohiePals.IO
{
    [Serializable]
    public class SignatureNotCorrectException : Exception
    {
        public string Signature { get; }
        public string Expected  { get; }
        public long   Offset    { get; }

        public SignatureNotCorrectException(uint signature, uint expected, long offset)
            : this(
                Encoding.ASCII.GetString(
                    new byte[]
                    {
                        (byte) (signature & 0xFF), (byte) ((signature >> 8) & 0xFF),
                        (byte) ((signature >> 16) & 0xFF), (byte) ((signature >> 24) & 0xFF)
                    }),
                Encoding.ASCII.GetString(
                    new byte[]
                    {
                        (byte) (expected & 0xFF), (byte) ((expected >> 8) & 0xFF),
                        (byte) ((expected >> 16) & 0xFF), (byte) ((expected >> 24) & 0xFF)
                    }),
                offset
            ) { }


        public SignatureNotCorrectException(string signature, string expected, long offset)
            : base("Signature '" + signature + "' at 0x" + offset.ToString("X8") + " does not match '" +
                   expected + "'.")
        {
            Signature = signature;
            Expected  = expected;
            Offset    = offset;
        }
    }
}