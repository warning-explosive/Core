namespace SpaceEngineers.Core.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Basics;
using Registration;

public class DependencyContainerOptions
{
    private readonly List<IDependencyInjectionRegistration> _manualRegistrations;
    private readonly HashSet<Assembly> _assemblies;
    private readonly HashSet<Type> _types;

    public DependencyContainerOptions()
    {
        _manualRegistrations = new List<IDependencyInjectionRegistration>();
        _assemblies = new HashSet<Assembly>
        {
            AssemblyExtensions.FindRequiredAssembly(AssemblyExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(DependencyInjection)))
        };
        _types = new HashSet<Type>();
    }

    public IReadOnlyCollection<IDependencyInjectionRegistration> ManualRegistrations => _manualRegistrations;

    public IReadOnlyCollection<Assembly> Assemblies => _assemblies;

    public IReadOnlyCollection<Type> Types => _types;

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(
            CombineHashCode(ManualRegistrations),
            CombineHashCode(Assemblies),
            CombineHashCode(Types));

        static int CombineHashCode<T>(IReadOnlyCollection<T> source)
        {
            return source.Any()
                ? source.Aggregate(int.MaxValue, HashCode.Combine)
                : int.MaxValue;
        }
    }

    // TODO: rename method "WithManualRegistrations"
    public DependencyContainerOptions WithManualRegistrations(params IDependencyInjectionRegistration[] manualRegistrations)
    {
        _manualRegistrations.AddRange(manualRegistrations);

        return this;
    }

    public DependencyContainerOptions WithPluginAssemblies(params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            if (!_assemblies.Add(assembly))
            {
                throw new InvalidOperationException($"Assembly '{assembly}' already added as plugin assembly");
            }
        }

        return this;
    }

    public DependencyContainerOptions WithPluginTypes(params Type[] types)
    {
        foreach (var type in types)
        {
            if (!_types.Add(type))
            {
                throw new InvalidOperationException($"Type '{type}' already added as plugin type");
            }
        }

        return this;
    }
}