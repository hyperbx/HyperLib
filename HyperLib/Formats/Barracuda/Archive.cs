using HyperLib.Helpers;
using HyperLib.IO.Compression;
using HyperLib.IO.Crypto;
using HyperLib.IO.Extensions;
using System.IO.Compression;

namespace HyperLib.Formats.Barracuda
{
    public class Archive : FileBase
    {
        private const uint _pcSignature = 0x46505556; // "FPUV"
        private const uint _pcVersion = 3;
        private const uint _xboxSignature = 0x414B5046; // "AKPF"
        private const uint _xboxVersion = 5;

        public override string Extension => ".apf";

        public bool IsPCVersion { get; set; } = false;

        /* TODO: figure out why compressed files just stop the game from launching,
                 despite using the correct compression algorithm. */
        public CompressionLevel CompressionLevel => CompressionLevel.NoCompression;

        public List<ArchiveFile> Files { get; set; } = [];

        public Archive() { }

        public Archive(string in_path) : base(in_path) { }

        public override void Read(Stream in_stream)
        {
            var reader = new BinaryObjectReader(in_stream, StreamOwnership.Retain, Endianness.Big);

            if (!reader.IsSignatureValid(_xboxSignature, false))
            {
                reader.Seek(0, SeekOrigin.Begin);

                if (reader.IsSignatureValid(_pcSignature, false))
                {
                    IsPCVersion = true;
                    reader.Endianness = Endianness.Little;
                }
                else
                {
                    throw new NotSupportedException("Could not identify archive type.");
                }
            }

            var version = reader.ReadUInt32();

            if (version != (IsPCVersion ? _pcVersion : _xboxVersion))
                throw new NotSupportedException($"Unsupported archive version.");

            var fileTableOffset = reader.ReadUInt32();
            var fileCount = reader.ReadUInt32();
            var reserved = reader.ReadArray<uint>(IsPCVersion ? 13 : 2);

            reader.Seek(fileTableOffset, SeekOrigin.Begin);

            for (int i = 0; i < fileCount; i++)
                Files.Add(ArchiveFile.Read(reader, IsPCVersion));
        }

        public override void Write(Stream in_stream, bool in_isOverwrite = true)
        {
            var writer = new BinaryObjectWriterEx(in_stream, StreamOwnership.Retain, Endianness.Big);

            writer.Write(IsPCVersion ? _pcSignature : _xboxSignature);

            if (IsPCVersion)
                writer.Endianness = Endianness.Little;

            writer.Write(IsPCVersion ? _pcVersion : _xboxVersion);
            writer.CreateTempField<uint>("fileTableOffset");
            writer.Write(Files.Count);

            // TODO: figure this out.
            if (IsPCVersion)
            {
                writer.Write(206594);
                writer.Write(943264441);
                writer.Write(1819047238);
                writer.WriteArray([0, 0, 0, 0, 0, 0, 0, 0]);
                writer.Write(51);
                writer.Write(3817756730);
            }
            else
            {
                writer.Write(0);
                writer.Write(55);
            }

            for (int i = 0; i < Files.Count; i++)
            {
                Files[i].Offset = (uint)writer.Position;

                writer.WriteArray(Files[i].Data);
            }

            writer.WriteTempField("fileTableOffset", (uint)writer.Position);

            for (int i = 0; i < Files.Count; i++)
                Files[i].Write(writer, IsPCVersion);
        }

        public override void Import(string in_path)
        {
            if (!Directory.Exists(in_path))
                return;

            foreach (var file in Directory.EnumerateFiles(in_path, "*", SearchOption.AllDirectories))
            {
                var relativePath = FileSystemHelper.GetRelativeDirectoryName(in_path, FileSystemHelper.TruncateAllExtensions(file), true);

                Logger.Log($"Importing file: {relativePath}");

                var data = File.ReadAllBytes(file);
                var uncompressedSize = (uint)data.Length;
                var crc32 = CRC32.Compute(data);

                if (CompressionLevel != CompressionLevel.NoCompression)
                {
                    if (IsPCVersion)
                    {
                        data = ZLib.Compress(data, CompressionLevel);
                    }
                    else
                    {
                        data = XCompress.Compress(data);
                    }
                }

                var compressedSize = CompressionLevel == CompressionLevel.NoCompression
                    ? 0
                    : (uint)data.Length;

                var node = new ArchiveFile(relativePath, uncompressedSize, compressedSize, ArchiveFile.GetTypeFromName(file), crc32, data);

                Files.Add(node);
            }
        }

        public override void Export(string in_path = "")
        {
            if (string.IsNullOrEmpty(in_path))
                in_path = FileSystemHelper.GetDirectoryNameOfFileName(Location);

            var dir = Directory.CreateDirectory(in_path).FullName;

            for (int i = 0; i < Files.Count; i++)
            {
                var file = Files[i];
                var name = $"{file.Name}.{file.Type}.bin";

                Logger.Log($"Exporting file: {name}");
#if !DEBUG
                try
#endif
                {
                    var filePath = Path.Combine(dir, name);
                    var dirPath = Path.GetDirectoryName(filePath);

                    Directory.CreateDirectory(dirPath);

                    if (file.IsCompressed)
                    {
                        if (IsPCVersion)
                        {
                            File.WriteAllBytes(filePath, ZLib.Decompress(file.Data));
                        }
                        else
                        {
                            File.WriteAllBytes(filePath, XCompress.Decompress(file.Data, (int)file.Size));
                        }
                    }
                    else
                    {
                        File.WriteAllBytes(filePath, file.Data);
                    }
                }
#if !DEBUG
                catch (Exception ex)
                {
                    Logger.Error($"Exporting failed: {name}\nReason: {ex}");
                }
#endif
            }
        }
    }

    public class ArchiveFile
    {
        public string Name;
        public uint Offset;
        public uint Size;
        public uint CompressedSize;
        public uint Type;
        public uint CRC32;
        public bool IsCompressed;
        public byte[] Data;

        public ArchiveFile() { }

        public ArchiveFile(string in_name, uint in_size, uint in_compressedSize, uint in_type, uint in_hash, byte[] in_data)
        {
            Name = in_name;
            Size = in_size;
            CompressedSize = in_compressedSize;
            Type = in_type;
            CRC32 = Type == 0 ? 0 : in_hash;
            IsCompressed = in_compressedSize > 0;
            Data = in_data;
        }

        public static ArchiveFile Read(BinaryObjectReader in_reader, bool in_isPCVersion)
        {
            var entry = new ArchiveFile
            {
                Name = in_reader.ReadString(in_isPCVersion ? StringBinaryFormat.NullTerminated : StringBinaryFormat.PrefixedLength32),
                Offset = in_reader.ReadUInt32(),
                Size = in_reader.ReadUInt32(),
                CompressedSize = in_reader.ReadUInt32(),
                Type = in_reader.ReadUInt32(),
                CRC32 = in_reader.ReadUInt32(),
                IsCompressed = in_reader.ReadUInt32() == 1
            };

            entry.Data = in_reader.ReadArrayAtOffset<byte>(entry.Offset, (int)(entry.IsCompressed ? entry.CompressedSize : entry.Size));

            return entry;
        }

        public void Write(BinaryObjectWriter in_writer, bool in_isPCVersion)
        {
            if (in_isPCVersion)
            {
                in_writer.WriteStringNullTerminated(Encoding.UTF8, Name);
            }
            else
            {
                in_writer.WriteStringPrefixedLength32(Encoding.UTF8, Name);
            }

            in_writer.Write(Offset);
            in_writer.Write(Size);
            in_writer.Write(CompressedSize);
            in_writer.Write(Type);
            in_writer.Write(Type == 0 ? 0 : CRC32);
            in_writer.Write(IsCompressed ? 1 : 0);
        }

        public static uint GetTypeFromName(string in_name)
        {
            var extensions = FileSystemHelper.GetAllExtensions(in_name);

            if (extensions.Count <= 1)
                return 0;

            if (uint.TryParse(extensions[extensions.Count - 2], out var out_type))
                return out_type;

            return 0;
        }
    }
}
