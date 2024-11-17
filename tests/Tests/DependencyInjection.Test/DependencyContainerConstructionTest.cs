namespace SpaceEngineers.Core.DependencyInjection.Test;

using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using AssemblyExtensions = Basics.AssemblyExtensions;
using CompositionInfo;
using Dependencies;
using SpaceEngineers.Core.Test.Api;
using SpaceEngineers.Core.Test.Api.ClassFixtures;
using Xunit.Abstractions;
using Xunit;

public class DependencyContainerConstructionTest(ITestOutputHelper output, TestFixture fixture)
    : TestBase(output, fixture)
{
    [Fact]
    public void BuildEmptyDependencyContainerTest()
    {
        var options = new DependencyContainerOptions();

        var dependencyContainer = Fixture.DependencyContainer(options);

        var compositionInfo = dependencyContainer
            .Resolve<ICompositionInfoExtractor>()
            .GetCompositionInfo();

        var visualization = dependencyContainer
            .Resolve<ICompositionInfoInterpreter<string>>()
            .Visualize(compositionInfo);

        Output.WriteLine($"Total: {compositionInfo.Count}{Environment.NewLine}");
        Output.WriteLine(visualization);
    }

    [Fact]
    internal void BuildBoundedDependencyContainerTest()
    {
        var assemblies = new[]
        {
            AssemblyExtensions.FindRequiredAssembly(AssemblyExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CliArgumentsParser))),
            AssemblyExtensions.FindRequiredAssembly(AssemblyExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(DependencyInjection), nameof(Test)))
        };

        var pluginTypes = new[]
        {
            typeof(IProgress<ExternalResolvable>),
            typeof(TestAdditionalType),
            typeof(IConfigurationProvider),
            typeof(ConfigurationProvider),
        };

        var options = new DependencyContainerOptions()
            .WithPluginAssemblies(assemblies)
            .WithPluginTypes(pluginTypes);

        var dependencyContainer = Fixture.DependencyContainer(options);

        var compositionInfo = dependencyContainer
            .Resolve<ICompositionInfoExtractor>()
            .GetCompositionInfo();

        var visualization = dependencyContainer
            .Resolve<ICompositionInfoInterpreter<string>>()
            .Visualize(compositionInfo);

        Output.WriteLine($"Total: {compositionInfo.Count}{Environment.NewLine}");
        Output.WriteLine(visualization);

        var allowedAssemblies = new[]
        {
            AssemblyExtensions.FindRequiredAssembly(AssemblyExtensions.BuildName(nameof(SimpleInjector))),
            AssemblyExtensions.FindRequiredAssembly(AssemblyExtensions.BuildName(nameof(Newtonsoft), nameof(Newtonsoft.Json))),
            AssemblyExtensions.FindRequiredAssembly(AssemblyExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Basics))),
            AssemblyExtensions.FindRequiredAssembly(AssemblyExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(DependencyInjection))),
            AssemblyExtensions.FindRequiredAssembly(AssemblyExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CliArgumentsParser))),
            AssemblyExtensions.FindRequiredAssembly(AssemblyExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(DependencyInjection), nameof(Test))),
        };

        Assert.True(compositionInfo.All(Satisfies));

        bool Satisfies(DependencyInfo info)
        {
            return TypeSatisfies(info.ServiceType)
                   && TypeSatisfies(info.ImplementationType)
                   && info.Dependencies.All(Satisfies);
        }

        bool TypeSatisfies(Type type)
        {
            var satisfies = allowedAssemblies.Contains(type.Assembly)
                            || pluginTypes.Contains(type);

            if (!satisfies)
            {
                Output.WriteLine(type.FullName);
            }

            return satisfies;
        }
    }

    private class TestAdditionalType
    {
    }
}