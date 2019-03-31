namespace SpaceEngineers.Core.Utilities.Extensions
{
    using System;
    using System.Reflection;

    public static partial class TypeExtensions
    {
        /// <summary>
        /// Call static method
        /// </summary>
        /// <param name="type">Contained type</param>
        /// <param name="methodName">Method name</param>
        /// <param name="args">Method args</param>
        /// <returns></returns>
        public static object CallStaticMethod(this Type type, string methodName, params object[] args)
        {
            var methodInfo = type.GetMethod(methodName,
                                            BindingFlags.Static
                                            | BindingFlags.Public
                                            | BindingFlags.NonPublic
                                            | BindingFlags.InvokeMethod);
            
            return methodInfo.Invoke(null, args);
        }
    }
}