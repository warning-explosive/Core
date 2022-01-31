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
        private readonly IModelValidator _modelValidator;
        private readonly IModelComparator _modelComparator;
        private readonly IModelChangesSorter _modelChangesSorter;
        private readonly IModelMigrator _modelMigrator;

        public DatabaseModelInitializer(
            IDatabaseModelBuilder databaseModelBuilder,
            ICodeModelBuilder codeModelBuilder,
            IModelValidator modelValidator,
            IModelComparator modelComparator,
            IModelChangesSorter modelChangesSorter,
            IModelMigrator modelMigrator)
        {
            _databaseModelBuilder = databaseModelBuilder;
            _codeModelBuilder = codeModelBuilder;
            _modelValidator = modelValidator;
            _modelComparator = modelComparator;
            _modelChangesSorter = modelChangesSorter;
            _modelMigrator = modelMigrator;
        }

        public async Task Initialize(CancellationToken token)
        {
            await _modelMigrator
                .ExecuteManualMigrations(token)
                .ConfigureAwait(false);

            var actualModel = await _databaseModelBuilder
                .BuildModel(token)
                .ConfigureAwait(false);

            var expectedModel = await _codeModelBuilder
                .BuildModel(token)
                .ConfigureAwait(false);

            if (expectedModel != null)
            {
                _modelValidator.Validate(expectedModel);
            }

            var modelChanges = _modelChangesSorter
                .Sort(_modelComparator.ExtractDiff(actualModel, expectedModel))
                .ToList();

            await _modelMigrator
                .ExecuteAutoMigrations(modelChanges, token)
                .ConfigureAwait(false);
        }
    }
}