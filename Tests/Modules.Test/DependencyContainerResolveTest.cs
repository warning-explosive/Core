namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using AutoRegistrationTest;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api;
    using CompositionRoot.Api.Abstractions;
    using CompositionRoot.Api.Abstractions.Container;
    using CompositionRoot.Api.Exceptions;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using CrossCuttingConcerns.Api.Abstractions;
    using DataAccess.Api.Model;
    using DataAccess.Api.Persisting;
    using DataAccess.Api.Reading;
    using DataAccess.Orm.Linq;
    using DataAccess.Orm.Sql.ObjectTransformers;
    using Registrations;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// DependencyContainerResolveTest
    /// </summary>
    public class DependencyContainerResolveTest : TestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public DependencyContainerResolveTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
            DependencyContainer = fixture.ModulesContainer();
        }

        private IDependencyContainer DependencyContainer { get; }

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

                        if (service.IsSubclassOfOpenGeneric(typeof(IInitializable<>)))
                        {
                            Assert.Throws<ComponentResolutionException>(() => resolve(DependencyContainer, service));
                        }
                        else if (!type.HasAttribute<UnregisteredComponentAttribute>())
                        {
                            resolve(DependencyContainer, service);
                        }
                        else
                        {
                            Assert.Throws<ComponentResolutionException>(() => resolve(DependencyContainer, service));
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
            Assert.Equal(DependencyContainer.Resolve<ISingletonService>(),
                         DependencyContainer.Resolve<ISingletonService>());

            // 2 - resolve via concrete type
            Assert.Equal(DependencyContainer.Resolve<SingletonService>(),
                         DependencyContainer.Resolve<SingletonService>());

            // 3 - cross equals resolve
            Assert.Equal(DependencyContainer.Resolve<ISingletonService>(),
                         DependencyContainer.Resolve<SingletonService>());
        }

        [Fact]
        internal void TypedUntypedSingletonTest()
        {
            Assert.Equal(DependencyContainer.Resolve<ISingletonService>(),
                         DependencyContainer.Resolve(typeof(ISingletonService)));
        }

        [Fact]
        internal void OpenGenericTest()
        {
            Assert.Equal(typeof(OpenGenericTestService<string>), DependencyContainer.Resolve<IOpenGenericTestService<string>>().GetType());
            Assert.Equal(typeof(ClosedGenericImplementationOfOpenGenericService), DependencyContainer.Resolve<IOpenGenericTestService<ExternalResolvable>>().GetType());
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
            Assert.Equal(typeof(ExternalResolvable), DependencyContainer.Resolve<IProgress<ExternalResolvable>>().GetType());
            Assert.Equal(typeof(ExternalResolvableOpenGeneric<object>), DependencyContainer.Resolve<IProgress<object>>().GetType());
        }

        [Fact]
        internal void UnregisteredServiceResolveTest()
        {
            Assert.True(typeof(BaseUnregisteredService).HasAttribute<UnregisteredComponentAttribute>());

            Assert.True(typeof(DerivedFromUnregisteredService).IsSubclassOf(typeof(BaseUnregisteredService)));
            Assert.True(typeof(IUnregisteredService).IsAssignableFrom(typeof(DerivedFromUnregisteredService)));
            Assert.True(typeof(IUnregisteredService).IsAssignableFrom(typeof(BaseUnregisteredService)));

            Assert.Equal(typeof(DerivedFromUnregisteredService), DependencyContainer.Resolve<IUnregisteredService>().GetType());
        }

        [Fact]
        internal void UnregisteredExternalServiceResolveTest()
        {
            Assert.True(typeof(BaseUnregisteredExternalServiceImpl).HasAttribute<UnregisteredComponentAttribute>());

            Assert.True(typeof(DerivedFromUnregisteredExternalServiceImpl).IsSubclassOf(typeof(BaseUnregisteredExternalServiceImpl)));
            Assert.True(typeof(IUnregisteredExternalService).IsAssignableFrom(typeof(DerivedFromUnregisteredExternalServiceImpl)));
            Assert.True(typeof(IUnregisteredExternalService).IsAssignableFrom(typeof(BaseUnregisteredExternalServiceImpl)));

            Assert.Equal(typeof(DerivedFromUnregisteredExternalServiceImpl), DependencyContainer.Resolve<IUnregisteredExternalService>().GetType());
            Assert.Throws<ComponentResolutionException>(() => DependencyContainer.Resolve<DerivedFromUnregisteredExternalServiceImpl>().GetType());
            Assert.Throws<ComponentResolutionException>(() => DependencyContainer.Resolve<BaseUnregisteredExternalServiceImpl>().GetType());
        }

        [Fact]
        internal void ManualRegistrationResolutionTest()
        {
            var cctorResolutionBehavior = DependencyContainer.Resolve<IConstructorResolutionBehavior>();

            Assert.True(cctorResolutionBehavior.TryGetConstructor(typeof(WiredTestService), out var cctor));

            var parameterType = cctor.GetParameters().Single().ParameterType;

            Assert.Equal(typeof(IIndependentTestService), parameterType);

            var expectedCollection = new[]
                                     {
                                         typeof(CollectionResolvableTestServiceImpl1),
                                         typeof(CollectionResolvableTestServiceImpl2)
                                     };

            var registration = Fixture.DelegateRegistration(container =>
            {
                container
                    .Register<IWiredTestService, WiredTestService>(EnLifestyle.Transient)
                    .Register<WiredTestService, WiredTestService>(EnLifestyle.Transient)
                    .Register<IIndependentTestService, IndependentTestService>(EnLifestyle.Singleton)
                    .Register<IndependentTestService, IndependentTestService>(EnLifestyle.Singleton)
                    .Register<ConcreteImplementationWithDependencyService, ConcreteImplementationWithDependencyService>(EnLifestyle.Transient)
                    .Register<ConcreteImplementationService, ConcreteImplementationService>(EnLifestyle.Singleton)
                    .RegisterCollection<ICollectionResolvableTestService>(expectedCollection, EnLifestyle.Transient)
                    .Register<IOpenGenericTestService<object>, OpenGenericTestService<object>>(EnLifestyle.Transient)
                    .Register<OpenGenericTestService<object>, OpenGenericTestService<object>>(EnLifestyle.Transient);
            });

            var options = new DependencyContainerOptions().WithManualRegistrations(registration);

            var localContainer = Fixture.ExactlyBoundedContainer(options);

            localContainer.Resolve<IWiredTestService>();
            localContainer.Resolve<WiredTestService>();

            localContainer.Resolve<IIndependentTestService>();
            localContainer.Resolve<IndependentTestService>();

            localContainer.Resolve<ConcreteImplementationWithDependencyService>();

            localContainer.Resolve<ConcreteImplementationService>();

            var actual = localContainer
                .ResolveCollection<ICollectionResolvableTestService>()
                .Select(r => r.GetType())
                .ToList();

            Assert.True(expectedCollection.OrderByDependencyAttribute().SequenceEqual(expectedCollection.Reverse()));
            Assert.True(expectedCollection.OrderByDependencyAttribute().SequenceEqual(actual));

            localContainer.Resolve<IOpenGenericTestService<object>>();
            localContainer.Resolve<OpenGenericTestService<object>>();
            Assert.Throws<ComponentResolutionException>(() => localContainer.Resolve<IOpenGenericTestService<string>>());
        }

        private static Func<TypeArgumentSelectionContext, Type?> HybridTypeArgumentSelector(IDependencyContainer dependencyContainer)
        {
            return ctx =>
                FromBypassedTypes(ctx)
                ?? FromExistedClosedTypesTypeArgumentSelector(dependencyContainer.Resolve<ITypeProvider>().AllLoadedTypes, ctx)
                ?? FromMatchesTypeArgumentSelector(ctx);
        }

        private static Type? FromBypassedTypes(TypeArgumentSelectionContext ctx)
        {
            if ((ctx.OpenGeneric == typeof(IReadRepository<,>)
                 || ctx.OpenGeneric == typeof(ReadRepository<,>)
                 || ctx.OpenGeneric == typeof(IObjectTransformer<,>)
                 || ctx.OpenGeneric == typeof(EntityToPrimaryKeyObjectTransformer<,>)
                 || ctx.OpenGeneric == typeof(PrimaryKeyToEntityObjectTransformer<,>)
                 || ctx.OpenGeneric == typeof(IRepository<,>)
                 || ctx.OpenGeneric == typeof(IBulkRepository<,>)
                 || ctx.OpenGeneric == typeof(DataAccess.Orm.InMemoryDatabase.Persisting.Repository<,>)
                 || ctx.OpenGeneric == typeof(DataAccess.Orm.InMemoryDatabase.Persisting.BulkRepository<,>)
                 || ctx.OpenGeneric == typeof(DataAccess.Orm.Sql.Persisting.Repository<,>)
                 || ctx.OpenGeneric == typeof(DataAccess.Orm.Sql.Persisting.BulkRepository<,>))
                && ctx.TypeArgument.GenericParameterPosition == 1)
            {
                return ctx
                    .Resolved
                    .Single()
                    .UnwrapTypeParameter(typeof(IUniqueIdentified<>));
            }

            return default;
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