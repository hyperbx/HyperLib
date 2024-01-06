using Amicitia.IO.Binary;
using HyperLib.Helpers;
using HyperLib.IO;
using System.Text;

namespace HyperLib.Games.TommunismEngine
{
    public class Archive : FileBase
    {
        public List<DirectoryNode> Directories = new();
        public List<FileNode> Files = new();

        public Archive() { }

        public Archive(string in_path) : base(in_path)
        {
            Read(in_path);
        }

        public void Read(string in_path, bool in_isIndexOnly = true)
        {
            var reader = new BinaryValueReader(in_path, Endianness.Little, Encoding.UTF8);

            var dirCount = reader.ReadInt32();
            var dirInfos = new List<DirectoryInfo>();

            for (int i = 0; i < dirCount; i++)
                dirInfos.Add(reader.Read<DirectoryInfo>());

            var fileCount = reader.ReadInt32();
            var fileInfos = new List<FileInfo>();

            for (int i = 0; i < fileCount; i++)
                fileInfos.Add(reader.Read<FileInfo>());

            var dirStringTableLength = reader.ReadInt32();
            var fileStringTableLength = reader.ReadInt32();

            var stringTableOffset = reader.Position;
            var stringTableLength = dirStringTableLength + fileStringTableLength;

            for (int i = 0; i < dirCount; i++)
            {
                if (reader.Position > stringTableOffset + dirStringTableLength)
                    throw new IndexOutOfRangeException("This directory's name is outside the bounds of the string table!");

                Directories.Add(new DirectoryNode(dirInfos[i], reader.ReadString(StringBinaryFormat.NullTerminated)));
            }

            for (int i = 0; i < fileCount; i++)
            {
                if (reader.Position > stringTableOffset + stringTableLength)
                    throw new IndexOutOfRangeException("This file's name is outside the bounds of the string table!");

                Files.Add(new FileNode(fileInfos[i], reader.ReadString(StringBinaryFormat.NullTerminated)));
            }

            if (in_isIndexOnly)
                return;

            foreach (var file in Files)
                file.Data = file.Read(reader);
        }

        public void Export(string in_path = "")
        {
            if (string.IsNullOrEmpty(in_path))
                in_path = FileSystemHelper.GetDirectoryWithFileName(Location);

            var dir = Directory.CreateDirectory(in_path).FullName;

            foreach (var file in Files)
            {
                var filePath = Path.Combine(dir, file.Name);
                var dirPath = Path.GetDirectoryName(filePath);

                Directory.CreateDirectory(dirPath);

                File.WriteAllBytes(filePath, file.Data);
            }
        }

        public struct DirectoryInfo
        {
            public int Index;
            public int FileCount;
        }

        public struct FileInfo
        {
            public int DataStart;
            public int DataSize;
            public int ParentIndex;
        }

        public class DirectoryNode
        {
            public DirectoryInfo Info { get; set; }
            public string Name { get; set; }

            public DirectoryNode(DirectoryInfo in_info)
            {
                Info = in_info;
            }

            public DirectoryNode(DirectoryInfo in_info, string in_name) : this(in_info)
            {
                Name = in_name;
            }
        }

        public class FileNode
        {
            public FileInfo Info { get; set; }
            public string Name { get; set; }
            public byte[] Data { get; set; }

            public FileNode(FileInfo in_info)
            {
                Info = in_info;
            }

            public FileNode(FileInfo in_info, string in_name) : this(in_info)
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
