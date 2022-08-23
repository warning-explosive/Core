namespace SpaceEngineers.Core.CompositionRoot.Test
{
    using System.Threading.Tasks;
    using Api.Abstractions;
    using Api.Exceptions;
    using AutoRegistrationTest;
    using Basics;
    using GenericHost.Internals;
    using Registrations;
    using SpaceEngineers.Core.Test.Api;
    using SpaceEngineers.Core.Test.Api.ClassFixtures;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// IScopedContainer class tests
    /// </summary>
    public class ScopedContainerTest : TestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        public ScopedContainerTest(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.CompositionRoot), nameof(Core.CompositionRoot.Test))),
            };

            var options = new DependencyContainerOptions().WithManualRegistrations(
                new ManuallyRegisteredServiceManualRegistration(),
                new ConfigurationProviderManualRegistration());

            DependencyContainer = fixture.BoundedAboveContainer(output, options, assemblies);
        }

        private IDependencyContainer DependencyContainer { get; }

        [Fact]
        internal async Task AsyncScopeTest()
        {
            Assert.Throws<ComponentResolutionException>(() => DependencyContainer.Resolve<IScopedService>());

            using (DependencyContainer.OpenScope())
            {
                var service = DependencyContainer.Resolve<IScopedService>();
                await service.DoSmth().ConfigureAwait(false);

                var anotherService = DependencyContainer.Resolve<IScopedService>();
                await anotherService.DoSmth().ConfigureAwait(false);
                Assert.True(ReferenceEquals(service, anotherService));

                using (DependencyContainer.OpenScope())
                {
                    anotherService = DependencyContainer.Resolve<IScopedService>();
                    await anotherService.DoSmth().ConfigureAwait(false);
                    Assert.False(ReferenceEquals(service, anotherService));
                }

                anotherService = DependencyContainer.Resolve<IScopedService>();
                await anotherService.DoSmth().ConfigureAwait(false);
                Assert.True(ReferenceEquals(service, anotherService));
            }
        }
    }
}