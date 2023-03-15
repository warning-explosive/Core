namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.ObjectTransformers
{
    using System;
    using System.ComponentModel;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;
    using CrossCuttingConcerns.ObjectBuilder;

    [Component(EnLifestyle.Singleton)]
    internal class DbNullObjectTransformer<TValue> : TypeConverter,
                                                     IObjectTransformer<DBNull, TValue>,
                                                     IResolvable<IObjectTransformer<DBNull, TValue>>
    {
        static DbNullObjectTransformer()
        {
            TypeDescriptor.AddAttributes(
                typeof(DBNull),
                new TypeConverterAttribute(typeof(DbNullTypeConverter)));
        }

        public TValue Transform(DBNull value)
        {
            if (typeof(TValue).IsReference() || typeof(TValue).IsNullable())
            {
                return (TValue)typeof(TValue).DefaultValue() !;
            }

            throw new NotSupportedException($"{nameof(DBNull)} cannot be transformed to {typeof(TValue).FullName}");
        }
    }
}