namespace SpaceEngineers.Core.Modules.Test
{
    using System.Threading.Tasks;
    using AutoWiringTest;
    using SimpleInjector;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// IScopedContainer class tests
    /// </summary>
    public class ScopedContainerTest : ModulesTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public ScopedContainerTest(ITestOutputHelper output)
            : base(output) { }

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