namespace SpaceEngineers.Core.Modules.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AutoRegistration;
    using Basics;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using CrossCuttingConcerns;
    using DataAccess.Contract.Abstractions;
    using DataAccess.PostgreSql.Settings;
    using GenericDomain;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// DataAccess assemblies test
    /// </summary>
    public class DataAccessTest : TestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">ModulesTestFixture</param>
        public DataAccessTest(ITestOutputHelper output, ModulesTestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// DataAccessTestData member
        /// </summary>
        /// <returns>Test data</returns>
        public static IEnumerable<object> DataAccessTestData()
        {
            var assemblyName = AssembliesExtensions.BuildName(
                nameof(SpaceEngineers),
                nameof(Core),
                nameof(DataAccess),
                nameof(DataAccess.Orm),
                nameof(DataAccess.Orm.PostgreSql));
            var typeName = AssembliesExtensions.BuildName(assemblyName, "QueryTranslator");
            var type = AssembliesExtensions.FindRequiredType(assemblyName, typeName);

            var assemblies = new[]
            {
                type.Assembly, // DataAccess.Orm.PostgreSql
                typeof(PostgreSqlSettings).Assembly, // DataAccess.PostgreSql
                typeof(CrossCuttingConcernsManualRegistration).Assembly, // CrossCuttingConcerns
            };

            yield return new object[] { nameof(DataAccess.Orm.PostgreSql), assemblies };
        }

        [Theory]
        [MemberData(nameof(DataAccessTestData))]
        internal void ReadRepositoryQueryAllTest(string name, Assembly[] assemblies)
        {
            Output.WriteLine($"{nameof(ReadRepositoryQueryAllTest)}: {name}");

            var options = new DependencyContainerOptions();

            var dependencyContainer = Fixture.BoundedAboveContainer(options, assemblies);

            using (dependencyContainer.OpenScope())
            {
                var readRepository = dependencyContainer.Resolve<IReadRepository<TestEntity>>();

                _ = readRepository
                    .All()
                    .Select(entity => entity.Id)
                    .ToList();

                /* TODO: IAsyncQueryable extensions
                _ = await readRepository
                    .All()
                    .Select(entity => entity.Id)
                    .ToListAsync()
                    .ConfigureAwait(false);
                */
            }
        }

        private class TestEntity : EntityBase
        {
        }
    }
}