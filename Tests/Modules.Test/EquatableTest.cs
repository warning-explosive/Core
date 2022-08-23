namespace SpaceEngineers.Core.Modules.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Core.Test.Api;
    using Core.Test.Api.ClassFixtures;
    using DataAccess.Orm.Sql.Model;
    using DataAccess.Orm.Sql.Translation;
    using DataAccess.Orm.Sql.Translation.Expressions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.Implementations;
    using GenericEndpoint.Messaging.MessageHeaders;
    using Messages;
    using Mocks;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// EquatableTest
    /// </summary>
    [SuppressMessage("Analysis", "CA1506", Justification = "application composition root")]
    public class EquatableTest : TestBase
    {
        /// <summary> .ctor </summary>
        /// <param name="output">ITestOutputHelper</param>
        /// <param name="fixture">TestFixture</param>
        public EquatableTest(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
        }

        /// <summary>
        /// EquatableTest data member
        /// </summary>
        /// <returns>EquatableTest data</returns>
        public static IEnumerable<object[]> EquatableTestData()
        {
            {
                var payload = new Command(42);
                var integrationMessageFactory = new IntegrationMessageFactory(new[] { new AnonymousUserScopeProvider() });
                var integrationMessage = integrationMessageFactory.CreateGeneralMessage(payload, TestIdentity.Endpoint10, null);
                var notEqualMessage = integrationMessageFactory.CreateGeneralMessage(payload, TestIdentity.Endpoint10, null);
                var equalMessage = integrationMessageFactory.CreateGeneralMessage(payload, TestIdentity.Endpoint10, null);
                equalMessage.OverwriteHeader(integrationMessage.ReadRequiredHeader<Id>());

                yield return new object[]
                {
                    typeof(IntegrationMessage),
                    integrationMessage,
                    equalMessage,
                    notEqualMessage
                };
            }

            {
                var trueConstant = new DataAccess.Orm.Sql.Translation.Expressions.ConstantExpression(typeof(bool), true);
                var falseConstant = new DataAccess.Orm.Sql.Translation.Expressions.ConstantExpression(typeof(bool), false);

                var expression = new DataAccess.Orm.Sql.Translation.Expressions.BinaryExpression(typeof(bool), BinaryOperator.Equal, trueConstant, falseConstant);
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.BinaryExpression(typeof(bool), BinaryOperator.Equal, trueConstant, falseConstant);
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.BinaryExpression(typeof(bool), BinaryOperator.Equal, falseConstant, trueConstant);

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.BinaryExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var trueConstant = new DataAccess.Orm.Sql.Translation.Expressions.ConstantExpression(typeof(bool), true);
                var falseConstant = new DataAccess.Orm.Sql.Translation.Expressions.ConstantExpression(typeof(bool), false);

                var expression = new DataAccess.Orm.Sql.Translation.Expressions.ConditionalExpression(typeof(bool), trueConstant, trueConstant, falseConstant);
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.ConditionalExpression(typeof(bool), trueConstant, trueConstant, falseConstant);
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.ConditionalExpression(typeof(bool), falseConstant, falseConstant, trueConstant);

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.ConditionalExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var expression = new DataAccess.Orm.Sql.Translation.Expressions.ConstantExpression(typeof(bool), true);
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.ConstantExpression(typeof(bool), true);
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.ConstantExpression(typeof(bool), false);

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.ConstantExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var source = new DataAccess.Orm.Sql.Translation.Expressions.QuerySourceExpression(typeof(object));

                var trueConstant = new DataAccess.Orm.Sql.Translation.Expressions.ConstantExpression(typeof(bool), true);
                var falseConstant = new DataAccess.Orm.Sql.Translation.Expressions.ConstantExpression(typeof(bool), false);

                var expression = new DataAccess.Orm.Sql.Translation.Expressions.FilterExpression(typeof(object), source, trueConstant);
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.FilterExpression(typeof(object), source, trueConstant);
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.FilterExpression(typeof(object), source, falseConstant);

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.FilterExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var context = new TranslationContext();
                var source = new DataAccess.Orm.Sql.Translation.Expressions.NamedSourceExpression(
                    typeof(object),
                    new DataAccess.Orm.Sql.Translation.Expressions.QuerySourceExpression(typeof(object)),
                    new DataAccess.Orm.Sql.Translation.Expressions.ParameterExpression(context, typeof(object)));

                Func<IReadOnlyDictionary<string, object?>, IIntermediateExpression> valuesExpressionProducer = map =>
                    new DataAccess.Orm.Sql.Translation.Expressions.ConstantExpression(typeof(bool), true);

                var expression = new DataAccess.Orm.Sql.Translation.Expressions.GroupByExpression(typeof(object), valuesExpressionProducer);
                expression.Apply(context, source);
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.GroupByExpression(typeof(object), valuesExpressionProducer);
                equalExpression.Apply(context, source);
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.GroupByExpression(typeof(string), valuesExpressionProducer);
                notEqualExpression.Apply(context, source);

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.GroupByExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var left = new DataAccess.Orm.Sql.Translation.Expressions.QuerySourceExpression(typeof(object));
                var right = new DataAccess.Orm.Sql.Translation.Expressions.QuerySourceExpression(typeof(string));
                var on = new DataAccess.Orm.Sql.Translation.Expressions.ConstantExpression(typeof(bool), true);

                var expression = new DataAccess.Orm.Sql.Translation.Expressions.JoinExpression(left, right, on);
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.JoinExpression(left, right, on);
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.JoinExpression(right, left, on);

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.JoinExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var parameter = new DataAccess.Orm.Sql.Translation.Expressions.ParameterExpression(new TranslationContext(), typeof(string));

                var expression = new DataAccess.Orm.Sql.Translation.Expressions.MethodCallExpression(typeof(string), nameof(string.ToLowerInvariant), parameter, Enumerable.Empty<IIntermediateExpression>());
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.MethodCallExpression(typeof(string), nameof(string.ToLowerInvariant), parameter, Enumerable.Empty<IIntermediateExpression>());
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.MethodCallExpression(typeof(string), nameof(string.ToUpperInvariant), parameter, Enumerable.Empty<IIntermediateExpression>());

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.MethodCallExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var parameter = new DataAccess.Orm.Sql.Translation.Expressions.ParameterExpression(new TranslationContext(), typeof(string));
                var member = typeof(string).GetProperty(nameof(string.Length)) !;
                var simpleBindingExpression = new DataAccess.Orm.Sql.Translation.Expressions.SimpleBindingExpression(member, typeof(int), parameter);

                var expression = new DataAccess.Orm.Sql.Translation.Expressions.NamedBindingExpression(nameof(Enumerable.Count), simpleBindingExpression);
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.NamedBindingExpression(nameof(Enumerable.Count), simpleBindingExpression);
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.NamedBindingExpression(nameof(string.Length), simpleBindingExpression);

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.NamedBindingExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var source = new DataAccess.Orm.Sql.Translation.Expressions.QuerySourceExpression(typeof(object));
                var parameter = new DataAccess.Orm.Sql.Translation.Expressions.ParameterExpression(new TranslationContext(), typeof(object));

                var strSource = new DataAccess.Orm.Sql.Translation.Expressions.QuerySourceExpression(typeof(string));
                var strParameter = new DataAccess.Orm.Sql.Translation.Expressions.ParameterExpression(new TranslationContext(), typeof(string));

                var expression = new DataAccess.Orm.Sql.Translation.Expressions.NamedSourceExpression(typeof(object), source, parameter);
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.NamedSourceExpression(typeof(object), source, parameter);
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.NamedSourceExpression(typeof(string), strSource, strParameter);

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.NamedSourceExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var expression = new DataAccess.Orm.Sql.Translation.Expressions.NewExpression(typeof(object));
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.NewExpression(typeof(object));
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.NewExpression(typeof(string));

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.NewExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var source = new DataAccess.Orm.Sql.Translation.Expressions.QuerySourceExpression(typeof(object));

                var parameter = new DataAccess.Orm.Sql.Translation.Expressions.ParameterExpression(new TranslationContext(), typeof(string));
                var property = typeof(string).GetProperty(nameof(string.Length)) !;
                var bindingExpression = new DataAccess.Orm.Sql.Translation.Expressions.SimpleBindingExpression(property, typeof(string), parameter);
                var orderByBindingExpressionAsc = new DataAccess.Orm.Sql.Translation.Expressions.OrderByBindingExpression(bindingExpression, EnOrderingDirection.ASC);
                var orderByBindingExpressionDesc = new DataAccess.Orm.Sql.Translation.Expressions.OrderByBindingExpression(bindingExpression, EnOrderingDirection.DESC);

                var expression = new DataAccess.Orm.Sql.Translation.Expressions.OrderByExpression(typeof(object), source, new[] { orderByBindingExpressionAsc });
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.OrderByExpression(typeof(object), source, new[] { orderByBindingExpressionAsc });
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.OrderByExpression(typeof(string), source, new[] { orderByBindingExpressionDesc });

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.OrderByExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var parameter = new DataAccess.Orm.Sql.Translation.Expressions.ParameterExpression(new TranslationContext(), typeof(string));
                var property = typeof(string).GetProperty(nameof(string.Length)) !;
                var bindingExpression = new DataAccess.Orm.Sql.Translation.Expressions.SimpleBindingExpression(property, typeof(string), parameter);

                var expression = new DataAccess.Orm.Sql.Translation.Expressions.OrderByBindingExpression(bindingExpression, EnOrderingDirection.ASC);
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.OrderByBindingExpression(bindingExpression, EnOrderingDirection.ASC);
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.OrderByBindingExpression(bindingExpression, EnOrderingDirection.DESC);

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.OrderByBindingExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var expression = new DataAccess.Orm.Sql.Translation.Expressions.ParameterExpression(new TranslationContext(), typeof(object));
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.ParameterExpression(new TranslationContext(), typeof(object));
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.ParameterExpression(new TranslationContext(), typeof(string));

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.ParameterExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var source = new DataAccess.Orm.Sql.Translation.Expressions.QuerySourceExpression(typeof(object));

                var expression = new DataAccess.Orm.Sql.Translation.Expressions.ProjectionExpression(typeof(object), source, Enumerable.Empty<IIntermediateExpression>());
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.ProjectionExpression(typeof(object), source, Enumerable.Empty<IIntermediateExpression>());
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.ProjectionExpression(typeof(string), source, Enumerable.Empty<IIntermediateExpression>());

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.ProjectionExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var expression = DataAccess.Orm.Sql.Translation.Expressions.QueryParameterExpression.Create(new TranslationContext(), typeof(string), nameof(String));
                var equalExpression = DataAccess.Orm.Sql.Translation.Expressions.QueryParameterExpression.Create(new TranslationContext(), typeof(string), nameof(String));
                var notEqualExpression = DataAccess.Orm.Sql.Translation.Expressions.QueryParameterExpression.Create(new TranslationContext(), typeof(string), typeof(string).FullName!);

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.QueryParameterExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var expression = new DataAccess.Orm.Sql.Translation.Expressions.QuerySourceExpression(typeof(object));
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.QuerySourceExpression(typeof(object));
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.QuerySourceExpression(typeof(string));

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.QuerySourceExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var source = new DataAccess.Orm.Sql.Translation.Expressions.QuerySourceExpression(typeof(object));

                var expression = new DataAccess.Orm.Sql.Translation.Expressions.RowsFetchLimitExpression(typeof(object), 42, source);
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.RowsFetchLimitExpression(typeof(object), 42, source);
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.RowsFetchLimitExpression(typeof(object), 24, source);

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.RowsFetchLimitExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var parameter = new DataAccess.Orm.Sql.Translation.Expressions.ParameterExpression(new TranslationContext(), typeof(EndpointIdentity));
                var logicalNameMember = typeof(EndpointIdentity).GetProperty(nameof(EndpointIdentity.LogicalName)) !;
                var instanceNameMember = typeof(EndpointIdentity).GetProperty(nameof(EndpointIdentity.InstanceName)) !;

                var expression = new DataAccess.Orm.Sql.Translation.Expressions.SimpleBindingExpression(logicalNameMember, typeof(int), parameter);
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.SimpleBindingExpression(logicalNameMember, typeof(int), parameter);
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.SimpleBindingExpression(instanceNameMember, typeof(int), parameter);

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.SimpleBindingExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var expression = new DataAccess.Orm.Sql.Translation.Expressions.SpecialExpression(typeof(string), nameof(DataAccess.Orm.Sql.Translation.Expressions.SpecialExpression));
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.SpecialExpression(typeof(string), nameof(DataAccess.Orm.Sql.Translation.Expressions.SpecialExpression));
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.SpecialExpression(typeof(string), typeof(DataAccess.Orm.Sql.Translation.Expressions.SpecialExpression).FullName!);

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.SpecialExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var trueConstant = new DataAccess.Orm.Sql.Translation.Expressions.ConstantExpression(typeof(bool), true);
                var falseConstant = new DataAccess.Orm.Sql.Translation.Expressions.ConstantExpression(typeof(bool), false);

                var expression = new DataAccess.Orm.Sql.Translation.Expressions.UnaryExpression(typeof(bool), UnaryOperator.Not, trueConstant);
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.UnaryExpression(typeof(bool), UnaryOperator.Not, trueConstant);
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.UnaryExpression(typeof(bool), UnaryOperator.Not, falseConstant);

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.UnaryExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var info = new SchemaInfo("schema1", Array.Empty<TableInfo>(), Array.Empty<ViewInfo>());
                var equalInfo = new SchemaInfo("schema1", Array.Empty<TableInfo>(), Array.Empty<ViewInfo>());
                var notEqualInfo = new SchemaInfo("schema2", Array.Empty<TableInfo>(), Array.Empty<ViewInfo>());

                yield return new object[]
                {
                    typeof(SchemaInfo),
                    info,
                    equalInfo,
                    notEqualInfo
                };
            }

            {
                var info = new TableInfo(typeof(object), default!);
                var equalInfo = new TableInfo(typeof(object), default!);
                var notEqualInfo = new TableInfo(typeof(string), default!);

                yield return new object[]
                {
                    typeof(TableInfo),
                    info,
                    equalInfo,
                    notEqualInfo
                };
            }

            {
                var info = new ViewInfo(typeof(object), string.Empty, default!);
                var equalInfo = new ViewInfo(typeof(object), string.Empty, default!);
                var notEqualInfo = new ViewInfo(typeof(string), string.Empty, default!);

                yield return new object[]
                {
                    typeof(ViewInfo),
                    info,
                    equalInfo,
                    notEqualInfo
                };
            }

            {
                var info = new MtmTableInfo(typeof(object), typeof(string), typeof(object), default!);
                var equalInfo = new MtmTableInfo(typeof(object), typeof(string), typeof(object), default!);
                var notEqualInfo = new MtmTableInfo(typeof(string), typeof(string), typeof(object), default!);

                yield return new object[]
                {
                    typeof(MtmTableInfo),
                    info,
                    equalInfo,
                    notEqualInfo
                };
            }

            {
                var table = new TableInfo(typeof(EndpointIdentity), default!);
                var logicalNameProperty = typeof(EndpointIdentity).GetProperty(nameof(EndpointIdentity.LogicalName)) !;
                var instanceNameProperty = typeof(EndpointIdentity).GetProperty(nameof(EndpointIdentity.InstanceName)) !;

                var info = new ColumnInfo(table, new[] { new ColumnProperty(logicalNameProperty, logicalNameProperty) }, default!);
                var equalInfo = new ColumnInfo(table, new[] { new ColumnProperty(logicalNameProperty, logicalNameProperty) }, default!);
                var notEqualInfo = new ColumnInfo(table, new[] { new ColumnProperty(instanceNameProperty, instanceNameProperty) }, default!);

                yield return new object[]
                {
                    typeof(ColumnInfo),
                    info,
                    equalInfo,
                    notEqualInfo
                };
            }

            {
                var table = new TableInfo(typeof(string), default!);
                var logicalNameProperty = typeof(EndpointIdentity).GetProperty(nameof(EndpointIdentity.LogicalName)) !;
                var instanceNameProperty = typeof(EndpointIdentity).GetProperty(nameof(EndpointIdentity.InstanceName)) !;
                var logicalNameColumn = new ColumnInfo(table, new[] { new ColumnProperty(logicalNameProperty, logicalNameProperty) }, default!);
                var instanceNameColumn = new ColumnInfo(table, new[] { new ColumnProperty(instanceNameProperty, instanceNameProperty) }, default!);

                var info = new IndexInfo(table, new[] { logicalNameColumn }, true);
                var equalInfo = new IndexInfo(table, new[] { logicalNameColumn }, true);
                var notEqualInfo = new IndexInfo(table, new[] { instanceNameColumn }, true);

                yield return new object[]
                {
                    typeof(IndexInfo),
                    info,
                    equalInfo,
                    notEqualInfo
                };
            }
        }

        [Theory]
        [MemberData(nameof(EquatableTestData))]
        internal void EqualsTest(
            Type type,
            object @object,
            object equalObject,
            object notEqualObject)
        {
            Output.WriteLine(type.FullName);

            Assert.IsType(type, @object);
            Assert.IsType(type, equalObject);
            Assert.IsType(type, notEqualObject);

            Assert.Equal(@object, equalObject);
            Assert.NotSame(@object, equalObject);
            Assert.NotEqual(@object, notEqualObject);
        }
    }
}