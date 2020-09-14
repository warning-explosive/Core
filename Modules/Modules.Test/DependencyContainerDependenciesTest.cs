namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoWiringApi.Services;
    using Basics;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// IDependencyContainer dependencies tests
    /// </summary>
    public class DependencyContainerDependenciesTest : ModulesTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public DependencyContainerDependenciesTest(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        internal void IsOurReferenceTest()
        {
            var solutionDirectory = SolutionExtensions.SolutionDirectory();
            Output.WriteLine(solutionDirectory);

            var ourAssembliesNames = solutionDirectory
                                    .Projects()
                                    .Select(p => p.AssemblyName())
                                    .ToHashSet();

            ourAssembliesNames.Each(Output.WriteLine);

            var provider = DependencyContainer.Resolve<ITypeProvider>();

            var ourAssemblies = provider
                               .AllLoadedAssemblies
                               .Where(assembly => ourAssembliesNames.Contains(assembly.GetName().Name!))
                               .ToList();

            var otherAssemblies = provider.AllLoadedAssemblies.Except(ourAssemblies).ToList();

            Assert.True(otherAssemblies.All(other => !provider.OurAssemblies.Contains(other)));
            Assert.True(OrderByName(ourAssemblies).SequenceEqual(OrderByName(provider.OurAssemblies)));

            IOrderedEnumerable<Assembly> OrderByName(IEnumerable<Assembly> assemblies)
            {
                return assemblies.OrderBy(assembly => assembly.GetName().Name);
            }
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
            var provider = DependencyContainer.Resolve<ITypeProvider>();
            var ourTypes = provider.OurTypes;

            // #1 - ITypeProvider
            var wrongOurTypes = ourTypes
                               .Where(t => !t.FullName?.StartsWith(nameof(SpaceEngineers), StringComparison.InvariantCulture) ?? true)
                               .ShowTypes("#1 - ITypeProvider", Output.WriteLine)
                               .ToArray();

            Assert.False(wrongOurTypes.Any());

            // #2 - missing
            wrongOurTypes = AssembliesExtensions.AllFromCurrentDomain()
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
            };

            wrongOurTypes = AssembliesExtensions.AllFromCurrentDomain()
                                                .SelectMany(asm => asm.GetTypes())
                                                .Where(t => (t.FullName?.StartsWith(nameof(SpaceEngineers), StringComparison.InvariantCulture) ?? true)
                                                         && !provider.IsOurType(t)
                                                         && excludedTypes.All(mask => !t.FullName.Contains(mask, StringComparison.InvariantCulture)))
                                                .ShowTypes("#3 - missing", Output.WriteLine)
                                                .ToArray();

            Assert.False(wrongOurTypes.Any());

            // #4 - uniqueness
            var notUniqueTypes = ourTypes.GroupBy(it => it)
                                         .Where(grp => grp.Count() > 1)
                                         .Select(grp => grp.Key.FullName)
                                         .ToList();

            if (notUniqueTypes.Any())
            {
                Output.WriteLine(string.Join(Environment.NewLine, notUniqueTypes));
            }

            Assert.Equal(ourTypes.Count(), ourTypes.Distinct().Count());
        }
    }
}