namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using Expressions;
    using Linq;
    using Model;
    using Transaction;

    [Component(EnLifestyle.Singleton)]
    internal class InsertLinqExpressionVisitor : ILinqExpressionVisitor,
                                                 ICollectionResolvable<ILinqExpressionVisitor>
    {
        private readonly IModelProvider _modelProvider;

        public InsertLinqExpressionVisitor(IModelProvider modelProvider)
        {
            _modelProvider = modelProvider;
        }

        internal static MethodInfo GetInsertValuesMethod { get; } = new MethodFinder(
                typeof(InsertLinqExpressionVisitor),
                nameof(GetInsertValues),
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod)
            {
                ArgumentTypes = new[] { typeof(IModelProvider), typeof(IReadOnlyCollection<IDatabaseEntity>) }
            }
            .FindMethod() ?? throw new InvalidOperationException("SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.InsertMethodCallExpressionTranslator.GetInsertValues()");

        public bool TryVisit(
            ExpressionVisitor visitor,
            TranslationContext context,
            Expression expression)
        {
            if (expression is not System.Linq.Expressions.MethodCallExpression methodCallExpression)
            {
                return false;
            }

            var method = methodCallExpression.Method.GenericMethodDefinitionOrSelf();

            if (method == LinqMethods.RepositoryInsert())
            {
                var getInsertValuesMethodCallExpression = Expression.Call(
                    null,
                    GetInsertValuesMethod,
                    Expression.Constant(_modelProvider),
                    Expression.Constant(new List<IDatabaseEntity>(), typeof(IReadOnlyCollection<IDatabaseEntity>)));

                using (context.WithinPathScope(methodCallExpression.Arguments[1]))
                using (context.WithinPathScope(getInsertValuesMethodCallExpression))
                {
                    _ = (IAdvancedDatabaseTransaction)((ConstantExpression)methodCallExpression.Arguments[0]).Value;
                    var entities = (IReadOnlyCollection<IDatabaseEntity>)((ConstantExpression)methodCallExpression.Arguments[1]).Value;
                    var insertBehavior = (EnInsertBehavior)((ConstantExpression)methodCallExpression.Arguments[2]).Value;

                    var map = entities
                        .SelectMany(_modelProvider.Flatten)
                        .Distinct(new UniqueIdentifiedEqualityComparer())
                        .ToDictionary(entity => new EntityKey(entity), entity => entity);

                    var stacks = map
                        .Values
                        .OrderByDependencies(entity => new EntityKey(entity), GetDependencies(_modelProvider, map))
                        .Stack(entity => entity.GetType());

                    var expressions = stacks
                        .Select(pair =>
                        {
                            var (type, stack) = pair;
                            var table = _modelProvider.Tables[type];

                            var values = stack
                                .Select(_ =>
                                {
                                    var values = table
                                        .Columns
                                        .Values
                                        .Where(column => !column.IsMultipleRelation)
                                        .Select(column =>
                                        {
                                            var name = context.NextCommandParameterName();
                                            context.CaptureCommandParameterExtractor(name, null);
                                            return new QueryParameterExpression(column.Type, name);
                                        })
                                        .ToList();

                                    return new ValuesExpression(values);
                                })
                                .ToList();

                            return new InsertExpression(type, insertBehavior, values);
                        })
                        .ToList();

                    var batchExpression = new BatchExpression(expressions);
                    context.Remember(batchExpression);
                }

                return true;
            }

            return false;
        }

        private static Func<IUniqueIdentified, IEnumerable<IUniqueIdentified>> GetDependencies(
            IModelProvider modelProvider,
            IReadOnlyDictionary<EntityKey, IUniqueIdentified> map)
        {
            return entity =>
            {
                var table = modelProvider.Tables[entity.GetType()];

                return table
                    .Columns
                    .Values
                    .Where(column => column.IsRelation)
                    .Select(DependencySelector(table, entity, map))
                    .Where(dependency => dependency != null)
                    .Select(dependency => dependency!);

                static Func<ColumnInfo, IUniqueIdentified?> DependencySelector(
                    ITableInfo table,
                    IUniqueIdentified entity,
                    IReadOnlyDictionary<EntityKey, IUniqueIdentified> map)
                {
                    return column => table.IsMtmTable
                        ? map[new EntityKey(column.Relation.Target, column.GetValue(entity) !)]
                        : column.GetRelationValue(entity);
                }
            };
        }

        private static IReadOnlyDictionary<string, ConstantExpression> GetInsertValues(
                IModelProvider modelProvider,
                IReadOnlyCollection<IDatabaseEntity> entities)
        {
            var map = entities
                .SelectMany(modelProvider.Flatten)
                .Distinct(new UniqueIdentifiedEqualityComparer())
                .ToDictionary(entity => new EntityKey(entity), entity => entity);

            return map
                .Values
                .OrderByDependencies(entity => new EntityKey(entity), GetDependencies(modelProvider, map))
                .SelectMany(entity => modelProvider
                    .Tables[entity.GetType()]
                    .Columns
                    .Values
                    .Where(column => !column.IsMultipleRelation)
                    .Select(column =>
                    {
                        var value = column.GetValue(entity);

                        return column.IsJsonColumn
                            ? Expression.Constant(new DatabaseJsonObject(value, column.Type), typeof(DatabaseJsonObject))
                            : Expression.Constant(value, column.Type);
                    }))
                .Select((expression, index) => (name: TranslationContext.CommandParameterFormat.Format(index), expression))
                .ToDictionary(
                    pair => pair.name,
                    pair => pair.expression,
                    StringComparer.OrdinalIgnoreCase);
        }
    }
}