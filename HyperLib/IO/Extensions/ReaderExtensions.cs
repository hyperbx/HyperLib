namespace HyperLib.IO.Extensions
{
    public static class ReaderExtensions
    {
        public static bool IsSignatureValid<T>(this BinaryValueReader in_reader, T in_expected, bool in_isExceptionOnInvalid = true) where T : unmanaged
        {
            var sig = in_reader.Read<T>();

            if (sig.Equals(in_expected))
                return true;

            if (in_isExceptionOnInvalid)
                throw new BadImageFormatException($"Signature mismatch (expected: 0x{in_expected:X}, received: 0x{sig:X})");

            return false;
        }
    }
}
