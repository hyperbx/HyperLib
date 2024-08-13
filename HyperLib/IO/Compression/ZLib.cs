using System.IO.Compression;

namespace HyperLib.IO.Compression
{
    public static class ZLib
    {
        public static byte[] Compress(byte[] in_uncompressedData, CompressionLevel in_compressionLevel = CompressionLevel.Optimal)
        {
            if (in_compressionLevel == CompressionLevel.NoCompression)
                return in_uncompressedData;

            using var result = new MemoryStream();
            using var zlib = new ZLibStream(result, in_compressionLevel);

            zlib.Write(in_uncompressedData, 0, in_uncompressedData.Length);
            zlib.Dispose();

            return result.ToArray();
        }

        public static byte[] Decompress(byte[] in_compressedData)
        {
            using var result = new MemoryStream();
            using var compressed = new MemoryStream(in_compressedData);
            using var zlib = new ZLibStream(compressed, CompressionMode.Decompress);

            zlib.CopyTo(result);

            return result.ToArray();
        }
    }
}
