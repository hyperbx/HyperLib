using Newtonsoft.Json;

namespace HyperLib.Formats.TommunismEngine
{
    public class Registry : FileBase
    {
        public override string Extension => ".dat";

        public List<string> PropertyNames { get; set; } =
        [
            "winwidth",
            "winheight",
            "widescreen",
            "fullscreen",
            "PROFILESETTING0",
            "PROFILESETTING1",
            "oldost"
        ];

        public PropertyData Data { get; set; }

        public Registry() { }

        public Registry(string in_path) : base(in_path) { }

        public override void Read(Stream in_stream)
        {
            var reader = new BinaryValueReader(in_stream, StreamOwnership.Retain, Endianness.Little, Encoding.UTF8);

            var propertyNameCount = reader.ReadInt32();

            var stringTableLength = reader.ReadInt32();
            var stringTableOffset = reader.Position;

            PropertyNames.Clear();

            for (int i = 0; i < propertyNameCount; i++)
            {
                if (reader.Position > stringTableOffset + stringTableLength)
                    throw new IndexOutOfRangeException("This property's name is outside the bounds of the string table.");

                PropertyNames.Add(reader.ReadString(StringBinaryFormat.NullTerminated));
            }

            // This data seems to have hardly any correlation to the string table.
            Data = reader.Read<PropertyData>();
        }

        public override void Write(Stream in_stream, bool in_isOverwrite = true)
        {
            var writer = new BinaryValueWriterEx(in_stream, StreamOwnership.Retain, Endianness.Little, Encoding.UTF8);

            var propertyNameCount = PropertyNames.Count;

            writer.Write(propertyNameCount);
            writer.CreateTempField<int>("stringTableLength");

            var stringTableLength = 0;

            for (int i = 0; i < propertyNameCount; i++)
            {
                var property = PropertyNames[i];

                writer.WriteStringNullTerminated(Encoding.UTF8, property);
                stringTableLength += property.Length + 1;
            }

            writer.WriteTempField("stringTableLength", stringTableLength);
            writer.Write(Data);
        }

        public override void Import(string in_path)
        {
            if (Path.GetExtension(in_path) != ".json" || !File.Exists(in_path))
                return;

            Data = JsonConvert.DeserializeObject<PropertyData>(File.ReadAllText(in_path));
        }

        public override void Export(string in_path = "")
        {
            if (string.IsNullOrEmpty(in_path))
                in_path = Path.ChangeExtension(Location, $"{Extension}.json");

            File.WriteAllText(in_path, JsonConvert.SerializeObject(Data, Formatting.Indented));
        }

        public struct PropertyData
        {
            public int UnkField1 { get; set; }
            public int Width { get; set; }
            public int UnkField2 { get; set; }
            public int Height { get; set; }
            public int UnkField3 { get; set; }
            public int UnkField4 { get; set; }
            public int UnkField5 { get; set; }
            public bool IsFullscreen { get; set; }
            public int UnkField6 { get; set; }
            public int BGMVolume { get; set; }
            public int UnkField7 { get; set; }
            public int SEVolume { get; set; }
            public int UnkField8 { get; set; }
            public bool IsOldOST { get; set; }
        }
    }
}
