namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Internals
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Core.DataAccess.Orm.Model.Abstractions;

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

            // TODO: generate migration
            // TODO: Apply migration (initial migration or regular migration)
        }
    }
}