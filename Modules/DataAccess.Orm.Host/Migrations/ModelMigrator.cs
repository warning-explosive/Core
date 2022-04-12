namespace SpaceEngineers.Core.DataAccess.Orm.Host.Migrations
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Model;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Model;

    [Component(EnLifestyle.Singleton)]
    internal class ModelMigrator : IModelMigrator,
                                   IResolvable<IModelMigrator>
    {
        private readonly IDatabaseModelBuilder _databaseModelBuilder;
        private readonly ICodeModelBuilder _codeModelBuilder;
        private readonly IDatabaseTypeProvider _databaseTypeProvider;
        private readonly IModelValidator _modelValidator;
        private readonly IModelComparator _modelComparator;
        private readonly IModelChangesSorter _modelChangesSorter;
        private readonly IModelMigrationsExecutor _modelMigrationsExecutor;

        public ModelMigrator(
            IDatabaseModelBuilder databaseModelBuilder,
            ICodeModelBuilder codeModelBuilder,
            IDatabaseTypeProvider databaseTypeProvider,
            IModelValidator modelValidator,
            IModelComparator modelComparator,
            IModelChangesSorter modelChangesSorter,
            IModelMigrationsExecutor modelMigrationsExecutor)
        {
            _databaseModelBuilder = databaseModelBuilder;
            _codeModelBuilder = codeModelBuilder;
            _databaseTypeProvider = databaseTypeProvider;
            _modelValidator = modelValidator;
            _modelComparator = modelComparator;
            _modelChangesSorter = modelChangesSorter;
            _modelMigrationsExecutor = modelMigrationsExecutor;
        }

        public async Task Upgrade(
            IReadOnlyCollection<IManualMigration> manualMigrations,
            CancellationToken token)
        {
            await _modelMigrationsExecutor
               .ExecuteManualMigrations(manualMigrations, token)
               .ConfigureAwait(false);

            var actualModel = await _databaseModelBuilder
               .BuildModel(token)
               .ConfigureAwait(false);

            var databaseEntities = _databaseTypeProvider
               .DatabaseEntities()
               .ToList();

            var expectedModel = await _codeModelBuilder
               .BuildModel(databaseEntities, token)
               .ConfigureAwait(false);

            if (expectedModel != null)
            {
                _modelValidator.Validate(expectedModel);
            }

            var modelChanges = _modelChangesSorter
               .Sort(_modelComparator.ExtractDiff(actualModel, expectedModel))
               .ToList();

            await _modelMigrationsExecutor
               .ExecuteAutoMigrations(modelChanges, token)
               .ConfigureAwait(false);
        }
    }
}