using System.Runtime.InteropServices;

namespace HyperLib.IO.Compression
{
    public class XCompress
    {
        private static readonly bool Is64Bit = Environment.Is64BitProcess;

        #region x86

        [DllImport("xcompress32.dll", EntryPoint = "XMemCreateCompressionContext")]
        private static extern int XMemCreateCompressionContext32(XMemCodecType in_codecType, int in_pCodecParams, int in_flags, ref int in_rContext);

        [DllImport("xcompress32.dll", EntryPoint = "XMemCreateDecompressionContext")]
        private static extern int XMemCreateDecompressionContext32(XMemCodecType in_codecType, int in_pCodecParams, int in_flags, ref int in_rContext);

        [DllImport("xcompress32.dll", EntryPoint = "XMemDestroyCompressionContext")]
        private static extern void XMemDestroyCompressionContext32(int in_pContext);

        [DllImport("xcompress32.dll", EntryPoint = "XMemDestroyDecompressionContext")]
        private static extern void XMemDestroyDecompressionContext32(int in_pContext);

        [DllImport("xcompress32.dll", EntryPoint = "XMemResetCompressionContext")]
        private static extern int XMemResetCompressionContext32(int in_pContext);

        [DllImport("xcompress32.dll", EntryPoint = "XMemResetDecompressionContext")]
        private static extern int XMemResetDecompressionContext32(int in_pContext);

        [DllImport("xcompress32.dll", EntryPoint = "XMemCompress")]
        private static extern int XMemCompress32(int in_pContext, byte[] in_pDestination, ref int in_rDestSize, byte[] in_pSource, int in_rSrcSize);

        [DllImport("xcompress32.dll", EntryPoint = "XMemDecompress")]
        private static extern int XMemDecompress32(int in_pContext, byte[] in_pDestination, ref int in_rDestSize, byte[] in_pSource, int in_rSrcSize);

        #endregion

        #region x64

        [DllImport("xcompress64.dll", EntryPoint = "XMemCreateCompressionContext")]
        private static extern long XMemCreateCompressionContext64(XMemCodecType in_codecType, int in_pCodecParams, int in_flags, ref long in_rContext);

        [DllImport("xcompress64.dll", EntryPoint = "XMemCreateDecompressionContext")]
        private static extern long XMemCreateDecompressionContext64(XMemCodecType in_codecType, int in_pCodecParams, int in_flags, ref long in_rContext);

        [DllImport("xcompress64.dll", EntryPoint = "XMemDestroyCompressionContext")]
        private static extern void XMemDestroyCompressionContext64(long in_pContext);

        [DllImport("xcompress64.dll", EntryPoint = "XMemDestroyDecompressionContext")]
        private static extern void XMemDestroyDecompressionContext64(long in_pContext);

        [DllImport("xcompress64.dll", EntryPoint = "XMemResetCompressionContext")]
        private static extern long XMemResetCompressionContext64(long in_pContext);

        [DllImport("xcompress64.dll", EntryPoint = "XMemResetDecompressionContext")]
        private static extern long XMemResetDecompressionContext64(long in_pContext);

        [DllImport("xcompress64.dll", EntryPoint = "XMemCompress")]
        private static extern long XMemCompress64(long in_pContext, byte[] in_pDestination, ref int in_rDestSize, byte[] in_pSource, int in_rSrcSize);

        [DllImport("xcompress64.dll", EntryPoint = "XMemDecompress")]
        private static extern long XMemDecompress64(long in_pContext, byte[] in_pDestination, ref int in_rDestSize, byte[] in_pSource, int in_rSrcSize);

        #endregion

        public static byte[] Compress(byte[] in_uncompressedData)
        {
            var compressedData = new byte[in_uncompressedData.Length];

            var len = in_uncompressedData.Length;
            var context = 0L;

            XMemCreateCompressionContext(XMemCodecType.LZX, 0, 0, ref context);
            XMemResetCompressionContext(context);
            XMemCompress(context, compressedData, ref len, in_uncompressedData, len);
            XMemDestroyCompressionContext(context);

            Array.Resize(ref compressedData, len);

            return compressedData;
        }

        public static byte[] Decompress(byte[] in_compressedData, int in_uncompressedSize)
        {
            var uncompressedData = new byte[in_uncompressedSize];

            var startLen = in_compressedData.Length;
            var endLen = uncompressedData.Length;
            var context = 0L;

            XMemCreateDecompressionContext(XMemCodecType.LZX, 0, 0, ref context);
            XMemResetDecompressionContext(context);
            XMemDecompress(context, uncompressedData, ref endLen, in_compressedData, startLen);
            XMemDestroyDecompressionContext(context);

            Array.Resize(ref uncompressedData, endLen);

            return uncompressedData;
        }

        public static long XMemCreateCompressionContext(XMemCodecType in_codecType, int in_pCodecParams, int in_flags, ref long in_rContext)
        {
            if (Is64Bit)
            {
                return XMemCreateCompressionContext64(in_codecType, in_pCodecParams, in_flags, ref in_rContext);
            }
            else
            {
                var pContext = (int)in_rContext;
                var result = XMemCreateCompressionContext32(in_codecType, in_pCodecParams, in_flags, ref pContext);

                in_rContext = pContext;

                return result;
            }
        }

        public static long XMemCreateDecompressionContext(XMemCodecType in_codecType, int in_pCodecParams, int in_flags, ref long in_rContext)
        {
            if (Is64Bit)
            {
                return XMemCreateDecompressionContext64(in_codecType, in_pCodecParams, in_flags, ref in_rContext);
            }
            else
            {
                var pContext = (int)in_rContext;
                var result = XMemCreateDecompressionContext32(in_codecType, in_pCodecParams, in_flags, ref pContext);

                in_rContext = pContext;

                return result;
            }
        }

        public static void XMemDestroyCompressionContext(long in_pContext)
        {
            if (Is64Bit)
            {
                XMemDestroyCompressionContext64(in_pContext);
            }
            else
            {
                XMemDestroyCompressionContext32((int)in_pContext);
            }
        }

        public static void XMemDestroyDecompressionContext(long in_pContext)
        {
            if (Is64Bit)
            {
                XMemDestroyDecompressionContext64(in_pContext);
            }
            else
            {
                XMemDestroyDecompressionContext32((int)in_pContext);
            }
        }

        public static long XMemResetCompressionContext(long in_pContext)
        {
            return Is64Bit
                ? XMemResetCompressionContext64(in_pContext)
                : XMemResetCompressionContext32((int)in_pContext);
        }

        public static long XMemResetDecompressionContext(long in_pContext)
        {
            return Is64Bit
                ? XMemResetDecompressionContext64(in_pContext)
                : XMemResetDecompressionContext32((int)in_pContext);
        }

        public static long XMemCompress(long in_pContext, byte[] in_pDestination, ref int in_pDestSize, byte[] in_pSource, int in_rSrcSize)
        {
            return Is64Bit
                ? XMemCompress64(in_pContext, in_pDestination, ref in_pDestSize, in_pSource, in_rSrcSize)
                : XMemCompress32((int)in_pContext, in_pDestination, ref in_pDestSize, in_pSource, in_rSrcSize);
        }

        public static long XMemDecompress(long in_pContext, byte[] in_pDestination, ref int in_pDestSize, byte[] in_pSource, int in_rSrcSize)
        {
            return Is64Bit
                ? XMemDecompress64(in_pContext, in_pDestination, ref in_pDestSize, in_pSource, in_rSrcSize)
                : XMemDecompress32((int)in_pContext, in_pDestination, ref in_pDestSize, in_pSource, in_rSrcSize);
        }

        public struct XMemCodecParametersLZX
        {
            public int Flags;
            public int WindowSize;
            public int CompressionPartitionSize;
        }

        public enum XMemCodecType
        {
            Default,
            LZX
        }
    }
}
