using Newtonsoft.Json;

namespace HyperLib.Formats.Barracuda
{
    public class TimedEvent : FileBase
    {
        public bool IsPCVersion { get; set; } = false;

        public List<TimedEventWrapper> Events { get; set; } = [];

        public TimedEvent() { }

        public TimedEvent(string in_path) : base(in_path) { }

        public override void Read(Stream in_stream)
        {
            var reader = new BinaryObjectReader(in_stream, StreamOwnership.Retain, Endianness.Big);

            var count = reader.ReadUInt32();

            for (int i = 0; i < count; i++)
            {
                var wrapper = new TimedEventWrapper();
                wrapper.Read(reader);

                Events.Add(wrapper);
            }
        }

        public override void Write(Stream in_stream, bool in_isOverwrite = true)
        {
            var writer = new BinaryObjectWriterEx(in_stream, StreamOwnership.Retain, Endianness.Big);

            writer.Write(Events.Count);

            foreach (var @event in Events)
                @event.Write(writer, IsPCVersion);
        }

        public override void Import(string in_path)
        {
            if (!File.Exists(in_path))
                return;

            Events = JsonConvert.DeserializeObject<List<TimedEventWrapper>>(File.ReadAllText(in_path))!;
        }

        public override void Export(string in_path = "")
        {
            if (string.IsNullOrEmpty(in_path))
                in_path = Path.ChangeExtension(Location, ".json");

            File.WriteAllText(in_path, JsonConvert.SerializeObject(Events, Formatting.Indented));
        }
    }

    public class TimedEventWrapper
    {
        public string Name { get; set; }

        public float UserData1 { get; set; }

        public uint UserData2 { get; set; }

        public object Root { get; set; }

        public void Read(BinaryObjectReader in_reader)
        {
            UserData1 = in_reader.ReadSingle();
            Name = in_reader.ReadString(StringBinaryFormat.NullTerminated);
            UserData2 = in_reader.ReadUInt32();

            var ajb = new JsonBinary();
            ajb.ReadObject(in_reader, false);

            Root = ajb.Root;
        }

        public void Write(BinaryObjectWriterEx in_writer, bool in_isPCVersion)
        {
            in_writer.WriteSingle(UserData1);
            in_writer.WriteString(StringBinaryFormat.NullTerminated, Name);
            in_writer.WriteUInt32(UserData2);

            var ajb = new JsonBinary()
            {
                IsPCVersion = in_isPCVersion,
                Root = Root
            };

            ajb.WriteBinary(in_writer);
        }
    }
}
