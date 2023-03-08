namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Basics;
    using Registrations;
    using SpaceEngineers.Core.Test.Api;
    using SpaceEngineers.Core.Test.Api.ClassFixtures;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// DependencyContainerDependenciesTest
    /// </summary>
    public class DependencyContainerDependenciesTest : TestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        public DependencyContainerDependenciesTest(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CompositionRoot), nameof(Test)))
            };

            var options = new DependencyContainerOptions()
               .WithManualRegistrations(new ManuallyRegisteredServiceManualRegistration());

            DependencyContainer = fixture.BoundedAboveContainer(output, options, assemblies);
        }

        private IDependencyContainer DependencyContainer { get; }

        [Fact]
        internal void IsOurReferenceTest()
        {
            var solutionFile = SolutionExtensions.SolutionFile();
            Output.WriteLine(solutionFile.FullName);

            var directory = solutionFile.Directory
                            ?? throw new InvalidOperationException("Solution directory wasn't found");

            var ourAssembliesNames = directory
                .ProjectFiles()
                .Select(p => p.AssemblyName())
                .ToHashSet();

            var provider = DependencyContainer.Resolve<ITypeProvider>();

            var ourAssemblies = provider
                .AllLoadedAssemblies
                .Where(assembly => ourAssembliesNames.Contains(assembly.GetName().Name!))
                .ToList();

            Output.WriteLine("Our assemblies:");
            ourAssemblies.Select(assembly => assembly.GetName().Name).Each(Output.WriteLine);

            var missing = ourAssemblies
                .Where(assembly => !provider.OurAssemblies.Contains(assembly))
                .ToList();

            Output.WriteLine("missing #1:");
            missing.Select(assembly => assembly.GetName().Name).Each(Output.WriteLine);

            Assert.Empty(missing);

            missing = ourAssemblies
                .Where(assembly => !AssembliesExtensions.AllOurAssembliesFromCurrentDomain().Contains(assembly))
                .ToList();

            Output.WriteLine("missing #2:");
            missing.Select(assembly => assembly.GetName().Name).Each(Output.WriteLine);

            Assert.Empty(missing);
        }

        [Fact]
        internal void UniqueAssembliesTest()
        {
            var provider = DependencyContainer.Resolve<ITypeProvider>();

            CheckAssemblies(provider.OurAssemblies);
            CheckAssemblies(provider.OurTypes.Select(t => t.Assembly).ToList());
            CheckAssemblies(provider.AllLoadedTypes.Select(t => t.Assembly).ToList());

            void CheckAssemblies(IReadOnlyCollection<Assembly> assemblies)
            {
                Assert.Equal(assemblies.Distinct().Count(), assemblies.GroupBy(a => a.FullName).Count());
            }
        }

        [Fact]
        internal void IsOurTypeTest()
        {
            var excludedAssemblies = new[]
            {
                nameof(System),
                nameof(Microsoft),
                "Windows"
            };

            var allAssemblies = AssembliesExtensions
                .AllAssembliesFromCurrentDomain()
                .Where(assembly =>
                {
                    var assemblyName = assembly.GetName().FullName;
                    return excludedAssemblies.All(excluded => !assemblyName.StartsWith(excluded, StringComparison.OrdinalIgnoreCase));
                })
                .ToArray();

            var provider = DependencyContainer.Resolve<ITypeProvider>();
            var ourTypes = provider.OurTypes;

            // #1 - ITypeProvider
            var wrongOurTypes = ourTypes
                .Where(type => !type.FullName?.StartsWith(nameof(SpaceEngineers), StringComparison.Ordinal) ?? true)
                .ShowTypes("#1 - ITypeProvider", Output.WriteLine)
                .ToArray();

            Assert.False(wrongOurTypes.Any());

            // #2 - missing
            wrongOurTypes = allAssemblies
                .SelectMany(asm => asm.GetTypes())
                .Except(ourTypes)
                .Where(type => provider.IsOurType(type))
                .ShowTypes("#2 - missing", Output.WriteLine)
                .ToArray();

            Assert.False(wrongOurTypes.Any());

            // #3 - missing
            var excludedTypes = new[]
            {
                "<>f",
                "<>c",
                "d__",
                "<PrivateImplementationDetails>",
                "AutoGeneratedProgram",
                "Xunit",
                "System.Runtime.CompilerServices",
                "Microsoft.CodeAnalysis",
                "Coverlet.Core.Instrumentation.Tracker"
            };

            var expectedAssemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Basics))),

                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(AutoRegistration), nameof(AutoRegistration.Api))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CompositionRoot))),

                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.Test), nameof(Core.Test.Api))),
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CompositionRoot), nameof(Test))),

                AssembliesExtensions.FindRequiredAssembly("System.Private.CoreLib")
            };

            wrongOurTypes = allAssemblies
                .SelectMany(asm => asm.GetTypes())
                .Where(type => (type.FullName?.StartsWith(nameof(SpaceEngineers), StringComparison.Ordinal) ?? true)
                            && expectedAssemblies.Contains(type.Assembly)
                            && !provider.IsOurType(type)
                            && excludedTypes.All(mask => !type.FullName.Contains(mask, StringComparison.Ordinal)))
                .ShowTypes("#3 - missing", Output.WriteLine)
                .ToArray();

            Assert.False(wrongOurTypes.Any());

            // #4 - uniqueness
            var notUniqueTypes = ourTypes
                .GroupBy(type => type)
                .Where(grp => grp.Count() > 1)
                .Select(grp => grp.Key.FullName)
                .ToList();

            if (notUniqueTypes.Any())
            {
                Output.WriteLine(string.Join(Environment.NewLine, notUniqueTypes));
            }

            Assert.Equal(ourTypes.Count, ourTypes.Distinct().Count());
        }
    }
}