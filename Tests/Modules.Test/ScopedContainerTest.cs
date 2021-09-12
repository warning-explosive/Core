namespace SpaceEngineers.Core.Modules.Test
{
    using System.Threading.Tasks;
    using AutoRegistrationTest;
    using CompositionRoot.Api.Abstractions.Container;
    using CompositionRoot.Api.Exceptions;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using Registrations;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// IScopedContainer class tests
    /// </summary>
    public class ScopedContainerTest : TestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public ScopedContainerTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
            DependencyContainer = fixture.ModulesContainer();
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