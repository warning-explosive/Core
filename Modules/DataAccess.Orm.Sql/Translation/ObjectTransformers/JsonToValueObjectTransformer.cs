namespace SpaceEngineers.Core.DataAccess.Orm.Sql.Translation.ObjectTransformers
{
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using CrossCuttingConcerns.Json;
    using CrossCuttingConcerns.ObjectBuilder;

    [Component(EnLifestyle.Singleton)]
    internal class JsonToValueObjectTransformer<TValue> : IObjectTransformer<string, TValue>,
                                                          IResolvable<IObjectTransformer<string, TValue>>
    {
        private readonly IJsonSerializer _jsonSerializer;

        public JsonToValueObjectTransformer(IJsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
        }

        public TValue Transform(string value)
        {
            return _jsonSerializer.DeserializeObject<TValue>(value);
        }
    }
}