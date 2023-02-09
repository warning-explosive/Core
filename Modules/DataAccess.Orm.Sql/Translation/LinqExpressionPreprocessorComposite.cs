namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class LinqExpressionPreprocessorComposite : ILinqExpressionPreprocessorComposite,
                                                         IResolvable<ILinqExpressionPreprocessorComposite>
    {
        private readonly IEnumerable<ILinqExpressionPreprocessor> _preProcessors;

        public LinqExpressionPreprocessorComposite(IEnumerable<ILinqExpressionPreprocessor> preProcessors)
        {
            _preProcessors = preProcessors;
        }

        public Expression Visit(Expression expression)
        {
            return _preProcessors.Aggregate(expression, (acc, next) => next.Visit(acc));
        }
    }
}