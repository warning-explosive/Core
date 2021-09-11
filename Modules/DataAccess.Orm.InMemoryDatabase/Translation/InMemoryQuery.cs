namespace SpaceEngineers.Core.DataAccess.Orm.InMemoryDatabase.Translation
{
    using System.Linq.Expressions;
    using Linq;

    internal class InMemoryQuery : IQuery
    {
        public InMemoryQuery(Expression expression)
        {
            Expression = expression;
        }

        public Expression Expression { get; }
    }
}