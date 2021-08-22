namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Basics;
    using CompositionRoot;
    using CompositionRoot.Api.Abstractions;
    using CompositionRoot.SimpleInjector;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using Dynamic.Api;
    using Dynamic.Api.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Test for IDynamicClassProvider
    /// </summary>
    public class DynamicClassProviderTest : TestBase
    {
        /// <summary> .cctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public DynamicClassProviderTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
            var assembly = AssembliesExtensions.FindRequiredAssembly(AssembliesExtensions.BuildName(nameof(SpaceEngineers), nameof(Core), nameof(Core.Dynamic)));

            var options = new DependencyContainerOptions();

            DependencyContainer = fixture.BoundedAboveContainer(options, options.UseSimpleInjector(), assembly);
        }

        /// <summary>
        /// DependencyContainer
        /// </summary>
        protected IDependencyContainer DependencyContainer { get; }

        /// <summary> DynamicClass test data member </summary>
        /// <returns>Test data</returns>
        public static IEnumerable<object[]> DynamicClassTestData()
        {
            var emptyPropertyValues = new Dictionary<DynamicProperty, object?>();

            var propertyValues = new Dictionary<DynamicProperty, object?>
            {
                [new DynamicProperty(typeof(bool), nameof(Boolean))] = true,
                [new DynamicProperty(typeof(int), nameof(Int32))] = 42,
                [new DynamicProperty(typeof(string), nameof(String))] = "qwerty"
            };

            var expectedFields = new[]
            {
                ("_boolean", typeof(bool)),
                ("_int32", typeof(int)),
                ("_string", typeof(string)),
            };

            var expectedProperties = new[]
            {
                (nameof(Boolean), typeof(bool)),
                (nameof(Int32), typeof(int)),
                (nameof(String), typeof(string)),
            };

            yield return new object[]
            {
                new Func<DynamicClass>(() => new DynamicClass()),
                emptyPropertyValues,
                new Action<Type>(type =>
                {
                    Assert.Equal(typeof(object), type.BaseType);
                    Assert.Empty(type.GetInterfaces());
                    Assert.Empty(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                    Assert.Empty(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty));
                }),
                new Action<object>(Assert.NotNull)
            };
            yield return new object[]
            {
                new Func<DynamicClass>(() => new DynamicClass().InheritsFrom(typeof(TestBaseClass))),
                emptyPropertyValues,
                new Action<Type>(type =>
                {
                    Assert.Equal(typeof(TestBaseClass), type.BaseType);
                    Assert.Empty(type.GetInterfaces());
                    Assert.Empty(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                    Assert.Empty(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty));
                }),
                new Action<object>(Assert.NotNull)
            };
            yield return new object[]
            {
                new Func<DynamicClass>(() => new DynamicClass().Implements(typeof(ITestInterface))),
                emptyPropertyValues,
                new Action<Type>(type =>
                {
                    Assert.Equal(typeof(object), type.BaseType);
                    Assert.Contains(typeof(ITestInterface), type.GetInterfaces());
                    Assert.Empty(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                    Assert.Empty(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty));
                }),
                new Action<object>(Assert.NotNull)
            };
            yield return new object[]
            {
                new Func<DynamicClass>(() => new DynamicClass().HasProperties(propertyValues.Keys.ToArray())),
                propertyValues,
                new Action<Type>(type =>
                {
                    Assert.Equal(typeof(object), type.BaseType);
                    Assert.Empty(type.GetInterfaces());

                    var actualFields = type
                        .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Select(field => (field.Name, field.FieldType))
                        .OrderBy(field => field.Name);

                    Assert.True(expectedFields.SequenceEqual(actualFields));

                    var actualProperties = type
                        .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
                        .Select(property => (property.Name, property.PropertyType))
                        .OrderBy(property => property.Name);

                    Assert.True(expectedProperties.SequenceEqual(actualProperties));
                }),
                new Action<object>(instance =>
                {
                    Assert.NotNull(instance);

                    foreach (var (property, value) in propertyValues)
                    {
                        Assert.Equal(value, instance.GetPropertyValue(property.Name));
                    }
                })
            };
            yield return new object[]
            {
                new Func<DynamicClass>(() => new DynamicClass().InheritsFrom(typeof(TestBaseClass)).Implements(typeof(ITestInterface)).HasProperties(propertyValues.Keys.ToArray())),
                propertyValues,
                new Action<Type>(type =>
                {
                    Assert.Equal(typeof(TestBaseClass), type.BaseType);
                    Assert.Contains(typeof(ITestInterface), type.GetInterfaces());
                    Assert.Single(type.GetInterfaces());

                    var actualFields = type
                        .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Select(field => (field.Name, field.FieldType))
                        .OrderBy(field => field.Name);

                    Assert.True(expectedFields.SequenceEqual(actualFields));

                    var actualProperties = type
                        .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
                        .Select(property => (property.Name, property.PropertyType))
                        .OrderBy(property => property.Name);

                    Assert.True(expectedProperties.SequenceEqual(actualProperties));
                }),
                new Action<object>(instance =>
                {
                    Assert.NotNull(instance);

                    foreach (var (property, value) in propertyValues)
                    {
                        Assert.Equal(value, instance.GetPropertyValue(property.Name));
                    }
                })
            };
        }

        [Theory]
        [MemberData(nameof(DynamicClassTestData))]
        internal void CacheTest(
            Func<DynamicClass> factory,
            IReadOnlyDictionary<DynamicProperty, object?> values,
            Action<Type> assertType,
            Action<object> assertInstance)
        {
            var provider = DependencyContainer.Resolve<IDynamicClassProvider>();

            var type = provider.CreateType(factory());
            var instance = provider.CreateInstance(factory(), values);

            type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
                .Each(property => Output.WriteLine($"{property.Name} ({property.PropertyType.Name}) - {property.GetValue(instance)?.ToString() ?? "null"}"));

            type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Each(field => Output.WriteLine($"{field.Name} ({field.FieldType.Name}) - {field.GetValue(instance)?.ToString() ?? "null"}"));

            assertType(type);
            assertInstance(instance);

            Assert.Equal(type, provider.CreateType(factory()));
            Assert.Equal(type, Activator.CreateInstance(type).GetType());
            Assert.Equal(type, provider.CreateInstance(factory(), values).GetType());
        }

        /// <summary>
        /// ITestInterface
        /// </summary>
        [SuppressMessage("Analysis", "CA1034", Justification = "for test reasons")]
        [SuppressMessage("Analysis", "SA1201", Justification = "for test reasons")]
        public interface ITestInterface
        {
        }

        /// <summary>
        /// TestBaseClass
        /// </summary>
        [SuppressMessage("Analysis", "CA1034", Justification = "for test reasons")]
        public class TestBaseClass
        {
        }
    }
}