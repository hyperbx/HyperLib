using Amicitia.IO.Binary;

namespace HyperLib.IO.Extensions
{
    public static class WriterExtensions
    {
        public static void WriteNullBytes(this BinaryValueWriter in_writer, int in_count)
        {
            in_writer.WriteBytes(new byte[in_count]);
        }
    }
}
