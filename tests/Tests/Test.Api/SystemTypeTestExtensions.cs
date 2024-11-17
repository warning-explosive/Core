namespace SpaceEngineers.Core.Test.Api;

using System;
using System.Collections.Generic;
using System.Linq;

public static class SystemTypeTestExtensions
{
    public static IEnumerable<Type> ShowTypes(
        this IEnumerable<Type> source,
        string tag,
        Action<string> show)
    {
        show(tag);
        return source.Select(type =>
        {
            show(type.FullName ?? "null");
            return type;
        });
    }
}