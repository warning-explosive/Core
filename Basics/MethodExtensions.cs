namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// System.Type.MethodInfo extensions
    /// </summary>
    public static class MethodExtensions
    {
        /// <summary>
        /// Call method
        /// </summary>
        /// <param name="target">Target invocation instance</param>
        /// <param name="methodName">Method name</param>
        /// <param name="args">Method args</param>
        /// <returns>Return value from called method if present or null instead</returns>
        public static object? CallMethod(this object target, string methodName, params object?[] args)
        {
            var methodInfo = target.GetType()
                                   .GetMethods(BindingFlags.Instance
                                               | BindingFlags.Public
                                               | BindingFlags.NonPublic
                                               | BindingFlags.InvokeMethod)
                                   .Single(m => FilterMethod(m, methodName, args.Length));

            return methodInfo?.Invoke(target, args);
        }

        /// <summary>
        /// Call static method
        /// </summary>
        /// <param name="type">Contained type</param>
        /// <param name="methodName">Method name</param>
        /// <param name="args">Method args</param>
        /// <returns>Return value from called method if present or null instead</returns>
        public static object? CallStaticMethod(this Type type, string methodName, params object?[] args)
        {
            var methodInfo = type.GetMethods(BindingFlags.Static
                                             | BindingFlags.Public
                                             | BindingFlags.NonPublic
                                             | BindingFlags.InvokeMethod)
                                 .Single(m => FilterMethod(m, methodName, args.Length));

            return methodInfo?.Invoke(null, args);
        }

        /// <summary>
        /// Call static generic method
        /// </summary>
        /// <param name="type">Contained type</param>
        /// <param name="methodName">Method name</param>
        /// <param name="genericArguments">Method generic arguments</param>
        /// <param name="args">Method args</param>
        /// <returns>Return value from called method if present or null instead</returns>
        public static object? CallStaticGenericMethod(this Type type, string methodName, Type[] genericArguments, params object?[] args)
        {
            var methodInfo = type.GetMethods(BindingFlags.Static
                                             | BindingFlags.Public
                                             | BindingFlags.NonPublic
                                             | BindingFlags.InvokeMethod)
                                 .Single(m => FilterMethod(m, methodName, args.Length)
                                              && FilterGenericMethod(m, genericArguments.Length));

            return methodInfo?.MakeGenericMethod(genericArguments)
                              .Invoke(null, args);
        }

        private static bool FilterMethod(MethodInfo methodInfo, string methodName, int parametersCount)
        {
            var methodParameters = methodInfo.GetParameters();

            return methodInfo.Name == methodName
                   && methodParameters.Length == parametersCount
                   && !methodInfo.IsDefined(typeof(ObsoleteAttribute));
        }

        private static bool FilterGenericMethod(MethodInfo methodInfo, int genericArgumentsCount)
        {
            return methodInfo.GetGenericArguments().Length == genericArgumentsCount;
        }
    }
}