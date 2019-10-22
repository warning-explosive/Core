namespace SpaceEngineers.Core.CompositionRoot.Extensions
{
    using System;
    using Abstractions;

    /// <summary>
    /// System.Type.MethodInfo extensions
    /// </summary>
    public static class MethodExtensions
    {
        private static readonly IMethodExtensions _methodExtensions = DependencyContainer.Resolve<IMethodExtensions>();
        
        /// <summary>
        /// Call static method
        /// </summary>
        /// <param name="type">Contained type</param>
        /// <param name="methodName">Method name</param>
        /// <param name="args">Method args</param>
        /// <returns></returns>
        public static object CallStaticMethod(this Type type, string methodName, params object[] args)
        {
            return _methodExtensions.CallStaticMethod(type, methodName, args);
        }

        /// <summary>
        /// Call static generic method
        /// </summary>
        /// <param name="type">Contained type</param>
        /// <param name="methodName">Method name</param>
        /// <param name="genericArguments">Method generic arguments</param>
        /// <param name="args">Method args</param>
        /// <returns></returns>
        public static object CallStaticGenericMethod(this Type type, string methodName,Type[] genericArguments, params object[] args)
        {
            return _methodExtensions.CallStaticGenericMethod(type, methodName, genericArguments, args);
        }
    }
}