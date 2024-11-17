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

    private static readonly string[] ExcludedAssemblies = new[]
    {
        nameof(System),
        nameof(Microsoft),
        "Windows"
    };

    private static readonly string[] RootAssemblies = new[]
    {
        "SpaceEngineers.Core.Basics",
        "SpaceEngineers.Core.AutoRegistration.Api",
        "SpaceEngineers.Core.CompositionRoot",

        "SpaceEngineers.Core.Analyzers.Api",
        "SpaceEngineers.Core.Benchmark.Api"
    };

    private static readonly Lazy<Assembly[]> AllAssembliesLoadedInCurrentAppDomain
        = new Lazy<Assembly[]>(WarmUpAppDomain, LazyThreadSafetyMode.ExecutionAndPublication);

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
            .SingleOrDefault(assembly => assembly.GetName().Name.Equals(assemblyName, StringComparison.Ordinal));
    }

    public static Assembly[] AllAssembliesFromCurrentDomain()
    {
        return AllAssembliesLoadedInCurrentAppDomain.Value;
    }

    public static Assembly[] Below(this Assembly[] allAssemblies, Assembly assembly)
    {
        var all = allAssemblies
            .Union(new[] { assembly })
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
            return Enumerable.Empty<Assembly>();
        }

        if (!all.TryGetValue(key, out var assembly))
        {
            return Enumerable.Empty<Assembly>();
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
            .GroupBy(assembly => assembly.GetName().Name)
            .SelectMany(RemoveDuplicates)
            .ToArray();
    }

    private static IEnumerable<Assembly> LoadReferences(AssemblyName assemblyName, HashSet<string> loaded)
    {
        var name = string.Intern(assemblyName.FullName);

        if (!loaded.Add(name))
        {
            return Enumerable.Empty<Assembly>();
        }

        if (assemblyName.ContentType == AssemblyContentType.WindowsRuntime)
        {
            return Enumerable.Empty<Assembly>();
        }

        var assembly = LoadByName(assemblyName);

        if (assembly == null)
        {
            return Enumerable.Empty<Assembly>();
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
            .Invoke(_ => default);
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