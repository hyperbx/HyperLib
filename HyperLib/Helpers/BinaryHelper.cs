namespace HyperLib.Helpers
{
    public class BinaryHelper
    {
        public static byte[] HexStringToByteArray(string in_hex)
        {
            in_hex = in_hex.Replace(" ", "");

            return Enumerable.Range(0, in_hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(in_hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
