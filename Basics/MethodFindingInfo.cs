namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// MethodFindingInfo
    /// </summary>
    public class MethodFindingInfo
    {
        /// <summary> .ctor </summary>
        /// <param name="declaringType">Type that declare method</param>
        /// <param name="methodName">Method name</param>
        /// <param name="bindingFlags">BindingFlags</param>
        public MethodFindingInfo(Type declaringType,
                                 string methodName,
                                 BindingFlags bindingFlags)
        {
            DeclaringType = declaringType;
            MethodName = methodName;
            BindingFlags = bindingFlags;
        }

        /// <summary> Type that declare method </summary>
        public Type DeclaringType { get; }

        /// <summary> Method name </summary>
        public string MethodName { get; }

        /// <summary> BindingFlags </summary>
        public BindingFlags BindingFlags { get; }

        /// <summary> TypeArguments </summary>
        public IReadOnlyCollection<Type> TypeArguments { get; set; } = Array.Empty<Type>();

        /// <summary> ArgumentTypes </summary>
        public IReadOnlyCollection<Type> ArgumentTypes { get; set; } = Array.Empty<Type>();

        /// <summary> Find method </summary>
        /// <returns>MethodInfo</returns>
        public MethodInfo? FindMethod()
        {
            var isGenericType = TypeArguments.Any();

            return isGenericType
                       ? DeclaringType.GetMethod(MethodName, TypeArguments.Count, BindingFlags, null, CallingConventions.Any, ArgumentTypes.ToArray(), null)
                       : DeclaringType.GetMethod(MethodName, BindingFlags, null, CallingConventions.Any, ArgumentTypes.ToArray(), null);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{DeclaringType.FullName}.{MethodName} with {TypeArguments.Count} type arguments and arguments types: {string.Join(", ", ArgumentTypes.Select(t => t.Name))}";
        }
    }
}