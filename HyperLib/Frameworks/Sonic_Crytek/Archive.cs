using HyperLib.Helpers;
using HyperLib.IO;
using HyperLib.IO.Extensions;

namespace HyperLib.Frameworks.Sonic_Crytek
{
    public class Archive : FileBase
    {
        public override string Extension => ".wiiu.stream"; // *.*.stream

        public bool IsIndexOnly { get; set; } = true;

        public List<ArchiveFile> Files { get; set; } = [];

        public Archive() { }

        public Archive(string in_path, bool in_isIndexOnly = true)
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
            var reader = new BinaryObjectReader(in_stream, StreamOwnership.Retain, Endianness.Big, Encoding.UTF8);

            if (!reader.IsSignatureValid(0x7374726D)) // strm
                return;

            while (reader.Position != reader.Length)
            {
                var file = reader.ReadObject<ArchiveFile>();
                
                file.ReadData(reader, IsIndexOnly);

                Files.Add(file);
            }
        }

        public override void Write(Stream in_stream, bool in_isOverwrite = true)
        {
            var writer = new BinaryObjectWriter(in_stream, StreamOwnership.Retain, Endianness.Big, Encoding.UTF8);

            writer.Write(0x7374726D); // strm

            foreach (var file in Files)
                writer.WriteObject(file);
        }

        public override void Import(string in_path)
        {
            if (!Directory.Exists(in_path))
                return;

            var metadataPath = Path.Combine(in_path, Path.GetFileName(in_path) + ".meta");
            var metadataList = new List<string>();

            void ImportFile(string in_file)
            {
                var relativePath = FileSystemHelper.GetRelativeDirectoryName(in_path, in_file, true);

                // TODO: compression and CRC32 hashing.
                var data = File.ReadAllBytes(in_file);
                var node = new ArchiveFile(0, (uint)data.Length, 0, 0, relativePath, data);

                SetSpecialFlags(ref node);

                if (node.SpecialFlags == ESpecialFlags.None)
                {
                    Logger.Log($"Importing file: {relativePath}");
                }
                else
                {
                    Logger.Utility($"Importing special file: {relativePath} (flags: {node.SpecialFlags})");
                }

                Files.Add(node);
            }

            if (File.Exists(metadataPath))
            {
                // Excludes the metadata file.
                metadataList.Add(metadataPath);

                using (var reader = File.OpenText(metadataPath))
                {
                    while (!reader.EndOfStream)
                    {
                        var relativePath = reader.ReadLine();

                        if (string.IsNullOrEmpty(relativePath))
                            continue;

                        var filePath = Path.Combine(in_path, FileSystemHelper.ConvertPathToWindows(relativePath));

                        if (File.Exists(filePath))
                        {
                            ImportFile(filePath);
                        }
                        else
                        {
                            Logger.Error($"Importing failed: {relativePath}");
                            Logger.Warning("Importing files out of order!");
                        }

                        metadataList.Add(filePath);
                    }
                }
            }
            else
            {
                Logger.Warning("Importing files out of order!");
            }

            foreach (var file in Directory.EnumerateFiles(in_path, "*", SearchOption.AllDirectories))
            {
                if (metadataList.Contains(file))
                    continue;

                ImportFile(file);
            }
        }

        public override void Export(string in_path = "")
        {
            if (string.IsNullOrEmpty(in_path))
                in_path = FileSystemHelper.GetDirectoryNameOfFileName(Location);

            var dir = Directory.CreateDirectory(in_path).FullName;

            // Create metadata file for strict file order.
            using (var writer = File.CreateText(Path.Combine(dir, Path.GetFileName(in_path) + ".meta")))
            {
                for (int i = 0; i < Files.Count; i++)
                {
                    var file = Files[i];

                    Logger.Log($"Exporting file: {file.Name}");

#if !DEBUG
                    try
#endif
                    {
                        if (file.SpecialFlags.ToString().EndsWith("Alt"))
                        {
                            file.Name = FileSystemHelper.ConvertPathToUnix(
                                FileSystemHelper.ChangeFileName(file.Name, $"{Path.GetFileNameWithoutExtension(file.Name)}_alt"));
                        }

                        var filePath = Path.Combine(dir, file.Name);
                        var dirPath = Path.GetDirectoryName(filePath);

                        Directory.CreateDirectory(dirPath);

                        File.WriteAllBytes(filePath, file.Data);
                    }
#if !DEBUG
                    catch (Exception ex)
                    {
                        Logger.Error($"Exporting failed: {file}\nReason: {ex}");
                    }
#endif

                    // Export file name to metadata.
                    writer.WriteLine(file.Name);
                }
            }
        }

        public static void SetSpecialFlags(ref ArchiveFile in_file)
        {
            if (!in_file.Name.Contains("characters") || !(Path.GetExtension(in_file.Name) is ".dds" or ".mtl"))
                return;

            var fileName = Path.GetFileNameWithoutExtension(in_file.Name);
            bool isAlt = fileName.EndsWith("_alt");

            if (fileName.StartsWith("sonic"))
            {
                in_file.SpecialFlags = isAlt
                    ? ESpecialFlags.SonicAlt
                    : ESpecialFlags.Sonic;
            }
            else if (fileName.StartsWith("tails"))
            {
                in_file.SpecialFlags = isAlt
                    ? ESpecialFlags.TailsAlt
                    : ESpecialFlags.Tails;
            }
            else if (fileName.StartsWith("amy"))
            {
                in_file.SpecialFlags = isAlt
                    ? ESpecialFlags.AmyAlt
                    : ESpecialFlags.Amy;
            }
            else if (fileName.StartsWith("knuckles"))
            {
                in_file.SpecialFlags = isAlt
                    ? ESpecialFlags.KnucklesAlt
                    : ESpecialFlags.Knuckles;
            }
            else
            {
                in_file.SpecialFlags = ESpecialFlags.None;
            }

            if (!isAlt)
                return;

            // Remove "_alt" suffix.
            in_file.Name = FileSystemHelper.ConvertPathToUnix(
                FileSystemHelper.ChangeFileName(in_file.Name, fileName.Remove(fileName.Length - 4)));
        }

        // LZSS compression research by NeKit, original decompression code by Paraxade.
        /*
            References;
                - https://forums.sonicretro.org/index.php?posts/810905
                - https://forums.sonicretro.org/index.php?posts/811201
        */
        ///////////////////////////////////////////////////////////////////////////////////

        public static unsafe bool Decompress(byte[] in_compressedData, uint in_compressedSize, ref byte[] in_uncompressedData, uint in_uncompressedSize)
        {
            static unsafe uint ReadSize(ref byte* in_src, bool in_isSeek)
            {
                var b = *in_src++;

                var size = in_isSeek
                    ? (uint)(b & 0x7F)
                    : (uint)(b & 0x3F);

                while ((b & 0x80) != 0)
                {
                    b = *in_src++;
                    size = (size << 7) | (uint)(b & 0x7F);
                }

                return size;
            }

            fixed (byte* p_compressedData = in_compressedData)
            fixed (byte* p_uncompressedData = in_uncompressedData)
            {
                byte* srcIndex = p_compressedData;
                byte* dstIndex = p_uncompressedData;

                while ((srcIndex - p_compressedData < in_compressedSize) && (dstIndex - p_uncompressedData < in_uncompressedSize))
                {
                    var b = *srcIndex;
                    var size = ReadSize(ref srcIndex, false);

                    if ((b & 0x40) != 0)
                    {
                        size += 3;

                        var seekSize = ReadSize(ref srcIndex, true);
                        var seekStart = dstIndex - seekSize;
                        var seekEnd = dstIndex;

                        for (var i = 0; i < size; i++)
                        {
                            *dstIndex++ = *seekStart++;

                            if (seekStart >= seekEnd)
                                seekStart -= seekSize;
                        }
                    }
                    else
                    {
                        for (uint i = 0; i < size; i++)
                            *dstIndex++ = *srcIndex++;
                    }
                }

                return (srcIndex - p_compressedData == in_compressedSize) && (dstIndex - p_uncompressedData == in_uncompressedSize);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////

        public struct ArchiveFile(uint in_compressedSize, uint in_uncompressedSize, uint in_hash, ESpecialFlags in_specialFlags, string in_name, byte[] in_data) : IBinarySerializable
        {
            public uint CompressedSize = in_compressedSize;
            public uint UncompressedSize = in_uncompressedSize;
            public uint Hash = in_hash;
            public ushort UnkField1;
            public ESpecialFlags SpecialFlags = in_specialFlags;
            public string Name = in_name;
            public byte[] Data = in_data;

            public void Read(BinaryObjectReader in_reader)
            {
                CompressedSize = in_reader.ReadUInt32();
                UncompressedSize = in_reader.ReadUInt32();
                Hash = in_reader.ReadUInt32();
                UnkField1 = in_reader.ReadUInt16();
                SpecialFlags = (ESpecialFlags)in_reader.ReadUInt16();
                Name = in_reader.ReadString(StringBinaryFormat.NullTerminated);
            }

            public void Write(BinaryObjectWriter in_writer)
            {
                in_writer.Write(CompressedSize);
                in_writer.Write(UncompressedSize);
                in_writer.Write(Hash);
                in_writer.Write(UnkField1);
                in_writer.Write(SpecialFlags);
                in_writer.WriteStringNullTerminated(Encoding.UTF8, Name);
                in_writer.WriteArray(Data);
            }

            public void ReadData(BinaryObjectReader in_reader, bool in_isIndexOnly = true)
            {
                if (in_isIndexOnly)
                {
                    in_reader.Seek(CompressedSize > 0 ? CompressedSize : UncompressedSize, SeekOrigin.Current);
                    return;
                }

                if (CompressedSize > 0)
                {
                    byte[] compressedData = in_reader.ReadArray<byte>((int)CompressedSize);
                    byte[] uncompressedData = new byte[UncompressedSize];

                    if (!Decompress(compressedData, CompressedSize, ref uncompressedData, UncompressedSize))
                    {
                        Logger.Error("Failed to decompress data.");
                        return;
                    }

                    Data = uncompressedData;
                }
                else
                {
                    Data = in_reader.ReadArray<byte>((int)UncompressedSize);
                }
            }
        }

        // Flag research by ik-01.
        public enum ESpecialFlags : ushort
        {
            KnucklesAlt = 0xFFFC,
            AmyAlt = 0xFFFD,
            TailsAlt = 0xFFFE,
            SonicAlt = 0xFFFF,
            None = 0,
            Sonic = 1,
            Tails = 2,
            Amy = 3,
            Knuckles = 4
        }
    }
}
