namespace SpaceEngineers.Core.Roslyn.Test.Implementations
{
    using System;
    using Abstractions;
    using AutoRegistration.Api.Abstractions;
    using AutoRegistration.Api.Attributes;
    using AutoRegistration.Api.Enumerations;
    using Microsoft.CodeAnalysis.Text;

    [Component(EnLifestyle.Singleton)]
    internal class NewLineSourceTransformer : ISourceTransformer,
                                              ICollectionResolvable<ISourceTransformer>
    {
        public SourceText Transform(SourceText source)
        {
            return SourceText.From(source
                .ToString()
                .Replace("\r\n", Environment.NewLine, StringComparison.Ordinal)
                .Replace("\n", Environment.NewLine, StringComparison.Ordinal));
        }
    }
}