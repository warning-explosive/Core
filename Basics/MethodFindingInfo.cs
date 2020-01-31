namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Exceptions;

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
            var isGenericMethod = TypeArguments.Any();

            return isGenericMethod
                       ? GetGenericMethod()
                       : DeclaringType.GetMethod(MethodName, BindingFlags, null, ArgumentTypes.ToArray(), null);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{DeclaringType.FullName}.{MethodName} with {TypeArguments.Count} type arguments and arguments types: {string.Join(", ", ArgumentTypes.Select(t => t.Name))}";
        }

        private MethodInfo? GetGenericMethod()
        {
            var methods = DeclaringType.GetMethods(BindingFlags)
                                       .Where(m => m.Name == MethodName
                                                && m.IsGenericMethod
                                                && ValidateParameters(ArgumentTypes.ToArray(), m.GetParameters().Select(z => z.ParameterType).ToArray())
                                                && ValidateParameters(TypeArguments.ToArray(), m.GetGenericArguments()))
                                       .ToArray();

            // todo: extension for single extraction
            if (methods.Length < 1)
            {
                throw new NotFoundException(MethodName);
            }

            if (methods.Length > 1)
            {
                string Generics(MethodInfo m) => string.Join(", ", m.GetGenericArguments().Select(g => g.Name));
                string Show(MethodInfo m) => DeclaringType.FullName + "." + m.Name + "[" + Generics(m) + "]";
                throw new AmbiguousMatchException(string.Join(", ", methods.Select(Show)));
            }

            return methods.Single();
        }

        private static bool ValidateParameters(Type[] actual, Type[] expected)
        {
            if (actual.Length != expected.Length)
            {
                return false;
            }

            return expected.Select((exp, i) => new { Type = exp, i })
                           .Join(actual.Select((act, i) => new { Type = act, i }),
                                 exp => exp.i,
                                 act => act.i,
                                 (exp, act) => new { Exp = exp.Type, Act = act.Type })
                           .All(pair => throw new NotImplementedException("Check type and constraints"));
        }
    }
}