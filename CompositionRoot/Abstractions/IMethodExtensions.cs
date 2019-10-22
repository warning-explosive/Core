namespace SpaceEngineers.Core.CompositionRoot.Abstractions
{
    using System;

    internal interface IMethodExtensions : IResolvable
    {
        /// <summary>
        /// Call static method
        /// </summary>
        /// <param name="type">Contained type</param>
        /// <param name="methodName">Method name</param>
        /// <param name="args">Method args</param>
        /// <returns>Method return value</returns>
        object CallStaticMethod(Type type,
                                string methodName,
                                params object[] args);

        /// <summary>
        /// Call static generic method
        /// </summary>
        /// <param name="type">Contained type</param>
        /// <param name="methodName">Method name</param>
        /// <param name="genericArguments">Method generic arguments</param>
        /// <param name="args">Method args</param>
        /// <returns>Method return value</returns>
        object CallStaticGenericMethod(Type type,
                                       string methodName,
                                       Type[] genericArguments,
                                       params object[] args);
    }
}