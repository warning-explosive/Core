namespace SpaceEngineers.Core.DependencyInjection.Test;

using System;
using System.Collections.Generic;
using Basics;
using Dependencies;
using Registrations;
using SpaceEngineers.Core.Test.Api;
using SpaceEngineers.Core.Test.Api.ClassFixtures;
using Xunit;
using Xunit.Abstractions;

public class DependencyContainerDecoratorsTest : TestBase
{
    public DependencyContainerDecoratorsTest(ITestOutputHelper output, TestFixture fixture)
        : base(output, fixture)
    {
        var assemblies = new[]
        {
            AssemblyExtensions.FindRequiredAssembly(AssemblyExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(DependencyInjection), nameof(Test)))
        };

        var options = new DependencyContainerOptions()
            .WithPluginAssemblies(assemblies)
            .WithManualRegistrations(new ManualDependencyInjectionRegistration());

        DependencyContainer = fixture.DependencyContainer(options);
    }

    private IDependencyContainer DependencyContainer { get; }

    [Fact]
    internal void DecoratorTest()
    {
        var service = DependencyContainer.Resolve<IDecorableService>();

        var types = new Dictionary<Type, Type>
        {
            [typeof(DecorableServiceDecorator1)] = typeof(DecorableServiceDecorator2),
            [typeof(DecorableServiceDecorator2)] = typeof(DecorableServiceDecorator3),
            [typeof(DecorableServiceDecorator3)] = typeof(DecorableService)
        };

        void CheckRecursive(IDecorableService resolved, Type type)
        {
            Assert.True(resolved.GetType() == type);
            Output.WriteLine(type.Name);

            if (types.TryGetValue(type, out var nextDecorateeType))
            {
                var decoratee = resolved.GetPropertyValue<IDecorableService>("Decoratee");
                CheckRecursive(decoratee, nextDecorateeType);
            }
        }

        CheckRecursive(service, typeof(DecorableServiceDecorator1));
    }

    [Fact]
    internal void OpenGenericDecoratorTest()
    {
        var service = DependencyContainer.Resolve<IOpenGenericDecorableService<object>>();

        var types = new Dictionary<Type, Type>
        {
            [typeof(OpenGenericDecorableServiceDecorator1<object>)] = typeof(OpenGenericDecorableServiceDecorator2<object>),
            [typeof(OpenGenericDecorableServiceDecorator2<object>)] = typeof(OpenGenericDecorableServiceDecorator3<object>),
            [typeof(OpenGenericDecorableServiceDecorator3<object>)] = typeof(OpenGenericDecorableService<object>)
        };

        void CheckRecursive(IOpenGenericDecorableService<object> resolved, Type type)
        {
            Assert.True(resolved.GetType() == type);
            Output.WriteLine(type.Name);

            if (types.TryGetValue(type, out var nextDecorateeType))
            {
                var decoratee = resolved.GetPropertyValue<IOpenGenericDecorableService<object>>("Decoratee");
                CheckRecursive(decoratee, nextDecorateeType);
            }
        }

        CheckRecursive(service, typeof(OpenGenericDecorableServiceDecorator1<object>));
    }
}