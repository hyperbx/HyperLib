using Amicitia.IO.Binary;
using HyperLib.Helpers;
using HyperLib.Helpers.Converters;
using HyperLib.IO;
using Newtonsoft.Json;
using System.Text;

namespace HyperLib.Games.TommunismEngine
{
    public class TexturePackage : FileBase
    {
        public List<Texture> Textures { get; set; } = [];

        public TexturePackage() { }

        public TexturePackage(string in_filePath) : base(in_filePath)
        {
            Read(in_filePath);
        }

        public void Read(string in_path)
        {
            var reader = new BinaryValueReader(in_path, Endianness.Little, Encoding.UTF8);

            var textureCount = reader.ReadInt32();

            for (int i = 0; i < textureCount; i++)
            {
                var dataStart = reader.ReadInt32();
                var dataSize = reader.ReadInt32();
                var attributes = reader.ReadArray<byte>(9);

                var pos = reader.Position;

                reader.Seek(dataStart, SeekOrigin.Begin);

                Textures.Add(new Texture(reader.ReadArray<byte>(dataSize), attributes));

                reader.Seek(pos, SeekOrigin.Begin);
            }
        }

        public void Write(string in_path)
        {
            var writer = new BinaryValueWriterEx(in_path, Endianness.Little, Encoding.UTF8);

            writer.Write(Textures.Count);

            for (int i = 0; i < Textures.Count; i++)
            {
                writer.CreateTempField<int>($"dataStart{i}");
                writer.Write(Textures[i].Data.Length);
                writer.WriteArray(Textures[i].Attributes);
            }

            for (int i = 0; i < Textures.Count; i++)
            {
                var pos = writer.Position;

                writer.WriteArray(Textures[i].Data);
                writer.WriteTempField($"dataStart{i}", (int)pos);
            }
        }

        public void Import(string in_path)
        {
            foreach (var file in Directory.EnumerateFiles(in_path, "*.png"))
            {
                var textureData = File.ReadAllBytes(file);
                var attributeFile = Path.Combine(Path.GetDirectoryName(file), Path.ChangeExtension(file, ".json"));

                byte[] attributeData = new byte[9];

                if (File.Exists(attributeFile))
                    attributeData = JsonConvert.DeserializeObject<Texture>(File.ReadAllText(attributeFile)).Attributes;

                Textures.Add(new Texture(textureData, attributeData));
            }
        }

        public void Export(string in_path = "")
        {
            if (string.IsNullOrEmpty(in_path))
                in_path = FileSystemHelper.GetDirectoryWithFileName(Location);

            var dir = Directory.CreateDirectory(in_path).FullName;

            for (int i = 0; i < Textures.Count; i++)
            {
                File.WriteAllBytes(Path.Combine(dir, $"{i}.png"), Textures[i].Data);
                File.WriteAllText(Path.Combine(dir, $"{i}.json"), JsonConvert.SerializeObject(Textures[i], Formatting.Indented));
            }
        }

        public class Texture
        {
            [JsonIgnore]
            public byte[] Data { get; set; }

            [JsonConverter(typeof(ByteArrayConverter))]
            public byte[] Attributes { get; set; } = new byte[9];

            public Texture(byte[] in_data, byte[] in_attributes)
            {
                Data = in_data;
                Attributes = in_attributes;
            }
        }
    }
}
