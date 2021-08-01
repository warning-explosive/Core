namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration;
    using AutoRegistration.Abstractions;
    using Basics;
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

            DependencyContainer = fixture.BoundedAboveContainer(new DependencyContainerOptions(), assembly);
        }

        /// <summary>
        /// DependencyContainer
        /// </summary>
        protected IDependencyContainer DependencyContainer { get; }

        /// <summary> DynamicClass test data member </summary>
        /// <returns>Test data</returns>
        public static IEnumerable<object[]> DynamicClassTestData()
        {
            var properties = new[]
            {
                new DynamicProperty(typeof(bool), nameof(Boolean)),
                new DynamicProperty(typeof(int), nameof(Int32)),
                new DynamicProperty(typeof(string), nameof(String))
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
                new Action<Type>(type =>
                {
                    Assert.Equal(typeof(object), type.BaseType);
                    Assert.Empty(type.GetInterfaces());
                    Assert.Empty(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                    Assert.Empty(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty));
                })
            };
            yield return new object[]
            {
                new Func<DynamicClass>(() => new DynamicClass().InheritsFrom(typeof(TestBaseClass))),
                new Action<Type>(type =>
                {
                    Assert.Equal(typeof(TestBaseClass), type.BaseType);
                    Assert.Empty(type.GetInterfaces());
                    Assert.Empty(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                    Assert.Empty(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty));
                })
            };
            yield return new object[]
            {
                new Func<DynamicClass>(() => new DynamicClass().Implements(typeof(ITestInterface))),
                new Action<Type>(type =>
                {
                    Assert.Equal(typeof(object), type.BaseType);
                    Assert.Contains(typeof(ITestInterface), type.GetInterfaces());
                    Assert.Empty(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                    Assert.Empty(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty));
                })
            };
            yield return new object[]
            {
                new Func<DynamicClass>(() => new DynamicClass().HasProperties(properties)),
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
                })
            };
            yield return new object[]
            {
                new Func<DynamicClass>(() => new DynamicClass().InheritsFrom(typeof(TestBaseClass)).Implements(typeof(ITestInterface)).HasProperties(properties)),
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
                })
            };
        }

        [Theory]
        [MemberData(nameof(DynamicClassTestData))]
        internal void CacheTest(Func<DynamicClass> factory, Action<Type> assert)
        {
            var provider = DependencyContainer.Resolve<IDynamicClassProvider>();

            var type = provider.Create(factory());

            type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
                .Each(field => Output.WriteLine($"{field.Name} ({field.PropertyType.Name})"));

            type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Each(field => Output.WriteLine($"{field.Name} ({field.FieldType.Name})"));

            assert(type);

            Assert.Equal(type, provider.Create(factory()));
            Assert.Equal(type, Activator.CreateInstance(type).GetType());
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