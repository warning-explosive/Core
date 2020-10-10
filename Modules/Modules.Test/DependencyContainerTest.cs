namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using AutoRegistration.Implementations;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringApi.Contexts;
    using AutoWiringApi.Enumerations;
    using AutoWiringApi.Services;
    using AutoWiringTest;
    using Basics;
    using Core.SettingsManager.Abstractions;
    using Moq;
    using SimpleInjector;
    using Xunit;
    using Xunit.Abstractions;
    using TypeExtensions = Basics.TypeExtensions;

    /// <summary>
    /// IDependencyContainer class tests
    /// </summary>
    public class DependencyContainerTest : ModulesTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public DependencyContainerTest(ITestOutputHelper output)
            : base(output) { }

        /// <summary>
        /// ResolveRegisteredServicesTest Data
        /// </summary>
        /// <returns>Test data</returns>
        public static IEnumerable<object[]> ResolveRegisteredServicesTestData()
        {
            return new List<object[]>
            {
                new object[]
                {
                    typeof(IResolvable),
                    new Func<IEnumerable<Type>, IEnumerable<Type>>(SingleResolvable),
                    new Func<IDependencyContainer, Type, object>((container, service) => container.Resolve(service))
                },
                new object[]
                {
                    typeof(ICollectionResolvable<>),
                    new Func<IEnumerable<Type>, IEnumerable<Type>>(Collections),
                    new Func<IDependencyContainer, Type, object>((container, service) => container.ResolveCollection(service))
                }
            };

            IEnumerable<Type> SingleResolvable(IEnumerable<Type> source)
            {
                return source.Where(t => typeof(IResolvable).IsAssignableFrom(t) && typeof(IResolvable) != t);
            }

            IEnumerable<Type> Collections(IEnumerable<Type> source)
            {
                return source.Where(t => t.IsSubclassOfOpenGeneric(typeof(ICollectionResolvable<>)))
                             .SelectMany(t => t.ExtractGenericArgumentsAt(typeof(ICollectionResolvable<>), 0))
                             .Where(type => !type.IsGenericParameter)
                             .Select(type => type.GenericTypeDefinitionOrSelf())
                             .Distinct();
            }
        }

        internal static Func<TypeArgumentSelectionContext, Type?> HybridTypeArgumentSelector(IDependencyContainer container)
        {
            return ctx => FromExistedClosedTypesTypeArgumentSelector(container.Resolve<ITypeProvider>().AllLoadedTypes, ctx)
                       ?? FromMatchesTypeArgumentSelector(ctx);
        }

        internal static Type? FromExistedClosedTypesTypeArgumentSelector(IEnumerable<Type> source, TypeArgumentSelectionContext ctx)
            => source
              .OrderBy(t => t.IsGenericType)
              .FirstOrDefault(t => (!t.IsGenericType || t.IsConstructedGenericType) && t.IsSubclassOfOpenGeneric(ctx.OpenGeneric))
             ?.ExtractGenericArgumentsAt(ctx.OpenGeneric, ctx.TypeArgument.GenericParameterPosition)
              .FirstOrDefault();

        internal static Type? FromMatchesTypeArgumentSelector(TypeArgumentSelectionContext ctx)
        {
            var isVersioned = ctx.OpenGeneric.IsSubclassOfOpenGeneric(typeof(IVersioned<>));

            var predicate = isVersioned
                                ? type => typeof(IResolvable).IsAssignableFrom(type) && type != typeof(IResolvable)
                                : new Func<Type, bool>(type => true);

            return ctx.Matches.Contains(typeof(object)) && !isVersioned
                       ? typeof(object)
                       : ctx.Matches.OrderBy(t => t.IsGenericType).FirstOrDefault(predicate);
        }

        [Theory]
        [MemberData(nameof(ResolveRegisteredServicesTestData))]
        internal void ResolveRegisteredServicesTest(Type apiService,
                                                    Func<IEnumerable<Type>, IEnumerable<Type>> selector,
                                                    Func<IDependencyContainer, Type, object> resolve)
        {
            Output.WriteLine(apiService.FullName);

            var genericTypeProvider = DependencyContainer.Resolve<IGenericTypeProvider>();

            using (DependencyContainer.OpenScope())
            {
                selector(DependencyContainer.Resolve<ITypeProvider>().OurTypes)
                   .Each(type =>
                         {
                             var service = type.IsGenericType
                                        && !type.IsConstructedGenericType
                                               ? genericTypeProvider.CloseByConstraints(type, HybridTypeArgumentSelector(DependencyContainer))
                                               : type;

                             if (type.HasAttribute<UnregisteredAttribute>())
                             {
                                 Assert.Throws<ActivationException>(() => resolve(DependencyContainer, service));
                             }
                             else
                             {
                                 resolve(DependencyContainer, service);
                             }
                         });
            }
        }

        [Fact]
        internal void EachServiceHasVersionedWrapperTest()
        {
            var genericTypeProvider = DependencyContainer.Resolve<IGenericTypeProvider>();
            var typeProvider = DependencyContainer.Resolve<ITypeProvider>();

            var localContainer = SetupDependencyContainer(Registration);

            using (localContainer.OpenScope())
            {
                localContainer
                   .Resolve<ITypeProvider>()
                   .OurTypes
                   .Where(t => typeof(IResolvable).IsAssignableFrom(t)
                               && t != typeof(IResolvable)
                               && !t.IsSubclassOfOpenGeneric(typeof(IDecorator<>)))
                   .Each(service =>
                         {
                             Type versioned;

                             if (service.IsGenericType
                              && !service.IsConstructedGenericType)
                             {
                                 var closedService = genericTypeProvider.CloseByConstraints(service, HybridTypeArgumentSelector(localContainer));
                                 versioned = typeof(IVersioned<>).MakeGenericType(closedService);
                             }
                             else
                             {
                                 versioned = typeof(IVersioned<>).MakeGenericType(service);
                             }

                             if (service.HasAttribute<UnregisteredAttribute>()
                              || service.IsSubclassOfOpenGeneric(typeof(IVersionFor<>)))
                             {
                                 Assert.Throws<ActivationException>(() => Resolve(versioned));
                             }
                             else
                             {
                                 Output.WriteLine(versioned.ToString());
                                 Resolve(versioned);
                             }
                         });
            }

            object Resolve(Type service) => localContainer.Resolve(service);

            void Registration(IRegistrationContainer container)
            {
                var source = typeProvider.AllLoadedTypes;

                source.Where(t => typeof(IResolvable).IsAssignableFrom(t)
                               && t != typeof(IResolvable)
                               && !t.IsSubclassOfOpenGeneric(typeof(IDecorator<>))
                               && t.IsGenericType
                               && !t.IsConstructedGenericType)
                      .Each(service =>
                            {
                                var closed = genericTypeProvider.CloseByConstraints(service, HybridTypeArgumentSelector(DependencyContainer));
                                var versionedService = typeof(IVersioned<>).MakeGenericType(closed);

                                Output.WriteLine(closed.ToString());
                                if (!container.HasRegistration(versionedService))
                                {
                                    container.RegisterVersioned(closed, EnLifestyle.Transient);
                                }
                            });
            }
        }

        [Fact]
        internal void ServiceWithOpenGenericVersionedDependencyTest()
        {
            var resolved = DependencyContainer.Resolve<ConcreteImplementationWithOpenGenericVersionedDependency<object>>();
            Assert.Equal(resolved.VersionedOpenGeneric.Current, resolved.VersionedOpenGeneric.Original);
        }

        [Fact]
        internal void ExternalServiceWithOpenGenericVersionedDependencyTest()
        {
            var resolved = DependencyContainer.Resolve<IVersioned<IComparable<ExternalResolvableImpl>>>();
            Assert.Equal(resolved.Current, resolved.Original);
        }

        [Fact]
        internal void DependencyContainerSelfResolveTest()
        {
            var container = DependencyContainer.Resolve<IDependencyContainer>();

            Assert.True(ReferenceEquals(container, DependencyContainer));
            Assert.True(container.Equals(DependencyContainer));

            var versionedContainer = DependencyContainer.Resolve<IVersionedContainer>();

            Assert.True(ReferenceEquals(versionedContainer, DependencyContainer));
            Assert.True(versionedContainer.Equals(DependencyContainer));

            var registrationContainer = DependencyContainer.Resolve<IRegistrationContainer>();

            Assert.True(ReferenceEquals(registrationContainer, DependencyContainer));
            Assert.True(registrationContainer.Equals(DependencyContainer));

            var scopedContainer = DependencyContainer.Resolve<IScopedContainer>();

            Assert.True(ReferenceEquals(scopedContainer, DependencyContainer));
            Assert.True(scopedContainer.Equals(DependencyContainer));

            container = DependencyContainer.Resolve<IWithInjectedDependencyContainer>().DependencyContainer;

            Assert.True(ReferenceEquals(container, DependencyContainer));
            Assert.True(container.Equals(DependencyContainer));

            versionedContainer = DependencyContainer.Resolve<IWithInjectedDependencyContainer>().VersionedContainer;

            Assert.True(ReferenceEquals(versionedContainer, DependencyContainer));
            Assert.True(versionedContainer.Equals(DependencyContainer));

            registrationContainer = DependencyContainer.Resolve<IWithInjectedDependencyContainer>().RegistrationContainer;

            Assert.True(ReferenceEquals(registrationContainer, DependencyContainer));
            Assert.True(registrationContainer.Equals(DependencyContainer));

            scopedContainer = DependencyContainer.Resolve<IWithInjectedDependencyContainer>().ScopedContainer;

            Assert.True(ReferenceEquals(scopedContainer, DependencyContainer));
            Assert.True(scopedContainer.Equals(DependencyContainer));
        }

        [Fact]
        internal void SingletonTest()
        {
            // 1 - resolve via service type
            Assert.Equal(DependencyContainer.Resolve<ISingletonTestService>(),
                         DependencyContainer.Resolve<ISingletonTestService>());

            // 2 - resolve via concrete type
            Assert.Equal(DependencyContainer.Resolve<SingletonTestServiceImpl>(),
                         DependencyContainer.Resolve<SingletonTestServiceImpl>());

            // 3 - cross equals resolve
            Assert.Equal(DependencyContainer.Resolve<ISingletonTestService>(),
                         DependencyContainer.Resolve<SingletonTestServiceImpl>());
        }

        [Fact]
        internal void TypedUntypedSingletonTest()
        {
            Assert.Equal(DependencyContainer.Resolve<ISingletonTestService>(),
                         DependencyContainer.Resolve(typeof(ISingletonTestService)));
        }

        [Fact]
        internal void OpenGenericTest()
        {
            Assert.Equal(typeof(OpenGenericTestServiceImpl<string>), DependencyContainer.Resolve<IOpenGenericTestService<string>>().GetType());
            Assert.Equal(typeof(ClosedGenericImplementationOfOpenGenericService), DependencyContainer.Resolve<IOpenGenericTestService<ExternalResolvableImpl>>().GetType());
        }

        [Fact]
        internal void AutoWiringSingletonTest()
        {
            Assert.Equal(DependencyContainer.Resolve<IWiredTestService>().IndependentTestService,
                         DependencyContainer.Resolve<IIndependentTestService>());
        }

        [Fact]
        internal void OrderedCollectionResolvableTest()
        {
            var resolvedTypes = DependencyContainer.ResolveCollection<ICollectionResolvableTestService>()
                                                   .Select(z => z.GetType());

            var types = new[]
            {
                typeof(CollectionResolvableTestServiceImpl3),
                typeof(CollectionResolvableTestServiceImpl2),
                typeof(CollectionResolvableTestServiceImpl1),
            };

            Assert.True(resolvedTypes.SequenceEqual(types));
        }

        [Fact]
        internal void UntypedOrderedCollectionResolvableTest()
        {
            var resolvedTypes = DependencyContainer.ResolveCollection(typeof(ICollectionResolvableTestService))
                                                   .Select(z => z.GetType());

            var types = new[]
            {
                typeof(CollectionResolvableTestServiceImpl3),
                typeof(CollectionResolvableTestServiceImpl2),
                typeof(CollectionResolvableTestServiceImpl1),
            };

            Assert.True(resolvedTypes.SequenceEqual(types));
        }

        [Fact]
        internal void SingletonOpenGenericCollectionResolvableTest()
        {
            var resolvedTypes = DependencyContainer.ResolveCollection<ISingletonGenericCollectionResolvableTestService<object>>()
                                                   .Select(z => z.GetType());

            var types = new[]
                        {
                            typeof(SingletonGenericCollectionResolvableTestServiceImpl3<object>),
                            typeof(SingletonGenericCollectionResolvableTestServiceImpl2<object>),
                            typeof(SingletonGenericCollectionResolvableTestServiceImpl1<object>),
                        };

            Assert.True(resolvedTypes.SequenceEqual(types));

            Assert.True(DependencyContainer
                       .ResolveCollection<ISingletonGenericCollectionResolvableTestService<object>>()
                       .SequenceEqual(DependencyContainer.ResolveCollection<ISingletonGenericCollectionResolvableTestService<object>>()));
        }

        [Fact]
        internal void ImplementationResolvableTest()
        {
            Assert.NotNull(DependencyContainer.Resolve<ConcreteImplementationService>());

            var withDependency = DependencyContainer.Resolve<ConcreteImplementationWithDependencyService>();
            Assert.NotNull(withDependency);
            Assert.NotNull(withDependency.Dependency);

            Assert.NotNull(DependencyContainer.Resolve<ConcreteImplementationGenericService<object>>());
        }

        [Fact]
        internal void ExternalResolvableTest()
        {
            Assert.Equal(typeof(ExternalResolvableImpl), DependencyContainer.Resolve<IComparable<ExternalResolvableImpl>>().GetType());
            Assert.Equal(typeof(ExternalResolvableOpenGenericImpl<object>), DependencyContainer.Resolve<IComparable<object>>().GetType());
        }

        [Fact]
        internal void UnregisteredServiceResolveTest()
        {
            Assert.NotNull(typeof(BaseUnregisteredServiceImpl).GetCustomAttribute<UnregisteredAttribute>(false));
            Assert.Null(typeof(BaseUnregisteredServiceImpl).GetCustomAttribute<LifestyleAttribute>(false));

            Assert.Null(typeof(DerivedFromUnregisteredServiceImpl).GetCustomAttribute<UnregisteredAttribute>(false));
            Assert.NotNull(typeof(DerivedFromUnregisteredServiceImpl).GetCustomAttribute<LifestyleAttribute>(false));

            Assert.True(typeof(DerivedFromUnregisteredServiceImpl).IsSubclassOf(typeof(BaseUnregisteredServiceImpl)));
            Assert.True(typeof(IUnregisteredService).IsAssignableFrom(typeof(DerivedFromUnregisteredServiceImpl)));
            Assert.True(typeof(IUnregisteredService).IsAssignableFrom(typeof(BaseUnregisteredServiceImpl)));

            Assert.Equal(typeof(DerivedFromUnregisteredServiceImpl), DependencyContainer.Resolve<IUnregisteredService>().GetType());
        }

        [Fact]
        internal void UnregisteredExternalServiceResolveTest()
        {
            Assert.NotNull(typeof(BaseUnregisteredExternalServiceImpl).GetCustomAttribute<UnregisteredAttribute>(false));
            Assert.Null(typeof(BaseUnregisteredExternalServiceImpl).GetCustomAttribute<LifestyleAttribute>(false));

            Assert.Null(typeof(DerivedFromUnregisteredExternalServiceImpl).GetCustomAttribute<UnregisteredAttribute>(false));
            Assert.NotNull(typeof(DerivedFromUnregisteredExternalServiceImpl).GetCustomAttribute<LifestyleAttribute>(false));

            Assert.True(typeof(DerivedFromUnregisteredExternalServiceImpl).IsSubclassOf(typeof(BaseUnregisteredExternalServiceImpl)));
            Assert.True(typeof(IUnregisteredExternalService).IsAssignableFrom(typeof(DerivedFromUnregisteredExternalServiceImpl)));
            Assert.True(typeof(IUnregisteredExternalService).IsAssignableFrom(typeof(BaseUnregisteredExternalServiceImpl)));

            Assert.Equal(typeof(DerivedFromUnregisteredExternalServiceImpl), DependencyContainer.Resolve<IUnregisteredExternalService>().GetType());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [SuppressMessage("Using IDisposables", "CA1508", Justification = "False positive")]
        internal void BoundedContainerTest(bool mode)
        {
            var settingsContainer = AutoRegistration.DependencyContainer
                                                    .CreateBounded(new[]
                                                                   {
                                                                       typeof(ISettingsManager<>).Assembly,
                                                                       typeof(ICompositionInfoExtractor).Assembly,
                                                                   },
                                                                   new DependencyContainerOptions());

            var versionFactory = new Func<IVersionFor<ITypeProvider>>(
                () =>
                {
                    var mock = new Mock<IVersionFor<ITypeProvider>>(MockBehavior.Loose);
                    mock.Setup(z => z.Version.AllLoadedTypes)
                        .Returns(() => settingsContainer
                                      .Resolve<ITypeProvider>()
                                      .AllLoadedTypes
                                      .Concat(new[]
                                              {
                                                  typeof(TestYamlSettings),
                                                  typeof(TestJsonSettings)
                                              })
                                      .ToList());
                    return mock.Object;
                });

            IReadOnlyCollection<IDependencyInfo> compositionInfo;

            using (settingsContainer.UseVersion<ITypeProvider>(versionFactory))
            {
                compositionInfo = settingsContainer.Resolve<ICompositionInfoExtractor>()
                                                   .GetCompositionInfo(mode);
            }

            Output.WriteLine($"Total: {compositionInfo.Count}\n");

            Output.WriteLine(settingsContainer.Resolve<ICompositionInfoInterpreter<string>>()
                                              .Visualize(compositionInfo));

            var allowedAssemblies = new[]
                                    {
                                        typeof(Container).Assembly,                 // SimpleInjector assembly,
                                        typeof(TypeExtensions).Assembly,            // Basics assembly
                                        typeof(LifestyleAttribute).Assembly,        // AutoWiringApi assembly
                                        typeof(IDependencyContainer).Assembly,      // AutoRegistration assembly
                                        typeof(ISettingsManager<>).Assembly,        // SettingsManager assembly
                                        typeof(ICompositionInfoExtractor).Assembly, // CompositionInfoExtractor assembly
                                    };

            Assert.True(compositionInfo.All(Satisfies));

            bool Satisfies(IDependencyInfo info)
            {
                return TypeSatisfies(info.ServiceType)
                    && TypeSatisfies(info.ImplementationType)
                    && info.Dependencies.All(Satisfies);
            }

            bool TypeSatisfies(Type type)
            {
                var condition = allowedAssemblies.Contains(type.Assembly);

                if (!condition)
                {
                    Output.WriteLine(type.FullName);
                }

                return condition;
            }
        }

        private class TestYamlSettings : IYamlSettings
        {
        }

        private class TestJsonSettings : IJsonSettings
        {
        }
    }
}