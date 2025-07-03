namespace SpaceEngineers.Core.Basics.Reflection;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class MethodFinder(
    Type declaringType,
    string methodName,
    BindingFlags bindingFlags)
{
    private static readonly ConcurrentDictionary<string, MethodInfo?> Cache = new(StringComparer.OrdinalIgnoreCase);

    public Type DeclaringType { get; } = declaringType;

    public string MethodName { get; } = methodName;

    public BindingFlags BindingFlags { get; } = bindingFlags;

    public IReadOnlyCollection<Type> TypeArguments { get; set; } = [];

    public IReadOnlyCollection<Type> ArgumentTypes { get; set; } = [];

    public override string ToString()
    {
        var properties = new Dictionary<string, string>
        {
            [nameof(DeclaringType)] = DeclaringType.FullName!,
            [nameof(MethodName)] = MethodName,
            [nameof(BindingFlags)] = BindingFlags.ToString("G"),
            [nameof(TypeArguments)] = TypeArguments.Select(type => type.FullName!).ToString(string.Empty),
            [nameof(ArgumentTypes)] = ArgumentTypes.Select(type => type.FullName!).ToString(string.Empty)
        };

        return properties.ToString(string.Empty);
    }

    public MethodInfo? FindMethod()
    {
        var key = string.Intern(ToString());

        return Cache.GetOrAdd(key, static (_, methodFinder) => Find(methodFinder), this);
    }

    private static MethodInfo? Find(MethodFinder methodFinder)
    {
        var isGenericMethod = methodFinder.TypeArguments.Any();

        return isGenericMethod
            ? FindGenericMethod(methodFinder)
            : FindNonGenericMethod(methodFinder);
    }

    private static MethodInfo? FindNonGenericMethod(MethodFinder methodFinder)
    {
        return methodFinder.DeclaringType.GetMethod(
            methodFinder.MethodName,
            methodFinder.BindingFlags,
            null,
            methodFinder.ArgumentTypes.ToArray(),
            null);
    }

    private static MethodInfo? FindGenericMethod(MethodFinder methodFinder)
    {
        var methods = methodFinder.DeclaringType
            .GetMethods(methodFinder.BindingFlags)
            .Where(methodInfo => methodInfo.Name == methodFinder.MethodName
                                 && methodInfo.IsGenericMethod
                                 && ValidateParameters(methodFinder.TypeArguments, methodInfo.GetGenericArguments())
                                 && ValidateParameters(methodFinder.ArgumentTypes, GetArgumentTypes(methodInfo)))
            .ToArray();

        IReadOnlyCollection<Type> GetArgumentTypes(MethodInfo methodInfo)
        {
            return methodInfo
                .MakeGenericMethod(methodFinder.TypeArguments.ToArray())
                .GetParameters()
                .Select(z => z.ParameterType)
                .ToArray();
        }

        return methods.Single(Amb, methodFinder);

        static string Amb(MethodFinder methodFinder, IEnumerable<MethodInfo> source)
        {
            string Generics(MethodInfo m) => m.GetGenericArguments().Select(g => g.Name).ToString(", ");
            string Show(MethodInfo m) => methodFinder.DeclaringType.FullName + "." + m.Name + "[" + Generics(m) + "]";
            return source.Select(Show).ToString(", ");
        }
    }

    private static bool ValidateParameters(IReadOnlyCollection<Type> actual, IReadOnlyCollection<Type> expected)
    {
        if (actual.Count != expected.Count)
        {
            return false;
        }

        return expected.Select((exp, i) => new { Type = exp, i })
            .Join(
                actual.Select((act, i) => new { Type = act, i }),
                exp => exp.i,
                act => act.i,
                (exp, act) => new { Exp = exp.Type, Act = act.Type })
            .All(pair => pair.Act.FitsForTypeArgument(pair.Exp));
    }
}