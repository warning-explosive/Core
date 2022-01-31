namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CompositionRoot.Api.Abstractions.Container;
    using Model;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class InitialMigration : IManualMigration, ICollectionResolvable<IManualMigration>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDatabaseModelBuilder _databaseModelBuilder;
        private readonly ICodeModelBuilder _codeModelBuilder;
        private readonly IModelValidator _modelValidator;
        private readonly IModelChangesSorter _modelChangesSorter;
        private readonly IModelComparator _modelComparator;

        public InitialMigration(
            IDependencyContainer dependencyContainer,
            IDatabaseModelBuilder databaseModelBuilder,
            ICodeModelBuilder codeModelBuilder,
            IModelValidator modelValidator,
            IModelComparator modelComparator,
            IModelChangesSorter modelChangesSorter)
        {
            _dependencyContainer = dependencyContainer;
            _databaseModelBuilder = databaseModelBuilder;
            _codeModelBuilder = codeModelBuilder;
            _modelValidator = modelValidator;
            _modelChangesSorter = modelChangesSorter;
            _modelComparator = modelComparator;
        }

        public string Name { get; } = "Initial migration";

        public async Task ExecuteManualMigration(CancellationToken token)
        {
            var actualModel = await _databaseModelBuilder
                .BuildModel(token)
                .ConfigureAwait(false);

            var databaseEntities = new[]
            {
                typeof(AppliedMigration)
            };

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

            await _dependencyContainer
                .Resolve<IModelMigrator>()
                .ExecuteAutoMigrations(modelChanges, token)
                .ConfigureAwait(false);
        }
    }
}