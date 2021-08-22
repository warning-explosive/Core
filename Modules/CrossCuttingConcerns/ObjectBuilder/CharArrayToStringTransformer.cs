namespace SpaceEngineers.Core.CrossCuttingConcerns.ObjectBuilder
{
    using Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class CharArrayToStringTransformer : IObjectTransformer<char[], string>
    {
        public string Transform(char[] value)
        {
            return new string(value);
        }
    }
}