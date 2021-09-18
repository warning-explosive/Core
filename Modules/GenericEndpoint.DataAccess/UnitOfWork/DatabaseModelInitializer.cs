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
        private readonly IDatabaseModelComparator _databaseModelComparator;

        public DatabaseModelInitializer(
            IDatabaseModelBuilder databaseModelBuilder,
            ICodeModelBuilder codeModelBuilder,
            IDatabaseModelComparator databaseModelComparator)
        {
            _databaseModelBuilder = databaseModelBuilder;
            _codeModelBuilder = codeModelBuilder;
            _databaseModelComparator = databaseModelComparator;
        }

        public async Task Initialize(CancellationToken token)
        {
            var actualModel = await _databaseModelBuilder
                .BuildModel(token)
                .ConfigureAwait(false);

            var expectedModel = await _codeModelBuilder
                .BuildModel(token)
                .ConfigureAwait(false);

            var modelChanges = _databaseModelComparator
                .ExtractDiff(actualModel, expectedModel)
                .ToList();

            // TODO: #110 - generate migration
            // TODO: #110 - Apply migration (initial migration or regular migration)
        }
    }
}