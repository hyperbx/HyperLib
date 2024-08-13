using HyperLib.IO.Compression;
using HyperLib.IO.Extensions;
using System.IO.Compression;

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

        // TODO: make these members not static...
        public static bool IsSonicNextArchive { get; set; } = false;

        public static CompressionLevel CompressionLevel { get; set; } = IsSonicNextArchive ? CompressionLevel.Optimal : CompressionLevel.NoCompression;

        public ArchiveDirectory Root { get; set; } = new();

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

            var root = reader.ReadObject<Node>();

            if (root.Type != ENodeType.Directory)
                throw new InvalidCastException("The root node is not a directory.");

            Root = new ArchiveDirectory(root, true);

            var stringTableOffset = header.NodeTableOffset + (root.DataSize * Node.SizeOf);
            var nodes = reader.ReadArrayAtOffset<Node>(header.NodeTableOffset, (int)root.DataSize);

            ParseNodes(0, new ArchiveDirectory() { Data = Root.Data }, true);

            uint ParseNodes(uint in_nodeIndex, ArchiveDirectory in_directory, bool in_isRoot = false)
            {
                ref var node = ref nodes[in_nodeIndex];

                reader.JumpTo(stringTableOffset + node.NameOffset);
                string name = reader.ReadString(StringBinaryFormat.NullTerminated);

                if (!in_isRoot)
                {
                    if (string.IsNullOrEmpty(name))
                        return node.DataSize;
                }

                switch (node.Type)
                {
                    case ENodeType.Directory:
                    {
                        var directory = new ArchiveDirectory
                        {
                            Name = name,
                            Parent = in_directory
                        };

                        in_directory.Data.Add(directory);

                        if (in_isRoot)
                            Root = directory;

                        in_directory = directory;

                        uint childNodeIndex = ++in_nodeIndex;

                        while (childNodeIndex < node.DataSize)
                            childNodeIndex = ParseNodes(childNodeIndex, in_directory);

                        return node.DataSize;
                    }

                    case ENodeType.File:
                    {
                        var file = new ArchiveFile()
                        {
                            Name = name,
                            Parent = in_directory,
                            UncompressedDataSize = node.UncompressedDataSize
                        };

                        in_directory.Data.Add(file);

                        return ++in_nodeIndex;
                    }

                    default:
                        throw new NotSupportedException($"Encountered an entry with an unsupported type: {node.Type:X}");
                }
            }
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
                in_writer.Write(Signature);
                in_writer.Write(NodeTableOffset);
                in_writer.Write(HeaderSize);
                in_writer.Write(DataOffset);

                if (IsSonicNextArchive)
                {
                    if (CompressionLevel == CompressionLevel.NoCompression)
                    {
                        in_writer.WriteNullBytes(12);
                        in_writer.Write(_sonicNextUncompressedSignature);
                    }
                    else
                    {
                        in_writer.Write(_sonicNextCompressedSignature);
                        in_writer.WriteNullBytes(12);
                    }
                }
                else
                {
                    for (int i = 0; i < 16; i++)
                        in_writer.Write<byte>(0xCC);
                }
            }
        }

        public struct Node : IBinarySerializable
        {
            public ENodeType Type;
            public uint NameOffset;
            public uint DataOffset;
            public uint DataSize;
            public uint UncompressedDataSize;

            public static int SizeOf { get; } = IsSonicNextArchive ? 16 : 12;

            public void Read(BinaryObjectReader in_reader)
            {
                if (in_reader.Endianness == Endianness.Big)
                {
                    Type = in_reader.Read<ENodeType>();
                    NameOffset = in_reader.ReadUInt24();
                }
                else
                {
                    NameOffset = in_reader.ReadUInt24();
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
                in_writer.WriteUInt24(NameOffset);
                in_writer.Write(DataOffset);
                in_writer.Write(DataSize);

                if (IsSonicNextArchive)
                    in_writer.Write(UncompressedDataSize);
            }
        }

        public class ArchiveNode { }

        public class ArchiveDirectory : ArchiveNode
        {
            public string Name { get; set; }
            public ArchiveNode Parent { get; set; }
            public List<ArchiveNode> Data { get; set; } = [];

            public ArchiveDirectory() { }

            public ArchiveDirectory(Node in_node, bool in_isRoot = false)
            {
                if (in_isRoot)
                    Name = ".";
            }
        }

        public class ArchiveFile : ArchiveNode
        {
            public string Name { get; set; }
            public ArchiveNode Parent { get; set; }
            public byte[] Data { get; set; }
            public uint UncompressedDataSize { get; set; }
            public bool IsCompressed { get; private set; } = IsSonicNextArchive;

            public void Export(string in_path)
            {
                File.WriteAllBytes(in_path, IsCompressed ? ZLib.Decompress(Data) : Data);
            }

            public void Compress(CompressionLevel in_compressionLevel)
            {
                if (!IsSonicNextArchive)
                    throw new NotSupportedException("Compression for this format is only supported by SONIC THE HEDGEHOG.");

                IsCompressed = true;

                UncompressedDataSize = (uint)Data.Length;
                Data = ZLib.Compress(Data, in_compressionLevel);
            }

            public void Decompress()
            {
                if (!IsSonicNextArchive)
                    throw new NotSupportedException("Compression for this format is only supported by SONIC THE HEDGEHOG.");

                IsCompressed = false;

                UncompressedDataSize = 0;
                Data = ZLib.Decompress(Data);
            }
        }

        public enum ENodeType : byte
        {
            File,
            Directory
        }
    }
}
