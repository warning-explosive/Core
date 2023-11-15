namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Migrations.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Transaction;

    [Component(EnLifestyle.Singleton)]
    internal class ModelChangesExtractor : IModelChangesExtractor,
                                           IResolvable<IModelChangesExtractor>
    {
        private readonly IDatabaseModelBuilder _databaseModelBuilder;
        private readonly ICodeModelBuilder _codeModelBuilder;
        private readonly IModelValidator _modelValidator;
        private readonly IModelComparator _modelComparator;
        private readonly IModelChangesSorter _modelChangesSorter;

        public ModelChangesExtractor(
            IDatabaseModelBuilder databaseModelBuilder,
            ICodeModelBuilder codeModelBuilder,
            IModelValidator modelValidator,
            IModelComparator modelComparator,
            IModelChangesSorter modelChangesSorter)
        {
            _databaseModelBuilder = databaseModelBuilder;
            _codeModelBuilder = codeModelBuilder;
            _modelValidator = modelValidator;
            _modelComparator = modelComparator;
            _modelChangesSorter = modelChangesSorter;
        }

        public async Task<IReadOnlyCollection<IModelChange>> ExtractChanges(
            IAdvancedDatabaseTransaction transaction,
            IReadOnlyCollection<Type> databaseEntities,
            CancellationToken token)
        {
            var actualModel = await _databaseModelBuilder
               .BuildModel(transaction, token)
               .ConfigureAwait(false);

            var expectedModel = await _codeModelBuilder
               .BuildModel(databaseEntities, token)
               .ConfigureAwait(false);

            if (expectedModel != null)
            {
                _modelValidator.Validate(expectedModel);
            }

            return _modelChangesSorter
               .Sort(_modelComparator.ExtractDiff(actualModel, expectedModel))
               .ToList();
        }
    }
}