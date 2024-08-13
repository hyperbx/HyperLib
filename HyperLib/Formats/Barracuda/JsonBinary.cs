using HyperLib.IO.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HyperLib.Formats.Barracuda
{
    public class JsonBinary : FileBase
    {
        private const uint _pcSignature = 0x56554A42; // VUJB
        private const uint _xboxSignature = 0x414B4A42; // AKJB
        private const uint _version = 1;

        public bool IsPCVersion { get; set; } = false;

        public object Root { get; set; }

        public JsonBinary() { }

        public JsonBinary(string in_path) : base(in_path) { }

        public override void Read(Stream in_stream)
        {
            var reader = new BinaryObjectReader(in_stream, StreamOwnership.Retain, Endianness.Big);

            // Type 4 files have a file size field at the start.
            if (ArchiveFile.GetTypeFromName(Location) == 4)
                reader.Seek(4, SeekOrigin.Begin);

            if (!reader.IsSignatureValid(_xboxSignature, false))
            {
                reader.Seek(0, SeekOrigin.Begin);

                if (reader.IsSignatureValid(_pcSignature, false))
                {
                    IsPCVersion = true;
                }
                else
                {
                    throw new NotSupportedException("Could not identify JSON binary type.");
                }
            }

            var version = reader.ReadUInt32();
            var rootNodeType = (EJsonValueType)reader.ReadUInt32();

            // This file is an empty JSON file.
            if (rootNodeType == EJsonValueType.Null)
                return;

            var rootNodeCount = reader.ReadUInt32();

            if (rootNodeType == EJsonValueType.Object)
            {
                var rootNode = new Dictionary<string, object>();

                // Populate root node recursively.
                for (int i = 0; i < rootNodeCount; i++)
                {
                    var node = ReadKeys(reader);

                    rootNode.Add(node.Key, node.Value);
                }

                Root = rootNode;
            }
            else if (rootNodeType == EJsonValueType.Array)
            {
                var rootNode = new List<object>();

                // Seek backwards to allow reading node count from ReadValues method.
                reader.Seek(-4, SeekOrigin.Current);

                // Populate root node recursively.
                rootNode.AddRange((List<object>)ReadValues(reader, rootNodeType));

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
            var writer = new BinaryObjectWriterEx(in_stream, StreamOwnership.Retain, Endianness.Big);

            // Type 4 files have a file size field at the start.
            if (ArchiveFile.GetTypeFromName(Location) == 4)
                writer.CreateTempField<uint>("fileSize");

            writer.Write(_xboxSignature);
            writer.Write(_version);

            var rootNodeType = Root.GetType();

            if (rootNodeType == typeof(Dictionary<string, object>))
            {
                var root = (Dictionary<string, object>)Root;

                writer.Write(EJsonValueType.Object);
                writer.Write(root.Count);

                // Write nodes recursively.
                foreach (var node in root)
                    WriteKeys(writer, node.Key, node.Value);
            }
            else if (rootNodeType == typeof(List<object>))
            {
                writer.Write(EJsonValueType.Array);

                // Write values recursively.
                WriteValues(writer, JArray.FromObject(Root));
            }
            else
            {
                throw new NotSupportedException($"Type {rootNodeType.Name} is not supported as root.");
            }

            // Write size field for type 4 files.
            if (ArchiveFile.GetTypeFromName(Location) == 4)
                writer.WriteTempField("fileSize", (uint)writer.Position - 4);
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
            else
            {
                throw new NotSupportedException("Unsupported root node.");
            }
        }

        public override void Export(string in_path = "")
        {
            if (string.IsNullOrEmpty(in_path))
                in_path = Path.ChangeExtension(Location, ".json");

            File.WriteAllText(in_path, JsonConvert.SerializeObject(Root, Formatting.Indented));
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
}
