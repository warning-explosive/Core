namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using System;
    using System.Reflection;
    using Abstractions;
    using Attributes;
    using Enumerations;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class MethodExtensionsImpl : IMethodExtensions
    {
        /// <inheritdoc />
        public object CallStaticMethod(Type type, string methodName, params object[] args)
        {
            var methodInfo = type.GetMethod(methodName,
                                            BindingFlags.Static
                                            | BindingFlags.Public
                                            | BindingFlags.NonPublic
                                            | BindingFlags.InvokeMethod);

            return ThrowIfNotFound(methodInfo).Invoke(null, args);
        }
        
        /// <inheritdoc />
        public object CallStaticGenericMethod(Type type, string methodName, Type[] genericArguments, params object[] args)
        {
            var methodInfo = type.GetMethod(methodName,
                                            BindingFlags.Static
                                            | BindingFlags.Public
                                            | BindingFlags.NonPublic
                                            | BindingFlags.InvokeMethod);
            
            return ThrowIfNotFound(methodInfo).MakeGenericMethod(genericArguments).Invoke(null, args);
        }

        private static MethodInfo ThrowIfNotFound(MethodInfo? methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            return methodInfo;
        }
    }
}