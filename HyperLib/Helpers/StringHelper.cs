namespace HyperLib.Helpers
{
    public class StringHelper
    {
        public static string Pluralise(string in_str, int in_count)
        {
            if (in_count == 1)
                return in_str;

            if (in_str.EndsWith('y'))
            {
                return $"{in_str.Substring(0, in_str.Length - 1)}ies";
            }
            else if (in_str.EndsWith('s') || in_str.EndsWith('x') || in_str.EndsWith('z') || in_str.EndsWith("ch") || in_str.EndsWith("sh"))
            {
                return $"{in_str}es";
            }
            else
            {
                return $"{in_str}s";
            }
        }
    }
}
