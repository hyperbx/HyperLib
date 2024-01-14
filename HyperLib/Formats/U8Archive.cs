using AuroraLib.Core;
using HyperLib.IO.Extensions;

namespace HyperLib.Formats
{
    public class U8Archive : FileBase
    {
        private const uint _signature = 0x55AA382DU;

        /* These constants are from uninitialised memory upon
           creating the original game's archives, and are used here
           to identify whether the input archive is for Sonic '06.
        
           The official tool is supposed to write 0xCC in their place,
           but the version Sonic Team used pulled from some other
           buffer and wrote these instead.
        
           This is a common error with '06 archives, as the
           uncompressed data size field also does the same thing. */
        private const uint _sonicNextCompressedSignature = 0xE4F91200U;
        private const uint _sonicNextUncompressedSignature = 0x00006301U;

        public override string Extension => ".arc";

        public bool IsIndexOnly { get; set; } = true;

        public static bool IsSonicNextArchive { get; set; } = false;

        public List<Info> Nodes { get; set; } = [];

        public U8Archive() { }

        public U8Archive(string in_path, bool in_isIndexOnly = true)
        {
            Read(in_path, in_isIndexOnly);
        }

        public void Read(string in_path, bool in_isIndexOnly)
        {
            IsIndexOnly = in_isIndexOnly;
            Read(in_path);
        }

        public override void Read(Stream in_stream)
        {
            var reader = new BinaryObjectReader(in_stream, StreamOwnership.Retain, Endianness.Big, Encoding.ASCII);
            var header = reader.ReadObject<Header>();

            var root = reader.ReadObject<Info>();

            if (root.Type != ENodeType.Directory)
                throw new InvalidCastException("The root node is not a directory.");

            Nodes.Add(root);

            for (int i = 0; i < root.DataSize - 1; i++)
                Nodes.Add(reader.ReadObject<Info>());
        }

        public struct Header : IBinarySerializable
        {
            public uint Signature;
            public uint NodeTableOffset;
            public uint HeaderSize;
            public uint DataOffset;

            public void Read(BinaryObjectReader in_reader)
            {
                Signature = in_reader.ReadUInt32();

                in_reader.Endianness = in_reader.GetEndiannessFromSignature(_signature, Signature);

                NodeTableOffset = in_reader.ReadUInt32();
                HeaderSize = in_reader.ReadUInt32();
                DataOffset = in_reader.ReadUInt32();

                var reserved = in_reader.ReadArray<uint>(4);

                IsSonicNextArchive =
                    reserved[0] == _sonicNextCompressedSignature ||
                    reserved[3] == _sonicNextUncompressedSignature;
            }

            public void Write(BinaryObjectWriter in_writer)
            {
                in_writer.Write(_signature);
                in_writer.Write(NodeTableOffset);
                in_writer.Write(HeaderSize);
                in_writer.Write(DataOffset);

                if (IsSonicNextArchive)
                {
                    in_writer.Write(_sonicNextCompressedSignature);
                    in_writer.WriteNullBytes(12);
                }
                else
                {
                    for (int i = 0; i < 16; i++)
                        in_writer.Write<byte>(0xCC);
                }
            }
        }

        public struct Info : IBinarySerializable
        {
            public ENodeType Type;
            public UInt24 NameOffset;
            public uint DataOffset;
            public uint DataSize;
            public uint UncompressedDataSize;

            public void Read(BinaryObjectReader in_reader)
            {
                if (in_reader.Endianness == Endianness.Big)
                {
                    Type = in_reader.Read<ENodeType>();
                    NameOffset = in_reader.Read<UInt24>();
                }
                else
                {
                    NameOffset = in_reader.Read<UInt24>();
                    Type = in_reader.Read<ENodeType>();
                }

                DataOffset = in_reader.ReadUInt32();
                DataSize = in_reader.ReadUInt32();

                if (IsSonicNextArchive)
                    UncompressedDataSize = in_reader.ReadUInt32();
            }

            public void Write(BinaryObjectWriter in_writer)
            {
                in_writer.Write(Type);
                in_writer.Write(NameOffset);
                in_writer.Write(DataOffset);
                in_writer.Write(DataSize);

                if (IsSonicNextArchive)
                    in_writer.Write(UncompressedDataSize);
            }
        }

        public enum ENodeType : byte
        {
            File,
            Directory
        }
    }
}
