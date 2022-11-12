namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Translation
{
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using GenericHost.Api.Abstractions;
    using Sql.Model;
    using Sql.Translation;
    using Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class QuerySourceExpressionTranslatorDecorator : IExpressionTranslator<QuerySourceExpression>,
                                                              IDecorator<IExpressionTranslator<QuerySourceExpression>>
    {
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IHostStartupActionsRegistry _hostStartupActionsRegistry;

        private bool _migrationsWasApplied;

        public QuerySourceExpressionTranslatorDecorator(
            IExpressionTranslator<QuerySourceExpression> decoratee,
            IDependencyContainer dependencyContainer,
            IHostStartupActionsRegistry hostStartupActionsRegistry)
        {
            Decoratee = decoratee;

            _dependencyContainer = dependencyContainer;
            _hostStartupActionsRegistry = hostStartupActionsRegistry;
            _migrationsWasApplied = false;
        }

        public IExpressionTranslator<QuerySourceExpression> Decoratee { get; }

        public string Translate(QuerySourceExpression expression, int depth)
        {
            if (!MigrationsWasApplied() && expression.Type.IsSqlView())
            {
                var sb = new StringBuilder();

                sb.Append('(');
                sb.Append(expression.Type.SqlViewQuery(_dependencyContainer));
                sb.Append(')');

                return sb.ToString();
            }

            return Decoratee.Translate(expression, depth);
        }

        private bool MigrationsWasApplied()
        {
            if (_migrationsWasApplied)
            {
                return true;
            }

            var upgradeDatabaseHostStartupActionType = TypeExtensions.FindType("SpaceEngineers.Core.GenericEndpoint.DataAccess.Host SpaceEngineers.Core.GenericEndpoint.DataAccess.Host.StartupActions.UpgradeDatabaseHostStartupAction");
            var upgradeDatabaseHostStartupAction = (IHostStartupAction)_dependencyContainer.Resolve(upgradeDatabaseHostStartupActionType);

            _migrationsWasApplied = _hostStartupActionsRegistry.Contains(upgradeDatabaseHostStartupAction);

            return _migrationsWasApplied;
        }
    }
}