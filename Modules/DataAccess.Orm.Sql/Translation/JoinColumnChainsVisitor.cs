namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Collections.Generic;
    using System.Linq;
    using Basics;
    using Expressions;
    using Model;

    internal class JoinColumnChainsVisitor : IntermediateExpressionVisitorBase
    {
        private readonly IModelProvider _modelProvider;

        public JoinColumnChainsVisitor(IModelProvider modelProvider)
        {
            _modelProvider = modelProvider;
        }

        protected override IIntermediateExpression VisitSimpleBinding(SimpleBindingExpression simpleBindingExpression)
        {
            if (CanBeJoined(simpleBindingExpression, out var source, out var chain))
            {
                return new ColumnChainExpression(chain, source);
            }

            return simpleBindingExpression;
        }

        private bool CanBeJoined(
            SimpleBindingExpression simpleBindingExpression,
            out IIntermediateExpression source,
            out IReadOnlyCollection<SimpleBindingExpression> chain)
        {
            chain = FlattenBindingChain(simpleBindingExpression)
                .Reverse()
                .ToList();

            source = chain
                .First()
                .Source;

            return _modelProvider.Tables.TryGetValue(source.Type, out var info)
                   && info.Columns.TryGetValue(chain.ToString("_", binding => binding.Name), out var columnInfo)
                   && (columnInfo.Relation != null
                       || columnInfo.IsInlinedObject);

            static IEnumerable<SimpleBindingExpression> FlattenBindingChain(SimpleBindingExpression simpleBindingExpression)
            {
                yield return simpleBindingExpression;

                while (simpleBindingExpression.Source is SimpleBindingExpression sourceSimpleBindingExpression)
                {
                    yield return sourceSimpleBindingExpression;
                    simpleBindingExpression = sourceSimpleBindingExpression;
                }
            }
        }
    }
}