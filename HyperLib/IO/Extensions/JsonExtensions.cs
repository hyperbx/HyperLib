using Newtonsoft.Json.Linq;

namespace HyperLib.IO.Extensions
{
    public static class JsonExtensions
    {
        public static object GetValue(this JToken in_token)
        {
#pragma warning disable CS8603 // Possible null reference return.

            if (in_token.Type == JTokenType.Integer)
            {
                try
                {
                    return in_token.ToObject<int>();
                }
                catch
                {
                    return in_token.ToObject<long>();
                }
            }

            return in_token.Type switch
            {
                JTokenType.Object   => (JObject)in_token,
                JTokenType.Array    => (JArray)in_token,
                JTokenType.Property => GetValue(((JProperty)in_token).Value),
                JTokenType.Float    => ((JValue)in_token).ToObject<float>(),
                JTokenType.String   => ((JValue)in_token).ToObject<string>(),
                JTokenType.Boolean  => ((JValue)in_token).ToObject<bool>(),
                JTokenType.Null     => null,
                JTokenType.Date     => ((JValue)in_token).ToObject<DateTime>(),
                JTokenType.Bytes    => ((JValue)in_token).ToObject<byte[]>(),
                _                   => throw new NotSupportedException($"Unsupported type: {in_token.Type}"),
            };

#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
