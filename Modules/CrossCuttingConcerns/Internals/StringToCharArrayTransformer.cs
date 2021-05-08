namespace SpaceEngineers.Core.CrossCuttingConcerns.Internals
{
    using Api.Abstractions;
    using AutoWiring.Api.Attributes;
    using AutoWiring.Api.Enumerations;

    [Component(EnLifestyle.Singleton)]
    internal class StringToCharArrayTransformer : IObjectTransformer<string, char[]>
    {
        public char[] Transform(string value)
        {
            return value.ToCharArray();
        }
    }
}