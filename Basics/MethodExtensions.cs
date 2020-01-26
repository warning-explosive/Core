namespace SpaceEngineers.Core.Basics
{
    using System;

    /// <summary>
    /// System.Type.MethodInfo extensions
    /// </summary>
    public static class MethodExtensions
    {
        /// <summary>
        /// Call method by reflection
        /// </summary>
        /// <param name="declaringType">Type that declare the method</param>
        /// <param name="methodName">Method name</param>
        /// <returns>MethodExecutionInfo</returns>
        public static MethodExecutionInfo CallMethod(this Type declaringType, string methodName)
        {
            return new MethodExecutionInfo(declaringType, methodName);
        }

        /// <summary>
        /// Call method by reflection
        /// </summary>
        /// <param name="target">Target instance of method call</param>
        /// <param name="methodName">Method name</param>
        /// <returns>MethodExecutionInfo</returns>
        public static MethodExecutionInfo CallMethod(this object target, string methodName)
        {
            return new MethodExecutionInfo(target.GetType(), methodName).ForInstance(target);
        }
    }
}