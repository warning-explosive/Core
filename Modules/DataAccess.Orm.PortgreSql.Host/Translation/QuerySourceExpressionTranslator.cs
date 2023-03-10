namespace SpaceEngineers.Core.DataAccess.Orm.PostgreSql.Host.Translation
{
    using System;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using GenericHost.Api.Abstractions;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation;
    using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions;

    [Component(EnLifestyle.Singleton)]
    internal class QuerySourceExpressionTranslator : ISqlExpressionTranslator<QuerySourceExpression>,
                                                     IResolvable<ISqlExpressionTranslator<QuerySourceExpression>>,
                                                     ICollectionResolvable<ISqlExpressionTranslator>
    {
        private readonly IModelProvider _modelProvider;
        private readonly ISqlViewQueryProviderComposite _sqlViewQueryProvider;
        private readonly IHostStartupActionsRegistry _hostStartupActionsRegistry;
        private readonly IHostStartupAction _hostStartupAction;

        public QuerySourceExpressionTranslator(
            IDependencyContainer dependencyContainer,
            IModelProvider modelProvider,
            ISqlViewQueryProviderComposite sqlViewQueryProvider,
            IHostStartupActionsRegistry hostStartupActionsRegistry)
         : this(
             modelProvider,
             sqlViewQueryProvider,
             hostStartupActionsRegistry,
             (IHostStartupAction)dependencyContainer.Resolve(TypeExtensions.FindType("SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Host SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Host.StartupActions.UpgradeDatabaseHostStartupAction")))
        {
        }

        private QuerySourceExpressionTranslator(
            IModelProvider modelProvider,
            ISqlViewQueryProviderComposite sqlViewQueryProvider,
            IHostStartupActionsRegistry hostStartupActionsRegistry,
            IHostStartupAction hostStartupAction)
        {
            _modelProvider = modelProvider;
            _sqlViewQueryProvider = sqlViewQueryProvider;
            _hostStartupActionsRegistry = hostStartupActionsRegistry;
            _hostStartupAction = hostStartupAction;
        }

        public string Translate(ISqlExpression expression, int depth)
        {
            return expression is QuerySourceExpression querySourceExpression
                ? Translate(querySourceExpression, depth)
                : throw new NotSupportedException($"Unsupported sql expression type {expression.GetType()}");
        }

        public string Translate(QuerySourceExpression expression, int depth)
        {
            var sb = new StringBuilder();

            if (!MigrationsWasApplied() && expression.Type.IsSqlView())
            {
                sb.AppendLine("(");

                var sqlViewQueryRows = _sqlViewQueryProvider
                    .GetQuery(expression.Type)
                    .Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var row in sqlViewQueryRows)
                {
                    sb.Append(new string('\t', depth + 1));
                    sb.AppendLine(row);
                }

                sb.Append('\t');
                sb.Append(')');
            }
            else
            {
                sb.Append('"');
                sb.Append(_modelProvider.SchemaName(expression.Type));
                sb.Append('"');
                sb.Append('.');
                sb.Append('"');
                sb.Append(_modelProvider.TableName(expression.Type));
                sb.Append('"');
            }

            return sb.ToString();
        }

        private bool MigrationsWasApplied()
        {
            return _hostStartupActionsRegistry.Contains(_hostStartupAction);
        }
    }
}