namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Abstractions;
    using Attributes;
    using Enumerations;

    /// <inheritdoc />
    [Lifestyle(EnLifestyle.Singleton)]
    internal class MethodExtensionsImpl : IMethodExtensions
    {
        /// <inheritdoc />
        public object CallMethod(object target, string methodName, params object?[] args)
        {
            var methodInfo = target.GetType()
                                   .GetMethods(BindingFlags.Instance
                                               | BindingFlags.Public
                                               | BindingFlags.NonPublic
                                               | BindingFlags.InvokeMethod)
                                   .Single(m => FilterMethod(m, methodName, args.Length));

            return methodInfo.ThrowIfNull().Invoke(target, args);
        }

        /// <inheritdoc />
        public object CallStaticMethod(Type type, string methodName, params object?[] args)
        {
            var methodInfo = type.GetMethods(BindingFlags.Static
                                             | BindingFlags.Public
                                             | BindingFlags.NonPublic
                                             | BindingFlags.InvokeMethod)
                                 .Single(m => FilterMethod(m, methodName, args.Length));

            return methodInfo.ThrowIfNull().Invoke(null, args);
        }
        
        /// <inheritdoc />
        public object CallStaticGenericMethod(Type type, string methodName, Type[] genericArguments, params object?[] args)
        {
            var methodInfo = type.GetMethods(BindingFlags.Static
                                             | BindingFlags.Public
                                             | BindingFlags.NonPublic
                                             | BindingFlags.InvokeMethod)
                                 .Single(m => FilterMethod(m, methodName, args.Length)
                                              && FilterGenericMethod(m, genericArguments.Length));
            
            return methodInfo.ThrowIfNull()
                             .MakeGenericMethod(genericArguments)
                             .Invoke(null, args);
        }

        private bool FilterMethod(MethodInfo methodInfo, string methodName, int parametersCount)
        {
            var methodParameters = methodInfo.GetParameters();
            
            return methodInfo.Name == methodName
                   && methodParameters.Length == parametersCount
                   && !methodInfo.IsDefined(typeof(ObsoleteAttribute));
        }

        private bool FilterGenericMethod(MethodInfo methodInfo, int genericArgumentsCount)
        {
            return methodInfo.GetGenericArguments().Length == genericArgumentsCount;
        }
    }
}