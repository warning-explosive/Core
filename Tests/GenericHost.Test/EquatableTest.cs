namespace SpaceEngineers.Core.GenericHost.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Basics.Enumerations;
    using DataAccess.Orm.Sql.Translation;
    using DataAccess.Orm.Sql.Translation.Expressions;
    using GenericEndpoint.Contract;
    using GenericEndpoint.Messaging;
    using GenericEndpoint.Messaging.MessageHeaders;
    using Messages;
    using SpaceEngineers.Core.Test.Api;
    using SpaceEngineers.Core.Test.Api.ClassFixtures;
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
                var reflectedType = typeof(Command);
                var integrationMessageFactory = new IntegrationMessageFactory(new[] { new AnonymousUserScopeProvider() });
                var integrationMessage = integrationMessageFactory.CreateGeneralMessage(payload, reflectedType, Array.Empty<IIntegrationMessageHeader>(), null);
                var notEqualMessage = integrationMessageFactory.CreateGeneralMessage(payload, reflectedType, Array.Empty<IIntegrationMessageHeader>(), null);
                var equalMessage = integrationMessageFactory.CreateGeneralMessage(payload, reflectedType, Array.Empty<IIntegrationMessageHeader>(), null);
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
                var context = new TranslationContext();

                var trueConstant = new DataAccess.Orm.Sql.Translation.Expressions.QueryParameterExpression(context, typeof(bool), _ => System.Linq.Expressions.Expression.Constant(false));
                var falseConstant = new DataAccess.Orm.Sql.Translation.Expressions.QueryParameterExpression(context, typeof(bool), _ => System.Linq.Expressions.Expression.Constant(false));

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
                var context = new TranslationContext();

                var trueConstant = new DataAccess.Orm.Sql.Translation.Expressions.QueryParameterExpression(context, typeof(bool), _ => System.Linq.Expressions.Expression.Constant(true));
                var falseConstant = new DataAccess.Orm.Sql.Translation.Expressions.QueryParameterExpression(context, typeof(bool), _ => System.Linq.Expressions.Expression.Constant(false));

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
                var expression = new DataAccess.Orm.Sql.Translation.Expressions.QueryParameterExpression(new TranslationContext(), typeof(bool), _ => System.Linq.Expressions.Expression.Constant(true));
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.QueryParameterExpression(new TranslationContext(), typeof(bool), _ => System.Linq.Expressions.Expression.Constant(true));
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.QueryParameterExpression(new TranslationContext(), typeof(string), _ => System.Linq.Expressions.Expression.Constant(true));

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.QueryParameterExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var context = new TranslationContext();

                var source = new DataAccess.Orm.Sql.Translation.Expressions.QuerySourceExpression(typeof(object));

                var trueConstant = new DataAccess.Orm.Sql.Translation.Expressions.QueryParameterExpression(context, typeof(bool), _ => System.Linq.Expressions.Expression.Constant(true));
                var falseConstant = new DataAccess.Orm.Sql.Translation.Expressions.QueryParameterExpression(context, typeof(bool), _ => System.Linq.Expressions.Expression.Constant(false));

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

                var left = new DataAccess.Orm.Sql.Translation.Expressions.QuerySourceExpression(typeof(object));
                var right = new DataAccess.Orm.Sql.Translation.Expressions.QuerySourceExpression(typeof(string));
                var on = new DataAccess.Orm.Sql.Translation.Expressions.QueryParameterExpression(context, typeof(bool), _ => System.Linq.Expressions.Expression.Constant(true));

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
                var context = new TranslationContext();

                var parameter = new DataAccess.Orm.Sql.Translation.Expressions.ParameterExpression(context, typeof(string));

                var expression = new DataAccess.Orm.Sql.Translation.Expressions.MethodCallExpression(typeof(string), nameof(string.ToLowerInvariant), parameter, Enumerable.Empty<ISqlExpression>());
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.MethodCallExpression(typeof(string), nameof(string.ToLowerInvariant), parameter, Enumerable.Empty<ISqlExpression>());
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.MethodCallExpression(typeof(string), nameof(string.ToUpperInvariant), parameter, Enumerable.Empty<ISqlExpression>());

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.MethodCallExpression),
                    expression,
                    equalExpression,
                    notEqualExpression
                };
            }

            {
                var context = new TranslationContext();

                var parameter = new DataAccess.Orm.Sql.Translation.Expressions.ParameterExpression(context, typeof(string));
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
                var context = new TranslationContext();

                var source = new DataAccess.Orm.Sql.Translation.Expressions.QuerySourceExpression(typeof(object));
                var parameter = new DataAccess.Orm.Sql.Translation.Expressions.ParameterExpression(context, typeof(object));

                var strSource = new DataAccess.Orm.Sql.Translation.Expressions.QuerySourceExpression(typeof(string));
                var strParameter = new DataAccess.Orm.Sql.Translation.Expressions.ParameterExpression(context, typeof(string));

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
                var context = new TranslationContext();

                var source = new DataAccess.Orm.Sql.Translation.Expressions.QuerySourceExpression(typeof(object));

                var parameter = new DataAccess.Orm.Sql.Translation.Expressions.ParameterExpression(context, typeof(string));
                var property = typeof(string).GetProperty(nameof(string.Length)) !;
                var bindingExpression = new DataAccess.Orm.Sql.Translation.Expressions.SimpleBindingExpression(property, typeof(string), parameter);
                var orderByBindingExpressionAsc = new DataAccess.Orm.Sql.Translation.Expressions.OrderByBindingExpression(bindingExpression, EnOrderingDirection.Asc);
                var orderByBindingExpressionDesc = new DataAccess.Orm.Sql.Translation.Expressions.OrderByBindingExpression(bindingExpression, EnOrderingDirection.Desc);

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
                var context = new TranslationContext();

                var parameter = new DataAccess.Orm.Sql.Translation.Expressions.ParameterExpression(context, typeof(string));
                var property = typeof(string).GetProperty(nameof(string.Length)) !;
                var bindingExpression = new DataAccess.Orm.Sql.Translation.Expressions.SimpleBindingExpression(property, typeof(string), parameter);

                var expression = new DataAccess.Orm.Sql.Translation.Expressions.OrderByBindingExpression(bindingExpression, EnOrderingDirection.Asc);
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.OrderByBindingExpression(bindingExpression, EnOrderingDirection.Asc);
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.OrderByBindingExpression(bindingExpression, EnOrderingDirection.Desc);

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

                var expression = new DataAccess.Orm.Sql.Translation.Expressions.ProjectionExpression(typeof(object), source, Enumerable.Empty<ISqlExpression>());
                var equalExpression = new DataAccess.Orm.Sql.Translation.Expressions.ProjectionExpression(typeof(object), source, Enumerable.Empty<ISqlExpression>());
                var notEqualExpression = new DataAccess.Orm.Sql.Translation.Expressions.ProjectionExpression(typeof(string), source, Enumerable.Empty<ISqlExpression>());

                yield return new object[]
                {
                    typeof(DataAccess.Orm.Sql.Translation.Expressions.ProjectionExpression),
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
                var context = new TranslationContext();

                var parameter = new DataAccess.Orm.Sql.Translation.Expressions.ParameterExpression(context, typeof(EndpointIdentity));
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
                var context = new TranslationContext();

                var trueConstant = new DataAccess.Orm.Sql.Translation.Expressions.QueryParameterExpression(context, typeof(bool), _ => System.Linq.Expressions.Expression.Constant(true));
                var falseConstant = new DataAccess.Orm.Sql.Translation.Expressions.QueryParameterExpression(context, typeof(bool), _ => System.Linq.Expressions.Expression.Constant(false));

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