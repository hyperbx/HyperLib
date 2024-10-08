﻿using Amicitia.IO;
using HyperLib.IO.Extensions;

namespace HyperLib.IO
{
    public class BinaryObjectWriterEx : BinaryObjectWriter
    {
        private readonly Dictionary<string, long> _tempFields = [];

        public BinaryObjectWriterEx(string filePath, Endianness endianness, Encoding encoding = null)
            : base(filePath, endianness, encoding) { }

        public BinaryObjectWriterEx(string filePath, FileStreamingMode fileStreamingMode, Endianness endianness, Encoding encoding = null, int bufferSize = 1048576)
            : base(filePath, fileStreamingMode, endianness, encoding, bufferSize) { }

        public BinaryObjectWriterEx(Stream stream, StreamOwnership streamOwnership, Endianness endianness, Encoding encoding = null, string fileName = null, int blockSize = 1048576)
            : base(stream, streamOwnership, endianness, encoding, fileName, blockSize) { }

        public void CreateTempField(string in_name, long in_offset, int in_size)
        {
            Seek(in_offset, SeekOrigin.Begin);

            // Create padding.
            this.WriteNullBytes(in_size);

            if (_tempFields.ContainsKey(in_name))
            {
                _tempFields[in_name] = in_offset;
                return;
            }

            _tempFields.Add(in_name, in_offset);
        }

        public void CreateTempField(string in_name, int in_size)
        {
            CreateTempField(in_name, Position, in_size);
        }

        public unsafe void CreateTempField<T>(string in_name, long in_offset) where T : unmanaged
        {
            CreateTempField(in_name, in_offset, sizeof(T));
        }

        public unsafe void CreateTempField<T>(string in_name) where T : unmanaged
        {
            CreateTempField<T>(in_name, Position);
        }

        public void WriteTempField<T>(string in_name, T in_value, bool in_removeOffset = true) where T : unmanaged
        {
            if (!_tempFields.ContainsKey(in_name))
                return;

            var pos = Position;

            SeekToTempField(in_name, false);
            Write(in_value);
            Seek(pos, SeekOrigin.Begin);

            if (in_removeOffset)
                _tempFields.Remove(in_name);
        }

        public void SeekToTempField(string in_name, bool in_removeOffset = true)
        {
            if (!_tempFields.ContainsKey(in_name))
                return;

            Seek(_tempFields[in_name], SeekOrigin.Begin);

            if (in_removeOffset)
                _tempFields.Remove(in_name);
        }
    }
}
