namespace SpaceEngineers.Core.CrossCuttingConcerns.ObjectBuilder
{
    using Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class CharArrayToStringTransformer : IObjectTransformer<char[], string>
    {
        public string Transform(char[] value)
        {
            return new string(value);
        }
    }
}