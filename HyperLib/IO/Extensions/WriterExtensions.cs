namespace HyperLib.IO.Extensions
{
    public static class WriterExtensions
    {
        public static void WriteNullBytes(this BinaryObjectWriter in_writer, int in_count)
        {
            in_writer.WriteBytes(new byte[in_count]);
        }

        public static void WriteInt24(this BinaryObjectWriter in_writer, int in_value)
        {
            WriteUInt24(in_writer, (uint)in_value);
        }

        public static void WriteUInt24(this BinaryObjectWriter in_writer, uint in_value)
        {
            var buf = new byte[3];

            if (in_writer.Endianness == Endianness.Big)
            {
                buf[0] = (byte)(in_value >> 16);
                buf[1] = (byte)(in_value >> 8);
                buf[2] = (byte)(in_value);
            }
            else
            {
                buf[0] = (byte)(in_value);
                buf[1] = (byte)(in_value >> 8);
                buf[2] = (byte)(in_value >> 16);
            }

            in_writer.WriteBytes(buf);
        }
    }
}
