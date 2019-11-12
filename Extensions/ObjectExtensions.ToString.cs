namespace SpaceEngineers.Core.Extensions
{
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Object ToString extension methods
    /// </summary>
    public static partial class ObjectExtensions
    {
        /// <summary>
        /// Show properties of object
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="flags">BindingFlags</param>
        /// <param name="blackList">Black list of properties</param>
        /// <returns></returns>
        public static string ShowProperties(this object instance, BindingFlags flags, params string[] blackList)
        {
            return string.Join("\n",
                        instance.GetType()
                                .GetProperties(flags)
                                .Where(z => !blackList.Contains(z.Name))
                                .Select(z => $"[{z.Name}] = {z.GetValue(instance)?.ToString() ?? "null"}"));
        }
    }
}