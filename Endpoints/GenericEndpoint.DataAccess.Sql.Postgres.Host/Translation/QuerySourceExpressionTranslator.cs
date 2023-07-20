namespace SpaceEngineers.Core.GenericEndpoint.DataAccess.Sql.Postgres.Host.Translation;

using System;
using System.Text;
using GenericHost;
using SpaceEngineers.Core.AutoRegistration.Api.Abstractions;
using SpaceEngineers.Core.AutoRegistration.Api.Attributes;
using SpaceEngineers.Core.AutoRegistration.Api.Enumerations;
using SpaceEngineers.Core.DataAccess.Orm.Sql.Model;
using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation;
using SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.Expressions;
using Sql.Host.StartupActions;

[Component(EnLifestyle.Singleton)]
internal class QuerySourceExpressionTranslator : ISqlExpressionTranslator<QuerySourceExpression>,
                                                 IResolvable<ISqlExpressionTranslator<QuerySourceExpression>>,
                                                 ICollectionResolvable<ISqlExpressionTranslator>
{
    private readonly IModelProvider _modelProvider;
    private readonly ISqlViewQueryProviderComposite _sqlViewQueryProvider;
    private readonly IHostedServiceRegistry _hostedServiceRegistry;
    private readonly IHostedServiceStartupAction _hostedServiceStartupAction;

    public QuerySourceExpressionTranslator(
        IModelProvider modelProvider,
        ISqlViewQueryProviderComposite sqlViewQueryProvider,
        IHostedServiceRegistry hostedServiceRegistry,
        UpgradeDatabaseHostedServiceStartupAction hostedServiceStartupAction)
    {
        _modelProvider = modelProvider;
        _sqlViewQueryProvider = sqlViewQueryProvider;
        _hostedServiceRegistry = hostedServiceRegistry;
        _hostedServiceStartupAction = hostedServiceStartupAction;
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
        return _hostedServiceRegistry.Contains(_hostedServiceStartupAction);
    }
}