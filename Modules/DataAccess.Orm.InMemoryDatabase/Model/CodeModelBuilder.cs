namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Model
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Orm.Model;

    [Component(EnLifestyle.Singleton)]
    internal class CodeModelBuilder : ICodeModelBuilder
    {
        public Task<DatabaseNode?> BuildModel(CancellationToken token)
        {
            return Task.FromResult(default(DatabaseNode));
        }
    }
}