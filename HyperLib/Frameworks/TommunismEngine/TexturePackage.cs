using HyperLib.Helpers;
using HyperLib.Helpers.Converters;
using Newtonsoft.Json;

namespace HyperLib.Frameworks.TommunismEngine
{
    public class TexturePackage : FileBase
    {
        public override string Extension => ".tp";

        public List<Texture> Textures { get; set; } = [];

        public TexturePackage() { }

        public TexturePackage(string in_path) : base(in_path) { }

        public override void Read(Stream in_stream)
        {
            var reader = new BinaryValueReader(in_stream, StreamOwnership.Retain, Endianness.Little, Encoding.UTF8);

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

        public override void Write(Stream in_stream, bool in_isOverwrite = true)
        {
            var writer = new BinaryValueWriterEx(in_stream, StreamOwnership.Retain, Endianness.Little, Encoding.UTF8);

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

        public override void Import(string in_path)
        {
            foreach (var file in Directory.EnumerateFiles(in_path, "*.png"))
            {
                Logger.Log($"Importing texture: {FileSystemHelper.GetRelativeDirectoryName(in_path, file)}");

                var textureData = File.ReadAllBytes(file);
                var attributeFile = Path.Combine(Path.GetDirectoryName(file), Path.ChangeExtension(file, ".json"));

                byte[] attributeData = new byte[9];

                if (File.Exists(attributeFile))
                    attributeData = JsonConvert.DeserializeObject<Texture>(File.ReadAllText(attributeFile)).Attributes;

                Textures.Add(new Texture(textureData, attributeData));
            }
        }

        public override void Export(string in_path = "")
        {
            if (string.IsNullOrEmpty(in_path))
                in_path = FileSystemHelper.GetDirectoryNameOfFileName(Location);

            var dir = Directory.CreateDirectory(in_path).FullName;
            var count = Textures.Count;

            Logger.Log($"Exporting {count} {StringHelper.Pluralise("texture", count)}: {Path.GetFileName(Location)}");

            for (int i = 0; i < count; i++)
            {
                var texture = Textures[i];

                File.WriteAllBytes(Path.Combine(dir, $"{i}.png"), texture.Data);

                // Export metadata for each image for extra data.
                File.WriteAllText(Path.Combine(dir, $"{i}.json"), JsonConvert.SerializeObject(texture, Formatting.Indented));
            }
        }

        public class Texture(byte[] in_data, byte[] in_attributes)
        {
            [JsonIgnore]
            public byte[] Data { get; set; } = in_data;

            [JsonConverter(typeof(ByteArrayConverter))]
            public byte[] Attributes { get; set; } = in_attributes;
        }
    }
}
