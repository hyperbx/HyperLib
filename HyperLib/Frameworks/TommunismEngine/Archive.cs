using HyperLib.Helpers;
using HyperLib.IO.Extensions;

namespace HyperLib.Frameworks.TommunismEngine
{
    public class Archive : FileBase
    {
        public override string Extension => ".dat";

        public bool IsIndexOnly { get; set; } = true;

        public List<ArchiveDirectory> Directories { get; set; } = [];
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
            var reader = new BinaryValueReader(in_stream, StreamOwnership.Retain, Endianness.Little, Encoding.UTF8);

            var dirCount = reader.ReadInt32();
            var dirInfos = new List<ArchiveDirectoryInfo>();

            for (int i = 0; i < dirCount; i++)
                dirInfos.Add(reader.Read<ArchiveDirectoryInfo>());

            var fileCount = reader.ReadInt32();
            var fileInfos = new List<ArchiveFileInfo>();

            for (int i = 0; i < fileCount; i++)
                fileInfos.Add(reader.Read<ArchiveFileInfo>());

            var dirStringTableLength = reader.ReadInt32();
            var fileStringTableLength = reader.ReadInt32();

            var stringTableOffset = reader.Position;
            var stringTableLength = dirStringTableLength + fileStringTableLength;

            for (int i = 0; i < dirCount; i++)
            {
                if (reader.Position > stringTableOffset + dirStringTableLength)
                    throw new IndexOutOfRangeException("This directory's name is outside the bounds of the string table.");

                Directories.Add(new ArchiveDirectory(dirInfos[i], reader.ReadString(StringBinaryFormat.NullTerminated)));
            }

            for (int i = 0; i < fileCount; i++)
            {
                if (reader.Position > stringTableOffset + stringTableLength)
                    throw new IndexOutOfRangeException("This file's name is outside the bounds of the string table.");

                Files.Add(new ArchiveFile(fileInfos[i], reader.ReadString(StringBinaryFormat.NullTerminated)));
            }

            if (IsIndexOnly)
                return;

            foreach (var file in Files)
                file.Data = file.Read(reader);
        }

        public override unsafe void Write(Stream in_stream, bool in_isOverwrite = true)
        {
            var writer = new BinaryValueWriterEx(in_stream, StreamOwnership.Retain, Endianness.Little, Encoding.UTF8);

            writer.Write(Directories.Count);

            // Root directory info (0 index, 0 files).
            writer.Write<long>(0);

            // This data doesn't matter afaik.
            foreach (var dir in Directories)
            {
                // Skip the last directory, for some reason.
                if (dir == Directories.Last())
                    break;

                writer.Write(dir.Info.Index);
                writer.Write(dir.Info.FileCount <= 0 ? 0 : dir.Info.FileCount - 1);
            }

            writer.Write(Files.Count);

            var fileTableOffset = writer.Position;

            // Write padding for later.
            writer.WriteNullBytes(sizeof(ArchiveFileInfo) * Files.Count);

            writer.CreateTempField<int>("dirStringTableLength");
            writer.CreateTempField<int>("fileStringTableLength");

            var dirStringTableLength = 0;
            foreach (var dir in Directories)
            {
                writer.WriteStringNullTerminated(Encoding.UTF8, dir.Name);
                dirStringTableLength += dir.Name.Length + 1;
            }

            var fileStringTableLength = 0;
            foreach (var file in Files)
            {
                writer.WriteStringNullTerminated(Encoding.UTF8, file.Name);
                fileStringTableLength += file.Name.Length + 1;
            }

            writer.WriteTempField("dirStringTableLength", dirStringTableLength);
            writer.WriteTempField("fileStringTableLength", fileStringTableLength);

            foreach (var file in Files)
            {
                var info = file.Info;
                info.DataStart = (int)writer.Position;
                file.Info = info;

                writer.WriteArray(file.Data);
            }

            writer.Seek(fileTableOffset, SeekOrigin.Begin);

            foreach (var file in Files)
                writer.Write(file.Info);
        }

        public override void Import(string in_path)
        {
            if (!Directory.Exists(in_path))
                return;

            int dirIndex = Directories.Count;

            if (dirIndex == 0)
                dirIndex++;

            foreach (var dir in Directory.EnumerateDirectories(in_path, "*", SearchOption.AllDirectories))
            {
                var relativePath = FileSystemHelper.GetRelativeDirectoryName(in_path, dir, true);

                Logger.Log($"Importing directory: {relativePath}");

                var info = new ArchiveDirectoryInfo(dirIndex, Directory.EnumerateFiles(dir).Count());

                Directories.Add(new ArchiveDirectory(info, relativePath));

                dirIndex++;
            }

            foreach (var file in Directory.EnumerateFiles(in_path, "*", SearchOption.AllDirectories))
            {
                var relativePath = FileSystemHelper.GetRelativeDirectoryName(in_path, file, true);

                Logger.Log($"Importing file: {relativePath}");

                var data = File.ReadAllBytes(file);
                var info = new ArchiveFileInfo(0, data.Length, 0);
                var node = new ArchiveFile(info, relativePath)
                {
                    Data = data
                };

                Files.Add(node);
            }
        }

        public override void Export(string in_path = "")
        {
            if (string.IsNullOrEmpty(in_path))
                in_path = FileSystemHelper.GetDirectoryNameOfFileName(Location);

            var dir = Directory.CreateDirectory(in_path).FullName;

            foreach (var file in Files)
            {
                Logger.Log($"Exporting file: {file.Name}");

#if !DEBUG
                try
#endif
                {
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
            }
        }

        public struct ArchiveDirectoryInfo(int in_index, int in_fileCount)
        {
            public int Index = in_index;
            public int FileCount = in_fileCount;
        }

        public struct ArchiveFileInfo(int in_dataStart, int in_dataSize, int in_parentIndex)
        {
            public int DataStart = in_dataStart;
            public int DataSize = in_dataSize;
            public int ParentIndex = in_parentIndex;
        }

        public class ArchiveDirectory(ArchiveDirectoryInfo in_info)
        {
            public ArchiveDirectoryInfo Info { get; set; } = in_info;
            public string Name { get; set; } = string.Empty;

            public ArchiveDirectory(ArchiveDirectoryInfo in_info, string in_name) : this(in_info)
            {
                Name = in_name;
            }
        }

        public class ArchiveFile(ArchiveFileInfo in_info)
        {
            public ArchiveFileInfo Info { get; set; } = in_info;
            public string Name { get; set; } = string.Empty;
            public byte[] Data { get; set; }

            public ArchiveFile(ArchiveFileInfo in_info, string in_name) : this(in_info)
            {
                Name = in_name;
            }

            public byte[] Read(BinaryValueReader in_reader)
            {
                var pos = in_reader.Position;

                in_reader.Seek(Info.DataStart, SeekOrigin.Begin);

                var data = in_reader.ReadArray<byte>(Info.DataSize);

                in_reader.Seek(pos, SeekOrigin.Begin);

                return data;
            }
        }
    }
}
