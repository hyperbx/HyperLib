﻿using System.Runtime.InteropServices;

namespace HyperLib.Helpers
{
    public class BinaryHelper
    {
        public static byte[] HexStringToByteArray(string in_hex)
        {
            in_hex = in_hex.Replace(" ", "");

            return Enumerable.Range(0, in_hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(in_hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static T SwapEndianness<T>(T value)
        {
            var bytes = new byte[Marshal.SizeOf(typeof(T))];

            Marshal.StructureToPtr(value, Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0), false);
            Array.Reverse(bytes);

            return ByteArrayToType<T>(bytes);
        }

        public static T ByteArrayToType<T>(byte[] bytes)
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
