namespace HyperLib.IO
{
    public class CRC32
    {
        private const uint _polynomial = 0xEDB88320;

        private static readonly uint[] _table = new uint[256];

        static CRC32()
        {
            for (uint i = 0; i < 256; i++)
            {
                var crc = i;

                for (uint j = 8; j > 0; j--)
                {
                    if ((crc & 1) == 1)
                    {
                        crc = (crc >> 1) ^ _polynomial;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }

                _table[i] = crc;
            }
        }

        public static uint Compute(Stream in_stream)
        {
            var crc = 0xFFFFFFFF;
            int b;

            while ((b = in_stream.ReadByte()) != -1)
                crc = (crc >> 8) ^ _table[(crc & 0xFF) ^ (uint)b];

            return ~crc;
        }

        public static uint Compute(byte[] in_buffer)
        {
            using (var ms = new MemoryStream(in_buffer))
                return Compute(ms);
        }

        public static uint Compute(string in_path)
        {
            using (var stream = File.OpenRead(in_path))
                return Compute(stream);
        }
    }
}
