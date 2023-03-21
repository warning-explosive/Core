namespace SpaceEngineers.Core.CompositionRoot.Test
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
    using Exceptions;
    using Registration;
    using Registrations;
    using SpaceEngineers.Core.Test.Api;
    using SpaceEngineers.Core.Test.Api.ClassFixtures;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// DependencyContainerResolveTest
    /// </summary>
    public class DependencyContainerResolveTest : TestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        public DependencyContainerResolveTest(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
            var assemblies = new[]
            {
                AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(CompositionRoot), nameof(Test))),
            };

            var options = new DependencyContainerOptions()
               .WithManualRegistrations(new ManuallyRegisteredServiceManualRegistration())
               .WithManualRegistrations(fixture.DelegateRegistration(container =>
               {
                   container.RegisterInstance(new ConcreteImplementationGenericService<string>());
               }));

            DependencyContainer = fixture.BoundedAboveContainer(output, options, assemblies);
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
                    typeof(IResolvable<>),
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
                   .Where(t => t.IsSubclassOfOpenGeneric(typeof(IResolvable<>)))
                   .SelectMany(t => t.ExtractGenericArgumentsAt(typeof(IResolvable<>)))
                   .Where(type => !type.IsGenericParameter && type.HasAttribute<ComponentAttribute>())
                   .Select(type => type.GenericTypeDefinitionOrSelf())
                   .Distinct();
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

                        var service = !type.IsConstructedOrNonGenericType()
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

            container = DependencyContainer.Resolve<IWithInjectedDependencyContainer>().DependencyContainer;

            Assert.True(ReferenceEquals(container, DependencyContainer));
            Assert.True(container.Equals(DependencyContainer));
        }

        [Fact]
        internal void SingletonTest()
        {
            // 1 - resolve via service type
            Assert.Equal(DependencyContainer.Resolve<ISingletonService>(),
                         DependencyContainer.Resolve<ISingletonService>());

            // 2 - resolve via concrete type
            Assert.Throws<ComponentResolutionException>(() => DependencyContainer.Resolve<SingletonService>());
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
        internal void ResolvableInstanceTest()
        {
            Assert.NotNull(DependencyContainer.Resolve<ConcreteImplementationGenericService<string>>());
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
                    .RegisterCollectionEntry<ICollectionResolvableTestService, CollectionResolvableTestServiceImpl1>(EnLifestyle.Transient)
                    .RegisterCollectionEntry<ICollectionResolvableTestService, CollectionResolvableTestServiceImpl2>(EnLifestyle.Transient)
                    .Register<IOpenGenericTestService<object>, OpenGenericTestService<object>>(EnLifestyle.Transient)
                    .Register<OpenGenericTestService<object>, OpenGenericTestService<object>>(EnLifestyle.Transient);
            });

            var options = new DependencyContainerOptions().WithManualRegistrations(registration);

            var localContainer = Fixture.ExactlyBoundedContainer(Output, options);

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

            Assert.True(expectedCollection.OrderByDependencies().SequenceEqual(expectedCollection.Reverse()));
            Assert.True(expectedCollection.OrderByDependencies().SequenceEqual(actual));

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