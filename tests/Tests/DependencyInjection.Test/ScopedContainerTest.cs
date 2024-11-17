/*namespace SpaceEngineers.Core.CompositionRoot.Test;

using System.Threading.Tasks;
using AutoRegistrationTest;
using Basics;
using DependencyInjection;
using DependencyInjection.Exceptions;
using Registrations;
using SpaceEngineers.Core.Test.Api;
using SpaceEngineers.Core.Test.Api.ClassFixtures;
using Xunit;
using Xunit.Abstractions;

public class ScopedContainerTest : TestBase
{
    public ScopedContainerTest(ITestOutputHelper output, TestFixture fixture)
        : base(output, fixture)
    {
        var assemblies = new[]
        {
            AssemblyExtensions.FindRequiredAssembly(AssemblyExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CompositionRoot), nameof(Test)))
        };

        var options = new DependencyContainerOptions()
            .WithPluginAssemblies(assemblies)
            .WithManualRegistrations(new ManualDependencyInjectionRegistration());

        DependencyContainer = fixture.DependencyContainer(options);
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
}*/