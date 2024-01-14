using HyperLib.Helpers;

namespace HyperLib.IO.Extensions
{
    public static class ReaderExtensions
    {
        public static byte[] ReadBytes(this BinaryValueReader in_reader, int in_count)
        {
            return in_reader.ReadArray<byte>(in_count);
        }

        public static bool IsSignatureValid<T>(this BinaryValueReader in_reader, T in_expected, T in_received, bool in_isExceptionOnInvalid = true) where T : unmanaged
        {
            if (in_expected.Equals(in_received))
                return true;

            if (in_isExceptionOnInvalid)
                throw new BadImageFormatException($"Signature mismatch (expected: 0x{in_expected:X}, received: 0x{in_received:X})");

            return false;
        }

        public static bool IsSignatureValid<T>(this BinaryValueReader in_reader, T in_expected, bool in_isExceptionOnInvalid = true) where T : unmanaged
        {
            return IsSignatureValid(in_reader, in_expected, in_reader.Read<T>(), in_isExceptionOnInvalid);
        }

        public static Endianness GetEndiannessFromSignature<T>(this BinaryValueReader in_reader, T in_expected, T in_received) where T : unmanaged
        {
            var expectedReversed = BinaryHelper.SwapEndianness(in_expected);

            if (in_received.Equals(expectedReversed))
            {
                return in_reader.Endianness == Endianness.Little
                    ? Endianness.Big
                    : Endianness.Little;
            }

            return in_reader.Endianness == Endianness.Big
                ? Endianness.Little
                : Endianness.Big;
        }

        public static Endianness GetEndiannessFromSignature<T>(this BinaryValueReader in_reader, T in_expected) where T : unmanaged
        {
            return GetEndiannessFromSignature(in_reader, in_reader.Read<T>(), in_expected);
        }

        public static int ReadInt24(this BinaryValueReader in_reader)
        {
            var buf = in_reader.ReadBytes(3);

            return in_reader.Endianness == Endianness.Big ?
                   buf[0] << 16 | buf[1] << 8 | buf[2] :
                   buf[2] << 16 | buf[1] << 8 | buf[0];
        }

        public static uint ReadUInt24(this BinaryValueReader in_reader)
        {
            var buf = in_reader.ReadBytes(3);

            return in_reader.Endianness == Endianness.Big ?
                   ((uint)buf[0] << 16 | (uint)buf[1] << 8 | buf[2]) :
                   ((uint)buf[2] << 16 | (uint)buf[1] << 8 | buf[0]);
        }
    }
}
