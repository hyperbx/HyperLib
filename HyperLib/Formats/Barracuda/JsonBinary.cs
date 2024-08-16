using HyperLib.IO.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace HyperLib.Formats.Barracuda
{
    public class JsonBinary : FileBase
    {
        private const uint _pcSignature = 0x56554A42; // VUJB
        private const uint _xboxSignature = 0x414B4A42; // AKJB
        private const uint _version = 1;

        public bool IsPCVersion { get; set; } = false;

        public class HeaderInfo
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public EHeaderType Type { get; set; } = EHeaderType.Default;

            public uint Identifier { get; set; }
        }

        public HeaderInfo Header { get; set; } = new();

        public object Root { get; set; }

        public JsonBinary() { }

        public JsonBinary(string in_path) : base(in_path) { }

        public EHeaderType GetHeaderType(BinaryObjectReader in_reader, uint in_expectedSig)
        {
            // 0
            var sig = in_reader.ReadUInt32();

            if (sig != in_expectedSig)
            {
                // 4
                sig = in_reader.ReadUInt32();

                if (sig != in_expectedSig)
                {
                    // 8
                    sig = in_reader.ReadUInt32();

                    if (sig == in_expectedSig)
                        return EHeaderType.Identifier;
                }
                else
                {
                    return EHeaderType.Size;
                }
            }
            else
            {
                return EHeaderType.Default;
            }

            return EHeaderType.Unknown;
        }

        public override void Read(Stream in_stream)
        {
            ReadObject(new BinaryObjectReader(in_stream, StreamOwnership.Retain, Endianness.Big));
        }

        public void ReadObject(BinaryObjectReader in_reader, bool in_isVerifyHeader = true)
        {
            if (in_isVerifyHeader)
            {
                Header.Type = GetHeaderType(in_reader, _xboxSignature);

                if (Header.Type == EHeaderType.Unknown)
                {
                    Header.Type = GetHeaderType(in_reader, _pcSignature);

                    if (Header.Type != EHeaderType.Unknown)
                        IsPCVersion = true;
                }

                in_reader.Seek(0, SeekOrigin.Begin);

                switch (Header.Type)
                {
                    case EHeaderType.Size:
                        in_reader.Seek(4, SeekOrigin.Current);
                        break;

                    case EHeaderType.Identifier:
                        Header.Identifier = in_reader.ReadUInt32();
                        in_reader.Seek(4, SeekOrigin.Current);
                        break;

                    case EHeaderType.Unknown:
                        throw new NotSupportedException("Could not identify JSON binary type.");
                }
            }

            var signature = in_reader.ReadUInt32();
            var version = in_reader.ReadUInt32();
            var rootNodeType = (EJsonValueType)in_reader.ReadUInt32();

            // This file is an empty JSON file.
            if (rootNodeType == EJsonValueType.Null)
                return;

            var rootNodeCount = in_reader.ReadUInt32();

            if (rootNodeType == EJsonValueType.Object)
            {
                var rootNode = new Dictionary<string, object>();

                // Populate root node recursively.
                for (int i = 0; i < rootNodeCount; i++)
                {
                    var node = ReadKeys(in_reader);

                    rootNode.Add(node.Key, node.Value);
                }

                Root = rootNode;
            }
            else if (rootNodeType == EJsonValueType.Array)
            {
                var rootNode = new List<object>();

                // Seek backwards to allow reading node count from ReadValues method.
                in_reader.Seek(-4, SeekOrigin.Current);

                // Populate root node recursively.
                rootNode.AddRange((List<object>)ReadValues(in_reader, rootNodeType));

                Root = rootNode;
            }
            else
            {
                throw new NotSupportedException($"Type {rootNodeType} is not supported as root.");
            }
        }

        private static KeyValuePair<string, object> ReadKeys(BinaryObjectReader in_reader)
        {
            var nodeKey = in_reader.ReadString(StringBinaryFormat.PrefixedLength32);
            var nodeType = (EJsonValueType)in_reader.ReadUInt32();

            // Create key/value tree.
            return new(nodeKey, ReadValues(in_reader, nodeType));
        }

        private static object ReadValues(BinaryObjectReader in_reader, EJsonValueType in_type)
        {
            switch (in_type)
            {
                case EJsonValueType.Null:
                    return null;

                case EJsonValueType.Int32:
                    return in_reader.ReadInt32();

                case EJsonValueType.Single:
                    return in_reader.ReadSingle();

                case EJsonValueType.Boolean:
                    return in_reader.ReadByte() == 1;

                case EJsonValueType.String:
                    return in_reader.ReadString(StringBinaryFormat.PrefixedLength32);

                case EJsonValueType.Array:
                {
                    var elements = new List<object>();
                    var elementCount = in_reader.ReadUInt32();

                    for (int i = 0; i < elementCount; i++)
                    {
                        var elementType = (EJsonValueType)in_reader.ReadUInt32();

                        // Populate elements recursively.
                        elements.Add(ReadValues(in_reader, elementType));
                    }

                    return elements;
                }

                case EJsonValueType.Object:
                {
                    var nestedNodes = new Dictionary<string, object>();
                    var nestedNodeCount = in_reader.ReadUInt32();

                    for (int i = 0; i < nestedNodeCount; i++)
                    {
                        var nestedNode = ReadKeys(in_reader);

                        // Populate nested nodes recursively.
                        nestedNodes.Add(nestedNode.Key, nestedNode.Value);
                    }

                    return nestedNodes;
                }

                case EJsonValueType.Int64:
                    return in_reader.ReadInt64();

                default:
                    throw new NotImplementedException($"Type {in_type} is not implemented.");
            }
        }

        public override void Write(Stream in_stream, bool in_isOverwrite = true)
        {
            WriteBinary(new BinaryObjectWriterEx(in_stream, StreamOwnership.Retain, Endianness.Big));
        }

        public void WriteBinary(BinaryObjectWriterEx in_writer)
        {
            switch (Header.Type)
            {
                case EHeaderType.Size:
                    in_writer.CreateTempField<uint>("fileSize");
                    break;

                case EHeaderType.Identifier:
                    in_writer.CreateTempField<uint>("identifier");
                    in_writer.CreateTempField<uint>("fileSize");
                    break;

                case EHeaderType.Unknown:
                    throw new NotSupportedException("Cannot write JSON binary without known type.");
            }

            in_writer.Write(IsPCVersion ? _pcSignature : _xboxSignature);
            in_writer.Write(_version);

            if (Root != null)
            {
                var rootNodeType = Root.GetType();

                /* Convert JSON objects to CLR types, in
                   case the user set the root node as such */
                if (rootNodeType == typeof(JObject))
                {
                    Root = ((JObject)Root).ToObject<Dictionary<string, object>>()!;
                }
                else if (rootNodeType == typeof(JArray))
                {
                    Root = ((JArray)Root).ToObject<List<object>>()!;
                }

                rootNodeType = Root.GetType();

                if (rootNodeType == typeof(Dictionary<string, object>))
                {
                    var root = (Dictionary<string, object>)Root;

                    in_writer.Write(EJsonValueType.Object);
                    in_writer.Write(root.Count);

                    // Write nodes recursively.
                    foreach (var node in root)
                        WriteKeys(in_writer, node.Key, node.Value);
                }
                else if (rootNodeType == typeof(List<object>))
                {
                    in_writer.Write(EJsonValueType.Array);

                    // Write values recursively.
                    WriteValues(in_writer, JArray.FromObject(Root));
                }
                else
                {
                    throw new NotSupportedException($"Type {rootNodeType.Name} is not supported as root.");
                }
            }
            else
            {
                in_writer.Write(0);
            }

            switch (Header.Type)
            {
                case EHeaderType.Size:
                    in_writer.WriteTempField("fileSize", (uint)in_writer.Position - 4);
                    break;

                case EHeaderType.Identifier:
                    in_writer.WriteTempField("identifier", Header.Identifier);
                    in_writer.WriteTempField("fileSize", (uint)in_writer.Position - 8);
                    break;
            }
        }

        private static EJsonValueType TransformCLRTypeToJsonType(Type? in_type)
        {
            if (in_type == typeof(int))
            {
                return EJsonValueType.Int32;
            }
            else if (in_type == typeof(float))
            {
                return EJsonValueType.Single;
            }
            else if (in_type == typeof(bool))
            {
                return EJsonValueType.Boolean;
            }
            else if (in_type == typeof(string))
            {
                return EJsonValueType.String;
            }
            else if (in_type == typeof(JArray))
            {
                return EJsonValueType.Array;
            }
            else if (in_type == typeof(JObject))
            {
                return EJsonValueType.Object;
            }
            else if (in_type == typeof(long))
            {
                return EJsonValueType.Int64;
            }

            return EJsonValueType.Null;
        }

        private static void WriteKeys(BinaryObjectWriter in_writer, string in_name, object in_value)
        {
            in_writer.WriteStringPrefixedLength32(Encoding.UTF8, in_name);
            in_writer.WriteUInt32((uint)TransformCLRTypeToJsonType(in_value.GetType()));

            WriteValues(in_writer, JToken.FromObject(in_value));
        }

        private static void WriteValues(BinaryObjectWriter in_writer, JToken in_token)
        {
            switch (in_token.Type)
            {
                case JTokenType.Null:
                    break;

                case JTokenType.Integer:
                {
                    if (in_token.GetValue()?.GetType() == typeof(long))
                    {
                        in_writer.WriteInt64((long)in_token);
                    }
                    else
                    {
                        in_writer.WriteInt32((int)in_token);
                    }

                    break;
                }

                case JTokenType.Float:
                    in_writer.WriteSingle((float)in_token);
                    break;

                case JTokenType.Boolean:
                    in_writer.WriteByte((byte)(((int)in_token > 0) ? 1 : 0));
                    break;

                case JTokenType.String:
                    in_writer.WriteStringPrefixedLength32(Encoding.UTF8, (string)in_token!);
                    break;

                case JTokenType.Array:
                {
                    var array = (JArray)in_token;

                    in_writer.Write(array.Count);

                    foreach (var item in array)
                    {
                        in_writer.Write(TransformCLRTypeToJsonType(item.GetValue()?.GetType()));
                        WriteValues(in_writer, item);
                    }

                    break;
                }

                case JTokenType.Object:
                {
                    var jObject = (JObject)in_token;

                    in_writer.Write(jObject.Count);

                    foreach (var property in jObject.Properties())
                    {
                        in_writer.WriteStringPrefixedLength32(Encoding.UTF8, property.Name);
                        in_writer.Write(TransformCLRTypeToJsonType(property.Value.GetValue()?.GetType()));

                        WriteValues(in_writer, property.Value);
                    }

                    break;
                }

                case JTokenType.Property:
                    in_writer.WriteStringPrefixedLength32(Encoding.UTF8, ((JProperty)in_token).Name);
                    WriteValues(in_writer, in_token);
                    break;

                default:
                    throw new NotSupportedException($"Type {in_token.Type} is not supported.");
            }
        }

        public override void Import(string in_path)
        {
            if (!File.Exists(in_path))
                return;

            var json = File.ReadAllText(in_path);
            var root = JsonConvert.DeserializeObject(json)!;

            if (root is JObject)
            {
                Root = JsonConvert.DeserializeObject<Dictionary<string, object>>(json)!;
            }
            else if (root is JArray)
            {
                Root = JsonConvert.DeserializeObject<List<object>>(json)!;
            }
            else if (root != null)
            {
                throw new NotSupportedException("Unsupported root node.");
            }

            var metadata = Path.ChangeExtension(in_path, ".meta");

            if (!File.Exists(metadata))
                return;

            Header = JsonConvert.DeserializeObject<HeaderInfo>(File.ReadAllText(metadata))!;
        }

        public override void Export(string in_path = "")
        {
            if (string.IsNullOrEmpty(in_path))
                in_path = Path.ChangeExtension(Location, ".json");

            File.WriteAllText(in_path, JsonConvert.SerializeObject(Root, Formatting.Indented));

            if (Header.Type == EHeaderType.Default)
                return;

            File.WriteAllText(Path.ChangeExtension(in_path, ".meta"), JsonConvert.SerializeObject(Header, Formatting.Indented));
        }
    }

    public enum EJsonValueType
    {
        Null,
        Int32,
        Single,
        Boolean,
        String,
        Array,
        Object,
        Int64
    }

    public enum EHeaderType
    {
        Unknown = -1,

        /// <summary>
        /// The binary starts with the signature at offset zero.
        /// </summary>
        Default,

        /// <summary>
        /// The binary starts with the file size (minus 4) at offset zero.
        /// </summary>
        Size,

        /// <summary>
        /// The binary starts with a 32-bit integer and the file size (minus 8) at offset zero.
        /// </summary>
        Identifier
    }
}
