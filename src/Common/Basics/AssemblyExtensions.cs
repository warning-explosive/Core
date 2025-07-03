namespace SpaceEngineers.Core.Basics;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using EqualityComparers;

public static class AssemblyExtensions
{
    private const string Dot = ".";

    private const string Duplicate = "xunit.runner.visualstudio.dotnetcore.testadapter";

    private static readonly Lazy<Assembly[]> AllAssembliesLoadedInCurrentAppDomain
        = new(WarmUpAppDomain, LazyThreadSafetyMode.ExecutionAndPublication);

    public static string GetAssemblyVersion(this Assembly assembly)
    {
        return assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version
               ?? assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version
               ?? assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
               ?? "1.0.0.0";
    }

    public static string BuildName(params string[] nameParts)
    {
        return nameParts.ToString(Dot);
    }

    public static Assembly FindRequiredAssembly(string assemblyName)
    {
        return FindAssembly(assemblyName)
               ?? throw new InvalidOperationException($"Assembly {assemblyName} should be found in current {nameof(AppDomain)}");
    }

    public static Assembly? FindAssembly(string assemblyName)
    {
        return AllAssembliesFromCurrentDomain()
            .Where(assembly => assembly.GetName().Name!.Equals(assemblyName, StringComparison.Ordinal))
            .SingleOrDefault(Amb, assemblyName);

        static string Amb(string assemblyName, IEnumerable<Assembly> assemblies)
        {
            return $"AppDomain has more than one loaded assembly {assemblyName}";
        }
    }

    public static Assembly[] AllAssembliesFromCurrentDomain()
    {
        return AllAssembliesLoadedInCurrentAppDomain.Value;
    }

    public static Assembly[] Below(this Assembly[] allAssemblies, Assembly assembly)
    {
        var all = allAssemblies
            .Union([assembly])
            .Distinct(new AssemblyByNameEqualityComparer())
            .ToDictionary(a => string.Intern(a.GetName().FullName));

        var visited = new HashSet<string>();

        return BelowReference(assembly.GetName(), all, visited).ToArray();
    }

    private static IEnumerable<Assembly> BelowReference(
        AssemblyName assemblyName,
        IReadOnlyDictionary<string, Assembly> all,
        HashSet<string> visited)
    {
        var key = string.Intern(assemblyName.FullName);

        if (!visited.Add(key))
        {
            return [];
        }

        if (!all.TryGetValue(key, out var assembly))
        {
            return [];
        }

        return new[] { assembly }
            .Concat(assembly
                .GetReferencedAssemblies()
                .SelectMany(name => BelowReference(name, all, visited)));
    }

    private static Assembly[] WarmUpAppDomain()
    {
        var loaded = new HashSet<string>();

        _ = Directory
            .GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly)
            .Select(AssemblyName.GetAssemblyName)
            .SelectMany(name => LoadReferences(name, loaded))
            .ToList();

        return AppDomain.CurrentDomain
            .GetAssemblies()
            .GroupBy(assembly => assembly.GetName().Name!)
            .SelectMany(RemoveDuplicates)
            .ToArray();
    }

    private static IEnumerable<Assembly> LoadReferences(AssemblyName assemblyName, HashSet<string> loaded)
    {
        var name = string.Intern(assemblyName.FullName);

        if (!loaded.Add(name))
        {
            return [];
        }

        if (assemblyName.ContentType == AssemblyContentType.WindowsRuntime)
        {
            return [];
        }

        var assembly = LoadByName(assemblyName);

        if (assembly == null)
        {
            return [];
        }

        return new[] { assembly }
            .Concat(assembly
                .GetReferencedAssemblies()
                .SelectMany(referenceName => LoadReferences(referenceName, loaded)));
    }

    private static Assembly? LoadByName(AssemblyName assemblyName)
    {
        return ExecutionExtensions
            .Try<AssemblyName, Assembly?>(AppDomain.CurrentDomain.Load, assemblyName)
            .Catch<FileNotFoundException>()
            .Invoke(_ => null);
    }

    private static IEnumerable<Assembly> RemoveDuplicates(IGrouping<string, Assembly> grp)
    {
        if (grp.Key.Equals(Duplicate, StringComparison.OrdinalIgnoreCase))
        {
            yield return grp.First();
            yield break;
        }

        foreach (var item in grp)
        {
            yield return item;
        }
    }
}