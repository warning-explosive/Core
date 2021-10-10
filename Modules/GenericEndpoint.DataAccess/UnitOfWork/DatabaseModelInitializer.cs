namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.UnitOfWork
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Core.DataAccess.Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class DatabaseModelInitializer : IEndpointInitializer,
                                              ICollectionResolvable<IEndpointInitializer>
    {
        private readonly IDatabaseModelBuilder _databaseModelBuilder;
        private readonly ICodeModelBuilder _codeModelBuilder;
        private readonly IModelComparator _modelComparator;
        private readonly IModelMigrator _modelMigrator;

        public DatabaseModelInitializer(
            IDatabaseModelBuilder databaseModelBuilder,
            ICodeModelBuilder codeModelBuilder,
            IModelComparator modelComparator,
            IModelMigrator modelMigrator)
        {
            _databaseModelBuilder = databaseModelBuilder;
            _codeModelBuilder = codeModelBuilder;
            _modelComparator = modelComparator;
            _modelMigrator = modelMigrator;
        }

        public async Task Initialize(CancellationToken token)
        {
            var actualModel = await _databaseModelBuilder
                .BuildModel(token)
                .ConfigureAwait(false);

            var expectedModel = await _codeModelBuilder
                .BuildModel(token)
                .ConfigureAwait(false);

            var modelChanges = _modelComparator
                .ExtractDiff(actualModel, expectedModel)
                .ToList();

            await _modelMigrator
                .Migrate(modelChanges, token)
                .ConfigureAwait(false);
        }
    }
}