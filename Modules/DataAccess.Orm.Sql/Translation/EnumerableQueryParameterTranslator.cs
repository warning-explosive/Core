namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CompositionRoot;
    using Extensions;

    [SuppressMessage("Analysis", "SA1011", Justification = "space between square brackets and nullable symbol")]
    [Component(EnLifestyle.Singleton)]
    internal class EnumerableQueryParameterTranslator<T> : IQueryParameterTranslator<IEnumerable<T>>,
                                                           IResolvable<IQueryParameterTranslator<IEnumerable<T>>>
    {
        private readonly IDependencyContainer _dependencyContainer;

        public EnumerableQueryParameterTranslator(IDependencyContainer dependencyContainer)
        {
            _dependencyContainer = dependencyContainer;
        }

        public string Translate(IEnumerable<T> value)
        {
            var expression = value
                .Select(item => item.QueryParameterSqlExpression(_dependencyContainer))
                .ToString(", ");

            var sb = new StringBuilder();

            sb.Append(expression);

            return sb.ToString();
        }
    }
}