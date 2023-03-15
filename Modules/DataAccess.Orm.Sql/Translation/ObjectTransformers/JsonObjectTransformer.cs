namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.ObjectTransformers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Json;
    using CrossCuttingConcerns.ObjectBuilder;
    using Model;

    [Component(EnLifestyle.Singleton)]
    internal class JsonObjectTransformer<TValue> : IObjectTransformer<string, TValue>,
                                                   IResolvable<IObjectTransformer<string, TValue>>
    {
        private readonly HashSet<Type> _jsonColumnTypes;
        private readonly IJsonSerializer _jsonSerializer;

        public JsonObjectTransformer(
            IModelProvider modelProvider,
            IJsonSerializer jsonSerializer)
        {
            _jsonColumnTypes ??= modelProvider
                .Tables
                .Values
                .SelectMany(table => table.Columns.Values)
                .Where(column => column.IsJsonColumn)
                .Select(column => column.Type)
                .ToHashSet();

            _jsonSerializer = jsonSerializer;
        }

        public TValue Transform(string value)
        {
            if (_jsonColumnTypes.Contains(typeof(TValue)))
            {
                return _jsonSerializer.DeserializeObject<TValue>(value);
            }

            throw new NotSupportedException(typeof(TValue).FullName);
        }
    }
}