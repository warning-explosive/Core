namespace SpaceEngineers.Core.Basics
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Method finder
    /// </summary>
    public class MethodFinder
    {
        private static readonly ConcurrentDictionary<string, MethodInfo?> Cache
            = new ConcurrentDictionary<string, MethodInfo?>(StringComparer.OrdinalIgnoreCase);

        /// <summary> .ctor </summary>
        /// <param name="declaringType">Type that declare method</param>
        /// <param name="methodName">Method name</param>
        /// <param name="bindingFlags">BindingFlags</param>
        public MethodFinder(Type declaringType,
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

        /// <inheritdoc />
        public override string ToString()
        {
            var properties = new Dictionary<string, string>
            {
                [nameof(DeclaringType)] = DeclaringType.FullName,
                [nameof(MethodName)] = MethodName,
                [nameof(BindingFlags)] = BindingFlags.ToString("G"),
                [nameof(TypeArguments)] = string.Join(string.Empty, TypeArguments.Select(type => type.FullName)),
                [nameof(ArgumentTypes)] = string.Join(string.Empty, ArgumentTypes.Select(type => type.FullName))
            };

            return string.Join(string.Empty, properties);
        }

        /// <summary> Find method </summary>
        /// <returns>MethodInfo</returns>
        public MethodInfo? FindMethod()
        {
            var key = string.Intern(ToString());

            return Cache.GetOrAdd(key, _ => Find());
        }

        private MethodInfo? Find()
        {
            var isGenericMethod = TypeArguments.Any();

            return isGenericMethod
                ? FindGenericMethod()
                : FindNonGenericMethod();
        }

        private MethodInfo? FindNonGenericMethod()
        {
            return DeclaringType.GetMethod(MethodName, BindingFlags, null, ArgumentTypes.ToArray(), null);
        }

        private MethodInfo? FindGenericMethod()
        {
            var methods = DeclaringType
                .GetMethods(BindingFlags)
                .Where(methodInfo => methodInfo.Name == MethodName
                            && methodInfo.IsGenericMethod
                            && ValidateParameters(TypeArguments, methodInfo.GetGenericArguments())
                            && ValidateParameters(ArgumentTypes, GetArgumentTypes(methodInfo)))
                .ToArray();

            IReadOnlyCollection<Type> GetArgumentTypes(MethodInfo methodInfo)
            {
                return methodInfo
                    .MakeGenericMethod(TypeArguments.ToArray())
                    .GetParameters()
                    .Select(z => z.ParameterType)
                    .ToArray();
            }

            string Amb(IEnumerable<MethodInfo> source)
            {
                string Generics(MethodInfo m) => string.Join(", ", m.GetGenericArguments().Select(g => g.Name));
                string Show(MethodInfo m) => DeclaringType.FullName + "." + m.Name + "[" + Generics(m) + "]";
                return string.Join(", ", methods.Select(Show));
            }

            return methods.InformativeSingle(Amb);
        }

        private static bool ValidateParameters(IReadOnlyCollection<Type> actual, IReadOnlyCollection<Type> expected)
        {
            if (actual.Count != expected.Count)
            {
                return false;
            }

            return expected.Select((exp, i) => new { Type = exp, i })
                           .Join(actual.Select((act, i) => new { Type = act, i }),
                                 exp => exp.i,
                                 act => act.i,
                                 (exp, act) => new { Exp = exp.Type, Act = act.Type })
                           .All(pair => pair.Act.FitsForTypeArgument(pair.Exp));
        }
    }
}