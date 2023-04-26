namespace SpaceEngineers.Core.CrossCuttingConcerns.Json
{
    using System;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Text.Json.Serialization.Metadata;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Basics;

    [Component(EnLifestyle.Singleton)]
    internal class PolymorphicJsonTypeInfoResolver : DefaultJsonTypeInfoResolver,
                                                     IResolvable<IJsonTypeInfoResolver>
    {
        internal const string TypeDiscriminatorPropertyName = "$type";

        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var jsonTypeInfo = base.GetTypeInfo(type, options);

            if (jsonTypeInfo.Kind == JsonTypeInfoKind.None)
            {
                return jsonTypeInfo;
            }

            var derivedTypes = jsonTypeInfo
                .Type
                .DerivedTypes()
                .Where(derivedType => derivedType.IsConcreteType()
                                      && !derivedType.IsGenericTypeDefinition
                                      && !derivedType.IsPartiallyClosed())
                .ToList();

            if (derivedTypes.Any())
            {
                jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
                {
                    TypeDiscriminatorPropertyName = TypeDiscriminatorPropertyName,
                    IgnoreUnrecognizedTypeDiscriminators = true,
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization
                };

                foreach (var derivedType in derivedTypes)
                {
                    var jsonDerivedType = new JsonDerivedType(derivedType, TypeNode.FromType(derivedType).ToString());
                    jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(jsonDerivedType);
                }
            }

            return jsonTypeInfo;
        }
    }
}