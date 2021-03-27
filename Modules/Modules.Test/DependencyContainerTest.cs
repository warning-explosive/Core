namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Contexts;
    using AutoWiring.Api.Enumerations;
    using AutoWiring.Api.Services;
    using AutoWiringTest;
    using Basics;
    using Basics.Test;
    using Core.SettingsManager.Abstractions;
    using Core.Test.Api.ClassFixtures;
    using GenericEndpoint.Abstractions;
    using GenericEndpoint.Contract.Abstractions;
    using GenericHost;
    using SimpleInjector;
    using Xunit;
    using Xunit.Abstractions;
    using TypeExtensions = Basics.TypeExtensions;

    /// <summary>
    /// IDependencyContainer class tests
    /// </summary>
    public class DependencyContainerTest : BasicsTestBase, IClassFixture<ModulesTestFixture>
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public DependencyContainerTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output)
        {
            var options = new DependencyContainerOptions
            {
                ExcludedAssemblies = new[]
                {
                    typeof(IIntegrationMessage).Assembly, // GenericEndpoint.Contract
                    typeof(IGenericEndpoint).Assembly, // GenericEndpoint
                    typeof(GenericHost).Assembly // GenericHost
                }
            };

            DependencyContainer = fixture.GetDependencyContainer(typeof(DependencyContainerTest).Assembly, options);
        }

        /// <summary>
        /// DependencyContainer
        /// </summary>
        protected IDependencyContainer DependencyContainer { get; }

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
                return source
                    .Where(type => typeof(IResolvable).IsAssignableFrom(type)
                                   && typeof(IResolvable) != type
                                   && type.HasAttribute<ComponentAttribute>());
            }

            IEnumerable<Type> Collections(IEnumerable<Type> source)
            {
                return source
                    .Where(t => t.IsSubclassOfOpenGeneric(typeof(ICollectionResolvable<>)))
                    .SelectMany(t => t.ExtractGenericArgumentsAt(typeof(ICollectionResolvable<>)))
                    .Where(type => !type.IsGenericParameter && type.HasAttribute<ComponentAttribute>())
                    .Select(type => type.GenericTypeDefinitionOrSelf())
                    .Distinct();
            }
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

                        Output.WriteLine(service.FullName);

                        var componentKind = type.GetRequiredAttribute<ComponentAttribute>().Kind;

                        if (componentKind == EnComponentKind.Regular
                            || componentKind == EnComponentKind.ManuallyRegistered
                            || componentKind == EnComponentKind.OpenGenericFallback)
                        {
                            resolve(DependencyContainer, service);
                        }
                        else
                        {
                            Assert.Throws<ActivationException>(() => resolve(DependencyContainer, service));
                        }
                    });
            }
        }

        [Fact]
        internal void DependencyContainerSelfResolveTest()
        {
            var container = DependencyContainer.Resolve<IDependencyContainer>();

            Assert.True(ReferenceEquals(container, DependencyContainer));
            Assert.True(container.Equals(DependencyContainer));

            var scopedContainer = DependencyContainer.Resolve<IScopedContainer>();

            Assert.True(ReferenceEquals(scopedContainer, DependencyContainer));
            Assert.True(scopedContainer.Equals(DependencyContainer));

            container = DependencyContainer.Resolve<IWithInjectedDependencyContainer>().DependencyContainer;

            Assert.True(ReferenceEquals(container, DependencyContainer));
            Assert.True(container.Equals(DependencyContainer));

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
                typeof(CollectionResolvableTestServiceImpl1)
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
                typeof(CollectionResolvableTestServiceImpl1)
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
                            typeof(SingletonGenericCollectionResolvableTestServiceImpl1<object>)
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
            Assert.Equal(EnComponentKind.Unregistered, typeof(BaseUnregisteredServiceImpl).GetCustomAttribute<ComponentAttribute>(false).Kind);
            Assert.Equal(EnComponentKind.Regular, typeof(DerivedFromUnregisteredServiceImpl).GetCustomAttribute<ComponentAttribute>(false).Kind);

            Assert.True(typeof(DerivedFromUnregisteredServiceImpl).IsSubclassOf(typeof(BaseUnregisteredServiceImpl)));
            Assert.True(typeof(IUnregisteredService).IsAssignableFrom(typeof(DerivedFromUnregisteredServiceImpl)));
            Assert.True(typeof(IUnregisteredService).IsAssignableFrom(typeof(BaseUnregisteredServiceImpl)));

            Assert.Equal(typeof(DerivedFromUnregisteredServiceImpl), DependencyContainer.Resolve<IUnregisteredService>().GetType());
        }

        [Fact]
        internal void UnregisteredExternalServiceResolveTest()
        {
            Assert.Equal(EnComponentKind.Unregistered, typeof(BaseUnregisteredExternalServiceImpl).GetCustomAttribute<ComponentAttribute>(false).Kind);
            Assert.Equal(EnComponentKind.Regular, typeof(DerivedFromUnregisteredExternalServiceImpl).GetCustomAttribute<ComponentAttribute>(false).Kind);

            Assert.True(typeof(DerivedFromUnregisteredExternalServiceImpl).IsSubclassOf(typeof(BaseUnregisteredExternalServiceImpl)));
            Assert.True(typeof(IUnregisteredExternalService).IsAssignableFrom(typeof(DerivedFromUnregisteredExternalServiceImpl)));
            Assert.True(typeof(IUnregisteredExternalService).IsAssignableFrom(typeof(BaseUnregisteredExternalServiceImpl)));

            Assert.Equal(typeof(DerivedFromUnregisteredExternalServiceImpl), DependencyContainer.Resolve<IUnregisteredExternalService>().GetType());
            Assert.Throws<ActivationException>(() => DependencyContainer.Resolve<DerivedFromUnregisteredExternalServiceImpl>().GetType());
            Assert.Throws<ActivationException>(() => DependencyContainer.Resolve<BaseUnregisteredExternalServiceImpl>().GetType());
        }

        [Fact]
        internal void ManualRegistrationResolutionTest()
        {
            var cctorResolutionBehavior = SimpleInjector(DependencyContainer)
                .Options
                .ConstructorResolutionBehavior;

            var parameterType = cctorResolutionBehavior
                .TryGetConstructor(typeof(WiredTestServiceImpl), out var error)
                .GetParameters()
                .Single()
                .ParameterType;

            Assert.Null(error);
            Assert.Equal(typeof(IIndependentTestService), parameterType);

            var expectedCollection = new[]
                                     {
                                         typeof(CollectionResolvableTestServiceImpl1),
                                         typeof(CollectionResolvableTestServiceImpl2)
                                     };

            var registration = DependencyContainerOptions.DelegateRegistration(
                container =>
                {
                    container.Register<IWiredTestService, WiredTestServiceImpl>();
                    container.Register<WiredTestServiceImpl, WiredTestServiceImpl>();
                    container.Register<IIndependentTestService, IndependentTestServiceImpl>();
                    container.Register<IndependentTestServiceImpl, IndependentTestServiceImpl>();
                    container.Register<ConcreteImplementationWithDependencyService, ConcreteImplementationWithDependencyService>();
                    container.Register<ConcreteImplementationService, ConcreteImplementationService>();
                    container.RegisterCollection<ICollectionResolvableTestService>(expectedCollection);
                    container.Register<IOpenGenericTestService<object>, OpenGenericTestServiceImpl<object>>();
                    container.Register<OpenGenericTestServiceImpl<object>, OpenGenericTestServiceImpl<object>>();
                });

            var options = new DependencyContainerOptions
            {
                ManualRegistrations = new[] { registration }
            };

            var empty = Array.Empty<Assembly>();
            var localContainer = SpaceEngineers.Core.AutoRegistration.DependencyContainer.CreateExactlyBounded(empty, options);

            localContainer.Resolve<IWiredTestService>();
            localContainer.Resolve<WiredTestServiceImpl>();

            localContainer.Resolve<IIndependentTestService>();
            localContainer.Resolve<IndependentTestServiceImpl>();

            localContainer.Resolve<ConcreteImplementationWithDependencyService>();

            localContainer.Resolve<ConcreteImplementationService>();

            var actual = localContainer
                .ResolveCollection<ICollectionResolvableTestService>()
                .Select(r => r.GetType())
                .ToList();

            Assert.True(expectedCollection.OrderByDependencyAttribute().SequenceEqual(expectedCollection.Reverse()));
            Assert.True(expectedCollection.OrderByDependencyAttribute().SequenceEqual(actual));

            localContainer.Resolve<IOpenGenericTestService<object>>();
            localContainer.Resolve<OpenGenericTestServiceImpl<object>>();
            Assert.Throws<ActivationException>(() => localContainer.Resolve<IOpenGenericTestService<string>>());

            static Container SimpleInjector(IDependencyContainer container) => container.GetFieldValue<Container>("_container");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        internal void BoundedContainerTest(bool mode)
        {
            var assemblies = new[]
            {
                typeof(ISettingsManager<>).Assembly,
                typeof(ICompositionInfoExtractor).Assembly
            };

            var boundedContainer = AutoRegistration.DependencyContainer.CreateExactlyBounded(assemblies, new DependencyContainerOptions());

            if (mode)
            {
                Assert.Throws<InvalidOperationException>(() => GetCompositionInfo(boundedContainer, mode));
            }
            else
            {
                _ = GetCompositionInfo(boundedContainer, mode);
            }

            var additionalTypes = new[]
            {
                typeof(TestJsonSettings),
                typeof(TestYamlSettings)
            };

            var extendedTypeProvider = new ExtendedTestTypeProvider(boundedContainer.Resolve<ITypeProvider>(), additionalTypes);
            var overrides = DependencyContainerOptions
                .DelegateRegistration(container =>
                {
                    container.RegisterInstance(typeof(ITypeProvider), extendedTypeProvider);
                    container.RegisterInstance(extendedTypeProvider.GetType(), extendedTypeProvider);
                });

            var extendedBoundedContainer = AutoRegistration.DependencyContainer
                .CreateExactlyBounded(
                    assemblies,
                    new DependencyContainerOptions
                    {
                        Overrides = new[] { overrides }
                    });

            var compositionInfo = GetCompositionInfo(extendedBoundedContainer, mode);

            Output.WriteLine($"Total: {compositionInfo.Count}\n");
            Output.WriteLine(boundedContainer.Resolve<ICompositionInfoInterpreter<string>>().Visualize(compositionInfo));

            var allowedAssemblies = new[]
            {
                typeof(Container).Assembly, // SimpleInjector assembly,
                typeof(TypeExtensions).Assembly, // Basics assembly
                typeof(ComponentAttribute).Assembly, // AutoWiring.Api assembly
                typeof(IDependencyContainer).Assembly, // AutoRegistration assembly
                typeof(ISettingsManager<>).Assembly, // SettingsManager assembly
                typeof(ICompositionInfoExtractor).Assembly // CompositionInfoExtractor assembly
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
                var satisfies = allowedAssemblies.Contains(type.Assembly)
                                || type == extendedTypeProvider.GetType();

                if (!satisfies)
                {
                    Output.WriteLine(type.FullName);
                }

                return satisfies;
            }

            static IReadOnlyCollection<IDependencyInfo> GetCompositionInfo(IDependencyContainer container, bool mode)
            {
                return container.Resolve<ICompositionInfoExtractor>().GetCompositionInfo(mode);
            }
        }

        internal static Func<TypeArgumentSelectionContext, Type?> HybridTypeArgumentSelector(IDependencyContainer container)
        {
            return ctx => FromExistedClosedTypesTypeArgumentSelector(container.Resolve<ITypeProvider>().AllLoadedTypes, ctx)
                          ?? FromMatchesTypeArgumentSelector(ctx);
        }

        private static Type? FromExistedClosedTypesTypeArgumentSelector(IEnumerable<Type> source, TypeArgumentSelectionContext ctx)
            => source
                .OrderBy(t => t.IsGenericType)
                .FirstOrDefault(t => t.IsConstructedOrSimpleType() && t.IsSubclassOfOpenGeneric(ctx.OpenGeneric))
               ?.ExtractGenericArgumentsAt(ctx.OpenGeneric, ctx.TypeArgument.GenericParameterPosition)
                .FirstOrDefault();

        private static Type? FromMatchesTypeArgumentSelector(TypeArgumentSelectionContext ctx)
        {
            return ctx.Matches.Contains(typeof(object))
                ? typeof(object)
                : ctx.Matches.OrderBy(t => t.IsGenericType).FirstOrDefault();
        }

        private class TestYamlSettings : IYamlSettings
        {
        }

        private class TestJsonSettings : IJsonSettings
        {
        }

        [Component(EnLifestyle.Singleton, EnComponentKind.Override)]
        private class ExtendedTestTypeProvider : ITypeProvider
        {
            private readonly ITypeProvider _decoratee;
            private readonly IReadOnlyCollection<Type> _additionalTypes;

            public ExtendedTestTypeProvider(
                ITypeProvider decoratee,
                IReadOnlyCollection<Type> additionalTypes)
            {
                _decoratee = decoratee;
                _additionalTypes = additionalTypes;
            }

            public IReadOnlyCollection<Assembly> AllLoadedAssemblies => _decoratee.AllLoadedAssemblies;

            public IReadOnlyCollection<Type> AllLoadedTypes => _decoratee.AllLoadedTypes.Concat(_additionalTypes).ToList();

            public IReadOnlyCollection<Assembly> OurAssemblies => _decoratee.OurAssemblies;

            public IReadOnlyCollection<Type> OurTypes => _decoratee.OurTypes;

            public IReadOnlyDictionary<string, IReadOnlyDictionary<string, Type>> TypeCache => _decoratee.TypeCache;

            public bool IsOurType(Type type) => _decoratee.IsOurType(type);
        }
    }
}