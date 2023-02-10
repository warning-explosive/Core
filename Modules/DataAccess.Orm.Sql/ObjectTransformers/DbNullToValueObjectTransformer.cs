namespace SpaceEngineers.Core.DataAccess.Orm.Sql.ObjectTransformers
{
    using System;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CrossCuttingConcerns.ObjectBuilder;

    [Component(EnLifestyle.Singleton)]
    internal class DbNullToValueObjectTransformer<TValue> : IObjectTransformer<DBNull, TValue>,
                                                            IResolvable<IObjectTransformer<DBNull, TValue>>
    {
        public TValue Transform(DBNull value)
        {
            if (typeof(TValue).IsReference())
            {
                return (TValue)typeof(TValue).DefaultValue() !;
            }

            var valueType = typeof(TValue).ExtractGenericArgumentAtOrSelf(typeof(Nullable<>));

            if (valueType != typeof(TValue))
            {
                return (TValue)typeof(Nullable<>).MakeGenericType(valueType).DefaultValue() !;
            }

            throw new NotSupportedException($"{nameof(DBNull)} cannot be transformed to {typeof(TValue).FullName}");
        }
    }
}