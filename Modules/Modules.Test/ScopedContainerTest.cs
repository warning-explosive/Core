namespace SpaceEngineers.Core.Modules.Test
{
    using System.Threading.Tasks;
    using AutoRegistration.Abstractions;
    using AutoWiringTest;
    using Basics.Test;
    using Core.Test.Api.ClassFixtures;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Contract.Abstractions;
    using GenericHost;
    using Registrations;
    using SimpleInjector;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// IScopedContainer class tests
    /// </summary>
    public class ScopedContainerTest : BasicsTestBase, IClassFixture<ModulesTestFixture>
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public ScopedContainerTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output)
        {
            var excludedAssemblies = new[]
            {
                typeof(IIntegrationMessage).Assembly, // GenericEndpoint.Contract
                typeof(IGenericEndpoint).Assembly, // GenericEndpoint
                typeof(GenericHost).Assembly // GenericHost
            };

            DependencyContainer = fixture.GetDependencyContainer(typeof(ScopedContainerTest).Assembly, excludedAssemblies);
        }

        /// <summary>
        /// DependencyContainer
        /// </summary>
        protected IDependencyContainer DependencyContainer { get; }

        [Fact]
        internal async Task AsyncScopeTest()
        {
            Assert.Throws<ActivationException>(() => DependencyContainer.Resolve<IScopedLifestyleService>());

            using (DependencyContainer.OpenScope())
            {
                var service = DependencyContainer.Resolve<IScopedLifestyleService>();
                await service.DoSmth().ConfigureAwait(false);

                var anotherService = DependencyContainer.Resolve<IScopedLifestyleService>();
                await anotherService.DoSmth().ConfigureAwait(false);
                Assert.True(ReferenceEquals(service, anotherService));

                using (DependencyContainer.OpenScope())
                {
                    anotherService = DependencyContainer.Resolve<IScopedLifestyleService>();
                    await anotherService.DoSmth().ConfigureAwait(false);
                    Assert.False(ReferenceEquals(service, anotherService));
                }

                anotherService = DependencyContainer.Resolve<IScopedLifestyleService>();
                await anotherService.DoSmth().ConfigureAwait(false);
                Assert.True(ReferenceEquals(service, anotherService));
            }
        }
    }
}