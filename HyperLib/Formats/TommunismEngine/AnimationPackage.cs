namespace HyperLib.Formats.TommunismEngine
{
    public class AnimationPackage : FileBase
    {
        public override string Extension => ".am";

        public FormatVersion Version = FormatVersion.Unknown;

        public AnimationPackage() { }

        public AnimationPackage(string in_path) : base(in_path) { }

        public override void Read(Stream in_stream)
        {
            var reader = new BinaryValueReader(in_stream, StreamOwnership.Retain, Endianness.Little, Encoding.UTF8);

            Version = (FormatVersion)reader.ReadInt32();

            if (!Enum.IsDefined(Version))
            {
                Version = FormatVersion.Unknown;

                reader.Seek(-4, SeekOrigin.Begin);
            }

            var stringTableLength = reader.ReadUInt16();
            var animationCount = reader.ReadUInt16();
            var framerate = reader.ReadSingle();

            // TODO
        }

        public enum FormatVersion : int
        {
            Unknown = -1,
            F100 = 0x46313030,
            F101 = 0x46313031
        }
    }
}
