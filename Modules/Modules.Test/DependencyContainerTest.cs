namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using AutoWiringApi.Abstractions;
    using AutoWiringApi.Attributes;
    using AutoWiringTest;
    using Basics;
    using CompositionInfoExtractor;
    using SettingsManager.Abstractions;
    using SimpleInjector;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Sdk;
    using TypeExtensions = Basics.TypeExtensions;

    /// <summary>
    /// DependencyContainer class tests
    /// </summary>
    public class DependencyContainerTest : ModulesTestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        public DependencyContainerTest(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        internal void IsOurTypeTest()
        {
            var ourTypes = DependencyContainer.Resolve<ITypeExtensions>()
                                              .OurTypes();

            var wrongOurTypes = ourTypes
                               .Where(t => !t.FullName?.StartsWith(nameof(SpaceEngineers), StringComparison.InvariantCulture) ?? true)
                               .ToArray();

            wrongOurTypes.Each(t => Output.WriteLine(t.FullName));
            Assert.False(wrongOurTypes.Any(), Show(wrongOurTypes));

            wrongOurTypes = AssembliesExtensions.AllFromCurrentDomain()
                                                .SelectMany(asm => asm.GetTypes())
                                                .Except(ourTypes)
                                                .Where(type => type.IsOurType())
                                                .ToArray();

            Assert.False(wrongOurTypes.Any(), Show(wrongOurTypes));

            string Show(IEnumerable<Type> types) => string.Join(Environment.NewLine, types.Select(t => t.FullName));
        }

        [Fact]
        internal void DependencyContainerSelfResolveTest()
        {
            var container = DependencyContainer.Resolve<IDependencyContainer>();

            Assert.True(ReferenceEquals(container, DependencyContainer));
            Assert.True(container.Equals(DependencyContainer));

            container = DependencyContainer.Resolve<IWithInjectedDependencyContainer>().InjectedDependencyContainer;

            Assert.True(ReferenceEquals(container, DependencyContainer));
            Assert.True(container.Equals(DependencyContainer));
        }

        [Fact]
        internal void SingletonTest()
        {
            Assert.Equal(DependencyContainer.Resolve<ISingletonTestService>(),
                         DependencyContainer.Resolve<ISingletonTestService>());
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
        internal void DecoratorTest()
        {
            var service = DependencyContainer.Resolve<IDecorableService>();

            var types = new Dictionary<Type, Type>
            {
                [typeof(DecorableServiceDecorator1)] = typeof(DecorableServiceDecorator2),
                [typeof(DecorableServiceDecorator2)] = typeof(DecorableServiceDecorator3),
                [typeof(DecorableServiceDecorator3)] = typeof(DecorableServiceImpl),
            };

            void CheckRecursive(IDecorableService resolved, Type type)
            {
                Assert.True(resolved.GetType() == type);
                Output.WriteLine(type.Name);

                if (types.TryGetValue(type, out var nextDecorateeType))
                {
                    var decorator = (IDecorableServiceDecorator)resolved;
                    CheckRecursive(decorator.Decoratee, nextDecorateeType);
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
                [typeof(OpenGenericDecorableServiceDecorator3<object>)] = typeof(OpenGenericDecorableServiceImpl<object>),
            };

            void CheckRecursive(IOpenGenericDecorableService<object> resolved, Type type)
            {
                Assert.True(resolved.GetType() == type);
                Output.WriteLine(type.Name);

                if (types.TryGetValue(type, out var nextDecorateeType))
                {
                    var decorator = (IOpenGenericDecorableServiceDecorator<object>)resolved;
                    CheckRecursive(decorator.Decoratee, nextDecorateeType);
                }
            }

            CheckRecursive(service, typeof(OpenGenericDecorableServiceDecorator1<object>));
        }

        [Fact]
        internal void ConditionalDecoratorTest()
        {
            var service = DependencyContainer.Resolve<IConditionalDecorableService>();

            var types = new Dictionary<Type, Type>
            {
                [typeof(ConditionalDecorableServiceDecorator1)] = typeof(ConditionalDecorableServiceDecorator3),
                [typeof(ConditionalDecorableServiceDecorator3)] = typeof(ConditionalDecorableServiceImpl),
            };

            void CheckRecursive(IConditionalDecorableService resolved, Type type)
            {
                Assert.True(resolved.GetType() == type);
                Output.WriteLine(type.Name);

                if (types.TryGetValue(type, out var nextDecorateeType))
                {
                    var decorator = (IConditionalDecorableServiceDecorator)resolved;
                    CheckRecursive(decorator.Decoratee, nextDecorateeType);
                }
            }

            CheckRecursive(service, typeof(ConditionalDecorableServiceDecorator1));
        }

        [Fact]
        internal void CollectionResolvableConditionDecorableTest()
        {
            var services = DependencyContainer.ResolveCollection<ICollectionResolvableConditionDecorableService>()
                                              .ToArray();

            var types = new[]
                        {
                            new Dictionary<Type, Type>
                            {
                                [typeof(CollectionResolvableConditionDecorableServiceDecorator3)] = typeof(CollectionResolvableConditionDecorableServiceImpl3)
                            },
                            new Dictionary<Type, Type>
                            {
                                [typeof(CollectionResolvableConditionDecorableServiceDecorator3)] = typeof(CollectionResolvableConditionDecorableServiceDecorator2),
                                [typeof(CollectionResolvableConditionDecorableServiceDecorator2)] = typeof(CollectionResolvableConditionDecorableServiceImpl2),
                            },
                            new Dictionary<Type, Type>
                            {
                                [typeof(CollectionResolvableConditionDecorableServiceDecorator3)] = typeof(CollectionResolvableConditionDecorableServiceDecorator1),
                                [typeof(CollectionResolvableConditionDecorableServiceDecorator1)] = typeof(CollectionResolvableConditionDecorableServiceImpl1),
                            }
                        };

            void CheckRecursive(ICollectionResolvableConditionDecorableService resolved, int i, Type type)
            {
                Assert.True(resolved.GetType() == type);
                Output.WriteLine(type.Name);

                if (types[i].TryGetValue(type, out var nextDecorateeType))
                {
                    var decorator = (ICollectionResolvableConditionDecorableServiceDecorator)resolved;

                    CheckRecursive(decorator.Decoratee, i, nextDecorateeType);
                }
            }

            CheckRecursive(services[0], 0, typeof(CollectionResolvableConditionDecorableServiceDecorator3));
            Output.WriteLine(string.Empty);
            CheckRecursive(services[1], 1, typeof(CollectionResolvableConditionDecorableServiceDecorator3));
            Output.WriteLine(string.Empty);
            CheckRecursive(services[2], 2, typeof(CollectionResolvableConditionDecorableServiceDecorator3));
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

            Assert.Null(typeof(DerivedUnregisteredServiceImpl).GetCustomAttribute<UnregisteredAttribute>(false));
            Assert.NotNull(typeof(DerivedUnregisteredServiceImpl).GetCustomAttribute<LifestyleAttribute>(false));

            Assert.True(typeof(DerivedUnregisteredServiceImpl).IsSubclassOf(typeof(BaseUnregisteredServiceImpl)));
            Assert.True(typeof(IUnregisteredService).IsAssignableFrom(typeof(DerivedUnregisteredServiceImpl)));
            Assert.True(typeof(IUnregisteredService).IsAssignableFrom(typeof(BaseUnregisteredServiceImpl)));

            Assert.Throws<ActivationException>(() => DependencyContainer.Resolve<IUnregisteredService>());
        }

        [Fact]
        internal void UnregisteredExternalServiceResolveTest()
        {
            Assert.NotNull(typeof(BaseUnregisteredExternalServiceImpl).GetCustomAttribute<UnregisteredAttribute>(false));
            Assert.Null(typeof(BaseUnregisteredExternalServiceImpl).GetCustomAttribute<LifestyleAttribute>(false));

            Assert.Null(typeof(DerivedUnregisteredExternalServiceImpl).GetCustomAttribute<UnregisteredAttribute>(false));
            Assert.NotNull(typeof(DerivedUnregisteredExternalServiceImpl).GetCustomAttribute<LifestyleAttribute>(false));

            Assert.True(typeof(DerivedUnregisteredExternalServiceImpl).IsSubclassOf(typeof(BaseUnregisteredExternalServiceImpl)));
            Assert.True(typeof(IUnregisteredExternalService).IsAssignableFrom(typeof(DerivedUnregisteredExternalServiceImpl)));
            Assert.True(typeof(IUnregisteredExternalService).IsAssignableFrom(typeof(BaseUnregisteredExternalServiceImpl)));

            Assert.Throws<ActivationException>(() => DependencyContainer.Resolve<IUnregisteredExternalService>());
        }

        [Fact]
        internal async void AsyncScopeTest()
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [SuppressMessage("Using IDisposables", "CA1508", Justification = "False positive")]
        internal void SlimContainerTest(bool mode)
        {
            var settingsContainer = AutoRegistration.DependencyContainer
                                                    .CreateBounded(new[]
                                                                   {
                                                                       typeof(ISettingsManager<>).Assembly,
                                                                       typeof(ICompositionInfoExtractor).Assembly,
                                                                   },
                                                                   new DependencyContainerOptions());

            DependencyInfo[] compositionInfo;

            // TODO: Intercept resolution of ITypeExtensions
            compositionInfo = settingsContainer.Resolve<ICompositionInfoExtractor>()
                                               .GetCompositionInfo(mode)
                                               .ToArray();

            Output.WriteLine($"Total: {compositionInfo.Length}\n");

            Output.WriteLine(settingsContainer.Resolve<ICompositionInfoInterpreter<string>>()
                                              .Visualize(compositionInfo));

            var allowedAssemblies = new[]
                                    {
                                        typeof(Container).Assembly,                 // SimpleInjector assembly,
                                        typeof(ITypeExtensions).Assembly,           // Basics assembly
                                        typeof(LifestyleAttribute).Assembly,        // AutoWiringApi assembly
                                        typeof(IDependencyContainer).Assembly,      // AutoRegistration assembly
                                        typeof(ISettingsManager<>).Assembly,        // SettingsManager assembly
                                        typeof(ICompositionInfoExtractor).Assembly, // CompositionInfoExtractor assembly
                                    };

            Assert.True(compositionInfo.All(Satisfy));

            bool Satisfy(DependencyInfo info)
            {
                return allowedAssemblies.Contains(info.ServiceType.Assembly)
                    && allowedAssemblies.Contains(info.ComponentType.Assembly)
                    && info.Dependencies.All(Satisfy);
            }
        }

        private class TypeExtensionsDecorator : ITypeExtensions
        {
            private readonly ITypeExtensions _decoratee;

            public TypeExtensionsDecorator(ITypeExtensions decoratee)
            {
                _decoratee = decoratee;
            }

            public IOrderedEnumerable<T> OrderByDependencies<T>(IEnumerable<T> source, Func<T, Type> accessor)
            {
                return _decoratee.OrderByDependencies(source, accessor);
            }

            public Type[] AllOurServicesThatContainsDeclarationOfInterface<TInterface>()
                where TInterface : class
            {
                return _decoratee.AllOurServicesThatContainsDeclarationOfInterface<TInterface>();
            }

            public Type[] AllLoadedTypes()
            {
                return _decoratee.AllLoadedTypes();
            }

            public Type[] OurTypes()
            {
                return _decoratee.OurTypes()
                                 .Concat(new[] { typeof(TestYamlConfig) })
                                 .ToArray();
            }

            public Assembly[] OurAssemblies()
            {
                return _decoratee.OurAssemblies();
            }

            public bool IsOurType(Type type)
            {
                return _decoratee.IsOurType(type);
            }

            public Type[] GetDependenciesByAttribute(Type type)
            {
                return _decoratee.GetDependenciesByAttribute(type);
            }

            public bool IsNullable(Type type)
            {
                return _decoratee.IsNullable(type);
            }

            public bool IsSubclassOfOpenGeneric(Type type, Type openGenericAncestor)
            {
                return _decoratee.IsSubclassOfOpenGeneric(type, openGenericAncestor);
            }

            public bool IsContainsInterfaceDeclaration(Type type, Type i)
            {
                return _decoratee.IsContainsInterfaceDeclaration(type, i);
            }

            public bool FitsForTypeArgument(Type typeForCheck, Type typeArgument)
            {
                return _decoratee.FitsForTypeArgument(typeForCheck, typeArgument);
            }

            public IEnumerable<Type> GetGenericArgumentsOfOpenGenericAt(Type derived, Type openGeneric, int typeArgumentAt = 0)
            {
                return _decoratee.GetGenericArgumentsOfOpenGenericAt(derived, openGeneric, typeArgumentAt);
            }

            public Type ExtractGenericTypeDefinition(Type type)
            {
                return _decoratee.ExtractGenericTypeDefinition(type);
            }
        }

        private class TestYamlConfig : IYamlSettings
        {
        }
    }
}