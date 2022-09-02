namespace SpaceEngineers.Core.Basics
{
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Regex extensions
    /// </summary>
    public static class RegexExtensions
    {
        /// <summary>
        /// Convert capture collection to joined string
        /// </summary>
        /// <param name="captures">CaptureCollection</param>
        /// <param name="source">Source of matching</param>
        /// <returns>Joined captures</returns>
        public static string Join(this CaptureCollection captures, string source)
        {
            var sb = new StringBuilder();

            foreach (Capture capture in captures)
            {
                sb.Append(source.Substring(capture.Index, capture.Length));
            }

            return sb.ToString();
        }
    }
}