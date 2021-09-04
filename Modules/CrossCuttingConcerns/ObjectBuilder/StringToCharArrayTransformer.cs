namespace SpaceEngineers.Core.CrossCuttingConcerns.ObjectBuilder
{
    using Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class StringToCharArrayTransformer : IObjectTransformer<string, char[]>
    {
        public char[] Transform(string value)
        {
            return value.ToCharArray();
        }
    }
}