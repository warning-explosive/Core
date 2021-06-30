namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using AutoWiring.Api;
    using AutoWiring.Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;
    using AutoWiringTest;
    using Basics;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using Registrations;
    using SimpleInjector;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// IDependencyContainer class tests
    /// </summary>
    public class DependencyContainerTest : TestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public DependencyContainerTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
            DependencyContainer = ModulesTestManualRegistration.Container(fixture);
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
                        Output.WriteLine(type.FullName);

                        var service = type.IsGenericType
                                      && !type.IsConstructedGenericType
                            ? genericTypeProvider.CloseByConstraints(type, HybridTypeArgumentSelector(DependencyContainer))
                            : type;

                        Output.WriteLine(service.FullName);

                        var component = type.GetRequiredAttribute<ComponentAttribute>();

                        if (service.IsSubclassOfOpenGeneric(typeof(IInitializable<>)))
                        {
                            Assert.Throws<ActivationException>(() => resolve(DependencyContainer, service));
                        }
                        else if (component.RegistrationKind == EnComponentRegistrationKind.AutomaticallyRegistered
                                 || component.RegistrationKind == EnComponentRegistrationKind.ManuallyRegistered)
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
            Assert.Equal(typeof(ExternalResolvableImpl), DependencyContainer.Resolve<IProgress<ExternalResolvableImpl>>().GetType());
            Assert.Equal(typeof(ExternalResolvableOpenGenericImpl<object>), DependencyContainer.Resolve<IProgress<object>>().GetType());
        }

        [Fact]
        internal void UnregisteredServiceResolveTest()
        {
            Assert.Equal(EnComponentRegistrationKind.Unregistered, typeof(BaseUnregisteredServiceImpl).GetCustomAttribute<ComponentAttribute>(false).RegistrationKind);

            Assert.True(typeof(DerivedFromUnregisteredServiceImpl).IsSubclassOf(typeof(BaseUnregisteredServiceImpl)));
            Assert.True(typeof(IUnregisteredService).IsAssignableFrom(typeof(DerivedFromUnregisteredServiceImpl)));
            Assert.True(typeof(IUnregisteredService).IsAssignableFrom(typeof(BaseUnregisteredServiceImpl)));

            Assert.Equal(typeof(DerivedFromUnregisteredServiceImpl), DependencyContainer.Resolve<IUnregisteredService>().GetType());
        }

        [Fact]
        internal void UnregisteredExternalServiceResolveTest()
        {
            Assert.Equal(EnComponentRegistrationKind.Unregistered, typeof(BaseUnregisteredExternalServiceImpl).GetCustomAttribute<ComponentAttribute>(false).RegistrationKind);

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

            var registration = Fixture.DelegateRegistration(container =>
            {
                container
                    .Register<IWiredTestService, WiredTestServiceImpl>()
                    .Register<WiredTestServiceImpl, WiredTestServiceImpl>()
                    .Register<IIndependentTestService, IndependentTestServiceImpl>()
                    .Register<IndependentTestServiceImpl, IndependentTestServiceImpl>()
                    .Register<ConcreteImplementationWithDependencyService, ConcreteImplementationWithDependencyService>()
                    .Register<ConcreteImplementationService, ConcreteImplementationService>()
                    .RegisterCollection<ICollectionResolvableTestService>(expectedCollection)
                    .Register<IOpenGenericTestService<object>, OpenGenericTestServiceImpl<object>>()
                    .Register<OpenGenericTestServiceImpl<object>, OpenGenericTestServiceImpl<object>>();
            });

            var options = new DependencyContainerOptions().WithManualRegistrations(registration);

            var localContainer = Fixture.ExactlyBoundedContainer(options);

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

        internal static Func<TypeArgumentSelectionContext, Type?> HybridTypeArgumentSelector(IDependencyContainer container)
        {
            return ctx => FromExistedClosedTypesTypeArgumentSelector(container.Resolve<ITypeProvider>().AllLoadedTypes, ctx)
                          ?? FromMatchesTypeArgumentSelector(ctx);
        }

        private static Type? FromExistedClosedTypesTypeArgumentSelector(IEnumerable<Type> source, TypeArgumentSelectionContext ctx)
            => source
                .OrderBy(t => t.IsGenericType)
                .FirstOrDefault(t => t.IsConstructedOrNonGenericType() && t.IsSubclassOfOpenGeneric(ctx.OpenGeneric))
               ?.ExtractGenericArgumentsAt(ctx.OpenGeneric, ctx.TypeArgument.GenericParameterPosition)
                .FirstOrDefault();

        private static Type? FromMatchesTypeArgumentSelector(TypeArgumentSelectionContext ctx)
        {
            return ctx.Matches.Contains(typeof(object))
                ? typeof(object)
                : ctx.Matches.OrderBy(t => t.IsGenericType).FirstOrDefault();
        }
    }
}