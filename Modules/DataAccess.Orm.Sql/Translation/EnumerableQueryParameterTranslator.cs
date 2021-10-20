namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot.Api.Abstractions.Container;
    using Extensions;

    [SuppressMessage("Analysis", "SA1011", Justification = "space between square brackets and nullable symbol")]
    [Component(EnLifestyle.Singleton)]
    internal class EnumerableQueryParameterTranslator<T> : IQueryParameterTranslator<IEnumerable<T>>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public EnumerableQueryParameterTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Translate(IEnumerable<T> value)
        {
            var sqlExpression = value
                .Select(item => item.QueryParameterSqlExpression(_dependencyContainer))
                .ToString(", ");

            var sb = new StringBuilder();

            sb.Append("(");
            sb.Append(sqlExpression);
            sb.Append(")");

            return sb.ToString();
        }
    }
}