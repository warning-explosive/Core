namespace SpaceEngineers.Core.Utilities.Extensions
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
        /// <param name="bindingFlags">BindingFlags</param>
        /// <returns></returns>
        public static string ShowProperties(this object instance, BindingFlags bindingFlags)
        {
            return string.Join("\n",
                        instance.GetType()
                                .GetProperties(bindingFlags)
                                .Select(z => $"[{z.Name}] = {z.GetValue(instance)?.ToString() ?? "null"}"));
        }
    }
}